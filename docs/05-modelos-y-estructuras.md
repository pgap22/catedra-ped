# Modelos y estructuras de datos

Este capitulo explica las clases de datos del dominio y las estructuras propias implementadas en `Estructuras/`. No asume que el lector sepa estructuras de datos.

## Que es un modelo

Un modelo es una clase que representa informacion del negocio. Normalmente no decide que hacer. Solo guarda datos con nombres claros.

Ejemplo simple:

```text
Producto
  SKU: SKU001
  Nombre: Arroz Blanco
  Stock: 40
```

En codigo, eso vive en `Modelos/Producto.cs`.

## `Beneficiario`

Ruta: `Modelos/Beneficiario.cs`

Representa una familia o persona que puede recibir ayuda.

| Campo | Tipo | Explicacion |
|---|---|---|
| `Id` | `int` | Identificador unico en SQLite. |
| `Nombre` | `string` | Nombre de la familia o responsable. |
| `MiembrosHogar` | `int` | Cantidad de personas en el hogar. Afecta el deficit. |
| `Activo` | `bool` | Si es `false`, no participa en distribuciones. |
| `FechaRegistro` | `DateTime` | Fecha de registro. Se carga, pero no define la primera entrega actual. |
| `NivelVulnerabilidad` | `int` | 1 baja, 2 media, 3 alta. |
| `VulnerabilidadTexto` | `string` calculado | Texto legible para UI. |

Constantes reales:

```text
VulnerabilidadBaja  = 1
VulnerabilidadMedia = 2
VulnerabilidadAlta  = 3
```

Ejemplo de objeto:

```json
{
  "Id": 4,
  "Nombre": "Familia Solis",
  "MiembrosHogar": 2,
  "Activo": true,
  "NivelVulnerabilidad": 3,
  "VulnerabilidadTexto": "Alta - Atención prioritaria"
}
```

Metodos importantes:

| Metodo | Que hace |
|---|---|
| `NormalizarNivelVulnerabilidad` | Si el nivel esta fuera de 1 a 3, devuelve media. |
| `ParsearNivelVulnerabilidad` | Convierte texto como `Alta`, `Media`, `Baja` o numeros a nivel. |
| `ObtenerEtiquetaVulnerabilidad` | Devuelve el texto visible para UI. |

## `Producto`

Ruta: `Modelos/Producto.cs`

Representa un item fisico de inventario.

| Campo | Tipo | Explicacion |
|---|---|---|
| `Id` | `int` | Identificador interno de SQLite. |
| `SKU` | `string` | Codigo unico visible, por ejemplo `SKU001`. |
| `Nombre` | `string` | Nombre del producto. |
| `IdCategoria` | `int` | Categoria a la que pertenece. |
| `Stock` | `double` | Cantidad disponible. |
| `MaximoPorEntrega` | `double?` | Limite por familia. `null` significa sin limite. |
| `DiasReposicion` | `int?` | Dias minimos antes de volver a sugerirlo a la misma familia. |
| `NombreCategoria` | `string` | Campo auxiliar para mostrar categoria en UI. |

Ejemplo:

```json
{
  "SKU": "SKU005",
  "Nombre": "Cepillo Dental",
  "IdCategoria": 3,
  "Stock": 20,
  "MaximoPorEntrega": 1,
  "DiasReposicion": 90,
  "NombreCategoria": "Higiene Personal"
}
```

Lectura junior de reglas:

- `MaximoPorEntrega = 1` significa que una familia no deberia recibir mas de 1 unidad de ese producto en una entrega.
- `DiasReposicion = 90` significa que si ya recibio ese producto hace menos de 90 dias, el algoritmo lo omite.

## `Categoria`

Ruta: `Modelos/Categoria.cs`

Representa un grupo de productos. Ejemplos: `Granos Basicos`, `Aceites y Grasas`, `Higiene Personal`.

| Campo | Tipo | Explicacion |
|---|---|---|
| `Id` | `int` | Identificador. |
| `Nombre` | `string` | Nombre visible. |

`ToString()` devuelve `Nombre`. Por eso los `ComboBox` pueden mostrar categorias sin configurar texto manualmente en varios formularios.

