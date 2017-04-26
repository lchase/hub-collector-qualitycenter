using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace QualityCenterMiner.QC
{
    class QualityCenterException
    {
        public readonly string Id;
        public readonly string Title;
        public readonly string StackTrace;

        public QualityCenterException(string xml)
        {
            XDocument doc = XDocument.Parse(xml);

            if (doc.Root.Name.LocalName.CompareTo("QCRestException") == 0)
            {
                Id = doc.Root.Element("Id").Value;
                Title = doc.Root.Element("Title").Value;
                StackTrace = doc.Root.Element("StackTrace").Value;
            }
        }

        public override string ToString()
        {
            return String.Format("Id: {0}, Title: {1}, Stack Trace: {2}", Id, Title, StackTrace);
        }
    }
}
