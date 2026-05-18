using System;
using System.Data.SQLite;
using ProyectoCatedra.Datos;
using ProyectoCatedra.Modelos;
using ProyectoCatedra.Estructuras;

namespace ProyectoCatedra.Servicios
{
    public class TasaConsumoServicio
    {
        private ConexionDB conexionDB;

        public TasaConsumoServicio()
        {
            conexionDB = new ConexionDB();
        }

        public ListaEnlazada ListarTodas()
        {
            ListaEnlazada lista = new ListaEnlazada();
            using (var conexion = conexionDB.ObtenerConexion())
            {
                conexion.Open();
                string sql = @"
                    SELECT t.IdCategoria, c.Nombre as NombreCategoria, t.TasaDiaria, t.IdUnidadBase, u.Nombre as NombreUnidadBase 
                    FROM TasaConsumo t
                    INNER JOIN Categorias c ON t.IdCategoria = c.Id
                    INNER JOIN UnidadesMedida u ON t.IdUnidadBase = u.Id";
                    
                using (var cmd = new SQLiteCommand(sql, conexion))
                {
                    using (var lector = cmd.ExecuteReader())
                    {
                        while (lector.Read())
                        {
                            lista.Agregar(new TasaConsumo
                            {
                                IdCategoria = Convert.ToInt32(lector["IdCategoria"]),
                                NombreCategoria = lector["NombreCategoria"].ToString() ?? "",
                                TasaDiaria = Convert.ToDouble(lector["TasaDiaria"]),
                                IdUnidadBase = Convert.ToInt32(lector["IdUnidadBase"]),
                                NombreUnidadBase = lector["NombreUnidadBase"].ToString() ?? ""
                            });
                        }
                    }
                }
            }
            return lista;
        }

        public void Guardar(TasaConsumo tasa)
        {
            using (var conexion = conexionDB.ObtenerConexion())
            {
                conexion.Open();
                string sql = @"
                    INSERT INTO TasaConsumo (IdCategoria, TasaDiaria, IdUnidadBase) 
                    VALUES (@idCat, @tasa, @idUni)
                    ON CONFLICT(IdCategoria) DO UPDATE SET 
                        TasaDiaria = excluded.TasaDiaria, 
                        IdUnidadBase = excluded.IdUnidadBase;";
                        
                using (var cmd = new SQLiteCommand(sql, conexion))
                {
                    cmd.Parameters.AddWithValue("@idCat", tasa.IdCategoria);
                    cmd.Parameters.AddWithValue("@tasa", tasa.TasaDiaria);
                    cmd.Parameters.AddWithValue("@idUni", tasa.IdUnidadBase);
                    cmd.ExecuteNonQuery();
                }
            }
        }
        
        public TasaConsumo? ObtenerPorCategoria(int idCategoria)
        {
            using (var conexion = conexionDB.ObtenerConexion())
            {
                conexion.Open();
                string sql = @"
                    SELECT t.IdCategoria, c.Nombre as NombreCategoria, t.TasaDiaria, t.IdUnidadBase, u.Nombre as NombreUnidadBase 
                    FROM TasaConsumo t
                    INNER JOIN Categorias c ON t.IdCategoria = c.Id
                    INNER JOIN UnidadesMedida u ON t.IdUnidadBase = u.Id
                    WHERE t.IdCategoria = @id";
                    
                using (var cmd = new SQLiteCommand(sql, conexion))
                {
                    cmd.Parameters.AddWithValue("@id", idCategoria);
                    using (var lector = cmd.ExecuteReader())
                    {
                        if (lector.Read())
                        {
                            return new TasaConsumo
                            {
                                IdCategoria = Convert.ToInt32(lector["IdCategoria"]),
                                NombreCategoria = lector["NombreCategoria"].ToString() ?? "",
                                TasaDiaria = Convert.ToDouble(lector["TasaDiaria"]),
                                IdUnidadBase = Convert.ToInt32(lector["IdUnidadBase"]),
                                NombreUnidadBase = lector["NombreUnidadBase"].ToString() ?? ""
                            };
                        }
                    }
                }
            }
            return null;
        }
        
        public void Eliminar(int idCategoria)
        {
            using (var conexion = conexionDB.ObtenerConexion())
            {
                conexion.Open();
                string sql = "DELETE FROM TasaConsumo WHERE IdCategoria = @id";
                using (var cmd = new SQLiteCommand(sql, conexion))
                {
                    cmd.Parameters.AddWithValue("@id", idCategoria);
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}