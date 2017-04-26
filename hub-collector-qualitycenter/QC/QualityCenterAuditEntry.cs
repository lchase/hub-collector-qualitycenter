using QualityCenterMiner.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace QualityCenterMiner.QC
{
    class QualityCenterAuditEntry
    {
        public string Type { get; set; } = "audit";
        public int Id { get; private set; }
        public string Action { get; private set; }
        public DateTime Time { get; private set; }
        public string User { get; private set; }

        public IList<PropertyChange> Changes { get; private set; }

        public static QualityCenterAuditEntry ParseSingle(string xml)
        {
            return null;
        }

        public static IList<QualityCenterAuditEntry> ParseMultiple(string xml)
        {
            var results = new List<QualityCenterAuditEntry>();
            string re = @"[^\x09\x0A\x0D\x20-\xD7FF\xE000-\xFFFD\x10000-x10FFFF]";
            xml = Regex.Replace(xml, re, "");
            var doc = XDocument.Parse(xml);
            var entities = doc.Root.Descendants("Audit");
            foreach (var node in entities)
            {
                results.Add(BuildAuditEntryFromNode(node));
            }
            return results;
        }

        private static QualityCenterAuditEntry BuildAuditEntryFromNode(XElement node)
        {
            var id = node.Descendants("Id").FirstOrDefault();
            var action = node.Descendants("Action").FirstOrDefault();
            var time = node.Descendants("Time").FirstOrDefault();
            var user = node.Descendants("User").FirstOrDefault();

            var propChanges = node.Descendants("Property");

            /*
            var element = fields.Where(f => f.Attribute("Name").Value == name).First();
            var value = (element.Descendants("Value").FirstOrDefault() == null ? defaultValue : element.Descendants("Value").FirstOrDefault().Value);
            return value;
            */

            var entry = new QualityCenterAuditEntry
            {
                Id = int.Parse(id.Value),
                Action = action.Value,
                Time = DateTime.Parse(time.Value),
                User = user.Value,
                Changes = new List<PropertyChange>()
            };

            foreach(var propChange in propChanges)
            {
                var label = propChange.Attributes("Label").FirstOrDefault().Value;
                var name = propChange.Attributes("Name").FirstOrDefault().Value;
                var oldValue = propChange.Descendants("OldValue").FirstOrDefault();
                var newValue = propChange.Descendants("NewValue").FirstOrDefault();
                entry.Changes.Add(new PropertyChange
                {
                    Label = label,
                    Name = name,
                    OldValue = (oldValue != null ? HtmlUtil.ConvertHtml(oldValue.Value) : null),
                    NewValue = (newValue != null ? HtmlUtil.ConvertHtml(newValue.Value) : null)
                });
            }

            return entry;
        }
    }
}
