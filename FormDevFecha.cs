using System;
using System.Drawing;
using System.Windows.Forms;
using ProyectoCatedra.Utilidades;

namespace ProyectoCatedra
{
    public class FormDevFecha : Form
    {
        private DateTimePicker dtpFecha = null!;
        private Label lblEstado = null!;

        public FormDevFecha()
        {
            Text = "Dev / Demo - Fecha";
            Size = new Size(420, 220);
            MinimumSize = new Size(400, 210);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Color.White;

            InicializarControles();
            ActualizarEstado();
        }

        private void InicializarControles()
        {
            Label lblTitulo = new Label();
            lblTitulo.Text = "Fecha usada por la demo";
            lblTitulo.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            lblTitulo.Location = new Point(20, 20);
            lblTitulo.Size = new Size(360, 25);

            dtpFecha = new DateTimePicker();
            dtpFecha.Format = DateTimePickerFormat.Custom;
            dtpFecha.CustomFormat = "dd/MM/yyyy HH:mm";
            dtpFecha.Value = RelojDemo.Ahora;
            dtpFecha.Location = new Point(20, 60);
            dtpFecha.Size = new Size(180, 25);

            Button btnSimular = new Button();
            btnSimular.Text = "Aplicar simulada";
            btnSimular.Location = new Point(20, 100);
            btnSimular.Size = new Size(130, 32);
            btnSimular.Click += (s, e) =>
            {
                RelojDemo.EstablecerFecha(dtpFecha.Value);
                ActualizarEstado();
            };

            Button btnReal = new Button();
            btnReal.Text = "Usar fecha real";
            btnReal.Location = new Point(160, 100);
            btnReal.Size = new Size(130, 32);
            btnReal.Click += (s, e) =>
            {
                RelojDemo.UsarFechaReal();
                dtpFecha.Value = RelojDemo.Ahora;
                ActualizarEstado();
            };

            lblEstado = new Label();
            lblEstado.Location = new Point(20, 145);
            lblEstado.Size = new Size(360, 35);
            lblEstado.ForeColor = Color.FromArgb(70, 70, 70);

            Controls.Add(lblTitulo);
            Controls.Add(dtpFecha);
            Controls.Add(btnSimular);
            Controls.Add(btnReal);
            Controls.Add(lblEstado);
        }

        private void ActualizarEstado()
        {
            string modo = RelojDemo.EstaSimulado ? "simulada" : "real";
            lblEstado.Text = $"Modo actual: fecha {modo}. Ahora = {RelojDemo.Ahora:dd/MM/yyyy HH:mm}";
        }
    }
}
