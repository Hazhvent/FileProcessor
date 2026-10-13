# 🚀 FileProcessor API - Flujo de Póliza Electrónica

Servicio de alto rendimiento de procesamiento masivo, conversión, auditoría y distribución de documentos digitales desarrollado sobre **.NET 10**. Esta solución surge de la necesidad de modernizar, desacoplar y optimizar el **Flujo de Póliza Electrónica**, eliminando cuellos de botella críticos del sistema heredado y proporcionando una arquitectura resiliente, escalable y mantenible.

---

## 🔄 1. Flujo de Póliza Electrónica

Esta refactorización responde a la necesidad de modernizar, desacoplar y optimizar la emisión masiva de certificados y pólizas electrónicas, reemplazando un sistema heredado acoplado por un servicio asíncrono de alto rendimiento.

1. **Ingreso de Solicitud**: Recepción de cargas masivas en formato JSON (REST) o llamadas legacy WCF/SOAP con datos del cliente y póliza.
2. **Transformación y Ensamblado**: Extracción de datos en Oracle DB, reemplazo dinámico de etiquetas en plantillas RTF y renderizado en memoria a PDF.
3. **Reglas de Aprobación**: Evaluación automática por montos y monedas para definir si el documento pasa directo a firma o requiere revisión humana.
4. **Almacenamiento en NAS**: Depósito ordenado en carpetas compartidas (`Pendientes`, `Prioridad`, `Completado`).
5. **Firma Digital**: Procesamiento del archivo en el NAS a través del motor externo de firma (Pegasus).
6. **Notificación Final**: Envío del documento firmado y cifrado al cliente final mediante correo electrónico.

```text
[1. Ingesta Request]      [2. Transformación]       [3. Reglas & Filtros]
  📥 Cliente / SOAP  ──►   ⚙️ Oracle DB + RTF ──►    ⚖️ Evaluación Montos
                           HTML → PDF Engine        / VIP Priorización
                                                           │
                                                           ▼
[6. Notificación]         [5. Firma Digital]        [4. Depósito NAS]
  📬 Correo Cliente  ◄──   🔏 Motor Pegasus    ◄──   📁 Almacén SMB/NAS

```

### ⚠️ Problemas del Monolito Legado (`WSDocumentoExterno`)

* **Inestabilidad por MS Office Interop**: La conversión RTF a PDF dependía de instalar MS Word en el IIS. Generaba bloqueos COM (`0x8000401A`) y requería reintentos constantes con `Thread.Sleep(3000)`, provocando congelamientos en producción.
* **Código Acoplado**: La clase `CargaDocumentoExternoCore.cs` concentraba más de 1,500 líneas monolíticas mezclando acceso a datos, manipulación de archivos y reglas de negocio.
* **Procesamiento Bloqueante**: Operaciones I/O síncronas directamente sobre el sistema de archivos de red que saturaban los hilos del servidor.
* **Manejo Frágil de Excepciones**: Capturas genéricas de errores que ocultaban la causa raíz de los fallos.

### 📊 Comparativa de Arquitectura

| Característica | Monolito Legado (.NET Framework 4.5) | Nueva API FileProcessor (.NET 10) |
| --- | --- | --- |
| **Conversión RTF → PDF** | MS Word COM Interop (`Winword.exe`) | `RtfPipe` + `PuppeteerSharp` (Headless Chromium) |
| **Estabilidad de Conversión** | Propensa a cuellos de botella y congelamientos | 100% en memoria, asíncrona y aislada |
| **Arquitectura** | Monolito procedural (`CargaDocumentoExternoCore`) | Clean Architecture + CQRS + Strategy + MediatR |
| **Protocolos de Entrada** | Servicio WCF rígido | REST Minimal APIs + SOAP Core WCF Emulation |
| **Conexión a Red (NAS)** | Mapeo frágil de unidades Windows | `SMBLibrary` nativo (SMB1/SMB2/SMB3) |
| **Validación de Datos** | Validaciones `if-else` manuales con excepciones | `FluentValidation` + Pipeline Behaviors |
| **Acceso a Datos** | ADO.NET acoplado manual | `Dapper` + `Oracle.ManagedDataAccess.Core` |

---

## 🏛️ 2. Arquitectura del Sistema (Clean Architecture)

La solución adopta **Clean Architecture**, aislando completamente las reglas de negocio de los detalles de infraestructura:

