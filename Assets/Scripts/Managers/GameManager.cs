using UnityEngine;
using CrazyRisk.LogicaJuego;
using CrazyRisk.Modelos;
using CrazyRisk.Estructuras;
using CrazyRisk.Red;
using TMPro;

namespace CrazyRisk.Managers
{
    public class GameManager : MonoBehaviour
    {
        [Header("Referencias de Objetos en Escena")]
        [SerializeField] private TerritorioUI[] territoriosUI;

        [Header("Configuración del Juego")]
        [SerializeField] private string nombreJugador1 = "Jugador 1";
        [SerializeField] private string colorJugador1 = "Verde";
        [SerializeField] private string nombreJugador2 = "Jugador 2";
        [SerializeField] private string colorJugador2 = "Morado";

        [Header("Sistema de Turnos")]
        [SerializeField] private ManejadorTurnos manejadorTurnos;
        [SerializeField] private TextMeshProUGUI textoInformacion;

        [Header("Panel de Victoria")]
        [SerializeField] private GameObject panelVictoria;
        [SerializeField] private TextMeshProUGUI textoGanador;

        [Header("Panel datos")]
        [SerializeField] private GameObject PanelDatos;
        [SerializeField] private TextMeshProUGUI DatosFase;

        // Sistema de lógica del juego
        private InicializadorJuego inicializadorJuego;
        private Lista<Territorio> territoriosLogica;
        private Jugador jugador1, jugador2, jugadorNeutral;
        private DetectorVictoria detectorVictoria;
        private ManejadorAtaques manejadorAtaques = new ManejadorAtaques();

        //Estado de preparacion
        private bool enFasePreparacion = false;
        private DistribuidorTerritorios distribuidor;

        // Sistema de red
        private AdministradorRed administradorRed;
        private bool esJuegoEnRed = false;
        private int cantidadJugadoresRed = 2;

        void Start()
        {
            if (panelVictoria != null)
                panelVictoria.SetActive(false);

            if (PanelDatos != null)
                PanelDatos.SetActive(false);

            detectorVictoria = new DetectorVictoria();

            VerificarJuegoEnRed();
            InicializarJuego();

            InvokeRepeating("VerificarEstadoPartida", 2f, 2f);
        }

        public ManejadorAtaques GetManejadorAtaques() => manejadorAtaques;

        private void VerificarJuegoEnRed()
        {
            if (PlayerPrefs.HasKey("NombreJugador"))
            {
                int modoSolo = PlayerPrefs.GetInt("ModoSolo", 0);

                if (modoSolo == 1)
                {
                    esJuegoEnRed = false;
                    cantidadJugadoresRed = 2;
                    Debug.Log("Modo solitario detectado - Sin networking");
                    return;
                }

                esJuegoEnRed = true;
                cantidadJugadoresRed = PlayerPrefs.GetInt("CantidadJugadores", 2);

                string nombreRed = PlayerPrefs.GetString("NombreJugador", "Jugador1");
                bool esServidor = PlayerPrefs.GetInt("EsServidor", 1) == 1;

                if (esServidor)
                    nombreJugador1 = nombreRed;
                else
                    nombreJugador2 = nombreRed;

                CrearAdministradorRed();
                Debug.Log($"Juego en red detectado: {cantidadJugadoresRed} jugadores");
            }
        }

        private void CrearAdministradorRed()
        {
            GameObject adminObj = new GameObject("AdministradorRed");
            administradorRed = adminObj.AddComponent<AdministradorRed>();
            Debug.Log("AdministradorRed creado y conectado al GameManager");
        }

        private void InicializarJuego()
        {
            Debug.Log("=== INICIANDO CRAZY RISK ===");

            inicializadorJuego = new InicializadorJuego();

            if (esJuegoEnRed)
                InicializarJuegoEnRed();
            else
                inicializadorJuego.InicializarJuegoCompleto(nombreJugador1, colorJugador1, nombreJugador2, colorJugador2);

            territoriosLogica = inicializadorJuego.getTerritorios();
            Lista<Jugador> jugadores = inicializadorJuego.getJugadores();

            jugador1 = jugadores[0];
            jugador2 = jugadores[1];
            jugadorNeutral = jugadores[2];

            distribuidor = inicializadorJuego.GetDistribuidor();

            BuscarTerritoriosEnEscena();
            ConectarLogicaConInterfaz();
            ActualizarColoresTerritorios();

            Debug.Log("Juego inicializado correctamente");
            MostrarEstadisticas();

            IniciarFasePreparacion();
        }

