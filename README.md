# Sistema de Gestión de Pedidos - .NET 8 Clean Architecture

Una solución completa de gestión de pedidos implementada con **Clean Architecture**, **.NET 8**, **PostgreSQL**, **RabbitMQ** y **Docker**.

## 🏗️ Arquitectura

```
/src
  /OrderManagement.Api           # ASP.NET Core Web API
  /OrderManagement.Application   # Casos de uso, DTOs, validaciones
  /OrderManagement.Domain        # Entidades, reglas de negocio
  /OrderManagement.Infrastructure # EF Core, repositorios, servicios externos
  /OrderManagement.Consumer      # Worker Service para eventos
/tests
  /OrderManagement.Tests         # Pruebas unitarias (xUnit)
/docker
  Dockerfile.api                 # Imagen Docker para API
  Dockerfile.consumer            # Imagen Docker para Consumer
  docker-compose.yml             # Orquestación completa
```

## 🚀 Tecnologías

- **.NET 8** - Framework principal
- **PostgreSQL** (Npgsql) - Base de datos principal
- **Entity Framework Core** - ORM con Code First
- **RabbitMQ** - Message broker para eventos
- **MailHog** - Servidor SMTP simulado para testing
- **Serilog** - Logging estructurado
- **FluentValidation** - Validaciones de DTOs
- **Mapster** - Mapeo de objetos
- **Docker** - Containerización
- **Swagger/OpenAPI** - Documentación de API

### Testing
- **xUnit** - Framework de pruebas
- **Moq** - Mocking framework
- **FluentAssertions** - Assertions fluidas

## 🎯 Características Principales

### Funcionalidades del Negocio
- ✅ **Gestión de Productos**: CRUD completo de productos con control de stock
- ✅ **Gestión de Pedidos**: Crear, consultar y cancelar pedidos
- ✅ **Validación de Stock**: Verificación automática antes de crear pedidos
- ✅ **Notificaciones Email**: Confirmación automática vía email
- ✅ **Eventos de Dominio**: OrderCreated y OrderCancelled
- ✅ **Soft Delete**: Eliminación lógica en entidades principales

### Patrones Implementados
- ✅ **Clean Architecture**: Separación estricta por capas
- ✅ **Repository Pattern**: Abstracción de acceso a datos
- ✅ **Unit of Work**: Gestión de transacciones
- ✅ **Factory Pattern**: Creación de entidades Order
- ✅ **CQRS**: Separación de comandos y consultas
- ✅ **Domain Events**: Eventos de dominio con handlers

### Aspectos Técnicos
- ✅ **Middleware Global**: Manejo centralizado de excepciones
- ✅ **Health Checks**: Endpoints de salud para PostgreSQL y RabbitMQ
- ✅ **Swagger**: Documentación interactiva de API
- ✅ **CORS**: Configurado para desarrollo
- ✅ **Logging**: Serilog con salida a consola
- ✅ **Docker**: Containerización completa

## 🛠️ Configuración Rápida

### Prerrequisitos
- [Docker](https://www.docker.com/get-started/) y Docker Compose
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) (solo para desarrollo)

### 1. Ejecutar con Docker (Recomendado)

```bash
# Clonar repositorio
git clone <repository-url>
cd SGCAN-F-26-2025

# Levantar toda la infraestructura
docker-compose up -d

# Ver logs en tiempo real
docker-compose logs -f api consumer
```

La aplicación estará disponible en:
- **API**: http://localhost:5000
- **Swagger**: http://localhost:5000/swagger
- **RabbitMQ Management**: http://localhost:15672 (guest/guest)
- **MailHog Web UI**: http://localhost:8025

### 2. Desarrollo Local

```bash
# Restaurar dependencias
dotnet restore

# Levantar solo servicios externos
docker-compose up -d postgres rabbitmq mailhog

# Ejecutar migraciones
cd src/OrderManagement.Api
dotnet ef database update

# Ejecutar API
dotnet run --project src/OrderManagement.Api

# Ejecutar Consumer (en otra terminal)
dotnet run --project src/OrderManagement.Consumer
```

## 📡 API Endpoints

### Productos
```http
GET    /api/products           # Listar productos
GET    /api/products/{id}      # Obtener producto
POST   /api/products           # Crear producto
PUT    /api/products/{id}      # Actualizar producto
DELETE /api/products/{id}      # Eliminar producto (soft delete)
```

### Pedidos
```http
GET    /api/orders             # Listar pedidos
GET    /api/orders/{id}        # Obtener pedido
POST   /api/orders             # Crear pedido
PUT    /api/orders/{id}/cancel # Cancelar pedido
```

### Salud del Sistema
```http
GET    /health                 # Health check general
GET    /health/ready           # Readiness check
```

## 📝 Ejemplos de Uso

### Crear Producto
```json
POST /api/products
{
  "name": "Laptop Gaming",
  "description": "Laptop para gaming de alta gama",
  "price": 1299.99,
  "stock": 10
}
```

