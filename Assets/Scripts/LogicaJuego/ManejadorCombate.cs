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
        /// <param name="cantidadDados">N�mero de dados (1-2)</param>
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
        /// Resuelve un combate individual comparando dados seg�n reglas de Risk
        /// </summary>
        public string ResolverCombateIndividual(int[] dadosAtacante, int[] dadosDefensor)
        {
            int tropasPerdidasAtacante = 0;
            int tropasPerdidasDefensor = 0;

            // N�mero de comparaciones = el menor entre ambos arrays
            int comparaciones = Math.Min(dadosAtacante.Length, dadosDefensor.Length);

            for (int i = 0; i < comparaciones; i++)
            {
                if (dadosAtacante[i] > dadosDefensor[i])
                {
                    tropasPerdidasDefensor++; // Atacante gana esta comparaci�n
                }
                else
                {
                    tropasPerdidasAtacante++; // Defensor gana (incluye empates)
                }
            }

            // Construir resultado final (Provisional mientras es implementado en interfaz grafica)
            string dadosAtacanteStr = string.Join(", ", dadosAtacante);
            string dadosDefensorStr = string.Join(", ", dadosDefensor);

            string resultado = $"=== RESULTADO DEL COMBATE ===\n";
            resultado += $"Dados Atacante: [{dadosAtacanteStr}]\n";
            resultado += $"Dados Defensor: [{dadosDefensorStr}]\n\n";
            resultado += $"\n--- BAJAS ---\n";
            resultado += $"Atacante pierde: {tropasPerdidasAtacante} tropa(s)\n";
            resultado += $"Defensor pierde: {tropasPerdidasDefensor} tropa(s)\n";
            resultado += $"==============================";

            return resultado;
        }

        /// <summary>
        /// Valida si un ataque es legal seg�n las reglas de Risk
        /// </summary>
        public bool ValidarAtaque(Territorio atacante, Territorio defensor)
        {
            // El territorio atacante debe tener al menos 2 tropas (deja 1 de guarnici�n)
            if (atacante.CantidadTropas < 2)
                return false;

            // El defensor debe tener al menos 1 tropa
            if (defensor.CantidadTropas < 1)
                return false;

            // No puedes atacar tus propios territorios
            if (atacante.PropietarioId == defensor.PropietarioId)
                return false;

            // Verificar si el territorio es adyacente
            if (!atacante.EsAdyacenteA(defensor.Id))
            {
                return false;
            }

            return true;
        }


    }


}
