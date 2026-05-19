# Flujos de uso

Este capitulo explica casos de uso completos. Un caso de uso conecta actor, pantalla, servicio, base de datos y resultado.

## Caso 1 - Configurar categorias

Actor: usuario administrativo.

Precondiciones:

- La app abre en `Form1`.
- El usuario sabe que productos manejara la organizacion.

Pasos:

1. Abrir `Inventario > Gestionar Categorias`.
2. Escribir nombre en `FormCategorias`.
3. Presionar `Guardar`.
4. El formulario valida que el nombre no este vacio.
5. El formulario llama `CategoriaServicio.ExisteNombre`.
6. Si no existe, llama `CategoriaServicio.Guardar`.
7. El servicio ejecuta `INSERT INTO Categorias`.
8. La pantalla recarga el `DataGridView`.

Archivos involucrados:

```text
Form1.cs
FormCategorias.cs
Servicios/CategoriaServicio.cs
Modelos/Categoria.cs
Datos/ConexionDB.cs
```

Resultado esperado:

La categoria queda en SQLite y aparece en la tabla.

Errores posibles:

- Nombre vacio.
- Nombre duplicado.
- Error SQLite.

## Caso 2 - Crear unidad y asociarla a categoria

Actor: usuario administrativo.

Precondiciones:

- Existe al menos una categoria.

Pasos:

1. Abrir `Inventario > Gestionar Unidades`.
2. En grupo `Crear o Editar Unidades`, escribir nombre.
3. Elegir tipo: peso, volumen o unidad.
4. Presionar `Guardar`.
5. Seleccionar una categoria en el grupo derecho.
6. Seleccionar una unidad en la tabla izquierda.
7. Presionar `Vincular a Categoria`.
8. `UnidadServicio.AsociarACategoria` inserta en `CategoriaUnidades`.

Resultado esperado:

La tabla pivote muestra la unidad permitida para esa categoria.

Por que importa:

`FormTasaConsumo` solo permite elegir unidades vinculadas a la categoria seleccionada.

## Caso 3 - Registrar producto con reglas de entrega

Actor: encargado de inventario.

Precondiciones:

- Existe categoria.

Pasos:

1. Abrir `Inventario > Gestionar Productos`.
2. Presionar `Nuevo Producto`.
3. Escribir nombre.
4. Elegir categoria.
5. Escribir stock.
6. Opcionalmente definir `Max. por entrega`.
7. Opcionalmente definir `Dias de espera`.
8. Presionar `Guardar`.
9. Si SKU esta como `(Auto-generado)`, se calcula con `ObtenerSiguienteSKU`.
10. `ProductoServicio.GuardarOSumarStock` inserta o suma stock.

Archivos:

```text
FormProductos.cs
Servicios/ProductoServicio.cs
Modelos/Producto.cs
Estructuras/MonticuloMaximo.cs
Estructuras/TablaHash.cs
```

Resultado esperado:

Producto aparece en la tabla.

Caso especial:

Si el SKU existe con mismo nombre y categoria, el sistema suma stock al existente. Si existe pero no coincide, genera nuevo SKU.

## Caso 4 - Registrar beneficiario con vulnerabilidad

Actor: secretaria.

Precondiciones:

- Se conoce nombre de familia y miembros del hogar.

Pasos:

1. Abrir `Beneficiarios > Padron de Familias`.
2. Ingresar nombre.
3. Ingresar miembros.
4. Elegir vulnerabilidad.
5. Presionar `Registrar`.
6. La UI valida nombre no vacio.
7. La UI revisa duplicados con `TablaHash`.
8. `BeneficiarioServicio.Guardar` inserta en SQLite.
9. El servicio agrega al `ArbolBST` y al indice por ID.

Resultado esperado:

La familia aparece en el padron y participa en distribuciones si `Activo = true`.

Impacto en algoritmo:

La vulnerabilidad cambia la prioridad y los dias iniciales si no tiene entregas previas.

## Caso 5 - Configurar tasa de consumo

Actor: administrador.

Precondiciones:

- Existe categoria.
- La categoria tiene al menos una unidad vinculada.

Pasos:

1. Abrir `Configuracion > Tasas de Consumo (Meta)`.
2. Elegir categoria.
3. La pantalla carga unidades permitidas.
4. Escribir tasa diaria.
5. Elegir unidad base.
6. Presionar `Guardar`.
7. `TasaConsumoServicio.Guardar` ejecuta upsert.

Ejemplo:

```text
Categoria: Granos Basicos
Tasa diaria: 0.4
Unidad base: Libras
```

Interpretacion:

Cada persona consume 0.4 libras por dia de esa categoria.

## Caso 6 - Configurar pack porcentual

Actor: administrador.

Precondiciones:

- Existe categoria.
- Existen productos dentro de esa categoria.

Pasos:

