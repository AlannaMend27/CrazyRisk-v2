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

        private Lista<Jugador> jugadores;
        private Lista<Continente> continentes;
        private int jugadorActualIndex = 0;
        private FaseTurno faseActual = FaseTurno.Refuerzos;
        private int refuerzosDisponibles = 0;

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

            if (manejadorRefuerzos == null)
                manejadorRefuerzos = new ManejadorRefuerzos();

            if (manejadorCombate == null)
                manejadorCombate = new ManejadorCombate();

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

            // SIEMPRE empezar en Refuerzos para ESTE jugador
            faseActual = FaseTurno.Refuerzos;

            // Calcular refuerzos
            refuerzosDisponibles = manejadorRefuerzos.CalcularRefuerzos(
                jugadorActual.getCantidadTerritorios(),
                jugadorActual.getTerritoriosControlados(),
                continentes
            );

            Debug.Log($"Refuerzos calculados para {jugadorActual.getNombre()}: {refuerzosDisponibles}");

            ActualizarUI();
            MostrarPanelFase();

            if (jugadorActual.getEsNeutral())
            {
                Debug.Log(">>> Neutral colocando refuerzos automaticamente");
                ColocarRefuerzosNeutralAutomatico();
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

                    // Pasar a ATAQUE del MISMO jugador
                    faseActual = FaseTurno.Ataque;
                    ActualizarUI();
                    MostrarPanelFase();

                    if (jugadorActual.getEsNeutral())
                    {
                        Debug.Log(">>> Neutral salta ataque");
                        SiguienteFase();
                    }
                    return;

                case FaseTurno.Ataque:
                    // Pasar a PLANEACION del MISMO jugador
                    faseActual = FaseTurno.Planeacion;
                    ActualizarUI();
                    MostrarPanelFase();

                    if (jugadorActual.getEsNeutral())
                    {
                        Debug.Log(">>> Neutral salta planeacion");
                        SiguienteFase();
                    }
                    return;

                case FaseTurno.Planeacion:
                    // AHORA SI pasar al SIGUIENTE jugador
                    SiguienteJugador();
                    return;
            }

            ActualizarUI();
            MostrarPanelFase();
        }

        private void SiguienteJugador()
        {
            jugadorActualIndex = (jugadorActualIndex + 1) % jugadores.getSize();
            ManagerSonidos.Instance?.ReproducirCambioTurno();
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

        private async void ColocarRefuerzosNeutralAutomatico()
        {
            Jugador neutral = GetJugadorActual();

            Debug.Log($"Neutral colocando {refuerzosDisponibles} refuerzos automaticamente");

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

                if (gameManager != null)
                {
                    gameManager.ActualizarTerritorioEspecifico(territorioElegido.Nombre);
                }

                Debug.Log($"Neutral coloco 1 tropa en {territorioElegido.Nombre}");
            }
        }

        public bool PuedeColocarRefuerzos() => faseActual == FaseTurno.Refuerzos && refuerzosDisponibles > 0;
        public bool PuedeAtacar() => faseActual == FaseTurno.Ataque;
        public bool PuedePlanear() => faseActual == FaseTurno.Planeacion;
    }
}