# OpinionETL — Sistema ETL de opiniones de clientes

Proyecto académico desarrollado en **C# y .NET** para extraer opiniones y datos maestros desde archivos CSV, normalizarlos, validar su integridad y cargarlos en una base de datos centralizada de **SQL Server**. La ejecución se realiza mediante un **Worker Service**, con **Entity Framework Core** para persistencia y **CsvHelper** para lectura de archivos.

## Objetivo

Centralizar opiniones de distintas procedencias en un modelo relacional consistente que permita consultar clasificaciones, puntuaciones, productos, clientes y fuentes de datos. El proceso conserva los registros originales en una tabla de *staging* antes de transformarlos.

## Tecnologías

- C# / .NET 10
- .NET Worker Service (`BackgroundService`)
- Entity Framework Core con proveedor de SQL Server
- SQL Server / SQL Server LocalDB
- CsvHelper
- Inyección de dependencias (`Microsoft.Extensions.DependencyInjection`)
- Logging estructurado (`ILogger<T>`) y medición de tiempos (`Stopwatch`)

## Estructura de la solución

```text
OpinionETL/
├── OpinionETL.Worker/
│   ├── Program.cs              # Host, configuración y servicios
│   ├── Worker.cs               # Punto de entrada de la ejecución
│   └── data/                   # Archivos CSV de entrada (copiados al directorio de salida)
├── OpinionETL.Core/
│   ├── Interfaces/             # Contratos de importadores y pipeline
│   ├── Models/                 # Entidades y resultados de importación
│   └── Services/
│       └── EtlPipeline.cs      # Coordinación de etapas
└── OpinionETL.Data/
    ├── Context/                # AppDbContext
    ├── Extraction/Csv/
    │   ├── Models/             # Modelos de filas CSV
    │   └── Mapping/            # Mapas CsvHelper
    ├── Helpers/                # Normalizadores y clasificador
    ├── Importers/              # Importadores y OpinionProcessor
    └── ...                     # Registro de dependencias
```

> Las rutas pueden variar ligeramente según la organización de carpetas del repositorio. Los CSV deben estar disponibles en `Path.Combine(AppContext.BaseDirectory, "data")` en tiempo de ejecución.

## Archivos de entrada

| Archivo | Contenido | Registros del conjunto de prueba |
|---|---|---:|
| `clients.csv` | Clientes | 500 |
| `products.csv` | Productos y categorías | 200 |
| `fuente_datos.csv` | Identificadores y tipos de fuentes | 100 |
| `surveys_part1.csv` | Encuestas | 500 |
| `web_reviews.csv` | Reseñas web | 200 |
| `social_comments.csv` | Comentarios de redes sociales | 200 |

En el conjunto de prueba se procesaron **900 opiniones** (500 encuestas, 200 reseñas web y 200 comentarios sociales).

## Flujo ETL

```text
Archivos CSV
     │
     ├── Catálogos: categorías, tipos de fuente y clasificaciones
     ├── Maestros: clientes y productos
     ├── Fuentes: Data_Source
     │
     └── Opiniones de tres procedencias
                │
          CsvExtractor + CsvMap
                │
          OpinionStagingImporter
                │
          Stage_Opinion (Pending)
                │
           OpinionProcessor
                ├── Normalización de IDs y fechas
                ├── Validación de puntuaciones
                ├── Resolución de claves foráneas
                ├── Clasificación de opiniones
                └── Resolución de fuentes
                │
           Opinion (tabla final)
                │
          Stage_Opinion (Processed / Rejected)
```

El `EtlPipeline` ejecuta secuencialmente los importadores y, al final, `OpinionProcessor`. El orden respeta las dependencias entre catálogos, datos maestros y opiniones.

## Reglas de transformación y calidad

