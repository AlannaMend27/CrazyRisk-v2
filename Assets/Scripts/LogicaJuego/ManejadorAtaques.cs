using UnityEngine;
using System;
using CrazyRisk.Estructuras;
using CrazyRisk.Modelos;
using CrazyRisk.Managers;

namespace CrazyRisk.LogicaJuego
{
    /// <summary>
    /// Gestiona la lógica de selección de territorios, ejecución de ataques y resolución de combates entre territorios
    /// </summary>
    public class ManejadorAtaques
    {
        private ManejadorCombate manejadorCombate;
        private Territorio territorioAtacante;
        private Territorio territorioDefensor;


        /// <summary>
        /// Inicializa el manejador de ataques y su manejador de combate interno
        /// </summary>
        public ManejadorAtaques()
        {
            manejadorCombate = new ManejadorCombate();
        }

        /// <summary>
        /// Selecciona el territorio atacante si pertenece al jugador y puede atacar
        /// </summary>
        public bool SeleccionarAtacante(Territorio territorio, int jugadorId)
        {
            if (territorio.PropietarioId != jugadorId)
            {
                Debug.Log("No puedes atacar desde un territorio que no controlas");
                return false;
            }

            if (!territorio.PuedeAtacar())
            {
                Debug.Log("El territorio necesita al menos 2 tropas para atacar");
                return false;
            }

            territorioAtacante = territorio;
            Debug.Log($"Territorio atacante seleccionado: {territorio.Nombre}");
            return true;
        }

        /// <summary>
        /// Selecciona el territorio defensor si es adyacente y no pertenece al jugador
        /// </summary>
        public bool SeleccionarDefensor(Territorio territorio, int jugadorId)
        {
            if (territorioAtacante == null)
            {
                Debug.Log("Primero debes seleccionar un territorio atacante");
                return false;
            }

            if (territorio.PropietarioId == jugadorId)
            {
                Debug.Log("No puedes atacar tus propios territorios");
                return false;
            }

            if (!territorioAtacante.EsAdyacenteA(territorio.Id))
            {
                Debug.Log("Solo puedes atacar territorios adyacentes");
                return false;
            }

            territorioDefensor = territorio;
            Debug.Log($"Territorio defensor seleccionado: {territorio.Nombre}");
            return true;
        }

        /// <summary>
        /// Ejecuta el ataque entre los territorios seleccionados usando el máximo de dados permitido
        /// </summary>
        public ResultadoAtaque EjecutarAtaque()
        {
            if (territorioAtacante == null || territorioDefensor == null)
            {
                Debug.LogError("Debes seleccionar territorios atacante y defensor");
                return null;
            }

            if (!manejadorCombate.ValidarAtaque(territorioAtacante, territorioDefensor))
            {
                Debug.LogError("Ataque invalido");
                return null;
            }

            int dadosAtacante = territorioAtacante.MaximoDadosAtaque();
            int dadosDefensor = Mathf.Min(territorioDefensor.CantidadTropas, 2);

            return EjecutarAtaqueConDados(dadosAtacante, dadosDefensor);
        }

