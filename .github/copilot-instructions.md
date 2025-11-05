# GitHub Copilot Instructions - Sistema de Gestión de Pedidos

## Arquitectura del Proyecto

Este es un sistema de gestión de pedidos implementado con **.NET 8** y **Clean Architecture**. La solución debe seguir estrictamente esta estructura:

```
/src
  /OrderManagement.Api           # ASP.NET Core Web API, controllers
  /OrderManagement.Application   # Casos de uso, DTOs, validaciones, interfaces
  /OrderManagement.Domain        # Entidades, enums, value objects, eventos
  /OrderManagement.Infrastructure # EF Core, repos, UoW, email, RabbitMQ, Serilog
  /OrderManagement.Consumer      # Worker Service para RabbitMQ consumer
/tests
  /OrderManagement.Tests         # xUnit + Moq + FluentAssertions
/docker
  Dockerfile.api
  docker-compose.yml
```

## Tecnologías y Patrones Específicos

### Base de Datos: PostgreSQL (Npgsql)
- Usar **Entity Framework Core** con Npgsql provider
- **GUID** como identificadores primarios para todas las entidades
- Aplicar **Soft Delete** en entidades principales

### Patrones Obligatorios
1. **Repository Pattern**: Interfaces en Application, implementaciones en Infrastructure
2. **Unit of Work**: Para gestión de transacciones
3. **Factory Pattern**: Para creación de objetos Order con lógica compleja

### Mensajería y Email
- **RabbitMQ**: Cola "order-created-queue" para eventos OrderCreated
- **MailHog**: SMTP simulado en puerto 1025 para testing de emails
- **Consumer Worker Service**: Separado con retry policy para procesar eventos

## Convenciones de Código Específicas

### Entidades del Dominio
```csharp
// Todas las entidades heredan de BaseEntity con Guid Id
public abstract class BaseEntity
{
    public Guid Id { get; protected set; }
    public DateTime CreatedAt { get; protected set; }
    public DateTime? UpdatedAt { get; protected set; }
    public bool IsDeleted { get; protected set; } // Soft Delete
}

// Enum de Status específico del proyecto
public enum OrderStatus { Pending, Completed, Cancelled }
```

### DTOs y Validaciones
- **FluentValidation** obligatorio para todos los DTOs de entrada
- Validar stock disponible en CreateOrderDto
- Email válido y al menos 1 item requeridos

### Configuración Docker
```yaml
# Puertos específicos del proyecto:
# API: 5000, PostgreSQL: 5432, RabbitMQ: 5672/15672, MailHog: 1025/8025
```

## Flujos de Trabajo Críticos

### Creación de Pedidos
1. Validar DTO con FluentValidation
2. Verificar stock disponible
3. Usar Factory para crear Order
4. Persistir con Repository + UoW
5. Publicar OrderCreatedEvent a RabbitMQ
6. Consumer envía email de confirmación via MailHog

### Manejo de Excepciones
- **Middleware global** para: NotFoundException (404), ValidationException (400), Exception genérica (500)
- **Serilog** solo a consola para logging

## Endpoints Requeridos
```
POST /api/orders         # Crear pedido
GET /api/orders/{id}     # Obtener pedido
GET /api/orders          # Listar (con paginación bonus)
PUT /api/orders/{id}/cancel # Cancelar pedido
GET /api/products        # Listar productos
GET /health              # Health check
```

## Configuración Essential (appsettings.json)
```json
{
  "ConnectionStrings": { "DefaultConnection": "PostgreSQL..." },
  "RabbitMQ": { "Host": "localhost", "Port": 5672, "Username": "guest", "Password": "guest" },
  "Email": { "SmtpHost": "localhost", "SmtpPort": 1025, "FromEmail": "orders@company.com" },
  "Serilog": { "MinimumLevel": "Information" }
}
```

## Tests Mínimos Requeridos
- OrderService: Crear pedido exitoso/sin stock/cancelar
- OrderRepository: Guardar y obtener
- Validator: Validar CreateOrderDto
- Usar **xUnit + Moq + FluentAssertions**

Cuando generes código, siempre respeta esta estructura y patrones específicos del proyecto.