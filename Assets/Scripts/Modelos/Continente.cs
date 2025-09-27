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
        private Lista<string> nombreTerritorios; // Nombres de los territorios que pertenecen a este continente

        public Continente(string nombre, int bonificacion)
        {
            this.nombre = nombre;
            this.bonificacion = bonificacion;
            this.nombreTerritorios = new Lista<string>();
            InicializarTerritorios();
        }

        // Getters y setters individuales
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
        /// Inicializa los territorios que pertenecen a este continente
        /// </summary>
        private void InicializarTerritorios()
        {
            switch (nombre)
            {
                case "Asia":
                    nombreTerritorios.Agregar("China");
                    nombreTerritorios.Agregar("India");
                    nombreTerritorios.Agregar("Japón");
                    nombreTerritorios.Agregar("Rusia");
                    nombreTerritorios.Agregar("Mongolia");
                    nombreTerritorios.Agregar("Corea");
                    nombreTerritorios.Agregar("Tailandia");
                    break;

                case "Europa":
                    nombreTerritorios.Agregar("Francia");
                    nombreTerritorios.Agregar("Alemania");
                    nombreTerritorios.Agregar("España");
                    nombreTerritorios.Agregar("Italia");
                    nombreTerritorios.Agregar("Reino Unido");
                    nombreTerritorios.Agregar("Polonia");
                    nombreTerritorios.Agregar("Grecia");
                    break;

                case "América del Norte":
                    nombreTerritorios.Agregar("Estados Unidos");
                    nombreTerritorios.Agregar("Canadá");
                    nombreTerritorios.Agregar("México");
                    nombreTerritorios.Agregar("Alaska");
                    nombreTerritorios.Agregar("Groenlandia");
                    nombreTerritorios.Agregar("Cuba");
                    nombreTerritorios.Agregar("Guatemala");
                    break;

                case "África":
                    nombreTerritorios.Agregar("Egipto");
                    nombreTerritorios.Agregar("Sudáfrica");
                    nombreTerritorios.Agregar("Nigeria");
                    nombreTerritorios.Agregar("Kenia");
                    nombreTerritorios.Agregar("Marruecos");
                    nombreTerritorios.Agregar("Congo");
                    nombreTerritorios.Agregar("Madagascar");
                    break;

                case "América del Sur":
                    nombreTerritorios.Agregar("Brasil");
                    nombreTerritorios.Agregar("Argentina");
                    nombreTerritorios.Agregar("Chile");
                    nombreTerritorios.Agregar("Perú");
                    nombreTerritorios.Agregar("Colombia");
                    nombreTerritorios.Agregar("Venezuela");
                    nombreTerritorios.Agregar("Uruguay");
                    break;

                case "Oceanía":
                    nombreTerritorios.Agregar("Australia");
                    nombreTerritorios.Agregar("Nueva Zelanda");
                    nombreTerritorios.Agregar("Indonesia");
                    nombreTerritorios.Agregar("Filipinas");
                    nombreTerritorios.Agregar("Papua Nueva Guinea");
                    nombreTerritorios.Agregar("Fiji");
                    nombreTerritorios.Agregar("Tahití");
                    break;
            }
        }


        ///<summary>
        /// Metodo que verifica la cantidad de bonus que recibira un jugador
        /// </summary>
        public int VerificaContinenteCompleto(Lista<Territorio> territoriosJugador)
        {
            int territoriosEnContinente = 0;

            for (int i = 0; i < territoriosJugador.getSize(); i++)
            {
                Territorio territorio = territoriosJugador.Obtener(i);
                if (territorio.getContinente() == nombre)
                {
                    territoriosEnContinente++;
                }
            }

            if (territoriosEnContinente == 7)
            {
                return bonificacion;
            }
            else
            {
                return 0;
            }

        }
    }
}