### Crear Pedido
```json
POST /api/orders
{
  "customerEmail": "cliente@email.com",
  "items": [
    {
      "productId": "550e8400-e29b-41d4-a716-446655440000",
      "quantity": 2
    }
  ]
}
```

### Respuesta Típica
```json
{
  "id": "6ba7b810-9dad-11d1-80b4-00c04fd430c8",
  "customerEmail": "cliente@email.com",
  "orderDate": "2024-01-15T10:30:00Z",
  "status": "Pending",
  "totalAmount": 2599.98,
  "items": [
    {
      "productId": "550e8400-e29b-41d4-a716-446655440000",
      "productName": "Laptop Gaming",
      "quantity": 2,
      "unitPrice": 1299.99,
      "totalPrice": 2599.98
    }
  ]
}
```

## 🧪 Pruebas

```bash
# Ejecutar todas las pruebas
dotnet test

# Ejecutar con coverage
dotnet test --collect:"XPlat Code Coverage"

# Ejecutar pruebas específicas
dotnet test --filter "Category=Unit"
```

### Cobertura de Pruebas
- ✅ **OrderService**: Crear pedido, validar stock, cancelar pedido
- ✅ **OrderRepository**: Persistencia y consultas
- ✅ **ProductService**: CRUD de productos
- ✅ **CreateOrderDtoValidator**: Validaciones de entrada
- ✅ **OrderFactory**: Creación de entidades con lógica de negocio

## 🐳 Docker

### Servicios Incluidos
- **API** (puerto 5000): Aplicación principal
- **Consumer** (background): Procesador de eventos
- **PostgreSQL** (puerto 5432): Base de datos principal
- **RabbitMQ** (puertos 5672, 15672): Message broker
- **MailHog** (puertos 1025, 8025): Servidor SMTP simulado

### Comandos Útiles
```bash
# Reconstruir imágenes
docker-compose build

# Ver logs específicos
docker-compose logs api
docker-compose logs consumer

# Reiniciar servicio específico
docker-compose restart api

# Limpiar volúmenes
docker-compose down -v
```

## 🔧 Configuración

### Variables de Entorno (docker-compose.yml)
```yaml
environment:
  - ASPNETCORE_ENVIRONMENT=Production
  - ConnectionStrings__DefaultConnection=Host=db;Port=5432;Database=pedidos_bd;Username=josh;Password=;Include Error Detail=true
  - RabbitMQ__Host=rabbit
  - Email__SmtpHost=mailhog
  - Email__SmtpPort=1025
```

### appsettings.json (para desarrollo local)
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=pedidos_bd;Username=josh;Password=;Include Error Detail=true"
  },
  "RabbitMQ": {
    "Host": "localhost",
    "Port": 5672,
    "Username": "guest",
    "Password": "guest",
    "QueueName": "order-created-queue"
  },
  "Email": {
    "SmtpHost": "localhost",
    "SmtpPort": 1025,
    "FromEmail": "no-reply@orders.local",
    "FromName": "Order Management System"
  }
}
```

## 📚 Estructura del Proyecto

### Domain Layer
```
/Entities       # Order, OrderItem, Product, BaseEntity
/Enums          # OrderStatus
/Factories      # OrderFactory con validaciones de stock
/Events         # OrderCreatedDomainEvent, OrderCancelledDomainEvent
```

### Application Layer
```
/DTOs           # CreateOrderDto, OrderResponseDto, ProductDto
/Services       # OrderService, ProductService
/Validators     # CreateOrderDtoValidator con FluentValidation
/Interfaces     # IOrderService, IProductService, IOrderRepository
```

### Infrastructure Layer
```
/Data           # AppDbContext, Configurations, Migrations
/Repositories   # OrderRepository, ProductRepository, UnitOfWork
/Services       # SmtpEmailSender, RabbitMQPublisher
/Extensions     # DependencyInjection
```

### API Layer
```
/Controllers    # OrdersController, ProductsController
/Middleware     # ExceptionHandlingMiddleware
/Extensions     # DependencyInjection, ConfigureServices
```

## 🔄 Flujo de Trabajo

1. **Cliente crea pedido** → API valida DTO y stock disponible
2. **OrderFactory crea entidad** → Se valida lógica de negocio
3. **Repository persiste** → Unit of Work confirma transacción
4. **Evento es publicado** → RabbitMQ recibe OrderCreatedEvent
5. **Consumer procesa evento** → Envía email de confirmación
6. **MailHog simula envío** → Email visible en interfaz web

## 🎯 Próximos Pasos

- [ ] Implementar paginación en listados
- [ ] Agregar autenticación JWT
- [ ] Implementar caché con Redis
- [ ] Agregar métricas con Prometheus
- [ ] Implementar CI/CD pipeline
- [ ] Agregar pruebas de integración

## 🤝 Contribución

1. Fork el proyecto
2. Crear feature branch (`git checkout -b feature/nueva-caracteristica`)
3. Commit cambios (`git commit -am 'Agregar nueva característica'`)
4. Push al branch (`git push origin feature/nueva-caracteristica`)
5. Crear Pull Request

## 📄 Licencia

Este proyecto está bajo la Licencia MIT - ver el archivo [LICENSE](LICENSE) para detalles.

---

**Desarrollado con ❤️ usando .NET 8 y Clean Architecture**

## Descripción General
Desarrollar una API REST para gestión de pedidos implementando arquitectura limpia y mejores prácticas de .NET Core.

## Requisitos Técnicos

### 1. Arquitectura (Clean architecture)

![CleanArchitecture.jpg](CleanArchitecture.jpg)

### 2. Funcionalidades (Core Features)

#### Endpoints Mínimos:

**Pedidos**
- `POST /api/orders` - Crear pedido
- `GET /api/orders/{id}` - Obtener pedido
- `GET /api/orders` - Listar pedidos (simple)
- `PUT /api/orders/{id}/cancel` - Cancelar pedido

**Productos**
- `GET /api/products` - Listar productos

### 3. Base de Datos

**SQL Server con EF Core**

**Modelos simplificados:**

```csharp
Order:
- Id (Guid)
- OrderNumber (string)
- CustomerName (string)
- CustomerEmail (string)
- OrderDate (DateTime)
- Status (enum: Pending, Completed, Cancelled)
- TotalAmount (decimal)
- OrderItems (List<OrderItem>)

