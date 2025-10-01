using UnityEngine;
using TMPro;
using CrazyRisk.Modelos;
using CrazyRisk.Estructuras;
using CrazyRisk.Managers;

namespace CrazyRisk.LogicaJuego
{
    public class ManejadorTurnos : MonoBehaviour
    {
        [Header("UI del Turno")]
        [SerializeField] private TextMeshProUGUI textoTurno;
        [SerializeField] private GameObject panelRefuerzos;
        [SerializeField] private GameObject panelAtaque;
        [SerializeField] private GameObject panelPlaneacion;

        // Estado del juego
        private Lista<Jugador> jugadores;
        private Lista<Continente> continentes;
        private int jugadorActualIndex = 0;
        private FaseTurno faseActual = FaseTurno.Refuerzos;
        private int refuerzosDisponibles = 0;
        private int jugadoresQueHanColocadoRefuerzos = 0;
        private bool todosColocaronRefuerzos = false;
        // Referencias
        private ManejadorRefuerzos manejadorRefuerzos;
        private ManejadorCombate manejadorCombate;
        private GameManager gameManager;

        public enum FaseTurno
        {
            Refuerzos,
            Ataque,
            Planeacion
        }

        void Start()
        {
            if (manejadorRefuerzos == null)
                manejadorRefuerzos = new ManejadorRefuerzos();

            if (manejadorCombate == null)
                manejadorCombate = new ManejadorCombate();
        }

        public void InicializarTurnos(Lista<Jugador> listaJugadores, Lista<Continente> listaContinentes)
        {
            jugadores = listaJugadores;
            continentes = listaContinentes;

            // Crear instancias aquí para asegurar que existan
            if (manejadorRefuerzos == null)
                manejadorRefuerzos = new ManejadorRefuerzos();

            if (manejadorCombate == null)
                manejadorCombate = new ManejadorCombate();

            // Asegurarse que existe el game manager
            if (gameManager == null)
                gameManager = FindObjectOfType<GameManager>();

            IniciarTurno();
        }

        public void IniciarTurno()
        {
            if (jugadores == null || jugadores.getSize() == 0)
            {
                Debug.LogError("No hay jugadores inicializados en ManejadorTurnos");
                return;
            }

            Jugador jugadorActual = GetJugadorActual();

            // Si todos ya colocaron refuerzos, ir a Ataque
            if (todosColocaronRefuerzos)
            {
                faseActual = FaseTurno.Ataque;
                todosColocaronRefuerzos = false;
                jugadoresQueHanColocadoRefuerzos = 0;
            }
            else
            {
                faseActual = FaseTurno.Refuerzos;
            }

            // Calcular refuerzos solo si es fase de refuerzos
            if (faseActual == FaseTurno.Refuerzos)
            {
                // Usar siempre ManejadorRefuerzos
                refuerzosDisponibles = manejadorRefuerzos.CalcularRefuerzos(
                    jugadorActual.getCantidadTerritorios(),
                    jugadorActual.getTerritoriosControlados(),
                    continentes
                );

                Debug.Log($"Refuerzos calculados para {jugadorActual.getNombre()}: {refuerzosDisponibles}");
            }

            ActualizarUI();
            MostrarPanelFase();

            if (jugadorActual.getEsNeutral() && faseActual == FaseTurno.Refuerzos)
            {
                Debug.Log(">>> Neutral colocando refuerzos automáticamente");
                ColocarRefuerzosNeutralAutomatico();
            }
            else if (jugadorActual.getEsNeutral() && faseActual == FaseTurno.Ataque)
            {
                Debug.Log(">>> Neutral salta ataque");
                SiguienteFase();
            }
            else if (jugadorActual.getEsNeutral() && faseActual == FaseTurno.Planeacion)
            {
                Debug.Log(">>> Neutral salta planeación");
                SiguienteFase();
            }
        

        }

