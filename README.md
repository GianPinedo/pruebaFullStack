# Prueba Técnica .NET Core - Sistema de Gestión de Pedidos

## Descripción General
Desarrollar una API REST para gestión de pedidos implementando arquitectura limpia y mejores prácticas de .NET Core.

## Requisitos Técnicos

### 1. Arquitectura
Se evaluará la arquitectura implementada

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
