using System;
using System.Drawing;
using System.Windows.Forms;
using ProyectoCatedra.Servicios;
using ProyectoCatedra.Modelos;
using ProyectoCatedra.Estructuras;
using ProyectoCatedra.Utilidades;

namespace ProyectoCatedra
{
    public class FormCategorias : Form
    {
        private CategoriaServicio servicio;
        private Pila undoStack;
        private TextBox txtNombre;
        private DataGridView dgv;
        private Button btnGuardar, btnEditar, btnEliminar, btnUndo;
        private Categoria seleccionado;

        public FormCategorias() { 
            servicio = new CategoriaServicio(); 
            undoStack = new Pila(); 
            InicializarComponentes(); 
            Cargar(); 
        }

        private void InicializarComponentes()
        {
            this.Text = "CRUD Categorías (Con Undo)";
            this.Size = new Size(550, 480);
            this.StartPosition = FormStartPosition.CenterParent;

            Label lbl = new Label { Text = "Nombre:", Location = new Point(20, 20), AutoSize = true };
            txtNombre = new TextBox { Location = new Point(20, 40), Size = new Size(180, 20) };
            
            btnGuardar = new Button { Text = "Guardar", Location = new Point(210, 38), Size = new Size(70, 25) };
            btnGuardar.Click += (s, e) => {
                if (servicio.ExisteNombre(txtNombre.Text)) { MessageBox.Show("Ya existe una categoría con ese nombre.", "Duplicado", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
                var c = new Categoria { Nombre = txtNombre.Text };
                servicio.Guardar(c);
                undoStack.Empujar(new AccionUndo(TipoAccion.Insertar, "Categorias", c));
                Cargar(); Limpiar();
            };

            btnEditar = new Button { Text = "Modificar", Location = new Point(290, 38), Size = new Size(70, 25), Enabled = false };
            btnEditar.Click += (s, e) => {
                var anterior = new Categoria { Id = seleccionado.Id, Nombre = seleccionado.Nombre };
                seleccionado.Nombre = txtNombre.Text;
                servicio.Actualizar(seleccionado);
                undoStack.Empujar(new AccionUndo(TipoAccion.Editar, "Categorias", anterior));
                Cargar(); Limpiar();
            };

            btnEliminar = new Button { Text = "Eliminar", Location = new Point(370, 38), Size = new Size(70, 25), Enabled = false };
            btnEliminar.Click += (s, e) => {
                undoStack.Empujar(new AccionUndo(TipoAccion.Eliminar, "Categorias", seleccionado));
                servicio.Eliminar(seleccionado.Id);
                Cargar(); Limpiar();
            };

            btnUndo = new Button { Text = "Deshacer (Pila)", Location = new Point(20, 360), Size = new Size(120, 30), BackColor = Color.LightGray };
            btnUndo.Click += Undo;

            dgv = new DataGridView { Location = new Point(20, 80), Size = new Size(490, 260), SelectionMode = DataGridViewSelectionMode.FullRowSelect, ReadOnly = true, AllowUserToAddRows = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill };
            dgv.Columns.Add("Id", "ID"); dgv.Columns.Add("Nom", "Nombre");
            dgv.SelectionChanged += (s, e) => {
                if (dgv.SelectedRows.Count > 0) {
                    seleccionado = new Categoria { Id = (int)dgv.SelectedRows[0].Cells[0].Value, Nombre = dgv.SelectedRows[0].Cells[1].Value.ToString() };
                    txtNombre.Text = seleccionado.Nombre; btnEditar.Enabled = btnEliminar.Enabled = true;
                }
            };

            Button btnImp = new Button { Text = "Importar", Location = new Point(150, 360), Size = new Size(80, 30) };
            btnImp.Click += Importar;

            Button btnPlant = new Button { Text = "Plantilla", Location = new Point(240, 360), Size = new Size(80, 30) };
            btnPlant.Click += (s, e) => ManejadorCSV.GuardarPlantillaConDialogo("plantilla_categorias.csv", "Nombre\nGranos\nLacteos");

            this.Controls.AddRange(new Control[] { lbl, txtNombre, btnGuardar, btnEditar, btnEliminar, btnUndo, dgv, btnImp, btnPlant });
        }

        private void Importar(object sender, EventArgs e)
        {
            try {
                OpenFileDialog ofd = new OpenFileDialog();
                if (ofd.ShowDialog() == DialogResult.OK) {
                    var l = ManejadorCSV.ParsearCategorias(ofd.FileName);
                    for (int i = 0; i < l.Conteo(); i++) servicio.Guardar((Categoria)l.Obtener(i));
                    Cargar();
                }
            } catch (Exception ex) { MessageBox.Show("Error al importar categorías:\n" + ex.Message, "Falla", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        private void Undo(object sender, EventArgs e)
        {
            var acc = (AccionUndo)undoStack.Pop();
            if (acc == null) { MessageBox.Show("No hay acciones para deshacer."); return; }
            var c = (Categoria)acc.Datos;
            if (acc.Tipo == TipoAccion.Insertar) {
                // Para borrar lo insertado necesito el ID. En este caso el ultimo nombre
                var id = servicio.ObtenerIdPorNombre(c.Nombre);
                if (id != -1) servicio.Eliminar(id);
            } else if (acc.Tipo == TipoAccion.Editar) {
                servicio.Actualizar(c);
            } else if (acc.Tipo == TipoAccion.Eliminar) {
                servicio.Guardar(c);
            }
            Cargar();
        }

        private void Limpiar() { txtNombre.Clear(); seleccionado = null; btnEditar.Enabled = btnEliminar.Enabled = false; }
        private void Cargar() { dgv.Rows.Clear(); var l = servicio.ListarTodas(); for (int i = 0; i < l.Conteo(); i++) { var c = (Categoria)l.Obtener(i); dgv.Rows.Add(c.Id, c.Nombre); } }
    }
}
