# Algoritmo de distribucion

Este capitulo explica el algoritmo mas importante del sistema. La implementacion real esta en:

```text
Servicios/DistribucionServicio.cs
```

Pantalla que lo ejecuta:

```text
FormDistribucion.cs
```

Estructura principal de prioridad:

```text
Estructuras/MonticuloMaximo.cs
```

## Objetivo

Generar una propuesta de ayuda social que responda estas preguntas:

```text
Que familia recibe?
Que producto fisico recibe?
Cuanto recibe?
Por que esa familia tiene prioridad?
Hay stock suficiente?
Se respeta la regla de reposicion del producto?
Se respeta el maximo por entrega?
```

## Entradas del algoritmo

| Entrada | Servicio que la entrega | Para que sirve |
|---|---|---|
| Beneficiarios | `BeneficiarioServicio.ListarTodos()` | Saber quienes pueden recibir. |
| Tasas de consumo | `TasaConsumoServicio.ListarTodas()` | Saber que categorias se distribuyen. |
| Packs | `CategoriaPackServicio.ListarPorCategoria()` | Convertir categoria en productos. |
| Stock de pack | `CategoriaPackDetalle.StockDisponible` | No entregar mas de lo disponible. |
| Historial por categoria | `HistorialServicio.ObtenerUltimaEntregaCategoria()` | Calcular dias desde ultima entrega. |
| Historial por producto | `HistorialServicio.ObtenerUltimaEntregaProducto()` | Aplicar dias de reposicion. |
| Filtro opcional | Parametro `categoriaIdFiltro` | Distribuir todas o solo una categoria. |

## Salida del algoritmo

Devuelve:

```text
ListaEnlazada de OrdenDetalle
```

Cada `OrdenDetalle` significa:

```text
La familia X recibira N unidades del producto Y,
porque tenia deficit D en la categoria Z.
```

## Paso 1 - Crear propuesta vacia

Codigo real conceptual:

```csharp
ListaEnlazada propuesta = new ListaEnlazada();
```

Explicacion:

La propuesta empieza vacia. El algoritmo ira agregando lineas si encuentra beneficiarios, deficit y stock.

## Paso 2 - Cargar beneficiarios y tasas

```csharp
var beneficiarios = beneficiarioServicio.ListarTodos();
var tasas = tasaServicio.ListarTodas();
```

Si no hay beneficiarios o no hay tasas:

```text
retorna propuesta vacia
```

Por que las tasas importan:

Una tasa define que una categoria se puede distribuir automaticamente.

```text
Sin tasa de consumo
  = el sistema no sabe cuanto necesita una persona por dia
  = no puede calcular deficit
```

## Paso 3 - Recorrer cada categoria con tasa

El algoritmo recorre cada `TasaConsumo`.

```text
Por cada tasa:
  categoria = tasa.IdCategoria
```

Si hay filtro de categoria y no coincide, se omite.

```text
Filtro = Higiene
Tasa actual = Granos
Resultado = omitir
```

## Paso 4 - Cargar pack de la categoria

```csharp
var pack = packServicio.ListarPorCategoria(tasa.IdCategoria);
```

Si no hay pack:

```text
No se puede convertir categoria en productos fisicos.
Se omite esa categoria.
```

Si el pack existe pero no suma 100%:

```text
Se lanza InvalidOperationException.
```

Regla exacta de validacion en `CategoriaPackServicio`:

```text
abs(total - 100) < 0.001
```

## Paso 5 - Calcular stock util del pack

Metodo:

```text
ObtenerStockTotalPack(pack)
```

Regla:

```text
total += floor(linea.StockDisponible)
```

Solo cuenta productos dentro del pack.

Ejemplo:

```text
Categoria Granos Basicos
Pack:
  Arroz stock 10
  Frijol stock 8

Producto fuera del pack:
  Harina stock 100

Stock util del pack = 10 + 8 = 18
Harina no cuenta porque no esta en el pack.
```

## Paso 6 - Crear heap de familias elegibles

```csharp
MonticuloMaximo heap = new MonticuloMaximo(beneficiarios.Conteo());
```

El heap es una cola de prioridad. La familia con mayor prioridad queda arriba.

```text
heap
  |
  v
familia mas prioritaria siempre sale primero
```

## Paso 7 - Calcular deficit por beneficiario

Metodo:

```text
CalcularDeficit(Beneficiario b, TasaConsumo tasa)
```

Primero busca ultima entrega confirmada de esa categoria:

```csharp
historialServicio.ObtenerUltimaEntregaCategoria(b.Id, tasa.IdCategoria)
```

### Si existe ultima entrega

```text
dias = RelojDemo.Ahora - fechaUltimaEntrega
```

Si la fecha de ultima entrega esta en el futuro, se ajusta a `RelojDemo.Ahora` para evitar dias negativos.

### Si no existe ultima entrega

