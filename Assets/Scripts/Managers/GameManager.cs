using UnityEngine;
using CrazyRisk.LogicaJuego;
using CrazyRisk.Modelos;
using CrazyRisk.Estructuras;
using CrazyRisk.Red;
using TMPro;
using Newtonsoft.Json;

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
        private Jugador jugador1, jugador2, jugador3, jugadorNeutral;
        private DetectorVictoria detectorVictoria;
        private ManejadorAtaques manejadorAtaques = new ManejadorAtaques();
        private ManejadorPlaneacion manejadorPlaneacion;

        //Estado de preparacion
        private bool enFasePreparacion = false;
        private bool crearNeutral = true;
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
            manejadorAtaques = new ManejadorAtaques();
            manejadorPlaneacion = new ManejadorPlaneacion();

            VerificarJuegoEnRed();
            InicializarJuego();

            InvokeRepeating("VerificarEstadoPartida", 2f, 2f);
        }

        private void VerificarJuegoEnRed()
        {
            if (PlayerPrefs.HasKey("NombreJugador"))
            {
                int modoSolo = PlayerPrefs.GetInt("ModoSolo", 0);

                if (modoSolo == 1)
                {
                    esJuegoEnRed = false;
                    cantidadJugadoresRed = 2;
                    crearNeutral = true;  // Modo solitario siempre con neutral
                    Debug.Log("Modo solitario detectado - Sin networking");
                    return;
                }

                esJuegoEnRed = true;
                cantidadJugadoresRed = PlayerPrefs.GetInt("CantidadJugadores", 2);

                // Si son 3 jugadores en red, NO crear neutral
                crearNeutral = (cantidadJugadoresRed == 2);

                string nombreRed = PlayerPrefs.GetString("NombreJugador", "Jugador1");
                bool esServidor = PlayerPrefs.GetInt("EsServidor", 1) == 1;

                if (esServidor)
                    nombreJugador1 = nombreRed;
                else
                    nombreJugador2 = nombreRed;

                CrearAdministradorRed();
                Debug.Log($"Juego en red detectado: {cantidadJugadoresRed} jugadores - Neutral: {crearNeutral}");
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
            {
                inicializadorJuego.InicializarJuegoCompleto(
                    nombreJugador1, colorJugador1,
                    nombreJugador2, colorJugador2,
                    cantidadJugadoresRed, crearNeutral);
            }

            territoriosLogica = inicializadorJuego.getTerritorios();
            if (manejadorPlaneacion != null)
            {
                manejadorPlaneacion.InicializarConTerritorios(territoriosLogica);
            }

            Lista<Jugador> jugadores = inicializadorJuego.getJugadores();

            jugador1 = jugadores[0];
            jugador2 = jugadores[1];

            if (jugadores.getSize() == 3)
            {
                // Verificar si es neutral o jugador 3
                if (jugadores[2].getEsNeutral())
                    jugadorNeutral = jugadores[2];
                else
                    jugador3 = jugadores[2];
            }

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
            distribuidor.OnTropaColocada += ActualizarTerritorioEspecifico;
            distribuidor.OnCambioTurno += ActualizarPanelDatos;
            distribuidor.OnPreparacionCompletada += FinalizarFasePreparacion;

            // Crear lista de IDs activos
            Lista<int> idsActivos = new Lista<int>();
            idsActivos.Agregar(1);
            idsActivos.Agregar(2);
            int idNeutral = -1;

            if (jugador3 != null)
                idsActivos.Agregar(3);
            else if (jugadorNeutral != null)
            {
                idsActivos.Agregar(3);
                idNeutral = 3; // Marcar que el ID 3 es neutral
            }

            Lista<Jugador> jugadores = inicializadorJuego.getJugadores();
            distribuidor.IniciarFasePreparacion(idsActivos, idNeutral, jugadores);

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
            jugador1.setTerritoriosControlados(new Lista<Territorio>());
            jugador2.setTerritoriosControlados(new Lista<Territorio>());

            if (jugador3 != null)
                jugador3.setTerritoriosControlados(new Lista<Territorio>());
            if (jugadorNeutral != null)
                jugadorNeutral.setTerritoriosControlados(new Lista<Territorio>());

            for (int i = 0; i < territoriosLogica.getSize(); i++)
            {
                Territorio territorio = territoriosLogica[i];

                if (territorio.PropietarioId == 1)
                    jugador1.getTerritoriosControlados().Agregar(territorio);
                else if (territorio.PropietarioId == 2)
                    jugador2.getTerritoriosControlados().Agregar(territorio);
                else if (territorio.PropietarioId == 3)
                {
                    if (jugador3 != null)
                        jugador3.getTerritoriosControlados().Agregar(territorio);
                    else if (jugadorNeutral != null)
                        jugadorNeutral.getTerritoriosControlados().Agregar(territorio);
                }
            }

            Debug.Log($"✓ Territorios actualizados:");
            Debug.Log($"  {jugador1.getNombre()}: {jugador1.getCantidadTerritorios()} territorios");
            Debug.Log($"  {jugador2.getNombre()}: {jugador2.getCantidadTerritorios()} territorios");
            if (jugador3 != null)
                Debug.Log($"  {jugador3.getNombre()}: {jugador3.getCantidadTerritorios()} territorios");
            if (jugadorNeutral != null)
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
                string nombreJugador = ObtenerNombreJugador(jugadorActualId);

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
            if (!enFasePreparacion || distribuidor == null)
                return false;

            // MODO RED: Enviar acción al servidor
            if (esJuegoEnRed && administradorRed != null)
            {
                if (!administradorRed.EsMiTurno())
                {
                    Debug.LogWarning("No es tu turno");
                    return false;
                }

                AccionJuego accion = new AccionJuego
                {
                    tipo = "COLOCAR_TROPA_PREPARACION",
                    territorioOrigen = nombreTerritorio,
                    cantidad = 1
                };

                administradorRed.EnviarAccionJuego(accion);

                // NO ejecutar localmente, esperar confirmación del servidor
                return true;
            }

            // MODO LOCAL: Ejecutar directamente
            bool exito = distribuidor.IntentarColocarTropa(nombreTerritorio);
            if (exito)
            {
                TerritorioUI territorioUI = BuscarTerritorioUIPorNombre(nombreTerritorio);
                if (territorioUI != null)
                {
                    territorioUI.ActualizarInterfaz();
                }
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
            string nombre1 = nombreJugador1;  
            string nombre2 = nombreJugador2;  

            bool esServidor = PlayerPrefs.GetInt("EsServidor", 1) == 1;
            string nombreRed = PlayerPrefs.GetString("NombreJugador", "Jugador1");

            if (esServidor)
                nombre1 = nombreRed;
            else
                nombre2 = nombreRed;

            inicializadorJuego.InicializarJuegoCompleto(
                nombre1, colorJugador1,
                nombre2, colorJugador2,
                cantidadJugadoresRed, crearNeutral);
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
                {
                    textoGanador.text = $"Ha ganado {ganador.getNombre()}\n¡Es toda una máquina!";
                }

                DesactivarControlesJuego();
            }

            Debug.Log($"VICTORIA! {ganador.getNombre()} ha ganado la partida");
        }

        private void DesactivarControlesJuego()
        {
            foreach (TerritorioUI territorio in territoriosUI)
            {
                if (territorio != null)
                {
                    territorio.enabled = false; 
                }
            }

            if (manejadorTurnos != null)
            {
                manejadorTurnos.enabled = false;
            }
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

        private string ObtenerNombreJugador(int jugadorId)
        {
            if (jugadorId == 1 && jugador1 != null)
                return jugador1.getNombre();
            else if (jugadorId == 2 && jugador2 != null)
                return jugador2.getNombre();
            else if (jugadorId == 3)
            {
                if (jugador3 != null)
                    return jugador3.getNombre();
                else if (jugadorNeutral != null)
                    return jugadorNeutral.getNombre();
            }

            return $"Jugador {jugadorId}";
        }

        private Color ObtenerColorPorJugador(int jugadorId)
        {
            if (jugadorId == 1)
                return Color.green;
            else if (jugadorId == 2)
                return new Color(0.5f, 0f, 0.8f);  // Morado
            else if (jugadorId == 3)
            {
                if (jugador3 != null)
                    return Color.cyan;  // Azul para jugador 3
                else
                    return Color.gray;  // Gris para neutral
            }

            return Color.white;
        }

        // Agregar getter
        public Jugador GetJugador3() => jugador3;

        private void MostrarEstadisticas()
        {
            Debug.Log("=== ESTADÍSTICAS DEL JUEGO ===");
            Debug.Log($"{jugador1.getNombre()}: {jugador1.getCantidadTerritorios()} territorios, {jugador1.getTotalTropas()} tropas");
            Debug.Log($"{jugador2.getNombre()}: {jugador2.getCantidadTerritorios()} territorios, {jugador2.getTotalTropas()} tropas");

            if (jugador3 != null)
                Debug.Log($"{jugador3.getNombre()}: {jugador3.getCantidadTerritorios()} territorios, {jugador3.getTotalTropas()} tropas");
            else if (jugadorNeutral != null)
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
                if (idActual == 3)
                    return jugador3 != null ? jugador3 : jugadorNeutral;
            }
            else if (manejadorTurnos != null)
            {
                return manejadorTurnos.GetJugadorActual();
            }

            return null;
        }
        public void EjecutarAccionDesdeRed(string datosJson)
        {
            try
            {
                AccionJuego accion = JsonConvert.DeserializeObject<AccionJuego>(datosJson);

                switch (accion.tipo)
                {
                    case "COLOCAR_TROPA_PREPARACION":
                        EjecutarColocarTropaPreparacion(accion.territorioOrigen);
                        break;

                    case "COLOCAR_REFUERZO":
                        EjecutarColocarRefuerzo(accion.territorioOrigen);
                        break;

                    case "ATACAR":
                        EjecutarAtaqueRed(accion);
                        break;

                    case "MOVER_TROPAS":
                        EjecutarMovimientoTropas(accion);
                        break;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error ejecutando acción desde red: {e.Message}");
            }
        }

        private void EjecutarColocarTropaPreparacion(string nombreTerritorio)
        {
            if (distribuidor != null && distribuidor.EstaEnFasePreparacion())
            {
                distribuidor.IntentarColocarTropa(nombreTerritorio);
                TerritorioUI territorioUI = BuscarTerritorioUIPorNombre(nombreTerritorio);
                if (territorioUI != null)
                {
                    territorioUI.ActualizarInterfaz();
                }
                ActualizarPanelDatos();
            }
        }

        private void EjecutarColocarRefuerzo(string nombreTerritorio)
        {
            Territorio territorio = BuscarTerritorioPorNombre(nombreTerritorio);
            if (territorio != null && manejadorTurnos != null)
            {
                territorio.AgregarTropas(1);
                manejadorTurnos.UsarRefuerzo();

                TerritorioUI territorioUI = BuscarTerritorioUIPorNombre(nombreTerritorio);
                if (territorioUI != null)
                {
                    territorioUI.ActualizarInterfaz();
                }
            }
        }

        private void EjecutarAtaqueRed(AccionJuego accion)
        {
            Territorio atacante = BuscarTerritorioPorNombre(accion.territorioOrigen);
            Territorio defensor = BuscarTerritorioPorNombre(accion.territorioDestino);

            if (atacante == null || defensor == null)
                return;

            if (manejadorAtaques == null)
                manejadorAtaques = new ManejadorAtaques();

            manejadorAtaques.SeleccionarAtacante(atacante, accion.jugadorId);
            manejadorAtaques.SeleccionarDefensor(defensor, accion.jugadorId);

            ManejadorAtaques.ResultadoAtaque resultado = manejadorAtaques.EjecutarAtaqueConDados(
                accion.dadosAtacante,
                accion.dadosDefensor
            );


            if (resultado != null)
            {
                ActualizarTerritorioEspecifico(accion.territorioOrigen);
                ActualizarTerritorioEspecifico(accion.territorioDestino);

                if (resultado.conquistado)
                {
                    Color colorJugador = ObtenerColorPorJugador(accion.jugadorId);
                    TerritorioUI defensorUI = BuscarTerritorioUIPorNombre(accion.territorioDestino);
                    if (defensorUI != null)
                    {
                        defensorUI.CambiarColor(colorJugador);
                    }
                }
            }
        }

        private void EjecutarMovimientoTropas(AccionJuego accion)
        {
            Territorio origen = BuscarTerritorioPorNombre(accion.territorioOrigen);
            Territorio destino = BuscarTerritorioPorNombre(accion.territorioDestino);

            if (origen != null && destino != null)
            {
                origen.CantidadTropas -= accion.cantidad;
                destino.CantidadTropas += accion.cantidad;

                ActualizarTerritorioEspecifico(accion.territorioOrigen);
                ActualizarTerritorioEspecifico(accion.territorioDestino);
            }
        }

        private Territorio BuscarTerritorioPorNombre(string nombre)
        {
            for (int i = 0; i < territoriosLogica.getSize(); i++)
            {
                if (territoriosLogica[i].Nombre == nombre)
                {
                    return territoriosLogica[i];
                }
            }
            return null;
        }

        public EstadoJuego ObtenerEstadoActual()
        {
            EstadoJuego estado = new EstadoJuego();

            if (territoriosLogica != null)
            {
                estado.territoriosPropietarios = new int[territoriosLogica.getSize()];
                estado.territoriosTropas = new int[territoriosLogica.getSize()];

                for (int i = 0; i < territoriosLogica.getSize(); i++)
                {
                    estado.territoriosPropietarios[i] = territoriosLogica[i].PropietarioId;
                    estado.territoriosTropas[i] = territoriosLogica[i].CantidadTropas;
                }

                estado.enFasePreparacion = enFasePreparacion;

                if (manejadorTurnos != null)
                {
                    estado.faseActual = manejadorTurnos.GetFaseActual().ToString();
                    estado.refuerzosDisponibles = manejadorTurnos.GetRefuerzosDisponibles();
                }
            }

            return estado;
        }

        public void ActualizarDesdeEstadoCompleto(EstadoJuego estado)
        {
            if (territoriosLogica == null || estado.territoriosPropietarios == null)
                return;

            for (int i = 0; i < territoriosLogica.getSize() && i < estado.territoriosPropietarios.Length; i++)
            {
                territoriosLogica[i].PropietarioId = estado.territoriosPropietarios[i];
                territoriosLogica[i].CantidadTropas = estado.territoriosTropas[i];
            }

            ActualizarColoresTerritorios();
            ActualizarVisualizacionCompleta();
        }

        public void ActualizarTurnoDesdeRed(int jugadorIdEnTurno)
        {
            Debug.Log($"Turno actualizado desde red: Jugador {jugadorIdEnTurno}");
        }

        public bool EsMiTurno()
        {
            if (esJuegoEnRed && administradorRed != null)
            {
                return administradorRed.EsMiTurno();
            }
            return true;
        }

        public ManejadorAtaques GetManejadorAtaques() => manejadorAtaques;
        public ManejadorPlaneacion GetManejadorPlaneacion() => manejadorPlaneacion;

        void OnDestroy()
        {
            TerritorioUI.LimpiarSeleccionesEstaticas();
        }
    }
}