using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QualityCenterMiner.Utils
{
    class XmlUtil
    {
        private static readonly ILog _log = LogManager.GetLogger(typeof(XmlUtil));

        public static string EncodeValue(string xml)
        {
            _log.Debug("Encoding xml original: " + xml);
            var xmlEncoded = xml.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("\"", "&quot;").Replace("'", "&apos;");
            _log.Debug("Encoding xml encoded: " + xmlEncoded);
            return xmlEncoded;
        }
    }
}
