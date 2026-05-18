using System;
using System.Data.SQLite;
using ProyectoCatedra.Datos;
using ProyectoCatedra.Estructuras;
using ProyectoCatedra.Modelos;

namespace ProyectoCatedra.Servicios
{
    public class BeneficiarioServicio
    {
        private ConexionDB conexionDB;
        private readonly ArbolBST arbolPorNombre;
        private readonly TablaHash indicePorId;

        public BeneficiarioServicio()
        {
            conexionDB = new ConexionDB();
            arbolPorNombre = new ArbolBST();
            indicePorId = new TablaHash(211);
            CargarDesdeBase();
        }

        public void Guardar(Beneficiario b)
        {
            using (var conexion = conexionDB.ObtenerConexion())
            {
                conexion.Open();
                string sql = "INSERT INTO Beneficiarios (Nombre, MiembrosHogar, Activo) VALUES (@nombre, @miembros, @activo); SELECT last_insert_rowid();";
                using (var comando = new SQLiteCommand(sql, conexion))
                {
                    comando.Parameters.AddWithValue("@nombre", b.Nombre);
                    comando.Parameters.AddWithValue("@miembros", b.MiembrosHogar);
                    comando.Parameters.AddWithValue("@activo", b.Activo ? 1 : 0);
                    b.Id = Convert.ToInt32(comando.ExecuteScalar());
                }
            }

            AgregarEnIndices(b);
        }

        public void Actualizar(Beneficiario b)
        {
            string claveId = b.Id.ToString();
            Beneficiario? actual = (Beneficiario?)indicePorId.Buscar(claveId);
            if (actual == null) return;

            string nombreAnterior = actual.Nombre;
            int miembrosAnterior = actual.MiembrosHogar;
            bool activoAnterior = actual.Activo;

            arbolPorNombre.EliminarValor(actual.Nombre, valor => ((Beneficiario)valor).Id == b.Id);

            try
            {
                using (var conexion = conexionDB.ObtenerConexion())
                {
                    conexion.Open();
                    string sql = "UPDATE Beneficiarios SET Nombre = @nombre, MiembrosHogar = @miembros, Activo = @activo WHERE Id = @id";
                    using (var comando = new SQLiteCommand(sql, conexion))
                    {
                        comando.Parameters.AddWithValue("@nombre", b.Nombre);
                        comando.Parameters.AddWithValue("@miembros", b.MiembrosHogar);
                        comando.Parameters.AddWithValue("@activo", b.Activo ? 1 : 0);
                        comando.Parameters.AddWithValue("@id", b.Id);
                        comando.ExecuteNonQuery();
                    }
                }

                actual.Nombre = b.Nombre;
                actual.MiembrosHogar = b.MiembrosHogar;
                actual.Activo = b.Activo;
                arbolPorNombre.Insertar(actual.Nombre, actual);
            }
            catch
            {
                actual.Nombre = nombreAnterior;
                actual.MiembrosHogar = miembrosAnterior;
                actual.Activo = activoAnterior;
                arbolPorNombre.Insertar(actual.Nombre, actual);
                throw;
            }
        }

        public void Eliminar(int id)
        {
            string claveId = id.ToString();
            Beneficiario? existente = (Beneficiario?)indicePorId.Buscar(claveId);
            if (existente == null) return;

            arbolPorNombre.EliminarValor(existente.Nombre, valor => ((Beneficiario)valor).Id == id);
            indicePorId.Eliminar(claveId);

            try
            {
                using (var conexion = conexionDB.ObtenerConexion())
                {
                    conexion.Open();
                    string sql = "DELETE FROM Beneficiarios WHERE Id = @id";
                    using (var comando = new SQLiteCommand(sql, conexion))
                    {
                        comando.Parameters.AddWithValue("@id", id);
                        comando.ExecuteNonQuery();
                    }
                }
            }
            catch
            {
                AgregarEnIndices(existente);
                throw;
            }
        }

        public ArbolBST CargarEnArbol()
        {
            return arbolPorNombre;
        }

        public ListaEnlazada ListarTodos()
        {
            return arbolPorNombre.ObtenerInOrder();
        }

        private void CargarDesdeBase()
        {
            using (var conexion = conexionDB.ObtenerConexion())
            {
                conexion.Open();
                string sql = "SELECT * FROM Beneficiarios";
                using (var comando = new SQLiteCommand(sql, conexion))
                using (var lector = comando.ExecuteReader())
                {
                    while (lector.Read())
                    {
                        var beneficiario = new Beneficiario
                        {
                            Id = Convert.ToInt32(lector["Id"]),
                            Nombre = lector["Nombre"]?.ToString() ?? string.Empty,
                            MiembrosHogar = Convert.ToInt32(lector["MiembrosHogar"]),
                            Activo = Convert.ToInt32(lector["Activo"]) == 1,
                            FechaRegistro = lector["FechaRegistro"] != DBNull.Value ? Convert.ToDateTime(lector["FechaRegistro"]) : DateTime.Now
                        };

                        AgregarEnIndices(beneficiario);
                    }
                }
            }
        }

        private void AgregarEnIndices(Beneficiario beneficiario)
        {
            arbolPorNombre.Insertar(beneficiario.Nombre, beneficiario);
            indicePorId.Insertar(beneficiario.Id.ToString(), beneficiario);
        }
    }
}
