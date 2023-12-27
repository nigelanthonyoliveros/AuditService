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
    }
}
