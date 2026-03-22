namespace ProyectoCatedra.Modelos
{
    public class UnidadMedida
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Tipo { get; set; } // Peso, Volumen, Unidad

        public override string ToString() => $"{Nombre} ({Tipo})";
    }
}
