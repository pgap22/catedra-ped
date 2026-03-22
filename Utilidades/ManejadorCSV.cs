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

        public static ListaEnlazada ParsearCategorias(string ruta)
        {
            ListaEnlazada lista = new ListaEnlazada();
            if (!File.Exists(ruta)) return lista;
            string[] lineas = File.ReadAllLines(ruta);
            for (int i = 1; i < lineas.Length; i++) // Saltamos encabezado
            {
                if (!string.IsNullOrWhiteSpace(lineas[i]))
                    lista.Agregar(new Categoria { Nombre = lineas[i].Trim() });
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
                if (p.Length >= 2)
                    lista.Agregar(new Beneficiario { Nombre = p[0].Trim(), MiembrosHogar = int.TryParse(p[1], out int m) ? m : 1, Activo = true });
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
                if (p.Length >= 4)
                    lista.Agregar(new Producto { SKU = p[0].Trim(), Nombre = p[1].Trim(), NombreCategoria = p[2].Trim(), Stock = double.TryParse(p[3], out double s) ? s : 0 });
            }
            return lista;
        }
    }
}
