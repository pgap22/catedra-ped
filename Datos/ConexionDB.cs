using System;
using System.Data.SQLite;
using System.IO;

namespace ProyectoCatedra.Datos
{
    public class ConexionDB
    {
        private string cadenaConexion;
        private string nombreBD = "donaciones.db";

        public ConexionDB()
        {
            cadenaConexion = $"Data Source={nombreBD};Version=3;";
            InicializarBD();
        }

        private void InicializarBD()
        {
            if (!File.Exists(nombreBD))
            {
                SQLiteConnection.CreateFile(nombreBD);
                using (var conexion = new SQLiteConnection(cadenaConexion))
                {
                    conexion.Open();
                    
                    // Categorías
                    string sqlCat = "CREATE TABLE Categorias (Id INTEGER PRIMARY KEY AUTOINCREMENT, Nombre TEXT NOT NULL);";
                    
                    // Unidades de Medida
                    string sqlUni = "CREATE TABLE UnidadesMedida (Id INTEGER PRIMARY KEY AUTOINCREMENT, Nombre TEXT NOT NULL, Tipo TEXT NOT NULL);";

                    // Pivote Categoria - Unidades Validas
                    string sqlPiv = "CREATE TABLE CategoriaUnidades (IdCategoria INTEGER, IdUnidad INTEGER, PRIMARY KEY(IdCategoria, IdUnidad));";

                    // Beneficiarios
                    string sqlBen = "CREATE TABLE Beneficiarios (Id INTEGER PRIMARY KEY AUTOINCREMENT, Nombre TEXT NOT NULL, MiembrosHogar INTEGER DEFAULT 1, Activo INTEGER DEFAULT 1);";

                    // Productos
                    string sqlProd = @"CREATE TABLE Productos (
                                            Id INTEGER PRIMARY KEY AUTOINCREMENT,
                                            SKU TEXT UNIQUE NOT NULL,
                                            Nombre TEXT NOT NULL,
                                            IdCategoria INTEGER,
                                            Stock REAL DEFAULT 0,
                                            FOREIGN KEY(IdCategoria) REFERENCES Categorias(Id)
                                          );";

                    using (var cmd = new SQLiteCommand(sqlCat, conexion)) cmd.ExecuteNonQuery();
                    using (var cmd = new SQLiteCommand(sqlUni, conexion)) cmd.ExecuteNonQuery();
                    using (var cmd = new SQLiteCommand(sqlPiv, conexion)) cmd.ExecuteNonQuery();
                    using (var cmd = new SQLiteCommand(sqlBen, conexion)) cmd.ExecuteNonQuery();
                    using (var cmd = new SQLiteCommand(sqlProd, conexion)) cmd.ExecuteNonQuery();
                }
            }
        }

        public SQLiteConnection ObtenerConexion() => new SQLiteConnection(cadenaConexion);
    }
}
