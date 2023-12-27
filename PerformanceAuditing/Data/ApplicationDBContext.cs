using Microsoft.EntityFrameworkCore;

namespace PerformanceAuditing.Data
{
    public class ApplicationDBContext : DbContext
    {

        public ApplicationDBContext(DbContextOptions<ApplicationDBContext> options) : base(options)

        {
            
        }
       
        public DbSet<AuditResults> AuditResults { get; set; }
    }
}
