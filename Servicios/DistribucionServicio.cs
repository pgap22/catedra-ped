using System;
using System.Data.SQLite;
using ProyectoCatedra.Datos;
using ProyectoCatedra.Modelos;
using ProyectoCatedra.Estructuras;
using ProyectoCatedra.Utilidades;

namespace ProyectoCatedra.Servicios
{
    public class DistribucionServicio
    {
        private ConexionDB conexionDB;
        private BeneficiarioServicio beneficiarioServicio;
        private ProductoServicio productoServicio;
        private TasaConsumoServicio tasaServicio;

        public DistribucionServicio()
        {
            conexionDB = new ConexionDB();
            beneficiarioServicio = new BeneficiarioServicio();
            productoServicio = new ProductoServicio();
            tasaServicio = new TasaConsumoServicio();
        }

        public ListaEnlazada GenerarPropuestaDistribucion(int categoriaIdFiltro = 0)
        {
            ListaEnlazada propuesta = new ListaEnlazada();
            
            // 1. Obtener beneficiarios activos
            var beneficiarios = beneficiarioServicio.ListarTodos(); // Should ideally only get actives, let's filter
            
            // 2. Obtener todas las tasas de consumo (que definen las categorias a distribuir)
            var tasas = tasaServicio.ListarTodas();
            
            // Si no hay tasas o no hay beneficiarios, no hay nada que hacer
            if (beneficiarios.Conteo() == 0 || tasas.Conteo() == 0) return propuesta;

            // Procesar categoria por categoria
            for (int i = 0; i < tasas.Conteo(); i++)
            {
                var tasa = (TasaConsumo)tasas.Obtener(i)!;
                if (categoriaIdFiltro > 0 && tasa.IdCategoria != categoriaIdFiltro) continue;
                
                // Obtener stock disponible total para la categoria en su unidad base
                double stockDisponible = ObtenerStockTotalCategoria(tasa.IdCategoria, tasa.IdUnidadBase);
                if (stockDisponible <= 0) continue; // Skip si no hay nada que dar

                var productoPrincipal = ObtenerProductoPrincipalCategoria(tasa.IdCategoria);

                MonticuloMaximo heap = new MonticuloMaximo(beneficiarios.Conteo());

                for (int j = 0; j < beneficiarios.Conteo(); j++)
                {
                    var b = (Beneficiario)beneficiarios.Obtener(j)!;
                    if (!b.Activo) continue; // Solo activos

                    var resultadoDeficit = CalcularDeficit(b, tasa);
                    double deficit = resultadoDeficit.Deficit;
                    
                    // La prioridad incluye el deficit (4 decimales), y desempates:
                    // 1) Miembros del hogar (mayor = más prioridad)
                    // 2) Antigüedad (id más bajo = más prioridad)
                    decimal prioridad = Math.Round((decimal)deficit, 4) 
                                      + (b.MiembrosHogar * 0.00001m) 
                                      + ((1000000m - b.Id) * 0.0000000001m);
                                      
                    // Guardamos la info temporal para el heap
                    var info = new InfoDistribucion
                    {
                        Beneficiario = b,
                        Deficit = deficit,
                        Asignado = 0,
                        ExplicacionCalculo = resultadoDeficit.Explicacion
                    };
                    
                    heap.Insertar(prioridad, info);
                }

                // Distribuir mientras haya stock y el heap no este vacio
                while (stockDisponible > 0)
                {
                    var max = heap.ExtraerMaximo();
                    if (max == null) break;
                    
                    var info = (InfoDistribucion)max;
                    
                    // Si el deficit es cero o negativo, igual le podemos dar algo si sobra?
                    // Según reglas: "Nadie recibe más de lo que razonablemente puede consumir"
                    // Para simplificar, le damos para 1 semana de consumo si el deficit es 0, o solo suplimos el deficit
                    double demanda = info.Deficit > 0 ? info.Deficit : (info.Beneficiario.MiembrosHogar * tasa.TasaDiaria * 7);
                    
                    if (demanda <= 0) demanda = info.Beneficiario.MiembrosHogar * tasa.TasaDiaria * 7; // Backup

                    double aAsignar = Math.Floor(Math.Min(demanda, stockDisponible));
                    if (aAsignar < 1)
                    {
                        if (stockDisponible < 1) break;
                        continue;
                    }
                    
                    // Solo asignamos si la cantidad es razonable (> 0.01) para evitar centavos de producto
                    if (aAsignar > 0.01)
                    {
                        info.Asignado += aAsignar;
                        stockDisponible -= aAsignar;
                        
                        propuesta.Agregar(new OrdenDetalle
                        {
                            BeneficiarioId = info.Beneficiario.Id,
                            NombreBeneficiario = info.Beneficiario.Nombre,
                            CategoriaId = tasa.IdCategoria,
                            NombreCategoria = tasa.NombreCategoria,
                            CantidadAsignada = info.Asignado,
                            DeficitCalculado = Math.Round(info.Deficit, 2),
                            ExplicacionCalculo = info.ExplicacionCalculo,
                            SKUProductoSugerido = productoPrincipal.SKU,
                            NombreProductoSugerido = productoPrincipal.Nombre,
                            NombreUnidadMedida = tasa.NombreUnidadBase
                        });
                    }
                }
            }
            
            return propuesta;
        }