OrderItem:
- Id (Guid)
- OrderId (Guid)
- ProductId (Guid)
- Quantity (int)
- UnitPrice (decimal)

Product:
- Id (Guid)
- Name (string)
- Price (decimal)
- Stock (int)
```

### 4. Colas de Mensajería + Email

**RabbitMQ + MailHog:**
- Publicar evento `OrderCreated` al crear un pedido
- Consumidor escucha el evento y envía email de confirmación vía MailHog
- MailHog captura los emails para testing (no envía emails reales)

**Flujo:**
```
API → RabbitMQ Queue → Consumer → MailHog SMTP → Email capturado
```

**Implementación requerida:**
- Publisher: Publicar `OrderCreatedEvent` en cola "order-created-queue"
- Consumer: Background service que escucha la cola
- Email Service: Enviar email usando SMTP de MailHog (puerto 1025)
- Template HTML básico para el email de confirmación

### 5. Patrones de Diseño (3 obligatorios)

1. **Repository Pattern**: Para acceso a datos
2. **Unit of Work**: Para transacciones
3. **Factory Pattern**: Para crear objetos Order

### 6. Manejo de Excepciones Globales

**Middleware de excepciones:**
```csharp
- NotFoundException (404)
- ValidationException (400)
- Exception genérica (500)
```

**Logging con Serilog** (consola únicamente)

### 7. Validaciones

**FluentValidation en CreateOrderDto:**
- CustomerName requerido
- Email válido
- Al menos 1 item
- Stock disponible

### 8. Pruebas Unitarias

**Mínimo 5 tests en:**
- OrderService: Crear pedido exitoso
- OrderService: Crear pedido sin stock (excepción)
- OrderService: Cancelar pedido
- OrderRepository: Guardar y obtener
- Validator: Validar DTO

**Usar:** xUnit, Moq, FluentAssertions

### 9. Dockerización

**docker-compose.yml con:**
- API .NET
- SQL Server
- RabbitMQ (con Management UI)
- **MailHog** (SMTP server + Web UI)

**Dockerfile** para la API

**Puertos expuestos:**
- API: 5000
- SQL Server: 1433
- RabbitMQ AMQP: 5672
- RabbitMQ Management: 15672
- MailHog SMTP: 1025
- MailHog Web UI: 8025

### 10. Configuración Esencial

**appsettings.json debe incluir:**
- ConnectionStrings (SQL Server)
- RabbitMQ (Host, Port, Username, Password)
- Email (SmtpHost, SmtpPort, FromEmail, FromName)
- Serilog configuration
- Utilizar un Vault (opcional)

**Otros:**
- Inyección de dependencias completa
- Swagger con documentación
- Health check: `/health`
- CORS habilitado

## Entregables

1. **Código fuente** con:
   - README.md con instrucciones detalladas
   - Commits descriptivos y organizados
   - .gitignore apropiado

2. **Archivos Docker**:
   - Dockerfile
   - docker-compose.yml

3. **Documentación**:
   - Instrucciones de ejecución con Docker
   - Collection de Postman con ejemplos de requests

4. **Scripts**:
   - Migraciones de Entity Framework
   - Seed data para productos


## Bonus (Opcional - Solo si sobra tiempo)

- ✅ Paginación en GET /api/orders
- ✅ Filtro por status en listado
- ✅ Soft delete en entidades
- ✅ Template HTML profesional para emails
- ✅ Retry policy en consumer de RabbitMQ
- ✅ Email de cancelación de pedido
- ✅ Variables de entorno en docker-compose
- ✅ Health checks avanzados (BD, RabbitMQ, MailHog)
