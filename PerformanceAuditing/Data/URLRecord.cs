using System.Collections.Immutable;

namespace PerformanceAuditing.Data
{
    public record URLRecord
    {
        // hashset to avoid duplicates
        // duplicates are also handled  when adding an item to the queue
        public ImmutableHashSet<string> URLs { get; set; } = ImmutableHashSet<string>.Empty; 
    }

    
}
