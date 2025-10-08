using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

namespace CrazyRisk.Red
{
    public class MenuRedUI : MonoBehaviour
    {
        [Header("Paneles de UI")]
        public GameObject panelPrincipal;
        public GameObject panelCrear;
        public GameObject panelUnirse;
        public GameObject panelEspera;

        [Header("Campos de Input")]
        public TMP_InputField inputNombre;
        public TMP_InputField inputIP;

        [Header("Botones")]
        public Button btnCrear;
        public Button btnUnirse;
        public Button btnIniciarJuego;
        public Button btnCancelar;

        [Header("Textos de Estado")]
        public TextMeshProUGUI textoEstado;
        public TextMeshProUGUI textoJugadores;
        public TextMeshProUGUI textoMensajes;

        // Componentes de red
        private ServidorRisk servidor;
        private ClienteRisk cliente;

        // Estado
        private bool esServidor;
        private string nombreLocal;

        void Start()
        {
            InicializarUI();
            ConfigurarEventos();
            MostrarPanel(panelPrincipal);
        }

        /// <summary>
        /// Configura la UI inicial
        /// </summary>
        private void InicializarUI()
        {
            // Configurar valores por defecto
            if (inputIP != null)
                inputIP.text = "127.0.0.1";

            // Ocultar botón de iniciar juego inicialmente
            if (btnIniciarJuego != null)
                btnIniciarJuego.gameObject.SetActive(false);

            LimpiarMensajes();
        }

        /// <summary>
        /// Configura eventos de botones
        /// </summary>
        private void ConfigurarEventos()
        {
            btnCrear?.onClick.AddListener(CrearPartida);
            btnUnirse?.onClick.AddListener(UnirseAPartida);
            btnIniciarJuego?.onClick.AddListener(IniciarJuego);
            btnCancelar?.onClick.AddListener(CancelarPartida);
        }

        /// <summary>
        /// Crea una nueva partida como servidor
        /// </summary>
        private void CrearPartida()
        {
            nombreLocal = inputNombre?.text?.Trim();

            if (string.IsNullOrEmpty(nombreLocal))
            {
                MostrarMensaje("Ingresa tu nombre de usuario", Color.red);
                return;
            }

            if (nombreLocal.Length < 2)
            {
                MostrarMensaje("El nombre debe tener al menos 2 caracteres", Color.red);
                return;
            }

            esServidor = true;

            // Guardar nombre del host para que ServidorRisk use este nombre al enviar la lista
            PlayerPrefs.SetString("NombreJugador", nombreLocal);
            PlayerPrefs.Save();

            // Crear componente servidor
            servidor = gameObject.AddComponent<ServidorRisk>();
            ConfigurarEventosServidor();

            if (servidor.IniciarServidor(nombreLocal))
            {
                MostrarPanel(panelEspera);
                ActualizarEstado($"Servidor creado por {nombreLocal}\nEsperando jugadores...\nIP: 127.0.0.1");
                ActualizarListaJugadores();
                MostrarMensaje("Servidor iniciado exitosamente", Color.green);
            }
            else
            {
                MostrarMensaje("Error al crear la partida", Color.red);
                DestruirServidor();
            }
        }

        /// <summary>
        /// Se une a una partida existente como cliente
        /// </summary>
        private void UnirseAPartida()
        {
            nombreLocal = inputNombre?.text?.Trim();
            string ip = inputIP?.text?.Trim();

            if (string.IsNullOrEmpty(nombreLocal))
            {
                MostrarMensaje("Ingresa tu nombre de usuario", Color.red);
                return;
            }

            if (string.IsNullOrEmpty(ip))
            {
                MostrarMensaje("Ingresa la IP del servidor", Color.red);
                return;
            }

            esServidor = false;

            // Crear componente cliente
            cliente = gameObject.AddComponent<ClienteRisk>();
            ConfigurarEventosCliente();

            MostrarPanel(panelEspera);
            ActualizarEstado($"Conectando a {ip}...");

            if (!cliente.ConectarAServidor(ip, nombreLocal))
            {
                MostrarMensaje("Error al conectar", Color.red);
                DestruirCliente();
                MostrarPanel(panelPrincipal);
            }
        }

        #region Eventos del Servidor

        private void ConfigurarEventosServidor()
        {
            servidor.OnClienteConectado += OnClienteConectado;
            servidor.OnClienteDesconectado += OnClienteDesconectado;
            servidor.OnMensajeRecibido += OnMensajeRecibidoServidor;
        }

        private void OnClienteConectado(string nombreCliente)
        {
            ActualizarListaJugadores();
            MostrarMensaje($"{nombreCliente} se unió a la partida", Color.green);

            if (servidor.PuedeIniciarJuego())
            {
                btnIniciarJuego?.gameObject.SetActive(true);
            }
        }