```text
FileProcessor.sln
│
├── 📂 FileProcessor (Web / API Host)
│   ├── 📂 Chronium/                    # Ejecutable embebido de Chromium a nivel de proyecto
│   ├── Program.cs                      # Entry Point & Pipeline Builder
│   └── appsettings.json                # Configuración global
│
├── 📂 FileProcessor.Application (Capa de Aplicación)
│   ├── Actions/                        # Sub-Handlers (Macro Actions) para descomponer lógica
│   ├── Configuration/                  # Métodos de extensión para DI (Api, Soap, MediatR, Db, etc.)
│   ├── Pipeline/Behaviors/             # Open Behaviors de MediatR (ValidationBehavior)
│   ├── Pipeline/Middlewares/            # Middlewares globales (GlobalExceptionMiddleware)
│   └── UseCases/                       # Handlers principales CQRS (ProcessDocumentBatchHandler)
│
├── 📂 FileProcessor.Core (Capa de Dominio y Abstracciones)
│   ├── Constants/                      # Constantes de API, Mensajes, Rutas y Validación
│   ├── Dto/ & Models/                  # Data Transfer Objects, Entidades y Settings
│   ├── Exceptions/                     # Excepciones de Dominio (BadRequest, NotFound, InternalServer)
│   └── Interfaces/                     # Contratos de Repositorios y Servicios (IPdfProcessorService, etc.)
│
└── 📂 FileProcessor.Infrastructure (Capa de Infraestructura)
    ├── Persistence/                    # Repositorios Dapper, DDL Oracle y DatabaseConfiguration
    └── Services/                       # Implementación de Puppeteer, PdfSharp, SMB, MailKit, Logging

```

---

## 🧩 3. Patrones de Diseño y Orquestación del Caso de Uso

### 🔀 1. CQRS y Descomposición en "Macro Actions"

El caso de uso principal (`ProcessDocumentBatchHandler`) actúa como un **Orquestador de Dominio**. Siguiendo el principio de Responsabilidad Única (SRP), delega submódulos de trabajo a través del `ISender` de MediatR ejecutando acciones atómicas (*Macro Actions*):

```text
ProcessDocumentBatchRequest (Mediator)
  │
  ├──► PrepareNetworkDirectoriesAction   (Conexión NAS y preparación de rutas)
  ├──► ProcessUserGreetingsAction        (Personalización de saludos por género/DNI)
  ├──► Workflow Branching (Según TipoConsulta):
  │     ├── ProcessExternalFileAction   (Carga/validación de archivos externos)
  │     ├── ProcessCampoFeAction         (Transformación de plantillas RTF Campo Fe)
  │     └── ProcessCajaLosAndesAction    (Transformación de plantillas RTF Los Andes)
  ├──► StoreAndAuditDocumentAction       (Escritura en NAS, Bitácora y Reglas de Aprobación)
  ├──► SendPendingApprovalEmailsAction   (Notificaciones SMTP a revisores)
  └──► RegisterBatchUsersAction          (Asociación final de lote y credenciales)

```

### 🎯 2. Patrón Strategy (Estrategias de Procesamiento)

Para eliminar el bloque condicional gigante del monolito anterior, se implementó el patrón **Strategy**. El orquestador evalúa las propiedades del documento (`TipoConsulta`) y selecciona en tiempo de ejecución la estrategia adecuada de generación y parseo:

* **Estrategia Archivo Importado (`ProcessExternalFileAction`)**: Procesa PDFs preexistentes de origen externo, validando restricciones de peso (30MB) y ruteo a carpetas de rechazo/aprobación.
* **Estrategia Plantilla RTF Campo Fe (`ProcessCampoFeAction`)**: Extrae la información de la póliza/certificado, sustituye variables en la plantilla RTF, la transforma a HTML y la renderiza a PDF vía Chromium Headless.
* **Estrategia Tabla Intermedia Los Andes (`ProcessCajaLosAndesAction`)**: Lee la configuración dinámica desde tablas intermedias de clientes, mapeando beneficiarios y planes para su posterior renderizado HTML → PDF.

### 🔝 3. Priorización Dinámica de Documentos

El orquestador reordena automáticamente los documentos recibidos en el lote evaluando coincidencias contra palabras clave configuradas (`PriorityWordsSettings`: `RPKit`, `VIP`, `URGENTE`), garantizando que los archivos de alta prioridad se procesen primero:

```csharp
var docs = request.Documentos.OrderByDescending(doc =>
    _priorityWordsSettings.Keys.Any(kw =>
        !string.IsNullOrWhiteSpace(kw) &&
        doc.Nombre.Contains(kw, StringComparison.OrdinalIgnoreCase))
).ToList();

```

