using log4net;
using Newtonsoft.Json;
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
    public class QualityCenterDefectEntry
    {
        private static readonly ILog _log = LogManager.GetLogger(typeof(QualityCenterDefectEntry));

        public string Type { get; set; } = "defect";

        public int Id { get; set; }
        public string Summary { get; set; }
        public string Description { get; set; }
        public string IsReproducible { get; set; }
        public string DetectedBy { get; set; }
        public string AssignedTo { get; set; }
        public string Status { get; set; }

        [JsonProperty(propertyName: "detected-on-date")]
        public DateTime? DetectedOnDate { get; set; }
        public Severity Severity { get; set; }
        public string TargetVersion { get; set; }
        public string Product { get; set; }
        public string SubSystem { get; set; }
        public string Component { get; set; }
        public string FoundInRelease { get; set; }
        public string FileAsType { get; set; }
        public string Source { get; set; }
        public string Tags { get; set; }
        public string Priority { get; private set; }
        public string State { get; private set; }
        public DateTime? ReadyForTestDate { get; private set; }
        public string TargetRelease { get; private set; }
        public DateTime? CodeCompleteDate { get; private set; }
        public DateTime? ReadyForBuildDate { get; private set; }
        public DateTime? ClosedDate { get; private set; }
        public DateTime? DevReqCloseDate { get; private set; }
        public string LastChangedBy { get; private set; }
        public DateTime? LastChangeDate { get; private set; }

        public Object Audits { get; set; }

        public static IList<QualityCenterDefectEntry> ParseMultiple(string xml)
        {
            var results = new List<QualityCenterDefectEntry>();
            string re = @"[^\x09\x0A\x0D\x20-\xD7FF\xE000-\xFFFD\x10000-x10FFFF]";
            xml = Regex.Replace(xml, re, "");
            var doc = XDocument.Parse(xml);
            var entities = doc.Root.Descendants("Entity");
            foreach (var node in entities)
            {
                results.Add(BuildDefectFromNode(node));
            }
            return results;
        }

        public static QualityCenterDefectEntry ParseSingle(string xml)
        {
            string re = @"[^\x09\x0A\x0D\x20-\xD7FF\xE000-\xFFFD\x10000-x10FFFF]";
            xml = Regex.Replace(xml, re, "");
            var doc = XDocument.Parse(xml);
            return BuildDefectFromNode(doc.Root);
        }

        private static QualityCenterDefectEntry BuildDefectFromNode(XElement node)
        {
            var fields = node.Descendants("Fields").First().Descendants("Field");

            var defect = new QualityCenterDefectEntry
            {
                Id = int.Parse(GetFieldStringValue(fields, "id")),
                Product = GetFieldStringValue(fields, QualityCenterField.Product),
                SubSystem = GetFieldStringValue(fields, QualityCenterField.Subsystem),
                Component = GetFieldStringValue(fields, QualityCenterField.Component),
                FoundInRelease = GetFieldStringValue(fields, QualityCenterField.FoundinRelease),
                Priority = GetFieldStringValue(fields, QualityCenterField.Priority),
                State = GetFieldStringValue(fields, QualityCenterField.State),
                TargetVersion = GetFieldStringValue(fields, QualityCenterField.TargetVersion),
                TargetRelease = GetFieldStringValue(fields, QualityCenterField.TargetRelease),
                Summary = WordUtil.SanitizeSubject(GetFieldStringValue(fields, "name")),
                Description = HtmlUtil.ConvertHtml(GetFieldStringValue(fields, "description")),
                Severity = TranslateSeverity(GetFieldStringValue(fields, "severity")),
                ReadyForTestDate = GetFieldDateValue(fields, QualityCenterField.ReadyforTestDate),
                CodeCompleteDate = GetFieldDateValue(fields, QualityCenterField.CodeCompleteDate),
                ReadyForBuildDate = GetFieldDateValue(fields, QualityCenterField.ReadyForBuildDate),
                ClosedDate = GetFieldDateValue(fields, QualityCenterField.ClosedDate),
                DevReqCloseDate = GetFieldDateValue(fields, QualityCenterField.DevReqCloseDate),
                LastChangedBy = GetFieldStringValue(fields, QualityCenterField.LastChangeBy),
                LastChangeDate = GetFieldDateValue(fields, QualityCenterField.LastChangeDate),
                DetectedBy = GetFieldStringValue(fields, "detected-by"),
                Status = GetFieldStringValue(fields, "status"),
                DetectedOnDate = GetFieldDateValue(fields, QualityCenterField.DetectedonDate),
                AssignedTo = GetFieldStringValue(fields, QualityCenterField.AssignTo),
                Tags = GetFieldStringValue(fields, QualityCenterField.Tags)
            };

            return defect;
        }

        public static List<QualityCenterDefectEntry> ParseSearchResult(string xml)
        {
            var results = new List<QualityCenterDefectEntry>();
            var doc = XDocument.Parse(xml);
            foreach (var entity in doc.Root.Descendants("Entity"))
            {
                var fields = entity.Descendants("Fields").First().Descendants("Field");

                var defect = new QualityCenterDefectEntry
                {
                    Id = int.Parse(GetFieldStringValue(fields, "id")),
                    Summary = WordUtil.SanitizeSubject(GetFieldStringValue(fields, "name")),
                    Description = HtmlUtil.ConvertHtml(GetFieldStringValue(fields, "description")),
                    Severity = TranslateSeverity(GetFieldStringValue(fields, "severity")),
                    DetectedBy = GetFieldStringValue(fields, "detected-by"),
                    Status = GetFieldStringValue(fields, "status"),
                    DetectedOnDate = GetFieldDateValue(fields, QualityCenterField.DetectedonDate),
                    AssignedTo = GetFieldStringValue(fields, QualityCenterField.AssignTo),
                    Tags = GetFieldStringValue(fields, QualityCenterField.Tags)
                };

                results.Add(defect);
            }

            return results;
        }

        private static string GetFieldStringValue(IEnumerable<XElement> fields, string name, string defaultValue = null)
        {
            var element = fields.Where(f => f.Attribute("Name").Value == name).First();
            var value = (element.Descendants("Value").FirstOrDefault() == null ? defaultValue : element.Descendants("Value").FirstOrDefault().Value);
            return value;
        }

        private static DateTime? GetFieldDateValue(IEnumerable<XElement> fields, string name)
        {
            var element = fields.Where(f => f.Attribute("Name").Value == name).First();
            DateTime? value = null;
            if (element.Descendants("Value").FirstOrDefault() != null && element.Descendants("Value").FirstOrDefault().Value.Length > 0)
            {
                value = DateTime.Parse(element.Descendants("Value").FirstOrDefault().Value);
            }
            return value;
        }

        private static Severity TranslateSeverity(string qcSeverity)
        {
            switch (qcSeverity)
            {
                case "1 - Critical":
                    return Severity.Critical;
                case "2 - High":
                    return Severity.High;
                case "3 - Medium":
                    return Severity.Medium;
                default:
                    return Severity.Low;
            }
        }

        public override string ToString()
        {
            return string.Format("Id: {0}, DetectedBy: {1}, DetectedOn: {2}, Severity: {3}, Status: {4}, Summary: {5}", Id, DetectedBy, DetectedOnDate, Severity, Status, Summary);
        }

        public string ToHtmlString()
        {
            var sb = new StringBuilder();
            var template = "<b>{0}: </b>{1}<br />";

            sb.Append(string.Format(template, "Id", Id));
            sb.Append(String.Format(template, "Type", FileAsType));
            sb.Append(String.Format(template, "Detected By", DetectedBy));
            sb.Append(String.Format(template, "Product", Product));
            sb.Append(String.Format(template, "Detected in Version", TargetVersion));
            sb.Append(String.Format(template, "Subsystem", SubSystem));
            sb.Append(String.Format(template, "Component", Component));
            sb.Append(String.Format(template, "Detected on Date", DetectedOnDate.Value.ToString("yyyy-MM-dd")));
            //sb.Append(String.Format(template, "Found in Release", "R10"));
            sb.Append(String.Format(template, "State", "New"));
            switch (Severity)
            {
                case Severity.Critical:
                    sb.Append(String.Format(template, "Severity", "1 - Critical"));
                    break;
                case Severity.High:
                    sb.Append(String.Format(template, "Severity", "2 - High"));
                    break;
                case Severity.Medium:
                    sb.Append(String.Format(template, "Severity", "3 - Medium"));
                    break;
                case Severity.Low:
                    sb.Append(String.Format(template, "Severity", "4 - Low"));
                    break;
            }

            sb.Append(String.Format(template, QualityCenterField.Summary, "[Support Butler] " + XmlUtil.EncodeValue(Summary)));
            sb.Append(String.Format(template, QualityCenterField.AssignTo, AssignedTo));

            sb.Append("<br/><br/>Description:<br/>");
            sb.Append(Description.Replace("\r\n", "<br />"));

            sb.Append(String.Format(template, QualityCenterField.TargetVersion, TargetVersion));

            return sb.ToString();
        }

        public string ToQualityCenterXml()
        {
            /*
                <Entity Type="defect">
                    <Fields>
                            <Field Name="detected-by">
                                <Value>henry_tilney</Value>
                            </Field>
                            <Field Name="creation-time">
                                <Value>2010-03-02</Value>
                            </Field>
                            <Field Name="severity">
                                <Value>2-Medium</Value>
                            </Field>
                            <Field Name="name">
                                <Value>Returned value not does not match
                                            value in database.</Value>
                            </Field>
                    </Fields>
                </Entity>
            */

            var FIELD_VALUE_TEMPLATE = "<Field Name=\"{0}\"><Value>{1}</Value></Field>";

            StringBuilder sb = new StringBuilder();
            sb.Append("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
            sb.Append("<Entity Type=\"defect\">");
            sb.Append("<Fields>");
            sb.Append(String.Format(FIELD_VALUE_TEMPLATE, QualityCenterField.Type, FileAsType));
            sb.Append(String.Format(FIELD_VALUE_TEMPLATE, QualityCenterField.DetectedBy, DetectedBy));
            sb.Append(String.Format(FIELD_VALUE_TEMPLATE, QualityCenterField.Product, XmlUtil.EncodeValue(Product)));
            sb.Append(String.Format(FIELD_VALUE_TEMPLATE, QualityCenterField.DetectedinVersion, TargetVersion));
            sb.Append(String.Format(FIELD_VALUE_TEMPLATE, QualityCenterField.Subsystem, XmlUtil.EncodeValue(SubSystem)));
            sb.Append(String.Format(FIELD_VALUE_TEMPLATE, QualityCenterField.Component, XmlUtil.EncodeValue(Component)));
            sb.Append(String.Format(FIELD_VALUE_TEMPLATE, QualityCenterField.DetectedonDate, DetectedOnDate.Value.ToString("yyyy-MM-dd")));
            sb.Append(String.Format(FIELD_VALUE_TEMPLATE, QualityCenterField.FoundinRelease, "R10"));

            sb.Append(String.Format(FIELD_VALUE_TEMPLATE, QualityCenterField.State, "New"));
            switch (Severity)
            {
                case Severity.Critical:
                    sb.Append(String.Format(FIELD_VALUE_TEMPLATE, QualityCenterField.Severity, "1 - Critical"));
                    break;
                case Severity.High:
                    sb.Append(String.Format(FIELD_VALUE_TEMPLATE, QualityCenterField.Severity, "2 - High"));
                    break;
                case Severity.Medium:
                    sb.Append(String.Format(FIELD_VALUE_TEMPLATE, QualityCenterField.Severity, "3 - Medium"));
                    break;
                case Severity.Low:
                    sb.Append(String.Format(FIELD_VALUE_TEMPLATE, QualityCenterField.Severity, "4 - Low"));
                    break;
            }

            sb.Append(String.Format(FIELD_VALUE_TEMPLATE, QualityCenterField.Summary, "[Support Butler] " + XmlUtil.EncodeValue(Summary)));
            sb.Append(String.Format(FIELD_VALUE_TEMPLATE, QualityCenterField.Description, "<![CDATA[<html><body>" + HtmlUtil.ConvertHtml(Description).Replace("\r\n", "<br />") + "<br />Source = " + Source + "<br />[Issue Created by Support Butler v" + AppInfo.Version + "]" + "</body></html>]]>"));
            sb.Append(String.Format(FIELD_VALUE_TEMPLATE, QualityCenterField.AssignTo, AssignedTo));
            sb.Append(String.Format(FIELD_VALUE_TEMPLATE, QualityCenterField.Tags, Tags));

            sb.Append(String.Format(FIELD_VALUE_TEMPLATE, QualityCenterField.TargetVersion, TargetVersion));
            sb.Append("</Fields>");
            sb.Append("</Entity>");
            return sb.ToString();
        }
    }
}
