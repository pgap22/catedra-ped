using System;
using System.Data.SQLite;
using ProyectoCatedra.Datos;
using ProyectoCatedra.Estructuras;
using ProyectoCatedra.Modelos;

namespace ProyectoCatedra.Servicios
{
    public class ProductoServicio
    {
        private ConexionDB conexionDB;
        private readonly TablaHash indicePorSku;

        public ProductoServicio()
        {
            conexionDB = new ConexionDB();
            indicePorSku = new TablaHash(401);
            CargarDesdeBase();
        }

        public void GuardarOSumarStock(Producto p)
        {
            Producto? existente = (Producto?)indicePorSku.Buscar(p.SKU);
            if (existente != null)
            {
                double stockAnterior = existente.Stock;
                existente.Stock += p.Stock;

                try
                {
                    using (var conexion = conexionDB.ObtenerConexion())
                    {
                        conexion.Open();
                        string sqlSum = "UPDATE Productos SET Stock = Stock + @extra WHERE SKU = @sku";
                        using (var cmdSum = new SQLiteCommand(sqlSum, conexion))
                        {
                            cmdSum.Parameters.AddWithValue("@extra", p.Stock);
                            cmdSum.Parameters.AddWithValue("@sku", p.SKU);
                            cmdSum.ExecuteNonQuery();
                        }
                    }
                }
                catch
                {
                    existente.Stock = stockAnterior;
                    throw;
                }

                return;
            }

            var nuevo = new Producto
            {
                SKU = p.SKU,
                Nombre = p.Nombre,
                IdCategoria = p.IdCategoria,
                Stock = p.Stock,
                MaximoPorEntrega = p.MaximoPorEntrega,
                DiasReposicion = p.DiasReposicion,
                NombreCategoria = p.NombreCategoria
            };

            indicePorSku.Insertar(nuevo.SKU, nuevo);

            try
            {
                using (var conexion = conexionDB.ObtenerConexion())
                {
                    conexion.Open();
                    string sqlIns = @"INSERT INTO Productos (SKU, Nombre, IdCategoria, Stock, MaximoPorEntrega, DiasReposicion)
                                      VALUES (@sku, @nombre, @idCat, @stock, @max, @dias);
                                      SELECT last_insert_rowid();";
                    using (var cmdIns = new SQLiteCommand(sqlIns, conexion))
                    {
                        cmdIns.Parameters.AddWithValue("@sku", nuevo.SKU);
                        cmdIns.Parameters.AddWithValue("@nombre", nuevo.Nombre);
                        cmdIns.Parameters.AddWithValue("@idCat", nuevo.IdCategoria);
                        cmdIns.Parameters.AddWithValue("@stock", nuevo.Stock);
                        cmdIns.Parameters.AddWithValue("@max", (object?)nuevo.MaximoPorEntrega ?? DBNull.Value);
                        cmdIns.Parameters.AddWithValue("@dias", (object?)nuevo.DiasReposicion ?? DBNull.Value);
                        nuevo.Id = Convert.ToInt32((long)cmdIns.ExecuteScalar());
                    }
                }
            }
            catch
            {
                indicePorSku.Eliminar(nuevo.SKU);
                throw;
            }
        }

        public void Actualizar(Producto p)
        {
            Producto? existente = (Producto?)indicePorSku.Buscar(p.SKU);
            if (existente == null) return;

            var respaldo = new Producto
            {
                SKU = existente.SKU,
                Nombre = existente.Nombre,
                IdCategoria = existente.IdCategoria,
                Stock = existente.Stock,
                MaximoPorEntrega = existente.MaximoPorEntrega,
                DiasReposicion = existente.DiasReposicion,
                NombreCategoria = existente.NombreCategoria
            };

            existente.Nombre = p.Nombre;
            existente.IdCategoria = p.IdCategoria;
            existente.Stock = p.Stock;
            existente.MaximoPorEntrega = p.MaximoPorEntrega;
            existente.DiasReposicion = p.DiasReposicion;
            existente.NombreCategoria = p.NombreCategoria;

            try
            {
                using (var conexion = conexionDB.ObtenerConexion())
                {
                    conexion.Open();
                    string sql = @"UPDATE Productos
                                   SET Nombre = @nombre,
                                       IdCategoria = @idCat,
                                       Stock = @stock,
                                       MaximoPorEntrega = @max,
                                       DiasReposicion = @dias
                                   WHERE SKU = @sku";
                    using (var comando = new SQLiteCommand(sql, conexion))
                    {
                        comando.Parameters.AddWithValue("@nombre", p.Nombre);
                        comando.Parameters.AddWithValue("@idCat", p.IdCategoria);
                        comando.Parameters.AddWithValue("@stock", p.Stock);
                        comando.Parameters.AddWithValue("@max", (object?)p.MaximoPorEntrega ?? DBNull.Value);
                        comando.Parameters.AddWithValue("@dias", (object?)p.DiasReposicion ?? DBNull.Value);
                        comando.Parameters.AddWithValue("@sku", p.SKU);
                        comando.ExecuteNonQuery();
                    }
                }
            }
            catch
            {
                existente.Nombre = respaldo.Nombre;
                existente.IdCategoria = respaldo.IdCategoria;
                existente.Stock = respaldo.Stock;
                existente.MaximoPorEntrega = respaldo.MaximoPorEntrega;
                existente.DiasReposicion = respaldo.DiasReposicion;
                existente.NombreCategoria = respaldo.NombreCategoria;
                throw;
            }
        }

