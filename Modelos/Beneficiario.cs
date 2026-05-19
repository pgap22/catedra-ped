using System;

namespace ProyectoCatedra.Modelos
{
    public class Beneficiario
    {
        public const int VulnerabilidadBaja = 1;
        public const int VulnerabilidadMedia = 2;
        public const int VulnerabilidadAlta = 3;

        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public int MiembrosHogar { get; set; }
        public bool Activo { get; set; }
        public DateTime FechaRegistro { get; set; } = DateTime.Now;
        public int NivelVulnerabilidad { get; set; } = VulnerabilidadMedia;

        public string VulnerabilidadTexto => ObtenerEtiquetaVulnerabilidad(NivelVulnerabilidad);

        public static int NormalizarNivelVulnerabilidad(int nivel)
        {
            if (nivel < VulnerabilidadBaja || nivel > VulnerabilidadAlta) return VulnerabilidadMedia;
            return nivel;
        }

        public static int ParsearNivelVulnerabilidad(string? valor)
        {
            if (string.IsNullOrWhiteSpace(valor)) return VulnerabilidadMedia;
            string texto = valor.Trim().ToLowerInvariant();

            if (int.TryParse(texto, out int nivel)) return NormalizarNivelVulnerabilidad(nivel);
            if (texto.Contains("alta")) return VulnerabilidadAlta;
            if (texto.Contains("baja")) return VulnerabilidadBaja;
            if (texto.Contains("media")) return VulnerabilidadMedia;

            return VulnerabilidadMedia;
        }

        public static string ObtenerEtiquetaVulnerabilidad(int nivel)
        {
            int nivelNormalizado = NormalizarNivelVulnerabilidad(nivel);
            if (nivelNormalizado == VulnerabilidadAlta) return "Alta - Atención prioritaria";
            if (nivelNormalizado == VulnerabilidadBaja) return "Baja - Apoyo preventivo";
            return "Media - Apoyo regular";
        }

        public override string ToString()
        {
            return $"{Nombre} ({MiembrosHogar} pers.)";
        }
    }
}
