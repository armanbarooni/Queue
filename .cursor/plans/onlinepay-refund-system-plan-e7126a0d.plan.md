<!-- e7126a0d-1b7f-44c5-adde-f818ed348ce3 a937479e-6657-445b-b7a2-1e15dc9bde1c -->
# Hangfire Platform - Modular Task Scheduling System

## Project Context

Building a **centralized Hangfire-based scheduling platform** in **.NET 8** for automated background tasks. Initially focused on Refund Processing, but designed to be scalable and extensible for future tasks (Notification, Settlement, Reporting, etc.).

## Core Principle

**Hangfire acts as scheduler/trigger only** - All business logic resides in database stored procedures (SPs). Jobs fetch data, call external services (ESB), then execute SPs for data updates. This ensures clean Separation of Concerns.

## Project Structure

Create a new solution with modular, task-based architecture:

```
HangfirePlatform/
├── Api/                     // Entry point + Hangfire Dashboard + Job registration
├── Core/                    // Shared abstractions: interfaces, base job classes, contracts
├── Tasks/
│   ├── RefundTask/          // Refund inquiry module (calls ESB, executes SPs)
│   └── (Future: NotificationTask, SettlementTask, CleanupTask)
├── Shared/                  // Common utilities: logging, config helpers, extensions
└── Infrastructure/          // Hangfire Storage config, DI setup, connection strings
```

**Solution Projects:**

1. **HangfirePlatform.Api** - Minimal API, Hangfire Dashboard, Job registration
2. **HangfirePlatform.Core** - Task interfaces, base classes, shared contracts
3. **HangfirePlatform.Tasks.Refund** - Refund task implementation
4. **HangfirePlatform.Shared** - Common utilities and helpers
5. **HangfirePlatform.Infrastructure** - Hangfire configuration, database executors

## Implementation Steps

### Phase 1: Project Setup and Domain Layer

1. **Create Solution and Projects**

- Create solution file: `OnlinePay.RefundSystem.sln`
- Create 4 class library projects (.NET 8)
- Create 1 web project (Minimal API, .NET 8)
- Configure project references: Api → Application → Domain, Api → Infrastructure → Application → Domain

2. **Domain Layer (`OnlinePay.RefundSystem.Domain`)**

- Create `Entities/RefundTransaction.cs` - Map to existing database table
- Create `Entities/BankSettings.cs` - Map to existing BankSettings table
- Create `Enums/RefundStatus.cs` - Map to existing States enum values
- Create `Enums/BankType.cs` - Enum for bank types (Mellat, Saman, Behpardakht)
- Create `Enums/FinalRefundStatus.cs` - Enum for final status (Pending, Success, Failed, NotFound, Error)
- Create `Interfaces/IRefundRepository.cs` - Repository interface
- Create `Interfaces/IBankSettingsRepository.cs` - For querying bank settings

### Phase 2: Infrastructure Layer - Database

3. **Database Context (`OnlinePay.RefundSystem.Infrastructure/Persistence`)**

- Create `RefundDbContext.cs` - EF Core DbContext mapping to existing tables
- Map `RefundTransaction` entity to existing `RefundTransaction` table
- Map `BankSettings` entity to existing `BankSettings` table
- Configure entity mappings (table names, column names, indexes)
- Handle existing schema (no migrations for production)

4. **Repository Implementation**

- Create `RefundRepository.cs` - Implement IRefundRepository
- Query pending refunds: `FinalRefundStatus IS NULL OR FinalRefundStatus = 'Pending' OR State = REFUND_IN_PROGRESS`
- Include BankSettings navigation property for bank type
- Create `BankSettingsRepository.cs` - For querying bank configuration

### Phase 3: Infrastructure Layer - ESB Integration

5. **ESB Client (`OnlinePay.RefundSystem.Infrastructure/ESB`)**

- Create `IEsbClient.cs` - Interface for bank refund queries
- Create `EsbClient.cs` - HTTP client implementation
- Implement `QueryRefundMellatAsync()` - Query Mellat BPM API using existing endpoint pattern
- Implement `QueryRefundSamanAsync()` - Placeholder for Saman integration
- Implement `QueryRefundBehpardakhtAsync()` - Placeholder for Behpardakht integration
- Create response DTOs: `MellatRefundResponse.cs`, `EsbRefundQueryResult.cs`
- Handle Basic Authentication for Mellat API
- Parse response codes (000 = Success, 700 = Username Invalid, 500 = Internal Error)

6. **ESB Request Models**

