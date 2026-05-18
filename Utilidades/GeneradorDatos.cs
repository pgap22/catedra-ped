using System;
using System.Data.SQLite;
using ProyectoCatedra.Datos;

namespace ProyectoCatedra.Utilidades
{
    public static class GeneradorDatos
    {
        public static void SembrarDatosPrueba()
        {
            var conexionDB = new ConexionDB();
            using (var conexion = conexionDB.ObtenerConexion())
            {
                conexion.Open();
                using (var tr = conexion.BeginTransaction())
                {
                    try
                    {
                        LimpiarBase(conexion, tr);

                        // 2. Insertar Categorias
                        string sqlCat = "INSERT INTO Categorias (Id, Nombre) VALUES (@id, @nom)";
                        var categorias = new[] { 
                            new { Id = 1, Nom = "Granos Básicos" }, 
                            new { Id = 2, Nom = "Aceites y Grasas" }, 
                            new { Id = 3, Nom = "Higiene Personal" },
                            new { Id = 4, Nom = "Lácteos y Derivados" },
                            new { Id = 5, Nom = "Proteínas (Carnes/Atún)" },
                            new { Id = 6, Nom = "Cereales y Bebidas" }
                        };
                        foreach (var c in categorias)
                        {
                            using (var cmd = new SQLiteCommand(sqlCat, conexion, tr))
                            {
                                cmd.Parameters.AddWithValue("@id", c.Id);
                                cmd.Parameters.AddWithValue("@nom", c.Nom);
                                cmd.ExecuteNonQuery();
                            }
                        }

                        // 3. Insertar Unidades Extendidas
                        string sqlUni = "INSERT INTO UnidadesMedida (Id, Nombre, Tipo) VALUES (@id, @nom, @tipo)";
                        var unidades = new[] { 
                            new { Id = 1, Nom = "Libras", Tipo = "Peso" }, 
                            new { Id = 2, Nom = "Litros", Tipo = "Volumen" }, 
                            new { Id = 3, Nom = "Unidades", Tipo = "Unidad" },
                            new { Id = 4, Nom = "Latas", Tipo = "Unidad" },
                            new { Id = 5, Nom = "Kilos", Tipo = "Peso" },
                            new { Id = 6, Nom = "Galones", Tipo = "Volumen" },
                            new { Id = 7, Nom = "Paquetes", Tipo = "Unidad" }
                        };
                        foreach (var u in unidades)
                        {
                            using (var cmd = new SQLiteCommand(sqlUni, conexion, tr))
                            {
                                cmd.Parameters.AddWithValue("@id", u.Id);
                                cmd.Parameters.AddWithValue("@nom", u.Nom);
                                cmd.Parameters.AddWithValue("@tipo", u.Tipo);
                                cmd.ExecuteNonQuery();
                            }
                        }

                        // 4. Pivote Categoria-Unidad (Múltiples unidades permitidas por categoría)
                        string sqlPiv = "INSERT INTO CategoriaUnidades (IdCategoria, IdUnidad) VALUES (@cId, @uId)";
                        var pivotes = new[] { 
                            // Granos (1): Libras(1), Kilos(5), Paquetes(7)
                            new[] {1,1}, new[] {1,5}, new[] {1,7},
                            // Aceites (2): Litros(2), Galones(6), Unidades(3 - ej. botellas)
                            new[] {2,2}, new[] {2,6}, new[] {2,3},
                            // Higiene (3): Unidades(3), Paquetes(7)
                            new[] {3,3}, new[] {3,7},
                            // Lácteos (4): Litros(2), Galones(6), Latas(4 - leche en polvo)
                            new[] {4,2}, new[] {4,6}, new[] {4,4},
                            // Proteínas (5): Libras(1), Latas(4)
                            new[] {5,1}, new[] {5,4},
                            // Bebidas (6): Litros(2), Paquetes(7)
                            new[] {6,2}, new[] {6,7}
                        };
                        foreach (var p in pivotes)
                        {
                            using (var cmd = new SQLiteCommand(sqlPiv, conexion, tr))
                            {
                                cmd.Parameters.AddWithValue("@cId", p[0]);
                                cmd.Parameters.AddWithValue("@uId", p[1]);
                                cmd.ExecuteNonQuery();
                            }
                        }

                        // 5. Tasas de Consumo (1 por cada categoría usando su unidad base)
                        string sqlTasa = "INSERT INTO TasaConsumo (IdCategoria, TasaDiaria, IdUnidadBase) VALUES (@cId, @tasa, @uId)";
                        var tasas = new[] { 
                            new { cId = 1, tasa = 0.4, uId = 1 },   // 0.4 libras de grano por persona/día
                            new { cId = 2, tasa = 0.05, uId = 2 },  // 0.05 litros de aceite por persona/día
                            new { cId = 3, tasa = 0.1, uId = 3 },   // 0.1 unidades de higiene por persona/día
                            new { cId = 4, tasa = 0.2, uId = 2 },   // 0.2 litros de lácteos por persona/día
                            new { cId = 5, tasa = 0.3, uId = 1 },   // 0.3 libras de proteína por persona/día
                            new { cId = 6, tasa = 0.15, uId = 7 }   // 0.15 paquetes de bebida por persona/día
                        };
                        foreach (var t in tasas)
                        {
                            using (var cmd = new SQLiteCommand(sqlTasa, conexion, tr))
                            {
                                cmd.Parameters.AddWithValue("@cId", t.cId);
                                cmd.Parameters.AddWithValue("@tasa", t.tasa);
                                cmd.Parameters.AddWithValue("@uId", t.uId);
                                cmd.ExecuteNonQuery();
                            }
                        }

                        // 6. Productos (+50 productos)
                        string sqlProd = "INSERT INTO Productos (SKU, Nombre, IdCategoria, Stock, IdUnidad) VALUES (@sku, @nom, @cId, @stock, @uId)";
                        var r = new Random();
                        
                        var catalogoProductos = new[] {
                            // Granos (Cat 1) - Base Unit 1 (Libras)
                            new { Nom = "Arroz Blanco San Francisco", Cat = 1, Uni = 1, MinS = 100, MaxS = 1000 },
                            new { Nom = "Frijol Seda", Cat = 1, Uni = 1, MinS = 150, MaxS = 800 },
                            new { Nom = "Maíz Blanco", Cat = 1, Uni = 1, MinS = 50, MaxS = 500 },
                            new { Nom = "Lentejas", Cat = 1, Uni = 1, MinS = 20, MaxS = 200 },
                            new { Nom = "Azúcar Blanca", Cat = 1, Uni = 1, MinS = 100, MaxS = 600 },
                            new { Nom = "Harina de Trigo", Cat = 1, Uni = 1, MinS = 50, MaxS = 400 },
                            new { Nom = "Sal Refinada", Cat = 1, Uni = 1, MinS = 30, MaxS = 300 },
                            new { Nom = "Arroz Precocido", Cat = 1, Uni = 1, MinS = 0, MaxS = 250 },
                            new { Nom = "Frijol Negro", Cat = 1, Uni = 1, MinS = 0, MaxS = 150 },
                            
                            // Aceites (Cat 2) - Base Unit 2 (Litros)
                            new { Nom = "Aceite Vegetal Ideal", Cat = 2, Uni = 2, MinS = 30, MaxS = 150 },
                            new { Nom = "Aceite de Girasol", Cat = 2, Uni = 2, MinS = 10, MaxS = 80 },
                            new { Nom = "Aceite de Oliva Extra Virgen", Cat = 2, Uni = 2, MinS = 5, MaxS = 40 },
                            new { Nom = "Aceite Mazola", Cat = 2, Uni = 2, MinS = 20, MaxS = 100 },
                            new { Nom = "Manteca Vegetal (en litros eq)", Cat = 2, Uni = 2, MinS = 10, MaxS = 50 },
                            
                            // Higiene (Cat 3) - Base Unit 3 (Unidades)
                            new { Nom = "Jabón de Baño Protex", Cat = 3, Uni = 3, MinS = 50, MaxS = 300 },
                            new { Nom = "Pasta Dental Colgate", Cat = 3, Uni = 3, MinS = 40, MaxS = 200 },
                            new { Nom = "Shampoo Head&Shoulders", Cat = 3, Uni = 3, MinS = 20, MaxS = 150 },
                            new { Nom = "Papel Higiénico Scott (Rollo)", Cat = 3, Uni = 3, MinS = 100, MaxS = 1000 },
                            new { Nom = "Detergente Ropa Xtra", Cat = 3, Uni = 3, MinS = 30, MaxS = 200 },
                            new { Nom = "Desodorante Rexona", Cat = 3, Uni = 3, MinS = 15, MaxS = 100 },
                            new { Nom = "Toallas Sanitarias Nosotras", Cat = 3, Uni = 3, MinS = 20, MaxS = 250 },
                            new { Nom = "Cepillo Dental", Cat = 3, Uni = 3, MinS = 50, MaxS = 300 },
                            new { Nom = "Jabón Zote", Cat = 3, Uni = 3, MinS = 40, MaxS = 150 },
                            
                            // Lácteos (Cat 4) - Base Unit 2 (Litros)
                            new { Nom = "Leche Líquida Salud", Cat = 4, Uni = 2, MinS = 50, MaxS = 400 },
                            new { Nom = "Leche Deslactosada", Cat = 4, Uni = 2, MinS = 20, MaxS = 150 },
                            new { Nom = "Crema Pura", Cat = 4, Uni = 2, MinS = 10, MaxS = 80 },
                            new { Nom = "Yogurt Líquido", Cat = 4, Uni = 2, MinS = 0, MaxS = 100 },
                            
                            // Proteínas (Cat 5) - Base Unit 1 (Libras)
                            new { Nom = "Pollo Entero Congelado", Cat = 5, Uni = 1, MinS = 30, MaxS = 250 },
                            new { Nom = "Carne de Res Molida", Cat = 5, Uni = 1, MinS = 20, MaxS = 150 },
                            new { Nom = "Atún en Agua (eq libras)", Cat = 5, Uni = 1, MinS = 50, MaxS = 300 },
                            new { Nom = "Sardina Pica Pica (eq libras)", Cat = 5, Uni = 1, MinS = 40, MaxS = 200 },
                            new { Nom = "Huevos (eq libras)", Cat = 5, Uni = 1, MinS = 100, MaxS = 400 },
                            new { Nom = "Carne de Cerdo", Cat = 5, Uni = 1, MinS = 0, MaxS = 80 },
                            new { Nom = "Salchichas", Cat = 5, Uni = 1, MinS = 20, MaxS = 100 },
                            
                            // Bebidas (Cat 6) - Base Unit 7 (Paquetes)
                            new { Nom = "Avena Mosh", Cat = 6, Uni = 7, MinS = 40, MaxS = 200 },
                            new { Nom = "Café Molido Musún", Cat = 6, Uni = 7, MinS = 30, MaxS = 150 },
                            new { Nom = "Café Instantáneo Listo", Cat = 6, Uni = 7, MinS = 20, MaxS = 100 },
                            new { Nom = "Fresco en Polvo Zuko", Cat = 6, Uni = 7, MinS = 50, MaxS = 500 },
                            new { Nom = "Cereal Corn Flakes", Cat = 6, Uni = 7, MinS = 15, MaxS = 80 },
                            new { Nom = "Incaparina", Cat = 6, Uni = 7, MinS = 25, MaxS = 120 }
                        };

                        int pCount = 1;
                        foreach(var p in catalogoProductos)
                        {
                            // A veces un producto puede no tener stock en la vida real, lo simulamos
                            double stock = r.Next(100) > 10 ? r.Next(p.MinS, p.MaxS) : 0; 

                            using (var cmd = new SQLiteCommand(sqlProd, conexion, tr))
                            {
                                cmd.Parameters.AddWithValue("@sku", $"SKU{pCount:D3}");
                                cmd.Parameters.AddWithValue("@nom", p.Nom);
                                cmd.Parameters.AddWithValue("@cId", p.Cat);
                                cmd.Parameters.AddWithValue("@stock", stock);
                                cmd.Parameters.AddWithValue("@uId", p.Uni);
                                cmd.ExecuteNonQuery();
                            }
                            pCount++;
                        }

                        using (var cmd = new SQLiteCommand("UPDATE Productos SET MaximoPorEntrega = 1, DiasReposicion = 90 WHERE Nombre = 'Cepillo Dental'", conexion, tr))
                        {
                            cmd.ExecuteNonQuery();
                        }

                        InsertarPack(conexion, tr, 1, 1, 45);
                        InsertarPack(conexion, tr, 1, 2, 25);
                        InsertarPack(conexion, tr, 1, 5, 20);
                        InsertarPack(conexion, tr, 1, 6, 10);
                        InsertarPack(conexion, tr, 2, 10, 70);
                        InsertarPack(conexion, tr, 2, 13, 30);
                        InsertarPack(conexion, tr, 3, 15, 35);
                        InsertarPack(conexion, tr, 3, 16, 25);
                        InsertarPack(conexion, tr, 3, 18, 20);
                        InsertarPack(conexion, tr, 3, 22, 20);

                        // 7. Beneficiarios (150 registros)
                        string sqlBen = "INSERT INTO Beneficiarios (Nombre, MiembrosHogar, Activo, FechaRegistro) VALUES (@nom, @miembros, 1, @fecha)";
                        string[] nombres = { "Juan", "Maria", "Carlos", "Ana", "Luis", "Carmen", "Pedro", "Laura", "Jose", "Rosa", "Jorge", "Marta", "Miguel", "Lucia", "Francisco", "Elena", "Roberto", "Sandra", "Diego", "Patricia" };
                        string[] apellidos = { "Garcia", "Martinez", "Rodriguez", "Lopez", "Perez", "Gonzalez", "Gomez", "Fernandez", "Ramirez", "Cruz", "Diaz", "Ortiz", "Mendez", "Reyes", "Alvarez", "Castillo", "Chavez", "Rivera", "Juarez", "Ramos" };
                        
                        for (int i = 0; i < 150; i++)
                        {
                            // Nombre aleatorio sin repetir la misma secuencia simple
                            string nom = $"{nombres[r.Next(nombres.Length)]} {apellidos[r.Next(apellidos.Length)]} {apellidos[r.Next(apellidos.Length)]}";
                            
                            // Peso en la aleatoriedad para tener hogares más realistas (más de 3-5, menos de 1 o 8+)
                            int chance = r.Next(100);
                            int miembros;
                            if (chance < 15) miembros = r.Next(1, 3); // 15% hogares pequeños (1-2)
                            else if (chance < 75) miembros = r.Next(3, 6); // 60% hogares medianos (3-5)
                            else miembros = r.Next(6, 10); // 25% hogares grandes (6-9)

                            // En datos demo todas empiezan recientes para no crear deficit artificial
                            // antes de que exista una primera entrega real.
                            DateTime fechaReg = RelojDemo.Ahora;

                            using (var cmd = new SQLiteCommand(sqlBen, conexion, tr))
                            {
                                cmd.Parameters.AddWithValue("@nom", nom);
                                cmd.Parameters.AddWithValue("@miembros", miembros);
                                cmd.Parameters.AddWithValue("@fecha", fechaReg.ToString("yyyy-MM-dd HH:mm:ss"));
                                cmd.ExecuteNonQuery();
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

        public static void SembrarDemoPequena()
        {
            var conexionDB = new ConexionDB();
            using (var conexion = conexionDB.ObtenerConexion())
            {
                conexion.Open();
                using (var tr = conexion.BeginTransaction())
                {
                    try
                    {
                        LimpiarBase(conexion, tr);

                        string sqlCat = "INSERT INTO Categorias (Id, Nombre) VALUES (@id, @nom)";
                        InsertarCategoria(conexion, tr, sqlCat, 1, "Granos Basicos");
                        InsertarCategoria(conexion, tr, sqlCat, 2, "Aceites y Grasas");
                        InsertarCategoria(conexion, tr, sqlCat, 3, "Higiene Personal");

                        string sqlUni = "INSERT INTO UnidadesMedida (Id, Nombre, Tipo) VALUES (@id, @nom, @tipo)";
                        InsertarUnidad(conexion, tr, sqlUni, 1, "Libras", "Peso");
                        InsertarUnidad(conexion, tr, sqlUni, 2, "Litros", "Volumen");
                        InsertarUnidad(conexion, tr, sqlUni, 3, "Unidades", "Unidad");

                        string sqlPiv = "INSERT INTO CategoriaUnidades (IdCategoria, IdUnidad) VALUES (@cId, @uId)";
                        InsertarCategoriaUnidad(conexion, tr, sqlPiv, 1, 1);
                        InsertarCategoriaUnidad(conexion, tr, sqlPiv, 2, 2);
                        InsertarCategoriaUnidad(conexion, tr, sqlPiv, 3, 3);

                        string sqlTasa = "INSERT INTO TasaConsumo (IdCategoria, TasaDiaria, IdUnidadBase) VALUES (@cId, @tasa, @uId)";
                        InsertarTasa(conexion, tr, sqlTasa, 1, 0.4, 1);
                        InsertarTasa(conexion, tr, sqlTasa, 2, 0.05, 2);
                        InsertarTasa(conexion, tr, sqlTasa, 3, 0.1, 3);

                        string sqlProd = "INSERT INTO Productos (SKU, Nombre, IdCategoria, Stock, IdUnidad, MaximoPorEntrega, DiasReposicion) VALUES (@sku, @nom, @cId, @stock, @uId, @max, @dias)";
                        InsertarProducto(conexion, tr, sqlProd, "SKU001", "Arroz Blanco", 1, 40, 1);
                        InsertarProducto(conexion, tr, sqlProd, "SKU002", "Frijol Rojo", 1, 25, 1);
                        InsertarProducto(conexion, tr, sqlProd, "SKU003", "Aceite Vegetal", 2, 12, 2);
                        InsertarProducto(conexion, tr, sqlProd, "SKU004", "Jabon de Bano", 3, 30, 3);
                        InsertarProducto(conexion, tr, sqlProd, "SKU005", "Cepillo Dental", 3, 20, 3, 1, 90);

                        InsertarPack(conexion, tr, 1, 1, 60);
                        InsertarPack(conexion, tr, 1, 2, 40);
                        InsertarPack(conexion, tr, 2, 3, 100);
                        InsertarPack(conexion, tr, 3, 4, 70);
                        InsertarPack(conexion, tr, 3, 5, 30);

                        string sqlBen = "INSERT INTO Beneficiarios (Nombre, MiembrosHogar, Activo, FechaRegistro) VALUES (@nom, @miembros, 1, @fecha)";
                        DateTime fecha = RelojDemo.Ahora;
                        InsertarBeneficiario(conexion, tr, sqlBen, "Familia Lopez", 5, fecha);
                        InsertarBeneficiario(conexion, tr, sqlBen, "Familia Perez", 3, fecha);
                        InsertarBeneficiario(conexion, tr, sqlBen, "Familia Martinez", 7, fecha);
                        InsertarBeneficiario(conexion, tr, sqlBen, "Familia Solis", 2, fecha);

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

        private static void LimpiarBase(SQLiteConnection conexion, SQLiteTransaction tr)
        {
            string[] tablas = { "OrdenDetalle", "Orden", "CategoriaPackDetalle", "TasaConsumo", "Productos", "CategoriaUnidades", "Categorias", "UnidadesMedida", "Beneficiarios" };
            foreach (var tabla in tablas)
            {
                using (var cmd = new SQLiteCommand($"DELETE FROM {tabla};", conexion, tr))
                {
                    cmd.ExecuteNonQuery();
                }
                using (var cmd = new SQLiteCommand($"DELETE FROM sqlite_sequence WHERE name='{tabla}';", conexion, tr))
                {
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private static void InsertarCategoria(SQLiteConnection conexion, SQLiteTransaction tr, string sql, int id, string nombre)
        {
            using (var cmd = new SQLiteCommand(sql, conexion, tr))
            {
                cmd.Parameters.AddWithValue("@id", id);
                cmd.Parameters.AddWithValue("@nom", nombre);
                cmd.ExecuteNonQuery();
            }
        }

        private static void InsertarUnidad(SQLiteConnection conexion, SQLiteTransaction tr, string sql, int id, string nombre, string tipo)
        {
            using (var cmd = new SQLiteCommand(sql, conexion, tr))
            {
                cmd.Parameters.AddWithValue("@id", id);
                cmd.Parameters.AddWithValue("@nom", nombre);
                cmd.Parameters.AddWithValue("@tipo", tipo);
                cmd.ExecuteNonQuery();
            }
        }

        private static void InsertarCategoriaUnidad(SQLiteConnection conexion, SQLiteTransaction tr, string sql, int categoriaId, int unidadId)
        {
            using (var cmd = new SQLiteCommand(sql, conexion, tr))
            {
                cmd.Parameters.AddWithValue("@cId", categoriaId);
                cmd.Parameters.AddWithValue("@uId", unidadId);
                cmd.ExecuteNonQuery();
            }
        }

        private static void InsertarTasa(SQLiteConnection conexion, SQLiteTransaction tr, string sql, int categoriaId, double tasa, int unidadId)
        {
            using (var cmd = new SQLiteCommand(sql, conexion, tr))
            {
                cmd.Parameters.AddWithValue("@cId", categoriaId);
                cmd.Parameters.AddWithValue("@tasa", tasa);
                cmd.Parameters.AddWithValue("@uId", unidadId);
                cmd.ExecuteNonQuery();
            }
        }

        private static void InsertarProducto(SQLiteConnection conexion, SQLiteTransaction tr, string sql, string sku, string nombre, int categoriaId, double stock, int unidadId, double? maximoPorEntrega = null, int? diasReposicion = null)
        {
            using (var cmd = new SQLiteCommand(sql, conexion, tr))
            {
                cmd.Parameters.AddWithValue("@sku", sku);
                cmd.Parameters.AddWithValue("@nom", nombre);
                cmd.Parameters.AddWithValue("@cId", categoriaId);
                cmd.Parameters.AddWithValue("@stock", stock);
                cmd.Parameters.AddWithValue("@uId", unidadId);
                cmd.Parameters.AddWithValue("@max", (object?)maximoPorEntrega ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@dias", (object?)diasReposicion ?? DBNull.Value);
                cmd.ExecuteNonQuery();
            }
        }

        private static void InsertarPack(SQLiteConnection conexion, SQLiteTransaction tr, int categoriaId, int productoId, double porcentaje)
        {
            string sql = "INSERT INTO CategoriaPackDetalle (CategoriaId, ProductoId, Porcentaje) VALUES (@cat, @prod, @porcentaje)";
            using (var cmd = new SQLiteCommand(sql, conexion, tr))
            {
                cmd.Parameters.AddWithValue("@cat", categoriaId);
                cmd.Parameters.AddWithValue("@prod", productoId);
                cmd.Parameters.AddWithValue("@porcentaje", porcentaje);
                cmd.ExecuteNonQuery();
            }
        }

        private static void InsertarBeneficiario(SQLiteConnection conexion, SQLiteTransaction tr, string sql, string nombre, int miembros, DateTime fecha)
        {
            using (var cmd = new SQLiteCommand(sql, conexion, tr))
            {
                cmd.Parameters.AddWithValue("@nom", nombre);
                cmd.Parameters.AddWithValue("@miembros", miembros);
                cmd.Parameters.AddWithValue("@fecha", fecha.ToString("yyyy-MM-dd HH:mm:ss"));
                cmd.ExecuteNonQuery();
            }
        }
    }
}
