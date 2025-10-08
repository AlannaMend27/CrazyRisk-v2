using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CrazyRisk.LogicaJuego;
using CrazyRisk.Modelos;

namespace CrazyRisk.Managers
{
    public class ControladorPlaneacion : MonoBehaviour
    {
        [Header("UI Planeaci�n")]
        [SerializeField] private GameObject panelMovimientoTropas;
        [SerializeField] private TextMeshProUGUI textoInfo;
        [SerializeField] private Slider sliderTropas;
        [SerializeField] private TextMeshProUGUI textoSlider;
        [SerializeField] private Button botonConfirmar;
        [SerializeField] private Button botonCancelar;

        private ManejadorPlaneacion manejadorPlaneacion;
        private ManejadorTurnos manejadorTurnos;
        private GameManager gameManager;
        private TerritorioUI territorioOrigenUI;
        private TerritorioUI territorioDestinoUI;

        /// <summary>
        /// Inicializa referencias, listeners y configura el panel de movimiento de tropas al iniciar el componente
        /// </summary>
        void Start()
        {
            gameManager = FindObjectOfType<GameManager>();

            manejadorPlaneacion = new ManejadorPlaneacion();

            if (gameManager != null)
            {
                manejadorPlaneacion.InicializarConTerritorios(gameManager.GetTerritorios());
            }

            if (panelMovimientoTropas != null)
                panelMovimientoTropas.SetActive(false);

            if (botonConfirmar != null)
                botonConfirmar.onClick.AddListener(ConfirmarMovimiento);

            if (botonCancelar != null)
                botonCancelar.onClick.AddListener(CancelarMovimiento);

            if (sliderTropas != null)
                sliderTropas.onValueChanged.AddListener(ActualizarTextoSlider);
        }

        /// <summary>
        /// Selecciona el territorio de origen para el movimiento de tropas si es válido
        /// </summary>
        public void SeleccionarOrigen(TerritorioUI territorioUI)
        {
            if (manejadorTurnos == null)
                manejadorTurnos = FindObjectOfType<ManejadorTurnos>();

            if (manejadorTurnos == null || !manejadorTurnos.PuedePlanear())
            {
                Debug.LogWarning("No est�s en fase de planeaci�n");
                return;
            }

            Jugador jugadorActual = manejadorTurnos.GetJugadorActual();
            Territorio territorio = territorioUI.GetTerritorioLogico();

            if (manejadorPlaneacion.SeleccionarOrigen(territorio, jugadorActual.getId()))
            {
                territorioOrigenUI = territorioUI;
                Debug.Log($"Origen seleccionado: {territorio.Nombre}");
            }
        }

        /// <summary>
        /// Selecciona el territorio de destino para el movimiento de tropas si es válido y muestra el panel de movimiento
        /// </summary>
        public void SeleccionarDestino(TerritorioUI territorioUI)
        {
            if (manejadorTurnos == null)
                manejadorTurnos = FindObjectOfType<ManejadorTurnos>();

            if (territorioOrigenUI == null)
            {
                Debug.LogWarning("Primero selecciona origen");
                return;
            }

            Jugador jugadorActual = manejadorTurnos.GetJugadorActual();
            Territorio territorio = territorioUI.GetTerritorioLogico();

            if (manejadorPlaneacion.SeleccionarDestino(territorio, jugadorActual.getId()))
            {
                territorioDestinoUI = territorioUI;
                MostrarPanelMovimiento();
            }
        }

        /// <summary>
        /// Muestra el panel para mover tropas y configura el slider y la información
        /// </summary>
        private void MostrarPanelMovimiento()
        {
            int tropas = manejadorPlaneacion.TropasDisponiblesParaMover();

            if (textoInfo != null)
                textoInfo.text = $"Mover de {manejadorPlaneacion.GetTerritorioOrigen().Nombre} a {manejadorPlaneacion.GetTerritorioDestino().Nombre}";

            if (sliderTropas != null)
            {
                sliderTropas.minValue = 1;
                sliderTropas.maxValue = tropas;
                sliderTropas.value = 1;
            }

            if (panelMovimientoTropas != null)
                panelMovimientoTropas.SetActive(true);

            ActualizarTextoSlider(1);
        }

        /// <summary>
        /// Actualiza el texto que muestra la cantidad de tropas seleccionadas en el slider
        /// </summary>
        private void ActualizarTextoSlider(float valor)
        {
            if (textoSlider != null)
                textoSlider.text = $"{(int)valor} tropa(s)";
        }

        /// <summary>
        /// Confirma el movimiento de tropas, actualiza las interfaces y limpia las selecciones
        /// </summary>
        private void ConfirmarMovimiento()
        {
            int cantidad = (int)sliderTropas.value;

            if (manejadorPlaneacion.MoverTropas(cantidad))
            {
                territorioOrigenUI.ActualizarInterfaz();
                territorioDestinoUI.ActualizarInterfaz();

                TerritorioUI.LimpiarSeleccionesEstaticas();
                LimpiarSeleccion();

                if (panelMovimientoTropas != null)
                    panelMovimientoTropas.SetActive(false);

                Debug.Log($"Movimiento completado: {cantidad} tropas");
            }
        }

        /// <summary>
        /// Cancela el movimiento de tropas y limpia las selecciones.
        /// </summary>
        private void CancelarMovimiento()
        {
            TerritorioUI.LimpiarSeleccionesEstaticas();
            LimpiarSeleccion();

            if (panelMovimientoTropas != null)
                panelMovimientoTropas.SetActive(false);
        }

        /// <summary>
        /// Limpia las referencias a los territorios seleccionados y la selección en el manejador de planeación.
        /// </summary>
        private void LimpiarSeleccion()
        {
            territorioOrigenUI = null;
            territorioDestinoUI = null;
            manejadorPlaneacion.LimpiarSeleccion();
        }

        /// <summary>
        /// Devuelve la instancia del manejador de planeación.
        /// </summary>
        public ManejadorPlaneacion GetManejador() => manejadorPlaneacion;
    }
}