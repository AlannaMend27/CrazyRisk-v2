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
        private ManejadorAtaques manejadorAtaques;
        private VisualizadorDados visualizadorDados;

        // Estado visual
        private Color colorOriginal;
        private Color colorHover;
        private bool estaSeleccionado = false;

        // Variables estáticas para mantener selección entre instancias
        private static TerritorioUI territorioAtacanteSeleccionado = null;
        private static TerritorioUI territorioDefensorSeleccionado = null;

        /// <summary>
        /// Inicializa el territorio visual, busca componentes y referencias necesarias.
        /// </summary>
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

        /// <summary>
        /// Busca y asigna los componentes visuales necesarios del territorio.
        /// </summary>
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

        /// <summary>
        /// Limpia las selecciones estáticas de atacante y defensor en todos los territorios.
        /// </summary>
        public static void LimpiarSeleccionesEstaticas()
        {
            if (territorioAtacanteSeleccionado != null)
            {
                territorioAtacanteSeleccionado.DeseleccionarTerritorio();
                territorioAtacanteSeleccionado = null;
            }

            if (territorioDefensorSeleccionado != null)
            {
                territorioDefensorSeleccionado.DeseleccionarTerritorio();
                territorioDefensorSeleccionado = null;
            }
        }
       
        /// <summary>
        /// Busca y asigna referencias a GameManager y ManejadorTurnos.
        /// </summary>
        private void BuscarReferencias()
        {
            gameManager = FindObjectOfType<GameManager>();
            manejadorTurnos = FindObjectOfType<ManejadorTurnos>();
        }

        /// <summary>
        /// Inicializa el territorio lógico asociado a este territorio visual y actualiza la interfaz.
        /// </summary>
        public void InicializarTerritorio(Territorio territorio)
        {
            territorioLogico = territorio;
            ActualizarInterfaz();
        }

        /// <summary>
        /// Actualiza la interfaz visual del territorio con la información lógica actual.
        /// </summary>
        public void ActualizarInterfaz()
        {
            if (territorioLogico == null) return;

            if (textoTropas != null)
                textoTropas.text = territorioLogico.CantidadTropas.ToString();

            if (textoNombre != null)
                textoNombre.text = territorioLogico.Nombre;
        }

        /// <summary>
        /// Cambia el color visual del territorio.
        /// </summary>
        public void CambiarColor(Color nuevoColor)
        {
            if (spriteRenderer != null)
            {
                colorOriginal = nuevoColor;
                spriteRenderer.color = nuevoColor;
                colorHover = Color.Lerp(nuevoColor, Color.white, 0.3f);
            }
        }

        /// <summary>
        /// Avanza a la siguiente fase del turno actual desde el territorio.
        /// </summary>
        public void OnContinuar()
        {
            if (manejadorTurnos == null)
                manejadorTurnos = FindObjectOfType<ManejadorTurnos>();

            if (manejadorTurnos != null)
            {
                manejadorTurnos.SiguienteFase();
            }
            else
            {
                Debug.LogError("No se encontró ManejadorTurnos");
            }
        }

        /// <summary>
        /// Cambia el color del territorio al pasar el mouse por encima, según el estado del juego.
        /// </summary>
        void OnMouseEnter()
        {
            if (spriteRenderer == null || estaSeleccionado) return;

            // Solo en fase de ataque
            if (manejadorTurnos != null && manejadorTurnos.GetFaseActual() == ManejadorTurnos.FaseTurno.Ataque)
            {
                // Si hay atacante seleccionado
                if (territorioAtacanteSeleccionado != null)
                {
                    Territorio atacante = territorioAtacanteSeleccionado.GetTerritorioLogico();
                    Jugador jugadorActual = manejadorTurnos.GetJugadorActual();

                    if (territorioLogico.PropietarioId != jugadorActual.getId() &&
                        atacante.EsAdyacenteA(territorioLogico.Id))
                    {
                        spriteRenderer.color = Color.red;
                        return;
                    }
                }
            }

            spriteRenderer.color = colorHover;
        }

        /// <summary>
        /// Restaura el color original del territorio al quitar el mouse.
        /// </summary>
        void OnMouseExit()
        {
            if (spriteRenderer != null && !estaSeleccionado)
                spriteRenderer.color = colorOriginal;
        }

        /// <summary>
        /// Maneja el clic sobre el territorio, gestionando la acción según la fase del juego.
        /// </summary>
        void OnMouseDown()
        {
            Debug.Log($"Click detectado en: {gameObject.name}");

            if (gameManager == null)
            {
                Debug.LogError("GameManager no asignado");
                return;
            }

            if (gameManager.EsJuegoEnRed() && !gameManager.EsMiTurno())
            {
                ManagerSonidos.Instance?.ReproducirError();
                Debug.LogWarning("No es tu turno. Espera tu oportunidad.");
                return;
            }

            if (gameManager.EstaEnFasePreparacion())
            {
                ManejarFasePreparacion();
                return;
            }

            if (manejadorTurnos == null)
            {
                Debug.Log("ManejadorTurnos era null, buscando...");
                manejadorTurnos = FindObjectOfType<ManejadorTurnos>();
            }

            if (manejadorTurnos == null)
            {
                Debug.LogError("ManejadorTurnos no encontrado en la escena");
                return;
            }

            if (territorioLogico == null)
            {
                Debug.LogError("TerritorioLogico es null");
                return;
            }

            Jugador jugadorActual = manejadorTurnos.GetJugadorActual();

            if (jugadorActual == null)
            {
                Debug.LogError("JugadorActual es null");
                return;
            }

            ManejadorTurnos.FaseTurno faseActual = manejadorTurnos.GetFaseActual();
            Debug.Log($"Fase actual: {faseActual}");

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

        /// <summary>
        /// Limpia referencias estáticas si este territorio es destruido.
        /// </summary>
        void OnDestroy()
        {
            // Limpiar referencias estáticas cuando se destruye
            if (territorioAtacanteSeleccionado == this)
                territorioAtacanteSeleccionado = null;

            if (territorioDefensorSeleccionado == this)
                territorioDefensorSeleccionado = null;
        }

        /// <summary>
        /// Maneja la colocación de tropas durante la fase de preparación.
        /// </summary>
        private void ManejarFasePreparacion()
        {
            if (territorioLogico == null)
            {
                Debug.LogError($"TerritorioLogico no asignado en {gameObject.name}");
                return;
            }

            Debug.Log($"Intentando colocar tropa en: {territorioLogico.Nombre}");

            bool exito = gameManager.IntentarColocarTropaPreparacion(territorioLogico.Nombre);

            if (exito)
            {
                ManagerSonidos.Instance?.ReproducirColocarTropas();
                ActualizarInterfaz();
                Debug.Log($"Tropa colocada exitosamente en {territorioLogico.Nombre}");
            }
            else
            {
                ManagerSonidos.Instance?.ReproducirError();
            }
        }


        /// <summary>
        /// Maneja la colocación de refuerzos en la fase correspondiente.
        /// </summary>
        private void ManejarRefuerzos(Jugador jugadorActual)
        {
            if (manejadorTurnos == null)
            {
                return;
            }

            if (!manejadorTurnos.PuedeColocarRefuerzos())
            {
                Debug.LogWarning("No es fase de refuerzos o no hay refuerzos disponibles");
                return;
            }

            if (jugadorActual == null)
            {
                Debug.LogError("JugadorActual es null");
                return;
            }

            bool esPropietario = territorioLogico.PropietarioId == jugadorActual.getId();

            if (esPropietario)
            {
                territorioLogico.AgregarTropas(1);
                manejadorTurnos.UsarRefuerzo();
                ManagerSonidos.Instance?.ReproducirColocarTropas();
                ActualizarInterfaz();
                Debug.Log($"{jugadorActual.getNombre()} colocó 1 refuerzo en {territorioLogico.Nombre}");
            }
            else
            {
                Debug.LogWarning($" {jugadorActual.getNombre()} intentó colocar refuerzo en territorio ajeno");
                ManagerSonidos.Instance?.ReproducirError();
            }
        }

        /// <summary>
        /// Gestiona la selección de atacante y defensor durante la fase de ataque.
        /// </summary>
        private void ManejarAtaque(Jugador jugadorActual)
        {
            // Buscar referencias
            if (manejadorAtaques == null)
                manejadorAtaques = gameManager.GetManejadorAtaques();

            if (visualizadorDados == null)
                visualizadorDados = FindObjectOfType<VisualizadorDados>();

            // Si es mi territorio con mas de 2 tropas y esta activado seleccionar como atacante
            if (territorioLogico.PropietarioId == jugadorActual.getId())
            {
                if (territorioLogico.PuedeAtacar())
                {
                    // Deseleccionar anteriores
                    if (territorioAtacanteSeleccionado != null)
                        territorioAtacanteSeleccionado.DeseleccionarTerritorio();

                    if (territorioDefensorSeleccionado != null)
                    {
                        territorioDefensorSeleccionado.DeseleccionarTerritorio();
                        territorioDefensorSeleccionado = null;
                    }

                    // Seleccionar este como atacante
                    if (manejadorAtaques.SeleccionarAtacante(territorioLogico, jugadorActual.getId()))
                    {
                        territorioAtacanteSeleccionado = this;
                        SeleccionarTerritorio(); // Amarillo
                        Debug.Log($"Atacante seleccionado: {territorioLogico.Nombre}");
                    }
                }
                else
                {
                    Debug.Log("Este territorio necesita al menos 2 tropas para atacar");
                }
            }
            // Si es enemigo y hay atacante seleccionado entonces seleccionar como defensor
            else if (territorioAtacanteSeleccionado != null)
            {
                if (manejadorAtaques.SeleccionarDefensor(territorioLogico, jugadorActual.getId()))
                {
                    // Deseleccionar defensor anterior si existía
                    if (territorioDefensorSeleccionado != null)
                        territorioDefensorSeleccionado.DeseleccionarTerritorio();

                    // Seleccionar este como defensor
                    territorioDefensorSeleccionado = this;
                    SeleccionarTerritorioRojo();
                    Debug.Log($"Defensor seleccionado: {territorioLogico.Nombre}. Presiona ATACAR para ejecutar.");
                }
            }
            else
            {
                Debug.Log("Primero selecciona un territorio propio para atacar");
            }
        }

        /// <summary>
        /// Selecciona visualmente el territorio como defensor (color rojo).
        /// </summary>
        private void SeleccionarTerritorioRojo()
        {
            estaSeleccionado = true;

            if (bordeSeleccion != null)
                bordeSeleccion.SetActive(true);

            if (spriteRenderer != null)
                spriteRenderer.color = Color.red;
        }

        /// <summary>
        /// Ejecuta el ataque entre los territorios seleccionados desde el botón de la interfaz.
        /// </summary>
        public static void EjecutarAtaqueDesdeBoton()
        {
            if (territorioAtacanteSeleccionado == null || territorioDefensorSeleccionado == null)
            {
                Debug.LogWarning("Debes seleccionar atacante y defensor");
                return;
            }

            GameManager gm = FindObjectOfType<GameManager>();

            // NUEVO: Validar en modo red
            if (gm.EsJuegoEnRed() && !gm.EsMiTurno())
            {
                Debug.LogWarning("No es tu turno para atacar");
                ManagerSonidos.Instance?.ReproducirError();
                return;
            }

            ControladorCombate controlador = FindObjectOfType<ControladorCombate>();

            if (controlador == null)
            {
                Debug.LogError("ControladorCombate no encontrado en la escena");
                return;
            }

            ManejadorAtaques manejador = gm?.GetManejadorAtaques();

            if (manejador == null)
            {
                Debug.LogError("ManejadorAtaques no encontrado");
                return;
            }

            controlador.IniciarPreguntaAtacar(
                territorioAtacanteSeleccionado,
                territorioDefensorSeleccionado,
                manejador
            );
        }

        /// <summary>
        /// Obtiene el color correspondiente al jugador según su ID.
        /// </summary>
        private Color ObtenerColorJugador(int jugadorId)
        {
            if (jugadorId == gameManager.GetJugador1().getId())
                return Color.green;
            else if (jugadorId == gameManager.GetJugador2().getId())
                return new Color(0.5f, 0f, 0.8f);
            return Color.gray;
        }

        /// <summary>
        /// Gestiona la selección de territorios para mover tropas en la fase de planeación.
        /// </summary>
        private void ManejarPlaneacion(Jugador jugadorActual)
        {
            Debug.Log($"=== PLANEACION: Click en {territorioLogico.Nombre} ===");
            Debug.Log($"Fase actual confirmada: {manejadorTurnos.GetFaseActual()}");

            if (territorioLogico.PropietarioId != jugadorActual.getId())
            {
                Debug.LogWarning("Solo puedes seleccionar tus propios territorios");
                return;
            }

            ControladorPlaneacion controlador = FindObjectOfType<ControladorPlaneacion>();
            if (controlador == null)
            {
                Debug.LogError("ControladorPlaneacion no encontrado en la escena");
                return;
            }

            ManejadorPlaneacion manejador = controlador.GetManejador();
            if (manejador == null)
            {
                Debug.LogError("ManejadorPlaneacion es null en ControladorPlaneacion");
                return;
            }

            Debug.Log($"Origen actual: {(manejador.GetTerritorioOrigen() != null ? manejador.GetTerritorioOrigen().Nombre : "NULL")}");

            // Si no hay origen seleccionado, este será el origen
            if (manejador.GetTerritorioOrigen() == null)
            {
                Debug.Log("Estableciendo como ORIGEN...");
                controlador.SeleccionarOrigen(this);
                SeleccionarTerritorio(); // Amarillo
            }
            // Si ya hay origen, este será el destino
            else
            {
                Debug.Log("Estableciendo como DESTINO...");
                controlador.SeleccionarDestino(this);
            }
        }

        /// <summary>
        /// Selecciona o deselecciona visualmente el territorio (color amarillo).
        /// </summary>
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

        /// <summary>
        /// Deselecciona visualmente el territorio y restaura su color original.
        /// </summary>
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