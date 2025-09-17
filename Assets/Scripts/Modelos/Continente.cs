using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CrazyRisk.Modelos
{
    /// <summary>
    /// Continentes del juego Crazy Risk con sus respectivas bonificaciones de refuerzo
    /// </summary>
    public enum Continente
    {
        /// <summary>
        /// Asia - Bonificación: +7 tropas
        /// </summary>
        Asia,

        /// <summary>
        /// Europa - Bonificación: +5 tropas
        /// </summary>
        Europa,

        /// <summary>
        /// América del Norte - Bonificación: +3 tropas
        /// </summary>
        AmericaNorte,

        /// <summary>
        /// África - Bonificación: +3 tropas
        /// </summary>
        Africa,

        /// <summary>
        /// América del Sur - Bonificación: +2 tropas
        /// </summary>
        AmericaSur,

        /// <summary>
        /// Oceanía - Bonificación: +2 tropas
        /// </summary>
        Oceania
    }

    /// <summary>
    /// Clase auxiliar para obtener información sobre los continentes
    /// </summary>
    public static class ContinenteHelper
    {
        /// <summary>
        /// Obtiene la bonificación de tropas para un continente específico
        /// </summary>
        /// <param name="continente">Continente del cual obtener la bonificación</param>
        /// <returns>Número de tropas de bonificación</returns>
        public static int ObtenerBonificacion(Continente continente)
        {
            switch (continente)
            {
                case Continente.Asia:
                    return 7;
                case Continente.Europa:
                    return 5;
                case Continente.AmericaNorte:
                    return 3;
                case Continente.Africa:
                    return 3;
                case Continente.AmericaSur:
                    return 2;
                case Continente.Oceania:
                    return 2;
                default:
                    return 0;
            }
        }

        /// <summary>
        /// Obtiene el nombre completo del continente para mostrar en interfaz
        /// </summary>
        /// <param name="continente">Continente del cual obtener el nombre</param>
        /// <returns>Nombre completo del continente</returns>
        public static string ObtenerNombreCompleto(Continente continente)
        {
            switch (continente)
            {
                case Continente.Asia:
                    return "Asia";
                case Continente.Europa:
                    return "Europa";
                case Continente.AmericaNorte:
                    return "América del Norte";
                case Continente.Africa:
                    return "África";
                case Continente.AmericaSur:
                    return "América del Sur";
                case Continente.Oceania:
                    return "Oceanía";
                default:
                    return "Desconocido";
            }
        }
    }
}
