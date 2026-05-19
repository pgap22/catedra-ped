# Servicios

Los servicios conectan la UI con SQLite y concentran reglas de negocio. En este proyecto son clases concretas, creadas directamente por los formularios.

## Mapa general

| Servicio | Archivo | Responsabilidad principal |
|---|---|---|
| `CategoriaServicio` | `Servicios/CategoriaServicio.cs` | CRUD de categorias. |
| `UnidadServicio` | `Servicios/UnidadServicio.cs` | CRUD de unidades y asociacion categoria-unidad. |
| `ProductoServicio` | `Servicios/ProductoServicio.cs` | Inventario, stock, busqueda por SKU. |
| `BeneficiarioServicio` | `Servicios/BeneficiarioServicio.cs` | Padrón de familias e indices en memoria. |
| `TasaConsumoServicio` | `Servicios/TasaConsumoServicio.cs` | Tasas diarias por categoria. |
| `CategoriaPackServicio` | `Servicios/CategoriaPackServicio.cs` | Packs porcentuales por categoria. |
| `HistorialServicio` | `Servicios/HistorialServicio.cs` | Lectura de historial y ultimas entregas. |
| `DistribucionServicio` | `Servicios/DistribucionServicio.cs` | Algoritmo de distribucion y confirmacion. |

## `ConexionDB`

Ruta: `Datos/ConexionDB.cs`

Aunque esta en `Datos/`, todos los servicios dependen de esta clase.

Responsabilidades:

- Construir cadena SQLite.
- Crear `donaciones.db` si falta.
- Crear tablas base.
- Agregar columnas faltantes.
- Crear tablas nuevas.
- Activar `PRAGMA foreign_keys = ON`.

Metodo publico:

| Metodo | Retorno | Explicacion |
|---|---|---|
| `ObtenerConexion()` | `SQLiteConnection` | Devuelve conexion configurada. El caller debe abrirla y cerrarla. |

Patron usado en servicios:

```csharp
using (var conexion = conexionDB.ObtenerConexion())
{
    conexion.Open();
    // ejecutar comando
}
```

## `CategoriaServicio`

Ruta: `Servicios/CategoriaServicio.cs`

Metodos:

| Metodo | Que hace | Efecto |
|---|---|---|
| `ExisteNombre(string nombre)` | Carga categorias y usa `TablaHash` para saber si existe. | Solo lectura. |
| `Guardar(Categoria c)` | Inserta una categoria. | `INSERT Categorias`. |
| `Actualizar(Categoria c)` | Cambia nombre por `Id`. | `UPDATE Categorias`. |
| `Eliminar(int id)` | Borra por `Id`. | `DELETE Categorias`. |
| `ObtenerIdPorNombre(string nombre)` | Busca `Id` por nombre. | Solo lectura. |
| `ListarTodas()` | Devuelve categorias ordenadas por nombre. | `ListaEnlazada`. |

Detalle importante:

`ExisteNombre` convierte nombres a mayusculas para comparar sin sensibilidad a minusculas.

```text
"granos" y "GRANOS" se consideran iguales.
```

## `UnidadServicio`

Ruta: `Servicios/UnidadServicio.cs`

Metodos:

| Metodo | Que hace |
|---|---|
| `Guardar(UnidadMedida u)` | Inserta unidad. |
| `Actualizar(UnidadMedida u)` | Actualiza nombre y tipo. |
| `ObtenerIdPorNombre(string nombre)` | Devuelve ID de la primera unidad con ese nombre. |
| `ListarTodas()` | Devuelve todas las unidades. |
| `AsociarACategoria(int idCat, int idUni)` | Inserta relacion en `CategoriaUnidades` con `INSERT OR IGNORE`. |
| `ListarPorCategoria(int idCat)` | Devuelve unidades permitidas para una categoria. |
| `EliminarAsociacion(int idCat, int idUni)` | Quita vinculo categoria-unidad. |
| `EliminarUnidad(int id)` | Borra vinculos y luego borra unidad. |

Flujo de asociacion:

```text
FormUnidades
  |
  +--> usuario elige categoria y unidad
  |
  +--> UnidadServicio.AsociarACategoria(idCat, idUni)
  |
  +--> INSERT OR IGNORE CategoriaUnidades
```