        /// <summary>
        /// Inicia la fase de preparación manual
        /// </summary>
        private void IniciarFasePreparacion()
        {
            enFasePreparacion = true;

            if (distribuidor == null)
            {
                Debug.LogError("❌ Distribuidor no está disponible");
                return;
            }

            Debug.Log(">>> Suscribiendo eventos del distribuidor...");

            distribuidor.OnTropaColocada += ActualizarTerritorioEspecifico;
            distribuidor.OnCambioTurno += ActualizarPanelDatos;
            distribuidor.OnPreparacionCompletada += FinalizarFasePreparacion;

            Debug.Log(">>> Eventos suscritos correctamente");

            distribuidor.IniciarFasePreparacion(1, 2, 3);

            if (PanelDatos != null)
                PanelDatos.SetActive(true);

            ActualizarPanelDatos();
        }

        /// <summary>
        /// Actualiza un territorio específico cuando se coloca una tropa
        /// </summary>
        public void ActualizarTerritorioEspecifico(string nombreTerritorio)
        {
            TerritorioUI territorio = BuscarTerritorioUIPorNombre(nombreTerritorio);

            if (territorio != null)
            {
                territorio.ActualizarInterfaz();
                Debug.Log($"✓ UI actualizada para: {nombreTerritorio}");
            }
            else
            {
                Debug.LogWarning($"No se encontró TerritorioUI para: {nombreTerritorio}");
            }

            
        }

        /// <summary>
        /// Actualiza la lista de territorios controlados por cada jugador para luego iniciar la fase
        /// </summary>
        private void ActualizarTerritoriosJugadores()
        {
            // Limpiar listas actuales
            jugador1.setTerritoriosControlados(new Lista<Territorio>());
            jugador2.setTerritoriosControlados(new Lista<Territorio>());
            jugadorNeutral.setTerritoriosControlados(new Lista<Territorio>());

            // Reagrupar territorios según el propietario actual
            for (int i = 0; i < territoriosLogica.getSize(); i++)
            {
                Territorio territorio = territoriosLogica[i];

                if (territorio.PropietarioId == jugador1.getId())
                {
                    jugador1.getTerritoriosControlados().Agregar(territorio);
                }
                else if (territorio.PropietarioId == jugador2.getId())
                {
                    jugador2.getTerritoriosControlados().Agregar(territorio);
                }
                else if (territorio.PropietarioId == jugadorNeutral.getId())
                {
                    jugadorNeutral.getTerritoriosControlados().Agregar(territorio);
                }
            }

            Debug.Log($"✓ Territorios actualizados:");
            Debug.Log($"  {jugador1.getNombre()}: {jugador1.getCantidadTerritorios()} territorios");
            Debug.Log($"  {jugador2.getNombre()}: {jugador2.getCantidadTerritorios()} territorios");
            Debug.Log($"  Neutral: {jugadorNeutral.getCantidadTerritorios()} territorios");
        }

        /// <summary>
        /// Actualiza el panel de datos según el estado actual del juego
        /// </summary>
        public void ActualizarPanelDatos()
        {
            if (DatosFase == null) return;

            // Durante preparación
            if (enFasePreparacion && distribuidor != null)
            {
                int jugadorActualId = distribuidor.GetJugadorActual();
                int tropasRestantes = distribuidor.GetTropasRestantesJugadorActual();
                string nombreJugador = distribuidor.GetNombreJugadorActual();

                DatosFase.text = $"FASE DE PREPARACIÓN\n" +
                                $"Turno: {nombreJugador}\n" +
                                $"Tropas restantes: {tropasRestantes}";
            }
            // Durante el juego normal
            else if (manejadorTurnos != null)
            {
                Jugador jugadorActual = manejadorTurnos.GetJugadorActual();
                string fase = manejadorTurnos.GetFaseActual().ToString();
                int refuerzos = manejadorTurnos.GetRefuerzosDisponibles();

                string textoFase = fase switch
                {
                    "Refuerzos" => $"FASE DE REFUERZOS\n" +
                                  $"Turno: {jugadorActual.getNombre()}\n" +
                                  $"Refuerzos restantes: {refuerzos}",

                    "Ataque" => $"FASE DE ATAQUE\n" +
                               $"Turno: {jugadorActual.getNombre()}\n" +
                               $"Selecciona territorio para atacar",

                    "Planeacion" => $"FASE DE PLANEACIÓN\n" +
                                   $"Turno: {jugadorActual.getNombre()}\n" +
                                   $"Mueve tropas entre tus territorios",

                    _ => $"Turno: {jugadorActual.getNombre()}"
                };

                DatosFase.text = textoFase;
            }
        }

        /// <summary>
        /// Actualiza la visualización de todos los territorios
        /// </summary>
        private void ActualizarVisualizacionCompleta()
        {
            foreach (TerritorioUI territorioUI in territoriosUI)
            {
                if (territorioUI != null)
                {
                    territorioUI.ActualizarInterfaz();
                }
            }
        }

