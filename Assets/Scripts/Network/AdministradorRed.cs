using UnityEngine;
using CrazyRisk.Managers;
using Newtonsoft.Json;

namespace CrazyRisk.Red
{
    public class AdministradorRed : MonoBehaviour
    {
        private ServidorRisk servidor;
        private ClienteRisk cliente;

        private bool esServidor;
        private int cantidadJugadores;
        private string nombreJugador;

        private GameManager gameManager;
        private int miJugadorId;
        private int jugadorEnTurno = 1;

        void Start()
        {
            DontDestroyOnLoad(gameObject);
            CargarConfiguracion();
            StartCoroutine(BuscarGameManagerYConectar());
        }

        private void CargarConfiguracion()
        {
            esServidor = PlayerPrefs.GetInt("EsServidor", 1) == 1;
            cantidadJugadores = PlayerPrefs.GetInt("CantidadJugadores", 2);
            nombreJugador = PlayerPrefs.GetString("NombreJugador", "Jugador1");

            Debug.Log($"AdministradorRed cargado: {nombreJugador} {(esServidor ? "(Servidor)" : "(Cliente)")} - {cantidadJugadores} jugadores");
        }

        private System.Collections.IEnumerator BuscarGameManagerYConectar()
        {
            while (gameManager == null)
            {
                gameManager = FindObjectOfType<GameManager>();
                yield return new UnityEngine.WaitForSeconds(0.1f);
            }

            if (esServidor)
                IniciarComoServidor();
            else
                IniciarComoCliente();
        }

        private void IniciarComoServidor()
        {
            servidor = gameObject.AddComponent<ServidorRisk>();
            servidor.OnMensajeRecibido = ProcesarMensajeServidor;
            servidor.OnClienteConectado += OnClienteConectadoServidor;
            servidor.OnClienteDesconectado += OnClienteDesconectadoServidor;

            if (servidor.IniciarServidor(nombreJugador))
            {
                miJugadorId = 1;
                Debug.Log("AdministradorRed funcionando como servidor - ID: 1");
            }
        }

