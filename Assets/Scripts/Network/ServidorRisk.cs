using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using Newtonsoft.Json;

namespace CrazyRisk.Red
{
    public class ServidorRisk : MonoBehaviour
    {
        [Header("Configuración")]
        [SerializeField] private int puerto = 12345;
        [SerializeField] private int maxJugadores = 3;

        private TcpListener servidor;
        private List<ClienteConectado> clientes = new List<ClienteConectado>();
        private readonly object clientesLock = new object();
        private Thread hiloServidor;
        private volatile bool activo = false;
        // flag para solicitar cierre ordenado del servidor
        private volatile bool solicitarCierreServidor = false;
        private int jugadorEnTurno = 1;
        private int cantidadJugadoresConectados = 1; // El servidor es jugador 1
        private UnityMainThreadDispatcher dispatcher;
        private string nombreHost = "Servidor";

        public System.Action<MensajeRed> OnMensajeRecibido;
        public System.Action<string> OnClienteConectado;
        public System.Action OnClienteDesconectado;

        private class ClienteConectado
        {
            public TcpClient tcpClient;
            public NetworkStream stream;
            public Thread hilo;
            public string nombre;
            public int id;
            public bool conectado = true;
            // lock para proteger escrituras concurrentes
            public object writeLock = new object();
        }

        void Start()
        {
            DontDestroyOnLoad(gameObject);
            if (!Application.isPlaying) return;
            dispatcher = UnityMainThreadDispatcher.Instance();
        }

        void Awake()
        {
            dispatcher = UnityMainThreadDispatcher.Instance();
        }

        public bool IniciarServidor(string nombreHost)
        {
            try
            {
                // Guardar el nombre del host para enviarlo en ACTUALIZAR_NOMBRES
                this.nombreHost = string.IsNullOrEmpty(nombreHost) ? "Servidor" : nombreHost;
                servidor = new TcpListener(IPAddress.Any, puerto);
                servidor.Start();
                activo = true;

                hiloServidor = new Thread(EscucharConexiones);
                hiloServidor.IsBackground = true;
                hiloServidor.Start();

                Debug.Log($"Servidor iniciado por {nombreHost} - Puerto {puerto}");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Error iniciando servidor: {e.Message}");
                return false;
            }
        }

        private void EscucharConexiones()
        {
            while (activo)
            {
                try
                {
                    int actuales;
                    lock (clientesLock) { actuales = clientes.Count; }
                    if (solicitarCierreServidor) break;
                    if (servidor.Pending() && actuales < maxJugadores - 1)
                    {
                        TcpClient nuevoCliente = servidor.AcceptTcpClient();
                        AgregarCliente(nuevoCliente);
                    }
                    Thread.Sleep(100);
                }
                catch (Exception e)
                {
                    if (activo) Debug.LogError($"Error escuchando conexiones: {e.Message}");
                }
            }
        }

        private void AgregarCliente(TcpClient tcpClient)
        {
            cantidadJugadoresConectados++;

            ClienteConectado cliente = new ClienteConectado
            {
                tcpClient = tcpClient,
                stream = tcpClient.GetStream(),
                id = cantidadJugadoresConectados
            };

            cliente.hilo = new Thread(() => EscucharCliente(cliente));
            cliente.hilo.IsBackground = true;
            cliente.hilo.Start();
            lock (clientesLock)
            {
                clientes.Add(cliente);
            }

            Debug.Log($"Cliente {cliente.id} conectado. Total: {cantidadJugadoresConectados} jugadores");
        }

