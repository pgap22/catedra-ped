# Base de datos

## Motor y archivo

El sistema usa SQLite local. No requiere servidor ni internet.

Archivo configurado:

```text
donaciones.db
```

Clase responsable:

```text
Datos/ConexionDB.cs
```

Cadena de conexion:

```csharp
Data Source=donaciones.db;Version=3;
```

## Activacion de claves foraneas

SQLite no siempre aplica claves foraneas si no se activan. Este proyecto las activa cuando la conexion pasa a estado abierto:

```csharp
PRAGMA foreign_keys = ON;
```

Esto ocurre en `ConexionDB.ObtenerConexion()`.

## Ciclo de creacion y migracion

```text
Constructor ConexionDB
  |
  +--> InicializarBD()
  |      |
  |      +--> Si no existe donaciones.db, crea archivo y tablas base
  |
  +--> ActualizarEsquema()
         |
         +--> Intenta agregar columnas nuevas
         +--> Crea tablas nuevas si faltan
```

## Tablas principales

```text
Categorias
UnidadesMedida
CategoriaUnidades
Beneficiarios
Productos
TasaConsumo
CategoriaPackDetalle
Orden
OrdenDetalle
```

## Diagrama relacional simplificado

```text
Categorias 1----N Productos
Categorias 1----1 TasaConsumo
Categorias 1----N CategoriaPackDetalle N----1 Productos

Categorias N----N UnidadesMedida
       mediante CategoriaUnidades

Beneficiarios 1----N OrdenDetalle N----1 Orden
Categorias     1----N OrdenDetalle
Productos      1----N OrdenDetalle
```

## `Categorias`

Creada en `ConexionDB.InicializarBD()`.

| Campo | Tipo | Significado |
|---|---|---|
| `Id` | `INTEGER PRIMARY KEY AUTOINCREMENT` | Identificador unico. |
| `Nombre` | `TEXT NOT NULL` | Nombre de la categoria, por ejemplo `Granos Basicos`. |

Usada por:

- `CategoriaServicio`
- `ProductoServicio`
- `TasaConsumoServicio`
- `CategoriaPackServicio`
- `DistribucionServicio`

## `UnidadesMedida`

| Campo | Tipo | Significado |
|---|---|---|
| `Id` | `INTEGER PRIMARY KEY AUTOINCREMENT` | Identificador unico. |
| `Nombre` | `TEXT NOT NULL` | Nombre visible, por ejemplo `Libras`. |
| `Tipo` | `TEXT NOT NULL` | Tipo general, por ejemplo `Peso`, `Volumen`, `Unidad`. |

Pantalla relacionada: `FormUnidades.cs`.

## `CategoriaUnidades`

Tabla pivote muchos-a-muchos.

| Campo | Tipo | Significado |
|---|---|---|
| `IdCategoria` | `INTEGER` | Categoria relacionada. |
| `IdUnidad` | `INTEGER` | Unidad permitida. |

Clave primaria compuesta:

```text
PRIMARY KEY(IdCategoria, IdUnidad)
```

Explicacion para junior:

Una tabla pivote conecta dos tablas cuando una categoria puede tener varias unidades y una unidad puede servir para varias categorias.

Ejemplo:

```text
Granos Basicos -> Libras
Granos Basicos -> Kilos
Aceites y Grasas -> Litros
Aceites y Grasas -> Galones
```

## `Beneficiarios`

| Campo | Tipo | Significado |
|---|---|---|
| `Id` | `INTEGER PRIMARY KEY AUTOINCREMENT` | Identificador de la familia. |
| `Nombre` | `TEXT NOT NULL` | Nombre de la familia o responsable. |
| `MiembrosHogar` | `INTEGER DEFAULT 1` | Cantidad de personas en el hogar. |
| `Activo` | `INTEGER DEFAULT 1` | `1` participa, `0` no participa. |
| `FechaRegistro` | `DATETIME DEFAULT CURRENT_TIMESTAMP` | Fecha de creacion del registro. |
| `NivelVulnerabilidad` | `INTEGER DEFAULT 2` | 1 baja, 2 media, 3 alta. |

Modelo: `Modelos/Beneficiario.cs`.

Servicio: `Servicios/BeneficiarioServicio.cs`.

Nota importante: `FechaRegistro` se carga, pero el algoritmo actual no la usa para la primera entrega. Usa dias iniciales por vulnerabilidad.

