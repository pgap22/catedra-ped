namespace ProyectoCatedra.Modelos
{
    public enum TipoAccion { Insertar, Editar, Eliminar, Importacion }

    public class AccionUndo
    {
        public TipoAccion Tipo { get; set; }
        public string Tabla { get; set; }
        public object Datos { get; set; } 

        public AccionUndo(TipoAccion tipo, string tabla, object datos)
        {
            Tipo = tipo;
            Tabla = tabla;
            Datos = datos;
        }
    }
}
