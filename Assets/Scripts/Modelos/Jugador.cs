using System;
using CrazyRisk.Estructuras;
using CrazyRisk.Modelos;

namespace CrazyRisk.Modelos
{
    public class Jugador
    {
        // Propiedades básicas del jugador
        private int id;
        private string nombre;
        private string color;
        private bool turno;
        private bool esNeutral;

        // Colecciones del jugador
        private Lista<Territorio> territoriosControlados;
        private Lista<Tarjeta> tarjetas;

        // Constructor
        public Jugador(int id, string nombre, string color, bool esNeutral = false)
        {
            this.id = id;
            this.nombre = nombre;
            this.color = color;
            this.turno = false;
            this.esNeutral = esNeutral;

            territoriosControlados = new Lista<Territorio>();
            tarjetas = new Lista<Tarjeta>();
        }

        // Constructor para jugador neutral
        public static Jugador CrearJugadorNeutral(int id, string color = "Gris")
        {
            Jugador neutral = new Jugador(id, "Neutral", color);
            neutral.setEsNeutral(true);
            return neutral;
        }


        // Getters y setters
        public int getId()
        {
            return id;
        }

        public string getNombre()
        {
            return nombre;
        }

        public string getColor()
        {
            return color;
        }

        public void setColor(string nuevoColor)
        {
            color = nuevoColor;
        }

        public bool getEsTurno()
        {
            return turno;
        }

        public void setEsTurno(bool dataTurno)
        {
            this.turno = dataTurno;
        }

        public bool getEsNeutral()
        {
            return esNeutral;
        }

        public void setEsNeutral(bool data)
        {
            this.esNeutral = data;
        }


        public Lista<Territorio> getTerritoriosControlados()
        {
            return territoriosControlados;
        }

        public void setTerritoriosControlados(Lista<Territorio> nuevosTerritorios)
        {
            territoriosControlados = nuevosTerritorios;
        }

        public Lista<Tarjeta> getTarjetas()
        {
            return tarjetas;
        }

        public void setTarjetas(Lista<Tarjeta> nuevasTarjetas)
        {
            tarjetas = nuevasTarjetas;
        }

        // Métodos para gestión de territorios

        /// <summary>
        /// Agrega un territorio a la lista de territorios controlados
        /// </summary>
        public void AgregarTerritorio(Territorio territorio)
        {
            territorio.PropietarioId = this.id;
            territoriosControlados.Agregar(territorio);
        }

        /// <summary>
        /// Remueve un territorio de la lista de territorios controlados
        /// </summary>
        public void RemoverTerritorio(Territorio territorio)
        {
            territoriosControlados.Remover(territorio);
        }

        /// <summary>
        /// Obtiene la cantidad total de territorios controlados
        /// </summary>
        public int getCantidadTerritorios()
        {
            return territoriosControlados.getSize();
        }

        /// <summary>
        /// Verifica si el jugador controla un territorio específico
        /// </summary>
        public bool ControlaTerritorio(Territorio territorio)
        {
            return territoriosControlados.Contiene(territorio);
        }

        /// <summary>
        /// Obtiene la cantidad total de tropas que tiene el jugador
        /// </summary>
        public int getTotalTropas()
        {
            int total = 0;
            for (int i = 0; i < territoriosControlados.getSize(); i++)
            {
                total += territoriosControlados[i].CantidadTropas;
            }
            return total;
        }
        

        // Métodos para condiciones de victoria

        /// <summary>
        /// Verifica si el jugador ha ganado la partida (controla todos los territorios)
        /// </summary>
        public bool HaGanado(int totalTerritoriosEnMapa)
        {
            return territoriosControlados.getSize() == totalTerritoriosEnMapa;
        }

        /// <summary>
        /// Verifica si el jugador ha perdido (no tiene territorios)
        /// </summary>
        public bool HaPerdido()
        {
            return territoriosControlados.getSize() == 0;
        }


        // Métodos para turnos y acciones

        /// <summary>
        /// Inicia el turno del jugador
        /// </summary>
        public void IniciarTurno()
        {
            setEsTurno(true);
        }

        /// <summary>
        /// Finaliza el turno del jugador
        /// </summary>
        public void FinalizarTurno()
        {
            setEsTurno(true);
        }

    }
}
