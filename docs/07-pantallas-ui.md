# Pantallas y UI

Este capitulo explica cada formulario WinForms: que muestra, que botones tiene, que servicios usa y que validaciones ejecuta.

## `Form1` - menu principal

Ruta: `Form1.cs`

Responsabilidad:

- Ser la pantalla inicial.
- Mostrar menu superior.
- Abrir formularios secundarios.
- Sembrar datos demo desde menu `Dev / Demo`.

Visual aproximado:

```text
+-----------------------------------------------------------+
| Configuracion | Inventario | Beneficiarios | Ayuda Social |
+-----------------------------------------------------------+
|                                                           |
|                 SISTEMA DE DONACIONES                     |
|                                                           |
|      Seleccione una opcion del menu superior              |
|                                                           |
+-----------------------------------------------------------+
```

Colores y layout:

- Fondo principal: azul claro `Color.FromArgb(241, 246, 252)`.
- Menu superior: azul oscuro `Color.FromArgb(32, 62, 92)`.
- Titulo central: `Segoe UI`, 28, bold.
- Tamaño inicial: `1000 x 560`.

Acciones:

| Menu | Abre |
|---|---|
| `Gestionar Categorias` | `FormCategorias` |
| `Gestionar Unidades` | `FormUnidades` |
| `Gestionar Productos` | `FormProductos` |
| `Configurar Packs por Categoria` | `FormConfigurarPacks` |
| `Padron de Familias` | `FormBeneficiarios` |
| `Tasas de Consumo (Meta)` | `FormTasaConsumo` |
| `Generar Asignacion (Reparto)` | `FormDistribucion` |
| `Historial de Entregas Realizadas` | `FormHistorial` |
| `Simular Fecha` | `FormDevFecha` |

## `FormCategorias` - gestion de categorias

Ruta: `FormCategorias.cs`

Servicio: `CategoriaServicio`.

Estructuras: `Pila`, `TablaHash` indirectamente en servicio e importacion.

Responsabilidad:

- Crear categorias.
- Modificar nombre.
- Eliminar categorias.
- Buscar por texto.
- Importar CSV.
- Descargar plantilla CSV.
- Deshacer ultimo cambio manual.

UI aproximada:

```text
[Nuevo]

Nombre: [________________] [Guardar] [Modificar] [Eliminar]

Buscar: [________________________] [Buscar]

+--------------------------------+
| ID | Nombre                     |
+--------------------------------+
| 1  | Granos Basicos             |
| 2  | Higiene Personal           |
+--------------------------------+

[Deshacer ultimo cambio manual] [Importar CSV] [Bajar Plantilla]
```

Validaciones:

| Accion | Validacion |
|---|---|
| Guardar | Nombre obligatorio. |
| Guardar | No permite nombre duplicado. |
| Modificar | Nombre obligatorio. |
| Modificar | No permite duplicado salvo el mismo registro. |
| Eliminar | Pide confirmacion. |
| Importar | Omite vacios y duplicados. |

Estados:

- `btnEditar` y `btnEliminar` empiezan deshabilitados.
- Se habilitan cuando el usuario selecciona una fila.

Plantilla CSV:

```csv
Nombre
Granos Basicos
Lacteos
Aceites
```

## `FormUnidades` - unidades y asociacion con categorias

Ruta: `FormUnidades.cs`

Servicios:

- `UnidadServicio`
- `CategoriaServicio`

Responsabilidad:

- Crear, modificar y eliminar unidades.
- Asociar unidad permitida a una categoria.
- Quitar vinculo categoria-unidad.
- Ver tabla pivote.
- Importar CSV.
- Deshacer cambios de unidades.

UI aproximada:

```text
+----------------------------------+  +----------------------------------+
| 1. Crear o Editar Unidades       |  | 2. Asignar Unidad a Categoria    |
| [Nuevo]                          |  | Categoria: [Granos Basicos v]    |
| Nombre: [________] Tipo: [____]  |  | [Vincular a Categoria]           |
| [Guardar] [Modificar] [Eliminar] |  | [Quitar Vinculo]                 |
+----------------------------------+  +----------------------------------+

Buscar: [________________] [Buscar]

+-----------------------------+    +-------------------------------+
| ID | Nombre | Tipo          |    | Categoria | Unidad Permitida |
+-----------------------------+    +-------------------------------+

[Deshacer ultimo cambio manual] [Importar CSV] [Bajar Plantilla]
```

