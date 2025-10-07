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
        private Thread hiloServidor;
        private bool activo = false;
        private int jugadorEnTurno = 1;
        private int cantidadJugadoresConectados = 1; // El servidor es jugador 1

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
        }

        void Start()
        {
            DontDestroyOnLoad(gameObject);
        }

        public bool IniciarServidor(string nombreHost)
        {
            try
            {
                servidor = new TcpListener(IPAddress.Any, puerto);
                servidor.Start();
                activo = true;

                hiloServidor = new Thread(EscucharConexiones);
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
                    if (servidor.Pending() && clientes.Count < maxJugadores - 1)
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
            cliente.hilo.Start();
            clientes.Add(cliente);

            Debug.Log($"Cliente {cliente.id} conectado. Total: {cantidadJugadoresConectados} jugadores");
        }

        private void EscucharCliente(ClienteConectado cliente)
        {
            byte[] buffer = new byte[4096];

            while (activo && cliente.conectado && cliente.tcpClient.Connected)
            {
                try
                {
                    int bytes = cliente.stream.Read(buffer, 0, buffer.Length);
                    if (bytes > 0)
                    {
                        string json = Encoding.UTF8.GetString(buffer, 0, bytes);
                        MensajeRed mensaje = JsonConvert.DeserializeObject<MensajeRed>(json);

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

                        UnityMainThreadDispatcher.Instance().Enqueue(() => {
                            OnMensajeRecibido?.Invoke(mensaje);
                        });
                    }
                }
                catch
                {
                    break;
                }
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

            // NUEVO: Enviar nombres de todos los jugadores a todos
            ActualizarNombresJugadores();

            UnityMainThreadDispatcher.Instance().Enqueue(() => {
                OnClienteConectado?.Invoke(cliente.nombre);
            });
        }

        private void ActualizarNombresJugadores()
        {
            string[] nombres = new string[cantidadJugadoresConectados];
            nombres[0] = PlayerPrefs.GetString("NombreJugador", "Servidor");

            for (int i = 0; i < clientes.Count; i++)
            {
                nombres[i + 1] = clientes[i].nombre;
            }

            MensajeRed mensaje = new MensajeRed
            {
                tipo = "ACTUALIZAR_NOMBRES",
                datos = JsonConvert.SerializeObject(nombres),
                jugadorId = 1
            };

            DistribuirMensaje(mensaje);
        }

        private void EnviarACliente(ClienteConectado cliente, MensajeRed mensaje)
        {
            try
            {
                string json = JsonConvert.SerializeObject(mensaje);
                byte[] datos = Encoding.UTF8.GetBytes(json);
                cliente.stream.Write(datos, 0, datos.Length);
            }
            catch
            {
                cliente.conectado = false;
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

            clientes.Remove(cliente);

            UnityMainThreadDispatcher.Instance().Enqueue(() => {
                OnClienteDesconectado?.Invoke();
            });

            Debug.Log($"Cliente {cliente.id} desconectado. Quedan: {clientes.Count + 1} jugadores");
        }

        public void DetenerServidor()
        {
            activo = false;

            foreach (var cliente in clientes)
                RemoverCliente(cliente);

            try
            {
                servidor?.Stop();
                hiloServidor?.Abort();
            }
            catch { }

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

            UnityMainThreadDispatcher.Instance().Enqueue(() => {
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

        public int GetCantidadJugadores() => clientes.Count + 1;
        public bool PuedeIniciarJuego() => clientes.Count >= 1;
        public bool ServidorActivo() => activo;
        public int GetJugadorEnTurno() => jugadorEnTurno;
        public int GetCantidadJugadoresConectados() => cantidadJugadoresConectados;

        public void EnviarEstadoACliente(int clienteId, EstadoJuego estado)
        {
            ClienteConectado cliente = clientes.Find(c => c.id == clienteId);
            if (cliente != null)
            {
                MensajeRed mensaje = new MensajeRed
                {
                    tipo = "ESTADO_COMPLETO",
                    datos = JsonConvert.SerializeObject(estado),
                    jugadorId = 1
                };
                EnviarACliente(cliente, mensaje);
            }
        }

        public List<string> GetNombresClientes()
        {
            List<string> nombres = new List<string>();
            foreach (var cliente in clientes)
                if (!string.IsNullOrEmpty(cliente.nombre))
                    nombres.Add(cliente.nombre);
            return nombres;
        }

        void OnDestroy()
        {
            DetenerServidor();
        }
    }
}