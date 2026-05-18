# Explicacion Del Sistema Y Demo Controlada

## 1. Objetivo Del Sistema

El Sistema de Gestion de Donaciones ayuda a una organizacion social a registrar inventario, beneficiarios y entregas de ayuda.

El problema principal que resuelve es decidir de forma ordenada y justificable que familia debe recibir ayuda primero cuando los productos son limitados.

El sistema evita repartir unicamente por orden de llegada o solo a criterio de la persona encargada. En su lugar, usa un calculo de necesidad acumulada llamado deficit.

## 2. Que Significa Distribuir Inteligentemente

Distribuir inteligentemente significa que el sistema no reparte todos los productos por igual sin contexto.

El sistema toma en cuenta:

- Cuantas personas hay en cada familia.
- Que tipo de producto se esta repartiendo.
- Cuanto deberia consumir una persona por dia.
- Que entregas recibio antes esa familia.
- Cuanto inventario hay disponible.
- Que producto exacto se descontara del inventario.

Con esa informacion, el sistema genera una propuesta automatica de distribucion.

## 3. Fuentes Nutricionales De Referencia

El sistema no pretende reemplazar una evaluacion medica o nutricional profesional. Las tasas de consumo son configurables y sirven como una aproximacion practica para apoyar decisiones de reparto.

### World Food Programme (WFP)

El Programa Mundial de Alimentos explica que una canasta alimentaria busca ayudar a las personas a cubrir sus requerimientos diarios de alimentacion y nutricion. En contextos donde una persona depende completamente de asistencia alimentaria, WFP menciona como referencia el objetivo de cubrir aproximadamente 2,100 kilocalorias por persona por dia, junto con macronutrientes y micronutrientes esenciales.

WFP tambien indica que una canasta alimentaria suele incluir cereales, legumbres, aceite vegetal y sal yodada.

Fuente consultada: WFP, "All you need to know about the WFP food basket".

### FAO Y Guias Alimentarias De El Salvador

La FAO registra las "Guias alimentarias para las familias salvadorenas", respaldadas por el Ministerio de Salud. Entre sus mensajes principales estan preparar comidas variadas, consumir verduras y frutas, incluir alimentos como leche, queso, cuajada, requeson o huevos, consumir proteinas animales algunas veces por semana, usar aceite vegetal en pequenas cantidades y beber suficiente agua.

Fuente consultada: FAO, "Food-based dietary guidelines - El Salvador".

## 4. Modulos Principales

### Inventario

Contiene categorias, unidades, productos y stock disponible.

Ejemplo:

| Elemento | Ejemplo |
|---|---|
| Categoria | Granos Basicos |
| Unidad | Libras |
| Producto | Arroz Blanco |
| Stock | 30 libras |

### Beneficiarios

Registra las familias que pueden recibir ayuda.

Cada beneficiario tiene nombre, cantidad de miembros del hogar y estado activo o inactivo.

Ejemplo:

| Beneficiario | Miembros |
|---|---:|
| Familia Lopez | 5 |
| Familia Perez | 3 |
| Familia Martinez | 7 |

### Tasas De Consumo

Define cuanto consume una persona por dia en cada categoria.

Ejemplo:

| Categoria | Tasa diaria | Unidad |
|---|---:|---|
| Granos Basicos | 0.4 | Libras |
| Aceites y Grasas | 0.05 | Litros |
| Higiene Personal | 0.1 | Unidades |

Estas tasas permiten estimar cuanto necesita una familia segun su tamano.

### Distribucion

Genera la propuesta automatica de entrega.

Muestra beneficiario, categoria, producto sugerido, SKU, cantidad a entregar, unidad y deficit calculado.

### Historial

Guarda las entregas confirmadas.

Sirve para saber que recibio cada familia, auditar las entregas y alimentar el calculo de deficit en futuras distribuciones.

## 5. Base Del Calculo

El sistema calcula necesidad por categoria.

La formula base es:

```text
Necesidad = miembros del hogar x tasa diaria x dias considerados
```

Ejemplo:

