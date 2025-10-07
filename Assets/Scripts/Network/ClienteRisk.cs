using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using Newtonsoft.Json;

namespace CrazyRisk.Red
{
    public class ClienteRisk : MonoBehaviour
    {
        [Header("Configuración")]
        [SerializeField] private int timeoutConexion = 5000;

        private TcpClient cliente;
        private NetworkStream stream;
        private Thread hilo;
        private bool conectado = false;
        private string nombreJugador;

        public System.Action<MensajeRed> OnMensajeRecibido;
        public System.Action OnConectado;
        public System.Action OnDesconectado;
        public System.Action<string> OnError;

        void Start()
        {
            DontDestroyOnLoad(gameObject);
        }

        public bool ConectarAServidor(string ip, string nombre)
        {
            try
            {
                nombreJugador = nombre;
                cliente = new TcpClient();

                cliente.ReceiveTimeout = timeoutConexion;
                cliente.SendTimeout = timeoutConexion;

                cliente.Connect(ip, 12345);
                stream = cliente.GetStream();
                conectado = true;

                hilo = new Thread(EscucharMensajes);
                hilo.Start();

                EnviarMensajeConexion();

                UnityMainThreadDispatcher.Instance().Enqueue(() => {
                    OnConectado?.Invoke();
                });

                Debug.Log($"Cliente {nombre} conectado al servidor {ip}");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Error conectando al servidor: {e.Message}");
                UnityMainThreadDispatcher.Instance().Enqueue(() => {
                    OnError?.Invoke($"No se pudo conectar: {e.Message}");
                });
                return false;
            }
        }

        private void EnviarMensajeConexion()
        {
            DatosJugador datos = new DatosJugador(nombreJugador, 0, "");
            MensajeRed mensaje = new MensajeRed
            {
                tipo = "CONEXION",
                datos = JsonConvert.SerializeObject(datos)
            };
            EnviarMensaje(mensaje);

            // Esperar un poco y solicitar estado
            Thread.Sleep(1000);

            MensajeRed solicitud = new MensajeRed
            {
                tipo = "SOLICITAR_ESTADO",
                datos = ""
            };
            EnviarMensaje(solicitud);
        }

        private void EscucharMensajes()
        {
            byte[] buffer = new byte[4096];

            while (conectado && cliente != null && cliente.Connected)
            {
                try
                {
                    int bytes = stream.Read(buffer, 0, buffer.Length);
                    if (bytes > 0)
                    {
                        string json = Encoding.UTF8.GetString(buffer, 0, bytes);
                        MensajeRed mensaje = JsonConvert.DeserializeObject<MensajeRed>(json);

                        UnityMainThreadDispatcher.Instance().Enqueue(() => {
                            OnMensajeRecibido?.Invoke(mensaje);
                        });
                    }
                    else
                    {
                        Debug.Log("El servidor cerró la conexión");
                        break;
                    }
                }
                catch (Exception e)
                {
                    if (conectado)
                    {
                        Debug.LogError($"Error leyendo del servidor: {e.Message}");
                        UnityMainThreadDispatcher.Instance().Enqueue(() => {
                            OnError?.Invoke($"Error de comunicación: {e.Message}");
                        });
                    }
                    break;
                }
            }

            Desconectar();
        }

        public bool EnviarMensaje(MensajeRed mensaje)
        {
            if (stream != null && cliente != null && cliente.Connected)
            {
                try
                {
                    string json = JsonConvert.SerializeObject(mensaje);
                    byte[] datos = Encoding.UTF8.GetBytes(json);
                    stream.Write(datos, 0, datos.Length);
                    return true;
                }
                catch (Exception e)
                {
                    Debug.LogError($"Error enviando mensaje: {e.Message}");
                    return false;
                }
            }
            return false;
        }

        public bool Reconectar(string ip)
        {
            Desconectar();
            Thread.Sleep(1000);
            return ConectarAServidor(ip, nombreJugador);
        }

        public void Desconectar()
        {
            conectado = false;

            try
            {
                stream?.Close();
                cliente?.Close();
                hilo?.Abort();
            }
            catch { }

            stream = null;
            cliente = null;

            UnityMainThreadDispatcher.Instance().Enqueue(() => {
                OnDesconectado?.Invoke();
            });

            Debug.Log($"Cliente {nombreJugador} desconectado del servidor");
        }

        public bool EstaConectado() => conectado;
        public string GetNombreJugador() => nombreJugador;

        void OnDestroy()
        {
            Desconectar();
        }
    }
}