using UnityEngine;
using CrazyRisk.Modelos;
using CrazyRisk.Estructuras;

namespace CrazyRisk.LogicaJuego
{
    /// <summary>
    /// Gestiona el intercambio y validación de tríos de tarjetas para refuerzos en el juego.
    /// </summary>
    public class ManejadorTarjetas : MonoBehaviour
    {
        private static int contadorGlobalIntercambios = 0;
        private ManejadorRefuerzos manejadorRefuerzos;

        /// <summary>
        /// Inicializa el manejador de refuerzos al iniciar el componente.
        /// </summary>
        void Start()
        {
            manejadorRefuerzos = new ManejadorRefuerzos();
        }

        /// <summary>
        /// Intenta intercambiar 3 tarjetas del jugador.
        /// </summary>
        public bool IntentarIntercambio(Jugador jugador, int indice1, int indice2, int indice3)
        {
            Lista<Tarjeta> tarjetas = jugador.getTarjetas();

            if (indice1 >= tarjetas.getSize() || indice2 >= tarjetas.getSize() || indice3 >= tarjetas.getSize())
            {
                Debug.LogError("Indices de tarjetas invalidos");
                return false;
            }

            Tarjeta t1 = tarjetas.Obtener(indice1);
            Tarjeta t2 = tarjetas.Obtener(indice2);
            Tarjeta t3 = tarjetas.Obtener(indice3);

            // Verificar que no esten usadas
            if (t1.FueUsada() || t2.FueUsada() || t3.FueUsada())
            {
                Debug.LogWarning("Una de las tarjetas ya fue usada");
                return false;
            }

            // Validar trio
            if (!manejadorRefuerzos.EsTrioValido(t1, t2, t3))
            {
                Debug.LogWarning("El trio de tarjetas no es valido");
                return false;
            }

            // Intercambiar
            int refuerzos = manejadorRefuerzos.IntercambiarTarjetas(t1, t2, t3);

            Debug.Log($"{jugador.getNombre()} intercambio tarjetas y obtuvo {refuerzos} refuerzos");

            return true;
        }

        /// <summary>
        /// Encuentra automáticamente el mejor trío posible de tarjetas para intercambio.
        /// </summary>
        public int[] EncontrarTrioValido(Jugador jugador)
        {
            Lista<Tarjeta> tarjetas = jugador.getTarjetas();

            // Buscar primero 3 iguales
            for (int i = 0; i < tarjetas.getSize(); i++)
            {
                if (tarjetas.Obtener(i).FueUsada()) continue;

                for (int j = i + 1; j < tarjetas.getSize(); j++)
                {
                    if (tarjetas.Obtener(j).FueUsada()) continue;

                    for (int k = j + 1; k < tarjetas.getSize(); k++)
                    {
                        if (tarjetas.Obtener(k).FueUsada()) continue;

                        Tarjeta t1 = tarjetas.Obtener(i);
                        Tarjeta t2 = tarjetas.Obtener(j);
                        Tarjeta t3 = tarjetas.Obtener(k);

                        if (manejadorRefuerzos.EsTrioValido(t1, t2, t3))
                        {
                            return new int[] { i, j, k };
                        }
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Devuelve el valor actual del contador global de intercambios.
        /// </summary>
        public int GetContadorGlobal()
        {
            return contadorGlobalIntercambios;
        }
    }
}