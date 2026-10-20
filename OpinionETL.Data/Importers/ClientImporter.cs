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
    public class ClientImporter : IClientImporter
    {
        private readonly AppDbContext _context;

        public ClientImporter(AppDbContext context)
        {
            _context = context;
        }

         public async Task<ImportResult> ImportAsync(CancellationToken ct)
        {
            var extractor = new CsvExtractor();
            string dataPath = Path.Combine(AppContext.BaseDirectory, "data");

            var result = new ImportResult()
            {
                TotalImported = 0,
                TotalDuplicated = 0,
                TotalRead = 0,
                TotalRejected = 0
            };

            var clients = extractor.Extract<ClientCsvRow, ClientCsvMap>(Path.Combine(dataPath, "clients.csv"));

            var existingClients = (await _context.Client
                    .Select(x => x.ID)
                    .ToHashSetAsync(ct));

            int pendingImport = 0;

            foreach (var client in clients)
            {
                result.TotalRead++;

                if (string.IsNullOrWhiteSpace(client.IdCliente) ||
                    string.IsNullOrWhiteSpace(client.Nombre) ||
                    string.IsNullOrWhiteSpace(client.Email))
                {
                    result.TotalRejected++;
                    continue;
                }

                if(!int.TryParse(client.IdCliente, out int clientId))
                {
                    result.TotalRejected++;
                    continue;
                }

                if (!existingClients.Add(clientId))
                {
                    result.TotalDuplicated++;
                    continue;
                }

               
                    result.TotalImported++;

                var entity = new Client()
                {
                    ID = clientId,
                    Name = client.Nombre.Trim(),
                    Email = client.Email.Trim()
                };

                _context.Client.Add(entity);
                pendingImport++;    
            }

            await _context.SaveChangesAsync(ct);
            result.TotalImported = pendingImport;
            return result;
        }    
    }
}

