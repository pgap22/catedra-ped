using System;
using System.Drawing;
using System.Windows.Forms;
using ProyectoCatedra.Servicios;
using ProyectoCatedra.Modelos;
using ProyectoCatedra.Estructuras;
using ProyectoCatedra.Utilidades;

namespace ProyectoCatedra
{
    public class FormProductos : Form
    {
        private ProductoServicio servicio;
        private CategoriaServicio catServicio;
        private Pila undoStack;
        private TextBox txtSKU, txtNombre;
        private NumericUpDown numStock;
        private ComboBox cbCat;
        private DataGridView dgv;
        private Button btnGuardar, btnEliminar, btnUndo, btnNuevo;

        public FormProductos() { 
            servicio = new ProductoServicio(); 
            catServicio = new CategoriaServicio(); 
            undoStack = new Pila(); 
            InicializarComponentes(); 
            CargarCats(); Cargar(); 
        }

        private void InicializarComponentes()
        {
            this.Text = "Inventario de Productos";
            this.Size = new Size(800, 520);
            this.StartPosition = FormStartPosition.CenterParent;

            btnNuevo = new Button { Text = "Nuevo (Auto-SKU)", Location = new Point(20, 15), Size = new Size(120, 25) };
            btnNuevo.Click += (s, e) => GenerarNuevoSKU();

            Label l1 = new Label { Text = "SKU:", Location = new Point(20, 45), AutoSize = true };
            txtSKU = new TextBox { Location = new Point(20, 65), Size = new Size(80, 20) };
            Label l2 = new Label { Text = "Nombre:", Location = new Point(110, 45), AutoSize = true };
            txtNombre = new TextBox { Location = new Point(110, 65), Size = new Size(150, 20) };
            Label l3 = new Label { Text = "Categoría:", Location = new Point(270, 45), AutoSize = true };
            cbCat = new ComboBox { Location = new Point(270, 65), Size = new Size(120, 20), DropDownStyle = ComboBoxStyle.DropDownList };
            Label l4 = new Label { Text = "Stock:", Location = new Point(400, 45), AutoSize = true };
            numStock = new NumericUpDown { Location = new Point(400, 65), Size = new Size(60, 20) };

            btnGuardar = new Button { Text = "Guardar", Location = new Point(470, 63), Size = new Size(80, 25) };
            btnGuardar.Click += (s, e) => {
                if (cbCat.SelectedItem == null || string.IsNullOrWhiteSpace(txtSKU.Text)) return;
                var p = new Producto { SKU = txtSKU.Text, Nombre = txtNombre.Text, IdCategoria = ((Categoria)cbCat.SelectedItem).Id, Stock = (double)numStock.Value };
                servicio.GuardarOSumarStock(p);
                undoStack.Empujar(new AccionUndo(TipoAccion.Insertar, "Productos", p));
                Cargar(); Limpiar();
            };

            btnEliminar = new Button { Text = "Eliminar", Location = new Point(560, 63), Size = new Size(80, 25), Enabled = false };
            btnEliminar.Click += (s, e) => {
                var p = (Producto)servicio.CargarEnHash().Buscar(txtSKU.Text);
                if (p != null) undoStack.Empujar(new AccionUndo(TipoAccion.Eliminar, "Productos", p));
                servicio.Eliminar(txtSKU.Text);
                Cargar(); Limpiar();
            };

            btnUndo = new Button { Text = "Deshacer", Location = new Point(20, 430), Size = new Size(100, 30) };
            btnUndo.Click += Revertir;

            dgv = new DataGridView { Location = new Point(20, 100), Size = new Size(740, 310), SelectionMode = DataGridViewSelectionMode.FullRowSelect, ReadOnly = true, AllowUserToAddRows = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill };
            dgv.Columns.Add("SKU", "SKU"); dgv.Columns.Add("Nom", "Nombre"); dgv.Columns.Add("Cat", "Categoría"); dgv.Columns.Add("Stock", "Stock");
            dgv.SelectionChanged += (s, e) => { if (dgv.SelectedRows.Count > 0) { txtSKU.Text = dgv.SelectedRows[0].Cells[0].Value.ToString(); txtNombre.Text = dgv.SelectedRows[0].Cells[1].Value.ToString(); numStock.Value = Convert.ToDecimal(dgv.SelectedRows[0].Cells[3].Value); btnEliminar.Enabled = true; } };

            Button btnImp = new Button { Text = "Importar", Location = new Point(130, 430), Size = new Size(100, 30) };
            btnImp.Click += Importar;

            this.Controls.AddRange(new Control[] { btnNuevo, l1, txtSKU, l2, txtNombre, l3, cbCat, l4, numStock, btnGuardar, btnEliminar, btnUndo, dgv, btnImp });
        }

        private void GenerarNuevoSKU()
        {
            Limpiar();
            var l = servicio.ListarTodos();
            if (l.Conteo() == 0) { txtSKU.Text = "P001"; return; }

            // Usamos MonticuloMaximo para encontrar el SKU mas alto
            MonticuloMaximo heap = new MonticuloMaximo();
            for(int i=0; i<l.Conteo(); i++) {
                var p = (Producto)l.Obtener(i);
                // Extraemos numero de "P001" -> 1
                if (p.SKU.Length > 1 && int.TryParse(p.SKU.Substring(1), out int num)) heap.Insertar(num, num);
            }
            int max = (int)(heap.VerMaximo() ?? 0);
            txtSKU.Text = "P" + (max + 1).ToString("D3");
        }

        private void Revertir(object sender, EventArgs e)
        {
            var acc = (AccionUndo)undoStack.Pop();
            if (acc == null) return;
            if (acc.Tipo == TipoAccion.Insertar) servicio.Eliminar(((Producto)acc.Datos).SKU);
            else if (acc.Tipo == TipoAccion.Eliminar) servicio.GuardarOSumarStock((Producto)acc.Datos);
            else if (acc.Tipo == TipoAccion.Importacion) {
                var l = (ListaEnlazada)acc.Datos;
                for(int i=0; i<l.Conteo(); i++) servicio.Eliminar(((Producto)l.Obtener(i)).SKU);
            }
            Cargar();
        }

        private void Importar(object sender, EventArgs e)
        {
            try {
                OpenFileDialog ofd = new OpenFileDialog();
                if (ofd.ShowDialog() == DialogResult.OK) {
                    var l = ManejadorCSV.ParsearProductos(ofd.FileName);
                    ListaEnlazada importados = new ListaEnlazada();
                    for (int i = 0; i < l.Conteo(); i++) {
                        var p = (Producto)l.Obtener(i);
                        int cid = catServicio.ObtenerIdPorNombre(p.NombreCategoria);
                        if (cid != -1) { p.IdCategoria = cid; servicio.GuardarOSumarStock(p); importados.Agregar(p); }
                    }
                    undoStack.Empujar(new AccionUndo(TipoAccion.Importacion, "Productos", importados));
                    Cargar();
                }
            } catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); }
        }

        private void CargarCats() { cbCat.Items.Clear(); var l = catServicio.ListarTodas(); for (int i = 0; i < l.Conteo(); i++) cbCat.Items.Add(l.Obtener(i)); }
        private void Cargar() { dgv.Rows.Clear(); var l = servicio.ListarTodos(); for (int i = 0; i < l.Conteo(); i++) { var p = (Producto)l.Obtener(i); dgv.Rows.Add(p.SKU, p.Nombre, p.NombreCategoria, p.Stock); } }
        private void Limpiar() { txtSKU.Clear(); txtNombre.Clear(); numStock.Value = 0; btnEliminar.Enabled = false; }
    }
}
