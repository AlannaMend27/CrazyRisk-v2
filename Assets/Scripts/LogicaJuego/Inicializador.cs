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
        // Propiedades del juego
        private Lista<Territorio> todosLosTerritorios;
        private Lista<Continente> todosContinentes;
        private Lista<Jugador> jugadores;
        private Jugador jugador1;
        private Jugador jugador2;
        private Jugador jugadorNeutral;
        private Random random;

        public InicializadorJuego()
        {
            todosLosTerritorios = new Lista<Territorio>();
            todosContinentes = new Lista<Continente>();
            jugadores = new Lista<Jugador>();
            random = new Random();
        }

        /// <summary>
        /// Inicializa todo el juego: territorios, continentes, jugadores y adyacencias
        /// </summary>
        public void InicializarJuegoCompleto(string nombreJugador1, string colorJugador1, string nombreJugador2, string colorJugador2)
        {
            // 1. Crear continentes
            InicializarContinentes();

            // 2. Crear todos los territorios
            InicializarTerritorios();

            // 3. Configurar adyacencias entre territorios
            ConfigurarAdyacencias();

            // 4. Crear jugadores
            InicializarJugadores(nombreJugador1, colorJugador1, nombreJugador2, colorJugador2);

            // 5. Distribuir territorios entre jugadores
            DistribuirTerritorios();

            // 6. Colocar tropas iniciales
            ColocarTropasIniciales();
        }

        /// <summary>
        /// Crea los 6 continentes del juego con las bonificaciones correctas
        /// </summary>
        private void InicializarContinentes()
        {
            todosContinentes.Agregar(new Continente("North America", 9));  // 9 territorios
            todosContinentes.Agregar(new Continente("South America", 4));  // 4 territorios
            todosContinentes.Agregar(new Continente("Europe", 7));         // 7 territorios
            todosContinentes.Agregar(new Continente("Africa", 6));         // 6 territorios
            todosContinentes.Agregar(new Continente("Asia", 12));           // 12 territorios
            todosContinentes.Agregar(new Continente("Oceania", 4));        // 4 territorios
        }

        /// <summary>
        /// Crea los 42 territorios del juego usando los territorios del repositorio Conquest
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
        /// Crea un territorio y lo agrega a la lista
        /// </summary>
        private void CrearTerritorio(int id, string nombre, string continente)
        {
            Territorio nuevoTerritorio = new Territorio(id, nombre, continente);
            todosLosTerritorios.Agregar(nuevoTerritorio);
        }

        /// <summary>
        /// Configura las adyacencias entre todos los territorios según el mapa clásico de Risk
        /// </summary>
        private void ConfigurarAdyacencias()
        {
            // North America
            ConfigurarAdyacencia(1, new int[] { 2, 4, 30 }); // Alaska -> Northwest Territory, Alberta, Kamchatka
            ConfigurarAdyacencia(2, new int[] { 1, 3, 4, 5 }); // Northwest Territory -> Alaska, Greenland, Alberta, Ontario
            ConfigurarAdyacencia(3, new int[] { 2, 5, 6, 14 }); // Greenland -> Northwest Territory, Ontario, Quebec, Iceland
            ConfigurarAdyacencia(4, new int[] { 1, 2, 7, 5 }); // Alberta -> Alaska, Northwest Territory, Western US, Ontario
            ConfigurarAdyacencia(5, new int[] { 2, 3, 4, 6, 7, 8 }); // Ontario -> Northwest Territory, Greenland, Alberta, Quebec, Western US, Eastern US
            ConfigurarAdyacencia(6, new int[] { 3, 5, 8 }); // Quebec -> Greenland, Ontario, Eastern US
            ConfigurarAdyacencia(7, new int[] { 4, 5, 8, 9 }); // Western United States -> Alberta, Ontario, Eastern US, Central America
            ConfigurarAdyacencia(8, new int[] { 5, 6, 7, 9 }); // Eastern United States -> Ontario, Quebec, Western US, Central America
            ConfigurarAdyacencia(9, new int[] { 7, 8, 10 }); // Central America -> Western US, Eastern US, Venezuela

            // South America
            ConfigurarAdyacencia(10, new int[] { 9, 11, 12 }); // Venezuela -> Central America, Brazil, Peru
            ConfigurarAdyacencia(11, new int[] { 10, 12, 13, 21 }); // Brazil -> Venezuela, Peru, Argentina, North Africa
            ConfigurarAdyacencia(12, new int[] { 10, 11, 13 }); // Peru -> Venezuela, Brazil, Argentina
            ConfigurarAdyacencia(13, new int[] { 11, 12 }); // Argentina -> Brazil, Peru

            // Europe
            ConfigurarAdyacencia(14, new int[] { 3, 15, 16 }); // Iceland -> Greenland, Great Britain, Scandinavia
            ConfigurarAdyacencia(15, new int[] { 14, 16, 17, 18 }); // Great Britain -> Iceland, Scandinavia, Northern Europe, Western Europe
            ConfigurarAdyacencia(16, new int[] { 14, 15, 17, 20 }); // Scandinavia -> Iceland, Great Britain, Northern Europe, Ukraine
            ConfigurarAdyacencia(17, new int[] { 15, 16, 18, 19, 20 }); // Northern Europe -> Great Britain, Scandinavia, Western Europe, Southern Europe, Ukraine
            ConfigurarAdyacencia(18, new int[] { 15, 17, 19, 21 }); // Western Europe -> Great Britain, Northern Europe, Southern Europe, North Africa
            ConfigurarAdyacencia(19, new int[] { 17, 18, 20, 21, 22, 37 }); // Southern Europe -> Northern Europe, Western Europe, Ukraine, North Africa, Egypt, Middle East
            ConfigurarAdyacencia(20, new int[] { 16, 17, 19, 27, 37, 38 }); // Ukraine -> Scandinavia, Northern Europe, Southern Europe, Ural, Middle East, Afghanistan

            // Africa
            ConfigurarAdyacencia(21, new int[] { 11, 18, 19, 22, 23, 24 }); // North Africa -> Brazil, Western Europe, Southern Europe, Egypt, East Africa, Congo
            ConfigurarAdyacencia(22, new int[] { 19, 21, 23, 37 }); // Egypt -> Southern Europe, North Africa, East Africa, Middle East
            ConfigurarAdyacencia(23, new int[] { 21, 22, 24, 25, 26, 37 }); // East Africa -> North Africa, Egypt, Congo, South Africa, Madagascar, Middle East
            ConfigurarAdyacencia(24, new int[] { 21, 23, 25 }); // Congo -> North Africa, East Africa, South Africa
            ConfigurarAdyacencia(25, new int[] { 23, 24, 26 }); // South Africa -> East Africa, Congo, Madagascar
            ConfigurarAdyacencia(26, new int[] { 23, 25 }); // Madagascar -> East Africa, South Africa

            // Asia
            ConfigurarAdyacencia(27, new int[] { 20, 28, 34, 38 }); // Ural -> Ukraine, Siberia, China, Afghanistan
            ConfigurarAdyacencia(28, new int[] { 27, 29, 31, 32, 34 }); // Siberia -> Ural, Yakutsk, Irkutsk, Mongolia, China
            ConfigurarAdyacencia(29, new int[] { 28, 30, 31 }); // Yakutsk -> Siberia, Kamchatka, Irkutsk
            ConfigurarAdyacencia(30, new int[] { 1, 29, 31, 32, 33 }); // Kamchatka -> Alaska, Yakutsk, Irkutsk, Mongolia, Japan
            ConfigurarAdyacencia(31, new int[] { 28, 29, 30, 32 }); // Irkutsk -> Siberia, Yakutsk, Kamchatka, Mongolia
            ConfigurarAdyacencia(32, new int[] { 28, 30, 31, 33, 34 }); // Mongolia -> Siberia, Kamchatka, Irkutsk, Japan, China
            ConfigurarAdyacencia(33, new int[] { 30, 32 }); // Japan -> Kamchatka, Mongolia
            ConfigurarAdyacencia(34, new int[] { 27, 28, 32, 35, 36, 38 }); // China -> Ural, Siberia, Mongolia, India, Siam, Afghanistan
            ConfigurarAdyacencia(35, new int[] { 34, 36, 37, 38 }); // India -> China, Siam, Middle East, Afghanistan
            ConfigurarAdyacencia(36, new int[] { 34, 35, 39 }); // Siam -> China, India, Indonesia
            ConfigurarAdyacencia(37, new int[] { 19, 20, 22, 23, 35, 38 }); // Middle East -> Southern Europe, Ukraine, Egypt, East Africa, India, Afghanistan
            ConfigurarAdyacencia(38, new int[] { 20, 27, 34, 35, 37 }); // Afghanistan -> Ukraine, Ural, China, India, Middle East

            // Oceania
            ConfigurarAdyacencia(39, new int[] { 36, 40, 41 }); // Indonesia -> Siam, New Guinea, Western Australia
            ConfigurarAdyacencia(40, new int[] { 39, 41, 42 }); // New Guinea -> Indonesia, Western Australia, Eastern Australia
            ConfigurarAdyacencia(41, new int[] { 39, 40, 42 }); // Western Australia -> Indonesia, New Guinea, Eastern Australia
            ConfigurarAdyacencia(42, new int[] { 40, 41 }); // Eastern Australia -> New Guinea, Western Australia
        }

        /// <summary>
        /// Configura adyacencias bidireccionales para un territorio
        /// </summary>
        private void ConfigurarAdyacencia(int territorioId, int[] adyacentes)
        {
            Territorio territorio = BuscarTerritorioPorId(territorioId);
            if (territorio != null)
            {
                for (int i = 0; i < adyacentes.Length; i++)
                {
                    territorio.AgregarAdyacente(adyacentes[i]);

                    // Configurar adyacencia bidireccional
                    Territorio territorioAdyacente = BuscarTerritorioPorId(adyacentes[i]);
                    if (territorioAdyacente != null)
                    {
                        territorioAdyacente.AgregarAdyacente(territorioId);
                    }
                }
            }
        }

        /// <summary>
        /// Busca un territorio por su ID
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
        /// Crea los jugadores del juego
        /// </summary>
        private void InicializarJugadores(string nombre1, string color1, string nombre2, string color2)
        {
            jugador1 = new Jugador(1, nombre1, color1);
            jugador2 = new Jugador(2, nombre2, color2);
            jugadorNeutral = Jugador.CrearJugadorNeutral(3, "Gris");

            jugadores.Agregar(jugador1);
            jugadores.Agregar(jugador2);
            jugadores.Agregar(jugadorNeutral);
        }

        /// <summary>
        /// Distribuye territorios entre jugadores usando DistribuidorTerritorios
        /// </summary>
        private void DistribuirTerritorios()
        {
            DistribuidorTerritorios distribuidor = new DistribuidorTerritorios(todosLosTerritorios);

            // Distribuir los 42 territorios (14 por jugador)
            distribuidor.DistribuirTerritorios(jugador1.getId(), jugador2.getId(), jugadorNeutral.getId());

            // Actualizar las listas de territorios de cada jugador
            ActualizarTerritoriosJugadores();
        }

        /// <summary>
        /// Coloca tropas iniciales usando DistribuidorTerritorios  
        /// </summary>
        private void ColocarTropasIniciales()
        {
            DistribuidorTerritorios distribuidor = new DistribuidorTerritorios(todosLosTerritorios);

            // Colocar las 78 tropas adicionales (26 por jugador)
            distribuidor.ColocarTropasIniciales(jugador1.getId(), jugador2.getId(), jugadorNeutral.getId());

            // Actualizar las listas de territorios después de colocar tropas
            ActualizarTerritoriosJugadores();
        }

        /// <summary>
        /// Actualiza las listas de territorios controlados por cada jugador
        /// </summary>
        private void ActualizarTerritoriosJugadores()
        {
            // Limpiar listas actuales
            for (int i = 0; i < jugadores.getSize(); i++)
            {
                jugadores[i].setTerritoriosControlados(new Lista<Territorio>());
            }

            // Reagrupar territorios por propietario
            for (int i = 0; i < todosLosTerritorios.getSize(); i++)
            {
                Territorio territorio = todosLosTerritorios[i];
                int propietarioId = territorio.PropietarioId;

                // Buscar jugador correspondiente y agregar territorio
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

        // Métodos públicos para obtener datos del juego
        public Lista<Territorio> getTerritorios() { return todosLosTerritorios; }
        public Lista<Continente> getContinentes() { return todosContinentes; }
        public Lista<Jugador> getJugadores() { return jugadores; }
    }
}
