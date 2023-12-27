using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PerformanceAuditing.Data
{
    [Table("tblAuditResults")]
    public class AuditResults
    {
        public AuditResults() { 
        
        
        }
        [Key]
        public int AuditID { get; set; }
        [DataType(DataType.Url)]
        public string URL { get; set; } = string.Empty;
        public DateTime AccessTime { get; set; }
        public TimeSpan ResponseTime { get; set; }

        public bool Reachable { get; set; }
    }
}
