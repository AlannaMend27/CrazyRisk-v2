using System;

namespace CrazyRisk.Modelos
{
    [Serializable]
    public enum TipoTarjeta
    {
        Infanteria,
        Caballeria,
        Artilleria
    }

    [Serializable]
    public class Tarjeta
    {
        private TipoTarjeta tipo;
        private string territorioAsociado;
        private bool fueUsada;

        public Tarjeta(TipoTarjeta tipo, string territorio)
        {
            this.tipo = tipo;
            this.territorioAsociado = territorio;
            this.fueUsada = false;
        }

        // Getters y setters
        public TipoTarjeta GetTipo() => tipo;
        public string GetTerritorio() => territorioAsociado;
        public bool FueUsada() => fueUsada;
        public void MarcarComoUsada() => fueUsada = true;

        public override string ToString()
        {
            return $"{tipo} - {territorioAsociado}";
        }

        /// <summary>
        /// Crea una tarjeta aleatoria asignando un tipo al azar y asociándola al territorio especificado.
        /// </summary>
        public static Tarjeta CrearTarjetaAleatoria(string territorio)
        {
            Random random = new Random();
            Array valores = Enum.GetValues(typeof(TipoTarjeta));
            TipoTarjeta tipoAleatorio = (TipoTarjeta)valores.GetValue(random.Next(valores.Length));

            return new Tarjeta(tipoAleatorio, territorio);
        }
    }
}