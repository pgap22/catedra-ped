using System;
using System.Data.SQLite;
using ProyectoCatedra.Datos;
using ProyectoCatedra.Modelos;
using ProyectoCatedra.Estructuras;

namespace ProyectoCatedra.Servicios
{
    public class CategoriaServicio
    {
        private ConexionDB conexionDB;
        public CategoriaServicio() { conexionDB = new ConexionDB(); }

        public bool ExisteNombre(string nombre)
        {
            var hash = new TablaHash();
            var todas = ListarTodas();
            for (int i = 0; i < todas.Conteo(); i++) { var c = (Categoria)todas.Obtener(i); hash.Insertar(c.Nombre.ToUpper(), c); }
            return hash.Buscar(nombre.ToUpper()) != null;
        }

        public void Guardar(Categoria c)
        {
            using (var conexion = conexionDB.ObtenerConexion())
            {
                conexion.Open();
                string sql = "INSERT INTO Categorias (Nombre) VALUES (@nombre)";
                using (var comando = new SQLiteCommand(sql, conexion)) { comando.Parameters.AddWithValue("@nombre", c.Nombre); comando.ExecuteNonQuery(); }
            }
        }

        public void Actualizar(Categoria c)
        {
            using (var conexion = conexionDB.ObtenerConexion())
            {
                conexion.Open();
                string sql = "UPDATE Categorias SET Nombre = @nombre WHERE Id = @id";
                using (var comando = new SQLiteCommand(sql, conexion)) { comando.Parameters.AddWithValue("@nombre", c.Nombre); comando.Parameters.AddWithValue("@id", c.Id); comando.ExecuteNonQuery(); }
            }
        }

        public void Eliminar(int id)
        {
            using (var conexion = conexionDB.ObtenerConexion())
            {
                conexion.Open();
                string sql = "DELETE FROM Categorias WHERE Id = @id";
                using (var comando = new SQLiteCommand(sql, conexion)) { comando.Parameters.AddWithValue("@id", id); comando.ExecuteNonQuery(); }
            }
        }

        public int ObtenerIdPorNombre(string nombre)
        {
            using (var conexion = conexionDB.ObtenerConexion())
            {
                conexion.Open();
                string sql = "SELECT Id FROM Categorias WHERE UPPER(Nombre) = @nombre";
                using (var comando = new SQLiteCommand(sql, conexion)) { comando.Parameters.AddWithValue("@nombre", nombre.ToUpper().Trim()); var res = comando.ExecuteScalar(); return res != null ? Convert.ToInt32(res) : -1; }
            }
        }

        public ListaEnlazada ListarTodas()
        {
            ListaEnlazada lista = new ListaEnlazada();
            using (var conexion = conexionDB.ObtenerConexion())
            {
                conexion.Open();
                string sql = "SELECT * FROM Categorias ORDER BY Nombre ASC";
                using (var comando = new SQLiteCommand(sql, conexion))
                {
                    using (var lector = comando.ExecuteReader())
                    {
                        while (lector.Read()) lista.Agregar(new Categoria { Id = Convert.ToInt32(lector["Id"]), Nombre = lector["Nombre"].ToString() });
                    }
                }
            }
            return lista;
        }
    }
}
