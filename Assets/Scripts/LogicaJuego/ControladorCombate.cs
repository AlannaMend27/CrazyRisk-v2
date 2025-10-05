using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CrazyRisk.LogicaJuego;
using CrazyRisk.Modelos;

namespace CrazyRisk.Managers
{
    public class ControladorCombate : MonoBehaviour
    {
        [Header("Paneles")]
        [SerializeField] private GameObject panelPreguntaAtacar;
        [SerializeField] private GameObject panelPreguntaDefender;

        [Header("Panel Atacante")]
        [SerializeField] private TextMeshProUGUI textoInfoAtacante;
        [SerializeField] private TMP_Dropdown dropdownAtacante;
        [SerializeField] private Button botonContinuarAtacante;

        [Header("Panel Defensor")]
        [SerializeField] private TextMeshProUGUI textoInfoDefensor;
        [SerializeField] private TMP_Dropdown dropdownDefensor;
        [SerializeField] private Button botonContinuarDefensor;

        private VisualizadorDados visualizadorDados;
        private ManejadorAtaques manejadorAtaques;
        private ManejadorTurnos manejadorTurnos;
        private GameManager gameManager;
        private TerritorioUI territorioAtacante;
        private TerritorioUI territorioDefensor;

        private string nombreAtacante;
        private string nombreDefensor;
        private int tropasAtacante;
        private int tropasDefensor;

        void Start()
        {
            visualizadorDados = FindObjectOfType<VisualizadorDados>();
            gameManager = FindObjectOfType<GameManager>();
            manejadorTurnos = FindObjectOfType<ManejadorTurnos>();

            if (panelPreguntaAtacar != null) panelPreguntaAtacar.SetActive(false);
            if (panelPreguntaDefender != null) panelPreguntaDefender.SetActive(false);

            if (botonContinuarAtacante != null)
                botonContinuarAtacante.onClick.AddListener(ContinuarDesdeAtacante);

            if (botonContinuarDefensor != null)
                botonContinuarDefensor.onClick.AddListener(ContinuarDesdeDefensor);
        }

        public void IniciarPreguntaAtacar(TerritorioUI atacante, TerritorioUI defensor, ManejadorAtaques manejador)
        {
            territorioAtacante = atacante;
            territorioDefensor = defensor;
            manejadorAtaques = manejador;

            var territorioAtacanteLogico = atacante.GetTerritorioLogico();
            var territorioDefensorLogico = defensor.GetTerritorioLogico();

            nombreAtacante = territorioAtacanteLogico.Nombre;
            nombreDefensor = territorioDefensorLogico.Nombre;
            tropasAtacante = territorioAtacanteLogico.CantidadTropas;
            tropasDefensor = territorioDefensorLogico.CantidadTropas;

            int maxDadosAtacante = Mathf.Min(tropasAtacante - 1, 3);

            if (textoInfoAtacante != null)
                textoInfoAtacante.text = $"{nombreAtacante} ataca a {nombreDefensor}\nTropas disponibles: {tropasAtacante}";

            ConfigurarDropdown(dropdownAtacante, maxDadosAtacante);

            if (panelPreguntaAtacar != null)
                panelPreguntaAtacar.SetActive(true);
        }

        private void ConfigurarDropdown(TMP_Dropdown dropdown, int maxOpciones)
        {
            if (dropdown == null) return;

            dropdown.ClearOptions();

            for (int i = 1; i <= maxOpciones; i++)
            {
                dropdown.options.Add(new TMP_Dropdown.OptionData($"{i} dado{(i > 1 ? "s" : "")}"));
            }

            dropdown.value = 0;
            dropdown.RefreshShownValue();
        }

        private void ContinuarDesdeAtacante()
        {
            if (panelPreguntaAtacar != null)
                panelPreguntaAtacar.SetActive(false);

            MostrarPreguntaDefender();
        }

        private void MostrarPreguntaDefender()
        {
            int maxDadosDefensor = Mathf.Min(tropasDefensor, 2);

            if (textoInfoDefensor != null)
                textoInfoDefensor.text = $"{nombreDefensor} se defiende\nTropas disponibles: {tropasDefensor}";

            ConfigurarDropdown(dropdownDefensor, maxDadosDefensor);

            if (panelPreguntaDefender != null)
                panelPreguntaDefender.SetActive(true);
        }

        private void ContinuarDesdeDefensor()
        {
            if (panelPreguntaDefender != null)
                panelPreguntaDefender.SetActive(false);

            EjecutarCombateConSeleccion();
        }

        private void EjecutarCombateConSeleccion()
        {
            int dadosAtacante = dropdownAtacante.value + 1;
            int dadosDefensor = dropdownDefensor.value + 1;

            if (manejadorAtaques == null)
            {
                Debug.LogError("ManejadorAtaques es null");
                return;
            }

            ResultadoAtaque resultado = manejadorAtaques.EjecutarAtaqueConDados(dadosAtacante, dadosDefensor);

            if (resultado != null && visualizadorDados != null)
            {
                visualizadorDados.MostrarCombateConDados(
                    nombreAtacante,
                    nombreDefensor,
                    dadosAtacante,
                    dadosDefensor
                );

                territorioAtacante.ActualizarInterfaz();
                territorioDefensor.ActualizarInterfaz();

                if (resultado.conquistado)
                {
                    if (manejadorTurnos == null)
                    {
                        manejadorTurnos = FindObjectOfType<ManejadorTurnos>();
                    }

                    if (manejadorTurnos != null)
                    {
                        Jugador jugadorActual = manejadorTurnos.GetJugadorActual();

                        Color colorJugador = jugadorActual.getId() == gameManager.GetJugador1().getId()
                            ? Color.green
                            : new Color(0.5f, 0f, 0.8f);

                        territorioDefensor.CambiarColor(colorJugador);
                    }
                }

                TerritorioUI.LimpiarSeleccionesEstaticas();
                manejadorAtaques.LimpiarSeleccion();
            }
        }
    }
}