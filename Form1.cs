using System;
using System.Drawing;
using System.Windows.Forms;
using ProyectoCatedra.Utilidades;

namespace ProyectoCatedra
{
    public partial class Form1 : Form
    {
        private MenuStrip menuPrincipal;
        private Label lblTituloCentro;
        private Label lblSubtitulo;

        public Form1()
        {
            InitializeComponent();
            this.AutoScaleMode = AutoScaleMode.None;
            this.Text = "Sistema de Donaciones";
            this.Size = new Size(900, 520);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.BackColor = Color.FromArgb(241, 246, 252);

            InicializarMenuSuperior();
            InicializarCentro();
            AplicarEscaladoDpi();
        }

        private void InicializarMenuSuperior()
        {
            menuPrincipal = new MenuStrip();
            menuPrincipal.Dock = DockStyle.Top;
            menuPrincipal.BackColor = Color.FromArgb(32, 62, 92);
            menuPrincipal.ForeColor = Color.White;
            menuPrincipal.Font = new Font("Segoe UI", 10, FontStyle.Bold);

            ToolStripMenuItem itemCategorias = new ToolStripMenuItem("Categorias");
            ToolStripMenuItem itemUnidades = new ToolStripMenuItem("Unidades");
            ToolStripMenuItem itemProductos = new ToolStripMenuItem("Productos");
            ToolStripMenuItem itemBeneficiarios = new ToolStripMenuItem("Beneficiarios");
            ToolStripMenuItem itemSalir = new ToolStripMenuItem("Salir");

            itemCategorias.Click += (s, e) => new FormCategorias().ShowDialog();
            itemUnidades.Click += (s, e) => new FormUnidades().ShowDialog();
            itemProductos.Click += (s, e) => new FormProductos().ShowDialog();
            itemBeneficiarios.Click += (s, e) => new FormBeneficiarios().ShowDialog();
            itemSalir.Click += (s, e) => this.Close();

            menuPrincipal.Items.Add(itemCategorias);
            menuPrincipal.Items.Add(itemUnidades);
            menuPrincipal.Items.Add(itemProductos);
            menuPrincipal.Items.Add(itemBeneficiarios);
            menuPrincipal.Items.Add(itemSalir);

            this.MainMenuStrip = menuPrincipal;
            this.Controls.Add(menuPrincipal);
        }

        private void InicializarCentro()
        {
            Panel panelCentro = new Panel();
            panelCentro.Dock = DockStyle.Fill;
            panelCentro.Padding = new Padding(20);

            lblTituloCentro = new Label();
            lblTituloCentro.Text = "SISTEMA DE DONACIONES";
            lblTituloCentro.Font = new Font("Segoe UI", 28, FontStyle.Bold);
            lblTituloCentro.ForeColor = Color.FromArgb(25, 50, 74);
            lblTituloCentro.TextAlign = ContentAlignment.MiddleCenter;
            lblTituloCentro.Dock = DockStyle.Fill;

            lblSubtitulo = new Label();
            lblSubtitulo.Text = "Seleccione una opcion del menu superior";
            lblSubtitulo.Font = new Font("Segoe UI", 12, FontStyle.Regular);
            lblSubtitulo.ForeColor = Color.FromArgb(80, 98, 117);
            lblSubtitulo.TextAlign = ContentAlignment.TopCenter;
            lblSubtitulo.Dock = DockStyle.Bottom;
            lblSubtitulo.Height = 70;

            panelCentro.Controls.Add(lblTituloCentro);
            panelCentro.Controls.Add(lblSubtitulo);
            this.Controls.Add(panelCentro);
        }

        private void AplicarEscaladoDpi()
        {
            float factor = DeviceDpi / 96f;
            if (factor <= 1f) return;

            this.Size = new Size((int)Math.Round(this.Width * factor), (int)Math.Round(this.Height * factor));
            EscaladorDpi.EscalarJerarquia(this, factor);
        }
    }
}
