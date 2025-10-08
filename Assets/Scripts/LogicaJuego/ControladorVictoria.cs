using UnityEngine;
using UnityEngine.SceneManagement;
using CrazyRisk.Red;

namespace CrazyRisk.Managers
{
    public class ControladorVictoria : MonoBehaviour
    {
        /// <summary>
        /// Desconecta del servidor o cliente si están activos, limpia las preferencias del jugador,
        /// restaura el tiempo del juego y carga la escena del menú principal.
        /// </summary>
        public void VolverAlMenu()
        {
            Debug.Log("Volviendo al men� principal...");

            // Desconecta del servidor/cliente si hay conexion activa
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

            // Limpiar PlayerPrefs de la sesion
            PlayerPrefs.DeleteKey("NombreJugador");
            PlayerPrefs.DeleteKey("EsServidor");
            PlayerPrefs.DeleteKey("ModoSolo");
            PlayerPrefs.DeleteKey("CantidadJugadores");
            PlayerPrefs.DeleteKey("IP");
            PlayerPrefs.Save();

            // Restaurar el tiempo (en caso de pausa)
            Time.timeScale = 1;

            // Cargar la escena del menu
            SceneManager.LoadScene("MenuPrincipal");
        }
    }
}
