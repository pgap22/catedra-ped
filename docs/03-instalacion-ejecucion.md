# Instalacion y ejecucion

## Requisitos

| Requisito | Motivo |
|---|---|
| Windows | El proyecto usa Windows Forms y target `net10.0-windows`. |
| SDK .NET compatible con `net10.0-windows` | Necesario para restaurar, compilar y ejecutar. |
| Acceso de escritura en la carpeta de ejecucion | SQLite crea o actualiza `donaciones.db`. |
| Paquete `System.Data.SQLite.Core` | Driver SQLite usado por los servicios. |

## Archivos de proyecto

| Archivo | Funcion |
|---|---|
| `ProyectoCatedra.sln` | Solucion de Visual Studio / .NET CLI. |
| `ProyectoCatedra.csproj` | Configuracion del proyecto WinForms. |
| `Program.cs` | Punto de entrada. |

## Restaurar dependencias

Desde la raiz del repositorio:

```bash
dotnet restore C:\Users\Gerardo\udb\ProyectoCatedra\ProyectoCatedra.sln
```

Que hace:

1. Lee `ProyectoCatedra.sln`.
2. Lee `ProyectoCatedra.csproj`.
3. Descarga el paquete `System.Data.SQLite.Core` si no esta restaurado.
4. Deja el proyecto listo para compilar.

## Compilar

```bash
dotnet build C:\Users\Gerardo\udb\ProyectoCatedra\ProyectoCatedra.sln
```

El proyecto genera salida como aplicacion Windows porque tiene:

```xml
<OutputType>WinExe</OutputType>
```

## Ejecutar

```bash
dotnet run --project C:\Users\Gerardo\udb\ProyectoCatedra\ProyectoCatedra.csproj
```

Al ejecutar, se abre `Form1`, que muestra el menu principal del sistema.

## Base de datos local

La base se llama `donaciones.db` y se configura en `Datos/ConexionDB.cs`:

```csharp
private string nombreBD = "donaciones.db";
cadenaConexion = $"Data Source={nombreBD};Version=3;";
```

Punto importante para juniors:

La ruta no es absoluta. SQLite busca o crea `donaciones.db` en el directorio desde donde se ejecuta el proceso. Si se ejecuta desde otra carpeta, puede aparecer otra base vacia.

## Inicializacion automatica

Cada servicio crea una instancia de `ConexionDB`. El constructor de `ConexionDB` hace:

```text
new ConexionDB()
  |
  +--> InicializarBD()
  |
  +--> ActualizarEsquema()
```

`InicializarBD()` crea el archivo SQLite si no existe.

`ActualizarEsquema()` crea tablas nuevas y agrega columnas faltantes con `ALTER TABLE`.

## Archivos ignorados por Git

El `.gitignore` ignora archivos generados y locales:

```text
bin/
obj/
donaciones.db
.vs/
*.user
*.suo
```

Esto es correcto porque:

- `bin/` y `obj/` son salidas de compilacion.
- `donaciones.db` contiene datos locales de prueba o produccion.
- `.vs/` y archivos `.user` son configuraciones personales de Visual Studio.

## Flujo recomendado para probar desde cero

1. Ejecutar la aplicacion.
2. Abrir `Dev / Demo > Sembrar Demo Pequena`.
3. Confirmar el aviso de borrado.
4. Esperar reinicio de la aplicacion.
5. Abrir `Ayuda Social > Generar Asignacion (Reparto)`.
6. Presionar `Generar Propuesta Automatica`.
7. Revisar filas sugeridas.
8. Confirmar una familia o todos los mostrados.
9. Abrir `Ayuda Social > Historial de Entregas Realizadas`.
10. Verificar que la entrega quedo guardada.

## Problemas comunes

| Sintoma | Causa probable | Que revisar |
|---|---|---|
| No compila por target Windows | Falta SDK compatible o se ejecuta en entorno no Windows. | `dotnet --info`. |
| Abre una base sin datos | Se ejecuto desde otro directorio y creo otro `donaciones.db`. | Carpeta de ejecucion. |
| No genera propuesta | Faltan tasas, packs, stock o beneficiarios activos. | `FormDistribucion` muestra diagnostico. |
| Pack no guarda | Porcentajes no suman exactamente 100. | `FormConfigurarPacks`. |
| No descuenta stock | La entrega no fue confirmada en `FormConfirmacionDistribucion`. | Historial y stock de productos. |
