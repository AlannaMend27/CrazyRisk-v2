using System;
using CrazyRisk.Estructuras;

namespace CrazyRisk.Modelos
{
    public class Territorio
    {
        /// <summary>
        /// Representa un territorio en el juego Crazy Risk.
        /// Cada territorio pertenece a un continente, tiene un propietario y cantidad de tropas.
        /// </summary>
        private int id;
        private string nombre;
        private string continente; 
        private int propietarioId;
        private int cantidadTropas;
        private Lista<int> territoriosAdyacentes;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="id">Identificador único</param>
        /// <param name="nombre">Nombre del territorio</param>
        /// <param name="continente">Nombre del continente al que pertenece</param>
        /// <param name="propietarioId">ID del propietario inicial (null si no se asigna aún)</param>
        /// <param name="tropasIniciales">Cantidad inicial de tropas (por defecto 1)</param>
        public Territorio(int id, string nombre, string continente, int propietarioId = 0, int tropasIniciales = 1)
        {
            this.id = id;
            this.nombre = nombre;
            this.continente = continente;
            this.propietarioId = propietarioId;
            this.cantidadTropas = tropasIniciales;
            this.territoriosAdyacentes = new Lista<int>();
        }

        // Getters y setters
        public int ObtenerID()
        {
            return id;
        }

        public void EstablecerID(int nuevoId)
        {
            id = nuevoId;
        }

        public string ObtenerNombre()
        {
            return nombre;
        }

        public void EstablecerNombre(string nuevoNombre)
        {
            nombre = nuevoNombre;
        }

        public string ObtenerContinente()
        {
            return continente;
        }

        public void EstablecerContinente(string nuevoContinente)
        {
            continente = nuevoContinente;
        }

        public int ObtenerPropietarioId()
        {
            return propietarioId;
        }

        public void EstablecerPropietarioId(int nuevoPropietario)
        {
            propietarioId = nuevoPropietario;
        }

        public int ObtenerCantidadTropas()
        {
            return cantidadTropas;
        }

        public void EstablecerCantidadTropas(int nuevaCantidad)
        {
            cantidadTropas = nuevaCantidad;
        }

        public Lista<int> ObtenerTerritoriosAdyacentes()
        {
            return territoriosAdyacentes;
        }

        public string getContinente()
        {
            return continente;
        }

        // Propiedades públicas
        public int Id
        {
            get { return id; }
            set { id = value; }
        }

        public string Nombre
        {
            get { return nombre; }
            set { nombre = value; }
        }

        public int PropietarioId
        {
            get { return propietarioId; }
            set { propietarioId = value; }
        }

        public int CantidadTropas
        {
            get { return cantidadTropas; }
            set { cantidadTropas = value; }
        }

        /// <summary>
        /// Agrega un territorio adyacente al territorio seleccionado
        /// </summary>
        /// <param name="territorioId">ID del territorio adyacente</param>
        public void AgregarAdyacente(int territorioId)
        {
            if (!territoriosAdyacentes.Contiene(territorioId))
            {
                territoriosAdyacentes.Agregar(territorioId);
            }
        }

        /// <summary>
        /// Verifica si este territorio es adyacente a otro
        /// </summary>
        /// <param name="territorioId">ID del territorio a verificar</param>
        /// <returns>true si son adyacentes, false en caso contrario</returns>
        public bool EsAdyacenteA(int territorioId)
        {
            return territoriosAdyacentes.Contiene(territorioId);
        }

        /// <summary>
        /// Agrega tropas al territorio
        /// </summary>
        /// <param name="cantidad">Cantidad de tropas a agregar</param>
        public void AgregarTropas(int cantidad)
        {
            if (cantidad < 0)
                throw new ArgumentException("No se pueden agregar tropas negativas");

            cantidadTropas += cantidad;
        }

        /// <summary>
        /// Remueve tropas del territorio, manteniendo mínimo 1 tropa
        /// </summary>
        /// <param name="cantidad">Cantidad de tropas a remover</param>
        /// <returns>Cantidad real de tropas removidas</returns>
        public int RemoverTropas(int cantidad)
        {
            if (cantidad <= 0)
                return 0;

            int tropasDisponibles = cantidadTropas - 1;
            int tropasARemover = Math.Min(cantidad, tropasDisponibles);

            cantidadTropas -= tropasARemover;

            return tropasARemover;
        }

        /// <summary>
        /// Verifica si el territorio puede atacar (tiene al menos 2 tropas)
        /// </summary>
        /// <returns>true si puede atacar, false en caso contrario</returns>
        public bool PuedeAtacar()
        {
            return cantidadTropas >= 2;
        }

        /// <summary>
        /// Calcula el máximo número de dados que puede usar para atacar
        /// </summary>
        /// <returns>Número de dados (1-3) que puede usar</returns>
        public int MaximoDadosAtaque()
        {
            if (cantidadTropas < 2)
            {
                return 0;
            }
            if (cantidadTropas == 2)
            {
                return 1;   
            }
            if (cantidadTropas == 3)
            {
                return 2;
            }
            else
            {
                return 3;
            }
        }

        /// <summary>
        /// Cambia el propietario del territorio (conquista)
        /// </summary>
        /// <param name="nuevoPropietarioId">ID del nuevo propietario</param>
        /// <param name="tropasConquistadoras">Tropas que ocuparán el territorio</param>
        public void CambiarPropietario(int nuevoPropietarioId, int tropasConquistadoras)
        {
            propietarioId = nuevoPropietarioId;
            cantidadTropas = tropasConquistadoras;
        }

        /// <summary>
        /// Verifica si el territorio pertenece al jugador especificado
        /// </summary>
        /// <param name="jugadorId">ID del jugador a verificar</param>
        /// <returns>true si el territorio pertenece al jugador</returns>
        public bool PerteneceA(int jugadorId)
        {
            return propietarioId == jugadorId;
        }

        public override string ToString()
        {
            return $"{nombre} ({continente}) - {propietarioId}: {cantidadTropas} tropas";
        }
    }
}