        private void IniciarComoCliente()
        {
            string ipServidor = PlayerPrefs.GetString("IP", "127.0.0.1");

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

        private void ProcesarMensajeServidor(MensajeRed mensaje)
        {
            Debug.Log($"[Servidor] Procesando: {mensaje.tipo} de jugador {mensaje.jugadorId}");

            switch (mensaje.tipo)
            {
                case "ACCION_JUEGO":
                    if (gameManager != null)
                    {
                        gameManager.EjecutarAccionDesdeRed(mensaje.datos);
                    }
                    break;

                case "CAMBIO_TURNO":
                    jugadorEnTurno = int.Parse(mensaje.datos);
                    if (gameManager != null)
                    {
                        gameManager.ActualizarTurnoDesdeRed(jugadorEnTurno);
                    }
                    break;

                case "SOLICITAR_ESTADO_GAMEMANAGER":
                    if (gameManager != null)
                    {
                        EstadoJuego estado = gameManager.ObtenerEstadoActual();
                        int clienteId = int.Parse(mensaje.datos);
                        servidor.EnviarEstadoACliente(clienteId, estado);
                    }
                    break;
            }
        }

        private void ProcesarMensajeCliente(MensajeRed mensaje)
        {
            Debug.Log($"[Cliente] Procesando: {mensaje.tipo}");

            switch (mensaje.tipo)
            {
                case "CONEXION_ACEPTADA":
                    DatosJugador datosAceptacion = JsonConvert.DeserializeObject<DatosJugador>(mensaje.datos);
                    miJugadorId = datosAceptacion.id;
                    Debug.Log($"Me asignaron ID: {miJugadorId}");
                    break;

                case "ACTUALIZAR_NOMBRES":
                    ActualizarNombresJugadores(mensaje.datos);
                    break;

                case "ESTADO_COMPLETO":
                    ActualizarEstadoLocal(mensaje);
                    break;

                case "ACCION_JUEGO":
                    if (gameManager != null)
                    {
                        gameManager.EjecutarAccionDesdeRed(mensaje.datos);
                    }
                    break;

                case "ACCION_RECHAZADA":
                    Debug.LogWarning($"Acción rechazada: {mensaje.datos}");
                    break;

                case "CAMBIO_TURNO":
                    jugadorEnTurno = int.Parse(mensaje.datos);
                    if (gameManager != null)
                    {
                        gameManager.ActualizarTurnoDesdeRed(jugadorEnTurno);
                    }
                    break;
            }
        }
        private void ActualizarNombresJugadores(string datosJson)
        {
            string[] nombres = JsonConvert.DeserializeObject<string[]>(datosJson);

            if (gameManager != null)
            {
                // Actualizar nombres en el GameManager
                if (nombres.Length > 0)
                    gameManager.GetJugador1().setNombre(nombres[0]);
                if (nombres.Length > 1)
                    gameManager.GetJugador2().setNombre(nombres[1]);
                if (nombres.Length > 2 && gameManager.GetJugador3() != null)
                    gameManager.GetJugador3().setNombre(nombres[2]);
            }
        }
        private void ActualizarEstadoLocal(MensajeRed mensaje)
        {
            try
            {
                EstadoJuego estado = JsonConvert.DeserializeObject<EstadoJuego>(mensaje.datos);

                if (gameManager != null)
                {
                    gameManager.ActualizarDesdeEstadoCompleto(estado);
                    jugadorEnTurno = estado.turnoActual;

                    if (miJugadorId == 0 && estado.cantidadJugadores > 0)
                    {
                        miJugadorId = estado.cantidadJugadores;
                    }
                }

                Debug.Log($"Estado del juego actualizado - Turno: {jugadorEnTurno}, Mi ID: {miJugadorId}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error actualizando estado: {e.Message}");
            }
        }

        #endregion

        public void EnviarAccionJuego(AccionJuego accion)
        {
            accion.jugadorId = miJugadorId;
            string datosJson = JsonConvert.SerializeObject(accion);

            MensajeRed mensaje = new MensajeRed
            {
                tipo = "ACCION_JUEGO",
                datos = datosJson,
                jugadorId = miJugadorId
            };

            if (esServidor && servidor != null)
            {
                servidor.EnviarATodos(mensaje);
                if (gameManager != null)
                {
                    gameManager.EjecutarAccionDesdeRed(datosJson);
                }
            }
            else if (!esServidor && cliente != null)
            {
                cliente.EnviarMensaje(mensaje);
            }
        }

        public void NotificarFinTurno()
        {
            MensajeRed mensaje = new MensajeRed
            {
                tipo = "FIN_TURNO",
                datos = "",
                jugadorId = miJugadorId
            };

            if (esServidor && servidor != null)
            {
                servidor.EnviarATodos(mensaje);
            }
            else if (cliente != null)
            {
                cliente.EnviarMensaje(mensaje);
            }
        }

        public int GetMiJugadorId() => miJugadorId;
        public int GetJugadorEnTurno() => jugadorEnTurno;
        public bool EsMiTurno() => miJugadorId == jugadorEnTurno;

        #region Envío de Mensajes

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

        public void NotificarSeleccionTerritorio(string nombreTerritorio)
        {
            EnviarMensaje("SELECCIONAR_TERRITORIO", nombreTerritorio);
        }

        public void NotificarColocarTropas(string nombreTerritorio, int cantidad)
        {
            string datosMsg = JsonConvert.SerializeObject(new { territorio = nombreTerritorio, tropas = cantidad });
            EnviarMensaje("COLOCAR_TROPAS", datosMsg);
        }

        public void NotificarAtaque(string territorioAtacante, string territorioDefensor, string resultado)
        {
            string datosMsg = JsonConvert.SerializeObject(new
            {
                atacante = territorioAtacante,
                defensor = territorioDefensor,
                resultado = resultado
            });
            EnviarMensaje("ATACAR", datosMsg);
        }

        public void NotificarFinalizarTurno()
        {
            EnviarMensaje("FINALIZAR_TURNO", "Turno finalizado");
        }

        public void EnviarEstadoCompleto()
        {
            if (!esServidor || gameManager == null) return;

            try
            {
                EstadoJuego estado = gameManager.ObtenerEstadoActual();
                string datosEstado = JsonConvert.SerializeObject(estado);
                EnviarMensaje("ESTADO_COMPLETO", datosEstado);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error enviando estado completo: {e.Message}");
            }
        }

        #endregion

        #region Eventos de Conexión

        private void OnClienteConectadoServidor(string nombreCliente)
        {
            Debug.Log($"Cliente conectado al administrador: {nombreCliente}");
            EnviarEstadoCompleto();
        }

        private void OnClienteDesconectadoServidor()
        {
            Debug.Log("Cliente desconectado del administrador");
        }

        private void OnConectadoCliente()
        {
            Debug.Log("Administrador cliente conectado exitosamente");
            EnviarMensaje("SOLICITAR_ESTADO", "");
        }

        private void OnDesconectadoCliente()
        {
            Debug.Log("Administrador cliente desconectado");
        }

        #endregion

        #region Métodos Públicos

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