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

        // Sistema de lógica del juego
        private InicializadorJuego inicializadorJuego;
        private Lista<Territorio> territoriosLogica;
        private Jugador jugador1, jugador2, jugadorNeutral;
        private DetectorVictoria detectorVictoria;

        // Sistema de red
        private AdministradorRed administradorRed;
        private bool esJuegoEnRed = false;
        private int cantidadJugadoresRed = 2;

        void Start()
        {
            if (panelVictoria != null)
                panelVictoria.SetActive(false);

            detectorVictoria = new DetectorVictoria();
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

            BuscarTerritoriosEnEscena();
            ConectarLogicaConInterfaz();
            ActualizarColoresTerritorios();

            Debug.Log("Juego inicializado correctamente");
            MostrarEstadisticas();

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

        private void MostrarPanelVictoria(Jugador ganador)
        {
            if (panelVictoria != null)
            {
                panelVictoria.SetActive(true);

                if (textoGanador != null)
                    textoGanador.text = $"¡{ganador.getNombre()} ha conquistado el mundo!";

                Time.timeScale = 0;
            }

            Debug.Log($"¡VICTORIA! {ganador.getNombre()} ha ganado la partida");
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
    }
}