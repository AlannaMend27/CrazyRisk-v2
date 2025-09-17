using UnityEngine;
using CrazyRisk.Estructuras;
using CrazyRisk.Modelos;
using CrazyRisk.LogicaJuego;

public class GameManager : MonoBehaviour
{
    private DistribuidorTerritorios distribuidor;
    private Lista<Territorio> territorios;

    void Start()
    {
        InicializarJuego();
        ProbarDistribucion();
        ProbarSistemaDados();
        ProbarColocacionTropas();
    }

    private void InicializarJuego()
    {
        distribuidor = new DistribuidorTerritorios();
        territorios = distribuidor.ObtenerTodosLosTerritorios();
        Debug.Log($"Juego inicializado con {territorios.Tama�o} territorios");
    }

    private void ProbarDistribucion()
    {
        // Distribuir territorios entre jugadores
        distribuidor.DistribuirTerritorios("Jugador1", "Jugador2", "Neutral");

        // Contar territorios por propietario
        int jugador1Count = 0;
        int jugador2Count = 0;
        int neutralCount = 0;

        for (int i = 0; i < territorios.Tama�o; i++)
        {
            switch (territorios[i].PropietarioId)
            {
                case "Jugador1": jugador1Count++; break;
                case "Jugador2": jugador2Count++; break;
                case "Neutral": neutralCount++; break;
            }
        }

        Debug.Log($"Distribuci�n completada:");
        Debug.Log($"Jugador1: {jugador1Count} territorios");
        Debug.Log($"Jugador2: {jugador2Count} territorios");
        Debug.Log($"Neutral: {neutralCount} territorios");

        // Mostrar algunos territorios como ejemplo
        MostrarEjemploTerritorios();
    }

    private void MostrarEjemploTerritorios()
    {
        Debug.Log("--- Ejemplo de territorios distribuidos ---");
        for (int i = 0; i < 10 && i < territorios.Tama�o; i++)
        {
            Debug.Log(territorios[i].ToString());
        }
    }

    private void ProbarSistemaDados()
    {
        ManejadorCombate combate = new ManejadorCombate();

        Debug.Log("=== Prueba del sistema de dados ===");

        // Simular varios lanzamientos
        for (int i = 0; i < 3; i++)
        {
            int[] dadosAtacante = combate.LanzarDadosAtacante(3);
            int[] dadosDefensor = combate.LanzarDadosDefensor(2);

            Debug.Log($"Lanzamiento {i + 1}:");
            Debug.Log($"Atacante: [{dadosAtacante[0]}, {dadosAtacante[1]}, {dadosAtacante[2]}]");
            Debug.Log($"Defensor: [{dadosDefensor[0]}, {dadosDefensor[1]}]");
            Debug.Log($"Resultado combate: {combate.ResolverCombate(dadosAtacante, dadosDefensor)}");

        }
    }

    // Metodos para probar la colocanci�n de tropas iniciales
    private void ProbarColocacionTropas()
    {
        Debug.Log("=== Prueba del sistema de colocaci�n de tropas ===");

        // Mostrar estado inicial despu�s de distribuci�n
        Debug.Log("--- Estado inicial (despu�s de distribuci�n) ---");
        MostrarEstadoTropas();

        // Ejecutar colocaci�n de tropas por turnos
        Debug.Log("--- Iniciando colocaci�n de tropas por turnos ---");
        distribuidor.ColocarTropasIniciales("Jugador1", "Jugador2", "Neutral");

        // Mostrar estado final
        Debug.Log("--- Estado final (despu�s de colocaci�n) ---");
        MostrarEstadoTropas();

        Debug.Log("=== Prueba de colocaci�n de tropas completada ===");
    }

    private void MostrarEstadoTropas()
    {
        int tropasJ1 = ContarTropasJugador("Jugador1");
        int tropasJ2 = ContarTropasJugador("Jugador2");
        int tropasNeutral = ContarTropasJugador("Neutral");

        Debug.Log($"Tropas totales - Jugador1: {tropasJ1}, Jugador2: {tropasJ2}, Neutral: {tropasNeutral}");

        // Mostrar algunos ejemplos de territorios con sus tropas
        Debug.Log("Ejemplos de territorios:");
        int ejemplosMostrados = 0;
        for (int i = 0; i < territorios.Tama�o && ejemplosMostrados < 6; i++)
        {
            Territorio territorio = territorios[i];
            Debug.Log($"  {territorio.Nombre} ({territorio.PropietarioId}): {territorio.CantidadTropas} tropas");
            ejemplosMostrados++;
        }
    }

    private int ContarTropasJugador(string jugadorId)
    {
        int total = 0;
        for (int i = 0; i < territorios.Tama�o; i++)
        {
            if (territorios[i].PropietarioId == jugadorId)
            {
                total += territorios[i].CantidadTropas;
            }
        }
        return total;
    }
}
