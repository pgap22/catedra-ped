# Riesgos, limitaciones y testing

Este capitulo documenta riesgos reales observados en el codigo. No significa que el sistema no funcione; significa que hay puntos a conocer antes de mantenerlo o defenderlo tecnicamente.

## No se encontraron tests automatizados

No se observaron proyectos o carpetas de pruebas automatizadas.

Riesgo:

```text
El algoritmo de distribucion tiene muchas reglas.
Sin pruebas, un cambio pequeño puede romper prioridad, stock o historial.
```

Pruebas minimas recomendadas:

| Area | Caso |
|---|---|
| Deficit | Familia sin historial usa dias iniciales por vulnerabilidad. |
| Deficit | Familia con historial usa dias desde ultima entrega. |
| Prioridad | Alta gana sobre media aunque tenga menor deficit. |
| Heap | `ExtraerMaximo` devuelve mayor prioridad. |
| Pack | Pack 60/40 divide con `floor`. |
| Pack | Pack que no suma 100 falla. |
| Stock | No se genera linea si stock es cero. |
| Reposicion | Producto recibido hace menos dias se omite. |
| Maximo | `MaximoPorEntrega` limita cantidad. |
| Confirmacion | Stock insuficiente hace rollback. |
| Historial | Confirmacion aparece en `FormHistorial`. |

## Migraciones simples con `catch` vacio

Ruta: `Datos/ConexionDB.cs`

Ejemplo:

```csharp
try { ALTER TABLE ... } catch { /* Ya existe */ }
```

Riesgo:

```text
El catch vacio tambien ocultaria errores distintos a "columna ya existe".
```

Como explicarlo:

Para un proyecto academico, es una migracion defensiva simple. Para produccion, convendria verificar el esquema o capturar errores especificos.

## Ruta relativa de `donaciones.db`

Ruta: `Datos/ConexionDB.cs`

La base se define como:

```text
donaciones.db
```

Riesgo:

Si se ejecuta desde otra carpeta, SQLite puede crear otra base vacia.

Sintoma:

```text
El sistema abre pero no aparecen datos esperados.
```

Mitigacion documental:

Ejecutar siempre desde la raiz esperada o definir ruta fija al empaquetar.

## Servicios con indices en memoria

Rutas:

```text
Servicios/ProductoServicio.cs
Servicios/BeneficiarioServicio.cs
```

Ejemplos:

```text
ProductoServicio -> TablaHash indicePorSku
BeneficiarioServicio -> ArbolBST arbolPorNombre + TablaHash indicePorId
```

Riesgo:

Si dos formularios estan abiertos y uno modifica datos, el otro puede tener una instancia con datos cargados antes.

Ejemplo:

```text
FormProductos abierto
Otro flujo cambia productos
FormProductos podria necesitar recargar o reabrirse
```

## `FechaRegistro` no define primera entrega

Ruta:

```text
Modelos/Beneficiario.cs
Servicios/DistribucionServicio.cs
```

El modelo tiene `FechaRegistro`, pero `CalcularDeficit` usa dias iniciales fijos cuando no hay historial:

```text
Alta 14
Media 7
Baja 4
```

Riesgo:

Un lector podria pensar que una familia registrada hace 30 dias acumula 30 dias de deficit. En la implementacion actual no es asi para primera entrega.

## Redondeo con `Math.Floor`

Ruta:

```text
Servicios/DistribucionServicio.cs
```

El algoritmo redondea hacia abajo:

```text
floor(2.9) = 2
floor(0.8) = 0
```

Ventaja:

- Evita entregar fracciones dificiles de manejar.
- Evita sobreasignar.

Riesgo:

- Puede subasignar.
- Puede quedar stock sin entregar por residuos del pack.

Ejemplo:

```text
Cantidad categoria = 2
Pack 60/40

Producto A = floor(1.2) = 1
Producto B = floor(0.8) = 0

Total entregado = 1 de 2
```

