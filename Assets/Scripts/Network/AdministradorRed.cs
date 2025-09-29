using UnityEngine;
using CrazyRisk.Managers;
using Newtonsoft.Json;

namespace CrazyRisk.Red
{
    /// <summary>
    /// Coordina la comunicaci�n de red y sincroniza el estado del juego
    /// Se conecta autom�ticamente al GameManager
    /// </summary>
    public class AdministradorRed : MonoBehaviour
    {
        // Componentes de red
        private ServidorRisk servidor;
        private ClienteRisk cliente;

        // Estado
        private bool esServidor;
        private int cantidadJugadores;
        private string nombreJugador;

        // Referencias del juego
        private GameManager gameManager;

        void Start()
        {
            DontDestroyOnLoad(gameObject);

            // Cargar configuraci�n guardada del men�
            CargarConfiguracion();

            // Buscar GameManager y conectar sistema de red
            StartCoroutine(BuscarGameManagerYConectar());
        }

        /// <summary>
        /// Carga la configuraci�n de la partida desde PlayerPrefs
        /// </summary>
        private void CargarConfiguracion()
        {
            esServidor = PlayerPrefs.GetInt("EsServidor", 1) == 1;
            cantidadJugadores = PlayerPrefs.GetInt("CantidadJugadores", 2);
            nombreJugador = PlayerPrefs.GetString("NombreJugador", "Jugador1");

            Debug.Log($"AdministradorRed cargado: {nombreJugador} {(esServidor ? "(Servidor)" : "(Cliente)")} - {cantidadJugadores} jugadores");
        }

        /// <summary>
        /// Busca el GameManager y establece la conexi�n de red
        /// </summary>
        private System.Collections.IEnumerator BuscarGameManagerYConectar()
        {
            // Esperar hasta que el GameManager est� disponible
            while (gameManager == null)
            {
                gameManager = FindObjectOfType<GameManager>();
                yield return new UnityEngine.WaitForSeconds(0.1f);
            }

            // Conectar sistema de red
            if (esServidor)
                IniciarComoServidor();
            else
                IniciarComoCliente();
        }

        /// <summary>
        /// Inicia el componente como servidor
        /// </summary>
        private void IniciarComoServidor()
        {
            servidor = gameObject.AddComponent<ServidorRisk>();
            servidor.OnMensajeRecibido = ProcesarMensajeServidor;
            servidor.OnClienteConectado += OnClienteConectadoServidor;
            servidor.OnClienteDesconectado += OnClienteDesconectadoServidor;

            if (servidor.IniciarServidor(nombreJugador))
            {
                Debug.Log("AdministradorRed funcionando como servidor");
            }
            else
            {
                Debug.LogError("Error al iniciar AdministradorRed como servidor");
            }
        }

        /// <summary>
        /// Inicia el componente como cliente
        /// </summary>
        private void IniciarComoCliente()
        {
            string ipServidor = "127.0.0.1"; // Por defecto localhost

            cliente = gameObject.AddComponent<ClienteRisk>();
            cliente.OnMensajeRecibido = ProcesarMensajeCliente;
            cliente.OnConectado += OnConectadoCliente;
            cliente.OnDesconectado += OnDesconectadoCliente;

            if (cliente.ConectarAServidor(ipServidor, nombreJugador))
            {
                Debug.Log("AdministradorRed funcionando como cliente");
            }
            else
            {
                Debug.LogError("Error al conectar AdministradorRed como cliente");
            }
        }

        #region Procesamiento de Mensajes

        /// <summary>
        /// Procesa mensajes recibidos cuando act�a como servidor
        /// </summary>
        private void ProcesarMensajeServidor(MensajeRed mensaje)
        {
            Debug.Log($"[Servidor] Procesando: {mensaje.tipo} de jugador {mensaje.jugadorId}");

            switch (mensaje.tipo)
            {
                case "SELECCIONAR_TERRITORIO":
                    ProcesarSeleccionTerritorio(mensaje);
                    break;

                case "COLOCAR_TROPAS":
                    ProcesarColocarTropas(mensaje);
                    break;

                case "ATACAR":
                    ProcesarAtaque(mensaje);
                    break;

                case "FINALIZAR_TURNO":
                    ProcesarFinalizarTurno(mensaje);
                    break;

                case "SOLICITAR_ESTADO":
                    EnviarEstadoCompleto();
                    break;
            }
        }

