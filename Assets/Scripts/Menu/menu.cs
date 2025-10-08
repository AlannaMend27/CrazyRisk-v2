using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class MainMenu : MonoBehaviour
{
    [Header("Pantallas")]
    public GameObject mainMenu;
    public GameObject crearPartidaMenu;
    public GameObject joinGameMenu;
    public GameObject optionsMenu;

    [Header("Campos")]
    public TMP_InputField inputNombreCrear;
    public TMP_InputField inputNombreJoin;
    public TMP_InputField inputIP;
    public TMP_Dropdown dropdownModo;
    public TextMeshProUGUI textoEstado;

    void Start()
    {
        MostrarMenuPrincipal();
        if (inputIP != null) inputIP.text = "127.0.0.1";
    }

    /// <summary>
    /// Muestra el menú principal y oculta los demás menús.
    /// </summary>
    public void MostrarMenuPrincipal()
    {
        mainMenu.SetActive(true);
        crearPartidaMenu.SetActive(false);
        joinGameMenu.SetActive(false);
        optionsMenu.SetActive(false);
        LimpiarMensajes();
    }

    /// <summary>
    /// Muestra el menú para crear partida y oculta los demás menús.
    /// </summary>
    public void MostrarCrearPartida()
    {
        mainMenu.SetActive(false);
        crearPartidaMenu.SetActive(true);
        joinGameMenu.SetActive(false);
        optionsMenu.SetActive(false);
        LimpiarMensajes();
    }

    /// <summary>
    /// Muestra el menú para unirse a partida y oculta los demás menús.
    /// </summary>
    public void MostrarJoinGame()
    {
        mainMenu.SetActive(false);
        crearPartidaMenu.SetActive(false);
        joinGameMenu.SetActive(true);
        optionsMenu.SetActive(false);
        LimpiarMensajes();
    }

    /// <summary>
    /// Muestra el menú de reglas/opciones y oculta los demás menús.
    /// </summary>
    public void MostrarReglas()
    {
        mainMenu.SetActive(false);
        crearPartidaMenu.SetActive(false);
        joinGameMenu.SetActive(false);
        optionsMenu.SetActive(true);
        LimpiarMensajes();
    }

    /// <summary>
    /// Crea una partida según el modo seleccionado y guarda las preferencias.
    /// </summary>
    public void CrearPartida()
    {
        string nombre = inputNombreCrear.text.Trim();
        if (!ValidarNombre(nombre)) return;

        PlayerPrefs.SetString("NombreJugador", nombre);

        switch (dropdownModo.value)
        {
            case 0: // Solitario
                PlayerPrefs.SetInt("ModoSolo", 1);
                PlayerPrefs.SetInt("EsServidor", 0);
                PlayerPrefs.SetInt("CantidadJugadores", 2);
                break;
            case 1: // Online 2 jugadores
                PlayerPrefs.SetInt("ModoSolo", 0);
                PlayerPrefs.SetInt("EsServidor", 1);
                PlayerPrefs.SetInt("CantidadJugadores", 2);
                break;
            case 2: // Online 3 jugadores
                PlayerPrefs.SetInt("ModoSolo", 0);
                PlayerPrefs.SetInt("EsServidor", 1);
                PlayerPrefs.SetInt("CantidadJugadores", 3);
                break;
        }

        PlayerPrefs.Save();
        SceneManager.LoadScene("mapita");
    }

    /// <summary>
    /// Permite unirse a una partida online, validando nombre e IP.
    /// </summary>
    public void UnirsePartida()
    {
        string nombre = inputNombreJoin.text.Trim();
        string ip = inputIP.text.Trim();

        if (!ValidarNombre(nombre) || !ValidarIP(ip)) return;

        PlayerPrefs.SetString("NombreJugador", nombre);
        PlayerPrefs.SetString("IP", ip);
        PlayerPrefs.SetInt("EsServidor", 0);
        PlayerPrefs.SetInt("ModoSolo", 0);
        PlayerPrefs.Save();

        SceneManager.LoadScene("mapita");
    }

    /// <summary>
    /// Cierra la aplicación.
    /// </summary>
    public void CerrarJuego()
    {
        Application.Quit();
    }

    /// <summary>
    /// Valida que el nombre ingresado sea válido.
    /// </summary>
    private bool ValidarNombre(string nombre)
    {
        if (string.IsNullOrEmpty(nombre) || nombre.Length < 2)
        {
            MostrarMensaje("Nombre debe tener al menos 2 caracteres", Color.red);
            return false;
        }
        return true;
    }

    /// <summary>
    /// Valida que la IP ingresada sea válida.
    /// </summary>
    private bool ValidarIP(string ip)
    {
        if (string.IsNullOrEmpty(ip))
        {
            MostrarMensaje("Ingresa la IP del servidor", Color.red);
            return false;
        }
        return true;
    }

    /// <summary>
    /// Muestra un mensaje en la interfaz con el color especificado.
    /// </summary>
    private void MostrarMensaje(string mensaje, Color color)
    {
        if (textoEstado != null)
        {
            textoEstado.text = mensaje;
            textoEstado.color = color;
        }
    }

    /// <summary>
    /// Limpia los mensajes de estado en la interfaz.
    /// </summary>
    private void LimpiarMensajes()
    {
        if (textoEstado != null) textoEstado.text = "";
    }
}