## `ProductoServicio`

Ruta: `Servicios/ProductoServicio.cs`

Responsabilidad:

- Mantener inventario.
- Cargar productos en `TablaHash indicePorSku`.
- Insertar productos nuevos.
- Sumar stock si el SKU ya existe.
- Actualizar y eliminar por SKU.

Campo interno:

```csharp
private readonly TablaHash indicePorSku;
```

Metodos:

| Metodo | Que hace |
|---|---|
| `GuardarOSumarStock(Producto p)` | Si SKU existe, suma stock. Si no existe, inserta. |
| `Actualizar(Producto p)` | Actualiza producto existente por SKU. |
| `Eliminar(string sku)` | Elimina por SKU. |
| `ListarTodos()` | Recorre `indicePorSku` y devuelve `ListaEnlazada`. |
| `CargarEnHash()` | Devuelve el hash interno. |
| `ListarPorCategoria(int idCategoria)` | Filtra productos por categoria. |
| `ObtenerStockProducto(int productoId)` | Consulta stock real en SQLite por ID. |

Regla critica:

```text
El SKU funciona como llave principal de trabajo en el servicio.
```

Si un producto existente se edita, no se cambia el SKU. Se actualizan nombre, categoria, stock y reglas.

## `BeneficiarioServicio`

Ruta: `Servicios/BeneficiarioServicio.cs`

Responsabilidad:

- Gestionar familias.
- Cargar datos en `ArbolBST` por nombre.
- Mantener `TablaHash` por ID.

Campos internos:

```csharp
private readonly ArbolBST arbolPorNombre;
private readonly TablaHash indicePorId;
```

Metodos:

| Metodo | Que hace |
|---|---|
| `Guardar(Beneficiario b)` | Inserta en SQLite y agrega a indices. |
| `Actualizar(Beneficiario b)` | Actualiza SQLite y reubica en arbol si cambia nombre. |
| `Eliminar(int id)` | Elimina de indices y SQLite. |
| `CargarEnArbol()` | Devuelve el `ArbolBST` interno. |
| `ListarTodos()` | Devuelve beneficiarios en orden alfabetico. |

Flujo de busqueda:

```text
FormBeneficiarios
  |
  +--> servicio.CargarEnArbol().BuscarParcial(texto)
  |
  +--> ArbolBST recorre in-order y compara fragmento
```

## `TasaConsumoServicio`

Ruta: `Servicios/TasaConsumoServicio.cs`

Responsabilidad:

Definir cuanto consume una persona por dia en una categoria.

Metodos:

| Metodo | Que hace |
|---|---|
| `ListarTodas()` | Lista tasas con nombre de categoria y unidad. |
| `Guardar(TasaConsumo tasa)` | Inserta o actualiza por categoria. |
| `ObtenerPorCategoria(int idCategoria)` | Devuelve tasa de una categoria. |
| `Eliminar(int idCategoria)` | Elimina la tasa. |

SQL importante:

```sql
ON CONFLICT(IdCategoria) DO UPDATE SET
  TasaDiaria = excluded.TasaDiaria,
  IdUnidadBase = excluded.IdUnidadBase
```

Esto evita duplicar tasas para la misma categoria.

## `CategoriaPackServicio`

Ruta: `Servicios/CategoriaPackServicio.cs`

Responsabilidad:

Guardar y validar como una categoria se divide entre productos fisicos.

Metodos:

| Metodo | Que hace |
|---|---|
| `ListarPorCategoria(int categoriaId)` | Devuelve lineas de pack con producto, SKU, stock y reglas. |
| `ObtenerTotalPorcentaje(int categoriaId)` | Suma porcentajes del pack. |
| `EsPackValido(int categoriaId)` | Verifica si suma 100 con tolerancia 0.001. |
| `GuardarPack(int categoriaId, ListaEnlazada lineas)` | Valida, borra pack anterior e inserta nuevo en transaccion. |
| `EliminarPack(int categoriaId)` | Borra pack de una categoria. |

Reglas:

```text
Porcentaje negativo: error.
Total distinto de 100: error.
Porcentaje <= 0: se ignora al insertar.
```