Tipos de unidad reales en combo:

```text
Peso (lb/kg)
Volumen (lt/ml)
Unidad (pza/bolsa)
```

Validaciones:

- Nombre obligatorio.
- Tipo obligatorio.
- No permite duplicado por combinacion `nombre|tipo`.
- Para vincular, exige categoria y unidad seleccionadas.

Plantilla CSV:

```csv
Nombre,Tipo
Libra,Peso (lb/kg)
Litro,Volumen (lt/ml)
Bolsa,Unidad (pza/bolsa)
```

## `FormProductos` - inventario

Ruta: `FormProductos.cs`

Servicios:

- `ProductoServicio`
- `CategoriaServicio`

Estructuras:

- `TablaHash` en `ProductoServicio`.
- `MonticuloMaximo` para generar siguiente SKU.
- `Pila` para deshacer.

Responsabilidad:

- Crear productos.
- Editar productos.
- Eliminar productos.
- Importar productos desde CSV.
- Generar SKU automatico.
- Mostrar stock bajo.
- Configurar reglas de entrega por producto.

UI aproximada:

```text
[Nuevo Producto]

SKU: [(Auto-generado)] Nombre: [________]
Categoria: [________ v] Stock: [__]
Max. por entrega: [__] Dias de espera: [__] [Guardar]

Buscar: [________________] [Buscar]             [Eliminar]

+----------------------------------------------------------------+
| SKU | Nombre | Categoria | Stock | Max. por entrega | Dias     |
+----------------------------------------------------------------+

[Deshacer ultimo cambio manual] [Importar CSV] [Bajar Plantilla]
0 en reglas = sin limite/sin espera. Filas rojas = stock bajo.
```

Validaciones:

- Nombre obligatorio.
- Categoria obligatoria.
- En importacion, SKU, nombre, categoria y stock deben ser validos.
- Stock negativo se omite en importacion.

Reglas de UI:

- `SKU` es readonly.
- Si `Max. por entrega = 0`, se guarda como `null`, o sea sin limite.
- Si `Dias de espera = 0`, se guarda como `null`, o sea sin reposicion.
- Stock `<= 5` pinta fila `LightCoral`.

Generacion de SKU:

```text
Lista productos existentes
  |
  +--> extrae numero de cada SKU
  |
  +--> inserta numero en MonticuloMaximo
  |
  +--> VerMaximo()
  |
  +--> nuevo SKU = SKU(max + 1 con 3 digitos)
```

Ejemplo:

```text
SKU001, SKU002, SKU010
max = 10
nuevo = SKU011
```

Plantilla CSV:

```csv
SKU,Nombre,NombreCategoria,Stock
SKU001,Arroz,Granos Basicos,50
SKU002,Leche,Lacteos,100
```

## `FormBeneficiarios` - padron de familias

Ruta: `FormBeneficiarios.cs`

Servicio: `BeneficiarioServicio`.

Estructuras:

- `ArbolBST` para busqueda parcial.
- `TablaHash` para duplicados.
- `Pila` para deshacer.

Responsabilidad:

- Registrar familias.
- Modificar familias.
- Eliminar familias.
- Buscar por nombre.
- Definir miembros del hogar.
- Definir vulnerabilidad.
- Importar CSV.

UI aproximada:

```text
[Nuevo]

Nombre: [______________] Miembros: [__]
Nivel de vulnerabilidad: [Alta/Media/Baja v]
[Registrar] [Modificar] [Eliminar]

+---------------------------------------------------------+
| Buscar Beneficiario (por Nombre)                        |
| [________________________] [Buscar]                     |
+---------------------------------------------------------+

+---------------------------------------------------------+
| ID | Nombre | Miembros | Vulnerabilidad                 |
+---------------------------------------------------------+

[Deshacer ultimo cambio manual] [Importar CSV] [Bajar Plantilla]
```