        /// <summary>
        /// Procesa mensajes recibidos cuando act�a como cliente
        /// </summary>
        private void ProcesarMensajeCliente(MensajeRed mensaje)
        {
            Debug.Log($"[Cliente] Procesando: {mensaje.tipo} de jugador {mensaje.jugadorId}");

            switch (mensaje.tipo)
            {
                case "ESTADO_COMPLETO":
                    ActualizarEstadoLocal(mensaje);
                    break;

                case "SELECCIONAR_TERRITORIO":
                    ProcesarSeleccionTerritorio(mensaje);
                    break;

                case "COLOCAR_TROPAS":
                    ProcesarColocarTropas(mensaje);
                    break;

                case "ATACAR":
                    ProcesarAtaque(mensaje);
                    break;

                case "FINALIZAR_TURNO":
                    ProcesarFinalizarTurno(mensaje);
                    break;
            }
        }

        /// <summary>
        /// Procesa selecci�n de territorio de otro jugador
        /// </summary>
        private void ProcesarSeleccionTerritorio(MensajeRed mensaje)
        {
            Debug.Log($"Jugador {mensaje.jugadorId} seleccion� territorio: {mensaje.datos}");
            // Aqu� puedes agregar l�gica visual para mostrar la selecci�n
        }

        /// <summary>
        /// Procesa colocaci�n de tropas de otro jugador
        /// </summary>
        private void ProcesarColocarTropas(MensajeRed mensaje)
        {
            Debug.Log($"Jugador {mensaje.jugadorId} coloc� tropas: {mensaje.datos}");
            // Actualizar el estado visual del territorio
        }

        /// <summary>
        /// Procesa ataque de otro jugador
        /// </summary>
        private void ProcesarAtaque(MensajeRed mensaje)
        {
            Debug.Log($"Jugador {mensaje.jugadorId} realiz� ataque: {mensaje.datos}");
            // Mostrar resultado del ataque
        }

        /// <summary>
        /// Procesa finalizaci�n de turno
        /// </summary>
        private void ProcesarFinalizarTurno(MensajeRed mensaje)
        {
            Debug.Log($"Jugador {mensaje.jugadorId} finaliz� su turno");
            // Cambiar al siguiente jugador
        }

