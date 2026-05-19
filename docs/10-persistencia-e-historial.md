# Persistencia e historial

Este capitulo explica que pasa cuando una propuesta deja de ser simulada y se convierte en una entrega real confirmada.

## Diferencia entre propuesta y entrega confirmada

| Concepto | Donde vive | Modifica stock real | Se ve en historial |
|---|---|---:|---:|
| Propuesta | Memoria en `FormDistribucion.propuestaActual` | No | No |
| Entrega confirmada | SQLite: `Orden` y `OrdenDetalle` | Si | Si |

La propuesta es editable. La entrega confirmada es trazabilidad permanente.

## Flujo de confirmacion

```text
FormDistribucion
  |
  +--> usuario presiona Entregar Familia Seleccionada
  |    o Entregar Todos Mostrados
  |
  +--> crea ListaEnlazada de OrdenDetalle
  |
  +--> abre FormConfirmacionDistribucion
  |
  +--> si usuario confirma:
          |
          v
     DistribucionServicio.ConfirmarDistribucion(detalles, observaciones)
          |
          +--> INSERT Orden
          +--> INSERT OrdenDetalle
          +--> UPDATE Productos
```

## `FormConfirmacionDistribucion`

Ruta: `FormConfirmacionDistribucion.cs`

La pantalla no guarda directamente. Solo muestra resumen y devuelve `DialogResult.OK` si el usuario confirma.

Columnas visibles:

```text
Familia
Vulnerabilidad
Categoria
Producto
SKU
Cantidad a descontar
Unidad
```

Reglas antes de confirmar:

- Redondea cantidades con `Math.Floor`.
- Ignora cantidades menores o iguales a cero.
- Calcula total de unidades fisicas a descontar.
- Deshabilita boton confirmar si no hay filas.

## Transaccion de confirmacion

Ruta: `Servicios/DistribucionServicio.cs`

Metodo:

```text
ConfirmarDistribucion(ListaEnlazada detalles, string observaciones)
```

La transaccion asegura atomicidad.

Atomicidad explicado para junior:

```text
O se guarda todo completo,
o no se guarda nada.
```

Flujo:

```text
BEGIN TRANSACTION
  INSERT Orden
  FOR cada detalle:
    INSERT OrdenDetalle
    UPDATE Productos
COMMIT
```

Si ocurre error:

```text
ROLLBACK
throw
```

## Insercion de `Orden`

SQL real conceptual:

```sql
INSERT INTO Orden (FechaGeneracion, Estado, Observaciones)
VALUES (@fecha, 'CONFIRMADA', @obs);
SELECT last_insert_rowid();
```

Fecha usada:

```text
RelojDemo.Ahora
```

Esto permite que una fecha simulada aparezca tambien en historial.

## Insercion de `OrdenDetalle`

Por cada linea se guarda:

```sql
INSERT INTO OrdenDetalle
  (OrdenId, BeneficiarioId, CategoriaId, ProductoId, CantidadAsignada, DeficitCalculado)
VALUES
  (@oId, @bId, @cId, @pId, @cant, @def)
```

Datos persistidos:

| Campo | De donde sale |
|---|---|
| `OrdenId` | ID de la orden recien creada. |
| `BeneficiarioId` | Fila de propuesta. |
| `CategoriaId` | Fila de propuesta. |
| `ProductoId` | Producto fisico sugerido. |
| `CantidadAsignada` | Cantidad final, ya ajustada. |
| `DeficitCalculado` | Deficit calculado por el algoritmo. |

## Descuento moderno por producto especifico

Si `ProductoId > 0`, se descuenta exactamente ese producto:

```sql
UPDATE Productos
SET Stock = Stock - @cant
WHERE Id = @pId AND Stock >= @cant
```

La condicion `Stock >= @cant` evita stock negativo.

Si SQLite actualiza cero filas:

