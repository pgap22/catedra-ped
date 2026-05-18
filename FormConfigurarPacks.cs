using System;
using System.Drawing;
using System.Windows.Forms;
using ProyectoCatedra.Estructuras;
using ProyectoCatedra.Modelos;
using ProyectoCatedra.Servicios;

namespace ProyectoCatedra
{
    public class FormConfigurarPacks : Form
    {
        private readonly CategoriaServicio categoriaServicio = new CategoriaServicio();
        private readonly ProductoServicio productoServicio = new ProductoServicio();
        private readonly CategoriaPackServicio packServicio = new CategoriaPackServicio();

        private ComboBox cbCategoria = new ComboBox();
        private DataGridView dgv = new DataGridView();
        private Label lblTotal = new Label();
        private Button btnGuardar = new Button();
        private Button btnEliminar = new Button();

        public FormConfigurarPacks()
        {
            InicializarComponentes();
            CargarCategorias();
        }

        private void InicializarComponentes()
        {
            AutoScaleMode = AutoScaleMode.None;
            Text = "Configurar Packs por Categoría";
            Size = new Size(760, 500);
            MinimumSize = new Size(720, 440);
            StartPosition = FormStartPosition.CenterParent;

            Label lblCategoria = new Label { Text = "Categoría:", Location = new Point(20, 22), AutoSize = true };
            cbCategoria.Location = new Point(90, 18);
            cbCategoria.Size = new Size(240, 25);
            cbCategoria.DropDownStyle = ComboBoxStyle.DropDownList;
            cbCategoria.SelectedIndexChanged += (s, e) => CargarProductosCategoria();

            lblTotal.Location = new Point(350, 22);
            lblTotal.AutoSize = true;
            lblTotal.Font = new Font(lblTotal.Font, FontStyle.Bold);

            btnGuardar.Text = "Guardar Pack";
            btnGuardar.Location = new Point(520, 15);
            btnGuardar.Size = new Size(100, 30);
            btnGuardar.Enabled = false;
            btnGuardar.Click += BtnGuardar_Click;

            btnEliminar.Text = "Eliminar Pack";
            btnEliminar.Location = new Point(630, 15);
            btnEliminar.Size = new Size(100, 30);
            btnEliminar.Click += BtnEliminar_Click;

            dgv.Location = new Point(20, 60);
            dgv.Size = new Size(710, 340);
            dgv.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dgv.AllowUserToAddRows = false;
            dgv.SelectionMode = DataGridViewSelectionMode.CellSelect;
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgv.Columns.Add("ProductoId", "ProductoId");
            dgv.Columns["ProductoId"].Visible = false;
            dgv.Columns.Add("SKU", "SKU");
            dgv.Columns["SKU"].ReadOnly = true;
            dgv.Columns.Add("Producto", "Producto");
            dgv.Columns["Producto"].ReadOnly = true;
            dgv.Columns.Add("Stock", "Stock");
            dgv.Columns["Stock"].ReadOnly = true;
            dgv.Columns.Add("Porcentaje", "Porcentaje (%)");
            dgv.CellEndEdit += (s, e) => ActualizarTotal();

            Label lblAyuda = new Label
            {
                Text = "Ingrese porcentajes enteros o decimales. El total debe ser exactamente 100% para guardar.",
                Location = new Point(20, 415),
                AutoSize = true,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left,
                ForeColor = Color.Gray,
                Font = new Font(Font, FontStyle.Italic)
            };

            Controls.AddRange(new Control[] { lblCategoria, cbCategoria, lblTotal, btnGuardar, btnEliminar, dgv, lblAyuda });
        }

        private void CargarCategorias()
        {
            cbCategoria.Items.Clear();
            var categorias = categoriaServicio.ListarTodas();
            for (int i = 0; i < categorias.Conteo(); i++)
            {
                var categoria = (Categoria)categorias.Obtener(i)!;
                cbCategoria.Items.Add(categoria);
            }
            cbCategoria.DisplayMember = "Nombre";
            cbCategoria.ValueMember = "Id";
            if (cbCategoria.Items.Count > 0) cbCategoria.SelectedIndex = 0;
        }

        private void CargarProductosCategoria()
        {
            dgv.Rows.Clear();
            if (cbCategoria.SelectedItem == null) return;

            int categoriaId = ((Categoria)cbCategoria.SelectedItem).Id;
            TablaHash porcentajes = new TablaHash(101);
            var existentes = packServicio.ListarPorCategoria(categoriaId);
            for (int i = 0; i < existentes.Conteo(); i++)
            {
                var linea = (CategoriaPackDetalle)existentes.Obtener(i)!;
                porcentajes.Insertar(linea.ProductoId.ToString(), linea.Porcentaje);
            }

            var productos = productoServicio.ListarPorCategoria(categoriaId);
            for (int i = 0; i < productos.Conteo(); i++)
            {
                var producto = (Producto)productos.Obtener(i)!;
                double porcentaje = (double?)porcentajes.Buscar(producto.Id.ToString()) ?? 0;
                dgv.Rows.Add(producto.Id, producto.SKU, producto.Nombre, producto.Stock, porcentaje);
            }

            ActualizarTotal();
        }

        private void ActualizarTotal()
        {
            double total = CalcularTotal();
            lblTotal.Text = $"Total: {total}%";
            bool valido = Math.Abs(total - 100) < 0.001 && dgv.Rows.Count > 0;
            lblTotal.ForeColor = valido ? Color.DarkGreen : Color.DarkRed;
            btnGuardar.Enabled = valido;
        }

        private double CalcularTotal()
        {
            double total = 0;
            for (int i = 0; i < dgv.Rows.Count; i++)
            {
                object? valor = dgv.Rows[i].Cells["Porcentaje"].Value;
                if (double.TryParse(valor?.ToString(), out double porcentaje) && porcentaje > 0)
                {
                    total += porcentaje;
                }
            }
            return Math.Round(total, 2);
        }

        private void BtnGuardar_Click(object? sender, EventArgs e)
        {
            if (cbCategoria.SelectedItem == null) return;
            int categoriaId = ((Categoria)cbCategoria.SelectedItem).Id;

            ListaEnlazada lineas = new ListaEnlazada();
            for (int i = 0; i < dgv.Rows.Count; i++)
            {
                object? valor = dgv.Rows[i].Cells["Porcentaje"].Value;
                if (!double.TryParse(valor?.ToString(), out double porcentaje)) porcentaje = 0;
                if (porcentaje <= 0) continue;

                lineas.Agregar(new CategoriaPackDetalle
                {
                    CategoriaId = categoriaId,
                    ProductoId = Convert.ToInt32(dgv.Rows[i].Cells["ProductoId"].Value),
                    Porcentaje = porcentaje
                });
            }

            try
            {
                packServicio.GuardarPack(categoriaId, lineas);
                MessageBox.Show("Pack guardado correctamente.", "Packs", MessageBoxButtons.OK, MessageBoxIcon.Information);
                CargarProductosCategoria();
            }
            catch (Exception ex)
            {
                MessageBox.Show("No se pudo guardar el pack: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnEliminar_Click(object? sender, EventArgs e)
        {
            if (cbCategoria.SelectedItem == null) return;
            int categoriaId = ((Categoria)cbCategoria.SelectedItem).Id;
            if (MessageBox.Show("¿Eliminar el pack de esta categoría?", "Packs", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;

            try
            {
                packServicio.EliminarPack(categoriaId);
                CargarProductosCategoria();
            }
            catch (Exception ex)
            {
                MessageBox.Show("No se pudo eliminar el pack: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
