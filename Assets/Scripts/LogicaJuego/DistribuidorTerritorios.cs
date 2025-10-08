using System;
using CrazyRisk.Estructuras;
using CrazyRisk.Modelos;

namespace CrazyRisk.LogicaJuego
{
    public class DistribuidorTerritorios
    {
        //Prpiedades
        private Lista<Territorio> todosLosTerritorios;
        private Random random;

        private int[] tropasRestantes;
        private int jugadorActualIndex = 0;
        private int[] idsJugadores;
        private bool fasePreparacionActiva = false;
        private int idJugadorNeutral = -1; 
        private Lista<Jugador> jugadoresRef;

        //EVENTOS
        // Evento para actualizar constantemente los numeros de los territorios
        public event System.Action<string> OnTropaColocada;

        //evento para notificar el cambio de turno
        public event System.Action OnCambioTurno;

        // Evento para notificar cuando termina la preparación
        public event System.Action OnPreparacionCompletada;

        public DistribuidorTerritorios(Lista<Territorio> territorios)
        {
            random = new Random();
            todosLosTerritorios = territorios;
        }

        /// <summary>
        /// Distribuye los 42 territorios aleatoriamente con 1 tropa inicial
        /// </summary>
        public void DistribuirTerritorios(Lista<int> idsJugadores)
        {
            int numJugadores = idsJugadores.getSize();
            Lista<int> propietarios = new Lista<int>();

            // Distribuir territorios equitativamente
            for (int i = 0; i < todosLosTerritorios.getSize(); i++)
            {
                int jugadorIndex = i % numJugadores;
                propietarios.Agregar(idsJugadores[jugadorIndex]);
            }

            // Mezclar para aleatoriedad
            propietarios.Mezclar(random);

            // Asignar territorios
            for (int i = 0; i < todosLosTerritorios.getSize(); i++)
            {
                todosLosTerritorios[i].PropietarioId = propietarios[i];
                todosLosTerritorios[i].CantidadTropas = 1;

                UnityEngine.Debug.Log($"Territorio {todosLosTerritorios[i].Nombre} → Jugador {propietarios[i]} (1 tropa)");
            }
        }

        /// <summary>
        /// Inicia la fase de preparación manual
        /// </summary>
        public void IniciarFasePreparacion(Lista<int> idsJugadores, int idNeutral = -1, Lista<Jugador> jugadores = null)
        {
            this.idJugadorNeutral = idNeutral;
            this.jugadoresRef = jugadores;
            int numJugadores = idsJugadores.getSize();

            this.idsJugadores = new int[numJugadores];
            tropasRestantes = new int[numJugadores];

            int tropasIniciales = (idNeutral != -1) ? 26 : 21;

            for (int i = 0; i < numJugadores; i++)
            {
                this.idsJugadores[i] = idsJugadores[i];
                tropasRestantes[i] = tropasIniciales;
            }

            jugadorActualIndex = 0;
            fasePreparacionActiva = true;

            if (EsNeutral(GetJugadorActual()))
            {
                ColocarTropaNeutralConDelay();
            }
        }

        /// <summary>
        /// Intenta colocar una tropa (solo para jugadores humanos)
        /// </summary>
        public bool IntentarColocarTropa(string nombreTerritorio)
        {
            if (!fasePreparacionActiva)
            {
                UnityEngine.Debug.LogWarning("No estás en fase de preparación");
                return false;
            }

            // Protección contra clicks múltiples
            if (tropasRestantes[jugadorActualIndex] <= 0)
            {
                UnityEngine.Debug.LogWarning("Ya colocaste todas tus tropas este turno");
                return false;
            }

            int jugadorActual = GetJugadorActual();

            // Evitar que jugadores humanos coloquen en turno del neutral
            if (EsNeutral(jugadorActual))
            {
                UnityEngine.Debug.LogWarning("Es el turno del neutral (colocación automática)");
                return false;
            }

            Territorio territorio = BuscarTerritorioPorNombre(nombreTerritorio);

            //En caso de que el terrritorio no exista
            if (territorio == null)
            {
                UnityEngine.Debug.LogError($"Territorio '{nombreTerritorio}' no encontrado");
                return false;
            }

            //En caso de que el territorio no sea del jugador en turno
            if (territorio.PropietarioId != jugadorActual)
            {
                UnityEngine.Debug.LogWarning($"Este territorio no es tuyo. Pertenece a: {territorio.PropietarioId}");
                return false;
            }

            //  Colocar tropa
            territorio.CantidadTropas++;
            tropasRestantes[jugadorActualIndex]--;
            CrazyRisk.Managers.ManagerSonidos.Instance?.ReproducirColocarTropas();

            UnityEngine.Debug.Log($"✓ {GetNombreJugadorActual()} colocó 1 tropa en {nombreTerritorio} (Total: {territorio.CantidadTropas})");
            UnityEngine.Debug.Log($"   Tropas restantes: {tropasRestantes[jugadorActualIndex]}");

            //Avanzar al siguiente jugador
            AvanzarTurno();
            return true;
        }

