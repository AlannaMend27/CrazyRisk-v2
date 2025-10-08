using UnityEngine;
using TMPro;
using CrazyRisk.Modelos;
using CrazyRisk.Estructuras;
using CrazyRisk.Managers;

namespace CrazyRisk.LogicaJuego
{
    /// <summary>
    /// Gestiona el flujo de turnos, fases y acciones de los jugadores durante la partida.
    /// </summary>
    public class ManejadorTurnos : MonoBehaviour
    {
        [Header("UI del Turno")]
        [SerializeField] private TextMeshProUGUI textoTurno;
        [SerializeField] private GameObject panelRefuerzos;
        [SerializeField] private GameObject panelAtaque;
        [SerializeField] private GameObject panelPlaneacion;
        [SerializeField] private GameObject botonSiguienteFase;

        //Propiedades
        private Lista<Jugador> jugadores;
        private Lista<Continente> continentes;
        private int jugadorActualIndex = 0;
        private FaseTurno faseActual = FaseTurno.Refuerzos;
        private int refuerzosDisponibles = 0;
        private bool ataqueRealizadoEnEsteTurno = false;

        private ManejadorRefuerzos manejadorRefuerzos;
        private ManejadorCombate manejadorCombate;
        private GameManager gameManager;

        public enum FaseTurno
        {
            Refuerzos,
            Ataque,
            Planeacion
        }

        /// <summary>
        /// Inicializa los manejadores de refuerzos y combate al iniciar el componente.
        /// </summary>
        void Start()
        {
            if (manejadorRefuerzos == null)
                manejadorRefuerzos = new ManejadorRefuerzos();

            if (manejadorCombate == null)
                manejadorCombate = new ManejadorCombate();
        }

        /// <summary>
        /// Inicializa la lista de jugadores y continentes, y comienza el primer turno.
        /// </summary>
        public void InicializarTurnos(Lista<Jugador> listaJugadores, Lista<Continente> listaContinentes)
        {
            jugadores = listaJugadores;
            continentes = listaContinentes;

            if (manejadorRefuerzos == null)
                manejadorRefuerzos = new ManejadorRefuerzos();

            if (manejadorCombate == null)
                manejadorCombate = new ManejadorCombate();

            if (gameManager == null)
                gameManager = FindObjectOfType<GameManager>();

            IniciarTurno();
        }

        /// <summary>
        /// Inicia el turno del jugador actual, calcula refuerzos y muestra la interfaz correspondiente.
        /// </summary>
        public void IniciarTurno()
        {
            if (jugadores == null || jugadores.getSize() == 0)
            {
                return;
            }

            Jugador jugadorActual = GetJugadorActual();

            // Resetear estado del turno
            faseActual = FaseTurno.Refuerzos;
            ataqueRealizadoEnEsteTurno = false;

            // Calcular refuerzos
            refuerzosDisponibles = manejadorRefuerzos.CalcularRefuerzos(
                jugadorActual.getCantidadTerritorios(),
                jugadorActual.getTerritoriosControlados(),
                continentes
            );

            Debug.Log($"=== INICIANDO TURNO DE {jugadorActual.getNombre()} ===");
            Debug.Log($"Fase: {faseActual}, Refuerzos: {refuerzosDisponibles}");

            ActualizarUI();
            MostrarPanelFase();

            if (jugadorActual.getEsNeutral())
            {
                EjecutarTurnoNeutral();
            }
        }

        /// <summary>
        /// Permite al jugador pasar a la siguiente fase del turno si cumple las condiciones.
        /// </summary>
        public void SiguienteFase()
        {
            Jugador jugadorActual = GetJugadorActual();

            // Solo permitir si no es neutral (el neutral se maneja automáticamente)
            if (jugadorActual.getEsNeutral())
            {
                return;
            }

            switch (faseActual)
            {
                case FaseTurno.Refuerzos:
                    if (refuerzosDisponibles > 0)
                    {
                        Debug.LogWarning($"No puedes pasar a ataque. Todavía tienes {refuerzosDisponibles} refuerzos por colocar.");
                        return;
                    }
                    faseActual = FaseTurno.Ataque;
                    Debug.Log($"Pasando a fase: Ataque - Jugador: {jugadorActual.getNombre()}");
                    break;

                case FaseTurno.Ataque:
                    // El jugador puede elegir pasar a planeación incluso sin atacar
                    faseActual = FaseTurno.Planeacion;
                    Debug.Log($"Pasando a fase: Planeación - Jugador: {jugadorActual.getNombre()}");
                    break;

                case FaseTurno.Planeacion:
                    Debug.Log($"Terminando turno de: {jugadorActual.getNombre()}");
                    SiguienteJugador();
                    return;
            }

            ActualizarUI();
            MostrarPanelFase();
        }

        /// <summary>
        /// Finaliza el turno después de una conquista y pasa automáticamente a la fase de planeación.
        /// </summary>
        public void FinalizarTurnoDespuesDeConquista()
        {
            // Si está en fase de ataque y conquistó, pasar a planeación automáticamente
            if (faseActual == FaseTurno.Ataque)
            {
                faseActual = FaseTurno.Planeacion;
                Debug.Log($"Conquista exitosa. Pasando automáticamente a Planeación.");
                ActualizarUI();
                MostrarPanelFase();
            }
        }

