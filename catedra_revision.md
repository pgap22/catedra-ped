# catedra_revision.md — Checklist técnico PED104 C# / Estructuras de Datos

> **Uso previsto:** dale este archivo a un LLM junto con el código del proyecto y dile:
>
> `Revisa @catedra_revision.md, inspecciona el código real del proyecto y dime si ya estamos base 10. No inventes clases ni arquitectura: valida lo que existe, detecta faltantes y propone cambios mínimos.`

---

## 0. Rol del LLM al usar este archivo

Actúa como **revisor técnico académico** de un proyecto universitario de C# para la materia **Programación con Estructuras de Datos (PED104)**.

Tu tarea NO es rediseñar todo el sistema ni convertirlo en software empresarial. Tu tarea es:

1. Revisar el código actual.
2. Compararlo contra los checks técnicos de la rúbrica.
3. Verificar si las features planteadas realmente existen y funcionan.
4. Identificar qué falta para llegar a una nota base 10.
5. Proponer cambios mínimos, claros y defendibles.
6. Implementar solo si el usuario lo pide después.

---

## 1. Regla principal: el código actual manda

Este archivo es una **guía de revisión**, no una arquitectura obligatoria.

Antes de opinar o programar:

- Lee la estructura real del proyecto.
- Identifica los formularios, clases, servicios, modelos y archivos existentes.
- No asumas que una clase existe solo porque este documento menciona una funcionalidad.
- No inventes nombres de clases.
- No fuerces una arquitectura nueva si el proyecto ya tiene una forma clara.
- Si el código usa otros nombres para una feature, respeta los nombres reales.
- Si algo no existe, márcalo como faltante y propone la forma más simple de integrarlo.

Cuando haya contradicción entre este markdown y el código:

> **El código real tiene prioridad.**

---

## 2. Qué NO se debe revisar ahorita

Omitir por completo la parte documental.

No evaluar:

- Carátula.
- Índice.
- Introducción.
- Fuentes.
- Diagramas.
- Formato del reporte.
- Redacción del documento.
- Páginas mínimas de innovación.
- Defensa oral.
- Presentación personal.

Este checklist es SOLO para revisar el **código C#**, la **funcionalidad**, la **base de datos**, la **interfaz** y la **evidencia real de estructuras de datos**.

---

# 3. Checklist principal de rúbrica final

## 3.1 Manejo de errores y excepciones

### Objetivo para base 10

El proyecto debe demostrar que contempla manejo de errores y excepciones en las operaciones principales.

### Revisar

- [ ] El sistema no se cae por campos vacíos.
- [ ] El sistema no se cae por números inválidos.
- [ ] El sistema no se cae por IDs inexistentes.
- [ ] El sistema no se cae si la base de datos falla o no existe.
- [ ] El sistema no se cae si el CSV tiene datos incorrectos.
- [ ] Hay validaciones antes de guardar, editar, eliminar o procesar datos.
- [ ] Hay `try/catch` en operaciones con base de datos, archivos CSV y acciones críticas.
- [ ] Los errores se muestran al usuario con mensajes entendibles.
- [ ] No se ocultan errores importantes con `catch { }` vacío.
- [ ] Las reglas de negocio se validan antes de ejecutar acciones importantes.

### Evidencia esperada

El LLM debe indicar:

- Archivo revisado.
- Método revisado.
- Qué validación existe.
- Qué escenario de error cubre.
- Qué escenario sigue sin cubrir.

### Señales de riesgo

- Todo está dentro de botones sin validación.
- Se usa `int.Parse`, `decimal.Parse` o conversiones directas sin `TryParse`.
- Hay operaciones SQLite sin manejo de excepción.
- El programa muestra errores técnicos crudos al usuario.
- El sistema permite guardar registros incompletos.

---

## 3.2 Codificación: clases, separación y estructura

### Objetivo para base 10

El código debe estar dividido en más de una clase y mostrar estructura clara usando clases, servicios, modelos, formularios, interfaces o elementos visuales.

