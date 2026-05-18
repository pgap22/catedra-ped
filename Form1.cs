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
            this.Size = new Size(1000, 560);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MinimumSize = new Size(900, 520);
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

            ToolStripMenuItem menuAjustes = new ToolStripMenuItem("Configuración");
            ToolStripMenuItem itemTasas = new ToolStripMenuItem("Tasas de Consumo (Meta)");
            menuAjustes.DropDownItems.Add(itemTasas);

            ToolStripMenuItem menuInventario = new ToolStripMenuItem("Inventario");
            ToolStripMenuItem itemCategorias = new ToolStripMenuItem("Gestionar Categorías");
            ToolStripMenuItem itemUnidades = new ToolStripMenuItem("Gestionar Unidades");
            ToolStripMenuItem itemProductos = new ToolStripMenuItem("Gestionar Productos");
            menuInventario.DropDownItems.Add(itemCategorias);
            menuInventario.DropDownItems.Add(itemUnidades);
            menuInventario.DropDownItems.Add(itemProductos);

            ToolStripMenuItem menuFamilias = new ToolStripMenuItem("Beneficiarios");
            ToolStripMenuItem itemBeneficiarios = new ToolStripMenuItem("Padrón de Familias");
            menuFamilias.DropDownItems.Add(itemBeneficiarios);

            ToolStripMenuItem menuEntregas = new ToolStripMenuItem("Ayuda Social");
            ToolStripMenuItem itemDistribucion = new ToolStripMenuItem("Generar Asignación (Reparto)");
            ToolStripMenuItem itemHistorial = new ToolStripMenuItem("Historial de Entregas Realizadas");
            menuEntregas.DropDownItems.Add(itemDistribucion);
            menuEntregas.DropDownItems.Add(itemHistorial);

            ToolStripMenuItem menuDevDemo = new ToolStripMenuItem("Dev / Demo");
            ToolStripMenuItem itemFechaDemo = new ToolStripMenuItem("Simular Fecha");
            ToolStripMenuItem itemDemoPequena = new ToolStripMenuItem("Sembrar Demo Pequeña");
            ToolStripMenuItem itemTest = new ToolStripMenuItem("Sembrar Datos de Prueba (150+)");
            menuDevDemo.DropDownItems.Add(itemFechaDemo);
            menuDevDemo.DropDownItems.Add(itemDemoPequena);
            menuDevDemo.DropDownItems.Add(itemTest);

            ToolStripMenuItem menuOpciones = new ToolStripMenuItem("Sistema");
            ToolStripMenuItem itemSalir = new ToolStripMenuItem("Salir del Sistema");
            menuOpciones.DropDownItems.Add(itemSalir);

            itemCategorias.Click += (s, e) => { var f = new FormCategorias(); f.Show(); };
            itemUnidades.Click += (s, e) => { var f = new FormUnidades(); f.Show(); };
            itemProductos.Click += (s, e) => { var f = new FormProductos(); f.Show(); };
            itemBeneficiarios.Click += (s, e) => { var f = new FormBeneficiarios(); f.Show(); };
            itemTasas.Click += (s, e) => { var f = new FormTasaConsumo(); f.Show(); };
            itemDistribucion.Click += (s, e) => { var f = new FormDistribucion(); f.Show(); };
            itemHistorial.Click += (s, e) => { var f = new FormHistorial(); f.Show(); };
            itemFechaDemo.Click += (s, e) => { var f = new FormDevFecha(); f.Show(); };
            itemDemoPequena.Click += (s, e) => {
                if (MessageBox.Show("Esto borrará todo y sembrará una demo pequeña. ¿Continuar?", "Demo Seed", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                {
                    try {
                        ProyectoCatedra.Utilidades.GeneradorDatos.SembrarDemoPequena();
                        MessageBox.Show("Demo pequeña regenerada. Reinicie el programa para que los servicios recarguen los datos en memoria.");
                        Application.Restart();
                    } catch (Exception ex) {
                        MessageBox.Show("Error: " + ex.Message);
                    }
                }
            };
            itemTest.Click += (s, e) => { 
                if (MessageBox.Show("Esto borrará todo y sembrará 150 familias y productos de prueba. ¿Continuar?", "Test Seed", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                {
                    try {
                        ProyectoCatedra.Utilidades.GeneradorDatos.SembrarDatosPrueba();
                        MessageBox.Show("Base de datos regenerada. Reinicie el programa para que los servicios recarguen los datos en memoria.");
                        Application.Restart();
                    } catch (Exception ex) {
                        MessageBox.Show("Error: " + ex.Message);
                    }
                }
            };
            itemSalir.Click += (s, e) => { Application.Exit(); };

            menuPrincipal.Items.Add(menuAjustes);
            menuPrincipal.Items.Add(menuInventario);
            menuPrincipal.Items.Add(menuFamilias);
            menuPrincipal.Items.Add(menuEntregas);
            menuPrincipal.Items.Add(menuDevDemo);
            menuPrincipal.Items.Add(menuOpciones);

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
            lblSubtitulo.Text = "Seleccione una opción del menú superior para comenzar";
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
