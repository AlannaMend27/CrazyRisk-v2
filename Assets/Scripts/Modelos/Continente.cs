using CrazyRisk.Estructuras;
using CrazyRisk.Modelos;

namespace CrazyRisk.Modelos
{
    /// <summary>
    /// Clase que representa un continente con sus territorios
    /// </summary>
    public class Continente
    {
        private string nombre;
        private int bonificacion;
        private Lista<string> nombreTerritorios;

        public Continente(string nombre, int bonificacion)
        {
            this.nombre = nombre;
            this.bonificacion = bonificacion;
            this.nombreTerritorios = new Lista<string>();
            InicializarTerritorios();
        }

        public string ObtenerNombre()
        {
            return nombre;
        }

        public void EstablecerNombre(string nuevoNombre)
        {
            nombre = nuevoNombre;
        }

        public int ObtenerBonificacion()
        {
            return bonificacion;
        }

        public void EstablecerBonificacion(int nuevaBonificacion)
        {
            bonificacion = nuevaBonificacion;
        }

        public Lista<string> ObtenerNombreTerritorios()
        {
            return nombreTerritorios;
        }

        /// <summary>
        /// Inicializa los territorios según el mapa clásico de Risk
        /// </summary>
        private void InicializarTerritorios()
        {
            switch (nombre)
            {
                case "North America":
                    nombreTerritorios.Agregar("Alaska");
                    nombreTerritorios.Agregar("Northwest Territory");
                    nombreTerritorios.Agregar("Greenland");
                    nombreTerritorios.Agregar("Alberta");
                    nombreTerritorios.Agregar("Ontario");
                    nombreTerritorios.Agregar("Quebec");
                    nombreTerritorios.Agregar("Western United States");
                    nombreTerritorios.Agregar("Eastern United States");
                    nombreTerritorios.Agregar("Central America");
                    break;

                case "South America":
                    nombreTerritorios.Agregar("Venezuela");
                    nombreTerritorios.Agregar("Brazil");
                    nombreTerritorios.Agregar("Peru");
                    nombreTerritorios.Agregar("Argentina");
                    break;

                case "Europe":
                    nombreTerritorios.Agregar("Iceland");
                    nombreTerritorios.Agregar("Great Britain");
                    nombreTerritorios.Agregar("Scandinavia");
                    nombreTerritorios.Agregar("Northern Europe");
                    nombreTerritorios.Agregar("Western Europe");
                    nombreTerritorios.Agregar("Southern Europe");
                    nombreTerritorios.Agregar("Ukraine");
                    break;

                case "Africa":
                    nombreTerritorios.Agregar("North Africa");
                    nombreTerritorios.Agregar("Egypt");
                    nombreTerritorios.Agregar("East Africa");
                    nombreTerritorios.Agregar("Congo");
                    nombreTerritorios.Agregar("South Africa");
                    nombreTerritorios.Agregar("Madagascar");
                    break;

                case "Asia":
                    nombreTerritorios.Agregar("Ural");
                    nombreTerritorios.Agregar("Siberia");
                    nombreTerritorios.Agregar("Yakutsk");
                    nombreTerritorios.Agregar("Kamchatka");
                    nombreTerritorios.Agregar("Irkutsk");
                    nombreTerritorios.Agregar("Mongolia");
                    nombreTerritorios.Agregar("Japan");
                    nombreTerritorios.Agregar("China");
                    nombreTerritorios.Agregar("India");
                    nombreTerritorios.Agregar("Siam");
                    nombreTerritorios.Agregar("Middle East");
                    nombreTerritorios.Agregar("Afghanistan");
                    break;

                case "Oceania":
                    nombreTerritorios.Agregar("Indonesia");
                    nombreTerritorios.Agregar("New Guinea");
                    nombreTerritorios.Agregar("Western Australia");
                    nombreTerritorios.Agregar("Eastern Australia");
                    break;
            }
        }

        /// <summary>
        /// Verifica si un jugador controla todos los territorios del continente
        /// </summary>
        public int VerificaContinenteCompleto(Lista<Territorio> territoriosJugador)
        {
            int territoriosEnContinente = 0;

            for (int i = 0; i < territoriosJugador.getSize(); i++)
            {
                Territorio territorio = territoriosJugador.Obtener(i);

                for (int j = 0; j < nombreTerritorios.getSize(); j++)
                {
                    if (territorio.Nombre == nombreTerritorios.Obtener(j))
                    {
                        territoriosEnContinente++;
                        break;
                    }
                }
            }

            if (territoriosEnContinente == nombreTerritorios.getSize())
            {
                return bonificacion;
            }

            return 0;
        }
    }
}