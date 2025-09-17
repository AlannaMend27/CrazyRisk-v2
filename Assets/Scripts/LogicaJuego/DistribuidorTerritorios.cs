using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CrazyRisk.Estructuras;
using CrazyRisk.Modelos;

namespace CrazyRisk.LogicaJuego
{
    public class DistribuidorTerritorios
    {
        private Lista<Territorio> todosLosTerritorios;
        private Random random;

        public DistribuidorTerritorios()
        {
            random = new Random();
            todosLosTerritorios = new Lista<Territorio>();
            InicializarTerritorios();
        }

        /// <summary>
        /// Inicializa los 42 territorios del mapa distribuidos en 6 continentes
        /// </summary>
        private void InicializarTerritorios()
        {
            int id = 0;

            // Asia (7 territorios) - Bonificación: +7 tropas
            todosLosTerritorios.Agregar(new Territorio(id++, "China", Continente.Asia));
            todosLosTerritorios.Agregar(new Territorio(id++, "India", Continente.Asia));
            todosLosTerritorios.Agregar(new Territorio(id++, "Japón", Continente.Asia));
            todosLosTerritorios.Agregar(new Territorio(id++, "Rusia", Continente.Asia));
            todosLosTerritorios.Agregar(new Territorio(id++, "Mongolia", Continente.Asia));
            todosLosTerritorios.Agregar(new Territorio(id++, "Corea", Continente.Asia));
            todosLosTerritorios.Agregar(new Territorio(id++, "Tailandia", Continente.Asia));

            // Europa (7 territorios) - Bonificación: +5 tropas
            todosLosTerritorios.Agregar(new Territorio(id++, "Francia", Continente.Europa));
            todosLosTerritorios.Agregar(new Territorio(id++, "Alemania", Continente.Europa));
            todosLosTerritorios.Agregar(new Territorio(id++, "España", Continente.Europa));
            todosLosTerritorios.Agregar(new Territorio(id++, "Italia", Continente.Europa));
            todosLosTerritorios.Agregar(new Territorio(id++, "Reino Unido", Continente.Europa));
            todosLosTerritorios.Agregar(new Territorio(id++, "Polonia", Continente.Europa));
            todosLosTerritorios.Agregar(new Territorio(id++, "Grecia", Continente.Europa));

            // América del Norte (7 territorios) - Bonificación: +5 tropas
            todosLosTerritorios.Agregar(new Territorio(id++, "Estados Unidos", Continente.AmericaNorte));
            todosLosTerritorios.Agregar(new Territorio(id++, "Canadá", Continente.AmericaNorte));
            todosLosTerritorios.Agregar(new Territorio(id++, "México", Continente.AmericaNorte));
            todosLosTerritorios.Agregar(new Territorio(id++, "Alaska", Continente.AmericaNorte));
            todosLosTerritorios.Agregar(new Territorio(id++, "Groenlandia", Continente.AmericaNorte));
            todosLosTerritorios.Agregar(new Territorio(id++, "Cuba", Continente.AmericaNorte));
            todosLosTerritorios.Agregar(new Territorio(id++, "Guatemala", Continente.AmericaNorte));

            // África (7 territorios) - Bonificación: +3 tropas
            todosLosTerritorios.Agregar(new Territorio(id++, "Egipto", Continente.Africa));
            todosLosTerritorios.Agregar(new Territorio(id++, "Sudáfrica", Continente.Africa));
            todosLosTerritorios.Agregar(new Territorio(id++, "Nigeria", Continente.Africa));
            todosLosTerritorios.Agregar(new Territorio(id++, "Kenia", Continente.Africa));
            todosLosTerritorios.Agregar(new Territorio(id++, "Marruecos", Continente.Africa));
            todosLosTerritorios.Agregar(new Territorio(id++, "Congo", Continente.Africa));
            todosLosTerritorios.Agregar(new Territorio(id++, "Madagascar", Continente.Africa));

            // América del Sur (7 territorios) - Bonificación: +2 tropas
            todosLosTerritorios.Agregar(new Territorio(id++, "Brasil", Continente.AmericaSur));
            todosLosTerritorios.Agregar(new Territorio(id++, "Argentina", Continente.AmericaSur));
            todosLosTerritorios.Agregar(new Territorio(id++, "Chile", Continente.AmericaSur));
            todosLosTerritorios.Agregar(new Territorio(id++, "Perú", Continente.AmericaSur));
            todosLosTerritorios.Agregar(new Territorio(id++, "Colombia", Continente.AmericaSur));
            todosLosTerritorios.Agregar(new Territorio(id++, "Venezuela", Continente.AmericaSur));
            todosLosTerritorios.Agregar(new Territorio(id++, "Uruguay", Continente.AmericaSur));

            // Oceanía (7 territorios) - Bonificación: +2 tropas
            todosLosTerritorios.Agregar(new Territorio(id++, "Australia", Continente.Oceania));
            todosLosTerritorios.Agregar(new Territorio(id++, "Nueva Zelanda", Continente.Oceania));
            todosLosTerritorios.Agregar(new Territorio(id++, "Indonesia", Continente.Oceania));
            todosLosTerritorios.Agregar(new Territorio(id++, "Filipinas", Continente.Oceania));
            todosLosTerritorios.Agregar(new Territorio(id++, "Papua Nueva Guinea", Continente.Oceania));
            todosLosTerritorios.Agregar(new Territorio(id++, "Fiji", Continente.Oceania));
            todosLosTerritorios.Agregar(new Territorio(id++, "Tahití", Continente.Oceania));

        }

