using UnityEngine;
using CrazyRisk.LogicaJuego;
using CrazyRisk.Managers;

public class UIController : MonoBehaviour
{
    [SerializeField] private GameObject panelIntercambio;

    private ManejadorTurnos manejadorTurnos;
    private GameManager gameManager;
    private ManejadorTarjetas manejadorTarjetas;

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

    public void OnAtacar()
    {
        TerritorioUI.EjecutarAtaqueDesdeBoton();
    }

    public void OnIntercambiar()
    {
        if (manejadorTurnos == null) return;

        var jugador = manejadorTurnos.GetJugadorActual();
        if (jugador == null || jugador.getEsNeutral()) return;

        // Buscar trío válido automáticamente
        int[] trio = manejadorTarjetas.EncontrarTrioValido(jugador);

        if (trio == null)
        {
            Debug.LogWarning($"{jugador.getNombre()} no tiene un trío válido para intercambiar");
            return;
        }

        // Intercambiar automáticamente
        bool exito = manejadorTarjetas.IntentarIntercambio(jugador, trio[0], trio[1], trio[2]);

        if (exito)
        {
            // Agregar refuerzos al jugador actual
            int refuerzosActuales = manejadorTurnos.GetRefuerzosDisponibles();
            // Los refuerzos ya fueron calculados en ManejadorRefuerzos.IntercambiarTarjetas

            Debug.Log("Intercambio exitoso. Coloca tus refuerzos adicionales.");
        }
    }

    public void OnAbandonar()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("MenuPrincipal");
    }
}
