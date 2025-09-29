using UnityEngine;
using TMPro;
using CrazyRisk.Modelos;
using CrazyRisk.Estructuras;

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

        // Referencias
        private ManejadorRefuerzos manejadorRefuerzos;
        private ManejadorCombate manejadorCombate;

        public enum FaseTurno
        {
            Refuerzos,
            Ataque,
            Planeacion
        }

        void Start()
        {
            manejadorRefuerzos = new ManejadorRefuerzos();
            manejadorCombate = new ManejadorCombate();
        }

        public void InicializarTurnos(Lista<Jugador> listaJugadores, Lista<Continente> listaContinentes)
        {
            jugadores = listaJugadores;
            continentes = listaContinentes;
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
            faseActual = FaseTurno.Refuerzos;

            // Calcular refuerzos con protección contra null
            if (manejadorRefuerzos != null && continentes != null)
            {
                refuerzosDisponibles = manejadorRefuerzos.CalcularRefuerzos(
                    jugadorActual.getCantidadTerritorios(),
                    jugadorActual.getTerritoriosControlados(),
                    continentes
                );
            }
            else
            {
                // Cálculo básico sin bonificación de continentes
                int territorios = jugadorActual.getCantidadTerritorios();
                refuerzosDisponibles = territorios / 3;
                if (refuerzosDisponibles < 3)
                    refuerzosDisponibles = 3;

                if (continentes == null)
                    Debug.LogWarning("Continentes es null - usando cálculo básico de refuerzos");
            }

            ActualizarUI();
            MostrarPanelFase();
        }

        public void SiguienteFase()
        {
            switch (faseActual)
            {
                case FaseTurno.Refuerzos:
                    if (refuerzosDisponibles <= 0)
                    {
                        faseActual = FaseTurno.Ataque;
                    }
                    else
                    {
                        Debug.Log("Debes colocar todos los refuerzos antes de continuar");
                        return;
                    }
                    break;

                case FaseTurno.Ataque:
                    faseActual = FaseTurno.Planeacion;
                    break;

                case FaseTurno.Planeacion:
                    SiguienteJugador();
                    return;
            }

            ActualizarUI();
            MostrarPanelFase();
        }

        private void SiguienteJugador()
        {
            do
            {
                jugadorActualIndex = (jugadorActualIndex + 1) % jugadores.getSize();
            }
            while (jugadores[jugadorActualIndex].getEsNeutral() && jugadorActualIndex != 0);

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
            ActualizarUI();
        }

        public bool PuedeColocarRefuerzos() => faseActual == FaseTurno.Refuerzos && refuerzosDisponibles > 0;
        public bool PuedeAtacar() => faseActual == FaseTurno.Ataque;
        public bool PuedePlanear() => faseActual == FaseTurno.Planeacion;
    }
}