namespace ProyectoCatedra.Modelos
{
    public class UnidadMedida
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty; // Peso, Volumen, Unidad

        public override string ToString() => $"{Nombre} ({Tipo})";
    }
}
