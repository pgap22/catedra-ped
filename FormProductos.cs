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
            this.StartPosition = FormStartPosition.CenterParent;

            btnNuevo.Text = "Nuevo (Auto-SKU)"; btnNuevo.Location = new Point(20, 15); btnNuevo.Size = new Size(120, 25);
            btnNuevo.Click += (s, e) => GenerarNuevoSKU();

            Label l1 = new Label { Text = "SKU:", Location = new Point(20, 45), AutoSize = true };
            txtSKU.Location = new Point(20, 65); txtSKU.Size = new Size(80, 20);
            Label l2 = new Label { Text = "Nombre:", Location = new Point(110, 45), AutoSize = true };
            txtNombre.Location = new Point(110, 65); txtNombre.Size = new Size(150, 20);
            Label l3 = new Label { Text = "Categoría:", Location = new Point(270, 45), AutoSize = true };
            cbCat.Location = new Point(270, 65); cbCat.Size = new Size(120, 20); cbCat.DropDownStyle = ComboBoxStyle.DropDownList;
            Label l4 = new Label { Text = "Stock:", Location = new Point(400, 45), AutoSize = true };
            numStock.Location = new Point(400, 65); numStock.Size = new Size(60, 20);

            btnGuardar.Text = "Guardar"; btnGuardar.Location = new Point(470, 63); btnGuardar.Size = new Size(80, 25);
            btnGuardar.Click += (s, e) =>
            {
                if (cbCat.SelectedItem == null || string.IsNullOrWhiteSpace(txtNombre.Text)) return;

                string nombreNuevo = txtNombre.Text.Trim();
                int idCategoria = ((Categoria)cbCat.SelectedItem).Id;
                double stockNuevo = (double)numStock.Value;

                if (productoSeleccionado != null)
                {
                    Producto anterior = CopiarProducto(productoSeleccionado);
                    productoSeleccionado.Nombre = nombreNuevo;
                    productoSeleccionado.IdCategoria = idCategoria;
                    productoSeleccionado.Stock = stockNuevo;

                    servicio.Actualizar(productoSeleccionado);
                    undoStack.Empujar(new AccionUndo(TipoAccion.Editar, "Productos", anterior));
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(txtSKU.Text)) txtSKU.Text = ObtenerSiguienteSKU();

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
                                Stock = stockNuevo
                            };
                            servicio.GuardarOSumarStock(suma);
                            undoStack.Empujar(new AccionUndo(TipoAccion.Editar, "Productos", anterior));
                            MessageBox.Show("Coincidio SKU, nombre y categoria. Se sumo stock al producto existente.", "Stock acumulado", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            string skuNuevo = ObtenerSiguienteSKU();
                            var pNuevo = new Producto { SKU = skuNuevo, Nombre = nombreNuevo, IdCategoria = idCategoria, Stock = stockNuevo };
                            servicio.GuardarOSumarStock(pNuevo);
                            undoStack.Empujar(new AccionUndo(TipoAccion.Insertar, "Productos", pNuevo));
                            MessageBox.Show("El SKU ya estaba en uso con otro nombre o categoria. Se genero un SKU nuevo para crear otro producto.", "SKU reasignado", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                    else
                    {
                        var p = new Producto { SKU = skuIngresado, Nombre = nombreNuevo, IdCategoria = idCategoria, Stock = stockNuevo };
                        servicio.GuardarOSumarStock(p);
                        undoStack.Empujar(new AccionUndo(TipoAccion.Insertar, "Productos", p));
                    }
                }

                Limpiar(); Cargar();
            };

            btnEliminar.Text = "Eliminar"; btnEliminar.Location = new Point(560, 63); btnEliminar.Size = new Size(80, 25); btnEliminar.Enabled = false;
            btnEliminar.Click += (s, e) =>
            {
                if (productoSeleccionado == null) return;

                Producto anterior = CopiarProducto(productoSeleccionado);
                undoStack.Empujar(new AccionUndo(TipoAccion.Eliminar, "Productos", anterior));
                servicio.Eliminar(productoSeleccionado.SKU);
                Limpiar(); Cargar(); 
            };

            btnUndo.Text = "Deshacer"; btnUndo.Location = new Point(20, 430); btnUndo.Size = new Size(100, 30);
            btnUndo.Click += (s, e) =>
            {
                var acc = (AccionUndo?)undoStack.Pop();
                if (acc == null) return;
                
                if (acc.Tipo == TipoAccion.Importacion)
                {
                    var l = (ListaEnlazada)acc.Datos;
                    for (int i = 0; i < l.Conteo(); i++)
                    {
                        var pImp = (Producto?)l.Obtener(i);
                        if (pImp != null) servicio.Eliminar(pImp.SKU);
                    }
                }
                else 
                {
                    var p = (Producto)acc.Datos;
                    if (acc.Tipo == TipoAccion.Insertar) servicio.Eliminar(p.SKU);
                    else if (acc.Tipo == TipoAccion.Eliminar) servicio.GuardarOSumarStock(p);
                    else if (acc.Tipo == TipoAccion.Editar) servicio.Actualizar(p);
                }
                Cargar();
            };

            dgv.Location = new Point(20, 100); dgv.Size = new Size(740, 310); dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect; dgv.ReadOnly = true; dgv.AllowUserToAddRows = false; dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgv.Columns.Add("SKU", "SKU"); dgv.Columns.Add("Nom", "Nombre"); dgv.Columns.Add("Cat", "Categoría"); dgv.Columns.Add("Stock", "Stock");
            dgv.SelectionChanged += (s, e) =>
            {
                if (dgv.SelectedRows.Count == 0) return;

                string sku = dgv.SelectedRows[0].Cells[0].Value?.ToString() ?? "";
                if (string.IsNullOrWhiteSpace(sku)) return;

                Producto? p = (Producto?)servicio.CargarEnHash().Buscar(sku);
                if (p == null) return;

                productoSeleccionado = p;
                txtSKU.Text = p.SKU;
                txtNombre.Text = p.Nombre;
                numStock.Value = Convert.ToDecimal(p.Stock);
                SeleccionarCategoriaPorId(p.IdCategoria);

                btnEliminar.Enabled = true;
                btnGuardar.Text = "Actualizar";
            };

            Button btnImp = new Button { Text = "Importar CSV", Location = new Point(130, 430), Size = new Size(100, 30) };
            btnImp.Click += (s, e) =>
            {
                try
                {
                    OpenFileDialog ofd = new OpenFileDialog();
                    if (ofd.ShowDialog() == DialogResult.OK)
                    {
                        var l = ManejadorCSV.ParsearProductos(ofd.FileName);
                        ListaEnlazada importados = new ListaEnlazada();
                        for (int i = 0; i < l.Conteo(); i++)
                        {
                            var p = (Producto?)l.Obtener(i);
                            if (p == null) continue;
                            int cid = catServicio.ObtenerIdPorNombre(p.NombreCategoria);
                            if (cid != -1) { p.IdCategoria = cid; servicio.GuardarOSumarStock(p); importados.Agregar(p); }
                        }
                        undoStack.Empujar(new AccionUndo(TipoAccion.Importacion, "Productos", importados));
                        Cargar();
                    }
                }
                catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); }
            };

            Button btnPlantilla = new Button { Text = "Bajar Plantilla", Location = new Point(240, 430), Size = new Size(110, 30) };
            btnPlantilla.Click += (s, e) => ManejadorCSV.GuardarPlantillaConDialogo("plantilla_productos.csv", "SKU,Nombre,NombreCategoria,Stock\nSKU001,Arroz,Granos Basicos,50\nSKU002,Leche,Lacteos,100");

            this.Controls.AddRange(new Control[] { btnNuevo, l1, txtSKU, l2, txtNombre, l3, cbCat, l4, numStock, btnGuardar, btnEliminar, btnUndo, dgv, btnImp, btnPlantilla });
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
            txtSKU.Text = ObtenerSiguienteSKU();
        }

        private void CargarCats() { cbCat.Items.Clear(); var l = catServicio.ListarTodas(); for (int i = 0; i < l.Conteo(); i++) { var c = l.Obtener(i); if (c != null) cbCat.Items.Add(c); } }
        private void Cargar() { dgv.Rows.Clear(); var l = servicio.ListarTodos(); for (int i = 0; i < l.Conteo(); i++) { var p = (Producto?)l.Obtener(i); if (p != null) dgv.Rows.Add(p.SKU, p.Nombre, p.NombreCategoria, p.Stock); } }
        private void Limpiar()
        {
            productoSeleccionado = null;
            txtSKU.Clear();
            txtNombre.Clear();
            numStock.Value = 0;
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
                NombreCategoria = p.NombreCategoria
            };
        }
    }
}
