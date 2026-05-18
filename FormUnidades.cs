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
        private Pila undoStack;

        private TextBox txtNombre = new TextBox();
        private ComboBox cbTipo = new ComboBox();
        private ComboBox cbCategorias = new ComboBox();
        private DataGridView dgvUnidades = new DataGridView();
        private DataGridView dgvPivote = new DataGridView();
        private Button btnNuevo = new Button();
        private Button btnGuardar = new Button();
        private Button btnEditar = new Button();
        private Button btnEliminar = new Button();
        private Button btnAsociar = new Button();
        private Button btnQuitarAsociacion = new Button();
        private Button btnUndo = new Button();
        private Button btnImp = new Button();
        private Button btnPlantilla = new Button();
        
        private UnidadMedida? unidadSeleccionada;

        public FormUnidades() { 
            servicio = new UnidadServicio(); 
            catServicio = new CategoriaServicio(); 
            undoStack = new Pila();
            InicializarComponentes(); 
            CargarTodo(); 
        }

        private void InicializarComponentes()
        {
            this.AutoScaleMode = AutoScaleMode.None;
            this.Text = "Unidades de Medida y Categorías";
            this.Size = new Size(820, 560);
            this.StartPosition = FormStartPosition.CenterParent;

            // Grupo 1: Crear Unidades (CRUD completo)
            GroupBox gb1 = new GroupBox { Text = "1. Crear o Editar Unidades", Location = new Point(20, 20), Size = new Size(370, 150) };
            
            btnNuevo.Text = "Nuevo"; btnNuevo.Location = new Point(15, 25); btnNuevo.Size = new Size(60, 25);
            btnNuevo.Click += (s, e) => Limpiar();

            Label lblNom = new Label { Text = "Nombre:", Location = new Point(15, 55), AutoSize = true };
            txtNombre.Location = new Point(15, 75); txtNombre.Size = new Size(150, 20);
            
            Label lblTip = new Label { Text = "Tipo:", Location = new Point(180, 55), AutoSize = true };
            cbTipo.Location = new Point(180, 75); cbTipo.Size = new Size(170, 20); cbTipo.DropDownStyle = ComboBoxStyle.DropDownList;
            cbTipo.Items.AddRange(new string[] { "Peso (lb/kg)", "Volumen (lt/ml)", "Unidad (pza/bolsa)" });
            
            btnGuardar.Text = "Guardar"; btnGuardar.Location = new Point(15, 110); btnGuardar.Size = new Size(70, 30);
            btnGuardar.Click += (s, e) => {
                if (string.IsNullOrWhiteSpace(txtNombre.Text) || cbTipo.SelectedIndex == -1) return;

                string nombreNuevo = txtNombre.Text.Trim();
                string tipoNuevo = cbTipo.SelectedItem?.ToString() ?? "";

                if (YaExisteUnidad(nombreNuevo, tipoNuevo))
                {
                    MessageBox.Show("Ya existe una unidad con el mismo nombre y tipo.", "Registro duplicado", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var u = new UnidadMedida { Nombre = nombreNuevo, Tipo = tipoNuevo };
                servicio.Guardar(u);
                undoStack.Empujar(new AccionUndo(TipoAccion.Insertar, "Unidades", u));
                Limpiar(); CargarTodo();
            };

            btnEditar.Text = "Modificar"; btnEditar.Location = new Point(90, 110); btnEditar.Size = new Size(70, 30); btnEditar.Enabled = false;
            btnEditar.Click += (s, e) => {
                if (unidadSeleccionada == null) return;
                var ant = new UnidadMedida { Id = unidadSeleccionada.Id, Nombre = unidadSeleccionada.Nombre, Tipo = unidadSeleccionada.Tipo };
                
                unidadSeleccionada.Nombre = txtNombre.Text;
                unidadSeleccionada.Tipo = cbTipo.SelectedItem?.ToString() ?? "";
                servicio.Actualizar(unidadSeleccionada);
                
                undoStack.Empujar(new AccionUndo(TipoAccion.Editar, "Unidades", ant));
                Limpiar(); CargarTodo();
            };

            btnEliminar.Text = "Eliminar"; btnEliminar.Location = new Point(165, 110); btnEliminar.Size = new Size(70, 30); btnEliminar.Enabled = false;
            btnEliminar.Click += (s, e) => {
                if (unidadSeleccionada == null) return;
                undoStack.Empujar(new AccionUndo(TipoAccion.Eliminar, "Unidades", unidadSeleccionada));
                servicio.EliminarUnidad(unidadSeleccionada.Id);
                Limpiar(); CargarTodo();
            };

            gb1.Controls.AddRange(new Control[] { btnNuevo, lblNom, txtNombre, lblTip, cbTipo, btnGuardar, btnEditar, btnEliminar });

            // Grupo 2: Asociar a Categoria
            GroupBox gb2 = new GroupBox { Text = "2. Asignar Unidad a una Categoría", Location = new Point(410, 20), Size = new Size(370, 150) };
            Label lblCat = new Label { Text = "Categoría:", Location = new Point(15, 30), AutoSize = true };
            cbCategorias.Location = new Point(15, 50); cbCategorias.Size = new Size(200, 20); cbCategorias.DropDownStyle = ComboBoxStyle.DropDownList;
            
            btnAsociar.Text = "Vincular a Categoría"; btnAsociar.Location = new Point(15, 90); btnAsociar.Size = new Size(150, 30);
            btnAsociar.Click += (s, e) => {
                if (cbCategorias.SelectedItem == null || dgvUnidades.SelectedRows.Count == 0) return;
                int idCat = ((Categoria)cbCategorias.SelectedItem).Id;
                int idUni = (int)dgvUnidades.SelectedRows[0].Cells[0].Value;
                servicio.AsociarACategoria(idCat, idUni);
                CargarPivote();
            };

            btnQuitarAsociacion.Text = "Quitar Vinculo"; btnQuitarAsociacion.Location = new Point(175, 90); btnQuitarAsociacion.Size = new Size(150, 30); btnQuitarAsociacion.Enabled = false;
            btnQuitarAsociacion.Click += (s, e) => {
                if (cbCategorias.SelectedItem == null || dgvPivote.SelectedRows.Count == 0) return;
                int idCat = ((Categoria)cbCategorias.SelectedItem).Id;
                string nombreUnidad = dgvPivote.SelectedRows[0].Cells[1].Value?.ToString() ?? "";
                int idUni = servicio.ObtenerIdPorNombre(nombreUnidad);
                if (idUni != -1) servicio.EliminarAsociacion(idCat, idUni);
                CargarPivote();
            };

            gb2.Controls.AddRange(new Control[] { lblCat, cbCategorias, btnAsociar, btnQuitarAsociacion });

            TextBox txtBuscar = new TextBox { Location = new Point(20, 185), Size = new Size(200, 20) };
            Button btnBuscar = new Button { Text = "Buscar", Location = new Point(230, 183), Size = new Size(80, 25) };
            btnBuscar.Click += (s, e) => CargarTodo(txtBuscar.Text);

            // DataGridViews
            dgvUnidades.Location = new Point(20, 220); dgvUnidades.Size = new Size(370, 210); dgvUnidades.SelectionMode = DataGridViewSelectionMode.FullRowSelect; dgvUnidades.ReadOnly = true; dgvUnidades.AllowUserToAddRows = false; dgvUnidades.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvUnidades.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            dgvUnidades.Columns.Add("Id", "ID"); dgvUnidades.Columns.Add("Nom", "Nombre"); dgvUnidades.Columns.Add("T", "Tipo");
            dgvUnidades.SelectionChanged += (s, e) => {
                if (dgvUnidades.SelectedRows.Count > 0) {
                    unidadSeleccionada = new UnidadMedida {
                        Id = (int)dgvUnidades.SelectedRows[0].Cells[0].Value,
                        Nombre = dgvUnidades.SelectedRows[0].Cells[1].Value?.ToString() ?? "",
                        Tipo = dgvUnidades.SelectedRows[0].Cells[2].Value?.ToString() ?? ""
                    };
                    txtNombre.Text = unidadSeleccionada.Nombre;
                    cbTipo.SelectedItem = unidadSeleccionada.Tipo;
                    btnEditar.Enabled = btnEliminar.Enabled = true;
                }
            };

            dgvPivote.Location = new Point(410, 220); dgvPivote.Size = new Size(370, 210); dgvPivote.SelectionMode = DataGridViewSelectionMode.FullRowSelect; dgvPivote.ReadOnly = true; dgvPivote.AllowUserToAddRows = false; dgvPivote.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvPivote.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dgvPivote.Columns.Add("Cat", "Categoría"); dgvPivote.Columns.Add("Uni", "Unidad Permitida");
            dgvPivote.SelectionChanged += (s, e) => {
                btnQuitarAsociacion.Enabled = dgvPivote.SelectedRows.Count > 0;
            };

            // Botones inferiores
            btnUndo.Text = "Deshacer último cambio manual"; btnUndo.Location = new Point(20, 450); btnUndo.Size = new Size(180, 30);
            btnUndo.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnImp.Text = "Importar CSV"; btnImp.Location = new Point(210, 450); btnImp.Size = new Size(100, 30);
            btnImp.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnPlantilla.Text = "Bajar Plantilla"; btnPlantilla.Location = new Point(320, 450); btnPlantilla.Size = new Size(110, 30);
            btnPlantilla.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnPlantilla.Click += (s, e) => ManejadorCSV.GuardarPlantillaConDialogo("plantilla_unidades.csv", "Nombre,Tipo\nLibra,Peso (lb/kg)\nLitro,Volumen (lt/ml)\nBolsa,Unidad (pza/bolsa)");

            this.Controls.AddRange(new Control[] { gb1, gb2, txtBuscar, btnBuscar, dgvUnidades, dgvPivote, btnUndo, btnImp, btnPlantilla });
            cbCategorias.SelectedIndexChanged += (s, e) => CargarPivote();
            AplicarEscaladoDpi();
        }

        private void AplicarEscaladoDpi()
        {
            float factor = DeviceDpi / 96f;
            if (factor <= 1f) return;

            this.Size = new Size((int)Math.Round(this.Width * factor), (int)Math.Round(this.Height * factor));
            EscaladorDpi.EscalarJerarquia(this, factor);
        }

        private void Limpiar() { 
            txtNombre.Clear(); 
            cbTipo.SelectedIndex = -1; 
            unidadSeleccionada = null; 
            btnEditar.Enabled = btnEliminar.Enabled = false; 
            dgvUnidades.ClearSelection(); 
        }

        private void CargarTodo(string filtro = "")
        {
            dgvUnidades.Rows.Clear();
            var l = servicio.ListarTodas();
            filtro = filtro.Trim().ToLower();

            for (int i = 0; i < l.Conteo(); i++) { 
                var u = (UnidadMedida?)l.Obtener(i); 
                if (u != null) 
                {
                    if (!string.IsNullOrEmpty(filtro) && !u.Nombre.ToLower().Contains(filtro) && !u.Tipo.ToLower().Contains(filtro)) continue;
                    dgvUnidades.Rows.Add(u.Id, u.Nombre, u.Tipo); 
                }
            }

            cbCategorias.Items.Clear();
            var cats = catServicio.ListarTodas();
            for (int i = 0; i < cats.Conteo(); i++) {
                var c = cats.Obtener(i);
                if (c != null) cbCategorias.Items.Add(c);
            }
            CargarPivote();
        }

        private void CargarPivote()
        {
            dgvPivote.Rows.Clear();
            btnQuitarAsociacion.Enabled = false;
            if (cbCategorias.SelectedItem == null) return;
            var cat = (Categoria)cbCategorias.SelectedItem;
            var l = servicio.ListarPorCategoria(cat.Id);
            for (int i = 0; i < l.Conteo(); i++) { 
                var u = (UnidadMedida?)l.Obtener(i); 
                if (u != null) dgvPivote.Rows.Add(cat.Nombre, u.Nombre); 
            }
        }

        private bool YaExisteUnidad(string nombre, string tipo)
        {
            TablaHash indice = new TablaHash(211);
            ListaEnlazada unidades = servicio.ListarTodas();

            for (int i = 0; i < unidades.Conteo(); i++)
            {
                UnidadMedida? u = (UnidadMedida?)unidades.Obtener(i);
                if (u == null) continue;

                string claveActual = ConstruirClaveUnidad(u.Nombre, u.Tipo);
                indice.Insertar(claveActual, u.Id);
            }

            string claveNueva = ConstruirClaveUnidad(nombre, tipo);
            return indice.Buscar(claveNueva) != null;
        }

        private string ConstruirClaveUnidad(string nombre, string tipo)
        {
            return nombre.Trim().ToLower() + "|" + tipo.Trim().ToLower();
        }
    }
}
