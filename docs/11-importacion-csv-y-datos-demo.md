# Importacion CSV y datos demo

Este capitulo explica como entran datos masivos y como se generan datos de prueba.

## Utilidad principal

Ruta: `Utilidades/ManejadorCSV.cs`

Responsabilidad:

- Guardar plantillas CSV.
- Contar filas de datos.
- Parsear categorias.
- Parsear beneficiarios.
- Parsear productos.
- Parsear unidades.

## Limitacion del parser CSV

El parser usa:

```csharp
linea.Split(',')
```

Esto significa que soporta CSV simple, pero no soporta correctamente:

- Campos entre comillas con comas internas.
- Escape de comillas.
- Validacion estricta de encabezados.

Ejemplo que funciona:

```csv
SKU001,Arroz,Granos Basicos,50
```

Ejemplo problematico:

```csv
SKU010,"Arroz, premium",Granos Basicos,50
```

El segundo tiene una coma dentro del nombre y `Split(',')` lo parte mal.

## Plantillas CSV

### Categorias

Pantalla: `FormCategorias.cs`

```csv
Nombre
Granos Basicos
Lacteos
Aceites
```

Parser:

```text
ManejadorCSV.ParsearCategorias
```

Reglas:

- Ignora encabezado.
- Cada linea posterior es un nombre.
- Omite lineas vacias.

### Unidades

Pantalla: `FormUnidades.cs`

```csv
Nombre,Tipo
Libra,Peso (lb/kg)
Litro,Volumen (lt/ml)
Bolsa,Unidad (pza/bolsa)
```

Parser:

```text
ManejadorCSV.ParsearUnidades
```

Reglas:

- Exige al menos 2 columnas.
- Nombre obligatorio.
- Tipo obligatorio.

### Productos

Pantalla: `FormProductos.cs`

```csv
SKU,Nombre,NombreCategoria,Stock
SKU001,Arroz,Granos Basicos,50
SKU002,Leche,Lacteos,100
```

Parser:

```text
ManejadorCSV.ParsearProductos
```

Reglas:

- Exige 4 columnas.
- SKU, nombre y categoria obligatorios.
- Stock debe ser numero y no negativo.
- Luego `FormProductos` busca categoria por nombre.
- Si la categoria no existe, omite producto.
- Si el SKU ya existe, omite en importacion.

### Beneficiarios

Pantalla: `FormBeneficiarios.cs`

```csv
Nombre,Miembros,NivelVulnerabilidad
Juan Perez,5,Media
Maria Lopez,3,Alta
Familia Solis,2,Baja
```

Parser:

```text
ManejadorCSV.ParsearBeneficiarios
```

Reglas:

- Exige al menos 2 columnas.
- Nombre obligatorio.
- Miembros debe ser entero positivo, si no, usa 1.
- Nivel puede ser texto o numero.
- Nivel desconocido cae a media.

## Conteo de filas invalidas

Metodo:

```text
ManejadorCSV.ContarFilasDatos
```

Cuenta lineas no vacias despues del encabezado.

Las pantallas calculan:

```text
invalidas = totalCsv - listaParseada.Conteo()
```

Ejemplo:

```text
Archivo con 10 filas de datos
Parser devuelve 8 objetos
invalidas = 2
```

## Deshacer importaciones

Las pantallas guardan una accion tipo `Importacion` en `Pila` con los registros insertados realmente.

```text
Importar CSV
  |
  +--> insertar validos no duplicados
  |
  +--> guardar ListaEnlazada insertados en AccionUndo
  |
  v
Boton deshacer
  |
  +--> elimina esos insertados
```

## Generador de datos

Ruta: `Utilidades/GeneradorDatos.cs`

Responsabilidad:

- Borrar datos existentes.
- Insertar datos demo pequenos.
- Insertar datos de prueba grandes.

Metodos publicos:

| Metodo | Que hace |
|---|---|
| `SembrarDemoPequena()` | Crea dataset pequeno y explicable. |
| `SembrarDatosPrueba()` | Crea dataset grande con 150 beneficiarios y muchos productos. |

## Limpieza de base

Metodo privado:

```text
LimpiarBase(SQLiteConnection conexion, SQLiteTransaction tr)
```

Tablas borradas:

```text
OrdenDetalle
Orden
CategoriaPackDetalle
TasaConsumo
Productos
CategoriaUnidades
Categorias
UnidadesMedida
Beneficiarios
```

Tambien limpia `sqlite_sequence` para reiniciar autoincrementos.

## Demo pequena

Metodo:

```text
GeneradorDatos.SembrarDemoPequena()
```

Categorias:

```text
1. Granos Basicos
2. Aceites y Grasas
3. Higiene Personal
```

Unidades:

```text
1. Libras
2. Litros
3. Unidades
```

Tasas:

```text
Granos Basicos: 0.4 Libras
Aceites y Grasas: 0.05 Litros
Higiene Personal: 0.1 Unidades
```

Productos:

```text
SKU001 Arroz Blanco     Granos  stock 40
SKU002 Frijol Rojo      Granos  stock 25
SKU003 Aceite Vegetal   Aceites stock 12
SKU004 Jabon de Bano    Higiene stock 30
SKU005 Cepillo Dental   Higiene stock 20 max 1 reposicion 90
```

Packs:

```text
Granos:
  Arroz 60%
  Frijol 40%

Aceites:
  Aceite Vegetal 100%

Higiene:
  Jabon 70%
  Cepillo 30%
```

Beneficiarios:

```text
Familia Lopez     5 miembros Media
Familia Perez     3 miembros Media
Familia Martinez  7 miembros Baja
Familia Solis     2 miembros Alta
```

Uso recomendado:

```text
Dev / Demo > Sembrar Demo Pequena
Ayuda Social > Generar Asignacion
```

## Datos de prueba grandes

Metodo:

```text
GeneradorDatos.SembrarDatosPrueba()
```

Incluye:

- 6 categorias.
- 7 unidades.
- Relaciones categoria-unidad.
- Tasas por categoria.
- Catalogo amplio de productos.
- Packs para varias categorias.
- 150 beneficiarios con nombres aleatorios.

Categorias grandes:

```text
Granos Basicos
Aceites y Grasas
Higiene Personal
Lacteos y Derivados
Proteinas (Carnes/Atun)
Cereales y Bebidas
```

Uso recomendado:

```text
Dev / Demo > Sembrar Datos de Prueba (150+)
```

## Fecha demo

Ruta: `Utilidades/RelojDemo.cs`

Propiedades:

| Propiedad | Que devuelve |
|---|---|
| `Ahora` | Fecha simulada si existe, si no `DateTime.Now`. |
| `EstaSimulado` | `true` si hay fecha simulada. |
| `FechaSimulada` | Fecha simulada nullable. |

Metodos:

| Metodo | Que hace |
|---|---|
| `EstablecerFecha(DateTime fecha)` | Activa fecha simulada. |
| `UsarFechaReal()` | Elimina simulacion. |

Importancia:

El algoritmo de deficit usa `RelojDemo.Ahora`, no `DateTime.Now` directamente.

```text
Simular fecha futura
  |
  v
Mas dias desde ultima entrega
  |
  v
Mayor deficit
```

## Escalado DPI

Ruta: `Utilidades/EscaladorDpi.cs`

Responsabilidad:

Escalar controles en pantallas cuando `DeviceDpi / 96f` es mayor a 1.

Flujo:

```text
Formulario calcula factor
  |
  v
EscaladorDpi.EscalarJerarquia(this, factor)
  |
  v
Recorre controles internos
  |
  +--> escala posicion
  +--> escala tamaño
  +--> escala margin
  +--> escala padding
```

Se usa en varios formularios principales para que la UI sea mas legible en pantallas con DPI alto.