## `UnidadMedida`

Ruta: `Modelos/UnidadMedida.cs`

Representa una unidad fisica.

| Campo | Tipo | Explicacion |
|---|---|---|
| `Id` | `int` | Identificador. |
| `Nombre` | `string` | Ejemplo: `Libras`, `Litros`, `Unidades`. |
| `Tipo` | `string` | Ejemplo: `Peso`, `Volumen`, `Unidad`. |

`ToString()` devuelve:

```text
Nombre (Tipo)
```

Ejemplo:

```text
Libras (Peso)
```

## `TasaConsumo`

Ruta: `Modelos/TasaConsumo.cs`

Define cuanto consume una persona por dia de una categoria.

| Campo | Tipo | Explicacion |
|---|---|---|
| `IdCategoria` | `int` | Categoria a la que aplica. |
| `NombreCategoria` | `string` | Nombre para mostrar en UI. |
| `TasaDiaria` | `double` | Consumo por persona por dia. |
| `IdUnidadBase` | `int` | Unidad usada para expresar la tasa. |
| `NombreUnidadBase` | `string` | Nombre visible de la unidad. |

Ejemplo:

```json
{
  "NombreCategoria": "Granos Basicos",
  "TasaDiaria": 0.4,
  "NombreUnidadBase": "Libras"
}
```

Interpretacion:

```text
Una persona consume 0.4 libras de granos basicos por dia.
```

## `CategoriaPackDetalle`

Ruta: `Modelos/CategoriaPackDetalle.cs`

Representa una linea del pack porcentual de una categoria.

| Campo | Tipo | Explicacion |
|---|---|---|
| `Id` | `int` | Identificador de la linea. |
| `CategoriaId` | `int` | Categoria del pack. |
| `NombreCategoria` | `string` | Nombre visible. |
| `ProductoId` | `int` | Producto incluido. |
| `SKUProducto` | `string` | SKU visible. |
| `NombreProducto` | `string` | Producto visible. |
| `Porcentaje` | `double` | Parte de la categoria que se entrega como este producto. |
| `StockDisponible` | `double` | Stock actual del producto. |
| `MaximoPorEntrega` | `double?` | Regla heredada del producto. |
| `DiasReposicion` | `int?` | Regla heredada del producto. |

Ejemplo grafico:

```text
Categoria: Granos Basicos

+----------------+------------+
| Producto       | Porcentaje |
+----------------+------------+
| Arroz Blanco   | 60%        |
| Frijol Rojo    | 40%        |
+----------------+------------+
```

Si la familia debe recibir 10 unidades de la categoria:

```text
Arroz  = floor(10 * 60 / 100) = 6
Frijol = floor(10 * 40 / 100) = 4
```

## `Orden`

Ruta: `Modelos/Orden.cs`

Representa una entrega confirmada como evento general.

| Campo | Tipo | Explicacion |
|---|---|---|
| `Id` | `int` | Numero de orden. |
| `FechaGeneracion` | `DateTime` | Momento de confirmacion. |
| `Estado` | `string` | Por defecto `BORRADOR`, al confirmar se guarda `CONFIRMADA`. |
| `Observaciones` | `string` | Texto libre del usuario. |

## `OrdenDetalle`

Ruta: `Modelos/OrdenDetalle.cs`

Representa una linea concreta de ayuda.

| Campo | Tipo | Explicacion |
|---|---|---|
| `Id` | `int` | Identificador. |
| `OrdenId` | `int` | Orden padre. |
| `BeneficiarioId` | `int` | Familia que recibe. |
| `NombreBeneficiario` | `string` | Nombre visible en UI. |
| `NivelVulnerabilidad` | `int` | Nivel numerico usado para prioridad. |
| `VulnerabilidadTexto` | `string` | Texto visible. |
| `CategoriaId` | `int` | Categoria distribuida. |
| `NombreCategoria` | `string` | Nombre visible. |
| `ProductoId` | `int` | Producto fisico. |
| `CantidadAsignada` | `double` | Cantidad sugerida o confirmada. |
| `DeficitCalculado` | `double` | Necesidad calculada. |
| `ExplicacionCalculo` | `string` | Texto largo que explica el calculo. |
| `SKUProductoSugerido` | `string` | SKU mostrado en propuesta. |
| `NombreProductoSugerido` | `string` | Producto mostrado en propuesta. |
| `NombreUnidadMedida` | `string` | Unidad visible. |

