## Why

La distribución actual calcula necesidad por categoría, pero solo muestra un producto sugerido y descuenta stock de varios productos sin registrar exactamente qué recibió cada familia. Esto puede producir entregas poco claras, como asignar una cantidad de "Granos Basicos" pero mostrar solo arroz aunque realmente se descuente frijol o azucar.

También hay productos que no deben tratarse como consumo diario, por ejemplo cepillos dentales o artículos de higiene con reposición cada varios meses. El sistema necesita distinguir paquetes consumibles por categoría y productos con reglas opcionales de reposición para que la asignación sea útil y defendible.

## What Changes

- Agregar configuración de packs por categoría usando porcentajes por producto.
- Generar propuestas con múltiples productos reales por familia y categoría, según el pack configurado y el stock disponible.
- Guardar en el historial el producto específico entregado, no solo la categoría.
- Mantener la prioridad de beneficiarios por déficit de categoría usando el flujo actual de distribución inteligente.
- Agregar reglas opcionales por producto para limitar entregas periódicas:
  - máximo por entrega;
  - días mínimos para reposición.
- Permitir que productos sin reglas de reposición sigan funcionando como productos normales de consumo.
- Actualizar la demo pequeña para incluir un pack claro por porcentajes.
- **BREAKING**: `OrdenDetalle` deberá registrar producto específico para nuevas entregas. Las entregas antiguas sin producto pueden mostrarse como históricas por categoría si existen datos previos.

## Capabilities

### New Capabilities
- `category-percentage-packs`: Configuración de packs por categoría que divide una entrega de categoría entre productos reales usando porcentajes.
- `product-replenishment-rules`: Reglas opcionales para productos que tienen máximo por entrega y periodo mínimo de reposición.

### Modified Capabilities
- `distribution-management`: La distribución debe proponer, confirmar y registrar productos específicos derivados del pack, manteniendo la prioridad por déficit de categoría.
- `delivery-history`: El historial debe mostrar los productos específicos entregados cuando la información esté disponible.

## Impact

- Base de datos SQLite:
  - nueva tabla para detalle de pack por categoría;
  - nuevas columnas opcionales en productos para reglas de reposición;
  - nueva referencia opcional o requerida a producto en `OrdenDetalle` para nuevas entregas.
- Servicios afectados:
  - `DistribucionServicio`;
  - `ProductoServicio`;
  - `HistorialServicio`;
  - `GeneradorDatos`.
- Formularios afectados:
  - `FormDistribucion`;
  - `FormProductos`;
  - nuevo o existente formulario para configurar packs;
  - `FormHistorial`.
- Estructuras propias siguen vigentes: `ListaEnlazada`, `TablaHash`, `MonticuloMaximo` y `Pila` deben seguir utilizándose donde corresponda.