- Create `MellatRefundRequest.cs` - Request model for Mellat API
- Support querying by date and by reference ID
- Map response to unified DTO format

### Phase 4: Application Layer

7. **Application Services (`OnlinePay.RefundSystem.Application/Services`)**

- Create `IRefundProcessor.cs` - Interface for processing refunds
- Create `RefundProcessor.cs` - Orchestrates refund status query
- Route to appropriate ESB client based on bank type
- Map ESB response to FinalRefundStatus enum
- Update refund entity with status and details

8. **Application DTOs (`OnlinePay.RefundSystem.Application/DTOs`)**

- Create `RefundResultDto.cs` - Result DTO with status, message, response code, details
- Create `EsbRefundQueryResult.cs` - ESB query result wrapper

9. **Hangfire Job (`OnlinePay.RefundSystem.Application/Jobs`)**

- Create `RefundStateSyncJob.cs` - Main Hangfire job class
- Query all pending refunds from repository
- Process each refund using RefundProcessor
- Update database with final status and details (JSON)
- Handle errors and logging
- Batch updates for performance

### Phase 5: Infrastructure Layer - Hangfire

10. **Hangfire Configuration (`OnlinePay.RefundSystem.Infrastructure/Hangfire`)**

 - Create `HangfireSetup.cs` - Extension methods for Hangfire configuration
 - Configure SQL Server storage using existing connection string
 - Configure Hangfire server options (worker count, queues)
 - Create `HangfireAuthorizationFilter.cs` - Dashboard authorization (basic for now)
 - Configure recurring job registration
 - Set cron expression from configuration (default: every 30 minutes)

### Phase 6: Configuration and Options

11. **Configuration (`OnlinePay.RefundSystem.Infrastructure/Configuration`)**

 - Create `RefundSystemOptions.cs` - Strongly-typed configuration class
 - Create nested classes: `DatabaseOptions`, `EsbOptions`, `MellatOptions`, `SamanOptions`, `BehpardakhtOptions`, `HangfireOptions`
 - Support `IOptionsSnapshot<T>` for hot configuration reload
 - Map from appsettings.json structure

12. **AppSettings (`OnlinePay.RefundSystem.Api/appsettings.json`)**

 - Configure connection string (read from existing OnlinePay config pattern)
 - Configure ESB endpoints for Mellat, Saman, Behpardakht
 - Configure Hangfire settings (worker count, cron expression, queue name)
 - Add environment-specific appsettings (Development, Production)

### Phase 7: API Layer

13. **Minimal API (`OnlinePay.RefundSystem.Api/Program.cs`)**

 - Configure services (DbContext, Repositories, ESB Client, Services, Jobs)
 - Configure Hangfire (server, dashboard, recurring jobs)
 - Configure health checks (database, Hangfire)
 - Register HTTP client for ESB with proper timeout
 - Configure logging (Serilog or built-in)
 - Set up dependency injection scopes for Hangfire jobs

14. **API Endpoints (`OnlinePay.RefundSystem.Api/Endpoints`)**

 - Create `RefundEndpoints.cs` - Endpoint mappings
 - `GET /` - Health check endpoint
 - `GET /health` - Health check with database and Hangfire status
 - `POST /api/refund/sync` - Manual trigger for refund sync job
 - `GET /api/refund/pending` - Query pending refunds (admin endpoint)
 - `GET /hangfire` - Hangfire dashboard (protected)

15. **Swagger Configuration**

 - Add Swagger/OpenAPI support for API documentation
 - Configure Swagger UI for development environment

### Phase 8: Database Integration

16. **Entity Framework Configuration**

 - Configure DbContext to use existing database schema
 - Map RefundTransaction entity to existing table structure
 - Handle case sensitivity and column naming conventions
 - Configure relationships (RefundTransaction → BankSettings)
 - Set up proper indexes for performance

17. **Database Access Pattern**

 - Use scoped DbContext for repository operations
 - Ensure Hangfire jobs create their own scope for database access
 - Handle connection string from configuration
 - Support connection string changes via IOptionsSnapshot

### Phase 9: Error Handling and Logging

18. **Logging Configuration**

 - Configure structured logging (Microsoft.Extensions.Logging)
 - Log ESB API requests/responses
 - Log job execution metrics (success count, failed count, duration)
 - Log errors with full context (refund ID, bank type, error details)

19. **Error Handling**

 - Handle ESB API failures gracefully
 - Retry logic via Hangfire retry mechanism
 - Store error details in refund.Details JSON field
 - Continue processing other refunds on individual failures