Ejemplo:

```json
{
  "NombreBeneficiario": "Familia Solis",
  "VulnerabilidadTexto": "Alta - Atención prioritaria",
  "NombreCategoria": "Granos Basicos",
  "NombreProductoSugerido": "Arroz Blanco",
  "CantidadAsignada": 6,
  "DeficitCalculado": 11.2,
  "NombreUnidadMedida": "Libras"
}
```

## `AccionUndo`

Ruta: `Modelos/AccionUndo.cs`

Representa una accion que puede deshacerse.

Tipos:

```text
Insertar
Editar
Eliminar
Importacion
```

Campos:

| Campo | Explicacion |
|---|---|
| `Tipo` | Que clase de accion fue. |
| `Tabla` | Nombre logico de la entidad afectada. |
| `Datos` | Objeto necesario para revertir. |

## Que es una estructura de datos

Una estructura de datos es una forma organizada de guardar informacion para resolver operaciones de manera mas clara o eficiente.

Ejemplo cotidiano:

```text
Una pila de platos:
  el ultimo plato que pones arriba
  es el primero que quitas
```

Eso en programacion se llama `Pila` o `Stack`.

## `ListaEnlazada`

Rutas:

- `Estructuras/ListaEnlazada.cs`
- `Estructuras/NodoLista.cs`

Una lista enlazada es una cadena de nodos. Cada nodo guarda un valor y una referencia al siguiente nodo.

```text
cabeza
  |
  v
[valor A | sig] -> [valor B | sig] -> [valor C | null]
```

Campos internos:

| Campo | Explicacion |
|---|---|
| `cabeza` | Primer nodo. |
| `cola` | Ultimo nodo, permite agregar rapido al final. |
| `contador` | Cantidad de elementos. |

Metodos:

| Metodo | Que hace |
|---|---|
| `Agregar(object valor)` | Inserta al final. |
| `Obtener(int indice)` | Recorre desde la cabeza hasta el indice. |
| `Conteo()` | Devuelve cuantos elementos hay. |
| `EstaVacia()` | Dice si no hay elementos. |
| `EliminarPrimero(Predicate<object>, out object?)` | Elimina el primer elemento que cumple una condicion. |
| `ParaCada(Action<object>)` | Ejecuta una accion por elemento. |
| `Limpiar()` | Vacía la lista. |

Usos reales:

- Servicios devuelven `ListaEnlazada` en vez de `List<T>`.
- `DistribucionServicio` devuelve propuesta como `ListaEnlazada`.
- CSV devuelve registros parseados en `ListaEnlazada`.
- `FormDistribucion` mantiene propuesta actual como `ListaEnlazada`.

Complejidad conceptual:

| Operacion | Costo aproximado |
|---|---|
| Agregar al final | O(1), porque existe `cola`. |
| Obtener por indice | O(n), porque debe caminar nodo por nodo. |

## `Pila`

Rutas:

- `Estructuras/Pila.cs`
- `Estructuras/NodoPila.cs`

Una pila funciona con regla LIFO: ultimo en entrar, primero en salir.

```text
tope
  |
  v
[ultimo cambio]
[cambio anterior]
[primer cambio]
```

Metodos:

| Metodo | Que hace |
|---|---|
| `Empujar(object valor)` | Pone un elemento arriba. |
| `Pop()` | Saca el elemento de arriba. |
| `EstaVacia()` | Indica si no hay elementos. |
| `Conteo()` | Devuelve cantidad. |

Usos reales:

- `FormCategorias`: deshacer guardar, editar, eliminar e importacion.
- `FormUnidades`: deshacer cambios de unidades.
- `FormProductos`: deshacer cambios de inventario.
- `FormBeneficiarios`: deshacer cambios de familias.
- `FormDistribucion`: revertir cambios manuales en cantidades antes de confirmar.

