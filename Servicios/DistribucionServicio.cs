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
        private const int DiasPrimeraEntregaAlta = 14;
        private const int DiasPrimeraEntregaMedia = 7;
        private const int DiasPrimeraEntregaBaja = 4;

        private ConexionDB conexionDB;
        private BeneficiarioServicio beneficiarioServicio;
        private ProductoServicio productoServicio;
        private TasaConsumoServicio tasaServicio;
        private CategoriaPackServicio packServicio;
        private HistorialServicio historialServicio;

        public DistribucionServicio()
        {
            conexionDB = new ConexionDB();
            beneficiarioServicio = new BeneficiarioServicio();
            productoServicio = new ProductoServicio();
            tasaServicio = new TasaConsumoServicio();
            packServicio = new CategoriaPackServicio();
            historialServicio = new HistorialServicio();
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
                
                var pack = packServicio.ListarPorCategoria(tasa.IdCategoria);
                if (pack.Conteo() == 0) continue;
                if (!packServicio.EsPackValido(tasa.IdCategoria))
                {
                    throw new InvalidOperationException($"El pack de la categoría '{tasa.NombreCategoria}' debe sumar exactamente 100% antes de distribuir.");
                }

                // El stock útil es el stock de los productos incluidos en el pack.
                double stockDisponible = ObtenerStockTotalPack(pack);
                if (stockDisponible <= 0) continue; // Skip si no hay nada que dar

                MonticuloMaximo heap = new MonticuloMaximo(beneficiarios.Conteo());

                for (int j = 0; j < beneficiarios.Conteo(); j++)
                {
                    var b = (Beneficiario)beneficiarios.Obtener(j)!;
                    if (!b.Activo) continue; // Solo activos

                    var resultadoDeficit = CalcularDeficit(b, tasa);
                    double deficit = resultadoDeficit.Deficit;
                    if (deficit <= 0) continue;
                     
                    decimal prioridad = ObtenerPesoVulnerabilidad(b.NivelVulnerabilidad)
                                      + Math.Round((decimal)deficit, 4) 
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
                    
                    double demanda = info.Deficit;

                    double aAsignar = Math.Floor(Math.Min(demanda, stockDisponible));
                    if (aAsignar < 1)
                    {
                        if (stockDisponible < 1) break;
                        continue;
                    }
                    
                    if (aAsignar > 0.01)
                    {
                        var resultadoSplit = CrearLineasProducto(info, tasa, pack, aAsignar);
                        if (resultadoSplit.TotalFisicoAsignado < 1) continue;

                        info.Asignado += resultadoSplit.TotalFisicoAsignado;
                        stockDisponible -= resultadoSplit.TotalFisicoAsignado;

                        for (int k = 0; k < resultadoSplit.Lineas.Conteo(); k++)
                        {
                            propuesta.Agregar(resultadoSplit.Lineas.Obtener(k)!);
                        }
                    }
                }
            }
            
            return propuesta;
        }

        public string ObtenerDiagnosticoSinPropuesta(int categoriaIdFiltro = 0)
        {
            var beneficiarios = beneficiarioServicio.ListarTodos();
            if (beneficiarios.Conteo() == 0) return "No hay beneficiarios registrados.";

            int activos = ContarBeneficiariosActivos(beneficiarios);
            if (activos == 0) return "Hay beneficiarios registrados, pero ninguno está activo.";

            var tasas = tasaServicio.ListarTodas();
            if (tasas.Conteo() == 0) return "No hay tasas de consumo configuradas.";

            bool hayTasaProcesada = false;
            bool hayPack = false;
            bool hayStock = false;
            bool hayDeficit = false;
            bool hayCantidadEntera = false;
            string primeraCategoriaSinPack = "";

            for (int i = 0; i < tasas.Conteo(); i++)
            {
                var tasa = (TasaConsumo)tasas.Obtener(i)!;
                if (categoriaIdFiltro > 0 && tasa.IdCategoria != categoriaIdFiltro) continue;

                hayTasaProcesada = true;
                var pack = packServicio.ListarPorCategoria(tasa.IdCategoria);
                if (pack.Conteo() == 0)
                {
                    if (string.IsNullOrEmpty(primeraCategoriaSinPack)) primeraCategoriaSinPack = tasa.NombreCategoria;
                    continue;
                }

                hayPack = true;
                if (!packServicio.EsPackValido(tasa.IdCategoria))
                {
                    double total = packServicio.ObtenerTotalPorcentaje(tasa.IdCategoria);
                    return $"El pack de la categoría '{tasa.NombreCategoria}' no suma 100%. Total actual: {total}%.";
                }

                double stockDisponible = ObtenerStockTotalPack(pack);
                if (stockDisponible <= 0) continue;

                hayStock = true;
                for (int j = 0; j < beneficiarios.Conteo(); j++)
                {
                    var b = (Beneficiario)beneficiarios.Obtener(j)!;
                    if (!b.Activo) continue;

                    double deficit = CalcularDeficit(b, tasa).Deficit;
                    if (deficit <= 0) continue;

                    hayDeficit = true;
                    double cantidadCategoria = Math.Floor(Math.Min(deficit, stockDisponible));
                    if (cantidadCategoria < 1) continue;

                    hayCantidadEntera = true;
                    if (PuedeCrearAlMenosUnaLineaProducto(b, pack, cantidadCategoria))
                    {
                        return "Sí hay datos suficientes para generar una propuesta, pero las líneas fueron omitidas por una condición no identificada. Revise reglas de pack, reposición o stock por producto.";
                    }
                }
            }

            if (!hayTasaProcesada) return "La categoría seleccionada no tiene tasa de consumo configurada.";
            if (!hayPack) return string.IsNullOrEmpty(primeraCategoriaSinPack)
                ? "Las categorías con tasa de consumo no tienen packs configurados."
                : $"La categoría '{primeraCategoriaSinPack}' tiene tasa, pero no tiene pack configurado.";
            if (!hayStock) return "Los packs configurados existen, pero sus productos no tienen stock disponible.";
            if (!hayDeficit) return "Los beneficiarios activos no tienen déficit pendiente. Esto pasa si recibieron ayuda recientemente.";
            if (!hayCantidadEntera) return "Hay déficit pendiente, pero es menor a 1 unidad entera. Avance la fecha de demo o espere a que el déficit acumule al menos 1 unidad.";

            return "Hay déficit y stock, pero el pack no generó productos entregables. Revise porcentajes, máximos por entrega y reglas de reposición.";
        }

        private int ContarBeneficiariosActivos(ListaEnlazada beneficiarios)
        {
            int total = 0;
            for (int i = 0; i < beneficiarios.Conteo(); i++)
            {
                var b = (Beneficiario)beneficiarios.Obtener(i)!;
                if (b.Activo) total++;
            }
            return total;
        }

        private bool PuedeCrearAlMenosUnaLineaProducto(Beneficiario beneficiario, ListaEnlazada pack, double cantidadCategoria)
        {
            for (int i = 0; i < pack.Conteo(); i++)
            {
                var lineaPack = (CategoriaPackDetalle)pack.Obtener(i)!;
                double solicitado = Math.Floor(cantidadCategoria * (lineaPack.Porcentaje / 100.0));
                if (solicitado < 1) continue;

                if (lineaPack.DiasReposicion.HasValue)
                {
                    DateTime? ultimaEntrega = historialServicio.ObtenerUltimaEntregaProducto(beneficiario.Id, lineaPack.ProductoId);
                    if (ultimaEntrega.HasValue)
                    {
                        double dias = (RelojDemo.Ahora - ultimaEntrega.Value).TotalDays;
                        if (dias < lineaPack.DiasReposicion.Value) continue;
                    }
                }

                if (lineaPack.MaximoPorEntrega.HasValue && solicitado > lineaPack.MaximoPorEntrega.Value)
                {
                    solicitado = Math.Floor(lineaPack.MaximoPorEntrega.Value);
                }

                if (lineaPack.StockDisponible <= 0) continue;
                if (Math.Min(solicitado, Math.Floor(lineaPack.StockDisponible)) >= 1) return true;
            }

            return false;
        }

        private (double Deficit, string Explicacion) CalcularDeficit(Beneficiario b, TasaConsumo tasa)
        {
            DateTime? ultimaEntrega = historialServicio.ObtenerUltimaEntregaCategoria(b.Id, tasa.IdCategoria);
            DateTime fechaBase = ultimaEntrega ?? RelojDemo.Ahora;
            double dias;
            string fuenteBase;

            if (ultimaEntrega.HasValue)
            {
                if (fechaBase > RelojDemo.Ahora) fechaBase = RelojDemo.Ahora;
                dias = Math.Max(0, (RelojDemo.Ahora - fechaBase).TotalDays);
                fuenteBase = "última entrega confirmada de la categoría";
            }
            else
            {
                dias = ObtenerDiasPrimeraEntrega(b.NivelVulnerabilidad);
                fuenteBase = $"primera entrega / paquete inicial según vulnerabilidad ({dias} días)";
            }

            double deficitFinal = b.MiembrosHogar * tasa.TasaDiaria * dias;
            string detalleBase = ultimaEntrega.HasValue ? $"{fuenteBase} ({fechaBase:yyyy-MM-dd HH:mm})" : fuenteBase;

            string explicacion = $"Cálculo para {b.Nombre}:\n" +
                                 $"- Prioridad social aplicada: {b.VulnerabilidadTexto}\n" +
                                 $"- Miembros del hogar: {b.MiembrosHogar}\n" +
                                 $"- Base del déficit: {detalleBase}\n" +
                                 $"- Días usados para cálculo: {Math.Round(dias, 1)} días\n" +
                                 $"- Tasa de consumo: {tasa.TasaDiaria} por persona/día\n" +
                                 $"Déficit calculado: {Math.Round(deficitFinal, 2)}";

            return (deficitFinal, explicacion);
        }

        private int ObtenerDiasPrimeraEntrega(int nivelVulnerabilidad)
        {
            int nivel = Beneficiario.NormalizarNivelVulnerabilidad(nivelVulnerabilidad);
            if (nivel == Beneficiario.VulnerabilidadAlta) return DiasPrimeraEntregaAlta;
            if (nivel == Beneficiario.VulnerabilidadBaja) return DiasPrimeraEntregaBaja;
            return DiasPrimeraEntregaMedia;
        }

        private decimal ObtenerPesoVulnerabilidad(int nivelVulnerabilidad)
        {
            int nivel = Beneficiario.NormalizarNivelVulnerabilidad(nivelVulnerabilidad);
            if (nivel == Beneficiario.VulnerabilidadAlta) return 100000m;
            if (nivel == Beneficiario.VulnerabilidadBaja) return 10000m;
            return 50000m;
        }

        private (ListaEnlazada Lineas, double TotalFisicoAsignado) CrearLineasProducto(InfoDistribucion info, TasaConsumo tasa, ListaEnlazada pack, double cantidadCategoria)
        {
            ListaEnlazada lineas = new ListaEnlazada();
            double totalFisico = 0;
            string detalleBase = info.ExplicacionCalculo +
                                 $"\n\nPack aplicado a {Math.Floor(cantidadCategoria)} unidades de {tasa.NombreCategoria}:";

            for (int i = 0; i < pack.Conteo(); i++)
            {
                var lineaPack = (CategoriaPackDetalle)pack.Obtener(i)!;
                double solicitado = Math.Floor(cantidadCategoria * (lineaPack.Porcentaje / 100.0));
                string explicacionProducto = detalleBase + $"\n- {lineaPack.NombreProducto}: {lineaPack.Porcentaje}% = {solicitado} unidades enteras.";

                if (solicitado < 1)
                {
                    continue;
                }

                if (lineaPack.DiasReposicion.HasValue)
                {
                    DateTime? ultimaEntrega = historialServicio.ObtenerUltimaEntregaProducto(info.Beneficiario.Id, lineaPack.ProductoId);
                    if (ultimaEntrega.HasValue)
                    {
                        double dias = (RelojDemo.Ahora - ultimaEntrega.Value).TotalDays;
                        if (dias < lineaPack.DiasReposicion.Value)
                        {
                            explicacionProducto += $"\n- Omitido por reposición: recibido hace {Math.Round(dias, 1)} días; regla: {lineaPack.DiasReposicion.Value} días.";
                            continue;
                        }
                    }
                }

                if (lineaPack.MaximoPorEntrega.HasValue && solicitado > lineaPack.MaximoPorEntrega.Value)
                {
                    explicacionProducto += $"\n- Limitado por máximo por entrega: {lineaPack.MaximoPorEntrega.Value}.";
                    solicitado = Math.Floor(lineaPack.MaximoPorEntrega.Value);
                }

                if (lineaPack.StockDisponible <= 0)
                {
                    explicacionProducto += "\n- Omitido por falta de stock del producto.";
                    continue;
                }

                double asignado = Math.Min(solicitado, Math.Floor(lineaPack.StockDisponible));
                if (asignado < solicitado)
                {
                    explicacionProducto += $"\n- Reducido por stock disponible: {lineaPack.StockDisponible}.";
                }

                if (asignado < 1) continue;

                lineaPack.StockDisponible -= asignado;
                totalFisico += asignado;

                lineas.Agregar(new OrdenDetalle
                {
                    BeneficiarioId = info.Beneficiario.Id,
                    NombreBeneficiario = info.Beneficiario.Nombre,
                    NivelVulnerabilidad = info.Beneficiario.NivelVulnerabilidad,
                    VulnerabilidadTexto = info.Beneficiario.VulnerabilidadTexto,
                    CategoriaId = tasa.IdCategoria,
                    NombreCategoria = tasa.NombreCategoria,
                    ProductoId = lineaPack.ProductoId,
                    CantidadAsignada = asignado,
                    DeficitCalculado = Math.Round(info.Deficit, 2),
                    ExplicacionCalculo = explicacionProducto,
                    SKUProductoSugerido = lineaPack.SKUProducto,
                    NombreProductoSugerido = lineaPack.NombreProducto,
                    NombreUnidadMedida = tasa.NombreUnidadBase
                });
            }

            return (lineas, totalFisico);
        }

        private double ObtenerStockTotalPack(ListaEnlazada pack)
        {
            double total = 0;
            for (int i = 0; i < pack.Conteo(); i++)
            {
                var linea = (CategoriaPackDetalle)pack.Obtener(i)!;
                total += Math.Floor(linea.StockDisponible);
            }
            return total;
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

        public double ObtenerStockProducto(int productoId)
        {
            return productoServicio.ObtenerStockProducto(productoId);
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
                            INSERT INTO OrdenDetalle (OrdenId, BeneficiarioId, CategoriaId, ProductoId, CantidadAsignada, DeficitCalculado) 
                            VALUES (@oId, @bId, @cId, @pId, @cant, @def)";
                            
                        // Descuento de stock en cascada a los productos de la categoria (estrategia FIFO por ID de producto)
                        string sqlProdSelect = "SELECT Id, Stock FROM Productos WHERE IdCategoria = @cId AND Stock > 0 ORDER BY Id ASC";
                        string sqlProdUpdate = "UPDATE Productos SET Stock = @nuevoStock WHERE Id = @pId";
                        string sqlProdExacto = "UPDATE Productos SET Stock = Stock - @cant WHERE Id = @pId AND Stock >= @cant";

                        for (int i = 0; i < detalles.Conteo(); i++)
                        {
                            var det = (OrdenDetalle)detalles.Obtener(i)!;
                            
                            using (var cmdD = new SQLiteCommand(sqlDetalle, conexion, tr))
                            {
                                cmdD.Parameters.AddWithValue("@oId", ordenId);
                                cmdD.Parameters.AddWithValue("@bId", det.BeneficiarioId);
                                cmdD.Parameters.AddWithValue("@cId", det.CategoriaId);
                                cmdD.Parameters.AddWithValue("@pId", det.ProductoId > 0 ? det.ProductoId : DBNull.Value);
                                cmdD.Parameters.AddWithValue("@cant", det.CantidadAsignada);
                                cmdD.Parameters.AddWithValue("@def", det.DeficitCalculado);
                                cmdD.ExecuteNonQuery();
                            }
                            
                            if (det.ProductoId > 0)
                            {
                                using (var cmdExacto = new SQLiteCommand(sqlProdExacto, conexion, tr))
                                {
                                    cmdExacto.Parameters.AddWithValue("@cant", det.CantidadAsignada);
                                    cmdExacto.Parameters.AddWithValue("@pId", det.ProductoId);
                                    int filas = cmdExacto.ExecuteNonQuery();
                                    if (filas == 0) throw new InvalidOperationException("No hay stock suficiente para uno de los productos asignados.");
                                }
                                continue;
                            }

                            // Compatibilidad con filas antiguas o flujos sin producto específico.
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
