using System;
using System.Drawing;
using System.Windows.Forms;
using ProyectoCatedra.Servicios;
using ProyectoCatedra.Modelos;
using ProyectoCatedra.Estructuras;

namespace ProyectoCatedra
{
    public class FormTasaConsumo : Form
    {
        private TasaConsumoServicio tasaServicio;
        private CategoriaServicio categoriaServicio;
        private UnidadServicio unidadServicio;

        private ComboBox cbCategoria = new ComboBox();
        private ComboBox cbUnidadBase = new ComboBox();
        private TextBox txtTasa = new TextBox();
        
        private DataGridView dgv = new DataGridView();
        private Button btnGuardar = new Button();
        private Button btnEliminar = new Button();
        private Button btnLimpiar = new Button();
        
        private TasaConsumo? seleccionado;

        public FormTasaConsumo()
        {
            tasaServicio = new TasaConsumoServicio();
            categoriaServicio = new CategoriaServicio();
            unidadServicio = new UnidadServicio();

            InicializarComponentes();
            CargarCategorias();
            CargarTasa();
        }

        private void InicializarComponentes()
        {
            this.AutoScaleMode = AutoScaleMode.None;
            this.Text = "Configurar Tasas de Consumo";
            this.Size = new Size(600, 450);
            this.StartPosition = FormStartPosition.CenterParent;

            // Labels and Inputs
            Label lblCat = new Label { Text = "Categoría:", Location = new Point(20, 20), AutoSize = true };
            cbCategoria.Location = new Point(120, 20);
            cbCategoria.Size = new Size(200, 25);
            cbCategoria.DropDownStyle = ComboBoxStyle.DropDownList;
            cbCategoria.DisplayMember = "Nombre";
            cbCategoria.SelectedIndexChanged += CbCategoria_SelectedIndexChanged;

            Label lblTasa = new Label { Text = "Tasa Diaria:", Location = new Point(20, 60), AutoSize = true };
            txtTasa.Location = new Point(120, 60);
            txtTasa.Size = new Size(100, 25);

            Label lblUnidad = new Label { Text = "Unidad Base:", Location = new Point(240, 60), AutoSize = true };
            cbUnidadBase.Location = new Point(320, 60);
            cbUnidadBase.Size = new Size(150, 25);
            cbUnidadBase.DropDownStyle = ComboBoxStyle.DropDownList;
            cbUnidadBase.DisplayMember = "Nombre";

            // Buttons
            btnGuardar.Text = "Guardar"; btnGuardar.Location = new Point(120, 100); btnGuardar.Size = new Size(80, 30);
            btnGuardar.Click += BtnGuardar_Click;
            
            btnEliminar.Text = "Eliminar"; btnEliminar.Location = new Point(210, 100); btnEliminar.Size = new Size(80, 30);
            btnEliminar.Enabled = false;
            btnEliminar.Click += BtnEliminar_Click;
            
            btnLimpiar.Text = "Limpiar"; btnLimpiar.Location = new Point(300, 100); btnLimpiar.Size = new Size(80, 30);
            btnLimpiar.Click += (s, e) => Limpiar();

            // DataGridView
            dgv.Location = new Point(20, 180); 
            dgv.Size = new Size(540, 210); 
            dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect; 
            dgv.ReadOnly = true; 
            dgv.AllowUserToAddRows = false; 
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgv.MultiSelect = false;
            dgv.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;

            dgv.Columns.Add("IdCategoria", "Id Cat");
            dgv.Columns["IdCategoria"]!.Visible = false;
            dgv.Columns.Add("NombreCategoria", "Categoría");
            dgv.Columns.Add("TasaDiaria", "Tasa Diaria");
            dgv.Columns.Add("IdUnidadBase", "Id Uni");
            dgv.Columns["IdUnidadBase"]!.Visible = false;
            dgv.Columns.Add("NombreUnidadBase", "Unidad Base");

            dgv.SelectionChanged += Dgv_SelectionChanged;

            this.Controls.Add(lblCat);
            this.Controls.Add(cbCategoria);
            this.Controls.Add(lblTasa);
            this.Controls.Add(txtTasa);
            this.Controls.Add(lblUnidad);
            this.Controls.Add(cbUnidadBase);
            this.Controls.Add(btnGuardar);
            this.Controls.Add(btnEliminar);
            this.Controls.Add(btnLimpiar);
            
            Label lblAyuda = new Label { 
                Text = "Nota: Define cuánto consume 1 persona en 1 día de esta categoría.\nEsto nos ayuda a calcular lo que necesita una familia completa.", 
                Location = new Point(20, 140), 
                AutoSize = true, 
                ForeColor = Color.Gray, 
                Font = new Font(this.Font, FontStyle.Italic) 
            };
            this.Controls.Add(lblAyuda);
            this.Controls.Add(dgv);

            this.Activated += (s, e) => {
                CargarCategorias();
                CargarTasa();
            };
        }

