using System;
using System.Drawing;
using System.Windows.Forms;

namespace ProyectoCatedra
{
    public partial class Form1 : Form
    {
        private Button btnCategorias, btnUnidades, btnProductos, btnBeneficiarios;
        private Label lblTitulo;

        public Form1()
        {
            InitializeComponent();
            this.Text = "SistDonaciones - Menú Principal";
            this.Size = new Size(400, 420);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            
            lblTitulo = new Label { 
                Text = "Gestión de Donaciones", 
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                Location = new Point(20, 20), Size = new Size(360, 30), TextAlign = ContentAlignment.MiddleCenter 
            };

            // 1. Categorías
            btnCategorias = new Button { Text = "Categorías", Location = new Point(100, 80), Size = new Size(200, 45), 
                                        Image = SystemIcons.Information.ToBitmap(), ImageAlign = ContentAlignment.MiddleLeft };
            btnCategorias.Click += (s, e) => new FormCategorias().ShowDialog();

            // 2. Unidades (NUEVO)
            btnUnidades = new Button { Text = "Unidades de Medida", Location = new Point(100, 140), Size = new Size(200, 45), 
                                       Image = SystemIcons.Exclamation.ToBitmap(), ImageAlign = ContentAlignment.MiddleLeft };
            btnUnidades.Click += (s, e) => new FormUnidades().ShowDialog();

            // 3. Productos
            btnProductos = new Button { Text = "Inventario de Productos", Location = new Point(100, 200), Size = new Size(200, 45), 
                                        Image = SystemIcons.Question.ToBitmap(), ImageAlign = ContentAlignment.MiddleLeft };
            btnProductos.Click += (s, e) => new FormProductos().ShowDialog();

            // 4. Beneficiarios
            btnBeneficiarios = new Button { Text = "Padrón de Beneficiarios", Location = new Point(100, 260), Size = new Size(200, 45), 
                                           Image = SystemIcons.Shield.ToBitmap(), ImageAlign = ContentAlignment.MiddleLeft };
            btnBeneficiarios.Click += (s, e) => new FormBeneficiarios().ShowDialog();

            this.Controls.AddRange(new Control[] { lblTitulo, btnCategorias, btnUnidades, btnProductos, btnBeneficiarios });
        }
    }
}