        /// <summary>
        /// Intenta colocar una tropa durante la preparación
        /// </summary>
        public bool IntentarColocarTropaPreparacion(string nombreTerritorio)
        {
            Debug.Log($">>> IntentarColocarTropaPreparacion llamado para: {nombreTerritorio}");
            Debug.Log($"    enFasePreparacion: {enFasePreparacion}");
            Debug.Log($"    distribuidor != null: {distribuidor != null}");

            if (!enFasePreparacion || distribuidor == null)
            {
                Debug.LogWarning("No estás en fase de preparación");
                return false;
            }

            bool exito = distribuidor.IntentarColocarTropa(nombreTerritorio);

            if (exito)
            {
                // Actualizar visualización del territorio
                TerritorioUI territorioUI = BuscarTerritorioUIPorNombre(nombreTerritorio);
                if (territorioUI != null)
                {
                    territorioUI.ActualizarInterfaz();
                    Debug.Log($"UI actualizada para: {nombreTerritorio}");
                }

                // Actualizar UI de preparación
                ActualizarPanelDatos();

            }

            return exito;
        }

        /// <summary>
        /// Finaliza la fase de preparación e inicia el juego normal
        /// </summary>
        private void FinalizarFasePreparacion()
        {
            Debug.Log(">>> GAME MANAGER: FinalizarFasePreparacion LLAMADO <<<");
            enFasePreparacion = false;

            Debug.Log("=== FINALIZANDO FASE DE PREPARACIÓN ===");

            // Desuscribirse de eventos
            distribuidor.OnTropaColocada -= ActualizarTerritorioEspecifico;
            distribuidor.OnCambioTurno -= ActualizarPanelDatos;
            distribuidor.OnPreparacionCompletada -= FinalizarFasePreparacion;

            // cambiar el texto del panel de información sobre la fase
            ActualizarPanelDatos();
            

            // Actualizar territorios antes de iniciar turnos
            ActualizarTerritoriosJugadores();

            Debug.Log("🎮 ¡FASE DE PREPARACIÓN COMPLETADA! Iniciando partida...");

            // Iniciar el sistema de turnos
            InicializarSistemaTurnos();
        }

        private void InicializarJuegoEnRed()
        {
            inicializadorJuego.InicializarJuegoCompleto(nombreJugador1, colorJugador1, nombreJugador2, colorJugador2);
        }

        private void InicializarSistemaTurnos()
        {
            if (manejadorTurnos == null)
            {
                GameObject turnosObj = new GameObject("ManejadorTurnos");
                manejadorTurnos = turnosObj.AddComponent<ManejadorTurnos>();
            }

            Lista<Jugador> jugadores = inicializadorJuego.getJugadores();
            Lista<Continente> continentes = inicializadorJuego.getContinentes();

            //Actualizar territorios de cada jugador antes de iniciar
            ActualizarTerritoriosJugadores();


            manejadorTurnos.InicializarTurnos(jugadores, continentes);

            Debug.Log("Sistema de turnos inicializado");
        }

        private void VerificarEstadoPartida()
        {
            if (inicializadorJuego == null) return;

            Lista<Jugador> jugadores = inicializadorJuego.getJugadores();
            EstadoPartida estado = detectorVictoria.VerificarEstadoPartida(jugadores);

            if (estado.juegoTerminado)
            {
                MostrarPanelVictoria(estado.ganador);
            }
        }

        /// <summary>
        /// Actualiza la UI durante la fase de preparación
        /// </summary>
        private void ActualizarDurantePreparacion()
        {
            if (enFasePreparacion)
            {
                ActualizarVisualizacionCompleta();
                ActualizarPanelDatos();
            }
        }

        private void MostrarPanelVictoria(Jugador ganador)
        {
            if (panelVictoria != null)
            {
                ManagerSonidos.Instance?.ReproducirVictoria();

                panelVictoria.SetActive(true);

                if (textoGanador != null)
                    textoGanador.text = $"{ganador.getNombre()} ha conquistado el mundo!";

                Time.timeScale = 0;
            }

            Debug.Log($"VICTORIA! {ganador.getNombre()} ha ganado la partida");
        }

        private void BuscarTerritoriosEnEscena()
        {
            TerritorioUI[] territoriosEncontrados = FindObjectsOfType<TerritorioUI>();
            Debug.Log($"Encontrados {territoriosEncontrados.Length} territorios en la escena");

            if (territoriosEncontrados.Length != 42)
                Debug.LogWarning($"Se esperaban 42 territorios, pero se encontraron {territoriosEncontrados.Length}");

            territoriosUI = territoriosEncontrados;
        }