        private void EscucharCliente(ClienteConectado cliente)
        {
            try
            {
                // Leave the stream open: we'll close it explicitly en RemoverCliente
                using (var reader = new System.IO.StreamReader(cliente.stream, Encoding.UTF8, false, 1024, leaveOpen: true))
                {
                    while (activo && cliente.conectado && cliente.tcpClient.Connected)
                    {
                        string line = reader.ReadLine();
                        if (line == null)
                            break;

                        MensajeRed mensaje = null;
                        try
                        {
                            mensaje = JsonConvert.DeserializeObject<MensajeRed>(line);
                        }
                        catch (Exception ex)
                        {
                            Debug.LogError($"Error deserializando mensaje del cliente {cliente.id}: {ex.Message}\n{line}");
                            continue;
                        }

                        if (mensaje == null) continue;

                        if (mensaje.tipo == "CONEXION" && string.IsNullOrEmpty(cliente.nombre))
                        {
                            ProcesarConexionInicial(cliente, mensaje);
                            continue;
                        }

                        mensaje.jugadorId = cliente.id;

                        switch (mensaje.tipo)
                        {
                            case "SOLICITAR_ESTADO":
                                EnviarEstadoCompleto(cliente);
                                break;

                            case "ACCION_JUEGO":
                                if (ValidarTurno(cliente.id) || EsPreparacion())
                                {
                                    DistribuirMensaje(mensaje, -1);
                                }
                                else
                                {
                                    EnviarRechazo(cliente, "No es tu turno");
                                }
                                break;

                            case "FIN_TURNO":
                                if (ValidarTurno(cliente.id))
                                {
                                    CambiarTurno();
                                    MensajeRed mensajeTurno = new MensajeRed
                                    {
                                        tipo = "CAMBIO_TURNO",
                                        jugadorId = jugadorEnTurno,
                                        datos = jugadorEnTurno.ToString()
                                    };
                                    DistribuirMensaje(mensajeTurno, -1);
                                }
                                break;

                            default:
                                DistribuirMensaje(mensaje, cliente.id);
                                break;
                        }

                        dispatcher?.Enqueue(() => {
                            OnMensajeRecibido?.Invoke(mensaje);
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error leyendo cliente: {ex.Message}");
            }

            RemoverCliente(cliente);
        }

        private void ProcesarConexionInicial(ClienteConectado cliente, MensajeRed mensaje)
        {
            DatosJugador datos = JsonConvert.DeserializeObject<DatosJugador>(mensaje.datos);
            cliente.nombre = datos.nombre;

            // Enviar confirmación con el ID asignado
            DatosJugador confirmacion = new DatosJugador(cliente.nombre, cliente.id, "");
            MensajeRed respuesta = new MensajeRed
            {
                tipo = "CONEXION_ACEPTADA",
                datos = JsonConvert.SerializeObject(confirmacion),
                jugadorId = 1
            };
            EnviarACliente(cliente, respuesta);

            // NUEVO: Enviar nombres de todos los jugadores a todos (hilo principal por PlayerPrefs)
            Debug.Log($"ProcesarConexionInicial - cliente.nombre={cliente.nombre}");
            lock (clientesLock)
            {
                foreach (var c in clientes)
                    Debug.Log($"Cliente conectado snapshot: id={c.id} nombre={c.nombre}");
            }
            ActualizarNombresJugadores();

            // Enviar estado completo del juego al cliente recién conectado
            try
            {
                // Pequeña espera para que el cliente procese la confirmación primero
                Thread.Sleep(200);
                EnviarEstadoCompleto(cliente);
            }
            catch (Exception) { }

            dispatcher?.Enqueue(() => {
                OnClienteConectado?.Invoke(cliente.nombre);
            });
        }

        private void ActualizarNombresJugadores()
        {
            // PlayerPrefs.GetString must be called on main thread. Ejecutamos todo el proceso en el hilo principal
            dispatcher?.Enqueue(() => {
                string serverName = this.nombreHost ?? PlayerPrefs.GetString("NombreJugador", "Servidor");

                List<ClienteConectado> snapshot;
                lock (clientesLock)
                {
                    snapshot = new List<ClienteConectado>(clientes);
                }

                // Si algún cliente tiene el mismo nombre que el host, marcar el host para disambiguar
                bool conflicto = false;
                foreach (var c in snapshot)
                {
                    if (string.Equals(c.nombre, serverName, StringComparison.OrdinalIgnoreCase))
                    {
                        conflicto = true;
                        break;
                    }
                }

                if (conflicto)
                {
                    serverName = serverName + " (Host)";
                }

                // Detectar duplicados y disambiguar nombres
                // Construimos un mapa de nombre -> ocurrencias (incluye host)
                var nameOccurrences = new System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<int>>(StringComparer.OrdinalIgnoreCase);
                // incluir host bajo su raw name
                if (!nameOccurrences.ContainsKey(this.nombreHost))
                    nameOccurrences[this.nombreHost] = new System.Collections.Generic.List<int>();
                nameOccurrences[this.nombreHost].Add(0); // 0 para host

                for (int i = 0; i < snapshot.Count; i++)
                {
                    string n = snapshot[i].nombre ?? "";
                    if (!nameOccurrences.ContainsKey(n))
                        nameOccurrences[n] = new System.Collections.Generic.List<int>();
                    nameOccurrences[n].Add(snapshot[i].id);
                }

                int total = snapshot.Count + 1;
                string[] nombres = new string[total];

                // Resolver nombre del host
                string hostDisplay = serverName;
                if (nameOccurrences.TryGetValue(this.nombreHost, out var hostList) && hostList.Count > 1)
                {
                    hostDisplay = serverName + " (Host)";
                }
                nombres[0] = hostDisplay;

                // Resolver nombres de clientes, si hay conflicto con host o entre clientes añadir sufijo con id
                for (int i = 0; i < snapshot.Count; i++)
                {
                    string raw = snapshot[i].nombre ?? "";
                    string display = raw;
                    if (string.Equals(raw, this.nombreHost, StringComparison.OrdinalIgnoreCase))
                    {
                        display = raw + $" (P{snapshot[i].id})";
                    }
                    else if (nameOccurrences.TryGetValue(raw, out var occ) && occ.Count > 1)
                    {
                        // varios clientes con el mismo nombre
                        display = raw + $" (P{snapshot[i].id})";
                    }
                    nombres[i + 1] = display;
                }

                MensajeRed mensaje = new MensajeRed
                {
                    tipo = "ACTUALIZAR_NOMBRES",
                    datos = JsonConvert.SerializeObject(nombres),
                    jugadorId = 1
                };

                // Distribuir desde el hilo principal es aceptable aquí (operación rápida)
                DistribuirMensaje(mensaje);
            });
        }

        private void EnviarACliente(ClienteConectado cliente, MensajeRed mensaje)
        {
            try
            {
                string json = JsonConvert.SerializeObject(mensaje) + "\n";
                byte[] datos = Encoding.UTF8.GetBytes(json);
                string preview = json.Length > 200 ? json.Substring(0, 200) + "..." : json;
                Debug.Log($"EnviarACliente id={cliente.id} tipo={mensaje.tipo} bytes={datos.Length} payload={preview}");
                lock (cliente.writeLock)
                {
                    cliente.stream.Write(datos, 0, datos.Length);
                    cliente.stream.Flush();
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error enviando a cliente {cliente.id}: {ex}");
                // Marcar cliente para remoción
                cliente.conectado = false;
                try { RemoverCliente(cliente); } catch (Exception) { }
            }
        }

        public void EnviarATodos(MensajeRed mensaje)
        {
            DistribuirMensaje(mensaje);
        }

        private void RemoverCliente(ClienteConectado cliente)
        {
            cliente.conectado = false;

            try
            {
                cliente.stream?.Close();
                cliente.tcpClient?.Close();
            }
            catch { }

            lock (clientesLock)
            {
                clientes.Remove(cliente);
            }

            dispatcher?.Enqueue(() => {
                OnClienteDesconectado?.Invoke();
            });

            int restantes;
            lock (clientesLock) { restantes = clientes.Count; }
            Debug.Log($"Cliente {cliente.id} desconectado. Quedan: {restantes + 1} jugadores");

            try
            {
                // esperar que el hilo cliente termine
                if (cliente.hilo != null && cliente.hilo.IsAlive)
                {
                    if (!cliente.hilo.Join(500))
                    {
                        Debug.LogWarning($"El hilo del cliente {cliente.id} no terminó en 500ms");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Error esperando hilo cliente {cliente.id}: {ex}");
            }
        }

        public void DetenerServidor()
        {
            // Señalar cierre ordenado
            solicitarCierreServidor = true;
            activo = false;

            List<ClienteConectado> copia;
            lock (clientesLock) { copia = new List<ClienteConectado>(clientes); }
            foreach (var cliente in copia)
                RemoverCliente(cliente);

            try
            {
                servidor?.Stop();
                // esperar a que el hilo servidor termine
                if (hiloServidor != null && hiloServidor.IsAlive)
                {
                    if (!hiloServidor.Join(500))
                    {
                        Debug.LogWarning("El hilo del servidor no terminó en 500ms");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Error deteniendo servidor limpiamente: {ex}");
            }

            Debug.Log("Servidor detenido");
        }

        private bool ValidarTurno(int jugadorId)
        {
            return jugadorId == jugadorEnTurno;
        }

        private bool EsPreparacion()
        {
            return true;
        }

        private void CambiarTurno()
        {
            jugadorEnTurno++;
            if (jugadorEnTurno > cantidadJugadoresConectados)
            {
                jugadorEnTurno = 1;
            }
            Debug.Log($"Turno cambiado a jugador {jugadorEnTurno}");
        }

        private void EnviarEstadoCompleto(ClienteConectado cliente)
        {
            MensajeRed solicitud = new MensajeRed
            {
                tipo = "SOLICITAR_ESTADO_GAMEMANAGER",
                jugadorId = 1,
                datos = cliente.id.ToString()
            };

            dispatcher?.Enqueue(() => {
                OnMensajeRecibido?.Invoke(solicitud);
            });
        }

        private void EnviarRechazo(ClienteConectado cliente, string razon)
        {
            MensajeRed rechazo = new MensajeRed
            {
                tipo = "ACCION_RECHAZADA",
                datos = razon,
                jugadorId = 1
            };
            EnviarACliente(cliente, rechazo);
        }

        private void DistribuirMensaje(MensajeRed mensaje, int excluirId = -1)
        {
            lock (clientesLock)
            {
                foreach (var cliente in clientes)
                {
                    if (excluirId == -1 || cliente.id != excluirId)
                    {
                        if (cliente.conectado)
                        {
                            EnviarACliente(cliente, mensaje);
                        }
                    }
                }
            }
        }

        public int GetCantidadJugadores() => clientes.Count + 1;
        public bool PuedeIniciarJuego() => clientes.Count >= 1;
        public bool ServidorActivo() => activo;
        public int GetJugadorEnTurno() => jugadorEnTurno;
        public int GetCantidadJugadoresConectados() => cantidadJugadoresConectados;

        public void EnviarEstadoACliente(int clienteId, EstadoJuego estado)
        {
            ClienteConectado cliente = null;
            lock (clientesLock)
            {
                cliente = clientes.Find(c => c.id == clienteId);
            }
            if (cliente != null)
            {
                MensajeRed mensaje = new MensajeRed
                {
                    tipo = "ESTADO_COMPLETO",
                    datos = JsonConvert.SerializeObject(estado),
                    jugadorId = 1
                };
                try
                {
                    EnviarACliente(cliente, mensaje);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error al enviar estado al cliente {clienteId}: {ex.Message}");
                }
            }
        }

        public List<string> GetNombresClientes()
        {
            List<string> nombres = new List<string>();
            lock (clientesLock)
            {
                foreach (var cliente in clientes)
                    if (!string.IsNullOrEmpty(cliente.nombre))
                        nombres.Add(cliente.nombre);
            }
            return nombres;
        }

        public string GetNombreHost()
        {
            return nombreHost;
        }

        void OnDestroy()
        {
            DetenerServidor();
        }
    }
}