### Revisar

- [ ] El proyecto NO está todo en un único archivo.
- [ ] Existen clases separadas para representar entidades/modelos del sistema.
- [ ] Existen clases separadas para lógica o acceso a datos cuando aplique.
- [ ] Los formularios no contienen toda la lógica del sistema mezclada sin separación.
- [ ] Hay nombres entendibles para clases, métodos, variables y controles.
- [ ] La lógica repetida está separada en métodos auxiliares simples.
- [ ] No hay bloques enormes de código copiado y pegado en varios formularios.
- [ ] El código compila sin errores.
- [ ] El código es entendible para un estudiante universitario.
- [ ] El código puede explicarse en defensa sin sonar a arquitectura enterprise incomprensible.

### Evidencia esperada

El LLM debe listar:

- Clases principales encontradas.
- Formularios principales encontrados.
- Servicios o helpers encontrados.
- Partes donde la lógica está demasiado mezclada.
- Refactor mínimo recomendado, solo si realmente hace falta.

### Señales de riesgo

- Todo está en `Form1.cs`.
- No hay modelos.
- No hay servicios.
- No hay estructura de carpetas mínima.
- Los botones hacen consultas SQL, validación, cálculo, actualización de UI y lógica de estructuras todo junto.
- Se creó demasiada arquitectura innecesaria que nadie del equipo puede defender.

---

## 3.3 Diseño del proyecto e interfaz

### Objetivo para base 10

La aplicación debe verse agradable, ser fácil de usar y coincidir razonablemente con lo planteado en la propuesta.

### Revisar

- [ ] Existe un formulario principal o menú para navegar el sistema.
- [ ] Las pantallas importantes son accesibles.
- [ ] Los botones tienen textos claros.
- [ ] Los campos tienen labels claros.
- [ ] Los `DataGridView` o listados muestran información útil.
- [ ] Las acciones críticas piden confirmación cuando corresponde.
- [ ] Después de guardar/editar/eliminar, la UI se actualiza.
- [ ] Hay mensajes de éxito o error entendibles.
- [ ] La interfaz no depende de que el usuario conozca el código.
- [ ] No se ve como un formulario vacío o una pantalla default sin trabajo visual.
- [ ] La UI es simple pero ordenada.

### Evidencia esperada

El LLM debe indicar:

- Pantallas revisadas.
- Función de cada pantalla.
- Problemas visuales detectados.
- Mejoras mínimas para que sea defendible.

### Señales de riesgo

- Solo existe la plantilla básica de Windows Forms.
- Hay botones sin texto claro.
- No hay navegación.
- Hay pantallas que no abren.
- Hay acciones que no hacen nada visible.
- El usuario no sabe qué hacer después de abrir el sistema.

---

## 3.4 Evidencia de utilización temática de PED

### Objetivo para base 10

Debe evidenciarse uso real de estructuras de datos en el proyecto, según el nivel esperado de la materia.

No basta con que existan archivos de estructuras. Deben estar integradas en flujos reales del sistema.

### Restricción fuerte

No usar únicamente estructuras lineales.

Debe haber al menos una estructura no lineal o de acceso eficiente, por ejemplo:

- Árbol.
- Heap / Montículo.
- Tabla Hash.
- Otra estructura propia no trivial.

### Revisar

- [ ] Existen estructuras de datos propias implementadas por estudiantes.
- [ ] Las estructuras NO son simples wrappers de `List<T>`, `Dictionary<K,V>`, `Queue<T>`, `Stack<T>` o similares.
- [ ] Las estructuras usan nodos propios, arreglos propios o lógica propia.
- [ ] Hay al menos una estructura no lineal o no exclusivamente lineal.
- [ ] Las estructuras tienen métodos principales funcionales.
- [ ] Las estructuras se usan desde alguna feature real.
- [ ] El código puede demostrar para qué sirve cada estructura.
- [ ] No se reemplaza la lógica principal de PED con colecciones preexistentes del framework.
- [ ] Si se usan colecciones de C#, son de apoyo secundario y no sustituyen la estructura requerida.
- [ ] Cada estructura usada tiene una razón clara dentro del sistema.

