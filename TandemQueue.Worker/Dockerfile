FROM mcr.microsoft.com/dotnet/runtime:9.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["TandemQueue.Worker/TandemQueue.Worker.csproj", "TandemQueue.Worker/"]
COPY ["TandemQueue.Infrastructure/TandemQueue.Infrastructure.csproj", "TandemQueue.Infrastructure/"]
COPY ["TandemQueue.Application/TandemQueue.Application.csproj", "TandemQueue.Application/"]
COPY ["TandemQueue.Shared/TandemQueue.Shared.csproj", "TandemQueue.Shared/"]
RUN dotnet restore "TandemQueue.Worker/TandemQueue.Worker.csproj"
COPY . .
WORKDIR "/src/TandemQueue.Worker"
RUN dotnet publish "TandemQueue.Worker.csproj" -c Release -o /app/out --no-restore

FROM base AS final
WORKDIR /app
COPY --from=build /app/out .
ENTRYPOINT ["dotnet", "TandemQueue.Worker.dll"]


