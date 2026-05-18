using System;
using System.Drawing;
using System.Windows.Forms;
using ProyectoCatedra.Servicios;
using ProyectoCatedra.Modelos;
using ProyectoCatedra.Estructuras;

namespace ProyectoCatedra
{
    public class FormDistribucion : Form
    {
        private DistribucionServicio servicio;
        private Pila undoStack; // Pila para deshacer
        private TablaHash stockPorProducto = new TablaHash(100); // Para validar sobregiro por producto
        
        private DataGridView dgv = new DataGridView();
        private Button btnGenerar = new Button();
        private Button btnConfirmar = new Button();
        private Button btnDeshacer = new Button();
        private TextBox txtObservaciones = new TextBox();
        private ComboBox cbFiltroCategoria = new ComboBox();
        private TextBox txtFiltroBeneficiario = new TextBox();
        private Button btnFiltrar = new Button();
        private Button btnLimpiarFiltro = new Button();
        private CategoriaServicio categoriaServicio = new CategoriaServicio();
        
        private ListaEnlazada? propuestaActual;

        public FormDistribucion()
        {
            servicio = new DistribucionServicio();
            undoStack = new Pila();
            InicializarComponentes();
        }

        private void InicializarComponentes()
        {
            this.AutoScaleMode = AutoScaleMode.None;
            this.Text = "Asignación de Ayuda a Familias";
            this.Size = new Size(800, 530);
            this.MinimumSize = new Size(780, 500);
            this.StartPosition = FormStartPosition.CenterParent;

            btnGenerar.Text = "Generar Propuesta Automática"; btnGenerar.Location = new Point(20, 15); btnGenerar.Size = new Size(180, 30);
            btnGenerar.Click += BtnGenerar_Click;

            btnDeshacer.Text = "Revertir Cambio Manual"; btnDeshacer.Location = new Point(210, 15); btnDeshacer.Size = new Size(150, 30);
            btnDeshacer.Enabled = false;
            btnDeshacer.Click += BtnDeshacer_Click;

            Label lblFiltroCat = new Label { Text = "Categoría:", Location = new Point(20, 60), AutoSize = true };
            cbFiltroCategoria.Location = new Point(80, 57);
            cbFiltroCategoria.Size = new Size(150, 25);
            cbFiltroCategoria.DropDownStyle = ComboBoxStyle.DropDownList;
            CargarFiltroCategorias();

            Label lblFiltroBen = new Label { Text = "Beneficiario:", Location = new Point(240, 60), AutoSize = true };
            txtFiltroBeneficiario.Location = new Point(315, 57);
            txtFiltroBeneficiario.Size = new Size(150, 25);
            
            btnFiltrar.Text = "Aplicar Filtros";
            btnFiltrar.Location = new Point(480, 55);
            btnFiltrar.Size = new Size(100, 28);
            btnFiltrar.Click += BtnFiltrar_Click;

            btnLimpiarFiltro.Text = "Limpiar Filtros";
            btnLimpiarFiltro.Location = new Point(590, 55);
            btnLimpiarFiltro.Size = new Size(100, 28);
            btnLimpiarFiltro.Click += BtnLimpiarFiltro_Click;

            Label lblObs = new Label { Text = "Observaciones:", Location = new Point(370, 20), AutoSize = true };
            txtObservaciones.Location = new Point(460, 17);
            txtObservaciones.Size = new Size(310, 25);

            btnConfirmar.Text = "Confirmar y Guardar Entrega"; btnConfirmar.Location = new Point(590, 95); btnConfirmar.Size = new Size(180, 35);
            btnConfirmar.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnConfirmar.BackColor = Color.LightGreen;
            btnConfirmar.Enabled = false;
            btnConfirmar.Click += BtnConfirmar_Click;

            dgv.Location = new Point(20, 140); 
            dgv.Size = new Size(750, 300); 
            dgv.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dgv.SelectionMode = DataGridViewSelectionMode.CellSelect; 
            dgv.ReadOnly = false; 
            dgv.AllowUserToAddRows = false; 
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            dgv.Columns.Add("Beneficiario", "Beneficiario");
            dgv.Columns["Beneficiario"].ReadOnly = true;
            dgv.Columns.Add("Categoria", "Categoría");
            dgv.Columns["Categoria"].ReadOnly = true;
            dgv.Columns.Add("Producto", "Producto");
            dgv.Columns["Producto"].ReadOnly = true;
            dgv.Columns.Add("SKU", "SKU");
            dgv.Columns["SKU"].ReadOnly = true;
            dgv.Columns.Add("Asignado", "A entregar");
            dgv.Columns.Add("Unidad", "Unidad");
            dgv.Columns["Unidad"].ReadOnly = true;
            dgv.Columns.Add("Deficit", "Déficit Calculado");
            dgv.Columns["Deficit"].ReadOnly = true;
            
            // Hidden columns for IDs
            dgv.Columns.Add("BId", "BId"); dgv.Columns["BId"].Visible = false;
            dgv.Columns.Add("CId", "CId"); dgv.Columns["CId"].Visible = false;
            dgv.Columns.Add("PId", "PId"); dgv.Columns["PId"].Visible = false;
            dgv.Columns.Add("Explicacion", "Explicacion"); dgv.Columns["Explicacion"].Visible = false;

            dgv.CellBeginEdit += Dgv_CellBeginEdit;
            dgv.CellEndEdit += Dgv_CellEndEdit;
            dgv.CellDoubleClick += Dgv_CellDoubleClick;

            this.Controls.Add(btnGenerar);
            this.Controls.Add(btnDeshacer);
            this.Controls.Add(lblFiltroCat);
            this.Controls.Add(cbFiltroCategoria);
            this.Controls.Add(lblFiltroBen);
            this.Controls.Add(txtFiltroBeneficiario);
            this.Controls.Add(btnFiltrar);
            this.Controls.Add(btnLimpiarFiltro);
            this.Controls.Add(lblObs);
            this.Controls.Add(txtObservaciones);
            this.Controls.Add(btnConfirmar);
            this.Controls.Add(dgv);
            
            Label lblAyuda = new Label { 
                Text = "Nota: Haz doble clic en la columna 'A entregar' para ajustar la cantidad manual. Doble clic en cualquier otro lado para ver el cálculo.", 
                Location = new Point(20, 460), 
                AutoSize = true, 
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left,
                ForeColor = Color.Gray, 
                Font = new Font(this.Font, FontStyle.Italic) 
            };
            this.Controls.Add(lblAyuda);
            
            this.Activated += (s, e) => {
                CargarFiltroCategorias();
            };
        }