        public void Eliminar(string sku)
        {
            Producto? existente = (Producto?)indicePorSku.Buscar(sku);
            if (existente == null) return;

            indicePorSku.Eliminar(sku);

            try
            {
                using (var conexion = conexionDB.ObtenerConexion())
                {
                    conexion.Open();
                    string sql = "DELETE FROM Productos WHERE SKU = @sku";
                    using (var comando = new SQLiteCommand(sql, conexion))
                    {
                        comando.Parameters.AddWithValue("@sku", sku);
                        comando.ExecuteNonQuery();
                    }
                }
            }
            catch
            {
                indicePorSku.Insertar(existente.SKU, existente);
                throw;
            }
        }

        public ListaEnlazada ListarTodos()
        {
            ListaEnlazada lista = new ListaEnlazada();
            indicePorSku.ParaCada((_, valor) =>
            {
                var p = (Producto)valor;
                if (string.IsNullOrWhiteSpace(p.NombreCategoria))
                {
                    p.NombreCategoria = ObtenerNombreCategoria(p.IdCategoria);
                }
                lista.Agregar(p);
            });
            return lista;
        }

        public TablaHash CargarEnHash()
        {
            return indicePorSku;
        }

        public ListaEnlazada ListarPorCategoria(int idCategoria)
        {
            ListaEnlazada lista = new ListaEnlazada();
            var todos = ListarTodos();
            for (int i = 0; i < todos.Conteo(); i++)
            {
                var p = (Producto?)todos.Obtener(i);
                if (p != null && p.IdCategoria == idCategoria) lista.Agregar(p);
            }
            return lista;
        }

        public double ObtenerStockProducto(int productoId)
        {
            using (var conexion = conexionDB.ObtenerConexion())
            {
                conexion.Open();
                string sql = "SELECT Stock FROM Productos WHERE Id = @id";
                using (var comando = new SQLiteCommand(sql, conexion))
                {
                    comando.Parameters.AddWithValue("@id", productoId);
                    var resultado = comando.ExecuteScalar();
                    return resultado == null || resultado == DBNull.Value ? 0 : Convert.ToDouble(resultado);
                }
            }
        }

        private void CargarDesdeBase()
        {
            using (var conexion = conexionDB.ObtenerConexion())
            {
                conexion.Open();
                string sql = "SELECT p.*, c.Nombre as CatNombre FROM Productos p LEFT JOIN Categorias c ON p.IdCategoria = c.Id";
                using (var comando = new SQLiteCommand(sql, conexion))
                using (var lector = comando.ExecuteReader())
                {
                    while (lector.Read())
                    {
                        var producto = new Producto
                        {
                            Id = Convert.ToInt32(lector["Id"]),
                            SKU = lector["SKU"]?.ToString() ?? string.Empty,
                            Nombre = lector["Nombre"]?.ToString() ?? string.Empty,
                            IdCategoria = Convert.ToInt32(lector["IdCategoria"]),
                            Stock = Convert.ToDouble(lector["Stock"]),
                            MaximoPorEntrega = lector["MaximoPorEntrega"] == DBNull.Value ? null : Convert.ToDouble(lector["MaximoPorEntrega"]),
                            DiasReposicion = lector["DiasReposicion"] == DBNull.Value ? null : Convert.ToInt32(lector["DiasReposicion"]),
                            NombreCategoria = lector["CatNombre"]?.ToString() ?? string.Empty
                        };

                        indicePorSku.Insertar(producto.SKU, producto);
                    }
                }
            }
        }

        private string ObtenerNombreCategoria(int idCategoria)
        {
            using (var conexion = conexionDB.ObtenerConexion())
            {
                conexion.Open();
                string sql = "SELECT Nombre FROM Categorias WHERE Id = @id";
                using (var comando = new SQLiteCommand(sql, conexion))
                {
                    comando.Parameters.AddWithValue("@id", idCategoria);
                    var resultado = comando.ExecuteScalar();
                    return resultado?.ToString() ?? string.Empty;
                }
            }
        }
    }
}
