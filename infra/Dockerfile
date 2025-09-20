# Imagem base leve
FROM alpine:3.18

# Instalar dependências: bash, curl, openjdk (necessário para kafka-topics)
RUN apk add --no-cache bash curl openjdk11 wget tar netcat-openbsd

# Baixar Kafka (apenas utilitários)
RUN wget https://archive.apache.org/dist/kafka/3.6.0/kafka_2.13-3.6.0.tgz \
    && tar -xzf kafka_2.13-3.6.0.tgz \
    && mv kafka_2.13-3.6.0 /opt/kafka \
    && rm kafka_2.13-3.6.0.tgz

# Adicionar kafka-tools ao PATH
ENV PATH="$PATH:/opt/kafka/bin"

# Definir diretório de trabalho
WORKDIR /scripts

# Copiar script de setup (na prática, você pode montar via volume no docker-compose)
# COPY infra-setup.sh /scripts/infra-setup.sh

# Tornar o script executável
# RUN chmod +x /scripts/infra-setup.sh

# Entrypoint do container: roda o script
ENTRYPOINT ["bash", "/scripts/infra-setup.sh"]