### Checklist de estructuras esperadas según la propuesta del proyecto

No asumir que estos nombres exactos existen. Verificar en el código real y mapear equivalentes.

- [ ] Tabla Hash propia para detección de duplicados, búsqueda por clave o importación.
- [ ] Montículo Máximo / MaxHeap propio para priorización.
- [ ] Árbol Binario de Búsqueda / BST propio para búsqueda ordenada.
- [ ] Pila propia para deshacer acciones o revertir cambios antes de confirmar.
- [ ] Lista Enlazada propia para historial, recorridos o almacenamiento dinámico.

### Evidencia esperada

El LLM debe responder con una tabla:

| Estructura | ¿Existe? | ¿Es propia? | ¿Dónde se usa? | ¿Feature real? | Riesgo |
|---|---:|---:|---|---:|---|

### Señales de riesgo

- Las estructuras existen pero nunca se llaman.
- El sistema usa `Dictionary` para lo que decía usar Tabla Hash propia.
- El sistema usa `PriorityQueue` o `SortedList` para lo que decía usar Heap propio.
- El sistema usa `Stack<T>` para lo que decía usar Pila propia.
- El sistema usa `List<T>` para todo y las estructuras propias solo están de adorno.
- No hay ningún flujo visible donde se pueda demostrar una estructura.

---

## 3.5 Sistema funcional y sin errores

### Objetivo para base 10

El proyecto debe estar finalizado o lo suficientemente completo, corresponder a los objetivos planteados y presentar conexión real a base de datos.

### Revisar

- [ ] El proyecto abre correctamente.
- [ ] El proyecto compila.
- [ ] Existe archivo `.sln` o `.csproj` usable.
- [ ] Las pantallas principales abren sin excepción.
- [ ] La base de datos existe o se crea automáticamente.
- [ ] La conexión SQLite funciona.
- [ ] Se pueden insertar datos.
- [ ] Se pueden consultar datos.
- [ ] Se pueden editar datos.
- [ ] Se pueden eliminar o desactivar datos cuando aplique.
- [ ] Las acciones principales afectan la base de datos o el estado del sistema.
- [ ] El sistema no depende de datos quemados para aparentar funcionamiento.
- [ ] Hay datos de prueba o una forma fácil de probar el flujo.
- [ ] La funcionalidad principal del proyecto está implementada.
- [ ] La aplicación no presenta errores obvios en tiempo de ejecución.

### Evidencia esperada

El LLM debe intentar identificar:

- Comando de compilación usado o recomendado.
- Resultado de compilación.
- Flujo de prueba mínimo.
- Pantallas funcionales.
- Pantallas incompletas.
- Errores críticos.
- Faltantes que afectan la nota.

### Señales de riesgo

- No compila.
- Faltan referencias.
- La base de datos no se encuentra.
- El sistema abre pero no guarda nada.
- Botones importantes no tienen evento.
- Hay formularios pendientes desconectados del menú.
- La feature principal del proyecto no existe.

---

# 4. Restricciones técnicas heredadas de Fase 1

Estas restricciones sí importan para el código.

## 4.1 Lenguaje y entorno

- [ ] El proyecto debe estar desarrollado en Visual C#.
- [ ] La interfaz esperada es de escritorio, idealmente Windows Forms si el proyecto ya va en esa línea.
- [ ] Se permite usar herramientas para mejorar apariencia e interactividad.
- [ ] No migrar el proyecto a web.
- [ ] No cambiar de tecnología principal sin razón extrema.

## 4.2 Estructuras desarrolladas por estudiantes

