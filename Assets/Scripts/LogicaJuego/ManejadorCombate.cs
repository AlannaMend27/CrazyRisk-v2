using System;
using CrazyRisk.Estructuras;
using CrazyRisk.Modelos;

namespace CrazyRisk.LogicaJuego
{
    public class ManejadorCombate
    {
        private Random random;

        public ManejadorCombate()
        {
            random = new Random();
        }

        /// <summary>
        /// Simula el lanzamiento de dados para el atacante
        /// </summary>
        /// <param name="cantidadDados">Número de dados (1-3)</param>
        /// <returns>Array de resultados ordenados de mayor a menor</returns>
        public int[] LanzarDadosAtacante(int cantidadDados)
        {
            if (cantidadDados < 1 || cantidadDados > 3)
                throw new ArgumentException("El atacante puede usar entre 1 y 3 dados");

            int[] dados = new int[cantidadDados];

            for (int i = 0; i < cantidadDados; i++)
            {
                // Dados del 1 al 6
                dados[i] = random.Next(1, 7);
            }

            // Ordenar de mayor a menor
            Array.Sort(dados);
            Array.Reverse(dados);

            return dados;
        }

        /// <summary>
        /// Simula el lanzamiento de dados para el defensor
        /// </summary>
        /// <param name="cantidadDados">Número de dados (1-2)</param>
        /// <returns>Array de resultados ordenados de mayor a menor</returns>
        public int[] LanzarDadosDefensor(int cantidadDados)
        {
            if (cantidadDados < 1 || cantidadDados > 2)
                throw new ArgumentException("El defensor puede usar entre 1 y 2 dados");

            int[] dados = new int[cantidadDados];

            for (int i = 0; i < cantidadDados; i++)
            {
                dados[i] = random.Next(1, 7);
            }

            Array.Sort(dados);
            Array.Reverse(dados);

            return dados;
        }

        /// <summary>
        /// Resuelve un combate completo comparando dados según reglas de Risk
        /// </summary>
        /// <param name="dadosAtacante">Dados del atacante ordenados</param>
        /// <param name="dadosDefensor">Dados del defensor ordenados</param>
        /// <returns>Resultado indicando tropas perdidas por cada lado</returns>
        public string ResolverCombate(int[] dadosAtacante, int[] dadosDefensor)
        {
            int tropasPerdidasAtacante = 0;
            int tropasPerdidasDefensor = 0;

            // Número de comparaciones = el menor entre ambos arrays
            int comparaciones = Math.Min(dadosAtacante.Length, dadosDefensor.Length);

            for (int i = 0; i < comparaciones; i++)
            {
                if (dadosAtacante[i] > dadosDefensor[i])
                {
                    tropasPerdidasDefensor++; // Atacante gana esta comparación
                }
                else
                {
                    tropasPerdidasAtacante++; // Defensor gana (incluye empates)
                }
            }

            return ("Resultado Combate - Atacante pierde: " + tropasPerdidasAtacante + ", Defensor pierde: " + tropasPerdidasDefensor);
        }
    }


}
