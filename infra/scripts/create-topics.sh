#!/bin/bash

echo "Aguardando Kafka iniciar..."

while ! nc -z kafka-desafio-elasticsearch1 9092; do
  sleep 15
done

echo "Kafka iniciado!"

echo "Aguardando Configuração Kafka..."
sleep 60

echo "Criando Tópico(s) Kafka..."

TOPICS=(
  "Transactions"
)

for TOPIC in "${TOPICS[@]}"; do
  kafka-topics --create \
    --bootstrap-server kafka-desafio-elasticsearch1:9092 \
    --replication-factor 1 \
    --partitions 12 \
    --topic "$TOPIC" \
    || echo "Tópico $TOPIC já existe"
done

echo "Todos os tópicos foram criados!"
