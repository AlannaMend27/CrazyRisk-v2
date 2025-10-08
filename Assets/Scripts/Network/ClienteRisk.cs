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
        [SerializeField] private int timeoutConexion = 0;
        [Header("Red")]
        [SerializeField] private int puertoServidor = 12345;

        private TcpClient cliente;
        private NetworkStream stream;
        private Thread hilo;
        private volatile bool conectado = false;
        // flag para señalizar cierre ordenado
        private volatile bool solicitarCierre = false;
        private string nombreJugador;
        private UnityMainThreadDispatcher dispatcher;

        public System.Action<MensajeRed> OnMensajeRecibido;
        public System.Action OnConectado;
        public System.Action OnDesconectado;
        public System.Action<string> OnError;

        void Start()
        {
            DontDestroyOnLoad(gameObject);
            // Evitar iniciar lógica de red mientras no estemos en Play Mode (evita bloquear el backend en compilaciones)
            if (!Application.isPlaying) return;
            // Cachear el dispatcher en el hilo principal para evitar llamar FindObjectOfType desde hilos en background
            dispatcher = UnityMainThreadDispatcher.Instance();
        }

        void Awake()
        {
            // Asegurar que dispatcher esté disponible incluso si Conectar/Iniciar se llama antes de Start()
            dispatcher = UnityMainThreadDispatcher.Instance();
        }

        public bool ConectarAServidor(string ip, string nombre)
        {
            try
            {
                nombreJugador = nombre;
                cliente = new TcpClient();

                // Configurar timeouts si se definieron (>0). Si es 0, dejamos lectura bloqueante.
                int actualTimeout = timeoutConexion;
                if (actualTimeout > 0)
                {
                    cliente.ReceiveTimeout = actualTimeout;
                    cliente.SendTimeout = actualTimeout;
                }
                else
                {
                    cliente.ReceiveTimeout = 0;
                    cliente.SendTimeout = 0;
                }
                cliente.NoDelay = true;

                cliente.Connect(ip, puertoServidor);
                stream = cliente.GetStream();
                conectado = true;

                hilo = new Thread(EscucharMensajes);
                hilo.IsBackground = true;
                hilo.Start();

                EnviarMensajeConexion();

                dispatcher?.Enqueue(() => {
                    OnConectado?.Invoke();
                });

                Debug.Log($"Cliente {nombre} conectado al servidor {ip}");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Error conectando al servidor: {e.Message}");
                dispatcher?.Enqueue(() => {
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
            try
            {
                using (var reader = new System.IO.StreamReader(stream, Encoding.UTF8, false, 1024, leaveOpen: true))
                {
                    while (conectado && cliente != null && cliente.Connected)
                    {
                        if (solicitarCierre) break;
                        string line = reader.ReadLine();
                        if (line == null)
                        {
                            Debug.Log("El servidor cerró la conexión");
                            break;
                        }

                        try
                        {
                            MensajeRed mensaje = JsonConvert.DeserializeObject<MensajeRed>(line);
                            dispatcher?.Enqueue(() => {
                                OnMensajeRecibido?.Invoke(mensaje);
                            });
                        }
                        catch (Exception ex)
                        {
                            Debug.LogError($"Error deserializando mensaje: {ex}\nPayload: {line}");
                        }
                    }
                }
            }
            catch (Exception e)
            {
                if (conectado)
                {
                    Debug.LogError($"Error leyendo del servidor: {e.Message}");
                    dispatcher?.Enqueue(() => {
                        OnError?.Invoke($"Error de comunicación: {e.Message}");
                    });
                }
            }

            // Cuando salimos del loop, hacemos cierre ordenado
            Desconectar();
        }

        public bool EnviarMensaje(MensajeRed mensaje)
        {
            if (stream != null && cliente != null && cliente.Connected)
            {
                try
                {
                    string json = JsonConvert.SerializeObject(mensaje) + "\n";
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
            // Señalizamos cierre y esperamos al hilo de escucha
            solicitarCierre = true;
            conectado = false;

            try
            {
                // cerrar stream y cliente para desbloquear lecturas
                try { stream?.Close(); } catch { }
                try { cliente?.Close(); } catch { }

                if (hilo != null && hilo.IsAlive)
                {
                    if (!hilo.Join(500))
                    {
                        Debug.LogWarning("El hilo de cliente no terminó en 500ms");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Error al desconectar cliente limpiamente: {ex}");
            }

            stream = null;
            cliente = null;

            // Loguear el stack trace para depurar por qué/desde dónde se llama Desconectar
            Debug.LogWarning($"Desconectar() llamado. StackTrace:\n{Environment.StackTrace}");
            dispatcher?.Enqueue(() => {
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

        // Permite configurar el puerto desde otro componente (por ejemplo AdministradorRed)
        public void SetPuertoServidor(int puerto)
        {
            this.puertoServidor = puerto;
        }
    }
}