using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CrazyRisk.Estructuras;
using CrazyRisk.Modelos;
using CrazyRisk.LogicaJuego;

namespace CrazyRisk.LogicaJuego
{
    public class InicializadorJuego
    {
        // Propiedades 
        private Lista<Territorio> todosLosTerritorios;
        private Lista<Continente> todosContinentes;
        private Lista<Jugador> jugadores;
        private Jugador jugador1;
        private Jugador jugador2;
        private Jugador jugador3;  
        private Jugador jugadorNeutral;
        private Random random;
        private DistribuidorTerritorios distribuidor;

        private bool crearNeutral = true; 
        private int cantidadJugadores = 2; 

        /// <summary>
        /// Constructor que inicializa las listas de territorios, continentes y jugadores.
        /// </summary>
        public InicializadorJuego()
        {
            todosLosTerritorios = new Lista<Territorio>();
            todosContinentes = new Lista<Continente>();
            jugadores = new Lista<Jugador>();
            random = new Random();
        }

        /// <summary>
        /// Devuelve el distribuidor de territorios.
        /// </summary>
        public DistribuidorTerritorios GetDistribuidor()
        {
            return distribuidor;
        }

        /// <summary>
        /// Inicializa con configuracion de cantidad de jugadores
        /// </summary>
        public void InicializarJuegoCompleto(string nombreJugador1, string colorJugador1,
                                             string nombreJugador2, string colorJugador2,
                                             int numJugadores, bool incluirNeutral)
        {
            cantidadJugadores = numJugadores;
            crearNeutral = incluirNeutral;

            InicializarContinentes();
            InicializarTerritorios();
            ConfigurarAdyacencias();

            distribuidor = new DistribuidorTerritorios(todosLosTerritorios);

            InicializarJugadores(nombreJugador1, colorJugador1, nombreJugador2, colorJugador2);
            DistribuirTerritorios();
        }

        /// <summary>
        /// Inicializa el juego completo con dos jugadores y un jugador neutral
        /// </summary>
        public void InicializarJuegoCompleto(string nombreJugador1, string colorJugador1,
                                             string nombreJugador2, string colorJugador2)
        {
            InicializarJuegoCompleto(nombreJugador1, colorJugador1, nombreJugador2, colorJugador2, 2, true);
        }

        /// <summary>
        /// Inicializa la lista de continentes con sus respectivos bonos.
        /// </summary>
        private void InicializarContinentes()
        {
            todosContinentes.Agregar(new Continente("North America", 9));
            todosContinentes.Agregar(new Continente("South America", 4));
            todosContinentes.Agregar(new Continente("Europe", 7));
            todosContinentes.Agregar(new Continente("Africa", 6));
            todosContinentes.Agregar(new Continente("Asia", 12));
            todosContinentes.Agregar(new Continente("Oceania", 4));
        }

        /// <summary>
        /// Inicializa la lista de territorios y los asigna a sus continentes
        /// </summary>
        private void InicializarTerritorios()
        {
            // North America (9 territorios)
            CrearTerritorio(1, "Alaska", "North America");
            CrearTerritorio(2, "Northwest Territory", "North America");
            CrearTerritorio(3, "Greenland", "North America");
            CrearTerritorio(4, "Alberta", "North America");
            CrearTerritorio(5, "Ontario", "North America");
            CrearTerritorio(6, "Quebec", "North America");
            CrearTerritorio(7, "Western United States", "North America");
            CrearTerritorio(8, "Eastern United States", "North America");
            CrearTerritorio(9, "Central America", "North America");

            // South America (4 territorios)
            CrearTerritorio(10, "Venezuela", "South America");
            CrearTerritorio(11, "Brazil", "South America");
            CrearTerritorio(12, "Peru", "South America");
            CrearTerritorio(13, "Argentina", "South America");

            // Europe (7 territorios)
            CrearTerritorio(14, "Iceland", "Europe");
            CrearTerritorio(15, "Great Britain", "Europe");
            CrearTerritorio(16, "Scandinavia", "Europe");
            CrearTerritorio(17, "Northern Europe", "Europe");
            CrearTerritorio(18, "Western Europe", "Europe");
            CrearTerritorio(19, "Southern Europe", "Europe");
            CrearTerritorio(20, "Ukraine", "Europe");

            // Africa (6 territorios)
            CrearTerritorio(21, "North Africa", "Africa");
            CrearTerritorio(22, "Egypt", "Africa");
            CrearTerritorio(23, "East Africa", "Africa");
            CrearTerritorio(24, "Congo", "Africa");
            CrearTerritorio(25, "South Africa", "Africa");
            CrearTerritorio(26, "Madagascar", "Africa");

            // Asia (12 territorios)
            CrearTerritorio(27, "Ural", "Asia");
            CrearTerritorio(28, "Siberia", "Asia");
            CrearTerritorio(29, "Yakutsk", "Asia");
            CrearTerritorio(30, "Kamchatka", "Asia");
            CrearTerritorio(31, "Irkutsk", "Asia");
            CrearTerritorio(32, "Mongolia", "Asia");
            CrearTerritorio(33, "Japan", "Asia");
            CrearTerritorio(34, "China", "Asia");
            CrearTerritorio(35, "India", "Asia");
            CrearTerritorio(36, "Siam", "Asia");
            CrearTerritorio(37, "Middle East", "Asia");
            CrearTerritorio(38, "Afghanistan", "Asia");

            // Oceania (4 territorios)
            CrearTerritorio(39, "Indonesia", "Oceania");
            CrearTerritorio(40, "New Guinea", "Oceania");
            CrearTerritorio(41, "Western Australia", "Oceania");
            CrearTerritorio(42, "Eastern Australia", "Oceania");
        }


