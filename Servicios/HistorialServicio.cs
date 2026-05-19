using System;
using System.Data.SQLite;
using ProyectoCatedra.Datos;
using ProyectoCatedra.Estructuras;

namespace ProyectoCatedra.Servicios
{
    public class HistorialServicio
    {
        private ConexionDB conexionDB;

        public HistorialServicio()
        {
            conexionDB = new ConexionDB();
        }

        public ListaEnlazada ObtenerHistorialDistribuciones()
        {
            ListaEnlazada historial = new ListaEnlazada();
            using (var conexion = conexionDB.ObtenerConexion())
            {
                conexion.Open();
                string sql = @"
                    SELECT 
                        o.Id AS OrdenId, 
                        o.FechaGeneracion, 
                        b.Nombre AS Beneficiario,
                        c.Nombre AS Categoria,
                        p.Nombre AS Producto,
                        od.CantidadAsignada,
                        u.Nombre AS Unidad,
                        od.DeficitCalculado
                    FROM Orden o
                    INNER JOIN OrdenDetalle od ON o.Id = od.OrdenId
                    INNER JOIN Beneficiarios b ON od.BeneficiarioId = b.Id
                    INNER JOIN Categorias c ON od.CategoriaId = c.Id
                    LEFT JOIN Productos p ON od.ProductoId = p.Id
                    INNER JOIN TasaConsumo t ON c.Id = t.IdCategoria
                    INNER JOIN UnidadesMedida u ON t.IdUnidadBase = u.Id
                    WHERE o.Estado = 'CONFIRMADA'
                    ORDER BY o.FechaGeneracion DESC, o.Id DESC, b.Nombre ASC";

                using (var cmd = new SQLiteCommand(sql, conexion))
                using (var lector = cmd.ExecuteReader())
                {
                    while (lector.Read())
                    {
                        var registro = new RegistroHistorial
                        {
                            OrdenId = Convert.ToInt32(lector["OrdenId"]),
                            Fecha = Convert.ToDateTime(lector["FechaGeneracion"]),
                            Beneficiario = lector["Beneficiario"].ToString() ?? "",
                            Categoria = lector["Categoria"].ToString() ?? "",
                            Producto = lector["Producto"] == DBNull.Value ? "" : lector["Producto"].ToString() ?? "",
                            CantidadAsignada = Convert.ToDouble(lector["CantidadAsignada"]),
                            Unidad = lector["Unidad"].ToString() ?? "",
                            DeficitCalculado = Convert.ToDouble(lector["DeficitCalculado"])
                        };
                        historial.Agregar(registro);
                    }
                }
            }
            return historial;
        }

        public DateTime? ObtenerUltimaEntregaProducto(int beneficiarioId, int productoId)
        {
            using (var conexion = conexionDB.ObtenerConexion())
            {
                conexion.Open();
                string sql = @"
                    SELECT MAX(o.FechaGeneracion)
                    FROM OrdenDetalle od
                    INNER JOIN Orden o ON od.OrdenId = o.Id
                    WHERE o.Estado = 'CONFIRMADA'
                      AND od.BeneficiarioId = @beneficiarioId
                      AND od.ProductoId = @productoId";

                using (var cmd = new SQLiteCommand(sql, conexion))
                {
                    cmd.Parameters.AddWithValue("@beneficiarioId", beneficiarioId);
                    cmd.Parameters.AddWithValue("@productoId", productoId);
                    var resultado = cmd.ExecuteScalar();
                    if (resultado == null || resultado == DBNull.Value) return null;
                    return Convert.ToDateTime(resultado);
                }
            }
        }

        public DateTime? ObtenerUltimaEntregaCategoria(int beneficiarioId, int categoriaId)
        {
            using (var conexion = conexionDB.ObtenerConexion())
            {
                conexion.Open();
                string sql = @"
                    SELECT MAX(o.FechaGeneracion)
                    FROM OrdenDetalle od
                    INNER JOIN Orden o ON od.OrdenId = o.Id
                    WHERE o.Estado = 'CONFIRMADA'
                      AND od.BeneficiarioId = @beneficiarioId
                      AND od.CategoriaId = @categoriaId";

                using (var cmd = new SQLiteCommand(sql, conexion))
                {
                    cmd.Parameters.AddWithValue("@beneficiarioId", beneficiarioId);
                    cmd.Parameters.AddWithValue("@categoriaId", categoriaId);
                    var resultado = cmd.ExecuteScalar();
                    if (resultado == null || resultado == DBNull.Value) return null;
                    return Convert.ToDateTime(resultado);
                }
            }
        }
    }

    public class RegistroHistorial
    {
        public int OrdenId { get; set; }
        public DateTime Fecha { get; set; }
        public string Beneficiario { get; set; } = string.Empty;
        public string Categoria { get; set; } = string.Empty;
        public string Producto { get; set; } = string.Empty;
        public double CantidadAsignada { get; set; }
        public string Unidad { get; set; } = string.Empty;
        public double DeficitCalculado { get; set; }
    }
}
