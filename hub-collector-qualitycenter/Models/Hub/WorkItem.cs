using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QualityCenterMiner.Models.Hub
{
    public class WorkItem
    {
        public int Id { get; set; }
        public string Type { get; set; } = "workItem";

        // Default to quality center data source
        public int DataSourceId { get; set; } = -1000;
        public string ExternalId { get; set; }
        public string Summary { get; set; }
        public string Detail { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ResolvedAt { get; set; }
        public DateTime? ReOpenedAt { get; set; }
        public DateTime? ClosedAt { get; set; }
        public string Severity { get; set; }
        public string Status { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string Creator { get; set; }
        public string Assignee { get; set; }
    }
}
