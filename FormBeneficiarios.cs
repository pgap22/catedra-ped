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
        private TextBox txtNombre, txtBuscar;
        private NumericUpDown numMiembros;
        private DataGridView dgv;
        private Button btnGuardar, btnEditar, btnEliminar, btnUndo;
        private Beneficiario seleccionado;

        public FormBeneficiarios() { 
            servicio = new BeneficiarioServicio(); 
            undoStack = new Pila(); 
            InicializarComponentes(); 
            Cargar(); 
        }

        private void InicializarComponentes()
        {
            this.Text = "Padrón de Beneficiarios";
            this.Size = new Size(650, 550);
            this.StartPosition = FormStartPosition.CenterParent;

            Label l1 = new Label { Text = "Nombre:", Location = new Point(20, 20), AutoSize = true };
            txtNombre = new TextBox { Location = new Point(20, 40), Size = new Size(180, 20) };
            Label l2 = new Label { Text = "Miembros:", Location = new Point(210, 20), AutoSize = true };
            numMiembros = new NumericUpDown { Location = new Point(210, 40), Size = new Size(60, 20), Minimum = 1 };

            btnGuardar = new Button { Text = "Registrar", Location = new Point(280, 38), Size = new Size(80, 25) };
            btnGuardar.Click += (s, e) => { 
                var b = new Beneficiario { Nombre = txtNombre.Text, MiembrosHogar = (int)numMiembros.Value, Activo = true };
                servicio.Guardar(b); 
                undoStack.Empujar(new AccionUndo(TipoAccion.Insertar, "Beneficiarios", b));
                Cargar(); Limpiar(); 
            };

            btnEditar = new Button { Text = "Modificar", Location = new Point(370, 38), Size = new Size(80, 25), Enabled = false };
            btnEditar.Click += (s, e) => { 
                var ant = new Beneficiario { Id = seleccionado.Id, Nombre = seleccionado.Nombre, MiembrosHogar = seleccionado.MiembrosHogar, Activo = true };
                seleccionado.Nombre = txtNombre.Text; seleccionado.MiembrosHogar = (int)numMiembros.Value; 
                servicio.Actualizar(seleccionado); 
                undoStack.Empujar(new AccionUndo(TipoAccion.Editar, "Beneficiarios", ant));
                Cargar(); Limpiar(); 
            };

            btnEliminar = new Button { Text = "Eliminar", Location = new Point(460, 38), Size = new Size(80, 25), Enabled = false };
            btnEliminar.Click += (s, e) => { 
                undoStack.Empujar(new AccionUndo(TipoAccion.Eliminar, "Beneficiarios", seleccionado));
                servicio.Eliminar(seleccionado.Id); 
                Cargar(); Limpiar(); 
            };

            btnUndo = new Button { Text = "Deshacer", Location = new Point(20, 460), Size = new Size(100, 30) };
            btnUndo.Click += (s, e) => {
                var acc = (AccionUndo)undoStack.Pop();
                if (acc == null) return;
                var b = (Beneficiario)acc.Datos;
                if (acc.Tipo == TipoAccion.Insertar) {
                    // Borrar el ultimo insertado por nombre (o id si lo tuvieramos)
                    var lista = servicio.ListarTodos();
                    for(int i=0; i<lista.Conteo(); i++) {
                        var temp = (Beneficiario)lista.Obtener(i);
                        if(temp.Nombre == b.Nombre) { servicio.Eliminar(temp.Id); break; }
                    }
                } else if (acc.Tipo == TipoAccion.Editar) servicio.Actualizar(b);
                else if (acc.Tipo == TipoAccion.Eliminar) servicio.Guardar(b);
                else if (acc.Tipo == TipoAccion.Importacion) {
                    var lImp = (ListaEnlazada)acc.Datos;
                    for(int i=0; i<lImp.Conteo(); i++) {
                        var bImp = (Beneficiario)lImp.Obtener(i);
                        var lista = servicio.ListarTodos();
                        for(int j=0; j<lista.Conteo(); j++) {
                            var temp = (Beneficiario)lista.Obtener(j);
                            if(temp.Nombre == bImp.Nombre) { servicio.Eliminar(temp.Id); break; }
                        }
                    }
                }
                Cargar();
            };

            GroupBox gb = new GroupBox { Text = "Búsqueda (BST Multi-Resultado)", Location = new Point(20, 80), Size = new Size(590, 60) };
            txtBuscar = new TextBox { Location = new Point(15, 25), Size = new Size(350, 20) };
            btnBuscar = new Button { Text = "Buscar", Location = new Point(380, 23), Size = new Size(140, 25) };
            btnBuscar.Click += (s, e) => {
                var resultados = servicio.CargarEnArbol().Buscar(txtBuscar.Text);
                dgv.Rows.Clear();
                if (resultados != null) { for (int i = 0; i < resultados.Conteo(); i++) { var b = (Beneficiario)resultados.Obtener(i); dgv.Rows.Add(b.Id, b.Nombre, b.MiembrosHogar); } }
                else MessageBox.Show("No se encontraron coincidencias.");
            };
            gb.Controls.AddRange(new Control[] { txtBuscar, btnBuscar });

            dgv = new DataGridView { Location = new Point(20, 160), Size = new Size(590, 280), SelectionMode = DataGridViewSelectionMode.FullRowSelect, ReadOnly = true, AllowUserToAddRows = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill };
            dgv.Columns.Add("Id", "ID"); dgv.Columns.Add("Nombre", "Nombre"); dgv.Columns.Add("M", "Miembros");
            dgv.SelectionChanged += (s, e) => {
                if (dgv.SelectedRows.Count > 0) {
                    seleccionado = new Beneficiario { Id = (int)dgv.SelectedRows[0].Cells[0].Value, Nombre = dgv.SelectedRows[0].Cells[1].Value.ToString(), MiembrosHogar = (int)dgv.SelectedRows[0].Cells[2].Value, Activo = true };
                    txtNombre.Text = seleccionado.Nombre; numMiembros.Value = seleccionado.MiembrosHogar; btnEditar.Enabled = btnEliminar.Enabled = true;
                }
            };

            Button btnImp = new Button { Text = "Importar", Location = new Point(130, 460), Size = new Size(80, 30) };
            btnImp.Click += (s, e) => {
                OpenFileDialog ofd = new OpenFileDialog();
                if (ofd.ShowDialog() == DialogResult.OK) {
                    var l = ManejadorCSV.ParsearBeneficiarios(ofd.FileName);
                    for (int i = 0; i < l.Conteo(); i++) servicio.Guardar((Beneficiario)l.Obtener(i));
                    undoStack.Empujar(new AccionUndo(TipoAccion.Importacion, "Beneficiarios", l));
                    Cargar();
                }
            };

            this.Controls.AddRange(new Control[] { l1, txtNombre, l2, numMiembros, btnGuardar, btnEditar, btnEliminar, gb, dgv, btnUndo, btnImp });
        }

        private void Limpiar() { txtNombre.Clear(); numMiembros.Value = 1; seleccionado = null; btnEditar.Enabled = btnEliminar.Enabled = false; }
        private void Cargar() { dgv.Rows.Clear(); var l = servicio.ListarTodos(); for (int i = 0; i < l.Conteo(); i++) { var b = (Beneficiario)l.Obtener(i); dgv.Rows.Add(b.Id, b.Nombre, b.MiembrosHogar); } }
    }
}