```text
Familia Lopez
Miembros: 5
Categoria: Granos Basicos
Tasa diaria: 0.4 libras/persona/dia
Dias: 7

Necesidad = 5 x 0.4 x 7 = 14 libras
```

Esto significa que, para una semana, la Familia Lopez necesitaria aproximadamente 14 libras de granos basicos.

## 6. Que Es El Deficit

El deficit es la diferencia entre lo que una familia deberia haber recibido y lo que realmente recibio.

```text
Deficit = necesidad estimada - cantidad ya recibida
```

Ejemplo:

```text
Necesidad estimada: 14 libras
Ya recibido anteriormente: 4 libras

Deficit = 14 - 4 = 10 libras
```

Mientras mayor sea el deficit, mayor prioridad tiene la familia para recibir ayuda.

## 7. Por Que El Sistema Trabaja Por Categorias

El sistema reparte por categoria porque una familia puede estar bien cubierta en un tipo de producto, pero no en otro.

Ejemplo:

- Familia Lopez recibio arroz la semana pasada.
- Pero no recibio aceite.
- Entonces puede tener bajo deficit en Granos Basicos, pero alto deficit en Aceites y Grasas.

Esto hace que la distribucion sea mas justa que repartir paquetes identicos sin analizar el historial.

## 8. Como Decide A Quien Dar Primero

Para cada categoria, el sistema calcula el deficit de cada beneficiario.

Luego coloca a los beneficiarios en una cola de prioridad usando un `MonticuloMaximo`.

El `MonticuloMaximo` permite que la familia con mayor deficit salga primero.

Criterios principales:

1. Mayor deficit.
2. Mayor cantidad de miembros en el hogar.
3. Menor ID como desempate estable.

Esto permite que la propuesta sea automatica, repetible y defendible.

## 9. Por Que Las Cantidades Se Redondean

En la vida real no es practico entregar cantidades como:

```text
2.7 paquetes
3.4 jabones
1.8 bolsas
```

Por eso el sistema trunca las cantidades a numeros enteros.

Ejemplo:

```text
Necesidad calculada: 8.4 libras
Cantidad propuesta: 8 libras
```

Esto facilita el uso del sistema durante una entrega real.

## 10. Como Se Evita Entregar Mas De Lo Disponible

El sistema valida que no se pueda entregar mas stock del que existe.

Para esto se usa una `TablaHash`.

La clave de la tabla es la categoria y el valor es el stock disponible.

Ejemplo:

| Clave | Valor |
|---|---:|
| Granos Basicos | 40 |
| Aceites y Grasas | 5 |
| Higiene Personal | 12 |

Si el usuario intenta modificar manualmente una entrega y sobrepasa el stock disponible, el sistema muestra una alerta y revierte el cambio.

Esto evita errores humanos y protege la integridad del inventario.

## 11. Que Producto Se Entrega Realmente

Aunque el algoritmo calcula la necesidad por categoria, la pantalla muestra un producto concreto.

Ejemplo:

| Beneficiario | Categoria | Producto | SKU | A entregar | Unidad |
|---|---|---|---|---:|---|
| Familia Lopez | Granos Basicos | Arroz Blanco | SKU001 | 14 | Libras |

Esto ayuda a que la persona encargada entienda que debe entregar fisicamente.

## 12. Condiciones Para Que Una Categoria Aparezca En Distribucion

Una categoria solo aparece en la propuesta automatica si cumple todas estas condiciones:

1. La categoria existe.
2. Tiene una unidad asociada.
3. Tiene una tasa de consumo configurada.
4. Tiene al menos un producto con stock mayor a 0.
5. Existen beneficiarios activos.

Si falta una de estas condiciones, el sistema no puede generar una entrega para esa categoria.

## 13. Demo Pequena Propuesta

La demo pequena debe tener pocos datos para que se pueda explicar facilmente.

### Categorias

| Categoria |
|---|
| Granos Basicos |
| Aceites y Grasas |
| Higiene Personal |

### Unidades

| Unidad | Tipo |
|---|---|
| Libras | Peso |
| Litros | Volumen |
| Unidades | Unidad |

### Productos

