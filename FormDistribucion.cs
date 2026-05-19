using System;
using System.Collections.Generic;
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
        private Button btnConfirmarFamilia = new Button();
        private Button btnDeshacer = new Button();
        private TextBox txtObservaciones = new TextBox();
        private ComboBox cbFiltroCategoria = new ComboBox();
        private Button btnFiltrar = new Button();
        private Button btnLimpiarFiltro = new Button();
        private Button btnExpandirTodo = new Button();
        private Button btnColapsarTodo = new Button();
        private CategoriaServicio categoriaServicio = new CategoriaServicio();
        private HashSet<int> familiasColapsadas = new HashSet<int>();
        
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

            btnFiltrar.Text = "Aplicar Filtros";
            btnFiltrar.Location = new Point(240, 55);
            btnFiltrar.Size = new Size(100, 28);
            btnFiltrar.Click += BtnFiltrar_Click;

            btnLimpiarFiltro.Text = "Limpiar Filtros";
            btnLimpiarFiltro.Location = new Point(350, 55);
            btnLimpiarFiltro.Size = new Size(100, 28);
            btnLimpiarFiltro.Click += BtnLimpiarFiltro_Click;

            btnExpandirTodo.Text = "Expandir todo";
            btnExpandirTodo.Location = new Point(480, 55);
            btnExpandirTodo.Size = new Size(105, 28);
            btnExpandirTodo.Click += (s, e) => { familiasColapsadas.Clear(); MostrarPropuesta(); };

            btnColapsarTodo.Text = "Colapsar todo";
            btnColapsarTodo.Location = new Point(595, 55);
            btnColapsarTodo.Size = new Size(105, 28);
            btnColapsarTodo.Click += (s, e) => ColapsarTodasLasFamiliasVisibles();

            Label lblObs = new Label { Text = "Observaciones:", Location = new Point(370, 20), AutoSize = true };
            txtObservaciones.Location = new Point(460, 17);
            txtObservaciones.Size = new Size(310, 25);

            btnConfirmarFamilia.Text = "Entregar Familia Seleccionada"; btnConfirmarFamilia.Location = new Point(385, 95); btnConfirmarFamilia.Size = new Size(195, 35);
            btnConfirmarFamilia.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnConfirmarFamilia.BackColor = Color.LightSkyBlue;
            btnConfirmarFamilia.Enabled = false;
            btnConfirmarFamilia.Click += BtnConfirmarFamilia_Click;

            btnConfirmar.Text = "Entregar Todos Mostrados"; btnConfirmar.Location = new Point(590, 95); btnConfirmar.Size = new Size(180, 35);
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

            dgv.Columns.Add("Detalle", "Familia / Producto");
            dgv.Columns["Detalle"]!.ReadOnly = true;
            dgv.Columns.Add("Categoria", "Categoría");
            dgv.Columns["Categoria"]!.ReadOnly = true;
            dgv.Columns.Add("SKU", "SKU");
            dgv.Columns["SKU"]!.ReadOnly = true;
            dgv.Columns.Add("Asignado", "A entregar");
            dgv.Columns.Add("Unidad", "Unidad");
            dgv.Columns["Unidad"]!.ReadOnly = true;
            dgv.Columns.Add("Deficit", "Déficit Calculado");
            dgv.Columns["Deficit"]!.ReadOnly = true;
            
            // Hidden columns for IDs
            dgv.Columns.Add("BId", "BId"); dgv.Columns["BId"]!.Visible = false;
            dgv.Columns.Add("CId", "CId"); dgv.Columns["CId"]!.Visible = false;
            dgv.Columns.Add("PId", "PId"); dgv.Columns["PId"]!.Visible = false;
            dgv.Columns.Add("NivelVulnerabilidad", "NivelVulnerabilidad"); dgv.Columns["NivelVulnerabilidad"]!.Visible = false;
            dgv.Columns.Add("Explicacion", "Explicacion"); dgv.Columns["Explicacion"]!.Visible = false;
            dgv.Columns.Add("Beneficiario", "Beneficiario"); dgv.Columns["Beneficiario"]!.Visible = false;
            dgv.Columns.Add("Vulnerabilidad", "Vulnerabilidad"); dgv.Columns["Vulnerabilidad"]!.Visible = false;
            dgv.Columns.Add("Producto", "Producto"); dgv.Columns["Producto"]!.Visible = false;
            dgv.Columns.Add("EsGrupo", "EsGrupo"); dgv.Columns["EsGrupo"]!.Visible = false;

            dgv.CellBeginEdit += Dgv_CellBeginEdit;
            dgv.CellEndEdit += Dgv_CellEndEdit;
            dgv.CellDoubleClick += Dgv_CellDoubleClick;
            dgv.SelectionChanged += (s, e) => ActualizarBotonesEntrega();

            this.Controls.Add(btnGenerar);
            this.Controls.Add(btnDeshacer);
            this.Controls.Add(lblFiltroCat);
            this.Controls.Add(cbFiltroCategoria);
            this.Controls.Add(btnFiltrar);
            this.Controls.Add(btnLimpiarFiltro);
            this.Controls.Add(btnExpandirTodo);
            this.Controls.Add(btnColapsarTodo);
            this.Controls.Add(lblObs);
            this.Controls.Add(txtObservaciones);
            this.Controls.Add(btnConfirmarFamilia);
            this.Controls.Add(btnConfirmar);
            this.Controls.Add(dgv);
            
            Label lblAyuda = new Label { 
                Text = "Nota: doble clic en una familia para expandir/colapsar. En productos, doble clic fuera de 'A entregar' muestra el cálculo.", 
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

            cbFiltroCategoria.SelectedIndexChanged += (s, e) => {
                if (propuestaActual != null) MostrarPropuesta();
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
                if (((ProyectoCatedra.Modelos.Categoria)cbFiltroCategoria.Items[i]!).Id == idCatSel)
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
                string diagnostico = servicio.ObtenerDiagnosticoSinPropuesta(categoriaIdFiltro);
                MessageBox.Show("No hay distribuciones sugeridas.\n\nMotivo principal:\n" + diagnostico, "Distribución", MessageBoxButtons.OK, MessageBoxIcon.Information);
                dgv.Rows.Clear();
                return;
            }

            undoStack = new Pila();
            btnDeshacer.Enabled = false;
            familiasColapsadas.Clear();
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
            
            int categoriaIdFiltro = 0;
            if (cbFiltroCategoria.SelectedItem != null)
            {
                var catObj = (ProyectoCatedra.Modelos.Categoria)cbFiltroCategoria.SelectedItem;
                categoriaIdFiltro = catObj.Id;
            }

            List<int> ordenFamilias = new List<int>();
            Dictionary<int, List<OrdenDetalle>> detallesPorFamilia = new Dictionary<int, List<OrdenDetalle>>();
            Dictionary<int, OrdenDetalle> resumenFamilia = new Dictionary<int, OrdenDetalle>();

            for (int i = 0; i < propuestaActual.Conteo(); i++)
            {
                var det = (OrdenDetalle)propuestaActual.Obtener(i)!;
                if (categoriaIdFiltro > 0 && det.CategoriaId != categoriaIdFiltro) continue;

                if (!detallesPorFamilia.ContainsKey(det.BeneficiarioId))
                {
                    ordenFamilias.Add(det.BeneficiarioId);
                    detallesPorFamilia[det.BeneficiarioId] = new List<OrdenDetalle>();
                    resumenFamilia[det.BeneficiarioId] = det;
                }

                detallesPorFamilia[det.BeneficiarioId].Add(det);
            }

            foreach (int beneficiarioId in ordenFamilias)
            {
                OrdenDetalle resumen = resumenFamilia[beneficiarioId];
                List<OrdenDetalle> detalles = detallesPorFamilia[beneficiarioId];
                bool colapsada = familiasColapsadas.Contains(beneficiarioId);
                string icono = colapsada ? ">" : "v";
                int filaGrupo = dgv.Rows.Add($"{icono} {resumen.NombreBeneficiario} - {resumen.VulnerabilidadTexto}", $"{detalles.Count} productos", "", "", "", "", resumen.BeneficiarioId, 0, 0, resumen.NivelVulnerabilidad, "", resumen.NombreBeneficiario, resumen.VulnerabilidadTexto, "", "1");
                AplicarEstiloFilaGrupo(dgv.Rows[filaGrupo]);

                if (colapsada) continue;

                foreach (OrdenDetalle det in detalles)
                {
                    int filaDetalle = dgv.Rows.Add("   " + det.NombreProductoSugerido, det.NombreCategoria, det.SKUProductoSugerido, det.CantidadAsignada, det.NombreUnidadMedida, det.DeficitCalculado, det.BeneficiarioId, det.CategoriaId, det.ProductoId, det.NivelVulnerabilidad, det.ExplicacionCalculo, det.NombreBeneficiario, det.VulnerabilidadTexto, det.NombreProductoSugerido, "0");
                    dgv.Rows[filaDetalle].Cells["Asignado"].ReadOnly = false;
                }
            }

            ActualizarBotonesEntrega();
        }

        private void AplicarEstiloFilaGrupo(DataGridViewRow row)
        {
            row.ReadOnly = true;
            row.DefaultCellStyle.BackColor = Color.Gainsboro;
            row.DefaultCellStyle.Font = new Font(dgv.Font, FontStyle.Bold);
            row.DefaultCellStyle.ForeColor = Color.Black;
        }

        private void ColapsarTodasLasFamiliasVisibles()
        {
            if (propuestaActual == null) return;
            familiasColapsadas.Clear();
            int categoriaIdFiltro = 0;
            if (cbFiltroCategoria.SelectedItem != null)
            {
                categoriaIdFiltro = ((ProyectoCatedra.Modelos.Categoria)cbFiltroCategoria.SelectedItem).Id;
            }

            for (int i = 0; i < propuestaActual.Conteo(); i++)
            {
                var det = (OrdenDetalle)propuestaActual.Obtener(i)!;
                if (categoriaIdFiltro > 0 && det.CategoriaId != categoriaIdFiltro) continue;
                familiasColapsadas.Add(det.BeneficiarioId);
            }

            MostrarPropuesta();
        }

        private double valorAnteriorCelda;

        private void Dgv_CellBeginEdit(object? sender, DataGridViewCellCancelEventArgs e)
        {
            if (EsFilaGrupo(dgv.Rows[e.RowIndex]) || dgv.Columns[e.ColumnIndex].Name != "Asignado")
            {
                e.Cancel = true;
                return;
            }

            if (dgv.Columns[e.ColumnIndex].Name == "Asignado")
            {
                var val = dgv.Rows[e.RowIndex].Cells[e.ColumnIndex].Value;
                valorAnteriorCelda = Convert.ToDouble(val);
            }
        }

        private void Dgv_CellEndEdit(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || EsFilaGrupo(dgv.Rows[e.RowIndex])) return;

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

                int idBen = Convert.ToInt32(dgv.Rows[e.RowIndex].Cells["BId"].Value);
                int idCat = Convert.ToInt32(dgv.Rows[e.RowIndex].Cells["CId"].Value);
                int idProducto = Convert.ToInt32(dgv.Rows[e.RowIndex].Cells["PId"].Value);
                double stockDisponible = ObtenerStockDisponibleProducto(idProducto);
                if (nuevoValor > stockDisponible)
                {
                    MessageBox.Show($"No hay suficiente inventario para ese producto.\n\nDisponible actual: {stockDisponible}\nCantidad solicitada: {nuevoValor}", "Límite de Stock", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    dgv.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = valorAnteriorCelda;
                    return;
                }

                dgv.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = nuevoValor;
                
                if (nuevoValor != valorAnteriorCelda)
                {
                    ActualizarCantidadPropuesta(idBen, idCat, idProducto, nuevoValor);

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

                    bool huboAjuste = AjustarAsignacionesPorStock(idProducto, idBen, idCat);
                    LimpiarAsignacionesSinCantidad();
                    MostrarPropuesta();
                    if (huboAjuste)
                    {
                        MessageBox.Show("Se refrescó la propuesta pendiente para que el total asignado no exceda el stock disponible.", "Propuesta actualizada", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
        }

        private void Dgv_CellDoubleClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            if (EsFilaGrupo(dgv.Rows[e.RowIndex]))
            {
                AlternarFamilia(dgv.Rows[e.RowIndex]);
                return;
            }

            if (dgv.Columns[e.ColumnIndex].Name != "Asignado")
            {
                var explicacion = dgv.Rows[e.RowIndex].Cells["Explicacion"].Value?.ToString();
                if (!string.IsNullOrEmpty(explicacion))
                {
                    MessageBox.Show(explicacion, "Explicación del Déficit", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private bool EsFilaGrupo(DataGridViewRow row)
        {
            return row.Cells["EsGrupo"].Value?.ToString() == "1";
        }

        private void AlternarFamilia(DataGridViewRow row)
        {
            int beneficiarioId = Convert.ToInt32(row.Cells["BId"].Value);
            if (beneficiarioId <= 0) return;

            if (familiasColapsadas.Contains(beneficiarioId)) familiasColapsadas.Remove(beneficiarioId);
            else familiasColapsadas.Add(beneficiarioId);

            MostrarPropuesta();
        }

        private void BtnDeshacer_Click(object? sender, EventArgs e)
        {
            object? accionObj = undoStack.Pop();
            if (accionObj != null)
            {
                var accion = (AccionEdicionCelda)accionObj;
                ActualizarCantidadPropuesta(accion.BeneficiarioId, accion.CategoriaId, accion.ProductoId, accion.ValorAnterior);
                AjustarAsignacionesPorStock(accion.ProductoId, accion.BeneficiarioId, accion.CategoriaId);
                LimpiarAsignacionesSinCantidad();
                MostrarPropuesta();
            }
            if (undoStack.EstaVacia()) btnDeshacer.Enabled = false;
        }

        private void BtnConfirmarFamilia_Click(object? sender, EventArgs e)
        {
            if (!ObtenerBeneficiarioSeleccionado(out int beneficiarioId, out string nombreBeneficiario))
            {
                MessageBox.Show("Seleccione el encabezado de una familia o uno de sus productos.", "Entrega por familia", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            ListaEnlazada detalles = CrearDetallesPorBeneficiario(beneficiarioId);
            ConfirmarDetalles(detalles, $"Entrega para familia: {nombreBeneficiario}");
        }

        private void BtnConfirmar_Click(object? sender, EventArgs e)
        {
            if (dgv.Rows.Count == 0) return;

            ListaEnlazada detallesFinales = CrearDetallesDesdeFilas(row => true);
            ConfirmarDetalles(detallesFinales, "Entrega para todos los beneficiarios mostrados");
        }

        private bool ObtenerBeneficiarioSeleccionado(out int beneficiarioId, out string nombreBeneficiario)
        {
            beneficiarioId = 0;
            nombreBeneficiario = "";
            if (dgv.Rows.Count == 0) return false;

            int rowIndex = dgv.CurrentCell?.RowIndex ?? -1;
            if (rowIndex < 0 && dgv.SelectedCells.Count > 0)
            {
                rowIndex = dgv.SelectedCells[0].RowIndex;
            }

            if (rowIndex < 0) return false;

            var row = dgv.Rows[rowIndex];
            beneficiarioId = Convert.ToInt32(row.Cells["BId"].Value);
            nombreBeneficiario = row.Cells["Beneficiario"].Value?.ToString() ?? row.Cells["Detalle"].Value?.ToString() ?? "";
            return beneficiarioId > 0;
        }

        private ListaEnlazada CrearDetallesDesdeFilas(Func<DataGridViewRow, bool> incluirFila)
        {
            ListaEnlazada detalles = new ListaEnlazada();
            for (int i = 0; i < dgv.Rows.Count; i++)
            {
                var row = dgv.Rows[i];
                if (row.IsNewRow || !incluirFila(row)) continue;
                if (EsFilaGrupo(row)) continue;

                double asignado = Math.Floor(Convert.ToDouble(row.Cells["Asignado"].Value));
                if (asignado <= 0) continue;

                detalles.Agregar(new OrdenDetalle
                {
                    BeneficiarioId = Convert.ToInt32(row.Cells["BId"].Value),
                    NombreBeneficiario = row.Cells["Beneficiario"].Value?.ToString() ?? "",
                    NivelVulnerabilidad = Convert.ToInt32(row.Cells["NivelVulnerabilidad"].Value),
                    VulnerabilidadTexto = row.Cells["Vulnerabilidad"].Value?.ToString() ?? Beneficiario.ObtenerEtiquetaVulnerabilidad(Beneficiario.VulnerabilidadMedia),
                    CategoriaId = Convert.ToInt32(row.Cells["CId"].Value),
                    NombreCategoria = row.Cells["Categoria"].Value?.ToString() ?? "",
                    ProductoId = Convert.ToInt32(row.Cells["PId"].Value),
                    NombreProductoSugerido = row.Cells["Producto"].Value?.ToString() ?? "",
                    SKUProductoSugerido = row.Cells["SKU"].Value?.ToString() ?? "",
                    NombreUnidadMedida = row.Cells["Unidad"].Value?.ToString() ?? "",
                    CantidadAsignada = asignado,
                    DeficitCalculado = Convert.ToDouble(row.Cells["Deficit"].Value)
                });
            }

            return detalles;
        }

        private ListaEnlazada CrearDetallesPorBeneficiario(int beneficiarioId)
        {
            ListaEnlazada detalles = new ListaEnlazada();
            if (propuestaActual == null) return detalles;

            int categoriaIdFiltro = 0;
            if (cbFiltroCategoria.SelectedItem != null)
            {
                categoriaIdFiltro = ((ProyectoCatedra.Modelos.Categoria)cbFiltroCategoria.SelectedItem).Id;
            }

            for (int i = 0; i < propuestaActual.Conteo(); i++)
            {
                var det = (OrdenDetalle)propuestaActual.Obtener(i)!;
                double asignado = Math.Floor(det.CantidadAsignada);
                if (det.BeneficiarioId != beneficiarioId || asignado <= 0) continue;
                if (categoriaIdFiltro > 0 && det.CategoriaId != categoriaIdFiltro) continue;

                detalles.Agregar(new OrdenDetalle
                {
                    BeneficiarioId = det.BeneficiarioId,
                    NombreBeneficiario = det.NombreBeneficiario,
                    NivelVulnerabilidad = det.NivelVulnerabilidad,
                    VulnerabilidadTexto = det.VulnerabilidadTexto,
                    CategoriaId = det.CategoriaId,
                    NombreCategoria = det.NombreCategoria,
                    ProductoId = det.ProductoId,
                    NombreProductoSugerido = det.NombreProductoSugerido,
                    SKUProductoSugerido = det.SKUProductoSugerido,
                    NombreUnidadMedida = det.NombreUnidadMedida,
                    CantidadAsignada = asignado,
                    DeficitCalculado = det.DeficitCalculado
                });
            }

            return detalles;
        }

        private void ConfirmarDetalles(ListaEnlazada detalles, string titulo)
        {
            if (detalles.Conteo() == 0)
            {
                MessageBox.Show("No hay productos con cantidad mayor a cero para entregar.", "Entrega", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (var form = new FormConfirmacionDistribucion(detalles, titulo))
            {
                if (form.ShowDialog(this) != DialogResult.OK) return;
            }

            try
            {
                servicio.ConfirmarDistribucion(detalles, txtObservaciones.Text);
                RemoverDetallesConfirmados(detalles);
                undoStack = new Pila();
                btnDeshacer.Enabled = false;
                RecargarStockPorProducto();
                MostrarPropuesta();

                MessageBox.Show("Entrega confirmada y stock actualizado exitosamente.");
                if (propuestaActual == null || propuestaActual.Conteo() == 0)
                {
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al confirmar: " + ex.Message);
            }
        }

        private void RemoverDetallesConfirmados(ListaEnlazada detalles)
        {
            if (propuestaActual == null) return;
            for (int i = 0; i < detalles.Conteo(); i++)
            {
                var confirmado = (OrdenDetalle)detalles.Obtener(i)!;
                propuestaActual.EliminarPrimero(valor =>
                {
                    var pendiente = (OrdenDetalle)valor;
                    return pendiente.BeneficiarioId == confirmado.BeneficiarioId
                        && pendiente.CategoriaId == confirmado.CategoriaId
                        && pendiente.ProductoId == confirmado.ProductoId;
                }, out _);
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

        private double ObtenerStockDisponibleProducto(int productoId)
        {
            if (productoId <= 0) return 0;
            return (double?)stockPorProducto.Buscar(productoId.ToString()) ?? servicio.ObtenerStockProducto(productoId);
        }

        private bool AjustarAsignacionesPorStock(int productoId, int beneficiarioProtegidoId, int categoriaProtegidaId)
        {
            if (propuestaActual == null || productoId <= 0) return false;

            double stockDisponible = ObtenerStockDisponibleProducto(productoId);
            double totalAsignado = SumarAsignadoProducto(productoId);
            double exceso = totalAsignado - stockDisponible;
            if (exceso <= 0) return false;

            bool huboAjuste = false;
            for (int i = propuestaActual.Conteo() - 1; i >= 0 && exceso > 0; i--)
            {
                var det = (OrdenDetalle)propuestaActual.Obtener(i)!;
                if (det.ProductoId != productoId) continue;
                if (det.BeneficiarioId == beneficiarioProtegidoId && det.CategoriaId == categoriaProtegidaId) continue;

                double reduccion = Math.Min(det.CantidadAsignada, exceso);
                det.CantidadAsignada -= reduccion;
                exceso -= reduccion;
                huboAjuste = true;
            }

            if (exceso > 0)
            {
                for (int i = 0; i < propuestaActual.Conteo() && exceso > 0; i++)
                {
                    var det = (OrdenDetalle)propuestaActual.Obtener(i)!;
                    if (det.ProductoId != productoId) continue;
                    if (det.BeneficiarioId != beneficiarioProtegidoId || det.CategoriaId != categoriaProtegidaId) continue;

                    double reduccion = Math.Min(det.CantidadAsignada, exceso);
                    det.CantidadAsignada -= reduccion;
                    exceso -= reduccion;
                    huboAjuste = true;
                }
            }

            return huboAjuste;
        }

        private double SumarAsignadoProducto(int productoId)
        {
            if (propuestaActual == null) return 0;
            double total = 0;
            for (int i = 0; i < propuestaActual.Conteo(); i++)
            {
                var det = (OrdenDetalle)propuestaActual.Obtener(i)!;
                if (det.ProductoId == productoId)
                {
                    total += det.CantidadAsignada;
                }
            }

            return total;
        }

        private void LimpiarAsignacionesSinCantidad()
        {
            if (propuestaActual == null) return;
            ListaEnlazada limpia = new ListaEnlazada();
            for (int i = 0; i < propuestaActual.Conteo(); i++)
            {
                var det = (OrdenDetalle)propuestaActual.Obtener(i)!;
                if (det.CantidadAsignada > 0)
                {
                    limpia.Agregar(det);
                }
            }

            propuestaActual = limpia;
        }

        private void RecargarStockPorProducto()
        {
            stockPorProducto = new TablaHash(100);
            if (propuestaActual == null) return;
            for (int i = 0; i < propuestaActual.Conteo(); i++)
            {
                var det = (OrdenDetalle)propuestaActual.Obtener(i)!;
                if (det.ProductoId > 0)
                {
                    stockPorProducto.Insertar(det.ProductoId.ToString(), servicio.ObtenerStockProducto(det.ProductoId));
                }
            }
        }

        private void ActualizarBotonesEntrega()
        {
            bool hayFamilias = dgv.Rows.Count > 0;
            bool hayProductosVisibles = false;
            for (int i = 0; i < dgv.Rows.Count; i++)
            {
                if (!EsFilaGrupo(dgv.Rows[i]))
                {
                    hayProductosVisibles = true;
                    break;
                }
            }

            btnConfirmar.Enabled = hayProductosVisibles;
            btnConfirmarFamilia.Enabled = hayFamilias;
        }
    }
}
