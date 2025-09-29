using UnityEngine;
using CrazyRisk.LogicaJuego;
using CrazyRisk.Managers;

public class UIController : MonoBehaviour
{
    private ManejadorTurnos manejadorTurnos;
    private GameManager gameManager;

    void Start()
    {
        manejadorTurnos = FindObjectOfType<ManejadorTurnos>();
        gameManager = FindObjectOfType<GameManager>();
    }

    public void OnContinuar()
    {
        manejadorTurnos?.SiguienteFase();
    }

    public void OnAtacar()
    {
        // Ya manejado por el sistema de fases
        Debug.Log("Fase de ataque");
    }

    public void OnIntercambiar()
    {
        Debug.Log("Intercambiar tarjetas - por implementar");
    }

    public void OnAbandonar()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("MenuPrincipal");
    }
}