### 🛡️ 4. Validaciones en Pipeline (Open Behaviors)

Toda solicitud entrante es interceptada por el `ValidationBehavior<,>`, ejecutando las reglas registradas con **FluentValidation** antes de llegar al Handler. Cualquier inconsistencia detiene la ejecución de forma segura y devuelve un error formateado a través del `GlobalExceptionMiddleware`.

---

## 🛠️ 4. Stack Tecnológico

| Categoría | Tecnología / Librería | Versión | Propósito |
| --- | --- | --- | --- |
| ⚡ **Runtime** | **.NET 10.0** | `10.0` | Framework base con C# 14, `ImplicitUsings`, `Nullable` y `AllowUnsafeBlocks`. |
| 🔀 **Arquitectura** | **MediatR** | `12.5.0` | Implementación del patrón Mediator, CQRS y pipeline behaviors. |
| 🛡️ **Validación** | **FluentValidation** | `12.1.1` | Validación fuertemente tipada de DTOs y comandos. |
| 🌐 **REST APIs** | **MinimalApis.Discovery** | `1.0.7` | Descubrimiento y registro automático de Minimal APIs. |
| 🔌 **SOAP Engine** | **SoapCore** | `1.2.1.16` | Emulación y alojamiento de endpoints SOAP/WCF sobre ASP.NET Core. |
| 🗄️ **Base de Datos** | **Oracle.ManagedDataAccess.Core** | `23.26.301` | Driver oficial gestionado para conexión con Oracle DB. |
| 🚀 **ORM / Querying** | **Dapper** | `2.1.86` | Micro-ORM de alto rendimiento para consultas y procedimientos almacenados. |
| 📑 **Conversion Pipeline** | **RtfPipe** | `2.0.7677` | Transformación de plantillas RTF a HTML. |
| 🌐 **PDF Generation** | **PuppeteerSharp** | `25.11.0` | Motor Chromium Headless para renderizar HTML a PDF de alta fidelidad. |
| 📄 **PDF Processing** | **PDFsharp** | `6.2.4` | Ensamblado de páginas, compresión de streams y manejo de documentos PDF. |
| 📁 **Almacenamiento Red** | **SMBLibrary** | `1.5.8.1` | Cliente nativo SMB1/SMB2/SMB3 para integración con servidores NAS. |
| 📬 **Notificaciones** | **MailKit** | `4.18.0` | Motor robusto para armado y envío de correos SMTP con adjuntos. |
| 📝 **Logging** | **Serilog.AspNetCore** | `10.0.0` | Logging estructurado hacia consola y archivos físicos rotativos. |
| 📖 **Documentación** | **Swashbuckle.AspNetCore** | `10.2.3` | Generación de especificación OpenAPI y Swagger UI interactivo. |

---

## ⚙️ 5. Desglose Detallado de Macro Actions (`Actions/`)

La carpeta `Actions/` contiene los sub-handlers modulares encargados de resolver tareas específicas de negocio:

* 📂 **`PrepareNetworkDirectoriesAction`**: Establece la conexión SMB con el almacenamiento NAS, creando o verificando las rutas físicas de trabajo.
* 👤 **`ProcessUserGreetingsAction`**: Consulta los datos de destinatarios en Oracle e inyecta saludos personalizados (*"Estimado Sr."*, *"Estimada Sra."*).
* 📄 **`ProcessExternalFileAction`**: Gestiona la ingesta de PDFs externos, verificando límites de peso (30MB), rechazos y carpetas de destino.
* ✝️ **`ProcessCampoFeAction`**: Genera documentos basados en la plantilla RTF "Campo Fe" mediante renderizado HTML/Chromium.
* 🏔️ **`ProcessCajaLosAndesAction`**: Procesa certificados dinámicos desde tablas intermedias de configuración ("Los Andes").
* 💾 **`StoreAndAuditDocumentAction`**: Deposita los PDFs en el almacenamiento masivo (NAS) e inserta registros de auditoría y bitácora.
* ⚖️ **`GetApprovalRuleAndReviewersAction`**: Evalúa reglas de aprobación en base a suma asegurada/moneda y obtiene los correos revisores.
* 📝 **`RegisterPolicyAction`**: Mapea e inserta los datos de la póliza/certificado procesado en la base de datos Oracle.
* ✉️ **`SendPendingApprovalEmailsAction`**: Dispara notificaciones por correo electrónico a los revisores asignados al lote.
* 👥 **`RegisterBatchUsersAction`**: Registra la asociación final de los usuarios externos con el lote generado.

---

