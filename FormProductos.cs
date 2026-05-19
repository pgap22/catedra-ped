using System;
using System.Drawing;
using System.Windows.Forms;
using ProyectoCatedra.Servicios;
using ProyectoCatedra.Modelos;
using ProyectoCatedra.Estructuras;
using ProyectoCatedra.Utilidades;

namespace ProyectoCatedra
{
    public class FormProductos : Form
    {
        private ProductoServicio servicio;
        private CategoriaServicio catServicio;
        private Pila undoStack;
        private TextBox txtSKU = new TextBox();
        private TextBox txtNombre = new TextBox();
        private NumericUpDown numStock = new NumericUpDown();
        private NumericUpDown numMaxEntrega = new NumericUpDown();
        private NumericUpDown numDiasReposicion = new NumericUpDown();
        private ComboBox cbCat = new ComboBox();
        private DataGridView dgv = new DataGridView();
        private Button btnGuardar = new Button();
        private Button btnEliminar = new Button();
        private Button btnUndo = new Button();
        private Button btnNuevo = new Button();
        private Producto? productoSeleccionado;

        public FormProductos()
        {
            servicio = new ProductoServicio();
            catServicio = new CategoriaServicio();
            undoStack = new Pila();
            InicializarComponentes();
            CargarCats(); Cargar();
        }

