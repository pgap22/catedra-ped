using System;

namespace ProyectoCatedra.Modelos
{
    public class Beneficiario
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public int MiembrosHogar { get; set; }
        public bool Activo { get; set; }
        public DateTime FechaRegistro { get; set; } = DateTime.Now;

        public override string ToString()
        {
            return $"{Nombre} ({MiembrosHogar} pers.)";
        }
    }
}
