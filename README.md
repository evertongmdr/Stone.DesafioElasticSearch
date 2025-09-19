# Desafio ElasticSearch Stone



## Tier 0

### Diagrama Arquitetura

![Descrição da imagem](https://github.com/evertongmdr/Stone.DesafioElasticSearch/blob/master/documentos/diagrama-arquitetura.png)

### Definições Tiers

#### Tier 1

##### Gerador de Massa de Dados - Console App (Kafka Producer)

Este aplicativo de console é um producer Kafka que gera e envia grandes volumes de dados de transações para um cluster Kafka. Ele é ideal para testes de performance, escalabilidade e cenários de estresse, simulando diferentes níveis de carga no sistema.

##### Como Funciona

Ao executar o aplicativo, você verá um menu interativo que permite escolher entre diferentes cenários de geração de dados. Cada cenário envia um número específico de mensagens por transação, simulando diferentes níveis de carga no sistema.


Descrição dos Cenários

##### Menu Principal

##### Descrição dos Cenários

| Opção | Cenário       | Batch Size | Máx. Batches/Envio | Delay (ms) | Total de Mensagens |
|-------|---------------|-----------|------------------|------------|------------------|
| 1     | Alta Carga    | 4.000     | 100              | 500        | 400.000          |
| 2     | Média Carga   | 1.000     | 50               | 250        | 50.000           |
| 3     | Baixa Carga   | 100       | 10               | 200        | 1.000            |

###### Explicação dos parâmetros

- **Batch Size:** Quantidade de mensagens geradas por lote.
- **Máx. Batches/Envio:** Quantos lotes são enviados em sequência antes de aguardar o delay.
- **Delay (ms):** Pausa entre envios de lotes, em milissegundos.

##### Detalhes Producer
- O producer Kafka é configurado com EnableIdempotence = true e Acks = All para garantir envio confiável.
- Cada batch é enviado em paralelo usando Parallel.ForEachAsync.
- As transações Kafka são iniciadas com BeginTransaction() e confirmadas com CommitTransaction().
- Em caso de erro, a transação é abortada com AbortTransaction() e registrada no log.
- Cada mensagem possui headers com informações de aplicação e correlationId.


## Tecnologias Usadas
- **.NET** 
- **Kafka**
- **ElasticSearch**
- **Docker**
