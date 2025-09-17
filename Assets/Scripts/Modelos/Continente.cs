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
        /// Asia - Bonificaci�n: +7 tropas
        /// </summary>
        Asia,

        /// <summary>
        /// Europa - Bonificaci�n: +5 tropas
        /// </summary>
        Europa,

        /// <summary>
        /// Am�rica del Norte - Bonificaci�n: +3 tropas
        /// </summary>
        AmericaNorte,

        /// <summary>
        /// �frica - Bonificaci�n: +3 tropas
        /// </summary>
        Africa,

        /// <summary>
        /// Am�rica del Sur - Bonificaci�n: +2 tropas
        /// </summary>
        AmericaSur,

        /// <summary>
        /// Ocean�a - Bonificaci�n: +2 tropas
        /// </summary>
        Oceania
    }

    /// <summary>
    /// Clase auxiliar para obtener informaci�n sobre los continentes
    /// </summary>
    public static class ContinenteHelper
    {
        /// <summary>
        /// Obtiene la bonificaci�n de tropas para un continente espec�fico
        /// </summary>
        /// <param name="continente">Continente del cual obtener la bonificaci�n</param>
        /// <returns>N�mero de tropas de bonificaci�n</returns>
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
                    return "Am�rica del Norte";
                case Continente.Africa:
                    return "�frica";
                case Continente.AmericaSur:
                    return "Am�rica del Sur";
                case Continente.Oceania:
                    return "Ocean�a";
                default:
                    return "Desconocido";
            }
        }
    }
}