        private void CargarCategorias()
        {
            try
            {
                int? idCatSel = (cbCategoria.SelectedItem as Categoria)?.Id;
                cbCategoria.Items.Clear();
                var lista = categoriaServicio.ListarTodas();
                for (int i = 0; i < lista.Conteo(); i++)
                {
                    var cat = lista.Obtener(i);
                    if (cat != null) cbCategoria.Items.Add(cat);
                }

                if (idCatSel.HasValue)
                {
                    for (int i = 0; i < cbCategoria.Items.Count; i++)
                    {
                        if (((Categoria)cbCategoria.Items[i]!).Id == idCatSel.Value)
                        {
                            cbCategoria.SelectedIndex = i;
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("No se pudieron cargar las categorías: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CbCategoria_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (cbCategoria.SelectedItem is Categoria cat)
            {
                cbUnidadBase.Items.Clear();
                var unidades = unidadServicio.ListarPorCategoria(cat.Id);
                for (int i = 0; i < unidades.Conteo(); i++)
                {
                    var unidad = unidades.Obtener(i);
                    if (unidad != null) cbUnidadBase.Items.Add(unidad);
                }
                if (cbUnidadBase.Items.Count > 0)
                {
                    cbUnidadBase.SelectedIndex = 0;
                }
            }
        }

        private void CargarTasa()
        {
            try
            {
                dgv.Rows.Clear();
                var lista = tasaServicio.ListarTodas();
                for (int i = 0; i < lista.Conteo(); i++)
                {
                    var t = (TasaConsumo)lista.Obtener(i)!;
                    dgv.Rows.Add(t.IdCategoria, t.NombreCategoria, t.TasaDiaria, t.IdUnidadBase, t.NombreUnidadBase);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("No se pudieron cargar las tasas: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Dgv_SelectionChanged(object? sender, EventArgs e)
        {
            if (dgv.SelectedRows.Count == 0) return;
            
            var row = dgv.SelectedRows[0];
            seleccionado = new TasaConsumo
            {
                IdCategoria = Convert.ToInt32(row.Cells["IdCategoria"].Value),
                NombreCategoria = row.Cells["NombreCategoria"].Value?.ToString() ?? "",
                TasaDiaria = Convert.ToDouble(row.Cells["TasaDiaria"].Value),
                IdUnidadBase = Convert.ToInt32(row.Cells["IdUnidadBase"].Value),
                NombreUnidadBase = row.Cells["NombreUnidadBase"].Value?.ToString() ?? ""
            };

            // Select Categoria
            for (int i = 0; i < cbCategoria.Items.Count; i++)
            {
                if (((Categoria)cbCategoria.Items[i]!).Id == seleccionado.IdCategoria)
                {
                    cbCategoria.SelectedIndex = i;
                    break;
                }
            }
            
            txtTasa.Text = seleccionado.TasaDiaria.ToString();
            
            // Select Unidad
            for (int i = 0; i < cbUnidadBase.Items.Count; i++)
            {
                if (((UnidadMedida)cbUnidadBase.Items[i]!).Id == seleccionado.IdUnidadBase)
                {
                    cbUnidadBase.SelectedIndex = i;
                    break;
                }
            }

            btnEliminar.Enabled = true;
            cbCategoria.Enabled = false; // No se puede editar la categoría de una tasa existente (es su PK)
        }

        private void BtnGuardar_Click(object? sender, EventArgs e)
        {
            if (cbCategoria.SelectedItem == null || cbUnidadBase.SelectedItem == null || string.IsNullOrWhiteSpace(txtTasa.Text))
            {
                MessageBox.Show("Complete todos los campos.");
                return;
            }

            if (!double.TryParse(txtTasa.Text, out double tasa) || tasa <= 0)
            {
                MessageBox.Show("La tasa debe ser un número válido mayor a 0.");
                return;
            }

            var cat = (Categoria)cbCategoria.SelectedItem;
            var uni = (UnidadMedida)cbUnidadBase.SelectedItem;

            var nuevaTasa = new TasaConsumo
            {
                IdCategoria = cat.Id,
                TasaDiaria = tasa,
                IdUnidadBase = uni.Id
            };

            try
            {
                tasaServicio.Guardar(nuevaTasa);
                MessageBox.Show("Tasa guardada.", "Tasas", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Limpiar();
                CargarTasa();
            }
            catch (Exception ex)
            {
                MessageBox.Show("No se pudo guardar la tasa: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnEliminar_Click(object? sender, EventArgs e)
        {
            if (seleccionado != null)
            {
                if (MessageBox.Show("¿Eliminar tasa?", "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                {
                    try
                    {
                        tasaServicio.Eliminar(seleccionado.IdCategoria);
                        Limpiar();
                        CargarTasa();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("No se pudo eliminar la tasa: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void Limpiar()
        {
            cbCategoria.SelectedIndex = -1;
            cbUnidadBase.Items.Clear();
            txtTasa.Clear();
            seleccionado = null;
            btnEliminar.Enabled = false;
            cbCategoria.Enabled = true;
            dgv.ClearSelection();
        }
    }
}