        /// <summary>
        /// Avanza al siguiente jugador y reinicia el turno.
        /// </summary>
        private void SiguienteJugador()
        {
            jugadorActualIndex = (jugadorActualIndex + 1) % jugadores.getSize();
            ManagerSonidos.Instance?.ReproducirCambioTurno();
            IniciarTurno();
        }

        /// <summary>
        /// Actualiza la interfaz de usuario con la información del turno y fase actual.
        /// </summary>
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

            // Actualizar estado del botón siguiente fase
            if (botonSiguienteFase != null)
            {
                bool puedePasarFase = !actual.getEsNeutral() &&
                                    (faseActual != FaseTurno.Refuerzos || refuerzosDisponibles == 0);
                botonSiguienteFase.SetActive(puedePasarFase);
            }

            if (gameManager != null)
            {
                gameManager.ActualizarPanelDatos();
            }
        }

        /// <summary>
        /// Muestra el panel correspondiente a la fase actual del turno.
        /// </summary>
        private void MostrarPanelFase()
        {
            if (panelRefuerzos != null)
            {
                panelRefuerzos.SetActive(faseActual == FaseTurno.Refuerzos);
                Debug.Log($"Panel Refuerzos activo: {faseActual == FaseTurno.Refuerzos}");
            }
            if (panelAtaque != null)
            {
                panelAtaque.SetActive(faseActual == FaseTurno.Ataque);
                Debug.Log($"Panel Ataque activo: {faseActual == FaseTurno.Ataque}");
            }
            if (panelPlaneacion != null)
            {
                panelPlaneacion.SetActive(faseActual == FaseTurno.Planeacion);
                Debug.Log($"Panel Planeacion activo: {faseActual == FaseTurno.Planeacion}");
            }
        }

        /// <summary>
        /// Reduce la cantidad de refuerzos disponibles en el turno.
        /// </summary>
        public void UsarRefuerzo()
        {
            if (refuerzosDisponibles > 0)
            {
                refuerzosDisponibles--;
                ActualizarUI();
            }
        }

        /// <summary>
        /// Ejecuta automáticamente el turno del ejército neutral.
        /// </summary>
        private async void EjecutarTurnoNeutral()
        {
            Jugador neutral = GetJugadorActual();

            // FASE REFUERZOS
            Debug.Log("Neutral - Fase Refuerzos");
            while (refuerzosDisponibles > 0)
            {
                await System.Threading.Tasks.Task.Delay(500);
                ColocarRefuerzoNeutralAleatorio();
            }

            // FASE ATAQUE (neutral no ataca según reglas)
            Debug.Log("Neutral - Saltando Fase Ataque");
            await System.Threading.Tasks.Task.Delay(500);

            // FASE PLANEACIÓN (neutral no planea)
            Debug.Log("Neutral - Saltando Fase Planeación");
            await System.Threading.Tasks.Task.Delay(500);

            // Pasar al siguiente jugador
            Debug.Log("Neutral - Terminando turno");
            SiguienteJugador();
        }

        /// <summary>
        /// Permite que el ejército neutral coloque refuerzos de forma automática en sus territorios.
        /// </summary>
        private void ColocarRefuerzoNeutralAleatorio()
        {
            if (refuerzosDisponibles <= 0) return;

            Jugador neutral = GetJugadorActual();
            Lista<Territorio> territoriosNeutral = neutral.getTerritoriosControlados();
            Territorio territorioElegido = manejadorRefuerzos.ElegirTerritorioAleatorio(territoriosNeutral);

            if (territorioElegido != null)
            {
                territorioElegido.AgregarTropas(1);
                refuerzosDisponibles--;

                if (gameManager != null)
                {
                    gameManager.ActualizarTerritorioEspecifico(territorioElegido.Nombre);
                }

                Debug.Log($"Neutral colocó 1 tropa en {territorioElegido.Nombre}. Refuerzos restantes: {refuerzosDisponibles}");
                ActualizarUI();
            }
        }

        /// <summary>
        /// Devuelve el jugador que tiene el turno actual.
        /// </summary>
        public Jugador GetJugadorActual() => jugadores[jugadorActualIndex];

        /// <summary>
        /// Devuelve la fase actual del turno.
        /// </summary>
        public FaseTurno GetFaseActual() => faseActual;

        /// <summary>
        /// Devuelve la cantidad de refuerzos disponibles en el turno actual.
        /// </summary>
        public int GetRefuerzosDisponibles() => refuerzosDisponibles;

        /// <summary>
        /// Agrega refuerzos adicionales al jugador actual.
        /// </summary>
        public void AgregarRefuerzosDinamicos(int cantidad)
        {
            refuerzosDisponibles += cantidad;
            Debug.Log($"Se agregaron {cantidad} refuerzos dinámicos. Total: {refuerzosDisponibles}");
            ActualizarUI();
        }

        // METODOS AUXILIARES

        public void RegistrarAtaqueRealizado()
        {
            ataqueRealizadoEnEsteTurno = true;
        }
        public bool PuedeColocarRefuerzos() => faseActual == FaseTurno.Refuerzos && refuerzosDisponibles > 0;

        public bool PuedeAtacar() => faseActual == FaseTurno.Ataque;

        public bool PuedePlanear() => faseActual == FaseTurno.Planeacion;

        public bool EsTurnoNeutral() => GetJugadorActual().getEsNeutral();

        public bool SeRealizoAtaqueEnEsteTurno() => ataqueRealizadoEnEsteTurno;
    }
}