        /// <summary>
        /// Crea un territorio y lo agrega a la lista de territorios
        /// </summary>
        private void CrearTerritorio(int id, string nombre, string continente)
        {
            Territorio nuevoTerritorio = new Territorio(id, nombre, continente);
            todosLosTerritorios.Agregar(nuevoTerritorio);
        }

        /// <summary>
        /// Configura las adyacencias de los territorios
        /// </summary>
        private void ConfigurarAdyacencias()
        {
            // North America
            ConfigurarAdyacencia(1, new int[] { 2, 4, 30 });
            ConfigurarAdyacencia(2, new int[] { 1, 3, 4, 5 });
            ConfigurarAdyacencia(3, new int[] { 2, 5, 6, 14 });
            ConfigurarAdyacencia(4, new int[] { 1, 2, 7, 5 });
            ConfigurarAdyacencia(5, new int[] { 2, 3, 4, 6, 7, 8 });
            ConfigurarAdyacencia(6, new int[] { 3, 5, 8 });
            ConfigurarAdyacencia(7, new int[] { 4, 5, 8, 9 });
            ConfigurarAdyacencia(8, new int[] { 5, 6, 7, 9 });
            ConfigurarAdyacencia(9, new int[] { 7, 8, 10 });

            // South America
            ConfigurarAdyacencia(10, new int[] { 9, 11, 12 });
            ConfigurarAdyacencia(11, new int[] { 10, 12, 13, 21 });
            ConfigurarAdyacencia(12, new int[] { 10, 11, 13 });
            ConfigurarAdyacencia(13, new int[] { 11, 12 });

            // Europe
            ConfigurarAdyacencia(14, new int[] { 3, 15, 16 });
            ConfigurarAdyacencia(15, new int[] { 14, 16, 17, 18 });
            ConfigurarAdyacencia(16, new int[] { 14, 15, 17, 20 });
            ConfigurarAdyacencia(17, new int[] { 15, 16, 18, 19, 20 });
            ConfigurarAdyacencia(18, new int[] { 15, 17, 19, 21 });
            ConfigurarAdyacencia(19, new int[] { 17, 18, 20, 21, 22, 37 });
            ConfigurarAdyacencia(20, new int[] { 16, 17, 19, 27, 37, 38 });

            // Africa
            ConfigurarAdyacencia(21, new int[] { 11, 18, 19, 22, 23, 24 });
            ConfigurarAdyacencia(22, new int[] { 19, 21, 23, 37 });
            ConfigurarAdyacencia(23, new int[] { 21, 22, 24, 25, 26, 37 });
            ConfigurarAdyacencia(24, new int[] { 21, 23, 25 });
            ConfigurarAdyacencia(25, new int[] { 23, 24, 26 });
            ConfigurarAdyacencia(26, new int[] { 23, 25 });

            // Asia
            ConfigurarAdyacencia(27, new int[] { 20, 28, 34, 38 });
            ConfigurarAdyacencia(28, new int[] { 27, 29, 31, 32, 34 });
            ConfigurarAdyacencia(29, new int[] { 28, 30, 31 });
            ConfigurarAdyacencia(30, new int[] { 1, 29, 31, 32, 33 });
            ConfigurarAdyacencia(31, new int[] { 28, 29, 30, 32 });
            ConfigurarAdyacencia(32, new int[] { 28, 30, 31, 33, 34 });
            ConfigurarAdyacencia(33, new int[] { 30, 32 });
            ConfigurarAdyacencia(34, new int[] { 27, 28, 32, 35, 36, 38 });
            ConfigurarAdyacencia(35, new int[] { 34, 36, 37, 38 });
            ConfigurarAdyacencia(36, new int[] { 34, 35, 39 });
            ConfigurarAdyacencia(37, new int[] { 19, 20, 22, 23, 35, 38 });
            ConfigurarAdyacencia(38, new int[] { 20, 27, 34, 35, 37 });

            // Oceania
            ConfigurarAdyacencia(39, new int[] { 36, 40, 41 });
            ConfigurarAdyacencia(40, new int[] { 39, 41, 42 });
            ConfigurarAdyacencia(41, new int[] { 39, 40, 42 });
            ConfigurarAdyacencia(42, new int[] { 40, 41 });
        }