        public void SiguienteFase()
        {
            Jugador jugadorActual = GetJugadorActual();

            switch (faseActual)
            {
                case FaseTurno.Refuerzos:

                    if (refuerzosDisponibles > 0)
                    {
                        Debug.Log("Debes colocar todos los refuerzos");
                        return;
                    }

                    // Contar este jugador
                    jugadoresQueHanColocadoRefuerzos++;

                    // Verificar si todos terminaron (2 jugadores, neutral no cuenta)
                    if (jugadoresQueHanColocadoRefuerzos >= 3)
                    {
                        todosColocaronRefuerzos = true;
                        Debug.Log("Todos colocaron refuerzos, siguiente turno será Ataque");
                    }

                    SiguienteJugador();
                    return;

                case FaseTurno.Ataque:
                    faseActual = FaseTurno.Planeacion;
                    ActualizarUI();
                    MostrarPanelFase();

                    // Si es neutral, saltar planeación también
                    if (jugadorActual.getEsNeutral())
                    {
                        SiguienteFase();
                    }
                    return;

                case FaseTurno.Planeacion:
                    SiguienteJugador();
                    return;
            }

            ActualizarUI();
            MostrarPanelFase();
        }

        private void SiguienteJugador()
        {
            jugadorActualIndex = (jugadorActualIndex + 1) % jugadores.getSize();
            IniciarTurno();
        }

        private void ActualizarUI()
        {
            Jugador actual = GetJugadorActual();
            string nombreFase = faseActual.ToString();

            if (textoTurno != null)
            {
                textoTurno.text = $"Turno de {actual.getNombre()} - Fase: {nombreFase}";

                if (faseActual == FaseTurno.Refuerzos)
                {
                    textoTurno.text += $"\nRefuerzos disponibles: {refuerzosDisponibles}";
                }
            }

            // Actualizar panel de datos del GameManager
            if (gameManager != null)
            {
                gameManager.ActualizarPanelDatos();
            }
        }

        private void MostrarPanelFase()
        {
            if (panelRefuerzos != null) panelRefuerzos.SetActive(faseActual == FaseTurno.Refuerzos);
            if (panelAtaque != null) panelAtaque.SetActive(faseActual == FaseTurno.Ataque);
            if (panelPlaneacion != null) panelPlaneacion.SetActive(faseActual == FaseTurno.Planeacion);
        }

        // Métodos públicos para usar desde otros scripts
        public Jugador GetJugadorActual() => jugadores[jugadorActualIndex];
        public FaseTurno GetFaseActual() => faseActual;
        public int GetRefuerzosDisponibles() => refuerzosDisponibles;

        public void UsarRefuerzo()
        {
            refuerzosDisponibles--;
            Debug.Log($"Refuerzos restantes: {refuerzosDisponibles}");

            ActualizarUI();

            if (refuerzosDisponibles <= 0)
            {
                Debug.Log(">>> Refuerzos agotados, llamando a SiguienteFase()");
                SiguienteFase();
            }
        }

        ///<summary>
        ///Metodo para lograr que el neutral coloque los refuerzos
        ///</summary>
        private async void ColocarRefuerzosNeutralAutomatico()
        {
            Jugador neutral = GetJugadorActual();

            Debug.Log($"Neutral colocando {refuerzosDisponibles} refuerzos automáticamente");

            while (refuerzosDisponibles > 0)
            {
                await System.Threading.Tasks.Task.Delay(1000);

                Lista<Territorio> territoriosNeutral = neutral.getTerritoriosControlados();
                Territorio territorioElegido = manejadorRefuerzos.ElegirTerritorioAleatorio(territoriosNeutral);

                if (territorioElegido == null)
                {
                    Debug.LogError("No se pudo elegir territorio para el neutral");
                    break;
                }

                territorioElegido.AgregarTropas(1);
                UsarRefuerzo();

                // Actualizar visualización usando el método existente
                if (gameManager != null)
                {
                    gameManager.ActualizarTerritorioEspecifico(territorioElegido.Nombre);
                }

                Debug.Log($"Neutral colocó 1 tropa en {territorioElegido.Nombre}");

            }
        }

        public bool PuedeColocarRefuerzos() => faseActual == FaseTurno.Refuerzos && refuerzosDisponibles > 0;
        public bool PuedeAtacar() => faseActual == FaseTurno.Ataque;
        public bool PuedePlanear() => faseActual == FaseTurno.Planeacion;
    }
}