Transaccion:

```text
BEGIN
  DELETE pack anterior
  INSERT lineas nuevas
COMMIT
```

Si algo falla:

```text
ROLLBACK
```

## `HistorialServicio`

Ruta: `Servicios/HistorialServicio.cs`

Responsabilidad:

- Mostrar historial confirmado.
- Buscar ultima entrega por producto.
- Buscar ultima entrega por categoria.

Metodos:

| Metodo | Que hace | Uso principal |
|---|---|---|
| `ObtenerHistorialDistribuciones()` | Lista entregas confirmadas con joins. | `FormHistorial`. |
| `ObtenerUltimaEntregaProducto(beneficiarioId, productoId)` | Fecha maxima por producto. | Regla de reposicion. |
| `ObtenerUltimaEntregaCategoria(beneficiarioId, categoriaId)` | Fecha maxima por categoria. | Calculo de deficit. |

Consulta conceptual de ultima entrega por categoria:

```text
Busca MAX(Orden.FechaGeneracion)
donde:
  Orden.Estado = CONFIRMADA
  OrdenDetalle.BeneficiarioId = X
  OrdenDetalle.CategoriaId = Y
```

## `DistribucionServicio`

Ruta: `Servicios/DistribucionServicio.cs`

Responsabilidad:

Es el nucleo de negocio. Genera propuestas, diagnostica por que no hay propuesta, consulta stock y confirma entregas.

Dependencias internas:

```text
ConexionDB
BeneficiarioServicio
ProductoServicio
TasaConsumoServicio
CategoriaPackServicio
HistorialServicio
```

Metodos publicos:

| Metodo | Que hace |
|---|---|
| `GenerarPropuestaDistribucion(int categoriaIdFiltro = 0)` | Crea `ListaEnlazada` de `OrdenDetalle`. |
| `ObtenerDiagnosticoSinPropuesta(int categoriaIdFiltro = 0)` | Devuelve mensaje de causa si no se genero nada. |
| `ObtenerStockTotalCategoria(int idCategoria, int idUnidadBase = 0)` | Suma stock de productos de una categoria. |
| `ObtenerStockProducto(int productoId)` | Delega a `ProductoServicio`. |
| `ConfirmarDistribucion(ListaEnlazada detalles, string observaciones)` | Inserta orden, detalle y descuenta stock en transaccion. |

Metodos privados clave:

| Metodo | Que hace |
|---|---|
| `CalcularDeficit(Beneficiario b, TasaConsumo tasa)` | Calcula necesidad por familia/categoria y crea explicacion. |
| `ObtenerDiasPrimeraEntrega(int nivelVulnerabilidad)` | Devuelve 14, 7 o 4 dias segun vulnerabilidad. |
| `ObtenerPesoVulnerabilidad(int nivelVulnerabilidad)` | Devuelve 100000, 50000 o 10000. |
| `CrearLineasProducto(...)` | Convierte cantidad de categoria en productos del pack. |
| `ObtenerStockTotalPack(ListaEnlazada pack)` | Suma stock de productos dentro del pack. |
| `PuedeCrearAlMenosUnaLineaProducto(...)` | Ayuda al diagnostico de propuesta vacia. |

Diagnosticos posibles:

| Condicion | Mensaje aproximado |
|---|---|
| No hay beneficiarios | `No hay beneficiarios registrados.` |
| Ninguno activo | `Hay beneficiarios registrados, pero ninguno está activo.` |
| No hay tasas | `No hay tasas de consumo configuradas.` |
| Categoria filtrada sin tasa | `La categoría seleccionada no tiene tasa de consumo configurada.` |
| Sin packs | `Las categorías con tasa de consumo no tienen packs configurados.` |
| Pack invalido | `El pack de la categoría ... no suma 100%.` |
| Sin stock | `Los packs configurados existen, pero sus productos no tienen stock disponible.` |
| Sin deficit | `Los beneficiarios activos no tienen déficit pendiente.` |
| Deficit menor a 1 | `Hay déficit pendiente, pero es menor a 1 unidad entera.` |

La explicacion completa del algoritmo esta en `09-algoritmo-distribucion.md`.
