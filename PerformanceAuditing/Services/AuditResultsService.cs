using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using PerformanceAuditing.Contracts;
using PerformanceAuditing.Data;

namespace PerformanceAuditing.Services
{
    public class AuditResultsService  : IAuditService
    {
        private readonly ApplicationDBContext context;
        private readonly ILogger<AuditResultsService> _logger;

        public AuditResultsService(ApplicationDBContext context,ILogger<AuditResultsService> logger)
        {
            this.context=context;
            this._logger=logger;
        }

        public async Task SaveResult(AuditResults result)
        {

            context.AuditResults.Add(result);

            try
            {
                await context.SaveChangesAsync();
               _logger.LogInformation("Results saved!");
            }
            catch ( Exception e)
            {

                _logger.LogError("Error on Saving Results {message}", e.Message);
            }
        }

        public async Task<List<AuditResults>> GetAllResults()
        {
            try
            {

                return await this.context.AuditResults.ToListAsync();
            }
            catch (Exception e)
            {
                _logger.LogError($"Error on fetching all the results due to : {e.Message}");
                return new List<AuditResults> () { };
                throw new Exception(e.Message);
                
            }

        }

        public async ValueTask<bool> FlushRecords()
        {
            try
            {
                _logger.LogInformation("Performing truncation...");
                await this.context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE tblAuditResults");

                //var entities = await context.AuditResults.ToListAsync();
                //this.context.AuditResults.RemoveRange(entities);

                await this.context.SaveChangesAsync();
                return true;
            }
            catch (Exception e)
            {
                _logger.LogError($"Error on truncation: {e.Message}");
                return false;
            }


        }
    }
}
