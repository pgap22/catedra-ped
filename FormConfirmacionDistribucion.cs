using System;
using System.Drawing;
using System.Windows.Forms;
using ProyectoCatedra.Estructuras;
using ProyectoCatedra.Modelos;

namespace ProyectoCatedra
{
    public class FormConfirmacionDistribucion : Form
    {
        private DataGridView dgvResumen = new DataGridView();
        private Label lblTitulo = new Label();
        private Label lblResumen = new Label();
        private Button btnConfirmar = new Button();
        private Button btnCancelar = new Button();

        public FormConfirmacionDistribucion(ListaEnlazada detalles, string titulo)
        {
            InicializarComponentes(titulo);
            CargarDetalles(detalles);
        }

        private void InicializarComponentes(string titulo)
        {
            this.AutoScaleMode = AutoScaleMode.None;
            this.Text = "Confirmar entrega";
            this.Size = new Size(760, 460);
            this.MinimumSize = new Size(720, 420);
            this.StartPosition = FormStartPosition.CenterParent;

            lblTitulo.Text = titulo;
            lblTitulo.Location = new Point(20, 15);
            lblTitulo.Size = new Size(700, 25);
            lblTitulo.Font = new Font(this.Font, FontStyle.Bold);

            lblResumen.Location = new Point(20, 45);
            lblResumen.Size = new Size(700, 35);
            lblResumen.ForeColor = Color.DarkRed;

            dgvResumen.Location = new Point(20, 85);
            dgvResumen.Size = new Size(700, 270);
            dgvResumen.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dgvResumen.ReadOnly = true;
            dgvResumen.AllowUserToAddRows = false;
            dgvResumen.AllowUserToDeleteRows = false;
            dgvResumen.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvResumen.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            dgvResumen.Columns.Add("Beneficiario", "Familia");
            dgvResumen.Columns.Add("Vulnerabilidad", "Vulnerabilidad");
            dgvResumen.Columns.Add("Categoria", "Categoría");
            dgvResumen.Columns.Add("Producto", "Producto");
            dgvResumen.Columns.Add("SKU", "SKU");
            dgvResumen.Columns.Add("Cantidad", "Cantidad a descontar");
            dgvResumen.Columns.Add("Unidad", "Unidad");

            btnConfirmar.Text = "Confirmar y descontar stock";
            btnConfirmar.Size = new Size(200, 35);
            btnConfirmar.Location = new Point(315, 370);
            btnConfirmar.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnConfirmar.BackColor = Color.LightGreen;
            btnConfirmar.DialogResult = DialogResult.OK;

            btnCancelar.Text = "Cancelar";
            btnCancelar.Size = new Size(100, 35);
            btnCancelar.Location = new Point(525, 370);
            btnCancelar.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnCancelar.DialogResult = DialogResult.Cancel;

            this.AcceptButton = btnConfirmar;
            this.CancelButton = btnCancelar;

            this.Controls.Add(lblTitulo);
            this.Controls.Add(lblResumen);
            this.Controls.Add(dgvResumen);
            this.Controls.Add(btnConfirmar);
            this.Controls.Add(btnCancelar);
        }

        private void CargarDetalles(ListaEnlazada detalles)
        {
            double total = 0;
            for (int i = 0; i < detalles.Conteo(); i++)
            {
                var det = (OrdenDetalle)detalles.Obtener(i)!;
                double cantidad = Math.Floor(det.CantidadAsignada);
                if (cantidad <= 0) continue;

                total += cantidad;
                dgvResumen.Rows.Add(
                    det.NombreBeneficiario,
                    det.VulnerabilidadTexto,
                    det.NombreCategoria,
                    det.NombreProductoSugerido,
                    det.SKUProductoSugerido,
                    cantidad,
                    det.NombreUnidadMedida
                );
            }

            lblResumen.Text = $"Revise la entrega antes de confirmar. Se descontarán {total} unidades del stock real.";
            btnConfirmar.Enabled = dgvResumen.Rows.Count > 0;
        }
    }
}