### Phase 10: Deployment Configuration

20. **Project Files and Dependencies**

 - Configure `.csproj` files with proper package references
 - Add EF Core SQL Server provider
 - Add Hangfire packages (Core, AspNetCore, SqlServer)
 - Add HTTP client extensions
 - Add health check packages

21. **Configuration Management**

 - Support environment-specific configuration
 - Use IOptionsSnapshot for hot reload
 - Document required configuration values
 - Add configuration validation

## Key Implementation Details

### Database Schema Mapping

- Map to existing `RefundTransaction` table (do not create new tables)
- Use existing column names: `id`, `refundTransactionId`, `transactionId`, `bankSettingsId`, `amount`, `state`, `final_refund_status`, `details`
- Join with `BankSettings` table to get `type` field for bank identification

### ESB Integration Pattern

- Mellat: Use `/bhrws/transactionInfo/getRefundTransactionByDateCompleteFromArchive` endpoint
- Query by date (current day) and filter by refund ID in response
- Use Basic Authentication with username/password
- Parse JSON response and map status codes

### Hangfire Job Execution

- Recurring job runs every 30 minutes (configurable)
- Job queries pending refunds in batches
- Processes each refund asynchronously
- Updates database in transaction
- Logs execution metrics

### Configuration Hot Reload

- Use `IOptionsSnapshot<RefundSystemOptions>` for configuration access
- Configuration changes in appsettings.json are picked up without restart
- ESB credentials can be updated without downtime

## Files to Create

### Domain Layer (5 files)

- `Domain/Entities/RefundTransaction.cs`
- `Domain/Entities/BankSettings.cs`
- `Domain/Enums/RefundStatus.cs`
- `Domain/Enums/BankType.cs`
- `Domain/Interfaces/IRefundRepository.cs`
- `Domain/Interfaces/IBankSettingsRepository.cs`

### Application Layer (5 files)

- `Application/DTOs/RefundResultDto.cs`
- `Application/Services/IRefundProcessor.cs`
- `Application/Services/RefundProcessor.cs`
- `Application/Jobs/RefundStateSyncJob.cs`

### Infrastructure Layer (12 files)

- `Infrastructure/Persistence/RefundDbContext.cs`
- `Infrastructure/Persistence/RefundRepository.cs`
- `Infrastructure/Persistence/BankSettingsRepository.cs`
- `Infrastructure/ESB/IEsbClient.cs`
- `Infrastructure/ESB/EsbClient.cs`
- `Infrastructure/ESB/Models/MellatRefundRequest.cs`
- `Infrastructure/ESB/Models/MellatRefundResponse.cs`
- `Infrastructure/ESB/Models/EsbRefundQueryResult.cs`
- `Infrastructure/Hangfire/HangfireSetup.cs`
- `Infrastructure/Hangfire/HangfireAuthorizationFilter.cs`
- `Infrastructure/Configuration/RefundSystemOptions.cs`

### API Layer (4 files)

- `Api/Program.cs`
- `Api/Endpoints/RefundEndpoints.cs`
- `Api/appsettings.json`
- `Api/appsettings.Development.json`

### Project Files (5 files)

- `OnlinePay.RefundSystem.sln`
- `Domain/OnlinePay.RefundSystem.Domain.csproj`
- `Application/OnlinePay.RefundSystem.Application.csproj`
- `Infrastructure/OnlinePay.RefundSystem.Infrastructure.csproj`
- `Api/OnlinePay.RefundSystem.Api.csproj`

**Total: ~31 files to create**

## Dependencies

### NuGet Packages

- Microsoft.EntityFrameworkCore.SqlServer (8.0.0)
- Microsoft.EntityFrameworkCore.Design (8.0.0)
- Hangfire.Core (1.8.6)
- Hangfire.AspNetCore (1.8.6)
- Hangfire.SqlServer (1.8.6)
- Swashbuckle.AspNetCore (6.5.0)
- AspNetCore.HealthChecks.SqlServer (8.0.0)
- AspNetCore.HealthChecks.Hangfire (8.0.0)

## Notes

- **No Database Migrations**: System connects to existing database, no schema changes
- **No Tests**: As per requirements, test projects are not included
- **Production Safety**: Configuration supports environment-specific settings
- **Hot Reload**: Configuration changes apply without restart using IOptionsSnapshot
- **Scalability**: Hangfire supports multiple workers and queue management
- **Monitoring**: Health checks and Hangfire dashboard provide operational visibility