Validaciones:

- Nombre obligatorio.
- No permite nombre duplicado normalizado con `Trim().ToLower()`.
- Miembros minimo 1 y maximo 999.
- Nivel desconocido en CSV cae a media.

Niveles visibles:

```text
Alta - Atención prioritaria
Media - Apoyo regular
Baja - Apoyo preventivo
```

Plantilla CSV:

```csv
Nombre,Miembros,NivelVulnerabilidad
Juan Perez,5,Media
Maria Lopez,3,Alta
Familia Solis,2,Baja
```

## `FormTasaConsumo` - tasas por categoria

Ruta: `FormTasaConsumo.cs`

Servicios:

- `TasaConsumoServicio`
- `CategoriaServicio`
- `UnidadServicio`

Responsabilidad:

- Definir cuanto consume una persona por dia de una categoria.
- Elegir unidad base entre unidades permitidas de la categoria.
- Listar tasas configuradas.
- Eliminar tasa.

UI aproximada:

```text
Categoria: [Granos Basicos v]
Tasa Diaria: [0.4]  Unidad Base: [Libras v]

[Guardar] [Eliminar] [Limpiar]

Nota: Define cuanto consume 1 persona en 1 dia de esta categoria.

+--------------------------------------------------+
| Categoria | Tasa Diaria | Unidad Base           |
+--------------------------------------------------+
```

Validaciones:

- Categoria obligatoria.
- Unidad base obligatoria.
- Tasa obligatoria.
- Tasa debe ser numero valido mayor a 0.

Comportamiento importante:

- Al seleccionar categoria, carga unidades permitidas con `UnidadServicio.ListarPorCategoria`.
- Al editar una tasa existente, la categoria se deshabilita porque `IdCategoria` es la clave primaria.
- Al activar el formulario, recarga categorias y tasas.

## `FormConfigurarPacks` - packs porcentuales

Ruta: `FormConfigurarPacks.cs`

Servicios:

- `CategoriaServicio`
- `ProductoServicio`
- `CategoriaPackServicio`

Responsabilidad:

- Elegir una categoria.
- Ver productos de esa categoria.
- Asignar porcentaje a cada producto.
- Guardar pack si suma 100%.
- Eliminar pack.

UI aproximada:

```text
Categoria: [Granos Basicos v]   Total: 100%   [Guardar Pack] [Eliminar Pack]

+------------------------------------------------------+
| SKU    | Producto      | Stock | Porcentaje (%)       |
+------------------------------------------------------+
| SKU001 | Arroz Blanco  | 40    | 60                   |
| SKU002 | Frijol Rojo   | 25    | 40                   |
+------------------------------------------------------+

Ingrese porcentajes enteros o decimales. El total debe ser exactamente 100% para guardar.
```

Validaciones:

- El boton `Guardar Pack` solo se habilita si el total es 100 con tolerancia 0.001.
- El servicio vuelve a validar antes de guardar.
- Porcentajes `<= 0` se ignoran al persistir.

## `FormDistribucion` - asignacion de ayuda

Ruta: `FormDistribucion.cs`

Servicios:

- `DistribucionServicio`
- `CategoriaServicio`

Estructuras:

- `ListaEnlazada` para propuesta actual.
- `Pila` para deshacer ediciones.
- `TablaHash` para stock por producto.
- `HashSet<int>` para familias colapsadas, auxiliar de UI.
- `Dictionary<int, List<OrdenDetalle>>` para agrupar, auxiliar de UI.

Responsabilidad:

- Generar propuesta automatica.
- Filtrar por categoria.
- Mostrar familias agrupadas.
- Expandir y colapsar familias.
- Editar cantidades.
- Revertir cambios manuales.
- Confirmar una familia.
- Confirmar todos los productos mostrados.
- Mostrar explicacion del calculo.

UI aproximada:

```text
[Generar Propuesta Automatica] [Revertir Cambio Manual]
Observaciones: [________________________________]

Categoria: [[Todas las categorias] v] [Aplicar Filtros] [Limpiar Filtros]
[Expandir todo] [Colapsar todo]

                         [Entregar Familia Seleccionada] [Entregar Todos Mostrados]

+----------------------------------------------------------------------------+
| Familia / Producto | Categoria | SKU | A entregar | Unidad | Deficit       |
+----------------------------------------------------------------------------+
| v Familia Solis - Alta - Atencion prioritaria | 2 productos                 |
|    Arroz Blanco    | Granos    | SKU001 | 6       | Libras | 11.2          |
|    Frijol Rojo     | Granos    | SKU002 | 4       | Libras | 11.2          |
+----------------------------------------------------------------------------+
```

Columnas visibles:

```text
Detalle
Categoria
SKU
Asignado
Unidad
Deficit
```

Columnas ocultas:

```text
BId
CId
PId
NivelVulnerabilidad
Explicacion
Beneficiario
Vulnerabilidad
Producto
EsGrupo
```

Interacciones:

| Accion | Resultado |
|---|---|
| `Generar Propuesta Automatica` | Llama `GenerarPropuestaDistribucion`. |
| `Aplicar Filtros` | Muestra solo categoria seleccionada. |
| `Limpiar Filtros` | Vuelve a todas. |
| Doble clic en familia | Expande o colapsa. |
| Doble clic en producto fuera de `A entregar` | Muestra explicacion del calculo. |
| Editar `A entregar` | Valida numero, no negativo, entero y stock. |
| `Revertir Cambio Manual` | Usa `Pila` para restaurar cantidad anterior. |
| `Entregar Familia Seleccionada` | Confirma solo una familia. |
| `Entregar Todos Mostrados` | Confirma filas visibles. |

Validacion de edicion:

```text
No numero -> revierte
Negativo -> revierte
Decimal -> aplica Math.Floor
Mayor que stock -> revierte
```

## `FormConfirmacionDistribucion` - confirmacion final

Ruta: `FormConfirmacionDistribucion.cs`

Responsabilidad:

- Mostrar resumen antes de descontar stock.
- Permitir cancelar.
- Permitir confirmar y devolver `DialogResult.OK`.

UI aproximada:

```text
Entrega para familia: Familia Solis

Revise la entrega antes de confirmar. Se descontaran 10 unidades del stock real.

+---------------------------------------------------------------------+
| Familia | Vulnerabilidad | Categoria | Producto | SKU | Cantidad |
+---------------------------------------------------------------------+

[Confirmar y descontar stock] [Cancelar]
```

Reglas:

- Redondea cantidades con `Math.Floor`.
- Ignora cantidades `<= 0`.
- Deshabilita confirmar si no hay filas.

## `FormHistorial` - historial de entregas

Ruta: `FormHistorial.cs`

Servicio: `HistorialServicio`.

Responsabilidad:

- Mostrar entregas confirmadas.
- Filtrar por beneficiario.
- Refrescar datos.

UI aproximada:

```text
[Refrescar] Beneficiario: [____________] [Filtrar]  Asignaciones mostradas: N

+--------------------------------------------------------------------------------+
| Orden | Fecha | Beneficiario | Categoria | Producto | Deficit | Asignado | Unidad |
+--------------------------------------------------------------------------------+
```

Columnas:

```text
Nº Orden
Fecha
Beneficiario
Categoria
Producto
Deficit Previo
Asignado
Unidad
```

Si el producto historico viene vacio, muestra:

```text
(historico por categoria)
```

## `FormDevFecha` - fecha demo

Ruta: `FormDevFecha.cs`

Utilidad: `RelojDemo`.

Responsabilidad:

- Simular fecha del sistema para pruebas.
- Volver a fecha real.

UI aproximada:

```text
Fecha usada por la demo

[18/05/2026 14:30]

[Aplicar simulada] [Usar fecha real]

Modo actual: fecha simulada. Ahora = 18/05/2026 14:30
```

Importancia:

El algoritmo usa `RelojDemo.Ahora`. Si el usuario avanza la fecha demo, aumenta el deficit de familias sin entregas recientes.