- [ ] Las estructuras de datos importantes deben estar implementadas por el equipo.
- [ ] No sustituir las estructuras PED con clases preexistentes del lenguaje.
- [ ] No depender de colecciones genéricas para resolver el núcleo del requisito académico.
- [ ] Se permiten arreglos, nodos propios y clases auxiliares propias.
- [ ] Se permite usar controles de WinForms como `DataGridView` para mostrar datos, siempre que la lógica PED no dependa de eso.
- [ ] Se permite usar colecciones del framework solo para tareas secundarias de UI o interoperabilidad, no como reemplazo de las estructuras evaluadas.

## 4.3 No usar estructuras lineales exclusivamente

- [ ] El proyecto no debe basarse solo en pila, cola o lista.
- [ ] Debe existir uso real de al menos una estructura no lineal o de acceso eficiente.
- [ ] El uso debe poder demostrarse en ejecución o en código.
- [ ] El equipo debe poder explicar por qué esa estructura mejora o resuelve algo.

## 4.4 Proyecto aplicado e innovador

- [ ] El sistema debe resolver un problema práctico del día a día.
- [ ] La innovación debe reflejarse en una feature o lógica útil, no solo en texto.
- [ ] La feature principal debe ser demostrable desde la aplicación.

---

# 5. Requerimientos técnicos planteados para este proyecto

Proyecto: **Sistema de Gestión de Donaciones para Organizaciones Sociales**.

No inventes clases por esta sección. Usa esto como checklist de features. Revisa el código y marca si existen, si están parciales o si faltan.

---

## 5.1 Stack técnico esperado

- [ ] Aplicación de escritorio en C#.
- [ ] Windows Forms o tecnología equivalente si el código actual ya existe así.
- [ ] SQLite como base de datos local.
- [ ] Funcionamiento offline.
- [ ] No requiere servidor externo.
- [ ] Estructuras de datos propias integradas al flujo.

---

## 5.2 Modelo de datos esperado

Verificar si el código o la base de datos maneja entidades equivalentes.

No exigir nombres exactos, pero sí intención funcional.

- [ ] Categorías.
- [ ] Unidades de medida.
- [ ] Productos / donaciones / inventario.
- [ ] Beneficiarios.
- [ ] Tasas de consumo o valores para calcular necesidad.
- [ ] Órdenes, solicitudes o distribuciones.
- [ ] Detalle de asignaciones.
- [ ] Historial de entregas.

---

## 5.3 Requerimientos funcionales

### RF01 — Registro de donaciones / productos

- [ ] Permite registrar producto o donación.
- [ ] Guarda nombre, categoría, cantidad y unidad.
- [ ] Maneja fecha de vencimiento si el modelo actual la incluye.
- [ ] Actualiza inventario disponible.
- [ ] Valida campos obligatorios.

### RF02 — Registro de beneficiarios

- [ ] Permite registrar familias o beneficiarios.
- [ ] Guarda nombre.
- [ ] Guarda número de integrantes o dato equivalente.
- [ ] Maneja estado activo/inactivo o equivalente.
- [ ] Permite buscar beneficiarios.

### RF03 — Cálculo automático de prioridad

- [ ] Existe una lógica para calcular déficit, prioridad o necesidad.
- [ ] La prioridad no es completamente manual.
- [ ] Considera datos del beneficiario.
- [ ] Considera historial o entregas previas si existe.
- [ ] El cálculo puede explicarse en defensa.

### RF04 — Cola de prioridad con MaxHeap

- [ ] Existe una estructura propia tipo MaxHeap/Montículo.
- [ ] Se usa para ordenar beneficiarios por prioridad, déficit o necesidad.
- [ ] No se usa `PriorityQueue` del framework para reemplazarla.
- [ ] La extracción de mayor prioridad está integrada al flujo de distribución.

### RF05 — Asignación de donaciones

- [ ] El sistema asigna recursos a beneficiarios.
- [ ] Descuenta stock del inventario.
- [ ] No permite asignar más de lo disponible.
- [ ] Genera una propuesta o registro de entrega.
- [ ] Maneja errores de stock insuficiente.

### RF06 — Historial de asignaciones

