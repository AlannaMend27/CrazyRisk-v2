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

        // Eventos
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

        /// <summary>
        /// Inicia el servidor TCP/IP
        /// </summary>
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

        /// <summary>
        /// Escucha nuevas conexiones en hilo separado
        /// </summary>
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

        /// <summary>
        /// Agrega un nuevo cliente a la lista
        /// </summary>
        private void AgregarCliente(TcpClient tcpClient)
        {
            ClienteConectado cliente = new ClienteConectado
            {
                tcpClient = tcpClient,
                stream = tcpClient.GetStream(),
                id = clientes.Count + 2 // El servidor es ID 1
            };

            cliente.hilo = new Thread(() => EscucharCliente(cliente));
            cliente.hilo.Start();
            clientes.Add(cliente);

            Debug.Log($"Cliente {cliente.id} conectado. Total: {clientes.Count + 1} jugadores");
        }

        /// <summary>
        /// Escucha mensajes de un cliente específico
        /// </summary>
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

                        // Procesar conexión inicial
                        if (mensaje.tipo == "CONEXION" && string.IsNullOrEmpty(cliente.nombre))
                        {
                            ProcesarConexionInicial(cliente, mensaje);
                        }

                        // Asignar ID del remitente
                        mensaje.jugadorId = cliente.id;

                        // Distribuir a otros clientes
                        DistribuirMensaje(mensaje, cliente.id);

                        // Notificar al servidor (hilo principal)
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

        /// <summary>
        /// Procesa la conexión inicial de un cliente
        /// </summary>
        private void ProcesarConexionInicial(ClienteConectado cliente, MensajeRed mensaje)
        {
            DatosJugador datos = JsonConvert.DeserializeObject<DatosJugador>(mensaje.datos);
            cliente.nombre = datos.nombre;

            UnityMainThreadDispatcher.Instance().Enqueue(() => {
                OnClienteConectado?.Invoke(cliente.nombre);
            });
        }

        /// <summary>
        /// Distribuye un mensaje a todos los clientes excepto al remitente
        /// </summary>
        private void DistribuirMensaje(MensajeRed mensaje, int excluirId = -1)
        {
            foreach (var cliente in clientes)
            {
                if (cliente.id != excluirId && cliente.conectado)
                {
                    EnviarACliente(cliente, mensaje);
                }
            }
        }

        /// <summary>
        /// Envía un mensaje a un cliente específico
        /// </summary>
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

        /// <summary>
        /// Envía un mensaje a todos los clientes conectados
        /// </summary>
        public void EnviarATodos(MensajeRed mensaje)
        {
            DistribuirMensaje(mensaje);
        }

        /// <summary>
        /// Remueve un cliente de la lista
        /// </summary>
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

        /// <summary>
        /// Detiene el servidor y desconecta todos los clientes
        /// </summary>
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

        // Métodos públicos de información
        public int GetCantidadJugadores() => clientes.Count + 1;
        public bool PuedeIniciarJuego() => clientes.Count >= 1;
        public bool ServidorActivo() => activo;
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
