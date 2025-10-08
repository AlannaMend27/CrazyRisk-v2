using UnityEngine;

namespace CrazyRisk.Managers
{
    /// <summary>
    /// Administra y reproduce los efectos de sonido del juego.
    /// Implementa el patrón Singleton para persistencia entre escenas.
    /// </summary>
    public class ManagerSonidos : MonoBehaviour
    {
        public static ManagerSonidos Instance;

        [Header("Efectos de Sonido")]
        [SerializeField] private AudioClip conquista;
        [SerializeField] private AudioClip colocarTropa;
        [SerializeField] private AudioClip conectadoOTurno;
        [SerializeField] private AudioClip dados;
        [SerializeField] private AudioClip victoria;
        [SerializeField] private AudioClip click;
        [SerializeField] private AudioClip error;

        [Header("Configuracion")]
        [SerializeField] private float volumenEfectos = 0.7f;

        private AudioSource audioSource;

        /// <summary>
        /// Inicializa el singleton y configura el AudioSource.
        /// </summary>
        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;

                // Asegurarse de que el GameObject sea raiz antes de usar DontDestroyOnLoad
                transform.SetParent(null);
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.volume = volumenEfectos;
        }

        /// <summary>
        /// Reproduce el sonido de conquista de territorio.
        /// </summary>
        public void ReproducirConquista()
        {
            if (conquista != null)
                audioSource.PlayOneShot(conquista);
        }

        /// <summary>
        /// Reproduce el sonido al colocar tropas.
        /// </summary>
        public void ReproducirColocarTropas()
        {
            if (colocarTropa != null)
                audioSource.PlayOneShot(colocarTropa);
        }

        /// <summary>
        /// Reproduce el sonido al cambiar de turno o al conectarse.
        /// </summary>
        public void ReproducirCambioTurno()
        {
            if (conectadoOTurno != null)
                audioSource.PlayOneShot(conectadoOTurno);
        }

        /// <summary>
        /// Reproduce el sonido de lanzamiento de dados.
        /// </summary>
        public void ReproducirDados()
        {
            if (dados != null)
                audioSource.PlayOneShot(dados);
        }

        /// <summary>
        /// Reproduce el sonido de victoria.
        /// </summary>
        public void ReproducirVictoria()
        {
            if (victoria != null)
                audioSource.PlayOneShot(victoria);
        }

        /// <summary>
        /// Reproduce el sonido de click en la interfaz.
        /// </summary>
        public void ReproducirClick()
        {
            if (click != null)
                audioSource.PlayOneShot(click);
        }

        /// <summary>
        /// Reproduce el sonido de error.
        /// </summary>
        public void ReproducirError()
        {
            if (error != null)
                audioSource.PlayOneShot(error);
        }
    }
}