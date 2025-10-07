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
        [SerializeField] private GameObject botonSiguienteFase;

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
                Debug.Log(">>> Ejército neutral ejecutando turno automático");
                EjecutarTurnoNeutral();
            }
        }

        // MÉTODO MODIFICADO: Control explícito del jugador para pasar fases
        public void SiguienteFase()
        {
            Jugador jugadorActual = GetJugadorActual();

            // Solo permitir si no es neutral (el neutral se maneja automáticamente)
            if (jugadorActual.getEsNeutral())
            {
                Debug.Log("El ejército neutral se maneja automáticamente");
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
                    Debug.Log($"✅ Pasando a fase: Ataque - Jugador: {jugadorActual.getNombre()}");
                    break;

                case FaseTurno.Ataque:
                    // El jugador puede elegir pasar a planeación incluso sin atacar
                    faseActual = FaseTurno.Planeacion;
                    Debug.Log($"✅ Pasando a fase: Planeación - Jugador: {jugadorActual.getNombre()}");
                    break;

                case FaseTurno.Planeacion:
                    Debug.Log($"✅ Terminando turno de: {jugadorActual.getNombre()}");
                    SiguienteJugador();
                    return;
            }

            ActualizarUI();
            MostrarPanelFase();
        }

        // NUEVO MÉTODO: Para pasar directamente al siguiente jugador (después de conquista)
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

        // MÉTODO MODIFICADO: Solo reducir refuerzos, NO pasar fase automáticamente
        public void UsarRefuerzo()
        {
            if (refuerzosDisponibles > 0)
            {
                refuerzosDisponibles--;
                Debug.Log($"Refuerzo usado. Restantes: {refuerzosDisponibles}");
                ActualizarUI();
            }
        }

        // MÉTODO: Ejecutar turno completo del ejército neutral
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

        // MÉTODO: Solo colocar un refuerzo
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

        public Jugador GetJugadorActual() => jugadores[jugadorActualIndex];
        public FaseTurno GetFaseActual() => faseActual;
        public int GetRefuerzosDisponibles() => refuerzosDisponibles;

        public void AgregarRefuerzosDinamicos(int cantidad)
        {
            refuerzosDisponibles += cantidad;
            Debug.Log($"Se agregaron {cantidad} refuerzos dinámicos. Total: {refuerzosDisponibles}");
            ActualizarUI();
        }

        // NUEVO: Método para registrar que se realizó un ataque
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