using System;
using System.Data.SQLite;
using ProyectoCatedra.Datos;
using ProyectoCatedra.Modelos;
using ProyectoCatedra.Estructuras;

namespace ProyectoCatedra.Servicios
{
    public class BeneficiarioServicio
    {
        private ConexionDB conexionDB;
        public BeneficiarioServicio() { conexionDB = new ConexionDB(); }

        public void Guardar(Beneficiario b)
        {
            using (var conexion = conexionDB.ObtenerConexion())
            {
                conexion.Open();
                string sql = "INSERT INTO Beneficiarios (Nombre, MiembrosHogar, Activo) VALUES (@nombre, @miembros, @activo)";
                using (var comando = new SQLiteCommand(sql, conexion)) { comando.Parameters.AddWithValue("@nombre", b.Nombre); comando.Parameters.AddWithValue("@miembros", b.MiembrosHogar); comando.Parameters.AddWithValue("@activo", b.Activo ? 1 : 0); comando.ExecuteNonQuery(); }
            }
        }

        public void Actualizar(Beneficiario b)
        {
            using (var conexion = conexionDB.ObtenerConexion())
            {
                conexion.Open();
                string sql = "UPDATE Beneficiarios SET Nombre = @nombre, MiembrosHogar = @miembros, Activo = @activo WHERE Id = @id";
                using (var comando = new SQLiteCommand(sql, conexion)) { comando.Parameters.AddWithValue("@nombre", b.Nombre); comando.Parameters.AddWithValue("@miembros", b.MiembrosHogar); comando.Parameters.AddWithValue("@activo", b.Activo ? 1 : 0); comando.Parameters.AddWithValue("@id", b.Id); comando.ExecuteNonQuery(); }
            }
        }

        public void Eliminar(int id)
        {
            using (var conexion = conexionDB.ObtenerConexion())
            {
                conexion.Open();
                string sql = "DELETE FROM Beneficiarios WHERE Id = @id";
                using (var comando = new SQLiteCommand(sql, conexion)) { comando.Parameters.AddWithValue("@id", id); comando.ExecuteNonQuery(); }
            }
        }

        public ArbolBST CargarEnArbol()
        {
            ArbolBST arbol = new ArbolBST();
            using (var conexion = conexionDB.ObtenerConexion())
            {
                conexion.Open();
                string sql = "SELECT * FROM Beneficiarios";
                using (var comando = new SQLiteCommand(sql, conexion))
                {
                    using (var lector = comando.ExecuteReader())
                    {
                        while (lector.Read())
                            arbol.Insertar(lector["Nombre"].ToString(), new Beneficiario { Id = Convert.ToInt32(lector["Id"]), Nombre = lector["Nombre"].ToString(), MiembrosHogar = Convert.ToInt32(lector["MiembrosHogar"]), Activo = Convert.ToInt32(lector["Activo"]) == 1 });
                    }
                }
            }
            return arbol;
        }

        public ListaEnlazada ListarTodos()
        {
            ListaEnlazada lista = new ListaEnlazada();
            using (var conexion = conexionDB.ObtenerConexion())
            {
                conexion.Open();
                string sql = "SELECT * FROM Beneficiarios ORDER BY Nombre ASC";
                using (var comando = new SQLiteCommand(sql, conexion))
                {
                    using (var lector = comando.ExecuteReader())
                    {
                        while (lector.Read())
                            lista.Agregar(new Beneficiario { Id = Convert.ToInt32(lector["Id"]), Nombre = lector["Nombre"].ToString(), MiembrosHogar = Convert.ToInt32(lector["MiembrosHogar"]), Activo = Convert.ToInt32(lector["Activo"]) == 1 });
                    }
                }
            }
            return lista;
        }
    }
}
