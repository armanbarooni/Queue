FROM mcr.microsoft.com/dotnet/runtime:9.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["PayQueue.Worker/PayQueue.Worker.csproj", "PayQueue.Worker/"]
COPY ["PayQueue.Infrastructure/PayQueue.Infrastructure.csproj", "PayQueue.Infrastructure/"]
COPY ["PayQueue.Application/PayQueue.Application.csproj", "PayQueue.Application/"]
COPY ["PayQueue.Shared/PayQueue.Shared.csproj", "PayQueue.Shared/"]
RUN dotnet restore "PayQueue.Worker/PayQueue.Worker.csproj"
COPY . .
WORKDIR "/src/PayQueue.Worker"
RUN dotnet publish "PayQueue.Worker.csproj" -c Release -o /app/out --no-restore

FROM base AS final
WORKDIR /app
COPY --from=build /app/out .
ENTRYPOINT ["dotnet", "PayQueue.Worker.dll"]


