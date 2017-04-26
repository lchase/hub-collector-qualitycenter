using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace QualityCenterMiner.QC
{
    class QualityCenterField
    {
        public static readonly string ActivityNo = "user-13";
        public static readonly string ActivityRelease = "user-72";
        public static readonly string ActualFixTimeH = "actual-fix-time";
        public static readonly string AdditionalLanguages = "user-48";
        public static readonly string AdditionalEmails = "user-70";
        public static readonly string AgileCraftFlag = "user-62";
        public static readonly string AssignTo = "user-02";
        public static readonly string AssignedTo = "owner";
        public static readonly string Association = "user-52";
        public static readonly string Attachment = "attachment";
        public static readonly string CCRecipient = "user-03";
        public static readonly string Category = "user-12";
        public static readonly string CloneRequired = "user-46";
        public static readonly string ClonedDate = "user-64";
        public static readonly string ClosedDate = "user-24";
        public static readonly string ClosedinVersion = "closing-version";
        public static readonly string ClosingBy = "user-28";
        public static readonly string ClosingDate = "closing-date";
        public static readonly string ClosingReason = "user-20";
        public static readonly string CodeCompleteBy = "user-35";
        public static readonly string CodeCompleteDate = "user-36";
        public static readonly string Comments = "user-31";
        public static readonly string Component = "user-76";
        public static readonly string CopiedFromOnyxID = "user-08";
        public static readonly string CopiedFromQCDefect = "user-90";
        public static readonly string CustomerSI = "user-32";
        public static readonly string CycleID = "cycle-id";
        public static readonly string Database = "user-78";
        public static readonly string DefectID = "id";
        public static readonly string Description = "description";
        public static readonly string DescriptionforReleaseNotes = "user-33";
        public static readonly string DetectedBy = "detected-by";
        public static readonly string DetectedinCycle = "detected-in-rcyc";
        public static readonly string DetectedinRelease = "detected-in-rel";
        public static readonly string DetectedinVersion = "detection-version";
        public static readonly string DetectedonDate = "creation-time";
        public static readonly string DevReqCloseDate = "user-22";
        public static readonly string DevReqClosedCount = "user-55";
        public static readonly string DevReqInfoCount = "user-54";
        public static readonly string DevReqInfoDate = "user-37";
        public static readonly string DevReqPostDate = "user-59";
        public static readonly string DeveloperComments = "dev-comments";
        public static readonly string DocinVersion = "user-57";
        public static readonly string Documented = "user-56";
        public static readonly string DocumentedDate = "user-66";
        public static readonly string DupBugID = "user-21";
        public static readonly string ESRnumber = "user-45";
        public static readonly string Escalatedticket = "user-51";
        public static readonly string EscapingDefect = "user-93";
        public static readonly string EstimatedDocumentationTimeH = "user-69";
        public static readonly string EstimatedFixDate = "user-34";
        public static readonly string EstimatedFixTimeH = "estimated-fix-time";
        public static readonly string ExtendedReference = "extended-reference";
        public static readonly string FailedCount = "user-47";
        public static readonly string FixedinBuildNumber = "user-19";
        public static readonly string FoundIn = "user-11";
        public static readonly string FoundinBuildNumber = "user-82";
        public static readonly string FoundinRelease = "user-06";
        public static readonly string FoundinVersion = "user-05";
        public static readonly string Frequency = "user-01";
        public static readonly string GlobalBuildNumber = "user-74";
        public static readonly string HasChange = "has-change";
        public static readonly string IRBReviewed = "user-41";
        public static readonly string Language = "user-84";
        public static readonly string LastChangeBy = "user-86";
        public static readonly string LastChangeDate = "user-88";
        public static readonly string MSIname = "user-80";
        public static readonly string MarkedasDocby = "user-77";
        public static readonly string MarkedasReqDocby = "user-75";
        public static readonly string Modified = "last-modified";
        public static readonly string NewComments = "dev-comments";
        public static readonly string OS = "user-79";
        public static readonly string OnyxQAID = "user-81";
        public static readonly string PPMRequestId = "request-id";
        public static readonly string PPMRequestNote = "request-note";
        public static readonly string PPMRequestType = "request-type";
        public static readonly string PPMServerURL = "request-server";
        public static readonly string PatchName = "user-14";
        public static readonly string PlannedActivityRelease = "user-73";
        public static readonly string PlannedClosingVersion = "planned-closing-ver";
        public static readonly string Platforms = "user-30";
        public static readonly string PosttoOpenDate = "user-63";
        public static readonly string PostponedDate = "user-61";
        public static readonly string Priority = "priority";
        public static readonly string Product = "user-04";
        public static readonly string Project = "project";
        public static readonly string Projects = "user-09";
        public static readonly string QARootCause = "user-53";
        public static readonly string RTCtoClosedDate = "user-40";
        public static readonly string RTCtoOpenedDate = "user-39";
        public static readonly string ReadyForBuildBy = "user-67";
        public static readonly string ReadyForBuildDate = "user-68";
        public static readonly string ReadyfortestBy = "user-26";
        public static readonly string ReadyforTestDate = "user-29";
        public static readonly string Region = "user-38";
        public static readonly string Regressionbug = "user-83";
        public static readonly string ReopenDate = "user-27";
        public static readonly string Reproducible = "reproducible";
        public static readonly string RequiredDocBy = "user-25";
        public static readonly string RequiresDoc = "user-58";
        public static readonly string RequiresDocDate = "user-65";
        public static readonly string Rootcause = "user-43";
        public static readonly string RunReference = "run-reference";
        public static readonly string SIClosureDate = "user-94";
        public static readonly string Severity = "severity";
        public static readonly string State = "user-23";
        public static readonly string Status = "status";
        public static readonly string StepReference = "step-reference";
        public static readonly string StoryID = "user-60";
        public static readonly string Subject = "subject";
        public static readonly string Subsystem = "user-07";
        public static readonly string Summary = "name";
        public static readonly string Tags = "user-89";
        public static readonly string TargetCycle = "target-rcyc";
        public static readonly string TargetRelease = "user-15";
        public static readonly string TargetRelease_NotInUse = "target-rel";
        public static readonly string TargetVersion = "user-16";
        public static readonly string TestReference = "test-reference";
        public static readonly string TestSetReference = "cycle-reference";
        public static readonly string TestedinRelease = "user-17";
        public static readonly string TestedinVersion = "user-18";
        public static readonly string ToMail = "to-mail";
        public static readonly string TranslationRequired = "user-50";
        public static readonly string Type = "user-42";
        public static readonly string UIChanged = "user-92";
        public static readonly string VersionStamp = "bug-ver-stamp";
        public static readonly string Workaround = "user-49";
        public static readonly string transfer2OnyxDate = "user-85";
        public static readonly string transfer2OnyxFlag = "user-71";

        public string PhysicalName { get; set; }
        public string Name { get; set; }
        public string Label { get; set; }

        /*
         * <?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>
         * <Fields>
         *   <Field 
         *     PhysicalName=\"BG_USER_13\" 
         *     Name=\"user-13\" 
         *     Label=\"Activity No\">
         *       <Size>100</Size>
         *       <History>false</History>
         *       <List-Id>85470</List-Id>
         *       <Required>false</Required>
         *       <System>false</System>
         *       <Type>LookupList</Type>
         *       <Verify>false</Verify>
         *       <Virtual>false</Virtual>
         *       <Active>true</Active>
         *       <Visible>true</Visible>
         *       <Editable>true</Editable>
         *       <Filterable>true</Filterable>
         *       <Groupable>true</Groupable>
         *       <SupportsMultivalue>false</SupportsMultivalue>
         *   </Field>
         * </Fields>
         */

        public static IEnumerable<QualityCenterField> Parse(string xml)
        {
            var doc = XDocument.Parse(xml);
            var query = from f in doc.Root.Descendants("Field")
                        select new QualityCenterField
                        {
                            PhysicalName = f.Attribute("PhysicalName").Value,
                            Name = f.Attribute("Name").Value,
                            Label = f.Attribute("Label").Value
                        };
            return query;
        }

        public override string ToString()
        {
            return string.Format("public static readonly string {0} = \"{1}\";", Label.Replace(" ", ""), Name);
        }
    }
}