        /// <summary>
        /// Ejecuta el ataque entre los territorios seleccionados usando la cantidad de dados especificada.
        /// </summary>
        public ResultadoAtaque EjecutarAtaqueConDados(int dadosAtacante, int dadosDefensor)
        {
            if (territorioAtacante == null || territorioDefensor == null)
            {
                Debug.LogError("Debes seleccionar territorios atacante y defensor");
                return null;
            }

            if (!manejadorCombate.ValidarAtaque(territorioAtacante, territorioDefensor))
            {
                Debug.LogError("Ataque invalido");
                return null;
            }

            int maxDadosAtacante = territorioAtacante.MaximoDadosAtaque();
            int maxDadosDefensor = Mathf.Min(territorioDefensor.CantidadTropas, 2);

            if (dadosAtacante < 1 || dadosAtacante > maxDadosAtacante)
            {
                Debug.LogError($"Dados atacante invalidos. Rango: 1-{maxDadosAtacante}");
                return null;
            }

            if (dadosDefensor < 1 || dadosDefensor > maxDadosDefensor)
            {
                Debug.LogError($"Dados defensor invalidos. Rango: 1-{maxDadosDefensor}");
                return null;
            }

            ManagerSonidos.Instance?.ReproducirDados();
            int[] resultadosAtacante = manejadorCombate.LanzarDadosAtacante(dadosAtacante);
            int[] resultadosDefensor = manejadorCombate.LanzarDadosDefensor(dadosDefensor);

            ResultadoAtaque resultado = new ResultadoAtaque();
            resultado.territorioAtacante = territorioAtacante.Nombre;
            resultado.territorioDefensor = territorioDefensor.Nombre;
            resultado.dadosAtacante = resultadosAtacante;
            resultado.dadosDefensor = resultadosDefensor;

            CalcularBajas(resultadosAtacante, resultadosDefensor, resultado);

            // Aplicar bajas
            territorioAtacante.CantidadTropas -= resultado.tropasPerdidasAtacante;
            territorioDefensor.CantidadTropas -= resultado.tropasPerdidasDefensor;

            // conquista automatica si la cantidad de tropas del defensor son 0
            if (territorioDefensor.CantidadTropas == 0)
            {
                resultado.conquistado = true;

                // Mover las tropas que atacaron
                int tropasAMover = dadosAtacante;

                // Validar que haya suficientes
                if (territorioAtacante.CantidadTropas > tropasAMover)
                {
                    territorioAtacante.CantidadTropas -= tropasAMover;
                    territorioDefensor.CantidadTropas = tropasAMover;
                    territorioDefensor.PropietarioId = territorioAtacante.PropietarioId;

                    Debug.Log($"CONQUISTA: {territorioDefensor.Nombre} conquistado! {tropasAMover} tropas movidas.");

                    // Dar tarjeta
                    GameManager gameManager = UnityEngine.Object.FindObjectOfType<GameManager>();
                    if (gameManager != null)
                    {
                        Jugador jugador = null;

                        if (gameManager.GetJugador1().getId() == territorioAtacante.PropietarioId)
                            jugador = gameManager.GetJugador1();
                        else if (gameManager.GetJugador2().getId() == territorioAtacante.PropietarioId)
                            jugador = gameManager.GetJugador2();
                        else if (gameManager.GetJugador3() != null &&
                                 gameManager.GetJugador3().getId() == territorioAtacante.PropietarioId)
                            jugador = gameManager.GetJugador3();

                        if (jugador != null && !jugador.getEsNeutral())
                        {
                            // Verificar limite de tarjetas antes de dar la siguiente
                            if (jugador.getTarjetas().getSize() >= 5)
                            {
                                Debug.LogWarning($"{jugador.getNombre()} tiene 5 tarjetas. Debe intercambiar antes de recibir otra.");
                                IntercambiarAutomaticamente(jugador);
                            }

                            // Dar la tarjeta luego del intercambio
                            Tarjeta nuevaTarjeta = Tarjeta.CrearTarjetaAleatoria(territorioDefensor.Nombre);
                            jugador.getTarjetas().Agregar(nuevaTarjeta);

                            Debug.Log($"{jugador.getNombre()} obtuvo tarjeta: {nuevaTarjeta}!");
                        }
                    }

                    // Notificar al ManejadorTurnos sobre la conquista exitosa
                    ManejadorTurnos manejadorTurnos = UnityEngine.Object.FindObjectOfType<ManejadorTurnos>();
                    if (manejadorTurnos != null)
                    {
                        manejadorTurnos.RegistrarAtaqueRealizado();
                    }
                }
                else
                {
                    Debug.LogError("No hay suficientes tropas para conquistar");
                    resultado.conquistado = false;
                }
            }
            else
            {
                // Registrar ataque incluso si no hubo conquista
                ManejadorTurnos manejadorTurnos = UnityEngine.Object.FindObjectOfType<ManejadorTurnos>();
                if (manejadorTurnos != null)
                {
                    manejadorTurnos.RegistrarAtaqueRealizado();
                }
            }

            string nombreAtacanteFinal = territorioAtacante.Nombre;
            string nombreDefensorFinal = territorioDefensor.Nombre;
            
            // Mostrar los dados REALES en el visualizador
            VisualizadorDados visualizador = UnityEngine.Object.FindObjectOfType<VisualizadorDados>();

            Debug.Log($"Visualizador encontrado: {visualizador != null}"); // ← AGREGAR ESTO

            if (visualizador != null)
            {
                Debug.Log($"Llamando a MostrarCombateConResultados con dados: [{string.Join(", ", resultadosAtacante)}] vs [{string.Join(", ", resultadosDefensor)}]"); // ← Y ESTO
                
                visualizador.MostrarCombateConResultados(
                    nombreAtacanteFinal,
                    nombreDefensorFinal,
                    resultadosAtacante,
                    resultadosDefensor,
                    resultado.tropasPerdidasAtacante,
                    resultado.tropasPerdidasDefensor
                );
            }
            else
            {
                Debug.LogError("❌ VisualizadorDados NO encontrado en la escena!"); // ← Y ESTO
            }

            return resultado;
        }