        /// <summary>
        /// Actualiza el estado local del juego (solo cliente)
        /// </summary>
        private void ActualizarEstadoLocal(MensajeRed mensaje)
        {
            try
            {
                EstadoJuego estado = JsonConvert.DeserializeObject<EstadoJuego>(mensaje.datos);

                if (gameManager != null && estado.territoriosPropietarios != null)
                {
                    var territorios = gameManager.GetTerritorios();

                    for (int i = 0; i < territorios.getSize() && i < estado.territoriosPropietarios.Length; i++)
                    {
                        territorios[i].PropietarioId = estado.territoriosPropietarios[i];
                        territorios[i].CantidadTropas = estado.territoriosTropas[i];
                    }

                    Debug.Log("Estado del juego actualizado desde el servidor");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error actualizando estado: {e.Message}");
            }
        }

        #endregion

        #region Env�o de Mensajes

        /// <summary>
        /// Env�a un mensaje a trav�s de la red
        /// </summary>
        public void EnviarMensaje(string tipo, string datos)
        {
            MensajeRed mensaje = new MensajeRed
            {
                tipo = tipo,
                datos = datos,
                jugadorId = esServidor ? 1 : 2
            };

            if (esServidor && servidor != null)
            {
                servidor.EnviarATodos(mensaje);
            }
            else if (!esServidor && cliente != null)
            {
                cliente.EnviarMensaje(mensaje);
            }
        }

        /// <summary>
        /// Notifica selecci�n de territorio
        /// </summary>
        public void NotificarSeleccionTerritorio(string nombreTerritorio)
        {
            EnviarMensaje("SELECCIONAR_TERRITORIO", nombreTerritorio);
        }

        /// <summary>
        /// Notifica colocaci�n de tropas
        /// </summary>
        public void NotificarColocarTropas(string nombreTerritorio, int cantidad)
        {
            string datos = JsonConvert.SerializeObject(new { territorio = nombreTerritorio, tropas = cantidad });
            EnviarMensaje("COLOCAR_TROPAS", datos);
        }

        /// <summary>
        /// Notifica ataque realizado
        /// </summary>
        public void NotificarAtaque(string territorioAtacante, string territorioDefensor, string resultado)
        {
            string datos = JsonConvert.SerializeObject(new
            {
                atacante = territorioAtacante,
                defensor = territorioDefensor,
                resultado = resultado
            });
            EnviarMensaje("ATACAR", datos);
        }

        /// <summary>
        /// Notifica finalizaci�n de turno
        /// </summary>
        public void NotificarFinalizarTurno()
        {
            EnviarMensaje("FINALIZAR_TURNO", "Turno finalizado");
        }

        /// <summary>
        /// Env�a el estado completo del juego (solo servidor)
        /// </summary>
        public void EnviarEstadoCompleto()
        {
            if (!esServidor || gameManager == null) return;

            try
            {
                EstadoJuego estado = ObtenerEstadoActual();
                string datosEstado = JsonConvert.SerializeObject(estado);
                EnviarMensaje("ESTADO_COMPLETO", datosEstado);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error enviando estado completo: {e.Message}");
            }
        }

        /// <summary>
        /// Obtiene el estado actual del juego
        /// </summary>
        private EstadoJuego ObtenerEstadoActual()
        {
            EstadoJuego estado = new EstadoJuego();

            var territorios = gameManager.GetTerritorios();
            if (territorios != null)
            {
                estado.territoriosPropietarios = new int[territorios.getSize()];
                estado.territoriosTropas = new int[territorios.getSize()];

                for (int i = 0; i < territorios.getSize(); i++)
                {
                    estado.territoriosPropietarios[i] = territorios[i].PropietarioId;
                    estado.territoriosTropas[i] = territorios[i].CantidadTropas;
                }

                estado.turnoActual = 1; // Puedes obtener esto del GameManager
                estado.cantidadJugadores = cantidadJugadores;
                estado.nombresJugadores = new string[] { nombreJugador };
            }

            return estado;
        }

        #endregion

        #region Eventos de Conexi�n

        private void OnClienteConectadoServidor(string nombreCliente)
        {
            Debug.Log($"Cliente conectado al administrador: {nombreCliente}");
            // Enviar estado actual al nuevo cliente
            EnviarEstadoCompleto();
        }

        private void OnClienteDesconectadoServidor()
        {
            Debug.Log("Cliente desconectado del administrador");
        }

        private void OnConectadoCliente()
        {
            Debug.Log("Administrador cliente conectado exitosamente");
            // Solicitar estado actual al servidor
            EnviarMensaje("SOLICITAR_ESTADO", "");
        }

        private void OnDesconectadoCliente()
        {
            Debug.Log("Administrador cliente desconectado");
        }

        #endregion

        #region M�todos P�blicos

        public bool EsServidor() => esServidor;
        public int GetCantidadJugadores() => cantidadJugadores;
        public string GetNombreJugador() => nombreJugador;
        public bool RedActiva() => (esServidor && servidor != null && servidor.ServidorActivo()) ||
                                   (!esServidor && cliente != null && cliente.EstaConectado());

        #endregion

        void OnDestroy()
        {
            if (servidor != null)
                servidor.DetenerServidor();

            if (cliente != null)
                cliente.Desconectar();
        }
    }
}