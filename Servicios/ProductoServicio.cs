using System;
using System.Data.SQLite;
using ProyectoCatedra.Datos;
using ProyectoCatedra.Modelos;
using ProyectoCatedra.Estructuras;

namespace ProyectoCatedra.Servicios
{
    public class ProductoServicio
    {
        private ConexionDB conexionDB;
        public ProductoServicio() { conexionDB = new ConexionDB(); }

        public void GuardarOSumarStock(Producto p)
        {
            using (var conexion = conexionDB.ObtenerConexion())
            {
                conexion.Open();
                // Verificamos si SKU existe
                string check = "SELECT Stock FROM Productos WHERE SKU = @sku";
                using (var cmdCheck = new SQLiteCommand(check, conexion))
                {
                    cmdCheck.Parameters.AddWithValue("@sku", p.SKU);
                    var res = cmdCheck.ExecuteScalar();
                    if (res != null) // EXISTE -> SUMAMOS
                    {
                        string sqlSum = "UPDATE Productos SET Stock = Stock + @extra WHERE SKU = @sku";
                        using (var cmdSum = new SQLiteCommand(sqlSum, conexion)) { cmdSum.Parameters.AddWithValue("@extra", p.Stock); cmdSum.Parameters.AddWithValue("@sku", p.SKU); cmdSum.ExecuteNonQuery(); }
                    }
                    else // NO EXISTE -> INSERTAMOS
                    {
                        string sqlIns = "INSERT INTO Productos (SKU, Nombre, IdCategoria, Stock) VALUES (@sku, @nombre, @idCat, @stock)";
                        using (var cmdIns = new SQLiteCommand(sqlIns, conexion)) { cmdIns.Parameters.AddWithValue("@sku", p.SKU); cmdIns.Parameters.AddWithValue("@nombre", p.Nombre); cmdIns.Parameters.AddWithValue("@idCat", p.IdCategoria); cmdIns.Parameters.AddWithValue("@stock", p.Stock); cmdIns.ExecuteNonQuery(); }
                    }
                }
            }
        }

        public void Actualizar(Producto p)
        {
            using (var conexion = conexionDB.ObtenerConexion())
            {
                conexion.Open();
                string sql = "UPDATE Productos SET Nombre = @nombre, IdCategoria = @idCat, Stock = @stock WHERE SKU = @sku";
                using (var comando = new SQLiteCommand(sql, conexion)) { comando.Parameters.AddWithValue("@nombre", p.Nombre); comando.Parameters.AddWithValue("@idCat", p.IdCategoria); comando.Parameters.AddWithValue("@stock", p.Stock); comando.Parameters.AddWithValue("@sku", p.SKU); comando.ExecuteNonQuery(); }
            }
        }

        public void Eliminar(string sku)
        {
            using (var conexion = conexionDB.ObtenerConexion())
            {
                conexion.Open();
                string sql = "DELETE FROM Productos WHERE SKU = @sku";
                using (var comando = new SQLiteCommand(sql, conexion)) { comando.Parameters.AddWithValue("@sku", sku); comando.ExecuteNonQuery(); }
            }
        }

        public ListaEnlazada ListarTodos()
        {
            ListaEnlazada lista = new ListaEnlazada();
            using (var conexion = conexionDB.ObtenerConexion())
            {
                conexion.Open();
                string sql = "SELECT p.*, c.Nombre as CatNombre FROM Productos p LEFT JOIN Categorias c ON p.IdCategoria = c.Id";
                using (var comando = new SQLiteCommand(sql, conexion))
                {
                    using (var lector = comando.ExecuteReader())
                    {
                        while (lector.Read())
                            lista.Agregar(new Producto { SKU = lector["SKU"].ToString(), Nombre = lector["Nombre"].ToString(), IdCategoria = Convert.ToInt32(lector["IdCategoria"]), Stock = Convert.ToDouble(lector["Stock"]), NombreCategoria = lector["CatNombre"]?.ToString() });
                    }
                }
            }
            return lista;
        }

        public TablaHash CargarEnHash()
        {
            TablaHash hash = new TablaHash();
            var l = ListarTodos();
            for (int i = 0; i < l.Conteo(); i++) { var p = (Producto)l.Obtener(i); hash.Insertar(p.SKU, p); }
            return hash;
        }
    }
}
