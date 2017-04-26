using JsonApiSerializer;
using log4net;
using log4net.Config;
using Newtonsoft.Json;
using QualityCenterMiner.Models;
using QualityCenterMiner.Models.Hub;
using QualityCenterMiner.QC;
using QualityCenterMiner.Utils;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace QualityCenterMiner
{
    class Program
    {
        private static readonly ILog _log = LogManager.GetLogger(typeof(Program));
        static void Main(string[] args)
        {
            XmlConfigurator.Configure();

            var qc = new QualityCenter("http://tlvqc:8080/qcbin/", "WAS", "IMPACT360");
            qc.Login("larry_chase", "qc");

            var defect = qc.GetDefect(195970);

            
            //var defectAudit = qc.GetDefectAudit(195970);

            // TODO: Should allow merging of filters with de-dupe.
            var query = new Dictionary<string, string> {
                { QualityCenterField.Product, "\"WFM Web Suite\" Or \"WFM-Core\" Or \"Report Framework\"" },
                { QualityCenterField.TargetVersion, "\"15.1 FP1\"" },
                { QualityCenterField.TargetRelease, "NOT \"HF*\"" },
                { QualityCenterField.Type, "Bug" },
                { QualityCenterField.Subsystem, "Not (Documentation Or \"Doc: Online Help\" Or \"SFP\" Or \"RFS\" Or \"Retail Financial Services\" Or \"RFS Scheduling\" Or \"Request Management\")" },
                { QualityCenterField.Component, "Not (\"My Requests\")" },
                { QualityCenterField.Category, "Not Localization" },
                { QualityCenterField.LastChangeDate, "> " + DateTime.Now.AddDays(-7).Date.ToString("yyyy-MM-dd")}
            };

            var defects = qc.GetDefects(query, false);
            qc.Logout();

            var workItems = MapDefects(defects);

            //var defects = qc.GetDefects( QualityCenterField.Component + "[\"Web Cal*\"]; " + QualityCenterField.State + "[Open]", true);



            File.WriteAllText(@"C:\_temp\defects.json", JsonConvert.SerializeObject(defects, new JsonApiSerializerSettings() { Formatting = Formatting.Indented }));
            File.WriteAllText(@"C:\_temp\workItems.json", JsonConvert.SerializeObject(workItems, new JsonApiSerializerSettings() { Formatting = Formatting.Indented }));
            File.WriteAllText(@"C:\_temp\workItem.json", JsonConvert.SerializeObject(workItems.ElementAt(0), new JsonApiSerializerSettings() { Formatting = Formatting.Indented }));

            // Authenticate.
            var hubClient = new HubClientUtil("http://localhost:8090/");
            hubClient.Authenticate("a@a.com", "c");

            try
            {
                var responseWorkItems = new List<WorkItem>();
                foreach (var workItem in workItems)
                {
                    responseWorkItems.Add(hubClient.Send<WorkItem>("api/workItem", "POST", null, JsonConvert.SerializeObject(workItem, new JsonApiSerializerSettings() { Formatting = Formatting.Indented })));
                }

                File.WriteAllText(@"C:\_temp\workItems-resp.json", JsonConvert.SerializeObject(workItems, new JsonApiSerializerSettings() { Formatting = Formatting.Indented }));
            } catch (Exception ex)
            {
                _log.Error(ex.Message);
            }
            

            /* body
            {
                "email": "a@a.com",
                "password": "c"
            }
            Header
            Content-Type: application/vnd.api+json

            Response: 
            {
                "token": <token>
            }

            Subsequent calls..

            Header:
            - Content-Type: application/vnd.api+json
            - Authorization: <token>
            */

            _log.Info("Press any key...");
            Console.ReadKey();
        }

        // Map
        private static IEnumerable<WorkItem> MapDefects(IEnumerable<QualityCenterDefectEntry> defects)
        {
            var results = new List<WorkItem>();

            foreach (var defect in defects)
            {
                results.Add(new WorkItem
                {
                    ExternalId = defect.Id.ToString(),
                    Summary = defect.Summary,
                    Detail = defect.Description,
                    CreatedAt = defect.DetectedOnDate.Value,
                    ResolvedAt = GetResolvedDate(defect),
                    //ReOpenedAt = entry.,
                    ClosedAt = defect.ClosedDate.HasValue ? defect.ClosedDate.Value : (DateTime?)null,
                    Severity = defect.Severity.ToString(),
                    Status = defect.State,
                    UpdatedAt = defect.LastChangeDate.Value,
                    Assignee = defect.AssignedTo,
                    Creator = defect.DetectedBy,
                });
            }

            return results.AsEnumerable();
        }

        private static DateTime? GetResolvedDate(QualityCenterDefectEntry entry)
        {
            //entry.ReadyForBuildDate.HasValue ? entry.ReadyForBuildDate.Value : entry.ReadyForTestDate
            return null;
        }
    }
}
