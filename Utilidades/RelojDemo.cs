using System;

namespace ProyectoCatedra.Utilidades
{
    public static class RelojDemo
    {
        private static DateTime? fechaSimulada;

        public static DateTime Ahora => fechaSimulada ?? DateTime.Now;
        public static bool EstaSimulado => fechaSimulada.HasValue;
        public static DateTime? FechaSimulada => fechaSimulada;

        public static void EstablecerFecha(DateTime fecha)
        {
            fechaSimulada = fecha;
        }

        public static void UsarFechaReal()
        {
            fechaSimulada = null;
        }
    }
}
