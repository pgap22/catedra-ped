using System;
using System.Drawing;
using System.Windows.Forms;
using ProyectoCatedra.Servicios;
using ProyectoCatedra.Modelos;
using ProyectoCatedra.Estructuras;
using ProyectoCatedra.Utilidades;

namespace ProyectoCatedra
{
    public class FormBeneficiarios : Form
    {
        private BeneficiarioServicio servicio;
        private Pila undoStack;
        private TextBox txtNombre = new TextBox();
        private TextBox txtBuscar = new TextBox();
        private NumericUpDown numMiembros = new NumericUpDown();
        private DataGridView dgv = new DataGridView();
        private Button btnNuevo = new Button();
        private Button btnGuardar = new Button();
        private Button btnEditar = new Button();
        private Button btnEliminar = new Button();
        private Button btnUndo = new Button();
        private Button btnBuscar = new Button();
        private Beneficiario? seleccionado;

        public FormBeneficiarios() { 
            servicio = new BeneficiarioServicio(); 
            undoStack = new Pila(); 
            InicializarComponentes(); 
            Cargar(); 
        }

        private void InicializarComponentes()
        {
            this.AutoScaleMode = AutoScaleMode.None;
            this.Text = "Padrón de Beneficiarios";
            this.Size = new Size(650, 550);
            this.StartPosition = FormStartPosition.CenterParent;

            btnNuevo.Text = "Nuevo"; btnNuevo.Location = new Point(20, 15); btnNuevo.Size = new Size(60, 25);
            btnNuevo.Click += (s, e) => Limpiar();

            Label l1 = new Label { Text = "Nombre:", Location = new Point(20, 45), AutoSize = true };
            txtNombre.Location = new Point(20, 65); txtNombre.Size = new Size(180, 20);

            Label l2 = new Label { Text = "Miembros:", Location = new Point(210, 45), AutoSize = true };
            numMiembros.Location = new Point(210, 65); numMiembros.Size = new Size(60, 20); numMiembros.Minimum = 1;

            btnGuardar.Text = "Registrar"; btnGuardar.Location = new Point(280, 63); btnGuardar.Size = new Size(80, 25);
            btnGuardar.Click += (s, e) => { 
                string nombreNuevo = txtNombre.Text.Trim();
                if (string.IsNullOrWhiteSpace(nombreNuevo)) return;
                if (ExisteNombreBeneficiario(nombreNuevo, -1)) {
                    MessageBox.Show("Ya existe un beneficiario con ese nombre.", "Nombre duplicado", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var b = new Beneficiario { Nombre = nombreNuevo, MiembrosHogar = (int)numMiembros.Value, Activo = true };
                servicio.Guardar(b); 
                undoStack.Empujar(new AccionUndo(TipoAccion.Insertar, "Beneficiarios", b));
                Limpiar(); Cargar(); 
            };

            btnEditar.Text = "Modificar"; btnEditar.Location = new Point(370, 63); btnEditar.Size = new Size(80, 25); btnEditar.Enabled = false;
            btnEditar.Click += (s, e) => { 
                if (seleccionado == null) return;

                string nombreNuevo = txtNombre.Text.Trim();
                if (string.IsNullOrWhiteSpace(nombreNuevo)) return;
                if (ExisteNombreBeneficiario(nombreNuevo, seleccionado.Id)) {
                    MessageBox.Show("Ya existe otro beneficiario con ese nombre.", "Nombre duplicado", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var ant = new Beneficiario { Id = seleccionado.Id, Nombre = seleccionado.Nombre, MiembrosHogar = seleccionado.MiembrosHogar, Activo = true };
                seleccionado.Nombre = nombreNuevo; seleccionado.MiembrosHogar = (int)numMiembros.Value; 
                servicio.Actualizar(seleccionado); 
                undoStack.Empujar(new AccionUndo(TipoAccion.Editar, "Beneficiarios", ant));
                Limpiar(); Cargar(); 
            };

            btnEliminar.Text = "Eliminar"; btnEliminar.Location = new Point(460, 63); btnEliminar.Size = new Size(80, 25); btnEliminar.Enabled = false;
            btnEliminar.Click += (s, e) => { 
                if (seleccionado == null) return;
                undoStack.Empujar(new AccionUndo(TipoAccion.Eliminar, "Beneficiarios", seleccionado));
                servicio.Eliminar(seleccionado.Id); 
                Limpiar(); Cargar(); 
            };

            btnUndo.Text = "Deshacer"; btnUndo.Location = new Point(20, 460); btnUndo.Size = new Size(100, 30);
            btnUndo.Click += (s, e) => {
                var acc = (AccionUndo?)undoStack.Pop();
                if (acc == null) return;
                
                if (acc.Tipo == TipoAccion.Importacion) {
                    var lImp = (ListaEnlazada)acc.Datos;
                    for(int i=0; i<lImp.Conteo(); i++) {
                        var bImp = (Beneficiario?)lImp.Obtener(i);
                        if (bImp == null) continue;
                        var lista = servicio.ListarTodos();
                        for(int j=0; j<lista.Conteo(); j++) {
                            var temp = (Beneficiario?)lista.Obtener(j);
                            if(temp != null && temp.Nombre == bImp.Nombre) { servicio.Eliminar(temp.Id); break; }
                        }
                    }
                } else {
                    var b = (Beneficiario)acc.Datos;
                    if (acc.Tipo == TipoAccion.Insertar) {
                        var lista = servicio.ListarTodos();
                        for(int i=0; i<lista.Conteo(); i++) {
                            var temp = (Beneficiario?)lista.Obtener(i);
                            if(temp != null && temp.Nombre == b.Nombre) { servicio.Eliminar(temp.Id); break; }
                        }
                    } else if (acc.Tipo == TipoAccion.Editar) servicio.Actualizar(b);
                    else if (acc.Tipo == TipoAccion.Eliminar) servicio.Guardar(b);
                }
                Cargar();
            };

            GroupBox gb = new GroupBox { Text = "Búsqueda (BST Multi-Resultado)", Location = new Point(20, 100), Size = new Size(590, 60) };
            txtBuscar.Location = new Point(15, 25); txtBuscar.Size = new Size(350, 20);
            btnBuscar.Text = "Buscar"; btnBuscar.Location = new Point(380, 23); btnBuscar.Size = new Size(140, 25);
            btnBuscar.Click += (s, e) => {
                var resultados = servicio.CargarEnArbol().Buscar(txtBuscar.Text);
                dgv.Rows.Clear();
                if (resultados != null) { 
                    for (int i = 0; i < resultados.Conteo(); i++) { 
                        var b = (Beneficiario?)resultados.Obtener(i); 
                        if (b != null) dgv.Rows.Add(b.Id, b.Nombre, b.MiembrosHogar); 
                    } 
                }
                else MessageBox.Show("No se encontraron coincidencias.");
            };
            gb.Controls.AddRange(new Control[] { txtBuscar, btnBuscar });

            dgv.Location = new Point(20, 170); dgv.Size = new Size(590, 280); dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect; dgv.ReadOnly = true; dgv.AllowUserToAddRows = false; dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgv.Columns.Add("Id", "ID"); dgv.Columns.Add("Nombre", "Nombre"); dgv.Columns.Add("M", "Miembros");
            dgv.SelectionChanged += (s, e) => {
                if (dgv.SelectedRows.Count > 0) {
                    seleccionado = new Beneficiario { 
                        Id = (int)dgv.SelectedRows[0].Cells[0].Value, 
                        Nombre = dgv.SelectedRows[0].Cells[1].Value?.ToString() ?? "", 
                        MiembrosHogar = (int)dgv.SelectedRows[0].Cells[2].Value, 
                        Activo = true 
                    };
                    txtNombre.Text = seleccionado.Nombre; numMiembros.Value = seleccionado.MiembrosHogar; btnEditar.Enabled = btnEliminar.Enabled = true;
                }
            };

            Button btnImp = new Button { Text = "Importar CSV", Location = new Point(130, 460), Size = new Size(100, 30) };
            btnImp.Click += (s, e) => {
                OpenFileDialog ofd = new OpenFileDialog();
                if (ofd.ShowDialog() == DialogResult.OK) {
                    var l = ManejadorCSV.ParsearBeneficiarios(ofd.FileName);
                    int omitidos = 0;
                    for (int i = 0; i < l.Conteo(); i++) {
                        var b = (Beneficiario?)l.Obtener(i);
                        if (b == null) continue;

                        string nombreImportado = b.Nombre.Trim();
                        if (string.IsNullOrWhiteSpace(nombreImportado)) { omitidos++; continue; }
                        if (ExisteNombreBeneficiario(nombreImportado, -1)) { omitidos++; continue; }

                        b.Nombre = nombreImportado;
                        servicio.Guardar(b);
                    }
                    undoStack.Empujar(new AccionUndo(TipoAccion.Importacion, "Beneficiarios", l));
                    Cargar();
                    if (omitidos > 0) MessageBox.Show("Se omitieron " + omitidos + " registros por nombre duplicado o vacio.", "Importacion con validacion", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            };

            Button btnPlantilla = new Button { Text = "Bajar Plantilla", Location = new Point(240, 460), Size = new Size(110, 30) };
            btnPlantilla.Click += (s, e) => ManejadorCSV.GuardarPlantillaConDialogo("plantilla_beneficiarios.csv", "Nombre,Miembros\nJuan Perez,5\nMaria Lopez,3");

            this.Controls.AddRange(new Control[] { btnNuevo, l1, txtNombre, l2, numMiembros, btnGuardar, btnEditar, btnEliminar, gb, dgv, btnUndo, btnImp, btnPlantilla });
            AplicarEscaladoDpi();
        }

        private void AplicarEscaladoDpi()
        {
            float factor = DeviceDpi / 96f;
            if (factor <= 1f) return;

            this.Size = new Size((int)Math.Round(this.Width * factor), (int)Math.Round(this.Height * factor));
            EscaladorDpi.EscalarJerarquia(this, factor);
        }

        private void Limpiar() { txtNombre.Clear(); numMiembros.Value = 1; seleccionado = null; btnEditar.Enabled = btnEliminar.Enabled = false; dgv.ClearSelection(); }
        private void Cargar() { dgv.Rows.Clear(); var l = servicio.ListarTodos(); for (int i = 0; i < l.Conteo(); i++) { var b = (Beneficiario?)l.Obtener(i); if (b != null) dgv.Rows.Add(b.Id, b.Nombre, b.MiembrosHogar); } }

        private bool ExisteNombreBeneficiario(string nombre, int idExcluir)
        {
            TablaHash indiceNombres = new TablaHash(211);
            ListaEnlazada lista = servicio.ListarTodos();

            for (int i = 0; i < lista.Conteo(); i++)
            {
                Beneficiario? b = (Beneficiario?)lista.Obtener(i);
                if (b == null) continue;
                if (b.Id == idExcluir) continue;

                string clave = NormalizarNombre(b.Nombre);
                indiceNombres.Insertar(clave, b.Id);
            }

            return indiceNombres.Buscar(NormalizarNombre(nombre)) != null;
        }

        private string NormalizarNombre(string nombre)
        {
            return nombre.Trim().ToLower();
        }
    }
}
