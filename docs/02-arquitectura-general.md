# Arquitectura general

## Vista por capas

El proyecto usa una arquitectura simple por responsabilidades. No usa inyeccion de dependencias ni repositorios abstractos. Para un proyecto WinForms academico, la separacion principal es suficiente y facil de defender.

```text
UI WinForms
  |
  v
Servicios
  |
  +--> Modelos
  +--> Estructuras propias
  +--> Utilidades
  |
  v
SQLite local: donaciones.db
```

## Arbol logico del proyecto

```text
ProyectoCatedra/
  Program.cs
  Form1.cs
  Form*.cs
  ProyectoCatedra.csproj
  ProyectoCatedra.sln
  PRD.md
  demo_explicacion_sistema.md
  catedra_revision.md

  Datos/
    ConexionDB.cs

  Modelos/
    AccionUndo.cs
    Beneficiario.cs
    Categoria.cs
    CategoriaPackDetalle.cs
    Orden.cs
    OrdenDetalle.cs
    Producto.cs
    TasaConsumo.cs
    UnidadMedida.cs

  Servicios/
    BeneficiarioServicio.cs
    CategoriaPackServicio.cs
    CategoriaServicio.cs
    DistribucionServicio.cs
    HistorialServicio.cs
    ProductoServicio.cs
    TasaConsumoServicio.cs
    UnidadServicio.cs

  Estructuras/
    ArbolBST.cs
    ElementoHeap.cs
    ListaEnlazada.cs
    MonticuloMaximo.cs
    NodoArbol.cs
    NodoHash.cs
    NodoLista.cs
    NodoPila.cs
    Pila.cs
    TablaHash.cs

  Utilidades/
    EscaladorDpi.cs
    GeneradorDatos.cs
    ManejadorCSV.cs
    RelojDemo.cs
```

## Responsabilidad de cada carpeta

| Carpeta | Responsabilidad | Ejemplo |
|---|---|---|
| Raiz | Formularios WinForms y entrada del programa. | `FormDistribucion.cs` |
| `Datos` | Conexion, creacion y actualizacion del esquema SQLite. | `ConexionDB.cs` |
| `Modelos` | Clases que representan datos del dominio. | `Producto.cs` |
| `Servicios` | Consultas SQLite y reglas de negocio. | `DistribucionServicio.cs` |
| `Estructuras` | Estructuras de datos implementadas manualmente. | `MonticuloMaximo.cs` |
| `Utilidades` | Helpers transversales: CSV, fecha demo, datos demo, DPI. | `ManejadorCSV.cs` |

## Flujo de arranque

```text
Windows ejecuta la app
  |
  v
Program.Main()
  |
  +--> ApplicationConfiguration.Initialize()
  |
  +--> Application.Run(new Form1())
            |
            v
       Form1 muestra menu principal
```

`Form1` no crea todos los formularios al inicio. Los abre cuando el usuario hace clic en un menu.

Ejemplo real:

```csharp
itemProductos.Click += (s, e) => { var f = new FormProductos(); f.Show(); };
```

## Flujo UI a base de datos

La mayoria de pantallas siguen este patron:

```text
Usuario llena campos
  |
  v
Click en boton
  |
  v
Formulario valida entrada
  |
  v
Servicio ejecuta SQL
  |
  v
SQLite guarda o consulta
  |
  v
Formulario recarga DataGridView
```

Ejemplo con productos:

```text
FormProductos
  |
  +--> valida nombre y categoria
  |
  +--> ProductoServicio.GuardarOSumarStock(producto)
          |
          +--> INSERT o UPDATE en Productos
          |
          +--> actualiza TablaHash indicePorSku
```

## Dependencias internas principales

```text
FormDistribucion
  |
  +--> DistribucionServicio
  +--> CategoriaServicio
  +--> Pila
  +--> TablaHash
  +--> ListaEnlazada

DistribucionServicio
  |
  +--> ConexionDB
  +--> BeneficiarioServicio
  +--> ProductoServicio
  +--> TasaConsumoServicio
  +--> CategoriaPackServicio
  +--> HistorialServicio
  +--> MonticuloMaximo
```

## Arquitectura de distribucion

El algoritmo es el flujo mas importante del sistema.

```text
Beneficiarios activos
Tasas de consumo
Packs por categoria
Historial de entregas
Stock de productos
       |
       v
DistribucionServicio.GenerarPropuestaDistribucion()
       |
       +--> calcula deficit
       +--> calcula prioridad
       +--> inserta en MonticuloMaximo
       +--> extrae mayor prioridad
       +--> divide por pack porcentual
       +--> produce OrdenDetalle
       |
       v
ListaEnlazada de OrdenDetalle
       |
       v
FormDistribucion muestra propuesta
```

## Decisiones arquitectonicas observadas

| Decision | Donde se ve | Consecuencia |
|---|---|---|
| SQLite directo con `System.Data.SQLite` | `Servicios/*.cs`, `Datos/ConexionDB.cs` | Simple y offline, pero SQL queda dentro de servicios. |
| Servicios concretos sin interfaces | `DistribucionServicio` crea sus dependencias | Facil para proyecto academico, menos flexible para tests. |
| Formularios construidos en codigo | `Form*.cs` | Todo el layout esta en C#, no en diseñador visual. |
| Estructuras propias no genericas | `Estructuras/*.cs` | Cumple objetivo academico de estructuras de datos. |
| Fecha demo global | `RelojDemo.cs` | Facil simular dias para deficit sin cambiar reloj del sistema. |
| Migraciones simples con `ALTER TABLE` en `try/catch` | `ConexionDB.ActualizarEsquema()` | Practico, pero puede ocultar errores reales. |

## Flujo entre pantallas

```text
Form1
  |
  +--> FormCategorias
  +--> FormUnidades
  +--> FormProductos
  +--> FormConfigurarPacks
  +--> FormBeneficiarios
  +--> FormTasaConsumo
  +--> FormDistribucion
  |       |
  |       +--> FormConfirmacionDistribucion
  |
  +--> FormHistorial
  +--> FormDevFecha
```

## Que significa que una pantalla sea independiente

Cada formulario crea sus propios servicios. Por ejemplo, `FormProductos` crea `ProductoServicio` y `CategoriaServicio` en su constructor.

Esto hace que cada pantalla pueda abrirse sola desde el menu, pero tambien significa que algunas pantallas pueden tener datos en memoria cargados al momento de abrirse. Si dos pantallas quedan abiertas a la vez y una cambia datos, la otra podria necesitar recargarse para ver el cambio.

## Uso de colecciones del framework

El proyecto usa estructuras propias para cumplir el nucleo academico. Tambien usa algunas colecciones de C# donde son auxiliares de UI, por ejemplo `Dictionary<int, List<OrdenDetalle>>` y `HashSet<int>` en `FormDistribucion` para agrupar familias y recordar cuales estan colapsadas.

Esto no reemplaza el algoritmo principal. El ordenamiento por prioridad se hace con `MonticuloMaximo` propio en `DistribucionServicio`.
