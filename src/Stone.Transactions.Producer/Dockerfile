FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
COPY --from=publish /app/publish .

# Copia o entrypoint unificado
COPY entrypoint.sh /usr/work/entrypoint.sh
RUN chmod +x /usr/work/entrypoint.sh

ENTRYPOINT ["/usr/work/entrypoint.sh"]

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["Stone.Transactions.Producer/Stone.Transactions.Producer.csproj", "Stone.Transactions.Producer/"]
COPY ["Stone.Common/Stone.Common.csproj", "Stone.Common.csproj"]
RUN dotnet restore "Stone.Transactions.Producer/Stone.Transactions.Producer.csproj"
COPY . .
WORKDIR "Stone.Transactions.Producer"
RUN dotnet build "Stone.Transactions.Producer.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Stone.Transactions.Producer.csproj" -c Release -o /app/publish /p:UseAppHost=false