## Residuos del pack no se reasignan

El sistema no intenta redistribuir sobrantes generados por redondeo.

Ejemplo:

```text
Cantidad categoria = 11
Pack 60/40

Arroz  = 6
Frijol = 4
Total = 10
Residuo = 1
```

Decision observada:

```text
Regla simple, auditable y conservadora.
```

## CSV simple, no parser formal

Ruta: `Utilidades/ManejadorCSV.cs`

Riesgo:

No soporta campos con coma interna.

Mitigacion:

Documentar que las plantillas deben usar valores simples sin comas dentro de campos.

## Modelo `Producto` no expone `IdUnidad`

Tabla `Productos` tiene campo:

```text
IdUnidad
```

Modelo `Producto` no tiene propiedad `IdUnidad`.

Riesgo:

La base soporta unidad por producto, pero la logica actual muestra y calcula distribucion usando unidad base de tasa.

Como defenderlo:

El diseño actual simplifica inventario asumiendo que stock se maneja en unidad compatible con la tasa de la categoria.

## Stock total de categoria vs stock de pack

`DistribucionServicio.ObtenerStockTotalCategoria` suma todos los productos de una categoria.

Pero `GenerarPropuestaDistribucion` usa:

```text
ObtenerStockTotalPack(pack)
```

Riesgo:

Puede existir stock en una categoria que no se use porque el producto no esta incluido en el pack.

Ejemplo:

```text
Categoria Granos:
  Arroz en pack stock 10
  Harina fuera de pack stock 100

Distribucion automatica usa 10, no 110.
```

## Compatibilidad legacy en confirmacion

Si `OrdenDetalle.ProductoId <= 0`, `ConfirmarDistribucion` descuenta por categoria usando productos por ID.

Riesgo:

Puede complicar auditoria si hubiera datos antiguos sin producto especifico.

Estado actual:

La generacion moderna crea lineas con `ProductoId`.

## No hay capa de repositorio abstracta

Los servicios mezclan:

- SQL.
- Reglas de negocio.
- Estructuras en memoria.

Ejemplo:

```text
ProductoServicio
  SQL + TablaHash + reglas de stock
```

Para el alcance academico, es defendible. Para una app grande, se separaria mas para testear.

## Checklist manual de regresion

Antes de entregar o defender, ejecutar manualmente:

1. Compilar con `dotnet build`.
2. Sembrar demo pequena.
3. Abrir categorias, unidades, productos y beneficiarios.
4. Confirmar que las tablas cargan.
5. Generar propuesta.
6. Editar una cantidad a `999` y verificar rechazo por stock.
7. Editar una cantidad valida y usar deshacer.
8. Confirmar una familia.
9. Verificar stock del producto bajo.
10. Abrir historial y ver entrega.
11. Simular fecha futura y generar nueva propuesta.

## Pruebas unitarias sugeridas por clase

| Clase | Prueba sugerida |
|---|---|
| `ListaEnlazada` | Agregar 3 elementos, obtener por indice, eliminar primero. |
| `Pila` | Empujar A, B y verificar que `Pop` devuelve B. |
| `TablaHash` | Insertar, buscar, reemplazar y eliminar clave. |
| `ArbolBST` | Insertar nombres, buscar parcial, obtener in-order. |
| `MonticuloMaximo` | Insertar prioridades y extraer en orden descendente. |
| `CategoriaPackServicio` | Validar suma 100 y fallo por suma 90. |
| `DistribucionServicio` | Dataset pequeño con prioridad por vulnerabilidad. |

## Riesgos de defensa tecnica

Preguntas que el equipo debe poder responder:

- Donde se calcula el deficit.
- Por que se usa heap maximo.
- Como se evita stock negativo.
- Que pasa si el pack no suma 100.
- Que significa vulnerabilidad alta.
- Por que se redondea hacia abajo.
- Donde queda el historial.
- Que estructuras son propias y donde se usan realmente.
