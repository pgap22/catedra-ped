# Glosario tecnico y de negocio

Este glosario funciona como diccionario del proyecto.

## Actor

Persona que usa el sistema en un caso de uso.

Ejemplo:

```text
Secretaria, encargado de inventario, auditor.
```

## Aplicacion offline

Aplicacion que funciona sin internet. Este sistema usa SQLite local, por eso no necesita servidor externo.

## Arbol BST

Estructura de datos donde cada nodo tiene hasta dos hijos. Los valores menores van a la izquierda y los mayores a la derecha.

En el proyecto:

```text
Estructuras/ArbolBST.cs
```

Uso:

```text
Buscar beneficiarios por nombre.
```

## Beneficiario

Familia o persona que puede recibir ayuda.

Modelo:

```text
Modelos/Beneficiario.cs
```

Campos importantes:

```text
Nombre
MiembrosHogar
Activo
NivelVulnerabilidad
```

## Categoria

Grupo logico de productos.

Ejemplos:

```text
Granos Basicos
Aceites y Grasas
Higiene Personal
```

## Categoria-unidad

Relacion que indica que unidades son validas para una categoria.

Ejemplo:

```text
Granos Basicos -> Libras
Aceites y Grasas -> Litros
```

Tabla:

```text
CategoriaUnidades
```

## CRUD

Siglas de crear, leer, actualizar y eliminar.

Ejemplo:

```text
FormCategorias permite CRUD de categorias.
```

## DataGridView

Control de WinForms para mostrar tablas.

Ejemplo:

```text
FormProductos muestra productos en un DataGridView.
```

## Deficit

Necesidad acumulada calculada por el sistema.

Formula:

```text
deficit = miembros del hogar * tasa diaria * dias
```

Ejemplo:

```text
5 personas * 0.4 libras * 7 dias = 14 libras
```

## Dias de reposicion

Cantidad de dias que deben pasar antes de sugerir otra entrega del mismo producto a la misma familia.

Ejemplo:

```text
Cepillo Dental con reposicion 90 dias.
```

## Entrega confirmada

Entrega que el usuario acepto y que ya desconto stock.

Se guarda en:

```text
Orden
OrdenDetalle
```

## FIFO

First In, First Out. Primero en entrar, primero en salir.

En el proyecto aparece solo como compatibilidad legacy para descontar productos por categoria cuando no hay `ProductoId` especifico.

## Foreign key

Clave foranea. Campo que apunta a otra tabla.

Ejemplo:

```text
Productos.IdCategoria apunta a Categorias.Id
```

## Heap maximo

Estructura donde el elemento con mayor prioridad siempre esta arriba.

En el proyecto:

```text
Estructuras/MonticuloMaximo.cs
```

Uso principal:

```text
Priorizar familias durante la distribucion.
```

## Historial

Registro de entregas confirmadas.

Pantalla:

```text
FormHistorial.cs
```

Servicio:

```text
HistorialServicio.cs
```

## LIFO

Last In, First Out. Ultimo en entrar, primero en salir.

Es la regla de una pila.

Ejemplo:

```text
El ultimo cambio manual es el primero que se deshace.
```

## Lista enlazada

Estructura formada por nodos conectados uno tras otro.

```text
[A] -> [B] -> [C] -> null
```

En el proyecto:

```text
Estructuras/ListaEnlazada.cs
```

## Maximo por entrega

Limite de cantidad de un producto que una familia puede recibir en una entrega.

Ejemplo:

```text
Cepillo Dental MaximoPorEntrega = 1
```

## Modelo

Clase que representa datos.

Ejemplos:

```text
Beneficiario
Producto
OrdenDetalle
```

## Monticulo maximo

Nombre en español de max heap.

Ver: Heap maximo.

## Orden

Evento de entrega confirmada.

Tabla:

```text
Orden
```

Modelo:

```text
Modelos/Orden.cs
```

## OrdenDetalle

Linea concreta de una orden.

Ejemplo:

```text
Familia Solis recibio 6 Libras de Arroz Blanco.
```

## Pack porcentual

Configuracion que divide una categoria en productos.

Ejemplo:

```text
Granos Basicos:
  Arroz 60%
  Frijol 40%
```

Pantalla:

```text
FormConfigurarPacks.cs
```

## Pila

Estructura LIFO usada para deshacer.

```text
tope -> ultimo cambio
```

En el proyecto:

```text
Estructuras/Pila.cs
```

## Propuesta

Lista temporal de entregas sugeridas por el algoritmo.

No modifica SQLite hasta confirmar.

En codigo:

```text
FormDistribucion.propuestaActual
```

## Reposicion

Regla para evitar entregar demasiado seguido un producto a la misma familia.

Ejemplo:

```text
Si recibio cepillo hace 20 dias y la regla es 90, se omite.
```

## RelojDemo

Utilidad para simular fecha.

Ruta:

```text
Utilidades/RelojDemo.cs
```

## Rollback

Operacion que deshace una transaccion cuando algo falla.

Ejemplo:

```text
Si no hay stock suficiente al confirmar, se cancela toda la orden.
```

## Servicio

Clase que ejecuta logica o SQL.

Ejemplos:

```text
ProductoServicio
DistribucionServicio
HistorialServicio
```

## SKU

Codigo unico de producto.

Ejemplo:

```text
SKU001
```

En el proyecto, `ProductoServicio` usa SKU como clave de busqueda en `TablaHash`.

## SQLite

Base de datos local en un archivo.

Archivo usado:

```text
donaciones.db
```

## Stock

Cantidad disponible de un producto.

Ejemplo:

```text
Arroz Blanco tiene stock 40.
```

## Tabla hash

Estructura de clave-valor para buscar rapido.

```text
clave -> valor
SKU001 -> Producto Arroz
```

En el proyecto:

```text
Estructuras/TablaHash.cs
```

## Tasa de consumo

Cantidad que una persona consume por dia de una categoria.

Ejemplo:

```text
0.4 libras/persona/dia de granos.
```

Pantalla:

```text
FormTasaConsumo.cs
```

## Transaccion

Bloque de operaciones de base de datos que se guarda completo o se cancela completo.

En confirmacion:

```text
INSERT Orden
INSERT OrdenDetalle
UPDATE Stock
```

Si falla algo, se hace rollback.

## Upsert

Operacion que inserta si no existe o actualiza si ya existe.

En el proyecto:

```text
TasaConsumoServicio.Guardar
```

Usa:

```sql
ON CONFLICT(IdCategoria) DO UPDATE
```

## Vulnerabilidad

Nivel social usado para priorizar familias.

Valores:

```text
1 Baja
2 Media
3 Alta
```

Impacto:

```text
Alta tiene mas peso en prioridad.
Alta usa 14 dias iniciales si no hay historial.
```
