# Logger Concept in .NET

A comprehensive guide to logging in .NET applications - from basics to advanced implementations.

## 📋 Table of Contents
- [What is Logging?](#what-is-logging)
- [Why Logging Matters](#why-logging-matters)
- [Built-in Logging in .NET](#built-in-logging-in-net)
- [Log Levels](#log-levels)
- [Getting Started](#getting-started)
- [Popular Logging Providers](#popular-logging-providers)
- [Best Practices](#best-practices)
- [Code Examples](#code-examples)
- [Advanced Concepts](#advanced-concepts)
- [Troubleshooting](#troubleshooting)

## What is Logging?

Logging is the process of recording application events, errors, and informational messages during execution. It's like a **black box** for your application that helps you understand what's happening under the hood.

```csharp
// Simple log example
logger.LogInformation("User {UserId} logged in at {Time}", userId, DateTime.Now);
```

## Why Logging Matters

✅ **Debugging** - Identify and fix issues faster  
✅ **Monitoring** - Track application health and performance  
✅ **Auditing** - Keep records of important operations  
✅ **Alerting** - Get notified when things go wrong  
✅ **Analytics** - Understand user behavior and patterns  

## Built-in Logging in .NET

.NET provides a built-in, extensible logging system through the `Microsoft.Extensions.Logging` namespace.

### Key Interfaces

| Interface | Purpose |
|-----------|---------|
| `ILogger<T>` | Primary interface for logging (generic version) |
| `ILogger` | Non-generic logging interface |
| `ILoggerFactory` | Creates logger instances |
| `ILoggerProvider` | Creates logging providers (console, file, etc.) |

## Log Levels

.NET defines **6 log levels** in order of severity:

| Level | Value | Description | Example |
|-------|-------|-------------|---------|
| **Trace** | 0 | Most detailed logs | "Entering method X with parameters Y" |
| **Debug** | 1 | Debugging information | "Database query executed in 150ms" |
| **Information** | 2 | General application flow | "User logged in successfully" |
| **Warning** | 3 | Unexpected but non-critical | "API response slow - took 2 seconds" |
| **Error** | 4 | Runtime errors/failures | "Failed to save record to database" |
| **Critical** | 5 | Application crashes | "Application failed to start" |

```csharp
// Examples of different log levels
logger.LogTrace("Entering method CalculateTotal with items: {Count}", items.Count);
logger.LogDebug("Cache hit ratio: {Ratio}%", cacheRatio);
logger.LogInformation("Order #{OrderId} processed successfully", orderId);
logger.LogWarning("Payment gateway response delayed");
logger.LogError(ex, "Failed to process payment for order {OrderId}", orderId);
logger.LogCritical("Database connection pool exhausted!");
```

## Getting Started

### 1. Basic Setup in Console Application

```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

// Setup service container
var services = new ServiceCollection();
services.AddLogging(builder =>
{
    builder.AddConsole();      // Log to console
    builder.AddDebug();        // Log to debug output
    builder.SetMinimumLevel(LogLevel.Information);
});

var serviceProvider = services.BuildServiceProvider();
var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

// Use the logger
logger.LogInformation("Application started");
```

### 2. Setup in ASP.NET Core

```csharp
var builder = WebApplication.CreateBuilder(args);

// Configure logging (usually already configured by default)
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
builder.Logging.AddEventLog();

var app = builder.Build();

// Inject logger in your endpoints or controllers
app.MapGet("/", (ILogger<Program> logger) =>
{
    logger.LogInformation("Home endpoint accessed at {Time}", DateTime.UtcNow);
    return "Hello World!";
});
```

## Popular Logging Providers

### Built-in Providers

| Provider | Package | Description |
|----------|---------|-------------|
| Console | `Microsoft.Extensions.Logging.Console` | Logs to console window |
| Debug | `Microsoft.Extensions.Logging.Debug` | Logs to debug output |
| EventLog | `Microsoft.Extensions.Logging.EventLog` | Windows Event Log |
| EventSource | `Microsoft.Extensions.Logging.EventSource` | ETW events |
| Azure App Services | Built-in | Logs to Azure portal |

### Third-Party Providers

| Provider | Package | Best For |
|----------|---------|----------|
| **Serilog** | `Serilog.Extensions.Logging` | Structured logging, multiple sinks |
| **NLog** | `NLog.Extensions.Logging` | Flexible configuration, legacy apps |
| **log4net** | `Microsoft.Extensions.Logging.Log4Net` | Enterprise, Apache-style logging |
| **Sentry** | `Sentry.AspNetCore` | Error tracking, alerts |
| **Application Insights** | `Microsoft.ApplicationInsights` | Azure monitoring, analytics |

## Code Examples

### Example 1: Basic Controller Logging

```csharp
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly ILogger<ProductsController> _logger;
    private readonly IProductService _productService;

    public ProductsController(
        ILogger<ProductsController> logger, 
        IProductService productService)
    {
        _logger = logger;
        _productService = productService;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Product>> GetProduct(int id)
    {
        _logger.LogInformation("Fetching product with ID: {ProductId}", id);

        try
        {
            var product = await _productService.GetByIdAsync(id);
            
            if (product == null)
            {
                _logger.LogWarning("Product not found: {ProductId}", id);
                return NotFound();
            }

            _logger.LogDebug("Product retrieved successfully: {ProductName}", product.Name);
            return Ok(product);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching product {ProductId}", id);
            return StatusCode(500, "An error occurred while processing your request");
        }
    }
}
```

### Example 2: Structured Logging with Serilog

```csharp
// Program.cs
using Serilog;

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File("logs/myapp-.txt", rollingInterval: RollingInterval.Day)
    .WriteTo.Seq("http://localhost:5341")  // Structured log server
    .Enrich.WithProperty("Application", "MyApp")
    .Enrich.WithMachineName()
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog();  // Replace default logging with Serilog

var app = builder.Build();

// Usage - logs become searchable objects
app.MapPost("/api/orders", (Order order, ILogger<Program> logger) =>
{
    logger.LogInformation("New order created: {@Order}", order);
    // @Order tells Serilog to serialize as structured object
});
```

### Example 3: Custom Logger Provider

```csharp
// Custom logger that sends logs to a database
public class DatabaseLoggerProvider : ILoggerProvider
{
    private readonly string _connectionString;

    public DatabaseLoggerProvider(string connectionString)
    {
        _connectionString = connectionString;
    }

    public ILogger CreateLogger(string categoryName)
    {
        return new DatabaseLogger(_connectionString, categoryName);
    }

    public void Dispose() { }
}

public class DatabaseLogger : ILogger
{
    private readonly string _connectionString;
    private readonly string _categoryName;

    public DatabaseLogger(string connectionString, string categoryName)
    {
        _connectionString = connectionString;
        _categoryName = categoryName;
    }

    public IDisposable BeginScope<TState>(TState state) => default;

    public bool IsEnabled(LogLevel logLevel) => logLevel >= LogLevel.Warning;

    public void Log<TState>(LogLevel logLevel, EventId eventId, 
        TState state, Exception exception, Func<TState, Exception, string> formatter)
    {
        if (!IsEnabled(logLevel)) return;

        var message = formatter(state, exception);
        
        // Save to database (simplified)
        using var connection = new SqlConnection(_connectionString);
        var sql = @"INSERT INTO Logs (Timestamp, Level, Category, Message, Exception) 
                   VALUES (@Timestamp, @Level, @Category, @Message, @Exception)";
        
        // Execute SQL...
    }
}

// Register the custom provider
builder.Logging.AddProvider(new DatabaseLoggerProvider(connectionString));
```

## Best Practices

### ✅ DO's

1. **Use structured logging**
   ```csharp
   // ❌ Bad
   logger.LogInformation($"User {userId} logged in");
   
   // ✅ Good
   logger.LogInformation("User {UserId} logged in", userId);
   ```

2. **Log at appropriate levels**
   ```csharp
   // Debug for development details
   logger.LogDebug("Cache keys: {Keys}", string.Join(", ", cacheKeys));
   
   // Error for exceptions
   logger.LogError(ex, "Failed to process payment");
   ```

3. **Include context with scopes**
   ```csharp
   using (logger.BeginScope("Processing order {OrderId}", orderId))
   {
       logger.LogInformation("Validating payment");
       logger.LogInformation("Updating inventory");
       // All logs in this block include the OrderId
   }
   ```

4. **Use async logging in production**
   ```csharp
   builder.Logging.AddConsole().AddFile(o => o.UseAsync = true);
   ```

### ❌ DON'Ts

1. **Don't log sensitive information**
   ```csharp
   // ❌ Never do this
   logger.LogInformation("Password: {Password}", password);
   logger.LogInformation("Credit card: {CardNumber}", creditCard);
   ```

2. **Don't log in tight loops**
   ```csharp
   // ❌ Bad - will flood logs
   for (int i = 0; i < 10000; i++)
   {
       logger.LogDebug("Processing item {i}", i);
   }
   
   // ✅ Good - log summary instead
   logger.LogInformation("Processed {Count} items", items.Count);
   ```

3. **Don't log exceptions without context**
   ```csharp
   // ❌ Bad
   logger.LogError(ex, "Error");
   
   // ✅ Good
   logger.LogError(ex, "Failed to send email to {EmailAddress}", email);
   ```

## Advanced Concepts

### Log Filtering

```csharp
// appsettings.json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "Microsoft.Hosting.Lifetime": "Information",
      "YourNamespace.Controllers": "Debug"
    }
  }
}
```

### Log Enrichment

Add custom properties to all logs:

```csharp
// Using Serilog enrichers
Log.Logger = new LoggerConfiguration()
    .Enrich.WithThreadId()
    .Enrich.WithEnvironmentName()
    .Enrich.WithProperty("Version", "1.2.3")
    .CreateLogger();
```

### Centralized Logging

For microservices or distributed systems, use centralized logging:

- **ELK Stack** (Elasticsearch, Logstash, Kibana)
- **Seq** - Structured log server
- **Splunk** - Enterprise log analytics
- **Azure Monitor / Application Insights**

## Troubleshooting

### Common Issues and Solutions

| Issue | Solution |
|-------|----------|
| No logs appearing | Check minimum log level |
| Performance degradation | Use async logging, reduce debug logs |
| Logs too verbose | Adjust filters in appsettings.json |
| Missing log entries | Check disk space, permissions |
| Structured logs not working | Use correct syntax `{@Property}` |

## Complete Working Example

Check out the [sample application](./samples) folder for a complete working example including:
- Console app with multiple providers
- ASP.NET Core API with structured logging
- Custom logger implementation
- Log aggregation with Seq

---

## 📚 Additional Resources

- [Microsoft Logging Documentation](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/logging/)
- [Serilog Documentation](https://serilog.net/)
- [NLog Documentation](https://nlog-project.org/)
- [.NET Logging Best Practices](https://docs.microsoft.com/en-us/dotnet/core/extensions/logging)

---

*This README was created to help developers understand and implement logging in .NET applications. Contributions and suggestions are welcome!*