        private void ConfigurarAdyacencia(int territorioId, int[] adyacentes)
        {
            Territorio territorio = BuscarTerritorioPorId(territorioId);
            if (territorio != null)
            {
                for (int i = 0; i < adyacentes.Length; i++)
                {
                    territorio.AgregarAdyacente(adyacentes[i]);
                    Territorio territorioAdyacente = BuscarTerritorioPorId(adyacentes[i]);
                    if (territorioAdyacente != null)
                    {
                        territorioAdyacente.AgregarAdyacente(territorioId);
                    }
                }
            }
        }

        /// <summary>
        /// Busca y retorna un territorio por su ID
        /// </summary>
        private Territorio BuscarTerritorioPorId(int id)
        {
            for (int i = 0; i < todosLosTerritorios.getSize(); i++)
            {
                if (todosLosTerritorios[i].Id == id)
                {
                    return todosLosTerritorios[i];
                }
            }
            return null;
        }

        /// <summary>
        /// Inicializa los jugadores y, si corresponde, el jugador neutral.
        /// </summary>
        private void InicializarJugadores(string nombre1, string color1, string nombre2, string color2)
        {
            jugador1 = new Jugador(1, nombre1, color1);
            jugador2 = new Jugador(2, nombre2, color2);

            jugadores.Agregar(jugador1);
            jugadores.Agregar(jugador2);

            if (cantidadJugadores == 3 && !crearNeutral)
            {
                jugador3 = new Jugador(3, "Jugador 3", "Azul");
                jugadores.Agregar(jugador3);
            }
            else if (crearNeutral)
            {
                jugadorNeutral = Jugador.CrearJugadorNeutral(3, "Gris");
                jugadores.Agregar(jugadorNeutral);
            }
        }

        /// <summary>
        /// Distribuye los territorios entre los jugadores activos y actualiza la información de control.
        /// </summary>
        private void DistribuirTerritorios()
        {
            // Obtener IDs de todos los jugadores activos
            Lista<int> idsActivos = new Lista<int>();
            for (int i = 0; i < jugadores.getSize(); i++)
            {
                idsActivos.Agregar(jugadores[i].getId());
            }

            distribuidor.DistribuirTerritorios(idsActivos);
            ActualizarTerritoriosJugadores();
        }

        /// <summary>
        /// Actualiza la lista de territorios controlados por cada jugador
        /// </summary>
        private void ActualizarTerritoriosJugadores()
        {
            for (int i = 0; i < jugadores.getSize(); i++)
            {
                jugadores[i].setTerritoriosControlados(new Lista<Territorio>());
            }

            for (int i = 0; i < todosLosTerritorios.getSize(); i++)
            {
                Territorio territorio = todosLosTerritorios[i];
                int propietarioId = territorio.PropietarioId;

                for (int j = 0; j < jugadores.getSize(); j++)
                {
                    if (jugadores[j].getId() == propietarioId)
                    {
                        jugadores[j].getTerritoriosControlados().Agregar(territorio);
                        break;
                    }
                }
            }
        }

        public Lista<Territorio> getTerritorios() { return todosLosTerritorios; }
        public Lista<Continente> getContinentes() { return todosContinentes; }
        public Lista<Jugador> getJugadores() { return jugadores; }
        public Jugador GetJugador3() => jugador3;
    } 
}   