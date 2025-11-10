FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["TandemQueue.Api/TandemQueue.Api.csproj", "TandemQueue.Api/"]
COPY ["TandemQueue.Application/TandemQueue.Application.csproj", "TandemQueue.Application/"]
COPY ["TandemQueue.Infrastructure/TandemQueue.Infrastructure.csproj", "TandemQueue.Infrastructure/"]
COPY ["TandemQueue.Shared/TandemQueue.Shared.csproj", "TandemQueue.Shared/"]
RUN dotnet restore "TandemQueue.Api/TandemQueue.Api.csproj"
COPY . .
WORKDIR "/src/TandemQueue.Api"
RUN dotnet publish "TandemQueue.Api.csproj" -c Release -o /app/out --no-restore

FROM base AS final
WORKDIR /app
COPY --from=build /app/out .
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "TandemQueue.Api.dll"]


