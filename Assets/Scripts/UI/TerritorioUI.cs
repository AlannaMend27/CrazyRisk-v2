using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CrazyRisk.Modelos;
using CrazyRisk.LogicaJuego;

namespace CrazyRisk.Managers
{
    public class TerritorioUI : MonoBehaviour
    {
        [Header("Componentes Visuales")]
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private TextMeshProUGUI textoTropas;
        [SerializeField] private TextMeshProUGUI textoNombre;
        [SerializeField] private GameObject bordeSeleccion;

        [Header("Configuración")]
        [SerializeField] private string nombreTerritorio;

        // Referencias
        private Territorio territorioLogico;
        private ManejadorTurnos manejadorTurnos;
        private GameManager gameManager;

        // Estado visual
        private Color colorOriginal;
        private Color colorHover;
        private bool estaSeleccionado = false;

        void Start()
        {
            if (string.IsNullOrEmpty(nombreTerritorio))
                nombreTerritorio = gameObject.name;

            BuscarComponentes();
            BuscarReferencias();

            if (spriteRenderer != null)
            {
                colorOriginal = spriteRenderer.color;
                colorHover = Color.Lerp(colorOriginal, Color.white, 0.3f);
            }

            if (bordeSeleccion != null)
                bordeSeleccion.SetActive(false);
        }

        private void BuscarComponentes()
        {
            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
                if (spriteRenderer == null)
                    spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            }

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

        private void BuscarReferencias()
        {
            gameManager = FindObjectOfType<GameManager>();
            manejadorTurnos = FindObjectOfType<ManejadorTurnos>();
        }

        public void InicializarTerritorio(Territorio territorio)
        {
            territorioLogico = territorio;
            ActualizarInterfaz();
        }

        public void ActualizarInterfaz()
        {
            if (territorioLogico == null) return;

            if (textoTropas != null)
                textoTropas.text = territorioLogico.CantidadTropas.ToString();

            if (textoNombre != null)
                textoNombre.text = territorioLogico.Nombre;
        }

        public void CambiarColor(Color nuevoColor)
        {
            if (spriteRenderer != null)
            {
                colorOriginal = nuevoColor;
                spriteRenderer.color = nuevoColor;
                colorHover = Color.Lerp(nuevoColor, Color.white, 0.3f);
            }
        }

        void OnMouseEnter()
        {
            if (spriteRenderer != null && !estaSeleccionado)
                spriteRenderer.color = colorHover;
        }

        void OnMouseExit()
        {
            if (spriteRenderer != null && !estaSeleccionado)
                spriteRenderer.color = colorOriginal;
        }

        void OnMouseDown()
        {
            if (territorioLogico == null || manejadorTurnos == null) return;

            Jugador jugadorActual = manejadorTurnos.GetJugadorActual();
            ManejadorTurnos.FaseTurno faseActual = manejadorTurnos.GetFaseActual();

            switch (faseActual)
            {
                case ManejadorTurnos.FaseTurno.Refuerzos:
                    ManejarRefuerzos(jugadorActual);
                    break;

                case ManejadorTurnos.FaseTurno.Ataque:
                    ManejarAtaque(jugadorActual);
                    break;

                case ManejadorTurnos.FaseTurno.Planeacion:
                    ManejarPlaneacion(jugadorActual);
                    break;
            }
        }

        private void ManejarRefuerzos(Jugador jugadorActual)
        {
            if (territorioLogico.PropietarioId == jugadorActual.getId() &&
                manejadorTurnos.PuedeColocarRefuerzos())
            {
                territorioLogico.AgregarTropas(1);
                manejadorTurnos.UsarRefuerzo();
                ActualizarInterfaz();

                Debug.Log($"{jugadorActual.getNombre()} colocó 1 refuerzo en {territorioLogico.Nombre}");
            }
            else
            {
                Debug.Log("Solo puedes colocar refuerzos en tus propios territorios");
            }
        }

        private void ManejarAtaque(Jugador jugadorActual)
        {
            if (territorioLogico.PropietarioId == jugadorActual.getId())
            {
                if (territorioLogico.PuedeAtacar())
                {
                    SeleccionarTerritorio();
                    Debug.Log($"Territorio atacante seleccionado: {territorioLogico.Nombre}");
                }
                else
                {
                    Debug.Log("Este territorio necesita al menos 2 tropas para atacar");
                }
            }
            else
            {
                Debug.Log($"Territorio enemigo: {territorioLogico.Nombre} - Objetivo de ataque");
            }
        }

        private void ManejarPlaneacion(Jugador jugadorActual)
        {
            if (territorioLogico.PropietarioId == jugadorActual.getId())
            {
                SeleccionarTerritorio();
                Debug.Log($"Territorio seleccionado para planeación: {territorioLogico.Nombre}");
            }
        }

        private void SeleccionarTerritorio()
        {
            estaSeleccionado = !estaSeleccionado;

            if (bordeSeleccion != null)
                bordeSeleccion.SetActive(estaSeleccionado);

            if (spriteRenderer != null)
            {
                if (estaSeleccionado)
                    spriteRenderer.color = Color.yellow;
                else
                    spriteRenderer.color = colorOriginal;
            }
        }

        public void DeseleccionarTerritorio()
        {
            estaSeleccionado = false;
            if (bordeSeleccion != null)
                bordeSeleccion.SetActive(false);
            if (spriteRenderer != null)
                spriteRenderer.color = colorOriginal;
        }

        // Getters
        public string GetNombreTerritorio() => nombreTerritorio;
        public Territorio GetTerritorioLogico() => territorioLogico;
        public bool EstaSeleccionado() => estaSeleccionado;

        public void SetNombreTerritorio(string nombre)
        {
            nombreTerritorio = nombre;
        }

        public void ActualizarTropas(int nuevasCantidad)
        {
            if (territorioLogico != null)
            {
                territorioLogico.CantidadTropas = nuevasCantidad;
                if (textoTropas != null)
                    textoTropas.text = nuevasCantidad.ToString();
            }
        }
    }
}