        private void ConectarLogicaConInterfaz()
        {
            for (int i = 0; i < territoriosLogica.getSize(); i++)
            {
                Territorio territorioLogico = territoriosLogica[i];
                TerritorioUI territorioVisual = BuscarTerritorioUIPorNombre(territorioLogico.Nombre);

                if (territorioVisual != null)
                {
                    territorioVisual.InicializarTerritorio(territorioLogico);

                    if (esJuegoEnRed && administradorRed != null)
                        ConectarEventosRed(territorioVisual);

                    Debug.Log($"Conectado: {territorioLogico.Nombre} - Propietario: {territorioLogico.PropietarioId} - Tropas: {territorioLogico.CantidadTropas}");
                }
                else
                {
                    Debug.LogError($"No se encontró TerritorioUI para: {territorioLogico.Nombre}");
                }
            }
        }

        private void ConectarEventosRed(TerritorioUI territorioUI)
        {
            // Implementación futura
        }

        private TerritorioUI BuscarTerritorioUIPorNombre(string nombre)
        {
            foreach (TerritorioUI teritorioUI in territoriosUI)
            {
                if (teritorioUI.name == nombre || teritorioUI.GetNombreTerritorio() == nombre)
                    return teritorioUI;
            }
            return null;
        }

        private void ActualizarColoresTerritorios()
        {
            foreach (TerritorioUI territorioUI in territoriosUI)
            {
                if (territorioUI.GetTerritorioLogico() != null)
                {
                    int propietarioId = territorioUI.GetTerritorioLogico().PropietarioId;
                    Color colorTerritorio = ObtenerColorPorJugador(propietarioId);
                    territorioUI.CambiarColor(colorTerritorio);
                }
            }
        }

        private Color ObtenerColorPorJugador(int jugadorId)
        {
            if (jugadorId == jugador1.getId())
                return Color.green;
            else if (jugadorId == jugador2.getId())
                return new Color(0.5f, 0f, 0.8f);
            else if (jugadorId == jugadorNeutral.getId())
                return Color.gray;

            return Color.white;
        }

        private void MostrarEstadisticas()
        {
            Debug.Log("=== ESTADÍSTICAS DEL JUEGO ===");
            Debug.Log($"{jugador1.getNombre()}: {jugador1.getCantidadTerritorios()} territorios, {jugador1.getTotalTropas()} tropas");
            Debug.Log($"{jugador2.getNombre()}: {jugador2.getCantidadTerritorios()} territorios, {jugador2.getTotalTropas()} tropas");
            Debug.Log($"Neutral: {jugadorNeutral.getCantidadTerritorios()} territorios, {jugadorNeutral.getTotalTropas()} tropas");

            if (esJuegoEnRed)
                Debug.Log($"Modo Red: {cantidadJugadoresRed} jugadores - {(administradorRed?.EsServidor() == true ? "SERVIDOR" : "CLIENTE")}");
        }

        [ContextMenu("Redistribuir Territorios")]
        public void RedistribuirTerritorios()
        {
            InicializarJuego();
        }

        public void NotificarAccionRed(string tipoAccion, string datos)
        {
            if (esJuegoEnRed && administradorRed != null)
                administradorRed.EnviarMensaje(tipoAccion, datos);
        }

        public void ActualizarDesdeRed()
        {
            ActualizarColoresTerritorios();

            foreach (TerritorioUI territorioUI in territoriosUI)
            {
                if (territorioUI.GetTerritorioLogico() != null)
                    territorioUI.ActualizarInterfaz();
            }
        }

        public Lista<Territorio> GetTerritorios() => territoriosLogica;
        public Jugador GetJugador1() => jugador1;
        public Jugador GetJugador2() => jugador2;
        public Jugador GetJugadorNeutral() => jugadorNeutral;
        public bool EsJuegoEnRed() => esJuegoEnRed;
        public AdministradorRed GetAdministradorRed() => administradorRed;
        public int GetCantidadJugadoresRed() => cantidadJugadoresRed;
        public bool EstaEnFasePreparacion() => enFasePreparacion;

        /// <summary>
        /// Obtiene el jugador actual (preparación o juego)
        /// </summary>
        public Jugador GetJugadorActual()
        {
            if (enFasePreparacion && distribuidor != null)
            {
                int idActual = distribuidor.GetJugadorActual();

                if (idActual == 1) return jugador1;
                if (idActual == 2) return jugador2;
                if (idActual == 3) return jugadorNeutral;
            }
            else if (manejadorTurnos != null)
            {
                return manejadorTurnos.GetJugadorActual();
            }

            return null;
        }
    }
}