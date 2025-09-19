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

## Tier 2

##### Consumer de Transações - Console App (Kafka Consumer)

Este aplicativo de console é um consumer Kafka que consome mensagens de transações geradas pelo Producer, processa os dados em paralelo e persiste no Elasticsearch. Ele é ideal para cenários de ingestão em larga escala e processamento confiável de mensagens.

##### Como Funciona

O app se conecta a um tópico Kafka, consome batches de mensagens e processa cada batch em paralelo usando **Channels** e **Tasks**. Ele garante:

- **Processamento em paralelo** com número configurável de canais (`NumberChannels`)
- **Retry de operações críticas** usando Polly para Kafka e Elasticsearch
- **Persistência confiável** no Elasticsearch
- **Commit controlado** das mensagens Kafka
- **Dead Letter Queue (DLQ)** para mensagens que falham

##### Escalabilidade Horizontal do Consumer

O Consumer Kafka foi projetado para rodar múltiplas instâncias em paralelo, aproveitando o agrupamento de consumidores (GroupId) do Kafka. Isso significa que várias instâncias podem processar mensagens do mesmo tópico, dividindo as partições entre si.

No caso:

- Cada consumidor é registrado como um **HostedService** e pertence ao mesmo **GroupId Kafka**.
-  O Kafka realiza load balancing automático:
      - Cada partição do tópico é atribuída a apenas um consumidor dentro do grupo.
      - Se houver mais consumidores que partições, alguns consumidores ficarão ociosos.
      - Se houver menos consumidores que partições, alguns consumidores processarão múltiplas partições.
   
##### Detalhes Consumer

1. **Inicialização do Consumer**
   - O app cria um Consumer Kafka com configurações de **Idempotência** e **Read Committed**.
   - O Consumer se inscreve no tópico configurado (`TransactionConsumer`).

2. **Leitura de mensagens do Kafka**
   - As mensagens chegam em **batches JSON** contendo listas de `Transaction`.
   - Cada batch é escrito em um **Channel** para processamento paralelo.

3. **Processamento paralelo dos batches**
   - Cada canal é processado por uma **Task separada** (`ProcessAndPersistBatchesToElasticAsync`).
   - Cada batch é persistido no **Elasticsearch** em bulk (`bulkSize = 5000`).
   - Se o Elasticsearch falhar, **Polly faz retry automático**.

4. **Commit das mensagens Kafka**
   - Somente após sucesso no Elasticsearch o Consumer **confirma o offset**.
   - Se o processamento falhar de forma crítica, as mensagens vão para a **Dead Letter Queue (DLQ)**.

5. **Notificação de erros críticos**
   - Erros graves são registrados no log e podem disparar alertas (Teams, Slack, SNS, etc.) via método `NotifyTeam`.

6. **Encerramento do Consumer**
   - Quando o app é interrompido, o Channel é fechado e todas as Tasks aguardam finalizar.
   - O Consumer fecha a conexão com Kafka de forma segura.
  
## Configurações Importantes

| Configuração                       | Descrição                                                                                 |
|-----------------------------------|-------------------------------------------------------------------------------------------|
| `NumberChannels`                   | Número de canais paralelos para processamento dos batches. Ajustável conforme recursos do host. |
| `ClientId / GroupId`               | Identificação do consumidor no cluster Kafka.                                             |
| `EnableAutoCommit / EnableAutoOffsetStore` | O offset é controlado manualmente após persistência no Elasticsearch.               |
| `IsolationLevel`                   | Garante leitura apenas de transações commitadas.                                         |

## Tecnologias Usadas
- **.NET** 
- **Kafka**
- **ElasticSearch**
- **Docker**
