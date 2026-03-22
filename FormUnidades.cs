using System;
using System.Drawing;
using System.Windows.Forms;
using ProyectoCatedra.Servicios;
using ProyectoCatedra.Modelos;
using ProyectoCatedra.Estructuras;
using ProyectoCatedra.Utilidades;

namespace ProyectoCatedra
{
    public class FormUnidades : Form
    {
        private UnidadServicio servicio;
        private CategoriaServicio catServicio;
        private TextBox txtNombre;
        private ComboBox cbTipo, cbCategorias;
        private DataGridView dgvUnidades, dgvPivote;
        private Button btnGuardar, btnAsociar;

        public FormUnidades() { 
            servicio = new UnidadServicio(); 
            catServicio = new CategoriaServicio(); 
            InicializarComponentes(); 
            CargarTodo(); 
        }

        private void InicializarComponentes()
        {
            this.Text = "Unidades de Medida y Categorías";
            this.Size = new Size(800, 550);
            this.StartPosition = FormStartPosition.CenterParent;

            GroupBox gb1 = new GroupBox { Text = "1. Crear Unidad de Medida", Location = new Point(20, 20), Size = new Size(350, 150) };
            txtNombre = new TextBox { Location = new Point(15, 40), Size = new Size(150, 20) };
            cbTipo = new ComboBox { Location = new Point(180, 40), Size = new Size(140, 20), DropDownStyle = ComboBoxStyle.DropDownList };
            cbTipo.Items.AddRange(new string[] { "Peso (lb/kg)", "Volumen (lt/ml)", "Unidad (pza/bolsa)" });
            btnGuardar = new Button { Text = "Registrar", Location = new Point(15, 80), Size = new Size(100, 30) };
            btnGuardar.Click += (s, e) => {
                if (string.IsNullOrWhiteSpace(txtNombre.Text) || cbTipo.SelectedIndex == -1) return;
                servicio.Guardar(new UnidadMedida { Nombre = txtNombre.Text, Tipo = cbTipo.SelectedItem.ToString() });
                CargarTodo();
            };
            gb1.Controls.AddRange(new Control[] { new Label { Text = "Nombre:", Location = new Point(15, 20) }, new Label { Text = "Tipo:", Location = new Point(180, 20) }, txtNombre, cbTipo, btnGuardar });

            GroupBox gb2 = new GroupBox { Text = "2. Asociar a Categoría", Location = new Point(400, 20), Size = new Size(350, 150) };
            cbCategorias = new ComboBox { Location = new Point(15, 40), Size = new Size(150, 20), DropDownStyle = ComboBoxStyle.DropDownList };
            btnAsociar = new Button { Text = "Asociar Seleccionada", Location = new Point(15, 80), Size = new Size(150, 30) };
            btnAsociar.Click += (s, e) => {
                if (cbCategorias.SelectedItem == null || dgvUnidades.SelectedRows.Count == 0) return;
                int idCat = ((Categoria)cbCategorias.SelectedItem).Id;
                int idUni = (int)dgvUnidades.SelectedRows[0].Cells[0].Value;
                servicio.AsociarACategoria(idCat, idUni);
                CargarPivote();
            };
            gb2.Controls.AddRange(new Control[] { new Label { Text = "Categoría:", Location = new Point(15, 20) }, cbCategorias, btnAsociar });

            dgvUnidades = new DataGridView { Location = new Point(20, 190), Size = new Size(350, 250), SelectionMode = DataGridViewSelectionMode.FullRowSelect, ReadOnly = true, AllowUserToAddRows = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill };
            dgvUnidades.Columns.Add("Id", "ID"); dgvUnidades.Columns.Add("Nom", "Nombre"); dgvUnidades.Columns.Add("T", "Tipo");

            dgvPivote = new DataGridView { Location = new Point(400, 190), Size = new Size(350, 250), ReadOnly = true, AllowUserToAddRows = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill };
            dgvPivote.Columns.Add("Cat", "Categoría"); dgvPivote.Columns.Add("Uni", "Unidad Permitida");

            Button btnPlantilla = new Button { Text = "Plantilla", Location = new Point(20, 460), Size = new Size(100, 30) };
            btnPlantilla.Click += (s, e) => ManejadorCSV.GuardarPlantillaConDialogo("plantilla_unidades.csv", "Nombre,Tipo\nLibra,Peso (lb/kg)\nLitro,Volumen (lt/ml)\nBolsa,Unidad (pza/bolsa)");

            this.Controls.AddRange(new Control[] { gb1, gb2, dgvUnidades, dgvPivote, btnPlantilla });
            cbCategorias.SelectedIndexChanged += (s, e) => CargarPivote();
        }

        private void CargarTodo()
        {
            dgvUnidades.Rows.Clear();
            var l = servicio.ListarTodas();
            for (int i = 0; i < l.Conteo(); i++) { var u = (UnidadMedida)l.Obtener(i); dgvUnidades.Rows.Add(u.Id, u.Nombre, u.Tipo); }

            cbCategorias.Items.Clear();
            var cats = catServicio.ListarTodas();
            for (int i = 0; i < cats.Conteo(); i++) cbCategorias.Items.Add(cats.Obtener(i));
        }

        private void CargarPivote()
        {
            dgvPivote.Rows.Clear();
            if (cbCategorias.SelectedItem == null) return;
            var cat = (Categoria)cbCategorias.SelectedItem;
            var l = servicio.ListarPorCategoria(cat.Id);
            for (int i = 0; i < l.Conteo(); i++) { var u = (UnidadMedida)l.Obtener(i); dgvPivote.Rows.Add(cat.Nombre, u.Nombre); }
        }
    }
}
