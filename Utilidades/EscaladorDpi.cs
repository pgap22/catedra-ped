using System;
using System.Drawing;
using System.Windows.Forms;

namespace ProyectoCatedra.Utilidades
{
    public static class EscaladorDpi
    {
        public static void EscalarJerarquia(Control raiz, float factor)
        {
            if (factor <= 1f) return;
            EscalarControlesInternos(raiz, factor);
        }

        private static void EscalarControlesInternos(Control padre, float factor)
        {
            foreach (Control control in padre.Controls)
            {
                if (control.Dock == DockStyle.None)
                {
                    control.Left = Escalar(control.Left, factor);
                    control.Top = Escalar(control.Top, factor);
                    control.Width = Escalar(control.Width, factor);
                    control.Height = Escalar(control.Height, factor);
                }

                control.Margin = new Padding(
                    Escalar(control.Margin.Left, factor),
                    Escalar(control.Margin.Top, factor),
                    Escalar(control.Margin.Right, factor),
                    Escalar(control.Margin.Bottom, factor));

                control.Padding = new Padding(
                    Escalar(control.Padding.Left, factor),
                    Escalar(control.Padding.Top, factor),
                    Escalar(control.Padding.Right, factor),
                    Escalar(control.Padding.Bottom, factor));

                if (control.Controls.Count > 0)
                {
                    EscalarControlesInternos(control, factor);
                }
            }
        }

        private static int Escalar(int valor, float factor)
        {
            return (int)Math.Round(valor * factor);
        }
    }
}
