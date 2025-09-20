#!/bin/bash

ES_HOST="http://elasticsearch-node1:9200"  # hostname do serviço Elasticsearch no Docker Compose
POLICY_NAME="transactions_index_policy"
INDEX_NAME="transactions-000001"
WRITE_ALIAS="transactions-write"
READ_ALIAS="transactions-read"

echo "Aguardando Elasticsearch iniciar..."

# Espera Elasticsearch responder
until curl -s "$ES_HOST" >/dev/null; do
  echo "Elasticsearch não disponível ainda, aguardando..."
  sleep 15
done

echo "Aguardando Configuração Elasticsearch..."
sleep 60s

echo "Criando configurações do index(s)..."

echo "Criando ILM policy: transactions_index_policy..."
RESPONSE=$(curl -s -X PUT "$ES_HOST/_ilm/policy/transactions_index_policy" \
  -H 'Content-Type: application/json' \
  -d @/scripts/transactions-policy.json)

# Verifica se há erro no JSON de resposta
if echo "$RESPONSE" | grep -q '"error"'; then
  if echo "$RESPONSE" | grep -q 'already exists'; then
    echo "ILM policy já existe. Continuando..."
  else
    echo "Erro ao criar ILM policy:"
    echo "$RESPONSE"
    exit 1
  fi
else
  echo "ILM policy criada com sucesso."
fi

# ----------------------------
# Criar índice
# ----------------------------
echo "Criando índice: transactions-000001..."
RESPONSE=$(curl -s -X PUT "$ES_HOST/transactions-000001" \
  -H 'Content-Type: application/json' \
  -d @/scripts/transactions-index.json)

# Verifica se há erro no JSON de resposta
if echo "$RESPONSE" | grep -q '"error"'; then
  if echo "$RESPONSE" | grep -q 'resource_already_exists_exception'; then
    echo "Índice já existe. Continuando..."
  else
    echo "Erro ao criar índice:"
    echo "$RESPONSE"
    exit 1
  fi
else
  echo "Índice criado com sucesso."
fi

echo "Configuração do Elasticsearch finalizada!"