        private (double Deficit, string Explicacion) CalcularDeficit(Beneficiario b, TasaConsumo tasa)
        {
            // Consumo esperado = Miembros * Tasa * Días desde registro
            TimeSpan antiguedad = RelojDemo.Ahora - b.FechaRegistro;
            double dias = Math.Max(1, antiguedad.TotalDays); // Mínimo 1 día para no multiplicar por 0
            
            // Obtener lo que ya recibió históricamente de esta categoría
            double totalRecibido = 0;
            using (var conexion = conexionDB.ObtenerConexion())
            {
                conexion.Open();
                string sql = @"
                    SELECT SUM(CantidadAsignada) 
                    FROM OrdenDetalle od
                    INNER JOIN Orden o ON od.OrdenId = o.Id
                    WHERE od.BeneficiarioId = @bId 
                      AND od.CategoriaId = @cId
                      AND o.Estado = 'CONFIRMADA'";
                      
                using (var cmd = new SQLiteCommand(sql, conexion))
                {
                    cmd.Parameters.AddWithValue("@bId", b.Id);
                    cmd.Parameters.AddWithValue("@cId", tasa.IdCategoria);
                    var res = cmd.ExecuteScalar();
                    if (res != DBNull.Value && res != null)
                    {
                        totalRecibido = Convert.ToDouble(res);
                    }
                }
            }

            double diasParaDeficit = totalRecibido == 0 ? 0 : dias;
            double consumoEsperado = b.MiembrosHogar * tasa.TasaDiaria * diasParaDeficit;
            double deficitFinal = consumoEsperado - totalRecibido;

            string explicacion = $"Cálculo para {b.Nombre}:\n" +
                                 $"- Miembros del hogar: {b.MiembrosHogar}\n" +
                                 $"- Días registrados en sistema: {Math.Round(dias, 1)} días\n" +
                                 $"- Tasa de consumo: {tasa.TasaDiaria} por persona/día\n" +
                                 (totalRecibido == 0 ? "- Sin entregas previas: no se acumula deuda por solo estar registrado\n" : "") +
                                 $"Meta usada para prioridad: {Math.Round(consumoEsperado, 2)}\n" +
                                 $"- Ya recibido anteriormente: {totalRecibido}\n\n" +
                                 $"Déficit Total (Meta - Recibido): {Math.Round(deficitFinal, 2)}";

            return (deficitFinal, explicacion);
        }

        public double ObtenerStockTotalCategoria(int idCategoria, int idUnidadBase = 0)
        {
            // Obtiene la suma del stock de todos los productos de esta categoria
            // Nota: Se asume que todos los productos de la categoria estan en la Unidad Base.
            // Si no, habria que hacer conversion, pero el PRD simplifica esto forzando a registrar 
            // en la unidad base (revisar diseño).
            
            double total = 0;
            using (var conexion = conexionDB.ObtenerConexion())
            {
                conexion.Open();
                string sql = "SELECT SUM(Stock) FROM Productos WHERE IdCategoria = @cId";
                using (var cmd = new SQLiteCommand(sql, conexion))
                {
                    cmd.Parameters.AddWithValue("@cId", idCategoria);
                    var res = cmd.ExecuteScalar();
                    if (res != DBNull.Value && res != null)
                    {
                        total = Convert.ToDouble(res);
                    }
                }
            }
            return total;
        }

        private (string SKU, string Nombre) ObtenerProductoPrincipalCategoria(int idCategoria)
        {
            string sku = "";
            string nombre = "";
            using (var conexion = conexionDB.ObtenerConexion())
            {
                conexion.Open();
                string sql = "SELECT SKU, Nombre FROM Productos WHERE IdCategoria = @cId AND Stock > 0 ORDER BY Id ASC LIMIT 1";
                using (var cmd = new SQLiteCommand(sql, conexion))
                {
                    cmd.Parameters.AddWithValue("@cId", idCategoria);
                    using (var lector = cmd.ExecuteReader())
                    {
                        if (lector.Read())
                        {
                            sku = lector["SKU"]?.ToString() ?? "";
                            nombre = lector["Nombre"]?.ToString() ?? "";
                        }
                    }
                }
            }
            return (sku, nombre);
        }

