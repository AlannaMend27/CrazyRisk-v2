using UnityEngine;
using UnityEngine.SceneManagement;
using CrazyRisk.Red;

namespace CrazyRisk.Managers
{
    public class ControladorVictoria : MonoBehaviour
    {
        public void VolverAlMenu()
        {
            Debug.Log("Volviendo al men� principal...");

            // Desconectar del servidor/cliente si hay conexi�n activa
            ServidorRisk servidor = FindObjectOfType<ServidorRisk>();
            if (servidor != null)
            {
                servidor.DetenerServidor();
                Debug.Log("Servidor detenido");
            }

            ClienteRisk cliente = FindObjectOfType<ClienteRisk>();
            if (cliente != null)
            {
                cliente.Desconectar();
                Debug.Log("Cliente desconectado");
            }

            // Limpiar PlayerPrefs de la sesi�n
            PlayerPrefs.DeleteKey("NombreJugador");
            PlayerPrefs.DeleteKey("EsServidor");
            PlayerPrefs.DeleteKey("ModoSolo");
            PlayerPrefs.DeleteKey("CantidadJugadores");
            PlayerPrefs.DeleteKey("IP");
            PlayerPrefs.Save();

            // Restaurar el tiempo (por si lo pausaste)
            Time.timeScale = 1;

            // Cargar la escena del men�
            SceneManager.LoadScene("MenuPrincipal"); // O el nombre de tu escena de men�
        }
    }
}
