using System;
using System.Data.SQLite;
using ProyectoCatedra.Datos;
using ProyectoCatedra.Estructuras;
using ProyectoCatedra.Modelos;

namespace ProyectoCatedra.Servicios
{
    public class CategoriaPackServicio
    {
        private readonly ConexionDB conexionDB;

        public CategoriaPackServicio()
        {
            conexionDB = new ConexionDB();
        }

        public ListaEnlazada ListarPorCategoria(int categoriaId)
        {
            ListaEnlazada lista = new ListaEnlazada();
            using (var conexion = conexionDB.ObtenerConexion())
            {
                conexion.Open();
                string sql = @"
                    SELECT cpd.Id, cpd.CategoriaId, c.Nombre AS Categoria,
                           p.Id AS ProductoId, p.SKU, p.Nombre AS Producto,
                           cpd.Porcentaje, p.Stock, p.MaximoPorEntrega, p.DiasReposicion
                    FROM CategoriaPackDetalle cpd
                    INNER JOIN Categorias c ON cpd.CategoriaId = c.Id
                    INNER JOIN Productos p ON cpd.ProductoId = p.Id
                    WHERE cpd.CategoriaId = @categoriaId
                    ORDER BY p.Nombre ASC";

                using (var cmd = new SQLiteCommand(sql, conexion))
                {
                    cmd.Parameters.AddWithValue("@categoriaId", categoriaId);
                    using (var lector = cmd.ExecuteReader())
                    {
                        while (lector.Read())
                        {
                            lista.Agregar(new CategoriaPackDetalle
                            {
                                Id = Convert.ToInt32(lector["Id"]),
                                CategoriaId = Convert.ToInt32(lector["CategoriaId"]),
                                NombreCategoria = lector["Categoria"].ToString() ?? string.Empty,
                                ProductoId = Convert.ToInt32(lector["ProductoId"]),
                                SKUProducto = lector["SKU"].ToString() ?? string.Empty,
                                NombreProducto = lector["Producto"].ToString() ?? string.Empty,
                                Porcentaje = Convert.ToDouble(lector["Porcentaje"]),
                                StockDisponible = Convert.ToDouble(lector["Stock"]),
                                MaximoPorEntrega = lector["MaximoPorEntrega"] == DBNull.Value ? null : Convert.ToDouble(lector["MaximoPorEntrega"]),
                                DiasReposicion = lector["DiasReposicion"] == DBNull.Value ? null : Convert.ToInt32(lector["DiasReposicion"])
                            });
                        }
                    }
                }
            }
            return lista;
        }

        public double ObtenerTotalPorcentaje(int categoriaId)
        {
            double total = 0;
            var lineas = ListarPorCategoria(categoriaId);
            for (int i = 0; i < lineas.Conteo(); i++)
            {
                var linea = (CategoriaPackDetalle)lineas.Obtener(i)!;
                total += linea.Porcentaje;
            }
            return total;
        }

        public bool EsPackValido(int categoriaId)
        {
            return Math.Abs(ObtenerTotalPorcentaje(categoriaId) - 100) < 0.001;
        }

        public void GuardarPack(int categoriaId, ListaEnlazada lineas)
        {
            double total = 0;
            for (int i = 0; i < lineas.Conteo(); i++)
            {
                var linea = (CategoriaPackDetalle)lineas.Obtener(i)!;
                if (linea.Porcentaje < 0) throw new InvalidOperationException("Los porcentajes no pueden ser negativos.");
                total += linea.Porcentaje;
            }

            if (Math.Abs(total - 100) > 0.001)
            {
                throw new InvalidOperationException($"El pack debe sumar exactamente 100%. Total actual: {total}%.");
            }

            using (var conexion = conexionDB.ObtenerConexion())
            {
                conexion.Open();
                using (var tr = conexion.BeginTransaction())
                {
                    try
                    {
                        using (var cmdDel = new SQLiteCommand("DELETE FROM CategoriaPackDetalle WHERE CategoriaId = @cat", conexion, tr))
                        {
                            cmdDel.Parameters.AddWithValue("@cat", categoriaId);
                            cmdDel.ExecuteNonQuery();
                        }

                        string sqlIns = @"INSERT INTO CategoriaPackDetalle (CategoriaId, ProductoId, Porcentaje)
                                          VALUES (@cat, @prod, @porcentaje)";
                        for (int i = 0; i < lineas.Conteo(); i++)
                        {
                            var linea = (CategoriaPackDetalle)lineas.Obtener(i)!;
                            if (linea.Porcentaje <= 0) continue;

                            using (var cmdIns = new SQLiteCommand(sqlIns, conexion, tr))
                            {
                                cmdIns.Parameters.AddWithValue("@cat", categoriaId);
                                cmdIns.Parameters.AddWithValue("@prod", linea.ProductoId);
                                cmdIns.Parameters.AddWithValue("@porcentaje", linea.Porcentaje);
                                cmdIns.ExecuteNonQuery();
                            }
                        }

                        tr.Commit();
                    }
                    catch
                    {
                        tr.Rollback();
                        throw;
                    }
                }
            }
        }

        public void EliminarPack(int categoriaId)
        {
            using (var conexion = conexionDB.ObtenerConexion())
            {
                conexion.Open();
                using (var cmd = new SQLiteCommand("DELETE FROM CategoriaPackDetalle WHERE CategoriaId = @cat", conexion))
                {
                    cmd.Parameters.AddWithValue("@cat", categoriaId);
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}
