
# Aguarda 60 segundos para garantir que o Kafka/Elasticsearch estejam prontos
sleep 60s

# Inicia a aplicação .NET
dotnet /app/Stone.Transactions.Producer.dll 2
