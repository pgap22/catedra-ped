using System;

namespace ProyectoCatedra.Modelos
{
    public class Orden
    {
        public int Id { get; set; }
        public DateTime FechaGeneracion { get; set; }
        public string Estado { get; set; } = "BORRADOR";
        public string Observaciones { get; set; } = "";
    }
}