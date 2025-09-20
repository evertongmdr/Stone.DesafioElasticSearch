#!/bin/bash

ES_HOST="http://elasticsearch-node1:9200"
POLICY_NAME="transactions_index_policy"
INDEX_NAME="transactions-000001"
WRITE_ALIAS="transactions-write"
READ_ALIAS="transactions-read"

echo "Iniciando configuração da infraestrutura..."

# ========================
# 1. Aguardar Elasticsearch
# ========================
echo "Aguardando Elasticsearch iniciar..."
until curl -s "$ES_HOST" >/dev/null; do
  echo "Elasticsearch não disponível ainda, aguardando..."
  sleep 15
done

echo "Elasticsearch disponível."
sleep 60s

# Criar ILM Policy
echo "Criando ILM policy: $POLICY_NAME..."
RESPONSE=$(curl -s -X PUT "$ES_HOST/_ilm/policy/$POLICY_NAME" \
  -H 'Content-Type: application/json' \
  -d @/elasticsearch/transactions-policy.json)

if echo "$RESPONSE" | grep -q '"error"'; then
  if echo "$RESPONSE" | grep -q 'already exists'; then
    echo "ILM policy já existe. Continuando..."
  else
    echo "Erro ao criar ILM policy:"
    echo "$RESPONSE"
  fi
else
  echo "ILM policy criada com sucesso."
fi

# Criar Índice
echo "Criando índice: $INDEX_NAME..."
RESPONSE=$(curl -s -X PUT "$ES_HOST/$INDEX_NAME" \
  -H 'Content-Type: application/json' \
  -d @/elasticsearch/transactions-index.json)

if echo "$RESPONSE" | grep -q '"error"'; then
  if echo "$RESPONSE" | grep -q 'resource_already_exists_exception'; then
    echo "Índice já existe. Continuando..."
  else
    echo "Erro ao criar índice:"
    echo "$RESPONSE"
  fi
else
  echo "Índice criado com sucesso."
fi

# ========================
# 2. Aguardar Kafka
# ========================

echo "Aguardando Kafka iniciar..."
while ! nc -z kafka-desafio-elasticsearch1 9092; do
  echo "Kafka não disponível ainda, aguardando..."
  sleep 15
done

echo "Kafka disponível."
sleep 30s

# Criar Tópicos Kafka
echo "Criando Tópico(s) Kafka..."
TOPICS=("Transactions")

for TOPIC in "${TOPICS[@]}"; do
  echo "Criando tópico: $TOPIC..."
  
  OUTPUT=$(/opt/kafka/bin/kafka-topics.sh --create \
    --bootstrap-server kafka-desafio-elasticsearch1:9092 \
    --replication-factor 1 \
    --partitions 12 \
    --topic "$TOPIC" 2>&1)

  STATUS=$?
  
  if [ $STATUS -eq 0 ]; then
    echo "Tópico $TOPIC criado com sucesso!"
  else
    echo "Falha ao criar tópico $TOPIC:"
    echo "$OUTPUT"
    
    if echo "$OUTPUT" | grep -q "Topic.*already exists"; then
      echo "Tópico $TOPIC já existia, continuando..."
    else
      echo "Erro inesperado, mas continuando..."
    fi
  fi
done

echo "Configuração da infraestrutura finalziada!"