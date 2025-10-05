using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CrazyRisk.LogicaJuego;

namespace CrazyRisk.Managers
{
    public class VisualizadorDados : MonoBehaviour
    {
        [Header("Sprites de Dados (1-6)")]
        [SerializeField] private Sprite[] spritesDados;

        [Header("Referencias UI")]
        [SerializeField] private GameObject panelCombate;
        [SerializeField] private TextMeshProUGUI textoAtaque;
        [SerializeField] private TextMeshProUGUI textoResultado;

        [Header("Dados Atacante")]
        [SerializeField] private Image dado1Atacante;
        [SerializeField] private Image dado2Atacante;
        [SerializeField] private Image dado3Atacante;

        [Header("Dados Defensor")]
        [SerializeField] private Image dado1Defensor;
        [SerializeField] private Image dado2Defensor;

        [Header("Boton")]
        [SerializeField] private Button botonContinuar;

        private ManejadorCombate manejadorCombate;

        void Start()
        {
            manejadorCombate = new ManejadorCombate();

            if (panelCombate != null)
                panelCombate.SetActive(false);

            if (botonContinuar != null)
                botonContinuar.onClick.AddListener(CerrarPanel);
        }
        public void MostrarCombateConDados(string nombreAtacante, string nombreDefensor, int dadosAtacante, int dadosDefensor)
        {
            if (manejadorCombate == null)
                manejadorCombate = new ManejadorCombate();

            // Lanzar dados con las cantidades especificadas
            int[] resultadosAtacante = manejadorCombate.LanzarDadosAtacante(dadosAtacante);
            int[] resultadosDefensor = manejadorCombate.LanzarDadosDefensor(dadosDefensor);

            if (panelCombate != null)
            {
                panelCombate.SetActive(true);
                panelCombate.transform.SetAsLastSibling();
            }

            if (textoAtaque != null)
                textoAtaque.text = nombreAtacante + " ataca a " + nombreDefensor;

            // Los dados no usados se ocultarán automáticamente
            MostrarDadosAtacante(resultadosAtacante);
            MostrarDadosDefensor(resultadosDefensor);

            string resultado = manejadorCombate.ResolverCombateIndividual(resultadosAtacante, resultadosDefensor);
            if (textoResultado != null)
                textoResultado.text = ExtraerResultadoSimple(resultado);
        } 

        public void MostrarCombate(string nombreAtacante, string nombreDefensor, int tropasAtacante, int tropasDefensor)
        {
            if (manejadorCombate == null)
                manejadorCombate = new ManejadorCombate();

            int dadosAtacante = Mathf.Min(tropasAtacante - 1, 3);
            int dadosDefensor = Mathf.Min(tropasDefensor, 2);

            int[] resultadosAtacante = manejadorCombate.LanzarDadosAtacante(dadosAtacante);
            int[] resultadosDefensor = manejadorCombate.LanzarDadosDefensor(dadosDefensor);

            if (panelCombate != null)
            {
                panelCombate.SetActive(true);
                panelCombate.transform.SetAsLastSibling();
            }

            if (textoAtaque != null)
                textoAtaque.text = nombreAtacante + " ataca a " + nombreDefensor;

            MostrarDadosAtacante(resultadosAtacante);
            MostrarDadosDefensor(resultadosDefensor);

            string resultado = manejadorCombate.ResolverCombateIndividual(resultadosAtacante, resultadosDefensor);
            if (textoResultado != null)
                textoResultado.text = ExtraerResultadoSimple(resultado);
        }

        private void MostrarDadosAtacante(int[] dados)
        {
            if (dado1Atacante != null) dado1Atacante.gameObject.SetActive(false);
            if (dado2Atacante != null) dado2Atacante.gameObject.SetActive(false);
            if (dado3Atacante != null) dado3Atacante.gameObject.SetActive(false);

            for (int i = 0; i < dados.Length; i++)
            {
                Image dadoActual = ObtenerDadoAtacante(i);
                if (dadoActual != null && spritesDados != null && spritesDados.Length >= 6)
                {
                    dadoActual.gameObject.SetActive(true);
                    dadoActual.sprite = spritesDados[dados[i] - 1];
                }
            }
        }

        private void MostrarDadosDefensor(int[] dados)
        {
            if (dado1Defensor != null) dado1Defensor.gameObject.SetActive(false);
            if (dado2Defensor != null) dado2Defensor.gameObject.SetActive(false);

            for (int i = 0; i < dados.Length; i++)
            {
                Image dadoActual = ObtenerDadoDefensor(i);
                if (dadoActual != null && spritesDados != null && spritesDados.Length >= 6)
                {
                    dadoActual.gameObject.SetActive(true);
                    dadoActual.sprite = spritesDados[dados[i] - 1];
                }
            }
        }

        private Image ObtenerDadoAtacante(int indice)
        {
            switch (indice)
            {
                case 0: return dado1Atacante;
                case 1: return dado2Atacante;
                case 2: return dado3Atacante;
                default: return null;
            }
        }

        private Image ObtenerDadoDefensor(int indice)
        {
            switch (indice)
            {
                case 0: return dado1Defensor;
                case 1: return dado2Defensor;
                default: return null;
            }
        }

        private string ExtraerResultadoSimple(string resultadoCompleto)
        {
            string[] lineas = resultadoCompleto.Split('\n');
            string resultado = "";

            foreach (string linea in lineas)
            {
                if (linea.Contains("pierde") || linea.Contains("BAJAS"))
                {
                    resultado += linea.Trim() + "\n";
                }
            }

            if (string.IsNullOrEmpty(resultado))
                return resultadoCompleto;

            return resultado.Trim();
        }

        private void CerrarPanel()
        {
            if (panelCombate != null)
                panelCombate.SetActive(false);
        }
    }
}