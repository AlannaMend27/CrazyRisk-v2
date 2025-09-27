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

        public DistribuidorTerritorios(Lista<Territorio> territorios)
        {
            random = new Random();
            todosLosTerritorios = territorios;
        }

        /// <summary>
        /// Distribuye los 42 territorios aleatoriamente entre 2 jugadores y el ejército neutral.
        /// Cada uno recibe exactamente 14 territorios.
        /// </summary>
        /// <param name="jugador1Id">ID del primer jugador</param>
        /// <param name="jugador2Id">ID del segundo jugador</param>
        /// <param name="neutralId">ID del ejército neutral</param>
        public void DistribuirTerritorios(int jugador1Id, int jugador2Id, int neutralId)
        {
            // Crear lista con 14 IDs de cada propietario
            Lista<int> propietarios = new Lista<int>();

            for (int i = 0; i < 14; i++)
            {
                propietarios.Agregar(jugador1Id);
                propietarios.Agregar(jugador2Id);
                propietarios.Agregar(neutralId);
            }

            // Mezclar aleatoriamente usando tu método Fisher-Yates
            propietarios.Mezclar(random);

            // Asignar territorios en orden aleatorio
            for (int i = 0; i < todosLosTerritorios.getSize(); i++)
            {
                todosLosTerritorios[i].CantidadTropas = 1;
                todosLosTerritorios[i].PropietarioId = propietarios[i];

            }
        }

        /// <summary>
        /// Este es el metodo que selecciona el territorio (de momento es aleatorio pero se ebe lograr que el jugador lo escoja)
        /// </summary>
        private string SeleccionarTerritorioAleatorio(int jugadorId)
        {
            Lista<Territorio> territorios = ObtenerTerritoriosPorJugador(jugadorId);
            int indiceAleatorio = random.Next(territorios.getSize());
            return territorios[indiceAleatorio].Nombre;
        }

        /// <summary>
        /// Coloca las 26 tropas adicionales para cada jugador después de la distribución inicial
        /// </summary>
        /// <param name="jugador1Id">ID del primer jugador</param>
        /// <param name="jugador2Id">ID del segundo jugador</param>
        /// <param name="neutralId">ID del ejército neutral</param>
        public void ColocarTropasIniciales(int jugador1Id, int jugador2Id, int neutralId)
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

        private void ColocarTropaEnTerritorio(int jugadorId, string nombreTerritorio)
        {
            Lista<Territorio> territoriosJugador = ObtenerTerritoriosPorJugador(jugadorId);

            // Buscar el territorio específico que el jugador eligió
            Territorio territorioElegido = null;
            for (int i = 0; i < territoriosJugador.getSize(); i++)
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
   
        private void ColocarTropasNeutral(int neutralId)
        {
            Lista<Territorio> territoriosNeutral = ObtenerTerritoriosPorJugador(neutralId);

            if (territoriosNeutral.getSize() == 0)
            {
                throw new InvalidOperationException($"El ejército neutral no tiene territorios");
            }

            int indiceAleatorio = random.Next(territoriosNeutral.getSize());
            Territorio territorio = territoriosNeutral[indiceAleatorio];

            territorio.CantidadTropas++;
            UnityEngine.Debug.Log($"Ejército Neutral coloca 1 tropa en {territorio.Nombre} (ahora tiene {territorio.CantidadTropas} tropas)");


        }

        /// <summary>
        /// Obtiene todos los territorios que pertenecen a un jugador
        /// </summary>
        /// <param name="jugadorId">ID del jugador</param>
        /// <returns>Lista de territorios del jugador</returns>
        private Lista<Territorio> ObtenerTerritoriosPorJugador(int jugadorId)
        {
            Lista<Territorio> territoriosJugador = new Lista<Territorio>();

            for (int i = 0; i < todosLosTerritorios.getSize(); i++)
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
        private int ContarTropasJugador(int jugadorId)
        {
            int total = 0;
            Lista<Territorio> territorios = ObtenerTerritoriosPorJugador(jugadorId);

            for (int i = 0; i < territorios.getSize(); i++)
            {
                total += territorios[i].CantidadTropas;
            }

            return total;
        }

        /// <summary>
        /// Método público para obtener territorios de un jugador (para uso externo)
        /// </summary>
        public Lista<Territorio> ObtenerTerritoriosDeJugador(int jugadorId)
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
        private void VerificarDistribucion(int jugador1Id, int jugador2Id, int neutralId)
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