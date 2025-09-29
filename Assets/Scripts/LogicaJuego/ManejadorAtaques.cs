using UnityEngine;
using CrazyRisk.Estructuras;
using CrazyRisk.Modelos;

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
                Debug.LogError("Ataque inválido");
                return null;
            }

            int dadosAtacante = territorioAtacante.MaximoDadosAtaque();
            int dadosDefensor = Mathf.Min(territorioDefensor.CantidadTropas, 2);

            int[] resultadosAtacante = manejadorCombate.LanzarDadosAtacante(dadosAtacante);
            int[] resultadosDefensor = manejadorCombate.LanzarDadosDefensor(dadosDefensor);

            ResultadoAtaque resultado = new ResultadoAtaque();
            resultado.territorioAtacante = territorioAtacante.Nombre;
            resultado.territorioDefensor = territorioDefensor.Nombre;
            resultado.dadosAtacante = resultadosAtacante;
            resultado.dadosDefensor = resultadosDefensor;

            CalcularBajas(resultadosAtacante, resultadosDefensor, resultado);

            territorioAtacante.CantidadTropas -= resultado.tropasPerdidasAtacante;
            territorioDefensor.CantidadTropas -= resultado.tropasPerdidasDefensor;

            if (territorioDefensor.CantidadTropas == 0)
            {
                resultado.conquistado = true;
            }

            Debug.Log($"Ataque ejecutado: {resultado.ToString()}");
            return resultado;
        }

        public bool ConquistarTerritorio(int nuevoPropietarioId, int tropasAMover)
        {
            if (territorioDefensor == null || territorioDefensor.CantidadTropas > 0)
            {
                Debug.LogError("No se puede conquistar este territorio");
                return false;
            }

            if (territorioAtacante == null || territorioAtacante.CantidadTropas <= tropasAMover)
            {
                Debug.LogError("No tienes suficientes tropas disponibles");
                return false;
            }

            territorioDefensor.CambiarPropietario(nuevoPropietarioId, tropasAMover);
            territorioAtacante.CantidadTropas -= tropasAMover;

            Debug.Log($"{territorioDefensor.Nombre} conquistado por jugador {nuevoPropietarioId}");
            return true;
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
                msg += "\n¡TERRITORIO CONQUISTADO!";

            return msg;
        }
    }
}
