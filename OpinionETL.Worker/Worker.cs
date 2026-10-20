using OpinionETL.Data.Extraction.Csv;
using OpinionETL.Data.Extraction.Csv.Mapping;
using OpinionETL.Data.Extraction.Csv.Models;

namespace OpinionETL.Worker
{
    public class Worker(ILogger<Worker> logger) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            //while (!stoppingToken.IsCancellationRequested)
            //{
            //    if (logger.IsEnabled(LogLevel.Information))
            //    {
            //        logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            //    }
            //    await Task.Delay(1000, stoppingToken);
            //}

            // Confirmación de que el directorio de datos existe y que todo extrae correctamente (temporal)

            var extractor = new CsvExtractor();

            string dataPath = Path.Combine(AppContext.BaseDirectory, "data");

            var clients = extractor.Extract<ClientCsvRow, ClientCsvMap>(Path.Combine(dataPath, "clients.csv"));
            var products = extractor.Extract<ProductCsvRow, ProductCsvMap>(Path.Combine(dataPath, "products.csv"));
            var socialComments = extractor.Extract<SocialCommentCsvRow, SocialCommentCsvMap>(Path.Combine(dataPath, "social_comments.csv"));
            var dataSources = extractor.Extract<DataSourceCsvRow, DataSourceCsvMap>(Path.Combine(dataPath, "fuente_datos.csv"));
            var surveys = extractor.Extract<SurveyCsvRow, SurveyCsvMap>(Path.Combine(dataPath, "surveys_part1.csv"));
            var webReviews = extractor.Extract<WebReviewCsvRow, WebReviewCsvMap>(Path.Combine(dataPath, "web_reviews.csv"));

            logger.LogInformation("Extracted {count} clients from CSV.", clients.Count);
            logger.LogInformation("Extracted {count} products from CSV.", products.Count);
            logger.LogInformation("Extracted {count} social comments from CSV.", socialComments.Count);
            logger.LogInformation("Extracted {count} data sources from CSV.", dataSources.Count);
            logger.LogInformation("Extracted {count} surveys from CSV.", surveys.Count);
            logger.LogInformation("Extracted {count} web reviews from CSV.", webReviews.Count);

        }
    }
}
