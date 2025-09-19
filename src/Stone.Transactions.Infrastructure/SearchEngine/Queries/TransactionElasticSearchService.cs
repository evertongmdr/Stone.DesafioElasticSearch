using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Aggregations;
using Elastic.Clients.Elasticsearch.Fluent;
using Elastic.Clients.Elasticsearch.QueryDsl;
using Elastic.Transport.Extensions;
using Stone.Common.Infrastructure.SearchEngine;
using Stone.Transactions.Domain.DTOs;
using Stone.Transactions.Domain.Entities;
using Stone.Transactions.Domain.Interfaces.Services;
using System.Diagnostics;
using System.Text;

namespace Stone.Transactions.Infrastructure.SearchEngine.Queries
{
    public class TransactionElasticSearchService : ElasticSearchService<Transaction>, ITransactoinSearchEngine
    {
        public TransactionElasticSearchService(SearchEngineSettings settings)
            : base(settings)
        { }

        public async Task<List<Transaction>> GetTransactionsAsync(TransactionQueryParametersDTO parameters)
        {
            var filters = new List<Query>
            {
                new TermQuery
                {
                    Field = Infer.Field<Transaction>(t => t.ClientId),
                    Value = parameters.ClientId.ToString()
                }
            };

            if (parameters.StartDate.HasValue && parameters.EndDate.HasValue)
            {
                filters.Add(new DateRangeQuery(Infer.Field<Transaction>(f => f.CreatedAt))
                {
                    Gte = parameters.StartDate,
                    Lte = parameters.EndDate
                });
            }

            Query query = new BoolQuery
            {
                Filter = filters
            };

            int from = (parameters.CurrentPage - 1) * parameters.PageSize;

            var searchRequest = new SearchRequest("transactions-read")
            {
                Query = query,
                From = from,
                Size = parameters.PageSize
            };

            return await SearchSafeAsync(searchRequest);
        }

        public async Task<List<DailyTransactionSummaryDTO>> GetDailyTotalsAsync(DailyTransactionSummaryParametersDTO parameters)
        {
            var filters = new List<Query>
            {
                new TermQuery
                {
                    Field = Infer.Field<Transaction>(t => t.ClientId),
                    Value = parameters.ClientId.ToString()
                }
            };



            filters.Add(new DateRangeQuery(Infer.Field<Transaction>(f => f.CreatedAt))
            {
                Gte = parameters.StartDate,
                Lte = parameters.EndDate
            });


            Query query = new BoolQuery
            {
                Filter = filters
            };

            var jsonFilters = _client.SourceSerializer.SerializeToString(query);

            Action<FluentDictionaryOfStringAggregation<Transaction>> dailyTotalsAggs = s =>
                s.Add("dateAggs", aggs => aggs
                    .DateHistogram(dh => dh
                        .Field(f => f.CreatedAt)
                        .CalendarInterval(CalendarInterval.Day)
                    )
                    .Aggregations(childAggs => childAggs
                        .Add("typeAggs", aggsType => aggsType
                            .Terms(t => t.Field(f => f.Type))
                            .Aggregations(subAggs => subAggs
                                .Add("totalAmount", sum => sum.Sum(su => su.Field(f => f.Amount)))
                            )
                        )
                    )
                );


            var response = await _client.SearchAsync<Transaction>(s =>
                s.Size(0)
                .Query(query)
                .Aggregations(dailyTotalsAggs)
            );

            Debug.WriteLine(response.ApiCallDetails != null
                ? Encoding.UTF8.GetString(response.ApiCallDetails.RequestBodyInBytes)
                : "No request body");

            if (!response.IsValidResponse || response.ElasticsearchServerError != null)
                throw new InvalidOperationException($"Erro ao calcular totais diários: {response.ElasticsearchServerError?.Error?.Reason}");

            var result = new List<DailyTransactionSummaryDTO>();
            var byDateAgg = response.Aggregations["dateAggs"] as DateHistogramAggregate;

            if (byDateAgg != null)
            {
                foreach (var dateBucket in byDateAgg.Buckets)
                {
                    var byTypeAgg = dateBucket.Aggregations["typeAggs"] as StringTermsAggregate;

                    if (byTypeAgg != null)
                    {
                        foreach (var typeBucket in byTypeAgg.Buckets)
                        {
                            var totalAmount = typeBucket.Aggregations["totalAmount"] as SumAggregate;

                            result.Add(new DailyTransactionSummaryDTO
                            {
                                Date = dateBucket.Key.Date.Date,
                                TransactionType = typeBucket.Key.ToString(),
                                TotalAmount = totalAmount?.Value != null
                                    ? Convert.ToDecimal(totalAmount?.Value)
                                    : 0m
                            });
                        }
                    }
                }
            }

            return result;
        }
    }
}
