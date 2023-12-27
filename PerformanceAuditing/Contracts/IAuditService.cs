using PerformanceAuditing.Data;

namespace PerformanceAuditing.Contracts
{
    public interface IAuditService
    {
        public Task SaveResult(AuditResults auditResults);
    }
}
