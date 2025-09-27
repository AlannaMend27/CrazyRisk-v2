using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CrazyRisk.Estructuras
{
    public class Lista<T>
    {
        // Clase interna para los nodos de la lista
        private class Node
        {
            public T value { get; set; }
            public Node next { get; set; }
            
            public Node(T value)
            {
                this.value = value;
                this.next = null;
            }
        }

        // Propiedades
        private Node head;
        private int size;


        // Constructor
        public Lista()
        {
            head = null;
            size = 0;
        }

        // get del tamano de la lista
        public int getSize()
        {
            return size;
        }

        //Verifica si la lista está vacía
        public bool EstaVacia()
        {
            return size == 0;
        }

        // Agrega un elemento al final de la lista

        public void Agregar(T elemento)
        {
            Node newNodito = new Node(elemento);
            
            if (head == null)
            {
                head = newNodito;
            }
            else
            {
                Node actual = head;
                while (actual.next != null)
                {
                    actual = actual.next;
                }
                actual.next = newNodito;
            }
            size++;
        }
       

        // Remueve el primer elemento que coincida
        public bool Remover(T elemento)
        {
            if (head == null)
                return false;

            if (head.value.Equals(elemento))
            {
                head = head.next;
                size--;
                return true;
            }

            Node actual = head;
            while (actual.next != null)
            {
                if (actual.next.value.Equals(elemento))
                {
                    actual.next = actual.next.next;
                    size--;
                    return true;
                }
                actual = actual.next;
            }
            
            return false;
        }

        // Obtiene el elemento en la posición especificada
        public T Obtener(int indice)
        {
            if (indice < 0 || indice >= size)
                throw new ArgumentOutOfRangeException("Índice fuera de rango");

            Node actual = head;
            for (int i = 0; i < indice; i++)
            {
                actual = actual.next;
            }
            return actual.value;
        }

        // Indexador para acceso directo
        public T this[int indice]
        {
            get => Obtener(indice);
            set
            {
                if (indice < 0 || indice >= size)
                    throw new ArgumentOutOfRangeException("Índice fuera de rango");

                Node actual = head;
                for (int i = 0; i < indice; i++)
                {
                    actual = actual.next;
                }
                actual.value = value;
            }
        }

        // Verifica si contiene un elemento
        public bool Contiene(T elemento)
        {
            Node actual = head;
            while (actual != null)
            {
                if (actual.value.Equals(elemento))
                    return true;
                actual = actual.next;
            }
            return false;
        }

        // Mezcla aleatoriamente - útil para territorios
        public void Mezclar(Random random)
        {
            if (size <= 1) return;

            T[] array = new T[size];
            Node actual = head;
            
            for (int i = 0; i < size; i++)
            {
                array[i] = actual.value;
                actual = actual.next;
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
            head = null;
            size = 0;
            foreach (T elemento in array)
            {
                Agregar(elemento);
            }
        }

        public override string ToString()
        {
            if (EstaVacia())
                return "Lista vacía";

            string resultado = "[";
            Node actual = head;
            
            while (actual != null)
            {
                resultado += actual.value.ToString();
                if (actual.next != null)
                    resultado += ", ";
                actual = actual.next;
            }
            
            resultado += "]";
            return resultado;
        }
    
    }
}