        private void CargarFiltroCategorias()
        {
            int idCatSel = 0;
            if (cbFiltroCategoria.SelectedItem != null)
            {
                idCatSel = ((ProyectoCatedra.Modelos.Categoria)cbFiltroCategoria.SelectedItem).Id;
            }

            cbFiltroCategoria.Items.Clear();
            cbFiltroCategoria.Items.Add(new ProyectoCatedra.Modelos.Categoria { Id = 0, Nombre = "[Todas las categorías]" });
            
            var categorias = categoriaServicio.ListarTodas();
            for (int i = 0; i < categorias.Conteo(); i++)
            {
                var cat = (ProyectoCatedra.Modelos.Categoria)categorias.Obtener(i)!;
                cbFiltroCategoria.Items.Add(cat);
            }
            cbFiltroCategoria.DisplayMember = "Nombre";
            cbFiltroCategoria.ValueMember = "Id";
            
            // Intentar mantener la seleccion
            bool encontrado = false;
            for (int i = 0; i < cbFiltroCategoria.Items.Count; i++)
            {
                if (((ProyectoCatedra.Modelos.Categoria)cbFiltroCategoria.Items[i]).Id == idCatSel)
                {
                    cbFiltroCategoria.SelectedIndex = i;
                    encontrado = true;
                    break;
                }
            }
            if (!encontrado) cbFiltroCategoria.SelectedIndex = 0;
        }

        private void BtnGenerar_Click(object? sender, EventArgs e)
        {
            GenerarYMostrar();
        }

        private void BtnFiltrar_Click(object? sender, EventArgs e)
        {
            if (propuestaActual != null) MostrarPropuesta();
        }

        private void BtnLimpiarFiltro_Click(object? sender, EventArgs e)
        {
            txtFiltroBeneficiario.Clear();
            cbFiltroCategoria.SelectedIndex = 0;
            if (propuestaActual != null) MostrarPropuesta();
        }

