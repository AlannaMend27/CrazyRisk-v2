using System;
using CrazyRisk.Estructuras;
using CrazyRisk.Modelos;

namespace CrazyRisk.LogicaJuego
{
    public class DistribuidorTerritorios
    {
        private Lista<Territorio> todosLosTerritorios;
        private Random random;

        private int[] tropasRestantes;
        private int jugadorActualIndex = 0;
        private int[] idsJugadores;
        private bool fasePreparacionActiva = false;

        //EVENTOS
        // Evento para actualizar constantemente los numeros de los territorios
        public event System.Action<string> OnTropaColocada;
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
        public void DistribuirTerritorios(int jugador1Id, int jugador2Id, int neutralId)
        {
            Lista<int> propietarios = new Lista<int>();

            for (int i = 0; i < 14; i++)
            {
                propietarios.Agregar(jugador1Id);
                propietarios.Agregar(jugador2Id);
                propietarios.Agregar(neutralId);
            }

            propietarios.Mezclar(random);

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
        public void IniciarFasePreparacion(int jugador1Id, int jugador2Id, int neutralId)
        {
            idsJugadores = new int[] { jugador1Id, jugador2Id, neutralId };
            tropasRestantes = new int[] { 26, 26, 26 };
            jugadorActualIndex = 0;
            fasePreparacionActiva = true;

            UnityEngine.Debug.Log("=== FASE DE PREPARACIÓN INICIADA ===");
            UnityEngine.Debug.Log($"Cada jugador debe colocar 26 tropas adicionales");
            UnityEngine.Debug.Log($"Turno de {GetNombreJugadorActual()}");

            //  Si el primer turno es del neutral, colocarlo automáticamente
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

            if (territorio == null)
            {
                UnityEngine.Debug.LogError($"Territorio '{nombreTerritorio}' no encontrado");
                return false;
            }

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
            VerificarDistribucion(idsJugadores[0], idsJugadores[1], idsJugadores[2]);

            UnityEngine.Debug.Log("🎮 Disparando evento OnPreparacionCompletada...");

            // Verificar si hay suscriptores
            if (OnPreparacionCompletada != null)
            {
                UnityEngine.Debug.Log($"   Hay {OnPreparacionCompletada.GetInvocationList().Length} suscriptor(es)");
                OnPreparacionCompletada.Invoke();
                UnityEngine.Debug.Log("   Evento disparado exitosamente");
            }
            else
            {
                UnityEngine.Debug.LogError("❌ NO HAY SUSCRIPTORES al evento OnPreparacionCompletada");
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

        public int GetJugadorActual() => idsJugadores[jugadorActualIndex];

        public int GetTropasRestantesJugadorActual() => tropasRestantes[jugadorActualIndex];

        public bool EstaEnFasePreparacion() => fasePreparacionActiva;

        public bool EsNeutral(int jugadorId) => jugadorId == idsJugadores[2];

        public bool EsTurnoDelJugador(int jugadorId) => GetJugadorActual() == jugadorId;

        /// <summary>
        /// Obtiene el nombre del jugador para mostrar en UI
        /// </summary>
        public string GetNombreJugadorActual()
        {
            int id = GetJugadorActual();

            if (EsNeutral(id))
                return "Ejército Neutral";
            else if (id == idsJugadores[0])
                return "Jugador 1";
            else
                return "Jugador 2";
        }

        // ========== MÉTODOS AUXILIARES ==========

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

        public Lista<Territorio> ObtenerTerritoriosDeJugador(int jugadorId)
        {
            return ObtenerTerritoriosPorJugador(jugadorId);
        }

        public Lista<Territorio> ObtenerTodosLosTerritorios()
        {
            return todosLosTerritorios;
        }

        private void VerificarDistribucion(int jugador1Id, int jugador2Id, int neutralId)
        {
            int tropasJ1 = ContarTropasJugador(jugador1Id);
            int tropasJ2 = ContarTropasJugador(jugador2Id);
            int tropasNeutral = ContarTropasJugador(neutralId);

            if (tropasJ1 != 40 || tropasJ2 != 40 || tropasNeutral != 40)
            {
                throw new InvalidOperationException(
                    $"❌ Error: J1={tropasJ1}, J2={tropasJ2}, Neutral={tropasNeutral}");
            }

            UnityEngine.Debug.Log($"✅ Verificación exitosa: Todos tienen 40 tropas");
        }
    }
}