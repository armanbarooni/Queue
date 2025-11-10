## TandemQueue Service Scaffold

This solution bootstraps a production-ready TandemQueue environment on .NET 9, designed for horizontal scalability and container-first deployment.

### Projects
- `TandemQueue.Domain` — Contracts, DTOs and service interfaces that define core behaviours without infrastructure details.
- `TandemQueue.Application` — Use-cases and job implementations built on domain abstractions (FluentValidation, orchestrators).
- `TandemQueue.Infrastructure` — Modular providers (`AddHangfireInfrastructure`, `AddMonitoringInfrastructure`, `AddRefundInfrastructure`) implementing domain contracts with Hangfire storage, health checks، و استعلام رفاند.
- `TandemQueue.Api` — Minimal API exposing job endpoints، Hangfire dashboard، health و metrics؛ وابسته به abstractionها.
- `TandemQueue.Worker` — Dedicated worker hosting the Hangfire server و recurring job registration + refund orchestrations.
- `TandemQueue.Shared` — Cross-cutting configuration models.

### Key Features
- Clean dependency flow (Domain → Application → Infrastructure → Hosts) with Hangfire server isolated to the worker process.
- Hangfire SQL Server storage with configurable queues/worker count plus dashboard routing from the API.
- Health endpoints (`/health/live`, `/health/ready`) and Prometheus metrics (`/metrics`).
- Serilog logging wired through configuration for both API و worker.
- Multi-stage Dockerfiles, docker-compose برای محلی (SQL Server).
- Kubernetes manifests with ConfigMap/Secret separation، probes، و HPA suggestions.
- GitHub Actions CI pipeline برای build، publish، و container image validation.
- Refund inquiry workflow که ESB را صدا می‌زند و وضعیت جدول `OnlinePay.dbo.RefundTransaction` را به‌روزرسانی می‌کند.
- اسکریپت ساخت TVP و Stored Procedureها در `database/refund_inquiry.sql` موجود است.

### Prerequisites
- .NET SDK 9.0.306 or newer
- Docker Desktop (for container workflows)
- kubectl (for Kubernetes deployment)

### Local Development
```bash
# Restore & build
dotnet restore
dotnet build

# Run API (with Hangfire dashboard and sample endpoints)
dotnet run --project TandemQueue.Api

# Run Worker (dedicated Hangfire server)
dotnet run --project TandemQueue.Worker
```

Visit:
- API Swagger/OpenAPI: `https://localhost:7095/openapi` (development certificate trusted)
- Hangfire dashboard: `https://localhost:7095/hangfire`
- Metrics: `https://localhost:7095/metrics`
- Sample job enqueue: `POST https://localhost:7095/api/jobs/sample-heartbeat`
- Refund inquiry trigger: `POST https://localhost:7095/api/jobs/refunds/inquiry`

### Docker & Compose
```bash
# Build containers
docker compose build

# Start full stack (API, Worker, SQL Server)
docker compose up
```

Environment overrides leverage ASP.NET Core's configuration binding. Example: `SqlServer__ConnectionString` or `Hangfire__Queues__0`.

### Kubernetes
1. Update `image` references in `deploy/k8s/*.yaml` to the published registry tags.
2. Apply manifests:
   ```bash
   kubectl apply -f deploy/k8s/namespace.yaml
   kubectl apply -f deploy/k8s/secret-example.yaml    # replace with secure secret source
   kubectl apply -f deploy/k8s/configmap.yaml
   kubectl apply -f deploy/k8s/api-deployment.yaml
   kubectl apply -f deploy/k8s/api-service.yaml
   kubectl apply -f deploy/k8s/api-hpa.yaml
   kubectl apply -f deploy/k8s/worker-deployment.yaml
   ```
3. Expose the API service via ingress/LoadBalancer of choice.

### GitHub Actions
` .github/workflows/ci.yml` performs restore, build, publish and container builds on pushes/prs. Extend with registry authentication to push images.

### Configuration Reference
| Setting | Description | Default |
| --- | --- | --- |
| `SqlServer:ConnectionString` | Hangfire storage connection | Local SQL Server connection |
| `Hangfire:Queues` | Job queues for server | `["default"]` |
| `Hangfire:WorkerCount` | Worker threads per server | `5` |
| `Hangfire:EnableDashboard` | Toggle dashboard middleware | `true` (API), `false` (Worker) |
| `RefundInquiry:ConnectionString` | DB connection for refund table | `Server=172.17.110.12;Database=OnlinePay;...` |
| `RefundInquiry:Endpoint` | ESB refund inquiry base URL | `https://esb.asiatech.ir/api/v1/bp/refund/inquiry/all` |

### Next Steps
- Add domain-specific jobs to `TandemQueue.Application` and register validators.
- Integrate persistent secrets manager (Azure Key Vault, AWS Secrets Manager, etc.).
- Extend observability (OpenTelemetry exporters to OTLP, distributed tracing backend).

