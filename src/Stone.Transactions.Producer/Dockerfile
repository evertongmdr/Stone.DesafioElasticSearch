FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app

# Fase de build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copia apenas o csproj para otimizar restore
COPY ["Stone.Transactions.Producer/Stone.Transactions.Producer.csproj", "Stone.Transactions.Producer/"]
RUN dotnet restore "Stone.Transactions.Producer/Stone.Transactions.Producer.csproj"

# Copia todo o código
COPY . .
WORKDIR "Stone.Transactions.Producer"
RUN dotnet build "Stone.Transactions.Producer.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Fase de publish
FROM build AS publish
RUN dotnet publish "Stone.Transactions.Producer.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Fase final
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Mantém o container ativo para poder entrar nele
CMD ["tail", "-f", "/dev/null"]

# ENTRYPOINT ["dotnet", "Stone.Transactions.Producer.dll"]