| SKU | Producto | Categoria | Stock | Unidad |
|---|---|---|---:|---|
| SKU001 | Arroz Blanco | Granos Basicos | 30 | Libras |
| SKU002 | Frijol Rojo | Granos Basicos | 10 | Libras |
| SKU003 | Aceite Vegetal | Aceites y Grasas | 5 | Litros |
| SKU004 | Jabon de Bano | Higiene Personal | 12 | Unidades |

### Beneficiarios

| Beneficiario | Miembros |
|---|---:|
| Familia Lopez | 5 |
| Familia Perez | 3 |
| Familia Martinez | 7 |
| Familia Solis | 2 |

### Tasas

| Categoria | Tasa diaria | Unidad |
|---|---:|---|
| Granos Basicos | 0.4 | Libras |
| Aceites y Grasas | 0.05 | Litros |
| Higiene Personal | 0.1 | Unidades |

## 14. Caso De Prueba Principal

### Paso 1: Revisar Inventario

Abrir:

```text
Inventario > Gestionar Productos
```

Verificar que existen los productos demo y que tienen stock.

### Paso 2: Revisar Beneficiarios

Abrir:

```text
Beneficiarios > Padron de Familias
```

Verificar que existen las familias demo.

### Paso 3: Revisar Tasas

Abrir:

```text
Configuracion > Tasas de Consumo
```

Verificar que cada categoria tiene tasa diaria y unidad base.

### Paso 4: Generar Propuesta

Abrir:

```text
Ayuda Social > Generar Asignacion
```

Presionar:

```text
Generar Propuesta Automatica
```

### Resultado Esperado

El sistema debe mostrar entregas propuestas por categoria.

La Familia Martinez deberia tener prioridad alta porque tiene mas miembros.

Ejemplo para Granos Basicos:

```text
Familia Martinez:
7 x 0.4 x 7 = 19.6
Propuesta aproximada: 19 libras

Familia Lopez:
5 x 0.4 x 7 = 14
Propuesta aproximada: 14 libras

Familia Perez:
3 x 0.4 x 7 = 8.4
Propuesta aproximada: 8 libras
```

## 15. Caso De Prueba: Sobregiro

### Objetivo

Probar que el sistema no permite entregar mas inventario del disponible.

### Pasos

1. Generar una propuesta automatica.
2. Editar manualmente una fila.
3. Escribir una cantidad exagerada, por ejemplo `999`.
4. Salir de la celda.

### Resultado Esperado

El sistema debe mostrar una alerta indicando que no hay suficiente inventario.

La cantidad debe volver al valor anterior.

## 16. Caso De Prueba: Historial

### Pasos

1. Confirmar una distribucion.
2. Abrir:

```text
Ayuda Social > Historial de Entregas Realizadas
```

3. Buscar una familia, por ejemplo:

```text
Lopez
```

### Resultado Esperado

El sistema debe mostrar que recibio esa familia, cuanto recibio y en que categoria.

## 17. Caso De Prueba: Segunda Distribucion

### Objetivo

Demostrar que el historial afecta la siguiente propuesta.

### Pasos

1. Confirmar una primera distribucion.
2. Generar una segunda propuesta.
3. Comparar los deficits.

### Resultado Esperado

Las familias que ya recibieron ayuda deben tener menor deficit en esa categoria.

Esto demuestra que el sistema no reparte a ciegas, sino que usa historial.

## 18. Estructuras De Datos Usadas

### TablaHash

Se usa para detectar duplicados, buscar por SKU y validar stock disponible por categoria.

### MonticuloMaximo

Se usa para priorizar familias con mayor deficit.

### ListaEnlazada

Se usa para recorrer datos del sistema, almacenar colecciones internas y manejar importaciones.

### Pila

Se usa para deshacer cambios manuales.

### ArbolBST

Se usa para buscar beneficiarios por nombre de forma eficiente.

## 19. Conclusion

El sistema distribuye de forma inteligente porque combina inventario, beneficiarios, tasas de consumo e historial.

La propuesta automatica no depende del criterio subjetivo de una persona, sino de reglas claras:

- Necesidad estimada.
- Deficit.
- Stock disponible.
- Historial de entregas.

Esto hace que la distribucion sea mas justa, auditable y facil de explicar.
