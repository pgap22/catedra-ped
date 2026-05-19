# Resumen ejecutivo

## Que es el sistema

El proyecto es una aplicacion de escritorio Windows para gestionar donaciones e inventario de ayuda social. Permite registrar productos, familias beneficiarias, reglas de consumo, packs porcentuales por categoria y entregas confirmadas.

La funcionalidad central es generar una propuesta automatica de distribucion. El sistema calcula que familia tiene mas necesidad por categoria y sugiere productos concretos a entregar, descontando stock cuando la entrega se confirma.

## Problema que resuelve

Una organizacion social puede tener poco inventario y muchas familias con necesidades distintas. Si se reparte manualmente, suelen aparecer problemas:

- Familias con mayor necesidad pueden quedar al final.
- Familias que ya recibieron ayuda recientemente pueden volver a recibir demasiado pronto.
- El inventario puede quedar desactualizado.
- Nadie puede explicar por que una familia recibio antes que otra.
- El historial se vuelve dificil de auditar.

Este sistema busca que la decision sea repetible, auditable y basada en reglas visibles.

## Usuarios principales

| Usuario | Que hace |
|---|---|
| Secretaria o encargado administrativo | Registra categorias, unidades, productos y beneficiarios. |
| Encargado de inventario | Actualiza stock, importa productos y revisa stock bajo. |
| Encargado de entregas | Genera propuesta, ajusta cantidades y confirma entregas. |
| Auditor o docente revisor | Consulta historial, revisa trazabilidad y valida estructuras de datos. |

## Capacidades funcionales reales

| Capacidad | Evidencia en codigo |
|---|---|
| Gestion de categorias | `FormCategorias.cs`, `Servicios/CategoriaServicio.cs` |
| Gestion de unidades | `FormUnidades.cs`, `Servicios/UnidadServicio.cs` |
| Asociacion categoria-unidad | `FormUnidades.cs`, tabla `CategoriaUnidades` |
| Gestion de productos | `FormProductos.cs`, `Servicios/ProductoServicio.cs` |
| Generacion de SKU | `FormProductos.ObtenerSiguienteSKU()` usa `MonticuloMaximo` |
| Gestion de beneficiarios | `FormBeneficiarios.cs`, `Servicios/BeneficiarioServicio.cs` |
| Niveles de vulnerabilidad | `Modelos/Beneficiario.cs` |
| Tasas de consumo | `FormTasaConsumo.cs`, `Servicios/TasaConsumoServicio.cs` |
| Packs porcentuales | `FormConfigurarPacks.cs`, `Servicios/CategoriaPackServicio.cs` |
| Distribucion automatica | `FormDistribucion.cs`, `Servicios/DistribucionServicio.cs` |
| Confirmacion y descuento de stock | `DistribucionServicio.ConfirmarDistribucion()` |
| Historial | `FormHistorial.cs`, `Servicios/HistorialServicio.cs` |
| Importacion CSV | `Utilidades/ManejadorCSV.cs` |
| Datos demo | `Utilidades/GeneradorDatos.cs` |
| Fecha simulada | `FormDevFecha.cs`, `Utilidades/RelojDemo.cs` |

## Stack tecnico detectado

| Elemento | Valor |
|---|---|
| Lenguaje | C# |
| UI | Windows Forms |
| Framework | `.NET 10.0 Windows` |
| Persistencia | SQLite local |
| Paquete NuGet | `System.Data.SQLite.Core` version `1.0.119` |
| Proyecto | `ProyectoCatedra.csproj` |
| Solucion | `ProyectoCatedra.sln` |

Configuracion real del proyecto:

```xml
<OutputType>WinExe</OutputType>
<TargetFramework>net10.0-windows</TargetFramework>
<Nullable>enable</Nullable>
<UseWindowsForms>true</UseWindowsForms>
<ImplicitUsings>enable</ImplicitUsings>
<PackageReference Include="System.Data.SQLite.Core" Version="1.0.119" />
```

## Punto de entrada

Ruta: `Program.cs`

```csharp
ApplicationConfiguration.Initialize();
Application.Run(new Form1());
```

Explicacion para junior:

1. `ApplicationConfiguration.Initialize()` prepara configuraciones basicas de WinForms.
2. `new Form1()` crea la ventana principal.
3. `Application.Run(...)` mantiene viva la aplicacion hasta que el usuario la cierre.

## Pantalla principal

Ruta: `Form1.cs`

`Form1` crea el menu superior y abre el resto de pantallas. No contiene el algoritmo de distribucion. Su responsabilidad principal es navegar.

Menu real:

```text
Configuracion
  - Tasas de Consumo (Meta)

Inventario
  - Gestionar Categorias
  - Gestionar Unidades
  - Gestionar Productos
  - Configurar Packs por Categoria

Beneficiarios
  - Padron de Familias

Ayuda Social
  - Generar Asignacion (Reparto)
  - Historial de Entregas Realizadas

Dev / Demo
  - Simular Fecha
  - Sembrar Demo Pequena
  - Sembrar Datos de Prueba (150+)

Sistema
  - Salir del Sistema
```

## Idea central de negocio

El sistema no reparte directamente por producto al inicio. Primero piensa por categoria.

Ejemplo:

```text
Categoria: Granos Basicos
Productos dentro del pack:
  - Arroz Blanco 60%
  - Frijol Rojo 40%
```

El deficit se calcula por categoria. Luego el pack convierte esa necesidad de categoria en productos fisicos concretos.

```text
Necesidad por categoria
       |
       v
Pack porcentual
       |
       +--> Producto A
       +--> Producto B
```

## Formula base del deficit

La formula real esta en `Servicios/DistribucionServicio.cs`, metodo privado `CalcularDeficit`.

```text
deficit = miembros del hogar * tasa diaria * dias considerados
```

Donde:

| Parte | Significado |
|---|---|
| `miembros del hogar` | Cuantas personas viven en la familia. |
| `tasa diaria` | Cuanto consume una persona por dia de una categoria. |
| `dias considerados` | Dias desde la ultima entrega o dias iniciales por vulnerabilidad. |

## Que hace diferente a este sistema

El proyecto tiene valor academico porque integra estructuras de datos propias en flujos reales:

| Estructura | Uso real |
|---|---|
| `ListaEnlazada` | Colecciones devueltas por servicios, propuestas e importaciones. |
| `Pila` | Deshacer cambios manuales. |
| `TablaHash` | Busqueda por SKU, duplicados, indices y stock por producto. |
| `ArbolBST` | Beneficiarios ordenados y busqueda parcial por nombre. |
| `MonticuloMaximo` | Priorizar familias en distribucion y generar siguiente SKU. |

## Relacion entre configuracion y distribucion

Para que la distribucion automatica genere filas, deben existir datos previos:

```text
Categorias
  + Unidades permitidas
  + Productos con stock
  + Tasa de consumo
  + Pack porcentual valido al 100%
  + Beneficiarios activos
  + Historial opcional
       |
       v
Propuesta automatica
```

Si falta una pieza, `DistribucionServicio.ObtenerDiagnosticoSinPropuesta()` devuelve un mensaje explicando el motivo principal.
