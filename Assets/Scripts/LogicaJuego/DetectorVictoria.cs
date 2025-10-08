using UnityEngine;
using CrazyRisk.Estructuras;
using CrazyRisk.Modelos;

namespace CrazyRisk.LogicaJuego
{
    /// <summary>
    /// Proporciona métodos para verificar condiciones de victoria, derrota y estado de la partida.
    /// </summary>
    public class DetectorVictoria
    {
        private const int TOTAL_TERRITORIOS = 42;

        /// <summary>
        /// Verifica si el jugador ha cumplido la condición de victoria.
        /// </summary>
        public bool VerificarVictoria(Jugador jugador)
        {
            return jugador.HaGanado(TOTAL_TERRITORIOS);
        }

        /// <summary>
        /// Verifica si el jugador ha sido derrotado.
        /// </summary>
        public bool VerificarDerrota(Jugador jugador)
        {
            return jugador.HaPerdido();
        }

        /// <summary>
        /// Busca y retorna el jugador ganador, si existe, ignorando jugadores neutrales.
        /// </summary>
        public Jugador BuscarGanador(Lista<Jugador> jugadores)
        {
            for (int i = 0; i < jugadores.getSize(); i++)
            {
                Jugador jugador = jugadores.Obtener(i);

                // no permite que el jugador neutral gane
                if (jugador.getEsNeutral())
                    continue;

                //verifica el jugador que ha ganado
                if (VerificarVictoria(jugador))
                {
                    Debug.Log($"�{jugador.getNombre()} ha ganado la partida!");
                    return jugador;
                }
            }

            return null;
        }

        /// <summary>
        /// Cuenta la cantidad de jugadores activos que no han sido derrotados, excluyendo neutrales.
        /// </summary>
        public int ContarJugadoresActivos(Lista<Jugador> jugadores)
        {
            int activos = 0;

            for (int i = 0; i < jugadores.getSize(); i++)
            {
                Jugador jugador = jugadores.Obtener(i);

                // se excluye al neutral
                if (jugador.getEsNeutral())
                    continue;

                if (!VerificarDerrota(jugador))
                {
                    activos++;
                }
            }

            return activos;
        }

        /// <summary>
        /// Verifica el estado actual de la partida y retorna información sobre el ganador y jugadores activos.
        /// </summary>
        public EstadoPartida VerificarEstadoPartida(Lista<Jugador> jugadores)
        {
            Jugador ganador = BuscarGanador(jugadores);

            if (ganador != null)
            {
                return new EstadoPartida
                {
                    juegoTerminado = true,
                    ganador = ganador,
                    jugadoresActivos = 1
                };
            }

            int activos = ContarJugadoresActivos(jugadores);

            return new EstadoPartida
            {
                juegoTerminado = false,
                ganador = null,
                jugadoresActivos = activos
            };
        }
    }

    /// <summary>
    /// Representa el estado de la partida, incluyendo si terminó, el ganador y la cantidad de jugadores activos.
    /// </summary>
    public class EstadoPartida
    {
        public bool juegoTerminado;
        public Jugador ganador;
        public int jugadoresActivos;

        /// <summary>
        /// Devuelve una representación en texto del estado de la partida.
        /// </summary>
        public override string ToString()
        {
            if (juegoTerminado && ganador != null)
            {
                return $"�Partida terminada! Ganador: {ganador.getNombre()}";
            }

            return $"Partida en curso - Jugadores activos: {jugadoresActivos}";
        }
    }
}
