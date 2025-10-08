using UnityEngine;
using CrazyRisk.LogicaJuego;
using CrazyRisk.Managers;

public class UIController : MonoBehaviour
{
    [SerializeField] private GameObject panelIntercambio;

    private ManejadorTurnos manejadorTurnos;
    private GameManager gameManager;
    private ManejadorTarjetas manejadorTarjetas;

    /// <summary>
    /// Inicializa referencias a los manejadores y oculta el panel de intercambio al iniciar.
    /// </summary>
    void Start()
    {
        manejadorTurnos = FindObjectOfType<ManejadorTurnos>();
        gameManager = FindObjectOfType<GameManager>();
        manejadorTarjetas = FindObjectOfType<ManejadorTarjetas>();

        if (manejadorTarjetas == null)
        {
            GameObject obj = new GameObject("ManejadorTarjetas");
            manejadorTarjetas = obj.AddComponent<ManejadorTarjetas>();
        }

        if (panelIntercambio != null)
            panelIntercambio.SetActive(false);
    }

    /// <summary>
    /// Avanza a la siguiente fase del turno actual.
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
            Debug.LogError("No se encontr� ManejadorTurnos");
        }
    }

    /// <summary>
    /// Ejecuta el ataque desde el botón de la interfaz.
    /// </summary>
    public void OnAtacar()
    {
        TerritorioUI.EjecutarAtaqueDesdeBoton();
    }

    /// <summary>
    /// Realiza el intercambio automático de tarjetas si el jugador tiene un trío válido.
    /// </summary>
    public void OnIntercambiar()
    {
        if (manejadorTurnos == null) return;

        var jugador = manejadorTurnos.GetJugadorActual();
        if (jugador == null || jugador.getEsNeutral()) return;

        // Buscar tr�o v�lido autom�ticamente
        int[] trio = manejadorTarjetas.EncontrarTrioValido(jugador);

        if (trio == null)
        {
            Debug.LogWarning($"{jugador.getNombre()} no tiene un tr�o v�lido para intercambiar");
            return;
        }

        // Intercambiar autom�ticamente
        bool exito = manejadorTarjetas.IntentarIntercambio(jugador, trio[0], trio[1], trio[2]);

        if (exito)
        {
            // Agregar refuerzos al jugador actual
            int refuerzosActuales = manejadorTurnos.GetRefuerzosDisponibles();
            // Los refuerzos ya fueron calculados en ManejadorRefuerzos.IntercambiarTarjetas

            Debug.Log("Intercambio exitoso. Coloca tus refuerzos adicionales.");
        }
    }

    /// <summary>
    /// Abandona la partida y regresa al menú principal.
    /// </summary>
    public void OnAbandonar()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("MenuPrincipal");
    }
}
