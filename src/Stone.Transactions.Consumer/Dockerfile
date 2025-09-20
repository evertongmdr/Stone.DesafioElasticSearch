
# Esta fase é usada durante a execução no VS no modo rápido (Padrão para a configuração de Depuração)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app

# Fase de build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copia apenas o csproj para otimizar restore
COPY ["Stone.Transactions.Consumer/Stone.Transactions.Consumer.csproj", "Stone.Transactions.Consumer/"]
RUN dotnet restore "Stone.Transactions.Consumer/Stone.Transactions.Consumer.csproj"

# Copia todo o código
COPY . .
WORKDIR "Stone.Transactions.Consumer"
RUN dotnet build "Stone.Transactions.Consumer.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Fase de publish
FROM build AS publish
RUN dotnet publish "Stone.Transactions.Consumer.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Fase final
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Stone.Transactions.Consumer.dll"]