1. Abrir `Inventario > Configurar Packs por Categoria`.
2. Elegir categoria.
3. La tabla carga productos de la categoria.
4. Escribir porcentajes.
5. Verificar que `Total: 100%` este en verde.
6. Presionar `Guardar Pack`.
7. `CategoriaPackServicio.GuardarPack` borra pack anterior e inserta nuevo.

Ejemplo:

```text
Arroz Blanco = 60
Frijol Rojo = 40
Total = 100
```

Resultado esperado:

El algoritmo puede convertir deficit de categoria en productos concretos.

## Caso 7 - Generar propuesta automatica

Actor: encargado de entregas.

Precondiciones:

- Hay beneficiarios activos.
- Hay tasas de consumo.
- Hay packs validos.
- Hay productos con stock.

Pasos:

1. Abrir `Ayuda Social > Generar Asignacion (Reparto)`.
2. Opcionalmente elegir categoria.
3. Presionar `Generar Propuesta Automatica`.
4. `FormDistribucion` llama `DistribucionServicio.GenerarPropuestaDistribucion`.
5. El servicio calcula deficits.
6. El servicio prioriza con `MonticuloMaximo`.
7. El servicio divide cantidades usando pack.
8. La pantalla agrupa por familia.

Resultado esperado:

Se muestran familias y productos sugeridos.

Si no hay propuesta:

`FormDistribucion` llama `ObtenerDiagnosticoSinPropuesta` y muestra causa probable.

## Caso 8 - Ajustar cantidad y deshacer

Actor: encargado de entregas.

Precondiciones:

- Hay propuesta generada.

Pasos:

1. Editar celda `A entregar` de una fila de producto.
2. La UI valida numero y stock.
3. Convierte decimal a entero con `Math.Floor`.
4. Actualiza `propuestaActual`.
5. Guarda accion en `Pila`.
6. Si se presiona `Revertir Cambio Manual`, hace `Pop`.
7. Restaura cantidad anterior.

Archivos:

```text
FormDistribucion.cs
Estructuras/Pila.cs
Modelos/OrdenDetalle.cs
```

Resultado esperado:

El usuario puede corregir el ultimo ajuste antes de confirmar.

## Caso 9 - Confirmar entrega de una familia

Actor: encargado de entregas.

Precondiciones:

- Hay propuesta visible.
- Hay una familia seleccionada.

Pasos:

1. Seleccionar encabezado de familia o producto de esa familia.
2. Presionar `Entregar Familia Seleccionada`.
3. `FormDistribucion` crea una `ListaEnlazada` solo con detalles de esa familia.
4. Abre `FormConfirmacionDistribucion`.
5. Usuario confirma.
6. `DistribucionServicio.ConfirmarDistribucion` inserta orden y detalles.
7. Descuenta stock.
8. Remueve detalles confirmados de la propuesta actual.

Resultado esperado:

Solo esa familia queda confirmada y desaparece de la propuesta pendiente.

## Caso 10 - Confirmar todos los mostrados

Actor: encargado de entregas.

Precondiciones:

- Hay filas visibles.

Pasos:

1. Aplicar filtro si se quiere confirmar solo una categoria visible.
2. Presionar `Entregar Todos Mostrados`.
3. La pantalla crea detalles desde filas visibles.
4. Abre confirmacion.
5. Al confirmar, guarda orden y descuenta stock.

Resultado esperado:

Todas las filas de producto visibles con cantidad mayor a cero quedan confirmadas.

## Caso 11 - Consultar historial

Actor: auditor, secretaria o docente.

Pasos:

1. Abrir `Ayuda Social > Historial de Entregas Realizadas`.
2. La pantalla llama `HistorialServicio.ObtenerHistorialDistribuciones`.
3. Se muestran orden, fecha, familia, categoria, producto, deficit y cantidad.
4. Opcionalmente escribir texto en filtro `Beneficiario`.
5. Presionar `Filtrar`.

Resultado esperado:

El usuario puede auditar que se entrego, a quien y cuando.

## Caso 12 - Simular fecha

Actor: usuario demo o docente.

Pasos:

1. Abrir `Dev / Demo > Simular Fecha`.
2. Elegir fecha futura.
3. Presionar `Aplicar simulada`.
4. Abrir distribucion y generar propuesta.

Resultado esperado:

El deficit aumenta porque `DistribucionServicio` usa `RelojDemo.Ahora`.

## Caso 13 - Sembrar demo pequena

Actor: usuario demo.

Pasos:

1. En `Form1`, abrir `Dev / Demo > Sembrar Demo Pequena`.
2. Confirmar aviso de borrado.
3. `GeneradorDatos.SembrarDemoPequena` limpia tablas.
4. Inserta categorias, unidades, tasas, productos, packs y familias.
5. La aplicacion se reinicia.

Resultado esperado:

El sistema queda listo para demostrar distribucion con pocos datos.