        private void GenerarYMostrar()
        {
            int categoriaIdFiltro = 0;
            if (cbFiltroCategoria.SelectedItem != null)
            {
                var catObj = (ProyectoCatedra.Modelos.Categoria)cbFiltroCategoria.SelectedItem;
                categoriaIdFiltro = catObj.Id;
            }

            try
            {
                propuestaActual = servicio.GenerarPropuestaDistribucion(categoriaIdFiltro);
            }
            catch (Exception ex)
            {
                MessageBox.Show("No se pudo generar la propuesta: " + ex.Message, "Distribución", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (propuestaActual.Conteo() == 0)
            {
                MessageBox.Show("No hay distribuciones sugeridas (falta inventario, beneficiarios activos, tasas o packs de categoría configurados al 100%).");
                dgv.Rows.Clear();
                return;
            }

            stockPorProducto = new TablaHash(100);
            for (int i = 0; i < propuestaActual.Conteo(); i++)
            {
                var det = (OrdenDetalle)propuestaActual.Obtener(i)!;
                if (det.ProductoId > 0)
                {
                    stockPorProducto.Insertar(det.ProductoId.ToString(), servicio.ObtenerStockProducto(det.ProductoId));
                }
            }

            MostrarPropuesta();
            MessageBox.Show($"Propuesta generada con {propuestaActual.Conteo()} asignaciones. Revise y ajuste si es necesario antes de confirmar.");
        }

        private void MostrarPropuesta()
        {
            if (propuestaActual == null) return;
            
            dgv.Rows.Clear();
            undoStack = new Pila(); // reset undo stack
            btnDeshacer.Enabled = false;
            
            string filtroNom = txtFiltroBeneficiario.Text.Trim().ToLower();
            
            int categoriaIdFiltro = 0;
            if (cbFiltroCategoria.SelectedItem != null)
            {
                var catObj = (ProyectoCatedra.Modelos.Categoria)cbFiltroCategoria.SelectedItem;
                categoriaIdFiltro = catObj.Id;
            }

            for (int i = 0; i < propuestaActual.Conteo(); i++)
            {
                var det = (OrdenDetalle)propuestaActual.Obtener(i)!;
                
                // Aplicar filtros visuales
                if (categoriaIdFiltro > 0 && det.CategoriaId != categoriaIdFiltro) continue;
                if (!string.IsNullOrEmpty(filtroNom) && !det.NombreBeneficiario.ToLower().Contains(filtroNom)) continue;

                dgv.Rows.Add(det.NombreBeneficiario, det.NombreCategoria, det.NombreProductoSugerido, det.SKUProductoSugerido, det.CantidadAsignada, det.NombreUnidadMedida, det.DeficitCalculado, det.BeneficiarioId, det.CategoriaId, det.ProductoId, det.ExplicacionCalculo);
            }

            btnConfirmar.Enabled = dgv.Rows.Count > 0;
        }

        private double valorAnteriorCelda;

        private void Dgv_CellBeginEdit(object? sender, DataGridViewCellCancelEventArgs e)
        {
            if (dgv.Columns[e.ColumnIndex].Name == "Asignado")
            {
                var val = dgv.Rows[e.RowIndex].Cells[e.ColumnIndex].Value;
                valorAnteriorCelda = Convert.ToDouble(val);
            }
        }

        private void Dgv_CellEndEdit(object? sender, DataGridViewCellEventArgs e)
        {
            if (dgv.Columns[e.ColumnIndex].Name == "Asignado")
            {
                var val = dgv.Rows[e.RowIndex].Cells[e.ColumnIndex].Value;
                if (!double.TryParse(val?.ToString(), out double nuevoValor) || nuevoValor < 0)
                {
                    MessageBox.Show("Valor inválido.");
                    dgv.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = valorAnteriorCelda;
                    return;
                }
                nuevoValor = Math.Floor(nuevoValor);

                if (nuevoValor > valorAnteriorCelda)
                {
                    string idProducto = dgv.Rows[e.RowIndex].Cells["PId"].Value.ToString() ?? "";
                     
                    double stockDisponible = (double?)stockPorProducto.Buscar(idProducto) ?? 0;
                    double totalAsignadoProducto = 0;
                     
                    // Sumar todo lo asignado en la propuesta
                    for (int i = 0; i < propuestaActual!.Conteo(); i++)
                    {
                        var det = (OrdenDetalle)propuestaActual.Obtener(i)!;
                        if (det.ProductoId.ToString() == idProducto)
                        {
                            totalAsignadoProducto += det.CantidadAsignada;
                        }
                    }

                    double diferencia = nuevoValor - valorAnteriorCelda;
                    if (totalAsignadoProducto + diferencia > stockDisponible)
                    {
                        MessageBox.Show($"No hay suficiente inventario para ese producto.\n\nDisponible total: {stockDisponible}\nAsignado total: {totalAsignadoProducto}\n\nNo se puede aumentar más.", "Límite de Stock", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        dgv.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = valorAnteriorCelda;
                        return;
                    }
                }

                dgv.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = nuevoValor;
                
                if (nuevoValor != valorAnteriorCelda)
                {
                    // Actualizar propuesta
                    int idBen = Convert.ToInt32(dgv.Rows[e.RowIndex].Cells["BId"].Value);
                    int idCat = Convert.ToInt32(dgv.Rows[e.RowIndex].Cells["CId"].Value);
                    int idProducto = Convert.ToInt32(dgv.Rows[e.RowIndex].Cells["PId"].Value);
                     
                    for (int i = 0; i < propuestaActual!.Conteo(); i++)
                    {
                        var det = (OrdenDetalle)propuestaActual.Obtener(i)!;
                        if (det.BeneficiarioId == idBen && det.CategoriaId == idCat && det.ProductoId == idProducto)
                        {
                            det.CantidadAsignada = nuevoValor;
                            break;
                        }
                    }

                    // Guardar en la pila para deshacer
                    var accion = new AccionEdicionCelda
                    {
                        Fila = e.RowIndex,
                        Columna = e.ColumnIndex,
                        BeneficiarioId = idBen,
                        CategoriaId = idCat,
                        ProductoId = idProducto,
                        ValorAnterior = valorAnteriorCelda
                    };
                    undoStack.Empujar(accion);
                    btnDeshacer.Enabled = true;
                }
            }
        }

        private void Dgv_CellDoubleClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && dgv.Columns[e.ColumnIndex].Name != "Asignado")
            {
                var explicacion = dgv.Rows[e.RowIndex].Cells["Explicacion"].Value?.ToString();
                if (!string.IsNullOrEmpty(explicacion))
                {
                    MessageBox.Show(explicacion, "Explicación del Déficit", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void BtnDeshacer_Click(object? sender, EventArgs e)
        {
            var accionObj = undoStack.Pop();
            if (accionObj != null)
            {
                var accion = (AccionEdicionCelda)accionObj;
                dgv.Rows[accion.Fila].Cells[accion.Columna].Value = accion.ValorAnterior;
                ActualizarCantidadPropuesta(accion.BeneficiarioId, accion.CategoriaId, accion.ProductoId, accion.ValorAnterior);
            }
            if (undoStack.EstaVacia()) btnDeshacer.Enabled = false;
        }

        private void BtnConfirmar_Click(object? sender, EventArgs e)
        {
            if (dgv.Rows.Count == 0) return;

            if (MessageBox.Show("¿Está seguro de confirmar esta distribución? Esta acción descontará el inventario permanentemente.", "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                ListaEnlazada detallesFinales = new ListaEnlazada();
                for (int i = 0; i < dgv.Rows.Count; i++)
                {
                    double asignado = Math.Floor(Convert.ToDouble(dgv.Rows[i].Cells["Asignado"].Value));
                    if (asignado > 0)
                    {
                        detallesFinales.Agregar(new OrdenDetalle
                        {
                            BeneficiarioId = Convert.ToInt32(dgv.Rows[i].Cells["BId"].Value),
                            CategoriaId = Convert.ToInt32(dgv.Rows[i].Cells["CId"].Value),
                            ProductoId = Convert.ToInt32(dgv.Rows[i].Cells["PId"].Value),
                            CantidadAsignada = asignado,
                            DeficitCalculado = Convert.ToDouble(dgv.Rows[i].Cells["Deficit"].Value)
                        });
                    }
                }

                try
                {
                    servicio.ConfirmarDistribucion(detallesFinales, txtObservaciones.Text);
                    MessageBox.Show("Distribución confirmada y stock actualizado exitosamente.");
                    this.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error al confirmar: " + ex.Message);
                }
            }
        }

        private class AccionEdicionCelda
        {
            public int Fila { get; set; }
            public int Columna { get; set; }
            public int BeneficiarioId { get; set; }
            public int CategoriaId { get; set; }
            public int ProductoId { get; set; }
            public double ValorAnterior { get; set; }
        }

        private void ActualizarCantidadPropuesta(int beneficiarioId, int categoriaId, int productoId, double cantidad)
        {
            if (propuestaActual == null) return;
            for (int i = 0; i < propuestaActual.Conteo(); i++)
            {
                var det = (OrdenDetalle)propuestaActual.Obtener(i)!;
                if (det.BeneficiarioId == beneficiarioId && det.CategoriaId == categoriaId && det.ProductoId == productoId)
                {
                    det.CantidadAsignada = cantidad;
                    return;
                }
            }
        }
    }
}