## `TablaHash`

Rutas:

- `Estructuras/TablaHash.cs`
- `Estructuras/NodoHash.cs`

Una tabla hash guarda pares clave-valor. La clave se transforma en un indice de arreglo mediante una funcion hash.

```text
Clave: "SKU001"
  |
  v
Funcion hash
  |
  v
Indice 37
  |
  v
tabla[37] -> Producto Arroz
```

Funcion hash real:

```text
hash inicia en 17
por cada caracter: hash = hash * 31 + caracter
indice = abs(hash) % tamaño
```

Colisiones:

Si dos claves caen en el mismo indice, se encadenan con `NodoHash.Siguiente`.

```text
tabla[5] -> [SKU001] -> [SKU999] -> null
```

Metodos:

| Metodo | Que hace |
|---|---|
| `Insertar(clave, valor)` | Agrega o reemplaza por clave. |
| `Buscar(clave)` | Devuelve valor si existe. |
| `Eliminar(clave)` | Quita la clave. |
| `ParaCada(accion)` | Recorre todos los pares. |

Usos reales:

- `ProductoServicio`: indice por SKU.
- `CategoriaServicio.ExisteNombre`: deteccion de nombres existentes.
- `FormBeneficiarios`: nombres duplicados.
- `FormUnidades`: duplicados por nombre y tipo.
- `FormConfigurarPacks`: porcentajes existentes por producto.
- `FormDistribucion`: stock por producto para validar sobregiro.

## `ArbolBST`

Rutas:

- `Estructuras/ArbolBST.cs`
- `Estructuras/NodoArbol.cs`

Un BST es un arbol binario de busqueda. Cada nodo tiene una llave. Las llaves menores van a la izquierda y las mayores a la derecha.

```text
              Marta
             /     \
          Ana       Pedro
            \
            Carlos
```

En este proyecto, la llave es el nombre del beneficiario.

Metodos:

| Metodo | Que hace |
|---|---|
| `Insertar(llave, valor)` | Inserta por nombre. |
| `Buscar(llave)` | Busca coincidencia exacta. |
| `BuscarParcial(fragmento)` | Recorre y devuelve nombres que contienen texto. |
| `Eliminar(llave)` | Elimina nodo por llave. |
| `EliminarValor(llave, criterio)` | Elimina un valor especifico en una llave. |
| `ObtenerInOrder()` | Devuelve datos ordenados alfabeticamente. |

Uso real:

- `BeneficiarioServicio` carga beneficiarios en `ArbolBST`.
- `FormBeneficiarios` usa `BuscarParcial` para busqueda por nombre.

Limitacion:

Si se insertan nombres ya ordenados, un BST simple puede degenerar en lista. No es AVL ni balanceado.

## `MonticuloMaximo`

Rutas:

- `Estructuras/MonticuloMaximo.cs`
- `Estructuras/ElementoHeap.cs`

Un monticulo maximo mantiene arriba el elemento con mayor prioridad.

Vista como arbol:

```text
          100
        /     \
      80       60
     /  \
   20    50
```

Vista como arreglo interno:

```text
indice:  0    1    2    3    4
valor: 100   80   60   20   50
```

Regla de indices:

```text
padre de i = (i - 1) / 2
hijo izquierdo = 2 * i + 1
hijo derecho   = 2 * i + 2
```

Metodos:

| Metodo | Que hace |
|---|---|
| `Insertar(decimal prioridad, object valor)` | Agrega y sube hasta respetar prioridad. |
| `ExtraerMaximo()` | Saca el elemento de mayor prioridad y reordena. |
| `VerMaximo()` | Mira el maximo sin sacarlo. |
| `Conteo()` | Cantidad de elementos. |

Usos reales:

- `DistribucionServicio.GenerarPropuestaDistribucion`: prioriza familias.
- `FormProductos.ObtenerSiguienteSKU`: encuentra el mayor correlativo de SKU.

Complejidad:

| Operacion | Costo aproximado |
|---|---|
| Insertar | O(log n) |
| Extraer maximo | O(log n) |
| Ver maximo | O(1) |
