## Context

El sistema actual usa `DistribucionServicio` para calcular déficit por categoría con tasas diarias, priorizar beneficiarios con `MonticuloMaximo`, y confirmar entregas en `Orden` / `OrdenDetalle`. La categoría funciona como unidad principal de decisión, pero el producto específico solo aparece como sugerencia visual. Al confirmar, el stock se descuenta desde productos de la categoría en orden FIFO, sin guardar exactamente qué producto recibió la familia.

Esto crea una diferencia entre lo mostrado, lo descontado y el historial. Para que la asignación sea defendible, la prioridad puede seguir siendo por categoría, pero la entrega confirmada debe partirse en productos reales según un pack definido por porcentajes.

El proyecto debe mantenerse simple, en WinForms y SQLite, usando estructuras propias del curso donde ya aplican. No se introducen dependencias externas ni se reemplazan estructuras propias por colecciones genéricas.

## Goals / Non-Goals

**Goals:**
- Configurar un pack por categoría usando porcentajes asociados a productos existentes.
- Usar el pack para convertir una cantidad asignada por categoría en varias líneas de producto real.
- Guardar el producto específico en cada detalle confirmado.
- Mantener la lógica de prioridad por déficit de categoría.
- Soportar reglas opcionales de reposición por producto para artículos no diarios.
- Mantener un flujo demostrable y comprensible para la defensa del proyecto.

**Non-Goals:**
- No implementar conversiones complejas entre unidades distintas dentro de la misma categoría.
- No agregar compras, proveedores ni lotes de inventario.
- No crear una planificación nutricional avanzada.
- No cambiar SQLite ni la arquitectura WinForms actual.
- No hacer una migración compleja de históricos antiguos más allá de permitir visualizarlos sin producto específico si existen.

## Decisions

### Mantener la prioridad por categoría y descomponer después por pack

La necesidad social se seguirá calculando por categoría, porque las tasas actuales están definidas por categoría y unidad base. Después de decidir cuánto corresponde a una familia en una categoría, el sistema divide esa cantidad usando el pack.

Alternativa considerada: calcular déficit por producto. Se descarta porque obligaría a configurar tasas por cada producto y complicaría demasiado la demo y la captura de datos.

### Usar porcentajes por producto en el pack

Cada categoría tendrá una tabla de detalle de pack con producto y porcentaje. La suma válida para una categoría debe ser 100% para que se use automáticamente.

Alternativa considerada: cantidades fijas por familia. Se descarta porque no escala bien con familias de diferente tamaño ni con déficits variables.

### Guardar producto específico en `OrdenDetalle`

`OrdenDetalle` debe incluir `ProductoId` o `SKUProducto` para nuevas entregas. Esto permite que historial, auditoría y descuento de inventario coincidan.

Alternativa considerada: guardar un texto con productos entregados dentro de una sola fila. Se descarta porque dificulta consultas, filtros e historial por producto.

### Generar varias filas de propuesta por familia/categoría

La UI de distribución debe mostrar una fila por producto asignado. La categoría y el beneficiario pueden repetirse, pero el usuario verá exactamente qué se entregará.

Ejemplo:

```text
Familia Lopez | Granos Basicos | Arroz Blanco | 10 lb
Familia Lopez | Granos Basicos | Frijol Rojo  | 6 lb
Familia Lopez | Granos Basicos | Azucar Blanca| 4 lb
```

Alternativa considerada: mostrar una fila agrupada y abrir detalle aparte. Se descarta inicialmente porque complica confirmación y edición manual.

### Aplicar reglas de reposición solo cuando estén configuradas

Los productos normales tendrán campos de reposición vacíos. Si un producto tiene `DiasReposicion`, el sistema revisa el historial de ese beneficiario y producto antes de sugerirlo. Si tiene `MaximoPorEntrega`, la propuesta no debe superar ese valor por beneficiario y entrega.

Alternativa considerada: crear tipos de producto separados. Se descarta porque dos columnas opcionales resuelven el caso sin agregar demasiada estructura.

### Tratar falta de stock en un producto del pack de forma conservadora

Si un producto del pack no tiene suficiente stock, el sistema asigna hasta donde alcance y no inventa una sustitución automática con otro producto salvo que ese otro producto ya tenga porcentaje en el pack.

Esto evita casos engañosos como cubrir arroz faltante con azúcar sin que el pack lo indique. La cantidad no cubierta queda sin asignar.

## Risks / Trade-offs

- Mayor cantidad de filas en distribución e historial -> Mantener columnas claras y filtros actuales para beneficiario/categoría.
- Packs mal configurados con porcentajes que no suman 100 -> Validar configuración antes de generar propuesta y mostrar mensaje claro.
- Stock insuficiente en un producto del pack -> Asignar solo disponible y explicar en cálculo que hubo faltante por producto.
- Historial antiguo sin producto específico -> Mostrarlo como entrega histórica por categoría cuando `ProductoId` esté vacío.
- Edición manual puede romper proporciones del pack -> Permitir edición por fila de producto, pero validar stock por producto y categoría antes de confirmar.
- Reposición de productos puede reducir entregas esperadas -> Mostrar en explicación cuando un producto fue omitido por haber sido entregado recientemente.

## Migration Plan

1. Actualizar esquema SQLite con tabla de pack y columnas opcionales de reposición.
2. Agregar columna de producto en `OrdenDetalle` permitiendo nulos para no romper históricos existentes.
3. Poblar la demo pequeña con packs por porcentaje.
4. Ajustar servicios para leer packs y generar detalles por producto.
5. Actualizar UI de distribución e historial.
6. Compilar y probar flujo completo: seed demo, propuesta, confirmación, historial y simulación de fecha.

Rollback: si se requiere revertir durante desarrollo, las columnas nuevas pueden quedar sin uso y la distribución puede volver temporalmente al flujo por categoría. No se deben eliminar datos confirmados sin autorización del usuario.

## Open Questions

- Si un pack no suma 100%, ¿se debe bloquear la distribución o solo excluir esa categoría? Recomendación inicial: bloquear esa categoría con mensaje claro.
- ¿La edición manual debe permitir cambiar producto o solo cantidad? Recomendación inicial: solo cantidad para mantener simple el primer alcance.
