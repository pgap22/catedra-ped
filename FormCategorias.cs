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
        private TextBox txtNombre = new TextBox();
        private DataGridView dgv = new DataGridView();
        private Button btnGuardar = new Button();
        private Button btnEditar = new Button();
        private Button btnEliminar = new Button();
        private Button btnUndo = new Button();
        private Button btnNuevo = new Button();
        private Categoria? seleccionado;

        public FormCategorias() { 
            servicio = new CategoriaServicio(); 
            undoStack = new Pila(); 
            InicializarComponentes(); 
            Cargar(); 
        }

        private void InicializarComponentes()
        {
            this.AutoScaleMode = AutoScaleMode.None;
            this.Text = "CRUD Categorías (Con Undo)";
            this.Size = new Size(550, 480);
            this.StartPosition = FormStartPosition.CenterParent;

            btnNuevo.Text = "Nuevo"; btnNuevo.Location = new Point(20, 15); btnNuevo.Size = new Size(70, 25);
            btnNuevo.Click += (s, e) => Limpiar();

            Label lbl = new Label { Text = "Nombre:", Location = new Point(20, 45), AutoSize = true };
            txtNombre.Location = new Point(20, 65); txtNombre.Size = new Size(180, 20);
            
            btnGuardar.Text = "Guardar"; btnGuardar.Location = new Point(210, 63); btnGuardar.Size = new Size(70, 25);
            btnGuardar.Click += (s, e) => {
                if (servicio.ExisteNombre(txtNombre.Text)) { MessageBox.Show("Ya existe una categoría con ese nombre."); return; }
                var c = new Categoria { Nombre = txtNombre.Text };
                servicio.Guardar(c);
                undoStack.Empujar(new AccionUndo(TipoAccion.Insertar, "Categorias", c));
                Cargar(); Limpiar();
            };

            btnEditar.Text = "Modificar"; btnEditar.Location = new Point(290, 63); btnEditar.Size = new Size(70, 25); btnEditar.Enabled = false;
            btnEditar.Click += (s, e) => {
                if (seleccionado == null) return;
                var anterior = new Categoria { Id = seleccionado.Id, Nombre = seleccionado.Nombre };
                seleccionado.Nombre = txtNombre.Text;
                servicio.Actualizar(seleccionado);
                undoStack.Empujar(new AccionUndo(TipoAccion.Editar, "Categorias", anterior));
                Cargar(); Limpiar();
            };

            btnEliminar.Text = "Eliminar"; btnEliminar.Location = new Point(370, 63); btnEliminar.Size = new Size(70, 25); btnEliminar.Enabled = false;
            btnEliminar.Click += (s, e) => {
                if (seleccionado == null) return;
                undoStack.Empujar(new AccionUndo(TipoAccion.Eliminar, "Categorias", seleccionado));
                servicio.Eliminar(seleccionado.Id);
                Cargar(); Limpiar();
            };

            btnUndo.Text = "Deshacer"; btnUndo.Location = new Point(20, 360); btnUndo.Size = new Size(100, 30);
            btnUndo.Click += (s, e) => {
                var acc = (AccionUndo?)undoStack.Pop();
                if (acc == null) return;
                
                if (acc.Tipo == TipoAccion.Importacion) {
                    var l = (ListaEnlazada)acc.Datos;
                    for (int i = 0; i < l.Conteo(); i++) {
                        var cImp = (Categoria?)l.Obtener(i);
                        if (cImp != null) {
                            var id = servicio.ObtenerIdPorNombre(cImp.Nombre);
                            if (id != -1) servicio.Eliminar(id);
                        }
                    }
                } else {
                    var c = (Categoria)acc.Datos;
                    if (acc.Tipo == TipoAccion.Insertar) {
                        var id = servicio.ObtenerIdPorNombre(c.Nombre);
                        if (id != -1) servicio.Eliminar(id);
                    } else if (acc.Tipo == TipoAccion.Editar) servicio.Actualizar(c);
                    else if (acc.Tipo == TipoAccion.Eliminar) servicio.Guardar(c);
                }
                Cargar();
            };

            dgv.Location = new Point(20, 100); dgv.Size = new Size(490, 240); dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect; dgv.ReadOnly = true; dgv.AllowUserToAddRows = false; dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgv.Columns.Add("Id", "ID"); dgv.Columns.Add("Nom", "Nombre");
            dgv.SelectionChanged += (s, e) => {
                if (dgv.SelectedRows.Count > 0) {
                    seleccionado = new Categoria { 
                        Id = (int)dgv.SelectedRows[0].Cells[0].Value, 
                        Nombre = dgv.SelectedRows[0].Cells[1].Value?.ToString() ?? "" 
                    };
                    txtNombre.Text = seleccionado.Nombre; btnEditar.Enabled = btnEliminar.Enabled = true;
                }
            };

            Button btnImp = new Button { Text = "Importar CSV", Location = new Point(130, 360), Size = new Size(100, 30) };
            btnImp.Click += (s, e) => {
                try {
                    OpenFileDialog ofd = new OpenFileDialog();
                    if (ofd.ShowDialog() == DialogResult.OK) {
                        var l = ManejadorCSV.ParsearCategorias(ofd.FileName);
                        
                        // Estructura de datos: TablaHash para O(1) en busqueda de duplicados
                        TablaHash hashActuales = new TablaHash();
                        var listaBD = servicio.ListarTodas();
                        for(int j=0; j<listaBD.Conteo(); j++) {
                            var catBD = (Categoria?)listaBD.Obtener(j);
                            if(catBD != null) hashActuales.Insertar(catBD.Nombre.ToUpper(), catBD);
                        }

                        ListaEnlazada insertadosReales = new ListaEnlazada();

                        for (int i = 0; i < l.Conteo(); i++) {
                            var c = (Categoria?)l.Obtener(i);
                            if (c != null && hashActuales.Buscar(c.Nombre.ToUpper()) == null) {
                                servicio.Guardar(c);
                                hashActuales.Insertar(c.Nombre.ToUpper(), c); // Lo agregamos al hash para no duplicarlo si viene 2 veces en el CSV
                                insertadosReales.Agregar(c);
                            }
                        }
                        
                        if (insertadosReales.Conteo() > 0) {
                            undoStack.Empujar(new AccionUndo(TipoAccion.Importacion, "Categorias", insertadosReales));
                            MessageBox.Show($"Se importaron {insertadosReales.Conteo()} categorías nuevas.");
                        } else {
                            MessageBox.Show("No se importó ninguna categoría. Todas estaban duplicadas.");
                        }
                        Cargar();
                    }
                } catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); }
            };

            Button btnPlantilla = new Button { Text = "Bajar Plantilla", Location = new Point(240, 360), Size = new Size(110, 30) };
            btnPlantilla.Click += (s, e) => ManejadorCSV.GuardarPlantillaConDialogo("plantilla_categorias.csv", "Nombre\nGranos Basicos\nLacteos\nAceites");

            this.Controls.AddRange(new Control[] { btnNuevo, lbl, txtNombre, btnGuardar, btnEditar, btnEliminar, btnUndo, dgv, btnImp, btnPlantilla });
            AplicarEscaladoDpi();
        }

        private void AplicarEscaladoDpi()
        {
            float factor = DeviceDpi / 96f;
            if (factor <= 1f) return;

            this.Size = new Size((int)Math.Round(this.Width * factor), (int)Math.Round(this.Height * factor));
            EscaladorDpi.EscalarJerarquia(this, factor);
        }

        private void Limpiar() { txtNombre.Clear(); seleccionado = null; btnEditar.Enabled = btnEliminar.Enabled = false; dgv.ClearSelection(); }
        private void Cargar() { dgv.Rows.Clear(); var l = servicio.ListarTodas(); for (int i = 0; i < l.Conteo(); i++) { var c = (Categoria?)l.Obtener(i); if (c != null) dgv.Rows.Add(c.Id, c.Nombre); } }
    }
}