        private void InicializarComponentes()
        {
            this.AutoScaleMode = AutoScaleMode.None;
            this.Text = "Inventario de Productos";
            this.Size = new Size(800, 520);
            this.MinimumSize = new Size(760, 480);
            this.StartPosition = FormStartPosition.CenterParent;

            btnNuevo.Text = "Nuevo Producto"; btnNuevo.Location = new Point(20, 15); btnNuevo.Size = new Size(120, 25);
            btnNuevo.Click += (s, e) => Limpiar();

            Label l1 = new Label { Text = "SKU:", Location = new Point(20, 45), AutoSize = true };
            txtSKU.Location = new Point(20, 65); txtSKU.Size = new Size(80, 20); txtSKU.ReadOnly = true; txtSKU.Text = "(Auto-generado)";
            Label l2 = new Label { Text = "Nombre:", Location = new Point(110, 45), AutoSize = true };
            txtNombre.Location = new Point(110, 65); txtNombre.Size = new Size(150, 20);
            Label l3 = new Label { Text = "Categoría:", Location = new Point(270, 45), AutoSize = true };
            cbCat.Location = new Point(270, 65); cbCat.Size = new Size(120, 20); cbCat.DropDownStyle = ComboBoxStyle.DropDownList;
            Label l4 = new Label { Text = "Stock:", Location = new Point(400, 45), AutoSize = true };
            numStock.Location = new Point(400, 65); numStock.Size = new Size(80, 20);
            numStock.Maximum = decimal.MaxValue;
            numStock.DecimalPlaces = 2;

            Label l5 = new Label { Text = "Máx. por entrega:", Location = new Point(490, 45), AutoSize = true };
            numMaxEntrega.Location = new Point(490, 65); numMaxEntrega.Size = new Size(90, 20);
            numMaxEntrega.Maximum = decimal.MaxValue;
            numMaxEntrega.DecimalPlaces = 2;

            Label l6 = new Label { Text = "Días de espera:", Location = new Point(590, 45), AutoSize = true };
            numDiasReposicion.Location = new Point(590, 65); numDiasReposicion.Size = new Size(90, 20);
            numDiasReposicion.Maximum = 3650;

            ToolTip ayudaReglas = new ToolTip();
            ayudaReglas.SetToolTip(numMaxEntrega, "Cantidad máxima que una familia puede recibir de este producto en una entrega. Use 0 para no limitar.");
            ayudaReglas.SetToolTip(l5, "Cantidad máxima que una familia puede recibir de este producto en una entrega. Use 0 para no limitar.");
            ayudaReglas.SetToolTip(numDiasReposicion, "Días que deben pasar antes de sugerir otra entrega del mismo producto a la misma familia. Use 0 si no aplica.");
            ayudaReglas.SetToolTip(l6, "Días que deben pasar antes de sugerir otra entrega del mismo producto a la misma familia. Use 0 si no aplica.");

            btnGuardar.Text = "Guardar"; btnGuardar.Location = new Point(690, 63); btnGuardar.Size = new Size(80, 25);
            btnGuardar.Click += (s, e) =>
            {
                if (cbCat.SelectedItem == null || string.IsNullOrWhiteSpace(txtNombre.Text))
                {
                    MessageBox.Show("Complete nombre y seleccione una categoría.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                try
                {
                    string nombreNuevo = txtNombre.Text.Trim();
                    int idCategoria = ((Categoria)cbCat.SelectedItem).Id;
                    double stockNuevo = (double)numStock.Value;
                    double? maxEntrega = numMaxEntrega.Value > 0 ? (double)numMaxEntrega.Value : null;
                    int? diasReposicion = numDiasReposicion.Value > 0 ? (int)numDiasReposicion.Value : null;

                    if (productoSeleccionado != null)
                    {
                        Producto anterior = CopiarProducto(productoSeleccionado);
                        productoSeleccionado.Nombre = nombreNuevo;
                        productoSeleccionado.IdCategoria = idCategoria;
                        productoSeleccionado.Stock = stockNuevo;
                        productoSeleccionado.MaximoPorEntrega = maxEntrega;
                        productoSeleccionado.DiasReposicion = diasReposicion;

                        servicio.Actualizar(productoSeleccionado);
                        undoStack.Empujar(new AccionUndo(TipoAccion.Editar, "Productos", anterior));
                    }
                    else
                    {
                        if (string.IsNullOrWhiteSpace(txtSKU.Text) || txtSKU.Text == "(Auto-generado)") txtSKU.Text = ObtenerSiguienteSKU();

                        string skuIngresado = txtSKU.Text.Trim();
                        Producto? existenteSku = (Producto?)servicio.CargarEnHash().Buscar(skuIngresado);

                        if (existenteSku != null)
                        {
                            bool mismoNombre = SonIguales(existenteSku.Nombre, nombreNuevo);
                            bool mismaCategoria = existenteSku.IdCategoria == idCategoria;

                            if (mismoNombre && mismaCategoria)
                            {
                                Producto anterior = CopiarProducto(existenteSku);
                                Producto suma = new Producto
                                {
                                    SKU = existenteSku.SKU,
                                    Nombre = existenteSku.Nombre,
                                    IdCategoria = existenteSku.IdCategoria,
                                    Stock = stockNuevo,
                                    MaximoPorEntrega = existenteSku.MaximoPorEntrega,
                                    DiasReposicion = existenteSku.DiasReposicion
                                };
                                servicio.GuardarOSumarStock(suma);
                                undoStack.Empujar(new AccionUndo(TipoAccion.Editar, "Productos", anterior));
                                MessageBox.Show("Coincidió SKU, nombre y categoría. Se sumó stock al producto existente.", "Stock acumulado", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                            else
                            {
                                string skuNuevo = ObtenerSiguienteSKU();
                                var pNuevo = new Producto { SKU = skuNuevo, Nombre = nombreNuevo, IdCategoria = idCategoria, Stock = stockNuevo, MaximoPorEntrega = maxEntrega, DiasReposicion = diasReposicion };
                                servicio.GuardarOSumarStock(pNuevo);
                                undoStack.Empujar(new AccionUndo(TipoAccion.Insertar, "Productos", pNuevo));
                                MessageBox.Show("El SKU ya estaba en uso. Se generó un SKU nuevo.", "SKU reasignado", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                        }
                        else
                        {
                            var p = new Producto { SKU = skuIngresado, Nombre = nombreNuevo, IdCategoria = idCategoria, Stock = stockNuevo, MaximoPorEntrega = maxEntrega, DiasReposicion = diasReposicion };
                            servicio.GuardarOSumarStock(p);
                            undoStack.Empujar(new AccionUndo(TipoAccion.Insertar, "Productos", p));
                        }
                    }

                    Limpiar(); Cargar();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ocurrió un error al guardar: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

            TextBox txtBuscar = new TextBox { Location = new Point(20, 105), Size = new Size(250, 20) };
            Button btnBuscar = new Button { Text = "Buscar", Location = new Point(280, 103), Size = new Size(80, 25) };
            btnBuscar.Click += (s, e) => Cargar(txtBuscar.Text);
            
            btnEliminar.Text = "Eliminar"; btnEliminar.Location = new Point(690, 95); btnEliminar.Size = new Size(80, 25); btnEliminar.Enabled = false;
            btnEliminar.Click += (s, e) =>
            {
                if (productoSeleccionado == null) return;
                if (MessageBox.Show("¿Eliminar este producto del inventario?", "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
                 
                try
                {
                    Producto anterior = CopiarProducto(productoSeleccionado);
                    undoStack.Empujar(new AccionUndo(TipoAccion.Eliminar, "Productos", anterior));
                    servicio.Eliminar(productoSeleccionado.SKU);
                    Limpiar(); Cargar();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("No se pudo eliminar el producto. Puede estar usado en packs o historial de distribuciones.\n\nDetalle: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

            dgv.Location = new Point(20, 140);
            dgv.Size = new Size(740, 270);
            dgv.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dgv.AllowUserToAddRows = false;
            dgv.ReadOnly = true;
            dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgv.MultiSelect = false;
            dgv.Columns.Add("SKU", "SKU");
            dgv.Columns.Add("Nombre", "Nombre");
            dgv.Columns.Add("Categoria", "Categoría");
            dgv.Columns.Add("Stock", "Stock");
            dgv.Columns.Add("MaxEntrega", "Máx. por entrega");
            dgv.Columns.Add("DiasReposicion", "Días de espera");
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgv.CellClick += (s, e) => {
                if (e.RowIndex >= 0) {
                    var row = dgv.Rows[e.RowIndex];
                    string sku = row.Cells[0].Value?.ToString() ?? "";
                    var producto = (Producto?)servicio.CargarEnHash().Buscar(sku);
                    if (producto != null) {
                        productoSeleccionado = producto;
                        txtSKU.Text = producto.SKU;
                        txtNombre.Text = producto.Nombre;
                        SeleccionarCategoriaPorId(producto.IdCategoria);
                        numStock.Value = (decimal)producto.Stock;
                        numMaxEntrega.Value = producto.MaximoPorEntrega.HasValue ? (decimal)producto.MaximoPorEntrega.Value : 0;
                        numDiasReposicion.Value = producto.DiasReposicion.HasValue ? producto.DiasReposicion.Value : 0;
                        btnGuardar.Text = "Guardar";
                        btnEliminar.Enabled = true;
                    }
                }
            };

            btnUndo.Text = "Deshacer último cambio manual"; btnUndo.Location = new Point(20, 430); btnUndo.Size = new Size(180, 30);
            btnUndo.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnUndo.Click += (s, e) => 
            {
                if (undoStack.EstaVacia())
                {
                    MessageBox.Show("No hay cambios para deshacer.", "Deshacer", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                try
                {
                    AccionUndo accion = (AccionUndo)undoStack.Pop()!;
                    if (accion.Tipo == TipoAccion.Insertar)
                    {
                        Producto p = (Producto)accion.Datos;
                        servicio.Eliminar(p.SKU);
                    }
                    else if (accion.Tipo == TipoAccion.Editar)
                    {
                        Producto p = (Producto)accion.Datos;
                        servicio.Actualizar(p);
                    }
                    else if (accion.Tipo == TipoAccion.Eliminar)
                    {
                        Producto p = (Producto)accion.Datos;
                        servicio.GuardarOSumarStock(p);
                    }
                    else if (accion.Tipo == TipoAccion.Importacion)
                    {
                        ListaEnlazada importados = (ListaEnlazada)accion.Datos;
                        for (int i = 0; i < importados.Conteo(); i++)
                        {
                            Producto p = (Producto)importados.Obtener(i)!;
                            servicio.Eliminar(p.SKU);
                        }
                    }
                    Limpiar(); Cargar();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ocurrió un error al deshacer: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

            Button btnImp = new Button { Text = "Importar CSV", Location = new Point(210, 430), Size = new Size(100, 30) };
            btnImp.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnImp.Click += (s, e) => 
            {
                OpenFileDialog ofd = new OpenFileDialog { Filter = "Archivo CSV (*.csv)|*.csv" };
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        ListaEnlazada productosCsv = ManejadorCSV.ParsearProductos(ofd.FileName);
                        int totalCsv = ManejadorCSV.ContarFilasDatos(ofd.FileName);
                        int invalidos = totalCsv - productosCsv.Conteo();
                        TablaHash hashActuales = servicio.CargarEnHash();
                        ListaEnlazada insertados = new ListaEnlazada();
                        int omitidos = 0;

                        for (int i = 0; i < productosCsv.Conteo(); i++)
                        {
                            Producto p = (Producto)productosCsv.Obtener(i)!;
                            
                            if (string.IsNullOrWhiteSpace(p.SKU) || string.IsNullOrWhiteSpace(p.Nombre) || p.Stock < 0)
                            {
                                omitidos++;
                                continue;
                            }

                            int idCat = catServicio.ObtenerIdPorNombre(p.NombreCategoria);
                            if (idCat == -1)
                            {
                                omitidos++;
                                continue;
                            }
                            
                            p.IdCategoria = idCat;

                            if (hashActuales.Buscar(p.SKU) != null)
                            {
                                omitidos++;
                                continue;
                            }

                            servicio.GuardarOSumarStock(p);
                            insertados.Agregar(p);
                            hashActuales.Insertar(p.SKU, p);
                        }

                        if (insertados.Conteo() > 0)
                        {
                            undoStack.Empujar(new AccionUndo(TipoAccion.Importacion, "Productos", insertados));
                        }

                        MessageBox.Show($"Importación finalizada.\nInsertados: {insertados.Conteo()}\nDuplicados o sin categoría: {omitidos}\nInválidos o vacíos: {invalidos}", "Importar", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        Limpiar(); Cargar();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error al importar: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            };
            Button btnPlantilla = new Button { Text = "Bajar Plantilla", Location = new Point(320, 430), Size = new Size(110, 30) };
            btnPlantilla.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnPlantilla.Click += (s, e) => ManejadorCSV.GuardarPlantillaConDialogo("plantilla_productos.csv", "SKU,Nombre,NombreCategoria,Stock\nSKU001,Arroz,Granos Basicos,50\nSKU002,Leche,Lacteos,100");

            Label lblStockBajo = new Label
            {
                Text = "0 en reglas = sin límite/sin espera. Filas rojas = stock bajo.",
                Location = new Point(450, 435),
                AutoSize = true,
                ForeColor = Color.DarkRed
            };

            this.Controls.AddRange(new Control[] { btnNuevo, l1, txtSKU, l2, txtNombre, l3, cbCat, l4, numStock, l5, numMaxEntrega, l6, numDiasReposicion, btnGuardar, btnEliminar, txtBuscar, btnBuscar, btnUndo, dgv, btnImp, btnPlantilla, lblStockBajo });
            AplicarEscaladoDpi();
        }

        private void AplicarEscaladoDpi()
        {
            float factor = DeviceDpi / 96f;
            if (factor <= 1f) return;

            this.Size = new Size((int)Math.Round(this.Width * factor), (int)Math.Round(this.Height * factor));
            EscaladorDpi.EscalarJerarquia(this, factor);
        }

        private void GenerarNuevoSKU()
        {
            Limpiar();
        }

        private void CargarCats() { cbCat.Items.Clear(); var l = catServicio.ListarTodas(); for (int i = 0; i < l.Conteo(); i++) { var c = l.Obtener(i); if (c != null) cbCat.Items.Add(c); } }
        private void Cargar(string filtro = "") 
        { 
            dgv.Rows.Clear(); 
            var l = servicio.ListarTodos(); 
            filtro = filtro.Trim().ToLower();

            for (int i = 0; i < l.Conteo(); i++) 
            { 
                var p = (Producto?)l.Obtener(i); 
                if (p != null) 
                {
                    if (!string.IsNullOrEmpty(filtro) && !p.Nombre.ToLower().Contains(filtro) && !p.SKU.ToLower().Contains(filtro)) continue;

                    int index = dgv.Rows.Add(p.SKU, p.Nombre, p.NombreCategoria, p.Stock, p.MaximoPorEntrega?.ToString() ?? "", p.DiasReposicion?.ToString() ?? ""); 
                    if (p.Stock <= 5)
                    {
                        dgv.Rows[index].DefaultCellStyle.BackColor = Color.LightCoral;
                    }
                } 
            } 
        }
        private void Limpiar()
        {
            productoSeleccionado = null;
            txtSKU.Text = "(Auto-generado)";
            txtNombre.Clear();
            numStock.Value = 0;
            numMaxEntrega.Value = 0;
            numDiasReposicion.Value = 0;
            cbCat.SelectedIndex = -1;
            btnEliminar.Enabled = false;
            btnGuardar.Text = "Guardar";
            dgv.ClearSelection();
        }

        private int ExtraerCorrelativoSKU(string sku)
        {
            if (string.IsNullOrWhiteSpace(sku)) return -1;

            int inicioDigitos = -1;
            for (int i = 0; i < sku.Length; i++)
            {
                if (char.IsDigit(sku[i]))
                {
                    inicioDigitos = i;
                    break;
                }
            }

            if (inicioDigitos == -1) return -1;

            string parteNumerica = sku.Substring(inicioDigitos);
            if (int.TryParse(parteNumerica, out int numero)) return numero;

            return -1;
        }

        private string ObtenerSiguienteSKU()
        {
            var l = servicio.ListarTodos();
            if (l.Conteo() == 0) return "SKU001";

            MonticuloMaximo heap = new MonticuloMaximo();
            for (int i = 0; i < l.Conteo(); i++)
            {
                var p = (Producto?)l.Obtener(i);
                if (p == null) continue;

                int numeroSku = ExtraerCorrelativoSKU(p.SKU);
                if (numeroSku > 0) heap.Insertar(numeroSku, numeroSku);
            }

            int max = (int)(heap.VerMaximo() ?? 0);
            return "SKU" + (max + 1).ToString("D3");
        }

        private bool SonIguales(string a, string b)
        {
            return NormalizarNombre(a) == NormalizarNombre(b);
        }

        private string NormalizarNombre(string nombre)
        {
            return nombre.Trim().ToLower();
        }

        private void SeleccionarCategoriaPorId(int idCategoria)
        {
            for (int i = 0; i < cbCat.Items.Count; i++)
            {
                Categoria? c = (Categoria?)cbCat.Items[i];
                if (c != null && c.Id == idCategoria)
                {
                    cbCat.SelectedIndex = i;
                    return;
                }
            }

            cbCat.SelectedIndex = -1;
        }

        private Producto CopiarProducto(Producto p)
        {
            return new Producto
            {
                SKU = p.SKU,
                Nombre = p.Nombre,
                IdCategoria = p.IdCategoria,
                Stock = p.Stock,
                MaximoPorEntrega = p.MaximoPorEntrega,
                DiasReposicion = p.DiasReposicion,
                NombreCategoria = p.NombreCategoria
            };
        }
    }
}
