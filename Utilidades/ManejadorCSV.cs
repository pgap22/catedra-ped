using System;
using System.IO;
using System.Windows.Forms;
using ProyectoCatedra.Estructuras;
using ProyectoCatedra.Modelos;

namespace ProyectoCatedra.Utilidades
{
    public class ManejadorCSV
    {
        public static void GuardarPlantillaConDialogo(string nombreSugerido, string contenido)
        {
            SaveFileDialog sfd = new SaveFileDialog
            {
                Filter = "Archivo CSV (*.csv)|*.csv",
                FileName = nombreSugerido
            };

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                File.WriteAllText(sfd.FileName, contenido);
                MessageBox.Show("Plantilla guardada con éxito en: " + sfd.FileName);
            }
        }

        public static int ContarFilasDatos(string ruta)
        {
            if (!File.Exists(ruta)) return 0;
            string[] lineas = File.ReadAllLines(ruta);
            int total = 0;
            for (int i = 1; i < lineas.Length; i++)
            {
                if (!string.IsNullOrWhiteSpace(lineas[i])) total++;
            }
            return total;
        }

        public static ListaEnlazada ParsearCategorias(string ruta)
        {
            ListaEnlazada lista = new ListaEnlazada();
            if (!File.Exists(ruta)) return lista;
            string[] lineas = File.ReadAllLines(ruta);
            for (int i = 1; i < lineas.Length; i++)
            {
                string nombre = lineas[i].Trim();
                if (!string.IsNullOrWhiteSpace(nombre)) lista.Agregar(new Categoria { Nombre = nombre });
            }
            return lista;
        }

        public static ListaEnlazada ParsearBeneficiarios(string ruta)
        {
            ListaEnlazada lista = new ListaEnlazada();
            if (!File.Exists(ruta)) return lista;
            string[] lineas = File.ReadAllLines(ruta);
            for (int i = 1; i < lineas.Length; i++)
            {
                string[] p = lineas[i].Split(',');
                if (p.Length < 2 || string.IsNullOrWhiteSpace(p[0])) continue;

                int miembros = int.TryParse(p[1], out int m) && m > 0 ? m : 1;
                lista.Agregar(new Beneficiario
                {
                    Nombre = p[0].Trim(),
                    MiembrosHogar = miembros,
                    NivelVulnerabilidad = p.Length >= 3 ? Beneficiario.ParsearNivelVulnerabilidad(p[2]) : Beneficiario.VulnerabilidadMedia,
                    Activo = true
                });
            }
            return lista;
        }

        public static ListaEnlazada ParsearProductos(string ruta)
        {
            ListaEnlazada lista = new ListaEnlazada();
            if (!File.Exists(ruta)) return lista;
            string[] lineas = File.ReadAllLines(ruta);
            for (int i = 1; i < lineas.Length; i++)
            {
                string[] p = lineas[i].Split(',');
                if (p.Length < 4) continue;
                if (string.IsNullOrWhiteSpace(p[0]) || string.IsNullOrWhiteSpace(p[1]) || string.IsNullOrWhiteSpace(p[2])) continue;
                if (!double.TryParse(p[3], out double stock) || stock < 0) continue;

                lista.Agregar(new Producto
                {
                    SKU = p[0].Trim(),
                    Nombre = p[1].Trim(),
                    NombreCategoria = p[2].Trim(),
                    Stock = stock
                });
            }
            return lista;
        }

        public static ListaEnlazada ParsearUnidades(string ruta)
        {
            ListaEnlazada lista = new ListaEnlazada();
            if (!File.Exists(ruta)) return lista;
            string[] lineas = File.ReadAllLines(ruta);
            for (int i = 1; i < lineas.Length; i++)
            {
                string[] p = lineas[i].Split(',');
                if (p.Length < 2) continue;
                if (string.IsNullOrWhiteSpace(p[0]) || string.IsNullOrWhiteSpace(p[1])) continue;

                lista.Agregar(new UnidadMedida { Nombre = p[0].Trim(), Tipo = p[1].Trim() });
            }
            return lista;
        }
    }
}