## 🔧 6. Configuración del Sistema (`appsettings.json`)

```json
{
  "ConnectionStrings": {
    "Database": "Data Source=<DB_HOST>:<DB_PORT>/<SERVICE_NAME>;User Id=<DB_USER>;Password=<DB_PASSWORD>;"
  },
  "ApplicationSettings": {
    "ApiName": "FileProcessor",
    "ApiVersion": "v0.1.0-alpha",
    "Description": "Api de procesamiento de documentos para el flujo de poliza electronica",
    "ContactName": "Cesar Galvez",
    "ContactEmail": "cesar.galvez@materiagris.pe",
    "LogDirectoryPath": "C:\\Logs\\FileProcessor\\fileprocessor-.log"
  },
  "PriorityWordsSettings": {
    "Keys": [ "RPKit", "VIP", "URGENTE" ]
  },
  "NASSettings": {
    "Host": "<NAS_HOST>",
    "Domain": "<DOMAIN>",
    "NewUsername": "<NAS_USER>",
    "NewPassword": "<NAS_PASSWORD>",
    "OldUsername": "<NAS_OLD_USER>",
    "OldPassword": "<NAS_OLD_PASSWORD>",
    "ShareFolder": "PRD"
  },
  "SoapSettings": {
    "EndpointPath": "/WsGenerarPDFexterno.svc"
  }
}

```

---

## ⚙️ 7. Pipeline y Extensibilidad (`Program.cs`)

El punto de entrada `Program.cs` delega la configuración del pipeline a métodos de extensión fuertemente tipados:

```csharp
var builder = WebApplication.CreateBuilder(args);

// Extensiones de builder (Servicios & DI)
builder.AddApi();          // Codificación, JSON, IHttpContextAccessor y servicios transversales
builder.AddLogs();         // Configura Serilog (Consola + Archivo rotativo)
builder.AddSwagger();      // Especificación OpenAPI enriched con información REST y SOAP
builder.AddMediatR();      // Handlers, Validators y ValidationBehavior
builder.AddDataBase();     // Registra OracleConnection (IDbConnection Scoped) y IRepository
builder.AddSoap();         // Configura SoapCore para exponer IWsGenerarPDFexterno

var app = builder.Build();

// Extensiones de app (Middlewares & Mapeos)
app.UseApiConfiguration();      // HTTPS Redirection, GlobalExceptionMiddleware, MapApis()
app.UseSwaggerConfiguration();  // Swagger UI con filtros y configuración personalizada
app.UseSoapConfiguration();     // Mapeo del endpoint SOAP /WsGenerarPDFexterno.svc

await app.RunAsync();

```

---

## 🗄️ 8. Configuración de Base de Datos (`DatabaseConfiguration`)

La persistencia utiliza Dapper sobre una conexión gestionada a Oracle DB. La conexión se registra con vida útil `Scoped` para garantizar que se mantenga viva durante la petición HTTP y se libere automáticamente al finalizar:

```csharp
builder.Services.AddScoped<IDbConnection>(sp =>
{
    var connection = new OracleConnection(connectionString);
    return connection;
});

```

---

## 📦 9. Requisitos de Despliegue y Chromium Embebido

Para permitir la conversión de documentos en entornos de servidor aislados (sin acceso a Internet), la aplicación incluye el ejecutable de Chromium embebido directamente a nivel de proyecto dentro de la carpeta `Chronium`. El archivo de proyecto `.csproj` está configurado para copiar automáticamente todo el contenido de esta carpeta al directorio de salida de compilación (`bin/`):

```xml
<ItemGroup>
  <None Include="Chronium\**\*">
    <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
  </None>
</ItemGroup>

```

---

## 🚦 10. Guía de Ejecución Local

1. **Prerrequisitos**:
* Tener instalado **.NET 10 SDK**.
* Conexión a red/VPN con acceso a Oracle DB y servidor NAS.


2. **Restaurar paquetes NuGet**:
```bash
dotnet restore

```


3. **Compilar la solución**:
```bash
dotnet build

```


4. **Ejecutar la API localmente**:
```bash
dotnet run --project FileProcessor/FileProcessor.csproj

```


5. **Probar Endpoints**:
* **Swagger UI (REST)**: `https://localhost:{port}/swagger`
* **WSDL (SOAP Legacy)**: `https://localhost:{port}/WsGenerarPDFexterno.svc?wsdl`


6. **Publicar para Despliegue (Release)**:
```bash
dotnet publish FileProcessor/FileProcessor.csproj -c Release -o ./publish

```