- [ ] Guarda quién recibió.
- [ ] Guarda qué recibió.
- [ ] Guarda cuándo recibió.
- [ ] Guarda cantidad entregada.
- [ ] Guarda déficit/prioridad si el modelo lo contempla.
- [ ] Permite consultar historial.

### RF07 — Alertas de inventario

- [ ] Detecta stock bajo.
- [ ] Muestra advertencia visual o mensaje.
- [ ] Detecta productos próximos a vencer si el modelo tiene vencimiento.
- [ ] No bloquea el sistema innecesariamente.

### RF08 — Importación desde CSV

- [ ] Permite importar productos, donaciones o beneficiarios desde CSV.
- [ ] Valida formato antes de guardar.
- [ ] Detecta duplicados.
- [ ] Usa Tabla Hash propia si aplica.
- [ ] Reporta filas inválidas sin cerrar la aplicación.

### RF09 — Funcionalidad deshacer

- [ ] Permite revertir ajustes manuales antes de confirmar una distribución.
- [ ] Usa Pila propia o estructura equivalente implementada por estudiantes.
- [ ] No usa `Stack<T>` para el núcleo de esta feature.
- [ ] Al confirmar, los cambios quedan permanentes.
- [ ] No permite deshacer distribuciones ya confirmadas si el diseño las declara inmutables.

---

## 5.4 Requerimientos no funcionales

### RNF01 — Usabilidad

- [ ] La interfaz es entendible para usuarios no técnicos.
- [ ] Hay labels y botones claros.
- [ ] Hay mensajes útiles.
- [ ] El flujo principal se puede completar sin explicación del programador.

### RNF02 — Rendimiento

- [ ] Las búsquedas y registros no se sienten lentos.
- [ ] Las estructuras PED aportan mejora o razonamiento algorítmico.
- [ ] No hay loops innecesarios muy evidentes en operaciones simples.

### RNF03 — Disponibilidad offline

- [ ] El sistema funciona sin internet.
- [ ] No depende de APIs externas para operar.
- [ ] SQLite o la persistencia local funciona correctamente.

### RNF04 — Mantenibilidad simple

- [ ] El código está organizado en carpetas o clases claras.
- [ ] La separación es suficiente para nivel universitario.
- [ ] No hay sobreingeniería.
- [ ] No hay arquitectura demasiado difícil de defender.

### RNF05 — Portabilidad

- [ ] El proyecto puede ejecutarse en Windows.
- [ ] Las dependencias están claras.
- [ ] No depende de rutas absolutas del equipo de un integrante.
- [ ] La base de datos se ubica o crea de forma razonable.

### RNF06 — Integridad de datos

- [ ] No permite guardar registros incompletos.
- [ ] No permite cantidades negativas.
- [ ] No permite eliminar datos que rompan relaciones importantes.
- [ ] Maneja duplicados.
- [ ] Mantiene consistencia entre inventario, entregas e historial.

---

# 6. Estilo de código esperado: estudiante universitario, no enterprise

El código debe ser correcto, limpio y defendible, pero no innecesariamente complejo.

## 6.1 Sí hacer

- [ ] Usar clases simples y claras.
- [ ] Usar métodos pequeños cuando ayuden a leer.
- [ ] Usar nombres en español si el proyecto ya está en español.
- [ ] Mantener consistencia con el estilo actual del proyecto.
- [ ] Usar `try/catch` donde hay base de datos, archivos o lógica crítica.
- [ ] Usar `TryParse` para entradas numéricas.
- [ ] Usar mensajes claros con `MessageBox.Show`.
- [ ] Validar antes de guardar.
- [ ] Actualizar grids/listas después de cambios.
- [ ] Comentar solo donde ayude a explicar estructuras o algoritmos.
- [ ] Mantener carpetas simples como `Modelos`, `Servicios`, `Formularios`, `Estructuras` si ya existen o encajan naturalmente.

## 6.2 No hacer

