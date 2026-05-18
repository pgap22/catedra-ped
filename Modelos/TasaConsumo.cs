namespace ProyectoCatedra.Modelos
{
    public class TasaConsumo
    {
        public int IdCategoria { get; set; }
        public string NombreCategoria { get; set; } = string.Empty;
        public double TasaDiaria { get; set; }
        public int IdUnidadBase { get; set; }
        public string NombreUnidadBase { get; set; } = string.Empty;
    }
}