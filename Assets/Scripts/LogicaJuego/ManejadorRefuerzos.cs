using System;
using CrazyRisk.Estructuras;
using CrazyRisk.Modelos;

namespace CrazyRisk.LogicaJuego
{
    /// <summary>
    /// Gestiona la lógica de refuerzos, intercambio de tarjetas y cálculo de bonificaciones en el juego.
    /// </summary>
    public class ManejadorRefuerzos
    {
        private int cantidadTarjetas;
        private static int contadorGlobalIntercambios = 0; // Lleva la cuenta global de intercambios de tarjetas
        private Random random;

        /// <summary>
        /// Constructor: inicializa la cantidad de tarjetas y el generador de números aleatorios.
        /// </summary>
        public ManejadorRefuerzos()
        {
            cantidadTarjetas = 0;
            random = new Random();
        }

        /// <summary>
        /// Calcula la cantidad de refuerzos que recibirá el jugador según sus territorios y continentes controlados.
        /// </summary>
        public int CalcularRefuerzos(int CantidadTerritorios, Lista<Territorio> territoriosJugador, Lista<Continente> Continentes)
        {
            int bonusTotal = 0;

            // Verificar bonus de continentes si existe la lista
            if (Continentes != null)
            {
                for (int i = 0; i < Continentes.getSize(); i++)
                {
                    Continente continente = Continentes.Obtener(i);
                    bonusTotal += continente.VerificaContinenteCompleto(territoriosJugador);
                }
            }

            int Tropas = (CantidadTerritorios / 3) + bonusTotal;

            // Mínimo 3 tropas por turno según las reglas de Risk
            if (Tropas < 3)
                Tropas = 3;

            return Tropas;
        }

        /// <summary>
        /// Calcula el valor de Fibonacci para determinar la cantidad de refuerzos al intercambiar tarjetas.
        /// </summary>
        public int Fibonacci()
        {
            if (contadorGlobalIntercambios == 0) return 0;
            if (contadorGlobalIntercambios == 1) return 2;
            if (contadorGlobalIntercambios == 2) return 3;

            int prev = 2;
            int current = 3;

            for (int i = 2; i < contadorGlobalIntercambios; i++)
            {
                int next = prev + current;
                prev = current;
                current = next;
            }

            return current;
        }

        /// <summary>
        /// Verifica si un trío de tarjetas es válido para intercambio.
        /// </summary>
        public bool EsTrioValido(Tarjeta t1, Tarjeta t2, Tarjeta t3)
        {
            // Tres iguales
            if (t1.GetTipo() == t2.GetTipo() && t2.GetTipo() == t3.GetTipo())
                return true;

            // Una de cada tipo
            if (t1.GetTipo() != t2.GetTipo() &&
                t2.GetTipo() != t3.GetTipo() &&
                t1.GetTipo() != t3.GetTipo())
                return true;

            return false;
        }

        /// <summary>
        /// Elige un territorio aleatorio de una lista.
        /// </summary>
        public Territorio ElegirTerritorioAleatorio(Lista<Territorio> territorios)
        {
            if (territorios == null || territorios.getSize() == 0)
                return null;

            int indiceAleatorio = random.Next(territorios.getSize());
            return territorios.Obtener(indiceAleatorio);
        }

        /// <summary>
        /// Intercambia un trío de tarjetas por refuerzos.
        /// </summary>
        public int IntercambiarTarjetas(Tarjeta t1, Tarjeta t2, Tarjeta t3)
        {
            if (!EsTrioValido(t1, t2, t3))
            {
                UnityEngine.Debug.LogWarning("Trío de tarjetas inválido");
                return 0;
            }

            // Marcar tarjetas como usadas
            t1.MarcarComoUsada();
            t2.MarcarComoUsada();
            t3.MarcarComoUsada();

            // Incrementar contador e intercambiar
            IncrementarContadorGlobal();
            int refuerzos = Fibonacci();

            UnityEngine.Debug.Log($"Tarjetas intercambiadas. Refuerzos obtenidos: {refuerzos}");
            return refuerzos;
        }

        /// <summary>
        /// Incrementa el contador global de intercambios de tarjetas.
        /// </summary>
        public void IncrementarContadorGlobal()
        {
            contadorGlobalIntercambios++;
        }

        /// <summary>
        /// Devuelve el valor actual del contador global de intercambios.
        /// </summary>
        public int GetContadorGlobal()
        {
            return contadorGlobalIntercambios;
        }

        /// <summary>
        /// Establece la cantidad de tarjetas que posee el jugador.
        /// </summary>
        public void SetCantidadTarjetas(int cantidad)
        {
            cantidadTarjetas = cantidad;
        }

        /// <summary>
        /// Devuelve la cantidad de tarjetas que posee el jugador.
        /// </summary>
        public int GetCantidadTarjetas()
        {
            return cantidadTarjetas;
        }
    }
}