## `Productos`

| Campo | Tipo | Significado |
|---|---|---|
| `Id` | `INTEGER PRIMARY KEY AUTOINCREMENT` | Identificador interno. |
| `SKU` | `TEXT UNIQUE NOT NULL` | Codigo unico de inventario. |
| `Nombre` | `TEXT NOT NULL` | Nombre del producto. |
| `IdCategoria` | `INTEGER` | Categoria del producto. |
| `Stock` | `REAL DEFAULT 0` | Cantidad disponible. |
| `IdUnidad` | `INTEGER` | Unidad del producto en base de datos. |
| `MaximoPorEntrega` | `REAL` | Limite por familia en una entrega. |
| `DiasReposicion` | `INTEGER` | Dias minimos para volver a sugerir el mismo producto. |

Modelo: `Modelos/Producto.cs`.

Observacion tecnica: la tabla tiene `IdUnidad`, pero el modelo `Producto` actual no expone esa propiedad. La logica de distribucion opera con la unidad base de la tasa.

## `TasaConsumo`

| Campo | Tipo | Significado |
|---|---|---|
| `IdCategoria` | `INTEGER PRIMARY KEY` | Categoria que tiene esta tasa. |
| `TasaDiaria` | `REAL NOT NULL` | Consumo por persona por dia. |
| `IdUnidadBase` | `INTEGER` | Unidad en la que se expresa la tasa. |

Regla:

```text
Una categoria tiene como maximo una tasa de consumo.
```

El servicio guarda con upsert:

```sql
INSERT INTO TasaConsumo (...)
ON CONFLICT(IdCategoria) DO UPDATE SET ...
```

## `CategoriaPackDetalle`

| Campo | Tipo | Significado |
|---|---|---|
| `Id` | `INTEGER PRIMARY KEY AUTOINCREMENT` | Identificador de la linea. |
| `CategoriaId` | `INTEGER NOT NULL` | Categoria del pack. |
| `ProductoId` | `INTEGER NOT NULL` | Producto incluido. |
| `Porcentaje` | `REAL NOT NULL` | Porcentaje del total de categoria. |

Restriccion:

```text
UNIQUE(CategoriaId, ProductoId)
```

Regla de negocio:

```text
La suma de porcentajes por categoria debe ser 100%.
```

Ejemplo:

```text
Categoria: Granos Basicos
  Producto Arroz Blanco -> 60%
  Producto Frijol Rojo  -> 40%
```

## `Orden`

| Campo | Tipo | Significado |
|---|---|---|
| `Id` | `INTEGER PRIMARY KEY AUTOINCREMENT` | Numero de orden. |
| `FechaGeneracion` | `DATETIME DEFAULT CURRENT_TIMESTAMP` | Fecha de confirmacion. |
| `Estado` | `TEXT NOT NULL` | En el flujo actual se guarda `CONFIRMADA`. |
| `Observaciones` | `TEXT` | Texto ingresado en `FormDistribucion`. |

## `OrdenDetalle`

| Campo | Tipo | Significado |
|---|---|---|
| `Id` | `INTEGER PRIMARY KEY AUTOINCREMENT` | Identificador de la linea. |
| `OrdenId` | `INTEGER` | Orden padre. |
| `BeneficiarioId` | `INTEGER` | Familia que recibe. |
| `CategoriaId` | `INTEGER` | Categoria entregada. |
| `ProductoId` | `INTEGER` | Producto fisico entregado. |
| `CantidadAsignada` | `REAL NOT NULL` | Cantidad descontada del stock. |
| `DeficitCalculado` | `REAL` | Necesidad calculada antes de asignar. |

`OrdenDetalle` es clave para auditoria. Permite contestar:

- Quien recibio.
- Que categoria recibio.
- Que producto especifico recibio.
- Cuanto recibio.
- Que deficit tenia.

## Flujo de escritura al confirmar una entrega

```text
DistribucionServicio.ConfirmarDistribucion(detalles, observaciones)
  |
  +--> BEGIN TRANSACTION
  |
  +--> INSERT INTO Orden
  |
  +--> Por cada detalle:
  |       +--> INSERT INTO OrdenDetalle
  |       +--> UPDATE Productos SET Stock = Stock - cantidad
  |
  +--> COMMIT
```

Si falla el stock o una consulta:

```text
ROLLBACK
```

Esto significa que no queda una orden a medias.
