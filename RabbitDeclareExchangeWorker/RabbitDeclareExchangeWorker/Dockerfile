FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
USER app
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["RabbitDeclareExchangeWorker/RabbitDeclareExchangeWorker.csproj", "RabbitDeclareExchangeWorker/"]
RUN dotnet restore "./RabbitDeclareExchangeWorker/./RabbitDeclareExchangeWorker.csproj"
COPY . .
WORKDIR "/src/RabbitDeclareExchangeWorker"
RUN dotnet build "./RabbitDeclareExchangeWorker.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./RabbitDeclareExchangeWorker.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "RabbitDeclareExchangeWorker.dll"]