- [ ] No convertir el proyecto en Clean Architecture.
- [ ] No agregar CQRS.
- [ ] No agregar MediatR.
- [ ] No agregar dependency injection avanzada.
- [ ] No agregar Unit of Work.
- [ ] No agregar Repository Pattern complejo si el proyecto no lo usa.
- [ ] No migrar a Entity Framework si el proyecto ya usa SQLite directo.
- [ ] No meter Docker.
- [ ] No convertir a API web.
- [ ] No cambiar WinForms por otra tecnología.
- [ ] No agregar autenticación compleja si no es necesaria.
- [ ] No crear capas innecesarias solo para sonar profesional.
- [ ] No reescribir todo el proyecto.
- [ ] No renombrar clases existentes salvo que sea estrictamente necesario.
- [ ] No introducir librerías externas grandes sin justificación.
- [ ] No usar colecciones genéricas para reemplazar estructuras PED propias.

---

# 7. Criterio “base 10”

Para decir que el proyecto está **base 10 técnico**, deben cumplirse todos estos puntos:

- [ ] Compila sin errores.
- [ ] Abre sin errores.
- [ ] Tiene conexión real a base de datos.
- [ ] Guarda y consulta datos reales.
- [ ] Tiene manejo de errores en operaciones principales.
- [ ] Está dividido en clases y formularios/servicios suficientes.
- [ ] La UI es usable y no parece plantilla vacía.
- [ ] Implementa features principales del proyecto.
- [ ] Usa estructuras de datos propias de forma real.
- [ ] No usa exclusivamente estructuras lineales.
- [ ] No reemplaza las estructuras propias con colecciones preexistentes.
- [ ] El flujo principal puede demostrarse en menos de 5 minutos.
- [ ] Un estudiante del equipo puede explicar el código sin depender de jerga enterprise.
- [ ] Las features prometidas en la propuesta están implementadas o justificadas como alcance parcial.
- [ ] No hay errores críticos evidentes en tiempo de ejecución.

Si falta uno de estos puntos, no digas “base 10”. Di:

> “Está cerca, pero todavía no lo marcaría base 10 por estos riesgos…”

---

# 8. Formato obligatorio de respuesta del LLM al revisar

Cuando revises el código usando este archivo, responde con este formato:

```md
# Diagnóstico técnico PED104

## 1. Veredicto rápido

Estado general: ✅ Base 10 / ⚠️ Cerca de base 10 / ❌ No base 10 todavía

Resumen:
- ...
- ...
- ...

## 2. Checklist de rúbrica final

| Criterio | Estado | Evidencia encontrada | Riesgo | Acción mínima |
|---|---|---|---|---|
| Manejo de errores | ✅/⚠️/❌ | Archivo/método | Bajo/Medio/Alto | ... |
| Codificación | ✅/⚠️/❌ | Archivo/método | Bajo/Medio/Alto | ... |
| Diseño/UI | ✅/⚠️/❌ | Formulario | Bajo/Medio/Alto | ... |
| Estructuras PED | ✅/⚠️/❌ | Archivo/feature | Bajo/Medio/Alto | ... |
| Sistema funcional/DB | ✅/⚠️/❌ | Flujo/DB | Bajo/Medio/Alto | ... |

## 3. Checklist de estructuras de datos

| Estructura | ¿Existe? | ¿Es propia? | ¿Dónde se usa? | ¿Feature real o adorno? | Comentario |
|---|---:|---:|---|---|---|
| Tabla Hash | Sí/No | Sí/No | ... | Real/Adorno | ... |
| MaxHeap/Montículo | Sí/No | Sí/No | ... | Real/Adorno | ... |
| BST/Árbol | Sí/No | Sí/No | ... | Real/Adorno | ... |
| Pila | Sí/No | Sí/No | ... | Real/Adorno | ... |
| Lista enlazada | Sí/No | Sí/No | ... | Real/Adorno | ... |

## 4. Checklist de requerimientos funcionales

| Req | Estado | Evidencia | Falta mínimo |
|---|---|---|---|
| RF01 Registro de donaciones/productos | ✅/⚠️/❌ | ... | ... |
| RF02 Registro de beneficiarios | ✅/⚠️/❌ | ... | ... |
| RF03 Cálculo de prioridad | ✅/⚠️/❌ | ... | ... |
| RF04 Cola de prioridad MaxHeap | ✅/⚠️/❌ | ... | ... |
| RF05 Asignación de donaciones | ✅/⚠️/❌ | ... | ... |
| RF06 Historial | ✅/⚠️/❌ | ... | ... |
| RF07 Alertas inventario | ✅/⚠️/❌ | ... | ... |
| RF08 CSV | ✅/⚠️/❌ | ... | ... |
| RF09 Deshacer | ✅/⚠️/❌ | ... | ... |

## 5. Checklist de restricciones técnicas

| Restricción | Estado | Evidencia | Riesgo |
|---|---|---|---|
| Visual C# | ✅/⚠️/❌ | ... | ... |
| Estructuras propias | ✅/⚠️/❌ | ... | ... |
| No solo lineales | ✅/⚠️/❌ | ... | ... |
| SQLite/local/offline | ✅/⚠️/❌ | ... | ... |
| No uso indebido de colecciones del framework | ✅/⚠️/❌ | ... | ... |

## 6. Problemas críticos ordenados por impacto

1. ...
2. ...
3. ...

## 7. Plan mínimo para llegar a base 10

1. ...
2. ...
3. ...

## 8. Archivos que tocaría si me pides implementar

| Archivo | Motivo | Cambio mínimo |
|---|---|---|
| ... | ... | ... |

## 9. Cosas que NO recomiendo hacer

- ...
- ...
- ...
```