        /// <summary>
        /// Calcula las bajas de ambos bandos comparando los resultados de los dados.
        /// </summary>
        private void CalcularBajas(int[] dadosAtacante, int[] dadosDefensor, ResultadoAtaque resultado)
        {
            // Ordenar los dados de mayor a menor
            Array.Sort(dadosAtacante);
            Array.Reverse(dadosAtacante);
            Array.Sort(dadosDefensor);
            Array.Reverse(dadosDefensor);

            int comparaciones = Math.Min(dadosAtacante.Length, dadosDefensor.Length);

            for (int i = 0; i < comparaciones; i++)
            {
                if (dadosAtacante[i] > dadosDefensor[i])
                {
                    // Defensor pierde una tropa
                    resultado.tropasPerdidasDefensor++;
                }
                else
                {
                    // Atacante pierde una tropa
                    resultado.tropasPerdidasAtacante++;
                }
            }
        }

        /// <summary>
        /// Realiza automáticamente el intercambio de tarjetas si el jugador supera el límite permitido.
        /// </summary>
        private void IntercambiarAutomaticamente(Jugador jugador)
        {
            ManejadorTarjetas manejadorTarjetas = UnityEngine.Object.FindObjectOfType<ManejadorTarjetas>();

            if (manejadorTarjetas == null)
            {
                GameObject obj = new GameObject("ManejadorTarjetas");
                manejadorTarjetas = obj.AddComponent<ManejadorTarjetas>();
            }

            int[] trio = manejadorTarjetas.EncontrarTrioValido(jugador);

            if (trio != null)
            {
                // Obtener refuerzos antes de intercambiar
                ManejadorRefuerzos manejadorRefuerzos = new ManejadorRefuerzos();
                int refuerzosObtenidos = manejadorRefuerzos.Fibonacci();

                // Realizar intercambio
                manejadorTarjetas.IntentarIntercambio(jugador, trio[0], trio[1], trio[2]);

                // Agregar refuerzos al turno actual si está en fase de refuerzos
                ManejadorTurnos manejadorTurnos = UnityEngine.Object.FindObjectOfType<ManejadorTurnos>();
                if (manejadorTurnos != null)
                {
                    manejadorTurnos.AgregarRefuerzosDinamicos(refuerzosObtenidos);
                    Debug.Log($"Intercambio automático: {jugador.getNombre()} recibió {refuerzosObtenidos} refuerzos");
                }
            }
            else
            {
                Debug.LogError($"{jugador.getNombre()} tiene 6 tarjetas pero no hay trío válido!");
            }
        }

        /// <summary>
        /// Limpia la selección de territorios atacante y defensor.
        /// </summary>
        public void LimpiarSeleccion()
        {
            territorioAtacante = null;
            territorioDefensor = null;
        }

        public Territorio GetTerritorioAtacante() => territorioAtacante;
        public Territorio GetTerritorioDefensor() => territorioDefensor;

        /// <summary>
        /// Indica si hay una selección válida de atacante y defensor.
        /// </summary>
        public bool TieneSeleccionValida()
        {
            return territorioAtacante != null && territorioDefensor != null;
        }

        /// <summary>
        /// Devuelve información textual sobre la selección actual de territorios.
        /// </summary>
        public string GetInfoSeleccion()
        {
            if (territorioAtacante == null && territorioDefensor == null)
                return "Sin selección";
            else if (territorioAtacante != null && territorioDefensor == null)
                return $"Atacante: {territorioAtacante.Nombre} - Selecciona defensor";
            else if (territorioAtacante != null && territorioDefensor != null)
                return $"Atacante: {territorioAtacante.Nombre} vs Defensor: {territorioDefensor.Nombre}";
            else
                return "Estado de selección inválido";
        }

        /// <summary>
        /// Representa el resultado de un ataque, incluyendo dados, bajas y si hubo conquista.
        /// </summary>
        public class ResultadoAtaque
        {
            public string territorioAtacante;
            public string territorioDefensor;
            public int[] dadosAtacante;
            public int[] dadosDefensor;
            public int tropasPerdidasAtacante;
            public int tropasPerdidasDefensor;
            public bool conquistado;

            public override string ToString()
            {
                string dados1 = string.Join(", ", dadosAtacante);
                string dados2 = string.Join(", ", dadosDefensor);

                string msg = $"{territorioAtacante} [{dados1}] vs {territorioDefensor} [{dados2}]\n";
                msg += $"Bajas - Atacante: {tropasPerdidasAtacante}, Defensor: {tropasPerdidasDefensor}";

                if (conquistado)
                    msg += "\nTERRITORIO CONQUISTADO!";

                return msg;
            }
        }
    }
}