        /// <summary>
        /// Coloca una tropa del neutral con delay para visualización
        /// </summary>
        private async void ColocarTropaNeutralConDelay()
        {
            await System.Threading.Tasks.Task.Delay(1000); // Espera 1 segundo

            if (!fasePreparacionActiva) return; // Por si la fase terminó

            Lista<Territorio> territoriosNeutral = ObtenerTerritoriosPorJugador(GetJugadorActual());

            if (territoriosNeutral.getSize() == 0)
            {
                UnityEngine.Debug.LogError("El neutral no tiene territorios");
                return;
            }

            int indiceAleatorio = random.Next(territoriosNeutral.getSize());
            Territorio territorio = territoriosNeutral[indiceAleatorio];

            territorio.CantidadTropas++;
            tropasRestantes[jugadorActualIndex]--;

            // Notificar al GameManager sobre que el neutral coloca una tropa para actualizar interfaz
            OnTropaColocada?.Invoke(territorio.Nombre);

            UnityEngine.Debug.Log($"✓ Neutral colocó 1 tropa en {territorio.Nombre} (Total: {territorio.CantidadTropas})");
            UnityEngine.Debug.Log($"   Tropas restantes: {tropasRestantes[jugadorActualIndex]}");

            AvanzarTurno();
        }

        /// <summary>
        /// Avanza al siguiente jugador
        /// </summary>
        private void AvanzarTurno()
        {
            if (TodosTerminaron())
            {
                FinalizarFasePreparacion();
                return;
            }

            jugadorActualIndex = (jugadorActualIndex + 1) % idsJugadores.Length;

            UnityEngine.Debug.Log($"--- Turno de {GetNombreJugadorActual()} ---");
            UnityEngine.Debug.Log($"   Tropas restantes: {GetTropasRestantesJugadorActual()}");

            // Notificar cambio de turno
            OnCambioTurno?.Invoke();

            // colocar con delay, en caso de ser neutral
            if (EsNeutral(GetJugadorActual()))
            {
                ColocarTropaNeutralConDelay();
            }
        }

        /// <summary>
        /// Verifica si todos terminaron
        /// </summary>
        private bool TodosTerminaron()
        {
            for (int i = 0; i < tropasRestantes.Length; i++)
            {
                if (tropasRestantes[i] > 0)
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Finaliza la fase de preparación
        /// </summary>
        private void FinalizarFasePreparacion()
        {
            fasePreparacionActiva = false;

            UnityEngine.Debug.Log("=== FASE DE PREPARACIÓN COMPLETADA (Distribuidor) ===");
            VerificarDistribucion();

            // Verificar si hay suscriptores al evento
            if (OnPreparacionCompletada != null)
            {
                OnPreparacionCompletada.Invoke();
            }
            else
            {
                UnityEngine.Debug.LogError("NO HAY SUSCRIPTORES al evento OnPreparacionCompletada");
            }
        }

        /// <summary>
        /// Busca territorio por nombre
        /// </summary>
        private Territorio BuscarTerritorioPorNombre(string nombre)
        {
            for (int i = 0; i < todosLosTerritorios.getSize(); i++)
            {
                if (todosLosTerritorios[i].Nombre == nombre)
                {
                    return todosLosTerritorios[i];
                }
            }
            return null;
        }

        // ========== MÉTODOS AUXILIARES ==========

        /// <summary>
        /// Obtiene el id del jugador que tiene el turno actual.
        /// </summary>
        public int GetJugadorActual() => idsJugadores[jugadorActualIndex];

        /// <summary>
        /// Obtiene la cantidad de tropas restantes para el jugador actual.
        /// </summary>
        public int GetTropasRestantesJugadorActual() => tropasRestantes[jugadorActualIndex];

        /// <summary>
        /// Indica si la fase de preparación está activa.
        /// </summary>
        public bool EstaEnFasePreparacion() => fasePreparacionActiva;

        /// <summary>
        /// Indica si el jugador especificado es el neutral.
        /// </summary>
        public bool EsNeutral(int jugadorId)
        {
            return jugadorId == idJugadorNeutral && idJugadorNeutral != -1;
        }
        
        /// <summary>
        /// Indica si es el turno del jugador especificado.
        /// </summary>
        public bool EsTurnoDelJugador(int jugadorId) => GetJugadorActual() == jugadorId;

        /// <summary>
        /// Obtiene el nombre del jugador para mostrar en UI
        /// </summary>
        public string GetNombreJugadorActual()
        {
            int id = GetJugadorActual();

            // Buscar en la lista de jugadores
            if (jugadoresRef != null)
            {
                for (int i = 0; i < jugadoresRef.getSize(); i++)
                {
                    Jugador jugador = jugadoresRef.Obtener(i);
                    if (jugador.getId() == id)
                    {
                        return jugador.getNombre();
                    }
                }
            }
            if (EsNeutral(id))
                return "Ejército Neutral";

            return $"Jugador {id}";
        }

        /// <summary>
        /// Devuelve la lista de todos los territorios.
        /// </summary>
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
        /// Cuenta el total de tropas que tiene el jugador especificado.
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
        /// Devuelve la lista de todos los territorios.
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
        /// Verifica que la distribución de tropas sea la esperada para cada jugador.
        /// </summary>
        private void VerificarDistribucion()
        {
            if (idsJugadores == null) return;

            // 2 jugadores + neutral = 40 tropas
            // 3 jugadores reales = 35 tropas
            int tropasEsperadas = (idJugadorNeutral != -1) ? 40 : 35;

            for (int i = 0; i < idsJugadores.Length; i++)
            {
                int tropas = ContarTropasJugador(idsJugadores[i]);

                if (tropas != tropasEsperadas)
                {
                    throw new InvalidOperationException(
                        $" Error: Jugador {idsJugadores[i]} tiene {tropas} tropas, esperadas {tropasEsperadas}");
                }
            }
        }
    }
}