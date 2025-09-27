using UnityEngine;
using CrazyRisk.LogicaJuego;
using CrazyRisk.Modelos;
using CrazyRisk.Estructuras;

namespace CrazyRisk.Managers
{
    public class GameManager : MonoBehaviour
    {
        [Header("Referencias de Objetos en Escena")]
        [SerializeField] private TerritorioUI[] territoriosUI; // Array con todos los territorios de la escena

        [Header("Configuración del Juego")]
        [SerializeField] private string nombreJugador1 = "Jugador 1";
        [SerializeField] private string colorJugador1 = "Azul";
        [SerializeField] private string nombreJugador2 = "Jugador 2";
        [SerializeField] private string colorJugador2 = "Rojo";

        [Header("Colores para los territorios")]
        [SerializeField] private Color colorJugador1Unity = Color.blue;
        [SerializeField] private Color colorJugador2Unity = Color.red;
        [SerializeField] private Color colorNeutralUnity = Color.gray;

        // Sistema de lógica del juego
        private InicializadorJuego inicializadorJuego;
        private Lista<Territorio> territoriosLogica;
        private Jugador jugador1, jugador2, jugadorNeutral;

        void Start()
        {
            InicializarJuego();
        }

        /// <summary>
        /// Inicializa todo el sistema de juego
        /// </summary>
        private void InicializarJuego()
        {
            Debug.Log("=== INICIANDO CRAZY RISK ===");

            // 1. Crear el inicializador
            inicializadorJuego = new InicializadorJuego();

            // 2. Inicializar todo el sistema lógico
            inicializadorJuego.InicializarJuegoCompleto(nombreJugador1, colorJugador1, nombreJugador2, colorJugador2);

            // 3. Obtener datos del juego
            territoriosLogica = inicializadorJuego.getTerritorios();
            Lista<Jugador> jugadores = inicializadorJuego.getJugadores();

            jugador1 = jugadores[0];
            jugador2 = jugadores[1];
            jugadorNeutral = jugadores[2];

            // 4. Buscar territorios en la escena
            BuscarTerritoriosEnEscena();

            // 5. Conectar lógica con interfaz
            ConectarLogicaConInterfaz();

            // 6. Actualizar colores visuales
            ActualizarColoresTerritorios();

            Debug.Log("Juego inicializado correctamente");
            MostrarEstadisticas();
        }

        /// <summary>
        /// Busca todos los objetos TerritorioUI en la escena
        /// </summary>
        private void BuscarTerritoriosEnEscena()
        {
            // Buscar todos los componentes TerritorioUI en la escena
            territoriosUI = FindObjectsOfType<TerritorioUI>();

            Debug.Log($"Encontrados {territoriosUI.Length} territorios en la escena");

            if (territoriosUI.Length != 42)
            {
                Debug.LogWarning($"Se esperaban 42 territorios, pero se encontraron {territoriosUI.Length}");
            }
        }

        /// <summary>
        /// Conecta cada territorio lógico con su representación visual
        /// </summary>
        private void ConectarLogicaConInterfaz()
        {
            for (int i = 0; i < territoriosLogica.getSize(); i++)
            {
                Territorio territorioLogico = territoriosLogica[i];

                // Buscar el territorio visual correspondiente por nombre
                TerritorioUI territorioVisual = BuscarTerritorioUIPorNombre(territorioLogico.Nombre);

                if (territorioVisual != null)
                {
                    // Conectar la lógica con la interfaz
                    territorioVisual.InicializarTerritorio(territorioLogico);
                    Debug.Log($"Conectado: {territorioLogico.Nombre} - Propietario: {territorioLogico.PropietarioId} - Tropas: {territorioLogico.CantidadTropas}");
                }
                else
                {
                    Debug.LogError($"No se encontró TerritorioUI para: {territorioLogico.Nombre}");
                }
            }
        }

        /// <summary>
        /// Busca un TerritorioUI específico por nombre
        /// </summary>
        private TerritorioUI BuscarTerritorioUIPorNombre(string nombre)
        {
            foreach (TerritorioUI teritorioUI in territoriosUI)
            {
                if (teritorioUI.name == nombre || teritorioUI.GetNombreTerritorio() == nombre)
                {
                    return teritorioUI;
                }
            }
            return null;
        }

        /// <summary>
        /// Actualiza los colores de todos los territorios según su propietario
        /// </summary>
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

        /// <summary>
        /// Obtiene el color correspondiente a un jugador
        /// </summary>
        private Color ObtenerColorPorJugador(int jugadorId)
        {
            if (jugadorId == jugador1.getId())
                return colorJugador1Unity;
            else if (jugadorId == jugador2.getId())
                return colorJugador2Unity;
            else if (jugadorId == jugadorNeutral.getId())
                return colorNeutralUnity;

            return Color.white; // Color por defecto
        }

        /// <summary>
        /// Muestra estadísticas del juego en consola
        /// </summary>
        private void MostrarEstadisticas()
        {
            Debug.Log("=== ESTADÍSTICAS DEL JUEGO ===");
            Debug.Log($"{jugador1.getNombre()}: {jugador1.getCantidadTerritorios()} territorios, {jugador1.getTotalTropas()} tropas");
            Debug.Log($"{jugador2.getNombre()}: {jugador2.getCantidadTerritorios()} territorios, {jugador2.getTotalTropas()} tropas");
            Debug.Log($"Neutral: {jugadorNeutral.getCantidadTerritorios()} territorios, {jugadorNeutral.getTotalTropas()} tropas");
        }

        /// <summary>
        /// Método público para redistribuir territorios (para testing)
        /// </summary>
        [ContextMenu("Redistribuir Territorios")]
        public void RedistribuirTerritorios()
        {
            InicializarJuego();
        }

        // Getters para que otros scripts accedan a los datos
        public Lista<Territorio> GetTerritorios() => territoriosLogica;
        public Jugador GetJugador1() => jugador1;
        public Jugador GetJugador2() => jugador2;
        public Jugador GetJugadorNeutral() => jugadorNeutral;
    }
}