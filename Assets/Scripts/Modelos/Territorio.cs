using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CrazyRisk.Estructuras;

namespace CrazyRisk.Modelos
{
    public class Territorio
    {
        /// <summary>
        /// Representa un territorio en el juego Crazy Risk.
        /// Cada territorio pertenece a un continente, tiene un propietario y cantidad de tropas.
        /// </summary>
        public int Id { get; set; }
        public string Nombre { get; set; }
        public Continente Continente { get; set; }
        public string PropietarioId { get; set; }
        public int CantidadTropas { get; set; }
        public Lista<int> TerritoriosAdyacentes { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="id">Identificador único</param>
        /// <param name="nombre">Nombre del territorio</param>
        /// <param name="continente">Continente al que pertenece</param>
        /// <param name="propietarioId">ID del propietario inicial (null si no se asigna aún)</param>
        /// <param name="tropasIniciales">Cantidad inicial de tropas (por defecto 1)</param>
        public Territorio(int id, string nombre, Continente continente, string propietarioId = null, int tropasIniciales = 1)
        {
            Id = id;
            Nombre = nombre;
            Continente = continente;
            PropietarioId = propietarioId;
            CantidadTropas = tropasIniciales;
            TerritoriosAdyacentes = new Lista<int>();
        }

        /// <summary>
        /// Agrega un territorio adyacente al territorio seleccionado
        /// </summary>
        /// <param name="territorioId">ID del territorio adyacente</param>
        public void AgregarAdyacente(int territorioId)
        {
            if (!TerritoriosAdyacentes.Contiene(territorioId))
            {
                TerritoriosAdyacentes.Agregar(territorioId);
            }
        }

        /// <summary>
        /// Verifica si este territorio es adyacente a otro
        /// </summary>
        /// <param name="territorioId">ID del territorio a verificar</param>
        /// <returns>true si son adyacentes, false en caso contrario</returns>
        public bool EsAdyacenteA(int territorioId)
        {
            return TerritoriosAdyacentes.Contiene(territorioId);
        }

        /// <summary>
        /// Agrega tropas al territorio
        /// </summary>
        /// <param name="cantidad">Cantidad de tropas a agregar</param>
        /// <exception cref="ArgumentException">Si la cantidad es negativa</exception>
        public void AgregarTropas(int cantidad)
        {
            if (cantidad < 0)
                throw new ArgumentException("No se pueden agregar tropas negativas");

            CantidadTropas += cantidad;
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

            int tropasDisponibles = CantidadTropas - 1;
            int tropasARemover = Math.Min(cantidad, tropasDisponibles);

            CantidadTropas -= tropasARemover;

            return tropasARemover;
        }


        /// <summary>
        /// Verifica si el territorio puede atacar (tiene al menos 2 tropas)
        /// </summary>
        /// <returns>true si puede atacar, false en caso contrario</returns>
        public bool PuedeAtacar()
        {
            if (CantidadTropas >= 2)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Calcula el máximo número de dados que puede usar para atacar
        /// </summary>
        /// <returns>Número de dados (1-3) que puede usar</returns>
        public int MaximoDadosAtaque()
        {
            if (CantidadTropas < 2) return 0;
            if (CantidadTropas == 2) return 1;
            if (CantidadTropas == 3) return 2;
            return 3; // 4 o más tropas = 3 dados máximo
        }

        /// <summary>
        /// Cambia el propietario del territorio (conquista)
        /// </summary>
        /// <param name="nuevoPropietarioId">ID del nuevo propietario</param>
        /// <param name="tropasConquistadoras">Tropas que ocuparán el territorio</param>
        public void CambiarPropietario(string nuevoPropietarioId, int tropasConquistadoras)
        {
            PropietarioId = nuevoPropietarioId;
            CantidadTropas = tropasConquistadoras;
        }

        /// <summary>
        /// Verifica si el territorio pertenece al jugador especificado
        /// </summary>
        /// <param name="jugadorId">ID del jugador a verificar</param>
        /// <returns>true si el territorio pertenece al jugador</returns>
        public bool PerteneceA(string jugadorId)
        {
            return PropietarioId == jugadorId;
        }

        /// <summary>
        /// Obtiene información básica del territorio para realizar pruebas
        /// </summary>
        /// <returns>Representación en cadena del territorio</returns>
        public override string ToString()
        {
            return $"{Nombre} ({Continente}) - {PropietarioId}: {CantidadTropas} tropas";
        }
















    }
}