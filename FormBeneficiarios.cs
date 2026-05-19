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
        private ComboBox cmbVulnerabilidad = new ComboBox();
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
            this.Size = new Size(800, 550);
            this.MinimumSize = new Size(780, 500);
            this.StartPosition = FormStartPosition.CenterParent;

            btnNuevo.Text = "Nuevo"; btnNuevo.Location = new Point(20, 15); btnNuevo.Size = new Size(60, 25);
            btnNuevo.Click += (s, e) => Limpiar();

            Label l1 = new Label { Text = "Nombre:", Location = new Point(20, 45), AutoSize = true };
            txtNombre.Location = new Point(20, 65); txtNombre.Size = new Size(180, 20);

            Label l2 = new Label { Text = "Miembros:", Location = new Point(210, 45), AutoSize = true };
            numMiembros.Location = new Point(210, 65); numMiembros.Size = new Size(60, 20); numMiembros.Minimum = 1; numMiembros.Maximum = 999;

            Label l3 = new Label { Text = "Nivel de vulnerabilidad:", Location = new Point(280, 45), AutoSize = true };
            cmbVulnerabilidad.Location = new Point(280, 65); cmbVulnerabilidad.Size = new Size(190, 23); cmbVulnerabilidad.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbVulnerabilidad.Items.Add(Beneficiario.ObtenerEtiquetaVulnerabilidad(Beneficiario.VulnerabilidadAlta));
            cmbVulnerabilidad.Items.Add(Beneficiario.ObtenerEtiquetaVulnerabilidad(Beneficiario.VulnerabilidadMedia));
            cmbVulnerabilidad.Items.Add(Beneficiario.ObtenerEtiquetaVulnerabilidad(Beneficiario.VulnerabilidadBaja));
            SeleccionarNivelVulnerabilidad(Beneficiario.VulnerabilidadMedia);

            ToolTip ayudaVulnerabilidad = new ToolTip();
            ayudaVulnerabilidad.SetToolTip(cmbVulnerabilidad, "Este nivel ayuda a priorizar familias cuando el stock no alcanza para todos.");

            btnGuardar.Text = "Registrar"; btnGuardar.Location = new Point(480, 63); btnGuardar.Size = new Size(80, 25);
            btnGuardar.Click += (s, e) => { 
                string nombreNuevo = txtNombre.Text.Trim();
                if (string.IsNullOrWhiteSpace(nombreNuevo))
                {
                    MessageBox.Show("Ingrese el nombre del beneficiario.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                if (ExisteNombreBeneficiario(nombreNuevo, -1)) {
                    MessageBox.Show("Ya existe un beneficiario con ese nombre.", "Nombre duplicado", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                try
                {
                    var b = new Beneficiario { Nombre = nombreNuevo, MiembrosHogar = (int)numMiembros.Value, NivelVulnerabilidad = ObtenerNivelVulnerabilidadSeleccionado(), Activo = true };
                    servicio.Guardar(b);
                    undoStack.Empujar(new AccionUndo(TipoAccion.Insertar, "Beneficiarios", b));
                    Limpiar(); Cargar();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("No se pudo registrar el beneficiario: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

            btnEditar.Text = "Modificar"; btnEditar.Location = new Point(570, 63); btnEditar.Size = new Size(80, 25); btnEditar.Enabled = false;
            btnEditar.Click += (s, e) => { 
                if (seleccionado == null) return;

                string nombreNuevo = txtNombre.Text.Trim();
                if (string.IsNullOrWhiteSpace(nombreNuevo))
                {
                    MessageBox.Show("Ingrese el nombre del beneficiario.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                if (ExisteNombreBeneficiario(nombreNuevo, seleccionado.Id)) {
                    MessageBox.Show("Ya existe otro beneficiario con ese nombre.", "Nombre duplicado", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                try
                {
                    var ant = new Beneficiario { Id = seleccionado.Id, Nombre = seleccionado.Nombre, MiembrosHogar = seleccionado.MiembrosHogar, NivelVulnerabilidad = seleccionado.NivelVulnerabilidad, Activo = true };
                    seleccionado.Nombre = nombreNuevo; seleccionado.MiembrosHogar = (int)numMiembros.Value; seleccionado.NivelVulnerabilidad = ObtenerNivelVulnerabilidadSeleccionado();
                    servicio.Actualizar(seleccionado);
                    undoStack.Empujar(new AccionUndo(TipoAccion.Editar, "Beneficiarios", ant));
                    Limpiar(); Cargar();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("No se pudo modificar el beneficiario: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

            btnEliminar.Text = "Eliminar"; btnEliminar.Location = new Point(660, 63); btnEliminar.Size = new Size(80, 25); btnEliminar.Enabled = false;
            btnEliminar.Click += (s, e) => { 
                if (seleccionado == null) return;
                if (MessageBox.Show("¿Eliminar este beneficiario?", "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;

                try
                {
                    undoStack.Empujar(new AccionUndo(TipoAccion.Eliminar, "Beneficiarios", seleccionado));
                    servicio.Eliminar(seleccionado.Id);
                    Limpiar(); Cargar();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("No se pudo eliminar el beneficiario: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

            btnUndo.Text = "Deshacer último cambio manual"; btnUndo.Location = new Point(20, 460); btnUndo.Size = new Size(180, 30);
            btnUndo.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnUndo.Click += (s, e) => {
                var acc = (AccionUndo?)undoStack.Pop();
                if (acc == null)
                {
                    MessageBox.Show("No hay cambios para deshacer.", "Deshacer", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                try
                {
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

                    Limpiar();
                    Cargar();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("No se pudo deshacer el cambio: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

            GroupBox gb = new GroupBox { Text = "Buscar Beneficiario (por Nombre)", Location = new Point(20, 100), Size = new Size(740, 60) };
            txtBuscar.Location = new Point(15, 25); txtBuscar.Size = new Size(350, 20);
            btnBuscar.Text = "Buscar"; btnBuscar.Location = new Point(380, 23); btnBuscar.Size = new Size(140, 25);
            btnBuscar.Click += (s, e) => {
                var resultados = servicio.CargarEnArbol().BuscarParcial(txtBuscar.Text);
                dgv.Rows.Clear();
                if (resultados.Conteo() > 0) { 
                    for (int i = 0; i < resultados.Conteo(); i++) { 
                        var b = (Beneficiario?)resultados.Obtener(i); 
                        if (b != null) AgregarFilaBeneficiario(b); 
                    } 
                }
                else MessageBox.Show("No se encontraron coincidencias.");
            };
            gb.Controls.AddRange(new Control[] { txtBuscar, btnBuscar });

            dgv.Location = new Point(20, 170); dgv.Size = new Size(740, 280); dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect; dgv.ReadOnly = true; dgv.AllowUserToAddRows = false; dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgv.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dgv.Columns.Add("Id", "ID"); dgv.Columns.Add("Nombre", "Nombre"); dgv.Columns.Add("M", "Miembros"); dgv.Columns.Add("Vulnerabilidad", "Vulnerabilidad"); dgv.Columns.Add("NivelValor", "NivelValor");
            dgv.Columns["NivelValor"]!.Visible = false;
            dgv.SelectionChanged += (s, e) => {
                if (dgv.SelectedRows.Count > 0) {
                    DataGridViewRow row = dgv.SelectedRows[0];
                    seleccionado = new Beneficiario { 
                        Id = Convert.ToInt32(row.Cells["Id"].Value ?? 0), 
                        Nombre = row.Cells["Nombre"].Value?.ToString() ?? "", 
                        MiembrosHogar = Convert.ToInt32(row.Cells["M"].Value ?? 1),
                        NivelVulnerabilidad = Convert.ToInt32(row.Cells["NivelValor"].Value),
                        Activo = true 
                    };
                    txtNombre.Text = seleccionado.Nombre; numMiembros.Value = seleccionado.MiembrosHogar; SeleccionarNivelVulnerabilidad(seleccionado.NivelVulnerabilidad); btnEditar.Enabled = btnEliminar.Enabled = true;
                }
            };

            Button btnImp = new Button { Text = "Importar CSV", Location = new Point(210, 460), Size = new Size(100, 30) };
            btnImp.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnImp.Click += (s, e) => {
                OpenFileDialog ofd = new OpenFileDialog();
                if (ofd.ShowDialog() == DialogResult.OK) {
                    try
                    {
                        var l = ManejadorCSV.ParsearBeneficiarios(ofd.FileName);
                        int totalCsv = ManejadorCSV.ContarFilasDatos(ofd.FileName);
                        int invalidas = totalCsv - l.Conteo();
                        int omitidos = 0;
                        ListaEnlazada insertados = new ListaEnlazada();
                        for (int i = 0; i < l.Conteo(); i++) {
                            var b = (Beneficiario?)l.Obtener(i);
                            if (b == null) continue;

                            string nombreImportado = b.Nombre.Trim();
                            if (string.IsNullOrWhiteSpace(nombreImportado)) { omitidos++; continue; }
                            if (ExisteNombreBeneficiario(nombreImportado, -1)) { omitidos++; continue; }

                            b.Nombre = nombreImportado;
                            servicio.Guardar(b);
                            insertados.Agregar(b);
                        }

                        if (insertados.Conteo() > 0)
                        {
                            undoStack.Empujar(new AccionUndo(TipoAccion.Importacion, "Beneficiarios", insertados));
                        }

                        Cargar();
                        MessageBox.Show($"Importación finalizada.\nInsertados: {insertados.Conteo()}\nDuplicados: {omitidos}\nInválidos o vacíos: {invalidas}", "Importar", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("No se pudo importar el CSV: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            };

            Button btnPlantilla = new Button { Text = "Bajar Plantilla", Location = new Point(320, 460), Size = new Size(110, 30) };
            btnPlantilla.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnPlantilla.Click += (s, e) => ManejadorCSV.GuardarPlantillaConDialogo("plantilla_beneficiarios.csv", "Nombre,Miembros,NivelVulnerabilidad\nJuan Perez,5,Media\nMaria Lopez,3,Alta\nFamilia Solis,2,Baja");

            this.Controls.AddRange(new Control[] { btnNuevo, l1, txtNombre, l2, numMiembros, l3, cmbVulnerabilidad, btnGuardar, btnEditar, btnEliminar, gb, dgv, btnUndo, btnImp, btnPlantilla });
            AplicarEscaladoDpi();
        }

        private void AplicarEscaladoDpi()
        {
            float factor = DeviceDpi / 96f;
            if (factor <= 1f) return;

            this.Size = new Size((int)Math.Round(this.Width * factor), (int)Math.Round(this.Height * factor));
            EscaladorDpi.EscalarJerarquia(this, factor);
        }

        private void Limpiar() { txtNombre.Clear(); numMiembros.Value = 1; SeleccionarNivelVulnerabilidad(Beneficiario.VulnerabilidadMedia); seleccionado = null; btnEditar.Enabled = btnEliminar.Enabled = false; dgv.ClearSelection(); }
        private void Cargar() { dgv.Rows.Clear(); var l = servicio.ListarTodos(); for (int i = 0; i < l.Conteo(); i++) { var b = (Beneficiario?)l.Obtener(i); if (b != null) AgregarFilaBeneficiario(b); } }

        private void AgregarFilaBeneficiario(Beneficiario b)
        {
            dgv.Rows.Add(b.Id, b.Nombre, b.MiembrosHogar, b.VulnerabilidadTexto, Beneficiario.NormalizarNivelVulnerabilidad(b.NivelVulnerabilidad));
        }

        private int ObtenerNivelVulnerabilidadSeleccionado()
        {
            return Beneficiario.ParsearNivelVulnerabilidad(cmbVulnerabilidad.SelectedItem?.ToString());
        }

        private void SeleccionarNivelVulnerabilidad(int nivel)
        {
            string etiqueta = Beneficiario.ObtenerEtiquetaVulnerabilidad(nivel);
            cmbVulnerabilidad.SelectedItem = etiqueta;
            if (cmbVulnerabilidad.SelectedIndex < 0) cmbVulnerabilidad.SelectedIndex = 1;
        }

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
