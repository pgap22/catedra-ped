# Documentacion tecnica - Sistema de Donaciones

Esta carpeta documenta el proyecto real ubicado en la raiz del repositorio. El objetivo es que una persona junior pueda leer los capitulos en orden y entender que hace el sistema, como fluye la informacion, que responsabilidad tiene cada archivo y como se calcula la distribucion automatica de ayuda.

La documentacion se basa en el codigo inspeccionado, no en suposiciones. Cuando un comportamiento proviene de un archivo concreto, se indica la ruta.

## Como leer esta documentacion

Si nunca viste el proyecto, lee en este orden:

1. `01-resumen-ejecutivo.md`: idea general del sistema.
2. `02-arquitectura-general.md`: mapa mental de carpetas, capas y flujo.
3. `03-instalacion-ejecucion.md`: como restaurar, compilar y ejecutar.
4. `04-base-de-datos.md`: tablas SQLite y relaciones.
5. `05-modelos-y-estructuras.md`: clases de datos y estructuras propias.
6. `06-servicios.md`: logica de negocio y acceso a datos.
7. `07-pantallas-ui.md`: cada pantalla, campos, botones y comportamiento.
8. `08-flujos-de-uso.md`: casos de uso completos de punta a punta.
9. `09-algoritmo-distribucion.md`: algoritmo de distribucion paso a paso.
10. `10-persistencia-e-historial.md`: confirmacion, stock e historial.
11. `11-importacion-csv-y-datos-demo.md`: CSV, plantillas y semillas demo.
12. `12-riesgos-limitaciones-testing.md`: riesgos tecnicos y pruebas faltantes.
13. `13-glosario.md`: diccionario de terminos del proyecto.

## Mapa rapido de capitulos

| Capitulo | Archivo | Pregunta que responde |
|---|---|---|
| 00 | `00-indice.md` | Donde esta cada tema. |
| 01 | `01-resumen-ejecutivo.md` | Que problema resuelve la aplicacion. |
| 02 | `02-arquitectura-general.md` | Como se conectan UI, servicios, modelos, estructuras y SQLite. |
| 03 | `03-instalacion-ejecucion.md` | Como correr el proyecto en Windows con .NET. |
| 04 | `04-base-de-datos.md` | Que tablas existen y para que sirve cada campo. |
| 05 | `05-modelos-y-estructuras.md` | Que representa cada clase y estructura de datos. |
| 06 | `06-servicios.md` | Que hace cada servicio y que metodos expone. |
| 07 | `07-pantallas-ui.md` | Que hace cada formulario y como interactua el usuario. |
| 08 | `08-flujos-de-uso.md` | Como se ejecutan los casos de uso reales. |
| 09 | `09-algoritmo-distribucion.md` | Como se calcula, prioriza y divide la ayuda. |
| 10 | `10-persistencia-e-historial.md` | Como se confirma una entrega y se guarda trazabilidad. |
| 11 | `11-importacion-csv-y-datos-demo.md` | Como entran datos masivos y como se siembra una demo. |
| 12 | `12-riesgos-limitaciones-testing.md` | Que fragilidades existen y como probarlas. |
| 13 | `13-glosario.md` | Que significa cada palabra tecnica o de negocio. |

## Cobertura de lo que pidio el usuario

| Tema pedido | Donde esta documentado |
|---|---|
| Cada pantalla | `07-pantallas-ui.md` |
| Cada servicio | `06-servicios.md` |
| Cada estructura | `05-modelos-y-estructuras.md` |
| Cada caso de uso | `08-flujos-de-uso.md` |
| Cada UI | `07-pantallas-ui.md` |
| Algoritmo de distribucion paso a paso | `09-algoritmo-distribucion.md` |
| Persistencia e historial | `04-base-de-datos.md`, `10-persistencia-e-historial.md` |
| CSV y datos demo | `11-importacion-csv-y-datos-demo.md` |
| Diccionario para juniors | `13-glosario.md` |
| Riesgos y pruebas | `12-riesgos-limitaciones-testing.md` |

## Convenciones usadas

`Ruta` significa archivo o carpeta real del repositorio.

`Modelo` significa una clase simple que representa datos, por ejemplo `Beneficiario` o `Producto`.

`Servicio` significa una clase que consulta SQLite o ejecuta logica de negocio, por ejemplo `DistribucionServicio`.

`Formulario` significa una pantalla WinForms, por ejemplo `FormProductos`.

`Estructura propia` significa una estructura implementada manualmente en `Estructuras/`, no una coleccion generica de C#.

## Diagrama global minimo

```text
Usuario
  |
  v
Form1 - menu principal
  |
  +--> Formularios CRUD: categorias, unidades, productos, beneficiarios
  |
  +--> Formularios de reglas: tasas de consumo, packs por categoria
  |
  +--> FormDistribucion
          |
          v
     DistribucionServicio
          |
          +--> BeneficiarioServicio
          +--> TasaConsumoServicio
          +--> CategoriaPackServicio
          +--> HistorialServicio
          +--> ProductoServicio
          |
          v
       SQLite donaciones.db
```

## Archivos principales del proyecto

| Archivo | Rol |
|---|---|
| `Program.cs` | Punto de entrada. Inicializa WinForms y abre `Form1`. |
| `Form1.cs` | Menu principal de navegacion. |
| `Datos/ConexionDB.cs` | Conexion SQLite, creacion de tablas y migraciones simples. |
| `Servicios/DistribucionServicio.cs` | Corazon del algoritmo de distribucion. |
| `FormDistribucion.cs` | Pantalla donde se genera, ajusta y confirma la propuesta. |
| `Estructuras/MonticuloMaximo.cs` | Heap maximo para priorizar beneficiarios. |
| `Modelos/OrdenDetalle.cs` | Linea concreta de entrega sugerida o confirmada. |