No usa `FechaRegistro`. Usa dias iniciales fijos por vulnerabilidad:

```text
Alta  -> 14 dias
Media -> 7 dias
Baja  -> 4 dias
```

Constantes reales:

```csharp
private const int DiasPrimeraEntregaAlta = 14;
private const int DiasPrimeraEntregaMedia = 7;
private const int DiasPrimeraEntregaBaja = 4;
```

### Formula de deficit

```text
deficit = miembros del hogar * tasa diaria * dias
```

Ejemplo:

```text
Familia Solis
Miembros: 2
Vulnerabilidad: Alta
Sin entrega previa
Dias iniciales: 14
Tasa granos: 0.4 libras/persona/dia

deficit = 2 * 0.4 * 14
deficit = 11.2 libras
```

Si `deficit <= 0`, la familia no entra al heap.

## Paso 8 - Calcular prioridad

Codigo real:

```csharp
decimal prioridad = ObtenerPesoVulnerabilidad(b.NivelVulnerabilidad)
                  + Math.Round((decimal)deficit, 4)
                  + (b.MiembrosHogar * 0.00001m)
                  + ((1000000m - b.Id) * 0.0000000001m);
```

Pesos de vulnerabilidad:

```text
Alta  -> 100000
Media -> 50000
Baja  -> 10000
```

Orden real de importancia:

1. Vulnerabilidad.
2. Deficit.
3. Miembros del hogar como desempate pequeño.
4. ID menor como desempate estable.

Ejemplo:

```text
Familia A: Alta, deficit 3
prioridad = 100000 + 3 = 100003

Familia B: Media, deficit 80
prioridad = 50000 + 80 = 50080

Gana Familia A porque Alta pesa mas que Media.
```

## Paso 9 - Insertar en heap

Por cada beneficiario activo con deficit positivo:

```csharp
heap.Insertar(prioridad, info);
```

`info` contiene:

```text
Beneficiario
Deficit
Asignado
ExplicacionCalculo
```

Visual:

```text
                 Familia Solis
              prioridad 100011.2
                /            \
       Familia Lopez      Familia Martinez
       prioridad 50014    prioridad 10011.2
```

## Paso 10 - Extraer mayor prioridad mientras haya stock

```csharp
while (stockDisponible > 0)
{
    var max = heap.ExtraerMaximo();
}
```

Si el heap esta vacio:

```text
break
```

## Paso 11 - Calcular cantidad de categoria a asignar

Codigo:

```csharp
double aAsignar = Math.Floor(Math.Min(demanda, stockDisponible));
```

Reglas:

```text
No entregar mas que el deficit.
No entregar mas que el stock util.
Redondear hacia abajo.
Si queda menor a 1, no genera linea.
```

Ejemplo:

```text
deficit = 11.2
stock = 20
min = 11.2
floor = 11

El sistema intentara repartir 11 unidades de la categoria.
```

## Paso 12 - Dividir categoria en productos del pack

Metodo:

```text
CrearLineasProducto(info, tasa, pack, aAsignar)
```

Para cada producto del pack:

```text
solicitado = floor(cantidadCategoria * porcentaje / 100)
```

Luego aplica reglas:

1. Si `solicitado < 1`, omite producto.
2. Si tiene `DiasReposicion`, revisa ultima entrega del producto.
3. Si todavia no pasaron suficientes dias, omite producto.
4. Si tiene `MaximoPorEntrega`, limita cantidad.
5. Si no hay stock, omite producto.
6. Si stock es menor que solicitado, reduce.
7. Si asignado final es mayor o igual a 1, crea `OrdenDetalle`.

## Paso 13 - Descontar stock temporal

Dentro de la propuesta se descuenta del objeto de pack:

```csharp
lineaPack.StockDisponible -= asignado;
```

Esto no toca SQLite todavia. Solo evita que la propuesta sobreasigne el mismo producto antes de confirmar.

```text
Stock real SQLite: todavia igual
Stock temporal pack: baja durante la generacion
```

## Paso 14 - Agregar lineas a propuesta

Cada linea generada se agrega a `propuesta`:

```csharp
propuesta.Agregar(resultadoSplit.Lineas.Obtener(k));
```

Al final, `FormDistribucion` recibe la lista y la muestra agrupada por familia.

## Ejemplo completo con numeros pequenos

Datos:

```text
Fecha actual: 2026-05-18

Categoria: Granos Basicos
Tasa: 0.4 libras/persona/dia

Pack:
  Arroz Blanco 60%, stock 10
  Frijol Rojo 40%, stock 10

Beneficiarios sin entregas previas:
  Familia Solis: 2 miembros, Alta
  Familia Lopez: 5 miembros, Media
  Familia Martinez: 7 miembros, Baja
```

### 1. Dias iniciales

