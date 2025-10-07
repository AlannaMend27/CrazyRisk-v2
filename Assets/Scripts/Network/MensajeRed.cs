using System;
using Newtonsoft.Json;

namespace CrazyRisk.Red
{
    [Serializable]
    public class MensajeRed
    {
        public string tipo;
        public string datos;
        public int jugadorId;
        public DateTime timestamp = DateTime.Now;
    }

    [Serializable]
    public class EstadoJuego
    {
        public int[] territoriosPropietarios;
        public int[] territoriosTropas;
        public int turnoActual;
        public int cantidadJugadores;
        public string[] nombresJugadores;
        public string faseActual;
        public int refuerzosDisponibles;
        public bool enFasePreparacion;
    }

    [Serializable]
    public class DatosJugador
    {
        public string nombre;
        public int id;
        public string color;

        public DatosJugador(string n, int i, string c)
        {
            nombre = n;
            id = i;
            color = c;
        }
    }

    [Serializable]
    public class AccionJuego
    {
        public string tipo;
        public string territorioOrigen;
        public string territorioDestino;
        public int cantidad;
        public int jugadorId;
        public int dadosAtacante;
        public int dadosDefensor;
    }
}