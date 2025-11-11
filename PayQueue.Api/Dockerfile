FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["PayQueue.Api/PayQueue.Api.csproj", "PayQueue.Api/"]
COPY ["PayQueue.Application/PayQueue.Application.csproj", "PayQueue.Application/"]
COPY ["PayQueue.Infrastructure/PayQueue.Infrastructure.csproj", "PayQueue.Infrastructure/"]
COPY ["PayQueue.Shared/PayQueue.Shared.csproj", "PayQueue.Shared/"]
RUN dotnet restore "PayQueue.Api/PayQueue.Api.csproj"
COPY . .
WORKDIR "/src/PayQueue.Api"
RUN dotnet publish "PayQueue.Api.csproj" -c Release -o /app/out --no-restore

FROM base AS final
WORKDIR /app
COPY --from=build /app/out .
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "PayQueue.Api.dll"]