```text
Solis    Alta  -> 14 dias
Lopez    Media -> 7 dias
Martinez Baja  -> 4 dias
```

### 2. Deficit

```text
Solis    = 2 * 0.4 * 14 = 11.2
Lopez    = 5 * 0.4 * 7  = 14.0
Martinez = 7 * 0.4 * 4  = 11.2
```

### 3. Prioridad

```text
Solis    = 100000 + 11.2 = 100011.2
Lopez    =  50000 + 14.0 =  50014.0
Martinez =  10000 + 11.2 =  10011.2
```

Orden:

```text
1. Solis
2. Lopez
3. Martinez
```

### 4. Heap

```text
                 Solis
             prioridad 100011.2
              /             \
        Lopez                 Martinez
   prioridad 50014       prioridad 10011.2
```

### 5. Sale Solis

Stock util:

```text
Arroz 10 + Frijol 10 = 20
```

Cantidad categoria:

```text
floor(min(11.2, 20)) = 11
```

Split:

```text
Arroz  = floor(11 * 0.60) = 6
Frijol = floor(11 * 0.40) = 4
```

Resultado:

```text
Solis recibe 6 Arroz y 4 Frijol.
```

Stock temporal:

```text
Arroz: 10 - 6 = 4
Frijol: 10 - 4 = 6
Total temporal = 10
```

Observacion:

```text
Se intentaban 11 unidades de categoria,
pero el split produjo 10 unidades fisicas por redondeo.
La unidad sobrante no se reasigna.
```

### 6. Sale Lopez

```text
floor(min(14, 10)) = 10
```

Split ideal:

```text
Arroz  = floor(10 * 0.60) = 6
Frijol = floor(10 * 0.40) = 4
```

Stock temporal disponible:

```text
Arroz: 4
Frijol: 6
```

Asignacion real:

```text
Arroz solicitado 6, disponible 4 -> asigna 4
Frijol solicitado 4, disponible 6 -> asigna 4
```

Resultado:

```text
Lopez recibe 4 Arroz y 4 Frijol.
```

Stock temporal:

```text
Arroz: 0
Frijol: 2
```

### 7. Sale Martinez

```text
floor(min(11.2, 2)) = 2
```

Split:

```text
Arroz  = floor(2 * 0.60) = 1
Frijol = floor(2 * 0.40) = 0
```

Stock:

```text
Arroz: 0
Frijol: 2
```

Resultado:

```text
Arroz se omite por falta de stock.
Frijol se omite porque solicitado es 0.
Martinez no genera linea.
```

### 8. Propuesta final

```text
Familia Solis - Alta
  +-- Arroz Blanco | 6 libras
  +-- Frijol Rojo  | 4 libras

Familia Lopez - Media
  +-- Arroz Blanco | 4 libras
  +-- Frijol Rojo  | 4 libras

Familia Martinez - Baja
  +-- Sin linea generada por stock/redondeo
```

Tabla:

```text
+------------------+----------------+--------------+----------+---------+
| Beneficiario     | Vulnerabilidad | Producto     | Cantidad | Deficit |
+------------------+----------------+--------------+----------+---------+
| Familia Solis    | Alta           | Arroz Blanco | 6        | 11.2    |
| Familia Solis    | Alta           | Frijol Rojo  | 4        | 11.2    |
| Familia Lopez    | Media          | Arroz Blanco | 4        | 14.0    |
| Familia Lopez    | Media          | Frijol Rojo  | 4        | 14.0    |
+------------------+----------------+--------------+----------+---------+
```

## Edge cases documentados

| Caso | Comportamiento |
|---|---|
| No hay beneficiarios | Propuesta vacia y diagnostico. |
| Beneficiarios inactivos | Se omiten. |
| No hay tasas | No hay categorias distribuibles. |
| Categoria filtrada sin tasa | Diagnostico especifico. |
| Categoria sin pack | Se omite y se diagnostica. |
| Pack no suma 100 | Error al generar o al guardar. |
| Productos sin stock | Categoria se omite. |
| Deficit menor o igual a cero | Beneficiario no entra al heap. |
| Deficit menor a una unidad | No genera entrega por `floor`. |
| Split deja residuos | Residuos no se reasignan. |
| Reposicion vigente | Producto se omite. |
| Maximo por entrega | Cantidad se limita. |
| Stock insuficiente | Cantidad se reduce. |
| Confirmacion con stock insuficiente | Rollback de transaccion. |

## Confirmacion de la propuesta

La generacion no modifica SQLite. Solo al confirmar ocurre:

```text
INSERT Orden
INSERT OrdenDetalle
UPDATE Productos SET Stock = Stock - cantidad
```

El `UPDATE` moderno exige stock suficiente:

```sql
UPDATE Productos
SET Stock = Stock - @cant
WHERE Id = @pId AND Stock >= @cant
```

Si no actualiza filas, lanza error y hace rollback.
