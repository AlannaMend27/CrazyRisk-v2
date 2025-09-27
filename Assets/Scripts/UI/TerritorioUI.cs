using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CrazyRisk.Modelos;

namespace CrazyRisk.Managers
{
    public class TerritorioUI : MonoBehaviour
    {
        [Header("Componentes Visuales")]
        [SerializeField] private SpriteRenderer spriteRenderer; // Para cambiar color del territorio
        [SerializeField] private TextMeshProUGUI textoTropas; // Para mostrar cantidad de tropas
        [SerializeField] private TextMeshProUGUI textoNombre; // Para mostrar nombre del territorio

        [Header("Configuraci�n")]
        [SerializeField] private string nombreTerritorio; // Nombre que debe coincidir con la l�gica

        // Referencia al territorio l�gico
        private Territorio territorioLogico;

        void Start()
        {
            // Si no se asign� manualmente, usar el nombre del GameObject
            if (string.IsNullOrEmpty(nombreTerritorio))
            {
                nombreTerritorio = gameObject.name;
            }

            // Buscar componentes autom�ticamente si no est�n asignados
            BuscarComponentes();
        }

        /// <summary>
        /// Busca autom�ticamente los componentes necesarios
        /// </summary>
        private void BuscarComponentes()
        {
            // Buscar SpriteRenderer (para cambiar color)
            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
                if (spriteRenderer == null)
                {
                    spriteRenderer = GetComponentInChildren<SpriteRenderer>();
                }
            }

            // Buscar textos (pueden estar en hijos)
            if (textoTropas == null)
            {
                TextMeshProUGUI[] textos = GetComponentsInChildren<TextMeshProUGUI>();
                foreach (var texto in textos)
                {
                    if (texto.name.ToLower().Contains("tropas") || texto.name.ToLower().Contains("troops"))
                    {
                        textoTropas = texto;
                        break;
                    }
                }
            }

            if (textoNombre == null)
            {
                TextMeshProUGUI[] textos = GetComponentsInChildren<TextMeshProUGUI>();
                foreach (var texto in textos)
                {
                    if (texto.name.ToLower().Contains("nombre") || texto.name.ToLower().Contains("name"))
                    {
                        textoNombre = texto;
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Inicializa el territorio con su l�gica correspondiente
        /// </summary>
        public void InicializarTerritorio(Territorio territorio)
        {
            territorioLogico = territorio;
            ActualizarInterfaz();
        }

        /// <summary>
        /// Actualiza la interfaz visual con los datos del territorio l�gico
        /// </summary>
        public void ActualizarInterfaz()
        {
            if (territorioLogico == null) return;

            // Actualizar texto de tropas
            if (textoTropas != null)
            {
                textoTropas.text = territorioLogico.CantidadTropas.ToString();
            }

            // Actualizar nombre del territorio
            if (textoNombre != null)
            {
                textoNombre.text = territorioLogico.Nombre;
            }
        }

        /// <summary>
        /// Cambia el color del territorio
        /// </summary>
        public void CambiarColor(Color nuevoColor)
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.color = nuevoColor;
            }
            else
            {
                Debug.LogWarning($"No se encontr� SpriteRenderer en {gameObject.name}");
            }
        }

        /// <summary>
        /// Maneja el clic en el territorio
        /// </summary>
        void OnMouseDown()
        {
            if (territorioLogico != null)
            {
                Debug.Log($"Territorio clickeado: {territorioLogico.Nombre} - Propietario: {territorioLogico.PropietarioId} - Tropas: {territorioLogico.CantidadTropas}");

                // Aqu� puedes agregar l�gica de selecci�n
                // Por ejemplo, notificar al GameManager que este territorio fue seleccionado
            }
        }

        // Getters p�blicos
        public string GetNombreTerritorio() => nombreTerritorio;
        public Territorio GetTerritorioLogico() => territorioLogico;

        /// <summary>
        /// M�todo para configurar manualmente el nombre (�til para testing)
        /// </summary>
        public void SetNombreTerritorio(string nombre)
        {
            nombreTerritorio = nombre;
        }

        /// <summary>
        /// Actualiza solo el n�mero de tropas
        /// </summary>
        public void ActualizarTropas(int nuevasCantidad)
        {
            if (territorioLogico != null)
            {
                territorioLogico.CantidadTropas = nuevasCantidad;
                if (textoTropas != null)
                {
                    textoTropas.text = nuevasCantidad.ToString();
                }
            }
        }
    }
}
