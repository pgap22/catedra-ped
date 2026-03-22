namespace ProyectoCatedra.Estructuras
{
    public class MonticuloMaximo
    {
        private ElementoHeap?[] elementos;
        private int contador;
        private int capacidad;

        public MonticuloMaximo(int capacidadInicial = 100)
        {
            capacidad = capacidadInicial;
            elementos = new ElementoHeap[capacidad];
            contador = 0;
        }

        public void Insertar(int prioridad, object valor)
        {
            if (contador == capacidad) Redimensionar();
            var nuevo = new ElementoHeap { Prioridad = prioridad, Valor = valor };
            elementos[contador] = nuevo;
            Subir(contador);
            contador++;
        }

        public object? ExtraerMaximo()
        {
            if (contador == 0 || elementos[0] == null) return null;
            object valor = elementos[0]!.Valor;
            elementos[0] = elementos[contador - 1];
            contador--;
            Bajar(0);
            return valor;
        }

        public object? VerMaximo()
        {
            if (contador == 0 || elementos[0] == null) return null;
            return elementos[0]!.Valor;
        }

        private void Subir(int indice)
        {
            while (indice > 0)
            {
                int padre = (indice - 1) / 2;
                if (elementos[indice]!.Prioridad <= elementos[padre]!.Prioridad) break;
                Intercambiar(indice, padre);
                indice = padre;
            }
        }

        private void Bajar(int indice)
        {
            while (true)
            {
                int izq = 2 * indice + 1;
                int der = 2 * indice + 2;
                int mayor = indice;

                if (izq < contador && elementos[izq]!.Prioridad > elementos[mayor]!.Prioridad) mayor = izq;
                if (der < contador && elementos[der]!.Prioridad > elementos[mayor]!.Prioridad) mayor = der;

                if (mayor == indice) break;
                Intercambiar(indice, mayor);
                indice = mayor;
            }
        }

        private void Intercambiar(int i, int j)
        {
            var temp = elementos[i];
            elementos[i] = elementos[j];
            elementos[j] = temp;
        }

        private void Redimensionar()
        {
            capacidad *= 2;
            var nuevoArr = new ElementoHeap[capacidad];
            for (int i = 0; i < contador; i++) nuevoArr[i] = elementos[i];
            elementos = nuevoArr;
        }

        public int Conteo() => contador;

        private class ElementoHeap
        {
            public int Prioridad { get; set; }
            public object Valor { get; set; } = string.Empty;
        }
    }
}
