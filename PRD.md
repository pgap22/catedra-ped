# Sistema de Gestión de Donaciones
## Documento de Diseño — Fase I
**Programación con Estructuras de Datos | PED104 | UDB Ciclo 01-2026**

---

## Índice

1. [¿Qué estamos construyendo?](#1-qué-estamos-construyendo)
2. [¿Para quién es el sistema?](#2-para-quién-es-el-sistema)
3. [El día a día — cómo se usa el sistema](#3-el-día-a-día--cómo-se-usa-el-sistema)
4. [Los módulos del sistema](#4-los-módulos-del-sistema)
5. [El corazón del sistema — el algoritmo de distribución](#5-el-corazón-del-sistema--el-algoritmo-de-distribución)
6. [Las estructuras de datos — por qué cada una](#6-las-estructuras-de-datos--por-qué-cada-una)
7. [El modelo de datos — qué guardamos](#7-el-modelo-de-datos--qué-guardamos)
8. [Casos límite — qué puede salir mal](#8-casos-límite--qué-puede-salir-mal)
9. [Herramientas que usaremos](#9-herramientas-que-usaremos)
10. [Orden de construcción](#10-orden-de-construcción)
11. [Innovación y por qué importa](#11-innovación-y-por-qué-importa)

---

## 1. ¿Qué estamos construyendo?

Una aplicación de escritorio para Windows que le permite a la secretaria de una organización de ayuda social (iglesia, ONG, comedor comunitario) gestionar donaciones y distribuirlas de forma **automática, justa y trazable** entre las familias beneficiarias.

El problema que resuelve es real y concreto: hoy en día estas organizaciones manejan todo en papel o en Excel sin lógica. El resultado es que:

- Algunas familias reciben ayuda dos veces mientras otras se quedan sin nada.
- Nadie sabe exactamente qué hay en bodega.
- Las decisiones de "a quién le doy primero" dependen del criterio subjetivo de quien esté ese día.
- Los productos vencen en bodega porque nadie los registró bien.

Nuestro sistema ataca todos estos problemas con algoritmos y estructuras de datos reales.

---

## 2. ¿Para quién es el sistema?

**Usuario principal:** La secretaria. No es programadora. No le interesan los algoritmos. Ella solo quiere abrir la aplicación, hacer su trabajo y que el sistema le diga qué darle a quién.

**Beneficiarios:** Las familias que reciben ayuda. Ellas no tocan el sistema directamente. Pueden llenar un formulario externo (Google Forms, Excel manual) con lo que necesitan, y la secretaria importa esa información.

**La organización:** Necesita poder demostrar que distribuye de forma justa y transparente. El historial que genera el sistema les sirve para rendir cuentas a donantes.

---

## 3. El día a día — cómo se usa el sistema

### Día 0 — La primera vez (configuración inicial)

Antes de registrar cualquier producto o familia, hay que decirle al sistema las reglas base. Esto solo se hace una vez.

**Paso 1 — Crear categorías**
La secretaria abre el módulo de catálogos y crea las categorías de productos que maneja la organización. Por ejemplo:
- Granos básicos
- Lácteos
- Aceites y grasas
- Higiene personal
- Medicamentos básicos

Sin categorías, no se puede registrar ningún producto. Son el primer bloque.

**Paso 2 — Definir unidades de medida**
Se registran las unidades que se usarán: libras, litros, unidades, bolsas. Y aquí viene algo importante: **no todas las unidades tienen sentido para todos los productos**. El sistema sabe esto. Si alguien intenta registrar jabón en litros, el sistema lo rechaza. Las unidades válidas para cada tipo de producto se configuran aquí.

**Paso 3 — Cargar el inventario inicial**
La primera vez, probablemente ya hay productos en bodega. En lugar de ingresarlos uno por uno, la secretaria importa un Excel con todo el inventario inicial. El sistema lo procesa en segundos.

---

### Semana normal — el flujo cotidiano

**Cuando llega una donación:**
Alguien dona 10 libras de arroz. La secretaria busca "arroz" en el inventario, le suma 10 libras y listo. Si el producto no existía, lo crea. Esto es máximo 3 clics.

**Cuando llegan solicitudes de las familias:**
Las familias pueden enviar lo que necesitan de dos maneras:
- **Canal digital:** llenan un Google Forms o un Excel y la secretaria lo importa.
- **Canal presencial:** la familia llega y la secretaria captura la solicitud directo en el sistema.

En ambos casos el resultado es el mismo: el sistema tiene registrada la "orden" de cada familia — qué necesitan esta semana.

**Cuando es momento de distribuir:**
La secretaria presiona "Generar distribución". El sistema hace todo el trabajo pesado en segundos y le presenta una lista ordenada: familia por familia, qué le toca de cada categoría. La secretaria solo tiene que imprimir esa lista y usarla al momento de entregar.

---

### Un ejemplo completo

Imaginemos que es viernes y hay esto en bodega:
- 20 libras de arroz
- 8 litros de aceite
- 15 barras de jabón

Y hay 5 familias registradas. El sistema sabe:
- Familia López: 4 personas, lleva 2 semanas sin recibir aceite.
- Familia Martínez: 6 personas, recibió de todo la semana pasada.
- Familia García: 2 personas, adulto mayor enfermo, recibió poco arroz.
- Familia Rodas: 5 personas, primera vez que solicita.
- Familia Pérez: 3 personas, recibió aceite hace 3 días.

El sistema no los trata a todos igual. Calcula para **cada categoría por separado** quién tiene más déficit — quién ha recibido menos en relación a lo que debería haber recibido según el tamaño de su familia y el tiempo transcurrido.

Resultado posible:
- **Aceite:** Familia López primero (2 semanas sin recibir), luego García, luego Rodas.
- **Arroz:** Familia Martínez primero (6 personas, necesita más cantidad), luego Rodas, luego Pérez.
- **Jabón:** Se distribuye equitativamente entre todos.

La secretaria ve esto en pantalla, puede ajustar manualmente si hay alguna razón especial, confirma y el sistema descuenta el stock automáticamente.

---

## 4. Los módulos del sistema

### Módulo 1 — Catálogos
**¿Qué hace?** Permite crear, editar y eliminar categorías de productos.

**Regla importante:** No se puede eliminar una categoría que tenga productos asociados. El sistema avisa cuántos productos la están usando antes de dejarte borrarla.

**Importación:** Se puede cargar un listado de categorías desde Excel o CSV para no tener que escribirlas una por una.

---

### Módulo 2 — Unidades de Medida y su pivote con productos
**¿Qué hace?** Administra las unidades disponibles y define cuáles son válidas para qué tipo de producto.

**¿Por qué existe este módulo?** Para evitar errores absurdos. Si el sistema no controla esto, alguien podría registrar "5 litros de jabón en barra" y el inventario quedaría corrupto. La relación entre producto y unidad válida es muchos-a-muchos: un producto puede medirse en varias unidades, y una unidad puede usarse en varios productos.

---

### Módulo 3 — Inventario (Productos)
**¿Qué hace?** Registra todos los productos en bodega con su cantidad disponible, categoría y unidad de medida. Permite agregar, editar y dar de baja productos.

**Importación masiva:** El primer día, o cuando llega una donación grande organizada en Excel, se puede importar el archivo completo. El sistema usa una tabla hash (Hashtable) indexada por SKU del producto para detectar duplicados en tiempo casi instantáneo — si el producto ya existe, actualiza la cantidad; si no existe, lo crea.

**Alertas:** Si la cantidad de un producto baja de un mínimo configurable, el sistema lo resalta visualmente.

---

### Módulo 4 — Beneficiarios
**¿Qué hace?** Gestiona el padrón de familias beneficiarias. Cada familia tiene:
- Nombre del responsable
- Número de miembros del hogar
- Historial de lo que ha recibido
- Estado (activa / inactiva)

**Importación:** Se puede importar el listado desde Excel. Útil cuando ya existe un padrón previo.

**Búsqueda:** Por nombre o identificador, usando un Árbol Binario de Búsqueda (BST) para encontrar cualquier familia en tiempo O(log n) — sin importar cuántas familias haya registradas.

---

### Módulo 5 — Órdenes y Solicitudes
**¿Qué hace?** Captura lo que cada familia solicita antes de una distribución.

Una "orden" es simplemente: "la familia X solicita Y cantidad de Z categoría". Puede ingresar manualmente o importarse desde un Excel que las familias llenaron previamente.

Las órdenes no son obligatorias para que el sistema funcione — si una familia no solicita nada, el sistema igual la considera en base a su déficit histórico.

---

### Módulo 6 — Distribución (el módulo estrella)
**¿Qué hace?** Corre el algoritmo, genera la propuesta de distribución y permite que la secretaria la revise y confirme.

**Flujo:**
1. La secretaria presiona "Generar distribución".
2. El sistema calcula el déficit de cada familia por cada categoría.
3. Genera una propuesta ordenada por prioridad.
4. La secretaria puede modificar cantidades manualmente si hay alguna razón especial.
5. Confirma → el sistema descuenta el stock y registra el historial.

Una distribución confirmada no se puede modificar. Solo se puede ver. Esto garantiza la trazabilidad.

---

### Módulo 7 — Historial y Reportes
**¿Qué hace?** Guarda un registro permanente de cada distribución: quién recibió qué, cuándo y en qué cantidad.

Este historial es la base de dos cosas:
- La transparencia hacia donantes ("mirá, distribuimos X kilos de arroz entre N familias este mes").
- El cálculo del déficit en la próxima distribución ("la familia López recibió esto la última vez, entonces ahora le calculamos lo que le falta").

---

## 5. El corazón del sistema — el algoritmo de distribución

Este es el punto más importante del documento. Todo lo demás son formularios. Esto es lo que hace al sistema inteligente.

### El problema que resuelve

Tenés recursos limitados y varias familias con necesidades distintas. ¿Cómo decidís qué le das a cada quien de forma justa, automática y sin favoritismos?

### La idea central: el Déficit por Categoría

Para cada familia y cada categoría de producto, el sistema calcula cuánto **debería haber recibido** hasta hoy versus cuánto **realmente recibió**. La diferencia es el déficit.

```
Consumo esperado = Miembros del hogar × Tasa de consumo estándar × Días transcurridos

Déficit = Consumo esperado - Lo que ya recibió en ese período
```

**¿De dónde sale la "tasa de consumo estándar"?**
No la inventamos. El Programa Mundial de Alimentos (PMA/WFP) y organismos como la FAO tienen tablas de requerimientos mínimos por persona para distintos tipos de alimentos. Por ejemplo, una persona adulta necesita aproximadamente 400 gramos de granos básicos por día. Estos valores son configurables en el sistema.

### Por qué se hace por categoría y no por familia en general

Este es el insight clave. Una familia puede estar bien surtida de arroz pero llevar semanas sin recibir aceite. Si calculamos una prioridad global por familia, ese déficit específico de aceite se pierde entre los promedios.

Al correr el algoritmo **por categoría de forma independiente**, cada familia tiene su posición en la cola para cada tipo de producto. Una familia puede ser la número 1 en aceite y la número 8 en granos básicos al mismo tiempo.

### El algoritmo paso a paso (sin código, para que lo entiendan todos)

Imaginen que el sistema hace esto mentalmente para **cada categoría**:

**Paso 1 — Calcular quién debe recibir más**
Para cada familia registrada, calcula su déficit en esa categoría. El resultado es una lista de familias con su "puntaje de necesidad" para ese producto.

**Paso 2 — Ordenar de mayor a menor déficit**
Aquí entra la Cola de Prioridad. El sistema organiza a todas las familias en una estructura donde la que tiene mayor déficit siempre queda al frente. Esta estructura se llama Max-Heap (montículo máximo) y garantiza que siempre podemos encontrar a "quien más necesita" en tiempo constante — sin recorrer toda la lista.

**Paso 3 — Distribuir mientras haya stock**
El sistema toma a la familia del frente de la cola (la de mayor déficit), le asigna la cantidad que le corresponde según su tamaño y el stock disponible, descuenta esa cantidad del inventario, y pasa a la siguiente familia.

**Paso 4 — Respetar límites razonables**
Nadie recibe más de lo que razonablemente puede consumir hasta la próxima distribución. Si hay abundancia de un producto, el excedente puede distribuirse equitativamente entre todos.

**Paso 5 — Guardar todo**
Cada asignación queda registrada en el historial. La próxima vez que corra el algoritmo, ese historial alimenta el cálculo del nuevo déficit.

### Un ejemplo numérico simple

Hay 6 litros de aceite. Dos familias:
- **Familia A:** 3 personas, lleva 20 días sin recibir aceite. Déficit calculado: 4.8 litros.
- **Familia B:** 5 personas, recibió aceite hace 5 días. Déficit calculado: 2.5 litros.

El sistema pone a la Familia A al frente. Le asigna su déficit completo: 4.8 litros. Quedan 1.2 litros. La Familia B recibe esos 1.2 litros (no alcanza para su déficit completo, pero es lo que hay). El sistema registra que Familia B quedó con un déficit pendiente de 1.3 litros — lo que afectará positivamente su posición en la próxima distribución.

Resultado: nadie quedó sin nada, y quien más necesitaba recibió más. Sin subjetividad, sin favoritismos.

---

## 6. Las estructuras de datos — por qué cada una

Este es el requisito académico central: implementar estructuras de datos desde cero, sin usar las librerías genéricas de C# como `List<T>`, `Dictionary<K,V>`, etc.

### Hashtable (Tabla Hash) — para el inventario

**¿Qué es?** Una estructura que guarda pares clave-valor y permite buscar cualquier elemento en tiempo prácticamente instantáneo, sin importar cuántos elementos tenga.

**¿Cómo funciona sin magia?** Cada producto tiene un SKU (código único). La Hashtable aplica una función matemática (función hash) a ese SKU para convertirlo en un número — ese número es la posición donde se guarda el producto en un arreglo interno. Para buscar el producto después, se aplica la misma función al SKU y se va directo a esa posición.

**¿Por qué la usamos?** Cuando importamos 500 productos desde un Excel, necesitamos saber para cada uno si ya existe en el inventario. Sin Hashtable, tendríamos que recorrer toda la lista para cada producto → 500 × 500 = 250,000 comparaciones. Con Hashtable, cada búsqueda es O(1) — una sola operación.

**¿Qué problema puede tener?** Si dos SKUs distintos generan el mismo número (colisión), hay que manejarla. Lo hacemos con encadenamiento: en cada posición del arreglo guardamos una lista de los elementos que cayeron ahí.

---

### Max-Heap (Montículo Máximo) — para la cola de prioridad

**¿Qué es?** Un árbol binario completo donde el elemento de mayor valor siempre está en la raíz (en el tope). Se guarda en un arreglo simple, no en nodos con punteros.

**¿Cómo funciona sin magia?** Imaginá un árbol como un torneo de tenis. El campeón (mayor déficit) siempre está arriba. Cuando eliminás al campeón (le asignás recursos y lo sacás de la cola), el árbol se reorganiza automáticamente para poner al siguiente mejor en la cima. Esa reorganización toma O(log n) pasos — si tenés 1000 familias, son máximo 10 pasos.

**¿Por qué la usamos y no una lista ordenada?** Una lista ordenada requiere recorrerla para insertar en la posición correcta → O(n). El Heap siempre inserta y extrae el máximo en O(log n). Para este sistema no hace una diferencia enorme en velocidad, pero sí en calidad académica: demuestra que entendemos por qué el Heap es superior para este caso de uso.

**¿Cuántos Heaps hay?** Uno por cada categoría de producto activa. Si hay 8 categorías, hay 8 Heaps independientes corriendo en paralelo durante la distribución.

---

### BST (Árbol Binario de Búsqueda) — para buscar beneficiarios

**¿Qué es?** Un árbol donde cada nodo tiene un valor, y todos los valores menores están a su izquierda y todos los mayores a su derecha. Esto permite buscar cualquier elemento en O(log n) siguiendo el camino correcto.

**¿Por qué la usamos?** Cuando la secretaria busca "López" entre 300 familias, el BST no revisa todos los registros — va a la izquierda o derecha en cada paso según si el nombre buscado es menor o mayor al nodo actual. Máximo log₂(300) ≈ 8 pasos.

**Limitación honesta:** Si los datos se insertan en orden alfabético, el BST degenera en una lista → O(n). La solución correcta es un árbol balanceado (AVL), pero para el alcance de este proyecto y el volumen de familias (probablemente menos de 200), el BST simple es adecuado y justificable.

---

### Stack (Pila) — para deshacer la última asignación

**¿Qué es?** Una estructura LIFO (Last In, First Out). El último que entró es el primero en salir. Exactamente como una pila de platos.

**¿Por qué la usamos?** Cuando la secretaria está revisando la distribución propuesta antes de confirmarla, puede haber cometido un error al ajustar manualmente una cantidad. El Stack guarda cada modificación en orden. Presionar "Deshacer" saca la última acción de la pila y revierte el cambio.

**Nota importante:** El Stack solo funciona antes de confirmar la distribución. Una vez confirmada, el historial es permanente — eso garantiza la integridad de los datos.

---

### Lista Enlazada Simple — para el historial

**¿Qué es?** Una cadena de nodos donde cada nodo apunta al siguiente. Se puede crecer dinámicamente sin saber de antemano cuántos elementos habrá.

**¿Por qué la usamos aquí?** El historial de distribuciones crece indefinidamente con el tiempo. No sabemos cuántas distribuciones habrá en un año. La lista enlazada se adapta a eso sin desperdiciar memoria. No necesitamos búsqueda rápida en el historial — generalmente se consulta en orden cronológico, que es exactamente lo que la lista nos da.

---

## 7. El modelo de datos — qué guardamos

El sistema usa SQLite como base de datos local. No requiere internet ni servidor. El archivo de la base de datos vive en la misma computadora que el programa.

### Las tablas principales

**Categoria**
Guarda los tipos de productos (Granos básicos, Higiene, etc.)
- id, nombre

**UnidadMedida**
Guarda las unidades disponibles y su tipo.
- id, nombre, tipo (peso / volumen / unidad)

**ProductoUnidadValida**
Define qué unidades son válidas para qué productos. Es la tabla pivote que evita registrar "5 litros de jabón".
- productoId, unidadId

**Producto**
El inventario.
- id, sku (código único), nombre, categoriaId, cantidadDisponible, unidadMedidaId

**Beneficiario**
El padrón de familias.
- id, nombre, miembrosHogar, activo

**TasaConsumo**
Los estándares de consumo por persona por categoría. Basados en tablas del PMA.
- categoriaId, gramosPersonaDia (o equivalente en la unidad de esa categoría)

**Orden**
Cada evento de distribución.
- id, fechaGeneracion, estado (borrador / confirmada), observaciones

**OrdenDetalle**
Lo que recibió cada familia en cada distribución.
- ordenId, beneficiarioId, categoriaId, cantidadAsignada, cantidadSolicitada, deficitCalculado

---

## 8. Casos límite — qué puede salir mal

Estos son los escenarios que el sistema debe manejar sin romperse ni dar resultados incorrectos.

**Stock cero al intentar distribuir**
El sistema genera la distribución de todas formas, pero para esa categoría todas las cantidades son cero. Muestra un aviso claro: "Sin stock disponible para Aceites y grasas". El déficit acumulado queda registrado y afectará la próxima distribución.

**Familia sin historial (primera vez)**
Si una familia no tiene historial, su déficit se calcula como si hubiera estado sin recibir desde que se registró. No queda en desventaja por ser nueva.

**Dos familias con exactamente el mismo déficit**
El desempate es por número de miembros del hogar (familia más grande tiene prioridad). Si también son iguales, gana la que tiene mayor antigüedad en el padrón (id más bajo). El resultado siempre es determinista — el mismo input produce siempre el mismo output.

**Excel con columnas mal nombradas o faltantes**
El sistema rechaza el archivo completo antes de procesar cualquier fila. Muestra exactamente qué columna falta. No procesa parcialmente — o todo el archivo es válido o no se importa nada.

**SKU duplicado dentro del mismo archivo Excel**
Se toma la última aparición y se registra un aviso en el reporte de importación. No falla silenciosamente.

**Categoría con productos al intentar eliminarla**
El sistema bloquea la eliminación y muestra cuántos productos están usando esa categoría. Para eliminar la categoría primero hay que reasignar o eliminar esos productos.

**Beneficiario inactivo**
No aparece en la generación de distribuciones. No se elimina del sistema — se desactiva. Esto preserva el historial.

**Modificación manual que supera el stock disponible**
Si la secretaria intenta asignarle a una familia más de lo que hay en bodega, el campo se pone en rojo inmediatamente y no deja confirmar hasta corregirlo.

---

## 9. Herramientas que usaremos

| Herramienta | Para qué |
|---|---|
| Visual C# / WinForms | Lenguaje y framework de la interfaz gráfica |
| SQLite | Base de datos local, sin servidor |
| System.Data.OleDb | Leer archivos Excel (.xlsx) |
| Lógica CSV propia | Parser manual para archivos CSV, sin librerías externas |
| Estructuras propias | Hashtable, MaxHeap, BST, Stack, Lista enlazada — todas implementadas desde cero, sin generics de C# |

**¿Por qué SQLite y no solo memoria RAM?**
Si el programa se cierra inesperadamente (se va la luz, la computadora se reinicia), todos los datos en memoria se pierden. SQLite guarda todo en disco de forma permanente. Para una organización que depende de estos datos para operar, perderlos no es una opción.

---

## 10. Orden de construcción

Recomendamos construir el sistema en este orden. Cada paso depende del anterior.

**Semana 1 — La base**
Módulo de Catálogos (Categorías). Es lo primero porque todo depende de él. Sin categorías no hay productos, sin productos no hay nada.

**Semana 1 — El pivote**
Unidades de Medida y la tabla que las conecta con los productos. Sin esto el inventario no tiene restricciones.

**Semana 2 — El inventario**
Módulo de Productos con importación desde Excel usando la Hashtable. Aquí es donde se prueba que el algoritmo de importación funciona correctamente.

**Semana 2 — Las familias**
Módulo de Beneficiarios con el BST para búsqueda. Incluye importación desde Excel.

**Semana 3 — El algoritmo**
El motor de distribución con los Max-Heaps por categoría. Esta es la parte más compleja y la que más tiempo necesita. Recomendamos probarla primero con datos inventados antes de conectarla a la UI.

**Semana 3 — La interfaz de distribución**
La pantalla donde la secretaria ve la propuesta, la ajusta y la confirma. Incluye el Stack para deshacer.

**Semana 4 — Historial y reportes**
Lo que queda registrado y lo que se puede imprimir o exportar.

---

## 11. Innovación y por qué importa

### Lo que existe hoy (y por qué no sirve)

Existen plataformas grandes de gestión humanitaria — algunas las usan Cruz Roja, ACNUR, grandes ONGs. Todas comparten el mismo problema: requieren internet constante, personal técnico para operarlas, licencias costosas, y están diseñadas para operaciones a escala nacional o internacional.

Una iglesia de barrio, un comedor comunitario, una pequeña fundación local no tiene nada de eso. Termina usando papel o Excel básico. Y ahí está el vacío que este sistema llena.

### Lo que hace diferente a nuestro sistema

**Distribución multi-categoría con déficit acumulado**
La mayoría de los sistemas de inventario simple distribuyen "por parejo" o según quién llega primero. Nuestro sistema sabe que una familia puede estar bien en arroz pero en déficit crítico de aceite, y trata cada categoría de forma independiente. Esto es lo que hacen los sistemas de distribución humanitaria a gran escala — nosotros lo traemos a escala de barrio.

**Trazabilidad real**
Cada decisión del sistema queda registrada. No solo "qué se entregó", sino también "por qué se entregó en ese orden" — el déficit calculado queda guardado junto con la asignación. Cualquier persona puede auditar una distribución y entender la lógica detrás de cada decisión.

**Basado en estándares reales**
Las tasas de consumo no son números inventados. Están basadas en tablas del Programa Mundial de Alimentos (WFP) y organismos de nutrición reconocidos. Esto le da credibilidad científica al sistema y permite que las organizaciones justifiquen sus decisiones de distribución.

**Sin internet, sin servidor, sin costo**
Funciona en cualquier computadora con Windows. No hay mensualidades, no hay dependencia de un proveedor externo, no hay riesgo de que "se caiga el sistema" en el momento de la distribución.

**Importación flexible**
Las familias pueden comunicar sus necesidades como quieran — un Excel que llenaron en casa, un Google Forms, o simplemente llegando en persona. El sistema acepta todos los canales y los trata igual.

### El impacto real

El sistema no solo ahorra tiempo a la secretaria. Cambia fundamentalmente quién recibe ayuda y cuándo. En contextos de escasez, esa diferencia puede ser entre una familia que come y una que no. Automatizar esa decisión con un criterio justo, transparente y auditable no es solo una mejora operativa — es una decisión ética.

---

*Documento elaborado para el proyecto de cátedra PED104 — Programación con Estructuras de Datos*
*Universidad Don Bosco | Ciclo 01-2026*