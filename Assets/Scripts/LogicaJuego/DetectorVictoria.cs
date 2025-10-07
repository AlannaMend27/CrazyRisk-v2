using UnityEngine;
using CrazyRisk.Estructuras;
using CrazyRisk.Modelos;

namespace CrazyRisk.LogicaJuego
{
    public class DetectorVictoria
    {
        private const int TOTAL_TERRITORIOS = 42;

        public bool VerificarVictoria(Jugador jugador)
        {
            return jugador.HaGanado(TOTAL_TERRITORIOS);
        }

        public bool VerificarDerrota(Jugador jugador)
        {
            return jugador.HaPerdido();
        }

        public Jugador BuscarGanador(Lista<Jugador> jugadores)
        {
            for (int i = 0; i < jugadores.getSize(); i++)
            {
                Jugador jugador = jugadores.Obtener(i);

                // NUEVO: El neutral no puede ganar
                if (jugador.getEsNeutral())
                    continue;

                if (VerificarVictoria(jugador))
                {
                    Debug.Log($"¡{jugador.getNombre()} ha ganado la partida!");
                    return jugador;
                }
            }

            return null;
        }

        public int ContarJugadoresActivos(Lista<Jugador> jugadores)
        {
            int activos = 0;

            for (int i = 0; i < jugadores.getSize(); i++)
            {
                Jugador jugador = jugadores.Obtener(i);

                // El neutral no cuenta como jugador activo para condición de victoria
                if (jugador.getEsNeutral())
                    continue;

                if (!VerificarDerrota(jugador))
                {
                    activos++;
                }
            }

            return activos;
        }

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

    public class EstadoPartida
    {
        public bool juegoTerminado;
        public Jugador ganador;
        public int jugadoresActivos;

        public override string ToString()
        {
            if (juegoTerminado && ganador != null)
            {
                return $"¡Partida terminada! Ganador: {ganador.getNombre()}";
            }

            return $"Partida en curso - Jugadores activos: {jugadoresActivos}";
        }
    }
}
