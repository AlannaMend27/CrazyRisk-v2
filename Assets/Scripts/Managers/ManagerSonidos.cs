using UnityEngine;

namespace CrazyRisk.Managers
{
    public class ManagerSonidos : MonoBehaviour
    {
        public static ManagerSonidos Instance;

        [Header("Efectos de Sonido")]
        [SerializeField] private AudioClip ataque;
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

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
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

        public void ReproducirAtaque()
        {
            if (ataque != null)
                audioSource.PlayOneShot(ataque);
        }

        public void ReproducirConquista()
        {
            if (conquista != null)
                audioSource.PlayOneShot(conquista);
        }

        public void ReproducirColocarTropas()
        {
            if (colocarTropa != null)
                audioSource.PlayOneShot(colocarTropa);
        }

        public void ReproducirCambioTurno()
        {
            if (conectadoOTurno != null)
                audioSource.PlayOneShot(conectadoOTurno);
        }

        public void ReproducirDados()
        {
            if (dados != null)
                audioSource.PlayOneShot(dados);
        }

        public void ReproducirVictoria()
        {
            if (victoria != null)
                audioSource.PlayOneShot(victoria);
        }

        public void ReproducirClick()
        {
            if (click != null)
                audioSource.PlayOneShot(click);
        }

        public void ReproducirError()
        {
            if (error != null)
                audioSource.PlayOneShot(error);
        }
    }
}