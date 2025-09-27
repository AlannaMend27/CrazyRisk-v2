using System;
using CrazyRisk.Estructuras;
using CrazyRisk.Modelos;

namespace CrazyRisk.LogicaJuego
{
    public class ManejadorRefuerzos
    {
        private int cantidadTarjetas;
        private static int contadorGlobalIntercambios = 0;
        private Random random;

        public ManejadorRefuerzos()
        {
            cantidadTarjetas = 0;
            random = new Random();
        }


        /// <summary>
        /// Calcular la cantidad de refuerzos que recibira el jugador
        /// </summary>
        public int CalcularRefuerzos(int CantidadTerritorios, Lista<Territorio> territoriosJugador, Lista<Continente> Continentes)
        {
            int bonusTotal = 0;
            for (int i = 0; i < 6; i++)
            {
                Continente continente = Continentes.Obtener(i);
                bonusTotal += continente.VerificaContinenteCompleto(territoriosJugador);
            }

            int Tropas = (CantidadTerritorios / 3) + bonusTotal;
            return Tropas;
           
        }

        public int Fibonacci()
        {
            int result = 0;
            int cont = 0;

            if (contadorGlobalIntercambios == 0)
            {
                return 0;
            }

            if (contadorGlobalIntercambios == 1)
            {
                return 2;
            }

            if (contadorGlobalIntercambios == 2)
            {
                return 3;
            }

            int prev = 2;
            int current = 3;

            while(cont <= contadorGlobalIntercambios - 2)
            {
                result = prev + current;
                prev = current;
                current = result;
                cont++;
            }

            return result;
        }


    }
}