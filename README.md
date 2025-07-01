# IdempotentApiDemo

A comprehensive ASP.NET Core application demonstrating idempotency implementation for API operations. Idempotent operations ensure that side effects occur only once, even when the same request is sent multiple times.

## Features

- Custom attribute (`[Idempotent]`) for implementing idempotent behavior in API requests
- Two distinct idempotent behavior strategies:
  - `ReturnFromCache`: Returns cached response for duplicate requests
  - `ThrowErrorIfExists`: Throws an error for duplicate requests
- Redis-based caching for request storage and locking mechanism
- Built-in distributed locking to handle concurrent requests safely
- Swagger/OpenAPI integration with automatic Idempotency-Key header documentation

## Architecture

### Controllers

- **OrdersController**: API endpoints for order operations
  - `POST /api/Orders`: Standard idempotent order creation (returns cached response)
  - `POST /api/Orders/strict`: Strict idempotent order creation (throws error on duplicates)
  - `GET /api/Orders/{id}`: Query order information by ID

### Idempotency Mechanism

The `[Idempotent]` attribute processes incoming requests by:

1. **Key Validation**: Validates the `Idempotency-Key` header (must be a valid GUID)
2. **Distributed Locking**: Acquires a Redis lock to prevent race conditions
3. **Cache Lookup**: Checks if a request with the same key was previously processed
4. **Behavior Strategy**: Applies the configured behavior strategy:
  - **ReturnFromCache**: Returns the previously cached response
  - **ThrowErrorIfExists**: Returns a 409 Conflict error
5. **Response Caching**: Caches successful responses (2xx status codes) for future requests
6. **TTL Management**: Automatically expires cached entries after the specified duration

### Key Components

- **IdempotentAttribute**: Core filter implementing the idempotency logic
- **IdempotentBehavior**: Enum defining response strategies for duplicate requests
- **IdempotentResponse**: Model for caching API responses with status codes
- **RedisSettings**: Configuration model for Redis connection parameters

## Configuration

### Redis Settings

Configure Redis connection in `appsettings.json`:

```json
{
  "RedisSettings": {
    "Host": "localhost",
    "Port": 6379,
    "Password": "your-redis-password",
    "InstanceName": "IdempotentApiDemo-"
  }
}
```

### Attribute Parameters

```csharp
[Idempotent(
    behavior: IdempotentBehavior.ReturnFromCache,  // Behavior strategy
    ttlMinutes: 60                                 // Cache TTL in minutes
)]
```

## Usage

### Making Idempotent Requests

Clients must include an `Idempotency-Key` header with a unique GUID:

```http
POST /api/Orders
Content-Type: application/json
Idempotency-Key: 550e8400-e29b-41d4-a716-446655440000

{
  "productId": 123
}
```

### Behavior Examples

#### Standard Endpoint (`/api/Orders`)
- **First Request**: Processes order and returns `201 Created`
- **Duplicate Request**: Returns cached `201 Created` response with same order data

#### Strict Endpoint (`/api/Orders/strict`)
- **First Request**: Processes order and returns `201 Created`
- **Duplicate Request**: Returns `409 Conflict` error

## Error Handling

The API handles various error scenarios:

- **Missing Idempotency-Key**: Returns `400 Bad Request`
- **Invalid Idempotency-Key**: Returns `400 Bad Request` (must be valid GUID)
- **Concurrent Processing**: Returns `409 Conflict` when another request with same key is being processed
- **Duplicate Request (Strict Mode)**: Returns `409 Conflict` for previously processed requests

## Use Cases

This implementation is ideal for:

- **Payment Processing**: Prevent duplicate charges from network retries
- **Order Management**: Avoid duplicate orders from double-clicks or form resubmissions
- **Financial Transactions**: Ensure transaction integrity in distributed systems
- **API Gateway Integration**: Handle client-side retry mechanisms safely
- **Mobile Applications**: Manage unreliable network conditions gracefully

## Technical Details

### Distributed Locking

The implementation uses Redis for distributed locking with:
- Lock expiration (5 seconds) to prevent deadlocks
- Automatic lock cleanup in finally blocks
- Conflict detection for concurrent requests

### Caching Strategy

- Only successful responses (2xx status codes) are cached
- Configurable TTL with automatic expiration
- JSON serialization for complex response objects
- Efficient key-based lookup using Redis

### Thread Safety

The solution handles concurrent scenarios through:
- Redis atomic operations for locking
- Proper exception handling and resource cleanup
- Race condition prevention between cache check and write operations

## Development

### Prerequisites

- .NET 9.0 SDK
- Redis server

### Running the Application

1. Start Redis server
2. Update Redis configuration in `appsettings.json`
3. Run the application:
   ```bash
   dotnet run --project src/IdempotentApiDemo.API
   ```
4. Access Swagger UI at `https://localhost:5001/swagger`

## API Documentation

The application includes comprehensive Swagger/OpenAPI documentation with:
- Automatic `Idempotency-Key` header documentation for POST endpoints
- Request/response examples
- Error response documentation
- Interactive API testing interface

This project demonstrates enterprise-grade idempotency implementation patterns essential for building reliable, distributed API systems.