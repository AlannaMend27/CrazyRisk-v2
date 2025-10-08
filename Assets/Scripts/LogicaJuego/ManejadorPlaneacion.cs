using UnityEngine;
using CrazyRisk.Estructuras;
using CrazyRisk.Modelos;

namespace CrazyRisk.LogicaJuego
{
    /// <summary>
    /// Gestiona la lógica de planeación de movimientos de tropas entre territorios propios.
    /// </summary>
    public class ManejadorPlaneacion
    {
        private Territorio territorioOrigen;
        private Territorio territorioDestino;
        private Lista<Territorio> todosLosTerritorios;

        /// <summary>
        /// Inicializa la lista de territorios disponibles para la planeación.
        /// </summary>
        public void InicializarConTerritorios(Lista<Territorio> territorios)
        {
            todosLosTerritorios = territorios;
        }

        /// <summary>
        /// Selecciona el territorio de origen para mover tropas si pertenece al jugador y tiene suficientes tropas.
        /// </summary>
        public bool SeleccionarOrigen(Territorio territorio, int jugadorId)
        {
            if (territorio.PropietarioId != jugadorId)
            {
                Debug.Log("Solo puedes mover tropas desde tus propios territorios");
                return false;
            }

            if (territorio.CantidadTropas <= 1)
            {
                Debug.Log("Necesitas al menos 2 tropas para poder mover");
                return false;
            }

            territorioOrigen = territorio;
            Debug.Log($"Territorio origen seleccionado: {territorio.Nombre}");
            return true;
        }

        /// <summary>
        /// Selecciona el territorio de destino para mover tropas si pertenece al jugador y existe una ruta válida.
        /// </summary>
        public bool SeleccionarDestino(Territorio territorio, int jugadorId)
        {
            Debug.Log($"Origen es null: {territorioOrigen == null}");

            if (territorioOrigen == null)
            {
                Debug.Log("Primero debes seleccionar un territorio de origen");
                return false;
            }

            if (territorio.PropietarioId != jugadorId)
            {
                Debug.Log("Solo puedes mover tropas a tus propios territorios");
                return false;
            }

            Debug.Log($"Mismo territorio: {territorio.Id == territorioOrigen.Id}");

            if (territorio.Id == territorioOrigen.Id)
            {
                Debug.Log("El origen y destino deben ser diferentes");
                return false;
            }

            bool hayRuta = ExisteRuta(territorioOrigen, territorio, jugadorId);

            if (!hayRuta)
            {
                Debug.Log("No existe una ruta de territorios propios entre origen y destino");
                return false;
            }

            territorioDestino = territorio;
            Debug.Log($"✓ Territorio destino seleccionado: {territorio.Nombre}");
            return true;
        }

        /// <summary>
        /// Mueve la cantidad indicada de tropas del territorio de origen al de destino si la operación es válida.
        /// </summary>
        public bool MoverTropas(int cantidadTropas)
        {
            if (territorioOrigen == null || territorioDestino == null)
            {
                Debug.LogError("Debes seleccionar territorios de origen y destino");
                return false;
            }

            if (cantidadTropas <= 0)
            {
                Debug.LogError("Debes mover al menos 1 tropa");
                return false;
            }

            if (cantidadTropas >= territorioOrigen.CantidadTropas)
            {
                Debug.LogError("Debes dejar al menos 1 tropa en el territorio de origen");
                return false;
            }

            territorioOrigen.CantidadTropas -= cantidadTropas;
            territorioDestino.CantidadTropas += cantidadTropas;

            Debug.Log($"Movidas {cantidadTropas} tropas de {territorioOrigen.Nombre} a {territorioDestino.Nombre}");
            return true;
        }

        /// <summary>
        /// Verifica si existe una ruta de territorios propios entre el origen y el destino.
        /// </summary>
        private bool ExisteRuta(Territorio origen, Territorio destino, int jugadorId)
        {
            if (origen.EsAdyacenteA(destino.Id))
                return true;

            Lista<int> visitados = new Lista<int>();
            Lista<int> porVisitar = new Lista<int>();

            porVisitar.Agregar(origen.Id);
            visitados.Agregar(origen.Id);

            while (!porVisitar.EstaVacia())
            {
                int actualId = porVisitar.Obtener(0);
                porVisitar.Remover(actualId);

                Territorio actual = BuscarTerritorioPorId(actualId);
                if (actual == null) continue;

                Lista<int> adyacentes = actual.ObtenerTerritoriosAdyacentes();

                for (int i = 0; i < adyacentes.getSize(); i++)
                {
                    int adyacenteId = adyacentes.Obtener(i);

                    if (adyacenteId == destino.Id)
                        return true;

                    if (!visitados.Contiene(adyacenteId))
                    {
                        Territorio adyacente = BuscarTerritorioPorId(adyacenteId);

                        if (adyacente != null && adyacente.PropietarioId == jugadorId)
                        {
                            porVisitar.Agregar(adyacenteId);
                            visitados.Agregar(adyacenteId);
                        }
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Busca y retorna un territorio por su ID.
        /// </summary>
        private Territorio BuscarTerritorioPorId(int id)
        {
            if (todosLosTerritorios == null) return null;

            for (int i = 0; i < todosLosTerritorios.getSize(); i++)
            {
                if (todosLosTerritorios.Obtener(i).Id == id)
                    return todosLosTerritorios.Obtener(i);
            }
            return null;
        }

        /// <summary>
        /// Devuelve la cantidad máxima de tropas disponibles para mover desde el territorio de origen.
        /// </summary>
        public int TropasDisponiblesParaMover()
        {
            if (territorioOrigen == null)
                return 0;

            return territorioOrigen.CantidadTropas - 1;
        }

        /// <summary>
        /// Limpia la selección de territorios de origen y destino.
        /// </summary>
        public void LimpiarSeleccion()
        {
            territorioOrigen = null;
            territorioDestino = null;
        }

        /// <summary>
        /// Devuelve el territorio de origen seleccionado.
        /// </summary>
        public Territorio GetTerritorioOrigen() => territorioOrigen;

        /// <summary>
        /// Devuelve el territorio de destino seleccionado.
        /// </summary>
        public Territorio GetTerritorioDestino() => territorioDestino;
    }
}
