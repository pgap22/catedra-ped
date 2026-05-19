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
            ActualizarEsquema();
        }

        private void InicializarBD()
        {
            if (!File.Exists(nombreBD))
            {
                SQLiteConnection.CreateFile(nombreBD);
                using (var conexion = ObtenerConexion())
                {
                    conexion.Open();
                    
                    string sqlCat = "CREATE TABLE IF NOT EXISTS Categorias (Id INTEGER PRIMARY KEY AUTOINCREMENT, Nombre TEXT NOT NULL);";
                    string sqlUni = "CREATE TABLE IF NOT EXISTS UnidadesMedida (Id INTEGER PRIMARY KEY AUTOINCREMENT, Nombre TEXT NOT NULL, Tipo TEXT NOT NULL);";
                    string sqlPiv = "CREATE TABLE IF NOT EXISTS CategoriaUnidades (IdCategoria INTEGER, IdUnidad INTEGER, PRIMARY KEY(IdCategoria, IdUnidad));";
                    string sqlBen = "CREATE TABLE IF NOT EXISTS Beneficiarios (Id INTEGER PRIMARY KEY AUTOINCREMENT, Nombre TEXT NOT NULL, MiembrosHogar INTEGER DEFAULT 1, Activo INTEGER DEFAULT 1, FechaRegistro DATETIME DEFAULT CURRENT_TIMESTAMP, NivelVulnerabilidad INTEGER DEFAULT 2);";
                    string sqlProd = @"CREATE TABLE IF NOT EXISTS Productos (
                                            Id INTEGER PRIMARY KEY AUTOINCREMENT,
                                            SKU TEXT UNIQUE NOT NULL,
                                            Nombre TEXT NOT NULL,
                                            IdCategoria INTEGER,
                                            Stock REAL DEFAULT 0,
                                            IdUnidad INTEGER,
                                            MaximoPorEntrega REAL,
                                            DiasReposicion INTEGER,
                                            FOREIGN KEY(IdCategoria) REFERENCES Categorias(Id),
                                            FOREIGN KEY(IdUnidad) REFERENCES UnidadesMedida(Id)
                                          );";

                    using (var cmd = new SQLiteCommand(sqlCat, conexion)) cmd.ExecuteNonQuery();
                    using (var cmd = new SQLiteCommand(sqlUni, conexion)) cmd.ExecuteNonQuery();
                    using (var cmd = new SQLiteCommand(sqlPiv, conexion)) cmd.ExecuteNonQuery();
                    using (var cmd = new SQLiteCommand(sqlBen, conexion)) cmd.ExecuteNonQuery();
                    using (var cmd = new SQLiteCommand(sqlProd, conexion)) cmd.ExecuteNonQuery();
                }
            }
        }

        private void ActualizarEsquema()
        {
            using (var conexion = ObtenerConexion())
            {
                conexion.Open();

                // Intentar añadir columnas a tablas antiguas si la BD ya existía
                try { new SQLiteCommand("ALTER TABLE Beneficiarios ADD COLUMN FechaRegistro DATETIME DEFAULT CURRENT_TIMESTAMP;", conexion).ExecuteNonQuery(); } catch { /* Ya existe */ }
                try { new SQLiteCommand("ALTER TABLE Beneficiarios ADD COLUMN NivelVulnerabilidad INTEGER DEFAULT 2;", conexion).ExecuteNonQuery(); } catch { /* Ya existe */ }
                try { new SQLiteCommand("ALTER TABLE Productos ADD COLUMN IdUnidad INTEGER REFERENCES UnidadesMedida(Id);", conexion).ExecuteNonQuery(); } catch { /* Ya existe */ }
                try { new SQLiteCommand("ALTER TABLE Productos ADD COLUMN MaximoPorEntrega REAL;", conexion).ExecuteNonQuery(); } catch { /* Ya existe */ }
                try { new SQLiteCommand("ALTER TABLE Productos ADD COLUMN DiasReposicion INTEGER;", conexion).ExecuteNonQuery(); } catch { /* Ya existe */ }

                // Crear nuevas tablas para la Fase 1
                string sqlTasa = @"CREATE TABLE IF NOT EXISTS TasaConsumo (
                                      IdCategoria INTEGER PRIMARY KEY, 
                                      TasaDiaria REAL NOT NULL, 
                                      IdUnidadBase INTEGER,
                                      FOREIGN KEY(IdCategoria) REFERENCES Categorias(Id),
                                      FOREIGN KEY(IdUnidadBase) REFERENCES UnidadesMedida(Id)
                                   );";
                string sqlOrden = @"CREATE TABLE IF NOT EXISTS Orden (
                                       Id INTEGER PRIMARY KEY AUTOINCREMENT, 
                                       FechaGeneracion DATETIME DEFAULT CURRENT_TIMESTAMP, 
                                       Estado TEXT NOT NULL, 
                                       Observaciones TEXT
                                    );";
                string sqlDetalle = @"CREATE TABLE IF NOT EXISTS OrdenDetalle (
                                         Id INTEGER PRIMARY KEY AUTOINCREMENT, 
                                          OrdenId INTEGER, 
                                          BeneficiarioId INTEGER, 
                                          CategoriaId INTEGER, 
                                          ProductoId INTEGER,
                                          CantidadAsignada REAL NOT NULL, 
                                          DeficitCalculado REAL,
                                          FOREIGN KEY(OrdenId) REFERENCES Orden(Id),
                                          FOREIGN KEY(BeneficiarioId) REFERENCES Beneficiarios(Id),
                                          FOREIGN KEY(CategoriaId) REFERENCES Categorias(Id),
                                          FOREIGN KEY(ProductoId) REFERENCES Productos(Id)
                                       );";
                string sqlPack = @"CREATE TABLE IF NOT EXISTS CategoriaPackDetalle (
                                      Id INTEGER PRIMARY KEY AUTOINCREMENT,
                                      CategoriaId INTEGER NOT NULL,
                                      ProductoId INTEGER NOT NULL,
                                      Porcentaje REAL NOT NULL,
                                      FOREIGN KEY(CategoriaId) REFERENCES Categorias(Id),
                                      FOREIGN KEY(ProductoId) REFERENCES Productos(Id),
                                      UNIQUE(CategoriaId, ProductoId)
                                   );";

                using (var cmd = new SQLiteCommand(sqlTasa, conexion)) cmd.ExecuteNonQuery();
                using (var cmd = new SQLiteCommand(sqlOrden, conexion)) cmd.ExecuteNonQuery();
                using (var cmd = new SQLiteCommand(sqlDetalle, conexion)) cmd.ExecuteNonQuery();
                try { new SQLiteCommand("ALTER TABLE OrdenDetalle ADD COLUMN ProductoId INTEGER REFERENCES Productos(Id);", conexion).ExecuteNonQuery(); } catch { /* Ya existe */ }
                using (var cmd = new SQLiteCommand(sqlPack, conexion)) cmd.ExecuteNonQuery();
            }
        }

        public SQLiteConnection ObtenerConexion()
        {
            var conexion = new SQLiteConnection(cadenaConexion);
            conexion.StateChange += (s, e) => {
                if (conexion.State == System.Data.ConnectionState.Open)
                {
                    using (var cmd = new SQLiteCommand("PRAGMA foreign_keys = ON;", conexion))
                    {
                        cmd.ExecuteNonQuery();
                    }
                }
            };
            return conexion;
        }
    }
}
