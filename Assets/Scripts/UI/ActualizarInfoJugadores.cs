using UnityEngine;
using TMPro;
using CrazyRisk.Managers;
using CrazyRisk.Modelos;
using CrazyRisk.LogicaJuego;

public class ActualizadorInfoJugadores : MonoBehaviour
{
    [Header("Textos de Jugadores")]
    [SerializeField] private TextMeshProUGUI infoJ1;
    [SerializeField] private TextMeshProUGUI infoJ2;
    [SerializeField] private TextMeshProUGUI infoJ3;

    [Header("Contador Global")]
    [SerializeField] private TextMeshProUGUI textoContadorGlobal;

    private GameManager gameManager;
    private ManejadorRefuerzos manejadorRefuerzos;

    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        manejadorRefuerzos = new ManejadorRefuerzos();
        InvokeRepeating("ActualizarInfo", 0.5f, 0.5f);
    }

    void ActualizarInfo()
    {
        if (gameManager == null) return;

        var j1 = gameManager.GetJugador1();
        var j2 = gameManager.GetJugador2();
        var j3 = gameManager.GetJugadorNeutral();

        if (j1 != null && infoJ1 != null)
            infoJ1.text = GenerarTextoJugador(j1);

        if (j2 != null && infoJ2 != null)
            infoJ2.text = GenerarTextoJugador(j2);

        if (j3 != null && infoJ3 != null)
            infoJ3.text = GenerarTextoJugador(j3);

        if (textoContadorGlobal != null && manejadorRefuerzos != null)
        {
            int contador = manejadorRefuerzos.GetContadorGlobal();
            int proximoValor = manejadorRefuerzos.Fibonacci();
            textoContadorGlobal.text = $"Intercambios: {contador}\nProximo: {proximoValor} tropas";
        }
    }

    private string GenerarTextoJugador(Jugador jugador)
    {
        int infanteria = 0, artilleria = 0, caballeria = 0;
        var tarjetas = jugador.getTarjetas();

        for (int i = 0; i < tarjetas.getSize(); i++)
        {
            Tarjeta tarjeta = tarjetas.Obtener(i);
            if (!tarjeta.FueUsada())
            {
                switch (tarjeta.GetTipo())
                {
                    case TipoTarjeta.Infanteria: infanteria++; break;
                    case TipoTarjeta.Artilleria: artilleria++; break;
                    case TipoTarjeta.Caballeria: caballeria++; break;
                }
            }
        }

        return $"Territorios: {jugador.getCantidadTerritorios()}\n" +
               $"Tropas: {jugador.getTotalTropas()}\n" +
               $"Infanteria: {infanteria}\n" +
               $"Artilleria: {artilleria}\n" +
               $"Caballeria: {caballeria}";
    }
}