        private void OnClienteDesconectado()
        {
            ActualizarListaJugadores();
            btnIniciarJuego?.gameObject.SetActive(false);
            MostrarMensaje("Un jugador se desconectó", Color.yellow);
        }

        private void OnMensajeRecibidoServidor(MensajeRed mensaje)
        {
            Debug.Log($"Servidor recibió: {mensaje.tipo} de jugador {mensaje.jugadorId}");
        }

        #endregion

        #region Eventos del Cliente

        private void ConfigurarEventosCliente()
        {
            cliente.OnConectado += OnConectadoCliente;
            cliente.OnDesconectado += OnDesconectadoCliente;
            cliente.OnMensajeRecibido += OnMensajeRecibidoCliente;
            cliente.OnError += OnErrorCliente;
        }

        private void OnConectadoCliente()
        {
            ActualizarEstado("¡Conectado exitosamente!\nEsperando que el host inicie la partida...");
            MostrarMensaje("Conectado al servidor", Color.green);
        }

        private void OnDesconectadoCliente()
        {
            MostrarMensaje("Desconectado del servidor", Color.yellow);
            MostrarPanel(panelPrincipal);
        }

        private void OnMensajeRecibidoCliente(MensajeRed mensaje)
        {
            if (mensaje.tipo == "INICIAR_JUEGO")
            {
                CargarJuego();
            }
        }

        private void OnErrorCliente(string error)
        {
            MostrarMensaje($"Error: {error}", Color.red);
        }

        #endregion

        /// <summary>
        /// Inicia el juego (solo para servidor)
        /// </summary>
        private void IniciarJuego()
        {
            if (esServidor && servidor != null && servidor.PuedeIniciarJuego())
            {
                // Enviar mensaje de inicio a todos los clientes
                MensajeRed mensajeInicio = new MensajeRed
                {
                    tipo = "INICIAR_JUEGO",
                    jugadorId = 1,
                    datos = "Iniciando partida"
                };

                servidor.EnviarATodos(mensajeInicio);
                CargarJuego();
            }
            else
            {
                MostrarMensaje("No hay suficientes jugadores conectados", Color.red);
            }
        }

        /// <summary>
        /// Carga la escena del juego
        /// </summary>
        private void CargarJuego()
        {
            GuardarDatosPartida();
            SceneManager.LoadScene("mapita");
        }

        /// <summary>
        /// Guarda información de la partida para el GameManager
        /// </summary>
        private void GuardarDatosPartida()
        {
            PlayerPrefs.SetString("NombreJugador", nombreLocal);
            PlayerPrefs.SetInt("EsServidor", esServidor ? 1 : 0);

            if (esServidor && servidor != null)
            {
                PlayerPrefs.SetInt("CantidadJugadores", servidor.GetCantidadJugadores());
            }
            else
            {
                PlayerPrefs.SetInt("CantidadJugadores", 2); // Asumimos 2 jugadores para cliente
            }

            PlayerPrefs.Save();
        }

        /// <summary>
        /// Cancela la partida y vuelve al menú principal
        /// </summary>
        private void CancelarPartida()
        {
            if (esServidor)
                DestruirServidor();
            else
                DestruirCliente();

            MostrarPanel(panelPrincipal);
            LimpiarMensajes();
        }

        #region Utilidades de UI

        private void MostrarPanel(GameObject panel)
        {
            panelPrincipal?.SetActive(panel == panelPrincipal);
            panelCrear?.SetActive(panel == panelCrear);
            panelUnirse?.SetActive(panel == panelUnirse);
            panelEspera?.SetActive(panel == panelEspera);
        }

        private void ActualizarEstado(string mensaje)
        {
            if (textoEstado != null)
                textoEstado.text = mensaje;
        }

        private void ActualizarListaJugadores()
        {
            if (!esServidor || servidor == null || textoJugadores == null) return;

            string lista = $"Jugadores ({servidor.GetCantidadJugadores()}/3):\n";
            lista += $"• {nombreLocal} (Host)\n";

            foreach (string nombre in servidor.GetNombresClientes())
                lista += $"• {nombre}\n";

            textoJugadores.text = lista;
        }

        private void MostrarMensaje(string mensaje, Color color)
        {
            if (textoMensajes != null)
            {
                textoMensajes.text = mensaje;
                textoMensajes.color = color;
            }
            Debug.Log($"[MenuUI] {mensaje}");
        }

        private void LimpiarMensajes()
        {
            if (textoMensajes != null)
                textoMensajes.text = "";
        }

        private void DestruirServidor()
        {
            if (servidor != null)
            {
                servidor.DetenerServidor();
                Destroy(servidor);
                servidor = null;
            }
        }

        private void DestruirCliente()
        {
            if (cliente != null)
            {
                cliente.Desconectar();
                Destroy(cliente);
                cliente = null;
            }
        }

        #endregion

        void OnDestroy()
        {
            // Limpiar recursos al destruir el objeto
            DestruirServidor();
            DestruirCliente();
        }
    }
}