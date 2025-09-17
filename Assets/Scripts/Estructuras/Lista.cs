using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CrazyRisk.Estructuras
{
    public class Lista<T>
    {
        // Nodo interno
        private class Nodo
        {
            public T Dato { get; set; }
            public Nodo Siguiente { get; set; }
            
            public Nodo(T dato)
            {
                Dato = dato;
                Siguiente = null;
            }
        }

        // Propiedades
        private Nodo cabeza;
        private int tamaño;

        public int Tamaño => tamaño;
        public bool EstaVacia => tamaño == 0;

        // Constructor
        public Lista()
        {
            cabeza = null;
            tamaño = 0;
        }

        // Agrega un elemento al final de la lista
        public void Agregar(T elemento)
        {
            Nodo nuevoNodo = new Nodo(elemento);
            
            if (cabeza == null)
            {
                cabeza = nuevoNodo;
            }
            else
            {
                Nodo actual = cabeza;
                while (actual.Siguiente != null)
                {
                    actual = actual.Siguiente;
                }
                actual.Siguiente = nuevoNodo;
            }
            tamaño++;
        }

        // Remueve el primer elemento que coincida
        public bool Remover(T elemento)
        {
            if (cabeza == null)
                return false;

            if (cabeza.Dato.Equals(elemento))
            {
                cabeza = cabeza.Siguiente;
                tamaño--;
                return true;
            }

            Nodo actual = cabeza;
            while (actual.Siguiente != null)
            {
                if (actual.Siguiente.Dato.Equals(elemento))
                {
                    actual.Siguiente = actual.Siguiente.Siguiente;
                    tamaño--;
                    return true;
                }
                actual = actual.Siguiente;
            }
            
            return false;
        }

        // Obtiene el elemento en la posición especificada
        public T Obtener(int indice)
        {
            if (indice < 0 || indice >= tamaño)
                throw new ArgumentOutOfRangeException("Índice fuera de rango");

            Nodo actual = cabeza;
            for (int i = 0; i < indice; i++)
            {
                actual = actual.Siguiente;
            }
            return actual.Dato;
        }

        // Indexador para acceso directo
        public T this[int indice]
        {
            get => Obtener(indice);
            set
            {
                if (indice < 0 || indice >= tamaño)
                    throw new ArgumentOutOfRangeException("Índice fuera de rango");

                Nodo actual = cabeza;
                for (int i = 0; i < indice; i++)
                {
                    actual = actual.Siguiente;
                }
                actual.Dato = value;
            }
        }

        // Verifica si contiene un elemento
        public bool Contiene(T elemento)
        {
            Nodo actual = cabeza;
            while (actual != null)
            {
                if (actual.Dato.Equals(elemento))
                    return true;
                actual = actual.Siguiente;
            }
            return false;
        }

        // Mezcla aleatoriamente - útil para territorios
        public void Mezclar(Random random)
        {
            if (tamaño <= 1) return;

            T[] array = new T[tamaño];
            Nodo actual = cabeza;
            
            for (int i = 0; i < tamaño; i++)
            {
                array[i] = actual.Dato;
                actual = actual.Siguiente;
            }
            
            // Fisher-Yates shuffle
            for (int i = array.Length - 1; i > 0; i--)
            {
                int j = random.Next(0, i + 1);
                T temp = array[i];
                array[i] = array[j];
                array[j] = temp;
            }

            // Reconstruir lista mezclada
            cabeza = null;
            tamaño = 0;
            foreach (T elemento in array)
            {
                Agregar(elemento);
            }
        }

        public override string ToString()
        {
            if (EstaVacia)
                return "Lista vacía";

            string resultado = "[";
            Nodo actual = cabeza;
            
            while (actual != null)
            {
                resultado += actual.Dato.ToString();
                if (actual.Siguiente != null)
                    resultado += ", ";
                actual = actual.Siguiente;
            }
            
            resultado += "]";
            return resultado;
        }
    
    }
}