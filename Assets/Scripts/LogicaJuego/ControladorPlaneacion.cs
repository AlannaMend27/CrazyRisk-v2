using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CrazyRisk.LogicaJuego;
using CrazyRisk.Modelos;

namespace CrazyRisk.Managers
{
    public class ControladorPlaneacion : MonoBehaviour
    {
        [Header("UI Planeación")]
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

        public void SeleccionarOrigen(TerritorioUI territorioUI)
        {
            if (manejadorTurnos == null)
                manejadorTurnos = FindObjectOfType<ManejadorTurnos>();

            if (manejadorTurnos == null || !manejadorTurnos.PuedePlanear())
            {
                Debug.LogWarning("No estás en fase de planeación");
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

        private void ActualizarTextoSlider(float valor)
        {
            if (textoSlider != null)
                textoSlider.text = $"{(int)valor} tropa(s)";
        }

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

        private void CancelarMovimiento()
        {
            TerritorioUI.LimpiarSeleccionesEstaticas();
            LimpiarSeleccion();

            if (panelMovimientoTropas != null)
                panelMovimientoTropas.SetActive(false);
        }

        private void LimpiarSeleccion()
        {
            territorioOrigenUI = null;
            territorioDestinoUI = null;
            manejadorPlaneacion.LimpiarSeleccion();
        }

        public ManejadorPlaneacion GetManejador() => manejadorPlaneacion;
    }
}