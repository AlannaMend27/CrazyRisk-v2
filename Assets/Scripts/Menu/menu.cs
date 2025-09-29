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

    // NAVEGACIÓN
    public void MostrarMenuPrincipal()
    {
        mainMenu.SetActive(true);
        crearPartidaMenu.SetActive(false);
        joinGameMenu.SetActive(false);
        optionsMenu.SetActive(false);
        LimpiarMensajes();
    }

    public void MostrarCrearPartida()
    {
        mainMenu.SetActive(false);
        crearPartidaMenu.SetActive(true);
        joinGameMenu.SetActive(false);
        optionsMenu.SetActive(false);
        LimpiarMensajes();
    }

    public void MostrarJoinGame()
    {
        mainMenu.SetActive(false);
        crearPartidaMenu.SetActive(false);
        joinGameMenu.SetActive(true);
        optionsMenu.SetActive(false);
        LimpiarMensajes();
    }

    public void MostrarReglas()
    {
        mainMenu.SetActive(false);
        crearPartidaMenu.SetActive(false);
        joinGameMenu.SetActive(false);
        optionsMenu.SetActive(true);
        LimpiarMensajes();
    }

    // ACCIONES PRINCIPALES
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

    public void CerrarJuego()
    {
        Application.Quit();
    }

    // VALIDACIONES
    private bool ValidarNombre(string nombre)
    {
        if (string.IsNullOrEmpty(nombre) || nombre.Length < 2)
        {
            MostrarMensaje("Nombre debe tener al menos 2 caracteres", Color.red);
            return false;
        }
        return true;
    }

    private bool ValidarIP(string ip)
    {
        if (string.IsNullOrEmpty(ip))
        {
            MostrarMensaje("Ingresa la IP del servidor", Color.red);
            return false;
        }
        return true;
    }

    // UTILIDADES
    private void MostrarMensaje(string mensaje, Color color)
    {
        if (textoEstado != null)
        {
            textoEstado.text = mensaje;
            textoEstado.color = color;
        }
    }

    private void LimpiarMensajes()
    {
        if (textoEstado != null) textoEstado.text = "";
    }
}