```text
No habia stock suficiente.
Se lanza excepcion.
Se hace rollback.
```

## Flujo legacy sin `ProductoId`

Existe una ruta de compatibilidad si `ProductoId <= 0`.

En ese caso el servicio descuenta por categoria usando productos con stock, ordenados por ID:

```sql
SELECT Id, Stock
FROM Productos
WHERE IdCategoria = @cId AND Stock > 0
ORDER BY Id ASC
```

Luego va descontando hasta cubrir la cantidad.

Documentacion importante:

```text
La generacion actual produce lineas con ProductoId especifico.
El flujo sin ProductoId debe entenderse como compatibilidad con datos antiguos o flujos legacy.
```

## Actualizacion de la propuesta tras confirmar

En `FormDistribucion.ConfirmarDetalles`:

1. Se confirma en el servicio.
2. Se llama `RemoverDetallesConfirmados`.
3. Se reinicia `undoStack`.
4. Se recarga stock por producto.
5. Se vuelve a mostrar la propuesta.

```text
Propuesta antes:
  Solis - Arroz
  Solis - Frijol
  Lopez - Arroz

Confirmar Solis

Propuesta despues:
  Lopez - Arroz
```

## Historial visible

Ruta UI: `FormHistorial.cs`

Servicio: `HistorialServicio.cs`

Consulta principal:

```text
Orden
  JOIN OrdenDetalle
  JOIN Beneficiarios
  JOIN Categorias
  LEFT JOIN Productos
  JOIN TasaConsumo
  JOIN UnidadesMedida
WHERE Orden.Estado = 'CONFIRMADA'
ORDER BY FechaGeneracion DESC
```

Columnas mostradas:

| Columna | Explicacion |
|---|---|
| `Nº Orden` | ID de orden confirmada. |
| `Fecha` | Fecha de generacion formateada. |
| `Beneficiario` | Familia que recibio. |
| `Categoria` | Categoria de ayuda. |
| `Producto` | Producto fisico. |
| `Deficit Previo` | Deficit calculado. |
| `Asignado` | Cantidad entregada. |
| `Unidad` | Unidad base de la tasa. |

## Historial como entrada del siguiente calculo

El historial no solo es reporte. Tambien alimenta futuras distribuciones.

```text
Entrega confirmada hoy
  |
  v
Orden + OrdenDetalle
  |
  v
HistorialServicio.ObtenerUltimaEntregaCategoria
  |
  v
Proxima distribucion calcula menos dias de deficit
```

Ejemplo:

```text
Familia Lopez recibio Granos hoy.

Manana se genera propuesta:
dias desde ultima entrega = 1
deficit = miembros * tasa * 1

Su deficit baja respecto a una familia que no recibio hace 14 dias.
```

## Ultima entrega por producto

Metodo:

```text
HistorialServicio.ObtenerUltimaEntregaProducto(beneficiarioId, productoId)
```

Uso:

Regla de reposicion en `DistribucionServicio.CrearLineasProducto`.

Ejemplo:

```text
Producto: Cepillo Dental
DiasReposicion: 90
Ultima entrega: hace 20 dias

20 < 90
=> se omite el cepillo para esa familia
```

## Ultima entrega por categoria

Metodo:

```text
HistorialServicio.ObtenerUltimaEntregaCategoria(beneficiarioId, categoriaId)
```

Uso:

Calculo de deficit.

Ejemplo:

```text
Ultima entrega de Granos: hace 10 dias
Miembros: 5
Tasa: 0.4

deficit = 5 * 0.4 * 10 = 20
```

## Integridad del stock

El sistema valida stock en dos momentos:

1. En propuesta, con stock temporal del pack.
2. En confirmacion, con SQLite y `WHERE Stock >= @cant`.

Esto protege contra:

- Ediciones manuales excesivas.
- Cambios de stock entre generacion y confirmacion.
- Confirmaciones parciales que intentan descontar mas de lo disponible.