- **Staging:** conserva los valores originales, incluso cuando tengan formatos diferentes. La identificación de registros usa la combinación `Origin` + `Original_Opinion_ID`.
- **Normalización de IDs:** convierte formatos como `C007`, `P016` y `8537` a enteros. La existencia de ese entero en una tabla maestra debe comprobarse antes de asignar la FK. **Una coincidencia numérica no garantiza que dos sistemas compartan la misma identidad**; la correspondencia entre orígenes debe estar validada.
- **Clientes y productos opcionales:** si no existe una correspondencia válida, la opinión puede conservarse con `Client_ID` o `Product_ID` en `NULL`.
- **Fechas y puntuaciones:** se transforman mediante helpers antes de crear la opinión definitiva.
- **Clasificación:** se utiliza el valor original reconocido o una regla basada en puntuación; los casos sin esos datos emplean la lógica definida en `OpinionClassifier`.
- **Fuentes:** los registros de `fuente_datos.csv` conservan su identificador externo. Para plataformas específicas como Instagram, Twitter y Facebook pueden crearse nuevas filas en `Data_Source` con `External_ID = NULL`.
- **Estado de Staging:** `Pending`, `Processed` o `Rejected`, con motivo de error cuando corresponde.
- **Idempotencia:** los importadores evitan duplicados y `OpinionProcessor` relaciona cada opinión final con `Stage_Opinion_ID`. Se recomienda una restricción única sobre esta última columna.

## Modelo de datos

Tablas principales:

| Tabla | Propósito |
|---|---|
| `Client` | Datos maestros de clientes |
| `Category` | Categorías de productos |
| `Product` | Productos asociados a categorías |
| `Source_Type` | Tipos de fuente (CSV, Web, Red Social) |
| `Classification` | Positiva, Negativa y Neutra |
| `Data_Source` | Origen de la información, con ID externo opcional |
| `Stage_Opinion` | Registros originales y estado de procesamiento |
| `Opinion` | Opiniones transformadas con claves foráneas y referencia a Staging |

La tabla `Opinion` contiene los campos `ID`, `Comment`, `Satisfaction_Score`, `Date`, `Client_ID`, `Product_ID`, `Data_Source_ID`, `Classification_ID` y `Stage_Opinion_ID`.

## Configuración y ejecución

### Requisitos

1. SDK de .NET 10.
2. SQL Server o SQL Server LocalDB.
3. Base de datos y tablas creadas con el script SQL del proyecto.
4. Archivos CSV disponibles en el directorio `data` del Worker y configurados para copiarse a la carpeta de salida.

### Cadena de conexión

El proyecto utiliza la clave `ConnectionStrings:DefaultConnection`. Se recomienda configurarla mediante **User Secrets**, sin subir credenciales al repositorio:

```bash
cd OpinionETL.Worker
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=(localdb)\\MSSQLLocalDB;Database=OpinionETL;Trusted_Connection=True;TrustServerCertificate=True;"
```

Ajusta el nombre de la base de datos y del servidor a tu entorno.

### Compilar y ejecutar

Desde la carpeta de la solución:

```bash
dotnet restore
dotnet build
dotnet run --project OpinionETL.Worker
```

Al arrancar, el Worker resuelve `IEtlPipeline` mediante un scope y ejecuta las etapas en orden. La versión actual está diseñada para una ejecución por arranque, no para una programación periódica.

### Logs

El pipeline registra inicio y fin de etapas, filas leídas/importadas/duplicadas/rechazadas, duración y errores. Para reducir el ruido de consultas de Entity Framework Core:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.Hosting.Lifetime": "Information",
      "Microsoft.EntityFrameworkCore": "Warning"
    }
  }
}
```

Esta configuración puede incluirse en `OpinionETL.Worker/appsettings.json`.

## Consideraciones

Este proyecto se desarrolló con fines académicos. Antes de llevarlo a producción se deberían revisar la equivalencia de identificadores entre sistemas, las restricciones e índices de integridad, la concurrencia entre ejecuciones, el tratamiento de datos personales y la clasificación de texto libre.
