using Microsoft.EntityFrameworkCore;
using OpinionETL.Core;
using OpinionETL.Core.Interfaces;
using OpinionETL.Core.Models;
using OpinionETL.Data.Context;
using OpinionETL.Data.Extraction.Csv;
using OpinionETL.Data.Extraction.Csv.Mapping;
using OpinionETL.Data.Extraction.Csv.Models;

namespace OpinionETL.Data.Importers
{
    public class OpinionStagingImporter : IOpinionStagingImporter
    {
        private readonly AppDbContext _context;

        public OpinionStagingImporter(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ImportResult> ImportAsync(
            CancellationToken ct = default)
        {
            var extractor = new CsvExtractor();

            string dataPath = Path.Combine(
                AppContext.BaseDirectory,
                "data");

            var result = new ImportResult();

            var surveys = extractor
                .Extract<SurveyCsvRow, SurveyCsvMap>(
                    Path.Combine(dataPath, "surveys_part1.csv"));

            var socialComments = extractor
                .Extract<SocialCommentCsvRow, SocialCommentCsvMap>(
                    Path.Combine(dataPath, "social_comments.csv"));

            var webReviews = extractor
                .Extract<WebReviewCsvRow, WebReviewCsvMap>(
                    Path.Combine(dataPath, "web_reviews.csv"));

            var stageOpinions = new List<StageOpinion>();

            foreach (var survey in surveys)
            {
                stageOpinions.Add(MapSurvey(survey));
            }

            foreach (var socialComment in socialComments)
            {
                stageOpinions.Add(MapSocialComment(socialComment));
            }

            foreach (var webReview in webReviews)
            {
                stageOpinions.Add(MapWebReview(webReview));
            }


            var existingKeys = (await _context.Stage_Opinion
                    .Select(x => new
                    {
                        x.Origin,
                        x.Original_Opinion_ID
                    })
                    .ToListAsync(ct))
                .Select(x => (
                    Origin: x.Origin.Trim().ToUpperInvariant(),
                    OpinionId: x.Original_Opinion_ID.Trim().ToUpperInvariant()
                ))
                .ToHashSet();

            int pendingImport = 0;

            foreach (var staging in stageOpinions)
            {
                result.TotalRead++;

                if (string.IsNullOrWhiteSpace(staging.Origin) ||
                    string.IsNullOrWhiteSpace(staging.Original_Opinion_ID))
                {
                    result.TotalRejected++;
                    continue;
                }

                staging.Origin = staging.Origin.Trim();

                staging.Original_Opinion_ID =
                    staging.Original_Opinion_ID.Trim();

                var key = (
                    staging.Origin.ToUpperInvariant(),
                    staging.Original_Opinion_ID.ToUpperInvariant()
                );

                if (!existingKeys.Add(key))
                {
                    result.TotalDuplicated++;
                    continue;
                }

               
                staging.Status = "Pending";
                staging.Loaded_At = DateTime.UtcNow;

                _context.Stage_Opinion.Add(staging);

                pendingImport++;
            }

            await _context.SaveChangesAsync(ct);
            result.TotalImported = pendingImport;
            return result;
        }

        #region Mapeos CSV a StageOpinion

        private StageOpinion MapSurvey(SurveyCsvRow survey)
        {
            return new StageOpinion
            {
                Origin = "Survey",
                Original_Opinion_ID = survey.IdOpinion,
                Original_Client_ID = survey.IdCliente,
                Original_Product_ID = survey.IdProducto,
                Comment = survey.Comentario,
                Original_Date = survey.Fecha,
                Original_Classification = survey.Clasificación,
                Original_Score = survey.PuntajeSatisfacción,
                Source_Name = survey.Fuente
            };
        }

        private StageOpinion MapSocialComment(
            SocialCommentCsvRow socialComment)
        {
            return new StageOpinion
            {
                Origin = "SocialComment",
                Original_Opinion_ID = socialComment.IdComment,
                Original_Client_ID = socialComment.IdCliente,
                Original_Product_ID = socialComment.IdProducto,
                Comment = socialComment.Comentario,
                Original_Date = socialComment.Fecha,
                Original_Classification = null,
                Original_Score = null,
                Source_Name = socialComment.Fuente
            };
        }

        private StageOpinion MapWebReview(WebReviewCsvRow webReview)
        {
            return new StageOpinion
            {
                Origin = "WebReview",
                Original_Opinion_ID = webReview.IdReview,
                Original_Client_ID = webReview.IdCliente,
                Original_Product_ID = webReview.IdProducto,
                Comment = webReview.Comentario,
                Original_Date = webReview.Fecha,
                Original_Classification = null,
                Original_Score = webReview.Rating,
                Source_Name = "Reseña Web"
            };
        }

        #endregion
    }
}
