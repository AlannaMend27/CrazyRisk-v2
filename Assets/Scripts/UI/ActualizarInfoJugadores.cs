using UnityEngine;
using TMPro;
using CrazyRisk.Managers;
using CrazyRisk.Modelos;

public class ActualizadorInfoJugadores : MonoBehaviour
{
    [Header("Textos Jugador 1")]
    [SerializeField] private TextMeshProUGUI territoriosJ1;
    [SerializeField] private TextMeshProUGUI tropasJ1;
    [SerializeField] private TextMeshProUGUI infanteriaJ1;
    [SerializeField] private TextMeshProUGUI artilleriaJ1;
    [SerializeField] private TextMeshProUGUI caballeriaJ1;

    [Header("Textos Jugador 2")]
    [SerializeField] private TextMeshProUGUI territoriosJ2;
    [SerializeField] private TextMeshProUGUI tropasJ2;
    [SerializeField] private TextMeshProUGUI infanteriaJ2;
    [SerializeField] private TextMeshProUGUI artilleriaJ2;
    [SerializeField] private TextMeshProUGUI caballeriaJ2;

    private GameManager gameManager;

    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        InvokeRepeating("ActualizarInfo", 0.5f, 0.5f);
    }

    void ActualizarInfo()
    {
        if (gameManager == null) return;

        var j1 = gameManager.GetJugador1();
        var j2 = gameManager.GetJugador2();

        if (j1 != null)
            ActualizarJugador(j1, territoriosJ1, tropasJ1, infanteriaJ1, artilleriaJ1, caballeriaJ1);

        if (j2 != null)
            ActualizarJugador(j2, territoriosJ2, tropasJ2, infanteriaJ2, artilleriaJ2, caballeriaJ2);
    }

    private void ActualizarJugador(Jugador jugador, TextMeshProUGUI textoTerritorios,
        TextMeshProUGUI textoTropas, TextMeshProUGUI textoInfanteria,
        TextMeshProUGUI textoArtilleria, TextMeshProUGUI textoCaballeria)
    {
        if (textoTerritorios != null)
            textoTerritorios.text = $"Territorios:\n{jugador.getCantidadTerritorios()}";

        if (textoTropas != null)
            textoTropas.text = $"Tropas:\n{jugador.getTotalTropas()}";

        int infanteria = 0, artilleria = 0, caballeria = 0;
        var tarjetas = jugador.getTarjetas();

        for (int i = 0; i < tarjetas.getSize(); i++)
        {
            Tarjeta tarjeta = tarjetas.Obtener(i);

            if (!tarjeta.FueUsada())
            {
                switch (tarjeta.GetTipo())
                {
                    case TipoTarjeta.Infanteria:
                        infanteria++;
                        break;
                    case TipoTarjeta.Artilleria:
                        artilleria++;
                        break;
                    case TipoTarjeta.Caballeria:
                        caballeria++;
                        break;
                }
            }
        }

        if (textoInfanteria != null)
            textoInfanteria.text = $"Infantería: {infanteria}";

        if (textoArtilleria != null)
            textoArtilleria.text = $"Artillería: {artilleria}";

        if (textoCaballeria != null)
            textoCaballeria.text = $"Caballería: {caballeria}";
    }
}