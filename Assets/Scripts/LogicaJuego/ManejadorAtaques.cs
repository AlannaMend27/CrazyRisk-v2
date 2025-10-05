using UnityEngine;
using CrazyRisk.Estructuras;
using CrazyRisk.Modelos;
using CrazyRisk.Managers;

namespace CrazyRisk.LogicaJuego
{
    public class ManejadorAtaques
    {
        private ManejadorCombate manejadorCombate;
        private Territorio territorioAtacante;
        private Territorio territorioDefensor;

        public ManejadorAtaques()
        {
            manejadorCombate = new ManejadorCombate();
        }

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
            ManagerSonidos.Instance?.ReproducirAtaque();

            territorioAtacante.CantidadTropas -= resultado.tropasPerdidasAtacante;
            territorioDefensor.CantidadTropas -= resultado.tropasPerdidasDefensor;

            // CONQUISTA AUTOMATICA si defensor queda en 0
            if (territorioDefensor.CantidadTropas == 0)
            {
                resultado.conquistado = true;

                // Mover AUTOMATICAMENTE las tropas que atacaron
                int tropasAMover = dadosAtacante;

                // Validar que haya suficientes
                if (territorioAtacante.CantidadTropas > tropasAMover)
                {
                    int propietarioAnterior = territorioDefensor.PropietarioId;

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

                        if (jugador != null && !jugador.getEsNeutral())
                        {
                            Tarjeta nuevaTarjeta = Tarjeta.CrearTarjetaAleatoria(territorioDefensor.Nombre);
                            jugador.getTarjetas().Agregar(nuevaTarjeta);

                            Debug.Log($"{jugador.getNombre()} obtuvo tarjeta: {nuevaTarjeta}!");

                            if (jugador.getTarjetas().getSize() >= 6)
                            {
                                Debug.LogWarning($"{jugador.getNombre()} tiene 6 tarjetas. Debe intercambiar!");
                            }
                        }
                    }
                }
                else
                {
                    Debug.LogError("No hay suficientes tropas para conquistar");
                    resultado.conquistado = false;
                }
            }

            Debug.Log($"Ataque ejecutado: {resultado.ToString()}");
            return resultado;
        }

        private void CalcularBajas(int[] dadosAtacante, int[] dadosDefensor, ResultadoAtaque resultado)
        {
            int comparaciones = Mathf.Min(dadosAtacante.Length, dadosDefensor.Length);

            for (int i = 0; i < comparaciones; i++)
            {
                if (dadosAtacante[i] > dadosDefensor[i])
                {
                    resultado.tropasPerdidasDefensor++;
                }
                else
                {
                    resultado.tropasPerdidasAtacante++;
                }
            }
        }

        public void LimpiarSeleccion()
        {
            territorioAtacante = null;
            territorioDefensor = null;
        }

        public Territorio GetTerritorioAtacante() => territorioAtacante;
        public Territorio GetTerritorioDefensor() => territorioDefensor;
    }

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