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
}