        /// <summary>
        /// Distribuye los 42 territorios aleatoriamente entre 2 jugadores y el ejército neutral.
        /// Cada uno recibe exactamente 14 territorios.
        /// </summary>
        /// <param name="jugador1Id">ID del primer jugador</param>
        /// <param name="jugador2Id">ID del segundo jugador</param>
        /// <param name="neutralId">ID del ejército neutral</param>
        public void DistribuirTerritorios(string jugador1Id, string jugador2Id, string neutralId)
        {
            // Crear lista con 14 IDs de cada propietario
            Lista<string> propietarios = new Lista<string>();

            for (int i = 0; i < 14; i++)
            {
                propietarios.Agregar(jugador1Id);
                propietarios.Agregar(jugador2Id);
                propietarios.Agregar(neutralId);
            }

            // Mezclar aleatoriamente usando tu método Fisher-Yates
            propietarios.Mezclar(random);

            // Asignar territorios en orden aleatorio
            for (int i = 0; i < todosLosTerritorios.Tamaño; i++)
            {
                todosLosTerritorios[i].CantidadTropas = 1;
                todosLosTerritorios[i].PropietarioId = propietarios[i];

            }
        }

        /// <summary>
        /// Este es el metodo que selecciona el territorio (de momento es aleatorio pero se ebe lograr que el jugador lo escoja)
        /// </summary>
        private string SeleccionarTerritorioAleatorio(string jugadorId)
        {
            Lista<Territorio> territorios = ObtenerTerritoriosPorJugador(jugadorId);
            int indiceAleatorio = random.Next(territorios.Tamaño);
            return territorios[indiceAleatorio].Nombre;
        }

        /// <summary>
        /// Coloca las 26 tropas adicionales para cada jugador después de la distribución inicial
        /// </summary>
        /// <param name="jugador1Id">ID del primer jugador</param>
        /// <param name="jugador2Id">ID del segundo jugador</param>
        /// <param name="neutralId">ID del ejército neutral</param>
        public void ColocarTropasIniciales(string jugador1Id, string jugador2Id, string neutralId)
        {
            int tropasAdicionales = 78;

            // Distribucion de turnos = 0: jugador1, 1: jugador2, 2: neutral
            int turno = 0;

            while (tropasAdicionales != 0)
            {
                if (turno == 0)
                {
                    turno = 1;
                    string territorioElegido = SeleccionarTerritorioAleatorio(jugador1Id);
                    ColocarTropaEnTerritorio(jugador1Id, territorioElegido);
                }

                else if (turno == 1)
                {
                    turno = 2;
                    string territorioElegido = SeleccionarTerritorioAleatorio(jugador2Id);
                    ColocarTropaEnTerritorio(jugador2Id, territorioElegido);
                }
                else
                {
                    turno = 0;
                    ColocarTropasNeutral(neutralId);
                }
                tropasAdicionales--;
            }

            // Verificar que cada jugador tenga exactamente 40 tropas
            VerificarDistribucion(jugador1Id, jugador2Id, neutralId);
        }

