using System;
using System.Data.SQLite;
using ProyectoCatedra.Datos;
using ProyectoCatedra.Modelos;
using ProyectoCatedra.Estructuras;

namespace ProyectoCatedra.Servicios
{
    public class UnidadServicio
    {
        private ConexionDB conexionDB;
        public UnidadServicio() { conexionDB = new ConexionDB(); }

        public void Guardar(UnidadMedida u)
        {
            using (var conexion = conexionDB.ObtenerConexion())
            {
                conexion.Open();
                string sql = "INSERT INTO UnidadesMedida (Nombre, Tipo) VALUES (@nom, @tipo)";
                using (var cmd = new SQLiteCommand(sql, conexion)) { cmd.Parameters.AddWithValue("@nom", u.Nombre); cmd.Parameters.AddWithValue("@tipo", u.Tipo); cmd.ExecuteNonQuery(); }
            }
        }

        public ListaEnlazada ListarTodas()
        {
            ListaEnlazada lista = new ListaEnlazada();
            using (var conexion = conexionDB.ObtenerConexion())
            {
                conexion.Open();
                string sql = "SELECT * FROM UnidadesMedida ORDER BY Nombre ASC";
                using (var cmd = new SQLiteCommand(sql, conexion)) { using (var lector = cmd.ExecuteReader()) { while (lector.Read()) lista.Agregar(new UnidadMedida { Id = Convert.ToInt32(lector["Id"]), Nombre = lector["Nombre"].ToString(), Tipo = lector["Tipo"].ToString() }); } }
            }
            return lista;
        }

        public void AsociarACategoria(int idCat, int idUni)
        {
            using (var conexion = conexionDB.ObtenerConexion())
            {
                conexion.Open();
                string sql = "INSERT OR IGNORE INTO CategoriaUnidades (IdCategoria, IdUnidad) VALUES (@cat, @uni)";
                using (var cmd = new SQLiteCommand(sql, conexion)) { cmd.Parameters.AddWithValue("@cat", idCat); cmd.Parameters.AddWithValue("@uni", idUni); cmd.ExecuteNonQuery(); }
            }
        }

        public ListaEnlazada ListarPorCategoria(int idCat)
        {
            ListaEnlazada lista = new ListaEnlazada();
            using (var conexion = conexionDB.ObtenerConexion())
            {
                conexion.Open();
                string sql = @"SELECT u.* FROM UnidadesMedida u 
                               JOIN CategoriaUnidades cu ON u.Id = cu.IdUnidad 
                               WHERE cu.IdCategoria = @cat";
                using (var cmd = new SQLiteCommand(sql, conexion))
                {
                    cmd.Parameters.AddWithValue("@cat", idCat);
                    using (var lector = cmd.ExecuteReader()) { while (lector.Read()) lista.Agregar(new UnidadMedida { Id = Convert.ToInt32(lector["Id"]), Nombre = lector["Nombre"].ToString(), Tipo = lector["Tipo"].ToString() }); }
                }
            }
            return lista;
        }

        public void EliminarAsociacion(int idCat, int idUni)
        {
            using (var conexion = conexionDB.ObtenerConexion())
            {
                conexion.Open();
                string sql = "DELETE FROM CategoriaUnidades WHERE IdCategoria = @cat AND IdUnidad = @uni";
                using (var cmd = new SQLiteCommand(sql, conexion)) { cmd.Parameters.AddWithValue("@cat", idCat); cmd.Parameters.AddWithValue("@uni", idUni); cmd.ExecuteNonQuery(); }
            }
        }

        public void EliminarUnidad(int id)
        {
            using (var conexion = conexionDB.ObtenerConexion())
            {
                conexion.Open();
                string sql = "DELETE FROM UnidadesMedida WHERE Id = @id";
                using (var cmd = new SQLiteCommand(sql, conexion)) { cmd.Parameters.AddWithValue("@id", id); cmd.ExecuteNonQuery(); }
            }
        }
    }
}