        public void ConfirmarDistribucion(ListaEnlazada detalles, string observaciones)
        {
            using (var conexion = conexionDB.ObtenerConexion())
            {
                conexion.Open();
                using (var tr = conexion.BeginTransaction())
                {
                    try
                    {
                        // 1. Crear la Orden
                        string sqlOrden = "INSERT INTO Orden (FechaGeneracion, Estado, Observaciones) VALUES (@fecha, 'CONFIRMADA', @obs); SELECT last_insert_rowid();";
                        long ordenId = 0;
                        using (var cmdO = new SQLiteCommand(sqlOrden, conexion, tr))
                        {
                            cmdO.Parameters.AddWithValue("@fecha", RelojDemo.Ahora.ToString("yyyy-MM-dd HH:mm:ss"));
                            cmdO.Parameters.AddWithValue("@obs", observaciones);
                            ordenId = (long)cmdO.ExecuteScalar();
                        }

                        // 2. Guardar Detalles y Descontar Stock
                        string sqlDetalle = @"
                            INSERT INTO OrdenDetalle (OrdenId, BeneficiarioId, CategoriaId, CantidadAsignada, DeficitCalculado) 
                            VALUES (@oId, @bId, @cId, @cant, @def)";
                            
                        // Descuento de stock en cascada a los productos de la categoria (estrategia FIFO por ID de producto)
                        string sqlProdSelect = "SELECT Id, Stock FROM Productos WHERE IdCategoria = @cId AND Stock > 0 ORDER BY Id ASC";
                        string sqlProdUpdate = "UPDATE Productos SET Stock = @nuevoStock WHERE Id = @pId";

                        for (int i = 0; i < detalles.Conteo(); i++)
                        {
                            var det = (OrdenDetalle)detalles.Obtener(i)!;
                            
                            using (var cmdD = new SQLiteCommand(sqlDetalle, conexion, tr))
                            {
                                cmdD.Parameters.AddWithValue("@oId", ordenId);
                                cmdD.Parameters.AddWithValue("@bId", det.BeneficiarioId);
                                cmdD.Parameters.AddWithValue("@cId", det.CategoriaId);
                                cmdD.Parameters.AddWithValue("@cant", det.CantidadAsignada);
                                cmdD.Parameters.AddWithValue("@def", det.DeficitCalculado);
                                cmdD.ExecuteNonQuery();
                            }
                            
                            // Descontar inventario
                            double aDescontar = det.CantidadAsignada;
                            
                            // Buscamos productos para descontar
                            ListaEnlazada prodsADescontar = new ListaEnlazada();
                            using (var cmdSel = new SQLiteCommand(sqlProdSelect, conexion, tr))
                            {
                                cmdSel.Parameters.AddWithValue("@cId", det.CategoriaId);
                                using (var lector = cmdSel.ExecuteReader())
                                {
                                    while (lector.Read())
                                    {
                                        prodsADescontar.Agregar(new ProductoInfo { Id = Convert.ToInt32(lector["Id"]), Stock = Convert.ToDouble(lector["Stock"]) });
                                    }
                                }
                            }
                            
                            for (int j = 0; j < prodsADescontar.Conteo() && aDescontar > 0; j++)
                            {
                                var pInfo = (ProductoInfo)prodsADescontar.Obtener(j)!;
                                int pId = pInfo.Id;
                                double pStock = pInfo.Stock;
                                
                                double descuentoActual = Math.Min(pStock, aDescontar);
                                double nuevoStock = pStock - descuentoActual;
                                aDescontar -= descuentoActual;
                                
                                using (var cmdUpd = new SQLiteCommand(sqlProdUpdate, conexion, tr))
                                {
                                    cmdUpd.Parameters.AddWithValue("@nuevoStock", nuevoStock);
                                    cmdUpd.Parameters.AddWithValue("@pId", pId);
                                    cmdUpd.ExecuteNonQuery();
                                }
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

        private class InfoDistribucion
        {
            public Beneficiario Beneficiario { get; set; } = null!;
            public double Deficit { get; set; }
            public double Asignado { get; set; }
            public string ExplicacionCalculo { get; set; } = "";
        }
        
        private class ProductoInfo
        {
            public int Id { get; set; }
            public double Stock { get; set; }
        }
    }
}
