using UnityEngine;
using TMPro;
using CrazyRisk.Managers;
using CrazyRisk.Modelos;
using CrazyRisk.LogicaJuego;

public class ActualizadorInfoJugadores : MonoBehaviour
{
    [Header("Textos de Nombres")]
    [SerializeField] private TextMeshProUGUI j1_nombre;
    [SerializeField] private TextMeshProUGUI j2_nombre;
    [SerializeField] private TextMeshProUGUI j3_nombre;

    [Header("Textos de Informaci�n")]
    [SerializeField] private TextMeshProUGUI infoJ1;
    [SerializeField] private TextMeshProUGUI infoJ2;
    [SerializeField] private TextMeshProUGUI infoJ3;

    [Header("Contador Global")]
    [SerializeField] private TextMeshProUGUI textoContadorGlobal;

    private GameManager gameManager;
    private ManejadorRefuerzos manejadorRefuerzos;

    /// <summary>
    /// Inicializa referencias y comienza la actualización periódica de la información de los jugadores.
    /// </summary>
    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        manejadorRefuerzos = new ManejadorRefuerzos();
        InvokeRepeating("ActualizarInfo", 0.5f, 0.5f);
    }

    /// <summary>
    /// Actualiza la información mostrada de los jugadores y el contador global en la interfaz.
    /// </summary>
    void ActualizarInfo()
    {
        if (gameManager == null) return;

        var j1 = gameManager.GetJugador1();
        var j2 = gameManager.GetJugador2();
        var j3 = gameManager.GetJugador3();
        var neutral = gameManager.GetJugadorNeutral();

        // Actualizar Jugador 1
        if (j1 != null)
        {
            if (j1_nombre != null)
                j1_nombre.text = j1.getNombre();

            if (infoJ1 != null)
                infoJ1.text = GenerarTextoJugador(j1);
        }

        // Actualizar Jugador 2
        if (j2 != null)
        {
            if (j2_nombre != null)
                j2_nombre.text = j2.getNombre();

            if (infoJ2 != null)
                infoJ2.text = GenerarTextoJugador(j2);
        }

        // Actualizar Jugador 3 o Neutral
        if (j3 != null)
        {
            if (j3_nombre != null)
                j3_nombre.text = j3.getNombre();

            if (infoJ3 != null)
                infoJ3.text = GenerarTextoJugador(j3);
        }
        else if (neutral != null)
        {
            if (j3_nombre != null)
                j3_nombre.text = neutral.getNombre();

            if (infoJ3 != null)
                infoJ3.text = GenerarTextoJugador(neutral);
        }

        // Actualizar contador global
        if (textoContadorGlobal != null && manejadorRefuerzos != null)
        {
            int contador = manejadorRefuerzos.GetContadorGlobal();
            int proximoValor = manejadorRefuerzos.Fibonacci();
            textoContadorGlobal.text = $"Intercambios: {contador}\nProximo: {proximoValor} tropas";
        }
    }

    /// <summary>
    /// Genera el texto de información para un jugador, incluyendo territorios, tropas y tarjetas.
    /// </summary>
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

        // AHORA EL NOMBRE NO SE INCLUYE AQU�
        return $"Territorios: {jugador.getCantidadTerritorios()}\n" +
               $"Tropas: {jugador.getTotalTropas()}\n" +
               $"Infanteria: {infanteria}\n" +
               $"Artilleria: {artilleria}\n" +
               $"Caballeria: {caballeria}";
    }
}