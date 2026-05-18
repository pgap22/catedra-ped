using System;
using System.Drawing;
using System.Windows.Forms;
using ProyectoCatedra.Servicios;
using ProyectoCatedra.Estructuras;

namespace ProyectoCatedra
{
    public class FormHistorial : Form
    {
        private HistorialServicio servicio;
        private DataGridView dgv = new DataGridView();
        private Button btnRefrescar = new Button();
        private TextBox txtBuscar = new TextBox();
        private Button btnBuscar = new Button();
        private Label lblTotal = new Label();

        public FormHistorial()
        {
            servicio = new HistorialServicio();
            InicializarComponentes();
            CargarDatos();
        }

        private void InicializarComponentes()
        {
            this.AutoScaleMode = AutoScaleMode.None;
            this.Text = "Historial de Entregas Realizadas";
            this.Size = new Size(850, 500);
            this.MinimumSize = new Size(800, 460);
            this.StartPosition = FormStartPosition.CenterParent;

            btnRefrescar.Text = "Refrescar";
            btnRefrescar.Location = new Point(20, 15);
            btnRefrescar.Size = new Size(100, 30);
            btnRefrescar.Click += (s, e) => CargarDatos();

            Label lblBuscar = new Label { Text = "Beneficiario:", Location = new Point(130, 22), AutoSize = true };
            txtBuscar.Location = new Point(210, 19);
            txtBuscar.Size = new Size(180, 23);
            btnBuscar.Text = "Filtrar";
            btnBuscar.Location = new Point(400, 15);
            btnBuscar.Size = new Size(80, 30);
            btnBuscar.Click += (s, e) => CargarDatos(txtBuscar.Text);

            lblTotal.Location = new Point(500, 20);
            lblTotal.AutoSize = true;
            lblTotal.Font = new Font(lblTotal.Font, FontStyle.Bold);

            dgv.Location = new Point(20, 60);
            dgv.Size = new Size(800, 380);
            dgv.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgv.ReadOnly = true;
            dgv.AllowUserToAddRows = false;
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            dgv.Columns.Add("OrdenId", "Nº Orden");
            dgv.Columns.Add("Fecha", "Fecha");
            dgv.Columns.Add("Beneficiario", "Beneficiario");
            dgv.Columns.Add("Categoria", "Categoría");
            dgv.Columns.Add("Producto", "Producto");
            dgv.Columns.Add("Deficit", "Déficit Previo");
            dgv.Columns.Add("Asignado", "Asignado");
            dgv.Columns.Add("Unidad", "Unidad");

            this.Controls.Add(btnRefrescar);
            this.Controls.Add(lblBuscar);
            this.Controls.Add(txtBuscar);
            this.Controls.Add(btnBuscar);
            this.Controls.Add(lblTotal);
            this.Controls.Add(dgv);
        }

        private void CargarDatos(string filtroBeneficiario = "")
        {
            dgv.Rows.Clear();
            var historial = servicio.ObtenerHistorialDistribuciones();
            string filtro = filtroBeneficiario.Trim().ToLower();
            int mostrados = 0;
            
            for (int i = 0; i < historial.Conteo(); i++)
            {
                var reg = (RegistroHistorial)historial.Obtener(i)!;
                if (!string.IsNullOrWhiteSpace(filtro) && !reg.Beneficiario.ToLower().Contains(filtro)) continue;

                dgv.Rows.Add(
                    reg.OrdenId,
                    reg.Fecha.ToString("yyyy-MM-dd HH:mm"),
                    reg.Beneficiario,
                    reg.Categoria,
                    string.IsNullOrWhiteSpace(reg.Producto) ? "(histórico por categoría)" : reg.Producto,
                    Math.Round(reg.DeficitCalculado, 2),
                    Math.Floor(reg.CantidadAsignada),
                    reg.Unidad
                );
                mostrados++;
            }
            lblTotal.Text = $"Asignaciones mostradas: {mostrados}";
        }
    }
}