        private void ColocarTropaEnTerritorio(string jugadorId, string nombreTerritorio)
        {
            Lista<Territorio> territoriosJugador = ObtenerTerritoriosPorJugador(jugadorId);

            // Buscar el territorio específico que el jugador eligió
            Territorio territorioElegido = null;
            for (int i = 0; i < territoriosJugador.Tamaño; i++)
            {
                if (territoriosJugador[i].Nombre == nombreTerritorio)
                {
                    territorioElegido = territoriosJugador[i];
                    break;
                }
            }

            if (territorioElegido == null)
            {
                throw new InvalidOperationException($"El jugador {jugadorId} no controla el territorio {nombreTerritorio}");
            }

            territorioElegido.CantidadTropas++;
            UnityEngine.Debug.Log($"Jugador {jugadorId} coloca 1 tropa en {territorioElegido.Nombre}");
        }

        /// <summary>
        /// Coloca tropas para el ejército neutral de manera completamente aleatoria
        /// </summary>
        /// <param name="neutralId">ID del ejército neutral</param>
        /// <param name="cantidadTropas">Cantidad de tropas a distribuir</param>
        private void ColocarTropasNeutral(string neutralId)
        {
            Lista<Territorio> territoriosNeutral = ObtenerTerritoriosPorJugador(neutralId);

            if (territoriosNeutral.Tamaño == 0)
            {
                throw new InvalidOperationException($"El ejército neutral no tiene territorios");
            }

            int indiceAleatorio = random.Next(territoriosNeutral.Tamaño);
            Territorio territorio = territoriosNeutral[indiceAleatorio];

            territorio.CantidadTropas++;
            UnityEngine.Debug.Log($"Ejército Neutral coloca 1 tropa en {territorio.Nombre} (ahora tiene {territorio.CantidadTropas} tropas)");


        }

        /// <summary>
        /// Obtiene todos los territorios que pertenecen a un jugador
        /// </summary>
        /// <param name="jugadorId">ID del jugador</param>
        /// <returns>Lista de territorios del jugador</returns>
        private Lista<Territorio> ObtenerTerritoriosPorJugador(string jugadorId)
        {
            Lista<Territorio> territoriosJugador = new Lista<Territorio>();

            for (int i = 0; i < todosLosTerritorios.Tamaño; i++)
            {
                if (todosLosTerritorios[i].PropietarioId == jugadorId)
                {
                    territoriosJugador.Agregar(todosLosTerritorios[i]);
                }
            }

            return territoriosJugador;
        }

        /// <summary>
        /// Cuenta el total de tropas de un jugador
        /// </summary>
        private int ContarTropasJugador(string jugadorId)
        {
            int total = 0;
            Lista<Territorio> territorios = ObtenerTerritoriosPorJugador(jugadorId);

            for (int i = 0; i < territorios.Tamaño; i++)
            {
                total += territorios[i].CantidadTropas;
            }

            return total;
        }

        /// <summary>
        /// Método público para obtener territorios de un jugador (para uso externo)
        /// </summary>
        public Lista<Territorio> ObtenerTerritoriosDeJugador(string jugadorId)
        {
            return ObtenerTerritoriosPorJugador(jugadorId);
        }

        public Lista<Territorio> ObtenerTodosLosTerritorios()
        {
            return todosLosTerritorios;
        }

        /// <summary>
        /// Verifica que la distribución final sea correcta (40 tropas por jugador)
        /// </summary>
        private void VerificarDistribucion(string jugador1Id, string jugador2Id, string neutralId)
        {
            int tropasJ1 = ContarTropasJugador(jugador1Id);
            int tropasJ2 = ContarTropasJugador(jugador2Id);
            int tropasNeutral = ContarTropasJugador(neutralId);

            if (tropasJ1 != 40 || tropasJ2 != 40 || tropasNeutral != 40)
            {
                throw new InvalidOperationException(
                    $"Error en distribución: J1={tropasJ1}, J2={tropasJ2}, Neutral={tropasNeutral}. " +
                    $"Todos deberían tener 40 tropas.");
            }

            UnityEngine.Debug.Log($"✓ Verificación exitosa: J1={tropasJ1}, J2={tropasJ2}, Neutral={tropasNeutral} tropas");
        }

    }
}