---

# 9. Protocolo antes de modificar código

Si el usuario pide implementar cambios, primero responde con un plan corto.

Antes de tocar código:

- [ ] Confirmar qué feature se va a reparar.
- [ ] Listar archivos que se tocarán.
- [ ] Explicar por qué esos archivos.
- [ ] Mantener cambios pequeños.
- [ ] No reescribir todo.
- [ ] No cambiar estilo general del proyecto.
- [ ] No meter arquitectura enterprise.
- [ ] No romper formularios existentes.
- [ ] No romper base de datos existente.
- [ ] No reemplazar estructuras PED propias.
- [ ] No borrar código sin explicar.

Después de modificar:

- [ ] Indicar qué se cambió.
- [ ] Indicar cómo probarlo.
- [ ] Indicar qué criterio de la rúbrica mejora.
- [ ] Indicar riesgos restantes.

---

# 10. Preguntas que el LLM debe poder contestar para defensa técnica

Aunque no se esté revisando la defensa oral, el código debe permitir responder estas preguntas:

- ¿Dónde se manejan errores?
- ¿Dónde se conecta a la base de datos?
- ¿Qué pasa si ingreso datos inválidos?
- ¿Qué estructura de datos propia usa el sistema?
- ¿Dónde se usa realmente esa estructura?
- ¿Por qué esa estructura es mejor que hacer todo con listas?
- ¿Qué parte del sistema demuestra innovación?
- ¿Cómo se calcula la prioridad o déficit?
- ¿Cómo se evita entregar más de lo disponible?
- ¿Cómo se guarda el historial?
- ¿Cómo se puede comprobar que el sistema funciona?

Si una pregunta no se puede contestar con el código actual, marcarlo como riesgo.

---

# 11. Escala de estado

Usa estos símbolos:

- ✅ Cumple claramente.
- ⚠️ Parcial, incompleto o con riesgo.
- ❌ No cumple.
- ❓ No se pudo verificar con los archivos disponibles.

No uses ✅ si no hay evidencia en código.

---

# 12. Regla final

El objetivo no es hacer el sistema más complejo.

El objetivo es que el proyecto:

1. Compile.
2. Funcione.
3. Use estructuras propias.
4. Tenga manejo de errores.
5. Sea fácil de usar.
6. Sea defendible por estudiantes.
7. Cumpla la rúbrica técnica con cambios mínimos.
