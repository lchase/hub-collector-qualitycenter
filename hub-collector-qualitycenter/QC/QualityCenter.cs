using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using log4net;
using System.Diagnostics;
using System.Collections.Specialized;
using System.Web;
using System.Net.Mail;
using QualityCenterMiner.Utils;

namespace QualityCenterMiner.QC
{
    public class QualityCenter
    {
        private static readonly ILog _log = LogManager.GetLogger(typeof(QualityCenter));

        string host;
        string port;
        string cookieInfo;
        string domain;
        string project;

        readonly string URL_BASE = "http://{0}:{1}/qcbin/";
        readonly string URL_AUTH = "authentication-point/authenticate";
        readonly string URL_LOGOUT = "authentication-point/logout";
        readonly string URL_DEFECT_COLLECTION = "rest/domains/{0}/projects/{1}/defects";
        readonly string URL_DEFECT_ATTACHMENTS_COLLECTION = "rest/domains/{0}/projects/{1}/defects/{2}/attachments";
        readonly string URL_GET_DEFECT = "rest/domains/{0}/projects/{1}/defects/{2}";
        readonly string URL_GET_DEFECT_BY_QUERY = "rest/domains/{0}/projects/{1}/defects?query={{{2}}}";
        readonly string URL_GET_USERS = "rest/domains/{0}/projects/{1}/customization/users";
        readonly string URL_GET_DEFECT_HISTORY = "rest/domains/{0}/projects/{1}/defects/{2}/audits";
        readonly string URL_GET_DEFECTS = "rest/domains/{0}/projects/{1}/defects?order-by={{status[ASC];name[DESC]}}&page-size=max&query={{{2}}}";
        //readonly string URL_GET_FIELDS = "rest/domains/{0}/projects/{1}/customization/entities/defect/fields";

        // lock url: http://host:port/qcbin/rest/domains/{domain}/projects/{project}/{Entity Type}/{Entity ID}/lock 
        // check in: http://host:port/qcbin/rest/domains/{domain}/projects/{project}/{Entity Type}/{Entity ID}/versions/check-in 
        // check-out: http://host:port/qcbin/rest/domains/{domain}/projects/{project}/{Entity Type}/{Entity ID}/versions/check-out 
        // logout:  

        public QualityCenter(string host, string port, string domain, string project)
        {
            this.host = host;
            this.port = port;
            this.domain = domain;
            this.project = project;
        }

        public QualityCenter(string url, string domain, string project)
        {
            URL_BASE = url;
            this.domain = domain;
            this.project = project;
        }

        private string BaseUrl
        {
            get { return String.Format(URL_BASE, host, port); }
        }

        private string GetFullUrl(string relativeUrl, params object[] args)
        {
            _log.Debug("FullUrl: " + String.Format(BaseUrl + relativeUrl, args));
            return String.Format(BaseUrl + relativeUrl, args);
        }

        public void Login(string username, string password)
        {
            var authenticationUrl = GetFullUrl(URL_AUTH);

            _log.Info("Connecting to Quality Center...");
            var request = WebRequest.Create(authenticationUrl);
            var authInfo = username + ":" + password;
            authInfo = Convert.ToBase64String(Encoding.Default.GetBytes(authInfo));
            request.Headers.Set(HttpRequestHeader.Authorization, "Basic " + authInfo);
            var response = request.GetResponse();
            this.cookieInfo = response.Headers[HttpResponseHeader.SetCookie].ToString();
            _log.Info("Connected.");
        }

        public void Logout()
        {
            var url = String.Format(URL_LOGOUT, host, port);
        }

        public QualityCenterDefectEntry CreateDefect(QualityCenterDefectEntry entry)
        {
            QualityCenterDefectEntry result = null;
            try
            {
                /*
                entry.Severity = Severity.Medium;
                if (entry.Summary.ToLower().Contains("critical"))
                {
                    entry.Severity = Severity.Critical;
                }
                else if (entry.Summary.ToLower().Contains("high"))
                {
                    entry.Severity = Severity.High;
                }
                else if (entry.Summary.ToLower().Contains("low"))
                {
                    entry.Severity = Severity.Low;
                }*/

                var request = WebRequest.Create(GetFullUrl(URL_DEFECT_COLLECTION, domain, project));
                request.Headers.Set(HttpRequestHeader.Cookie, cookieInfo);
                request.ContentType = "application/xml";

                request.Method = "POST";

                using (System.IO.Stream s = request.GetRequestStream())
                {
                    using (System.IO.StreamWriter sw = new System.IO.StreamWriter(s))
                    {
                        var xml = entry.ToQualityCenterXml();
                        sw.Write(xml);
                        sw.Flush();
                    }
                }

                var response = request.GetResponse();

                using (System.IO.Stream s = response.GetResponseStream())
                {
                    using (System.IO.StreamReader sr = new System.IO.StreamReader(s))
                    {
                        var responseBody = sr.ReadToEnd();

                        // Read parse this defect out.
                        result = QualityCenterDefectEntry.ParseSingle(responseBody);
                    }
                }
            }
            catch (WebException wex)
            {
                using (var stream = wex.Response.GetResponseStream())
                {
                    using (var reader = new StreamReader(stream))
                    {
                        var exception = new QualityCenterException(reader.ReadToEnd());
                        _log.Error(exception.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex.ToString());
            }
            return result;
        }

        public QualityCenterDefectEntry GetDefect(int id, bool withAudit = false)
        {
            var requestDefect = WebRequest.Create(GetFullUrl(URL_GET_DEFECT, domain, project, id));
            requestDefect.Headers.Set(HttpRequestHeader.Cookie, cookieInfo);
            var responseDefect = requestDefect.GetResponse();

            var reader = new StreamReader(responseDefect.GetResponseStream());

            var result = reader.ReadToEnd();

            var entry = QualityCenterDefectEntry.ParseSingle(result);

            if (withAudit) entry.Audits = GetDefectAudit(id);

            reader.Close();

            return entry;
        }

        public Object GetDefectAudit(int id)
        {
            var requestDefect = WebRequest.Create(GetFullUrl(URL_GET_DEFECT_HISTORY, domain, project, id));
            requestDefect.Headers.Set(HttpRequestHeader.Cookie, cookieInfo);
            var responseDefect = requestDefect.GetResponse();

            var reader = new StreamReader(responseDefect.GetResponseStream());

            var result = reader.ReadToEnd();
            return QualityCenterAuditEntry.ParseMultiple(result);
        }

        public List<QualityCenterDefectEntry> GetDefectsByTitle(string title)
        {
            try
            {
                var requestDefect = WebRequest.Create(GetFullUrl(URL_GET_DEFECT_BY_QUERY, domain, project, QualityCenterField.Summary + "['*" + title.Replace("'", "\\'") + "*']"));
                requestDefect.Headers.Set(HttpRequestHeader.Cookie, cookieInfo);
                var responseDefect = requestDefect.GetResponse();

                var reader = new StreamReader(responseDefect.GetResponseStream());

                var result = reader.ReadToEnd();

                var results = QualityCenterDefectEntry.ParseSearchResult(result);

                reader.Close();

                return results;
            }
            catch (WebException wex)
            {

                using (var stream = wex.Response.GetResponseStream())
                {
                    using (var reader = new StreamReader(stream))
                    {
                        var exception = new QualityCenterException(reader.ReadToEnd());
                        _log.Error(exception);
                    }
                }
            }
            return new List<QualityCenterDefectEntry>();
        }

        /*
        public UserEntry GetUser(string email)
        {
            var request = WebRequest.Create(GetFullUrl(URL_GET_USERS, domain, project));
            request.Headers.Set(HttpRequestHeader.Cookie, cookieInfo);
            var response = request.GetResponse();

            var reader = new StreamReader(response.GetResponseStream());
            var result = reader.ReadToEnd();

            var users = UserEntry.Parse(result);

            // Try to find the user strictly by email first.
            UserEntry user = users.Where(u => u.Email.ToLower().CompareTo(email.ToLower()) == 0).OrderBy(u => StringUtil.LevenshteinDistance(u.FullName, email)).FirstOrDefault();

            // If you can't find them this way, tokenize the email and try to find them by name or full name.
            MailAddress address = new MailAddress(email);
            string username = address.User;
            if (username.Contains("."))
            {
                // look up with full name and username "." replace with a " "
                user = users.Where(u => u.FullName.ToLower().CompareTo(username.Replace(".", " ").ToLower()) == 0).FirstOrDefault();
                if (user == null)
                {
                    // couldn't find it, look up with name replacing "." with a "_"
                    user = users.Where(u => u.Name.ToLower().CompareTo(username.Replace(".", "_").ToLower()) == 0).FirstOrDefault();
                }
            }
            else
            {
                // I guess it is 1 word or has an underscore, use that.
                user = users.Where(u => u.Name.ToLower().CompareTo(username.ToLower()) == 0 || u.FullName.ToLower().CompareTo(username.ToLower()) == 0).FirstOrDefault();
            }

            reader.Close();

            return user;
        }

        public void AddAttachment(int defectId, FileParameter fp)
        {
            string contentType = MIMEAssist.GetMIMEType(fp.Name);
            NameValueCollection nvc = new NameValueCollection();
            nvc["filename"] = fp.Name;
            string paramName = "file";
            string file = fp.FullName;

            string boundary = "---------------------------" + DateTime.Now.Ticks.ToString("x");
            byte[] boundarybytes = System.Text.Encoding.ASCII.GetBytes("\r\n--" + boundary + "\r\n");

            HttpWebRequest wr = (HttpWebRequest)WebRequest.Create(GetFullUrl(URL_DEFECT_ATTACHMENTS_COLLECTION,
                    domain, project, defectId));
            wr.ContentType = "multipart/form-data; boundary=" + boundary;
            wr.Method = "POST";
            wr.KeepAlive = true;
            wr.Headers.Set(HttpRequestHeader.Cookie, cookieInfo);

            Stream rs = wr.GetRequestStream();

            string formdataTemplate = "Content-Disposition: form-data; name=\"{0}\"\r\n\r\n{1}";
            foreach (string key in nvc.Keys)
            {
                rs.Write(boundarybytes, 0, boundarybytes.Length);
                string formitem = string.Format(formdataTemplate, key, nvc[key]);
                byte[] formitembytes = System.Text.Encoding.UTF8.GetBytes(formitem);
                rs.Write(formitembytes, 0, formitembytes.Length);
            }
            rs.Write(boundarybytes, 0, boundarybytes.Length);

            string headerTemplate = "Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"\r\nContent-Type: {2}\r\n\r\n";
            string header = string.Format(headerTemplate, paramName, file, contentType);
            byte[] headerbytes = System.Text.Encoding.UTF8.GetBytes(header);
            rs.Write(headerbytes, 0, headerbytes.Length);

            FileStream fileStream = new FileStream(file, FileMode.Open, FileAccess.Read);
            byte[] buffer = new byte[4096];
            int bytesRead = 0;
            while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) != 0)
            {
                rs.Write(buffer, 0, bytesRead);
            }
            fileStream.Close();

            byte[] trailer = System.Text.Encoding.ASCII.GetBytes("\r\n--" + boundary + "--\r\n");
            rs.Write(trailer, 0, trailer.Length);
            rs.Close();

            WebResponse wresp = null;
            try
            {
                wresp = wr.GetResponse();
                Stream stream2 = wresp.GetResponseStream();
                StreamReader reader2 = new StreamReader(stream2);
                _log.Debug(string.Format("File uploaded, server response is: {0}", reader2.ReadToEnd()));
            }
            catch (WebException wex)
            {

                using (var stream = wex.Response.GetResponseStream())
                {
                    using (var reader = new StreamReader(stream))
                    {
                        var exception = new QualityCenterException(reader.ReadToEnd());
                    }
                }
            }
            catch (Exception ex)
            {
                _log.Error("Error uploading file", ex);
                if (wresp != null)
                {
                    wresp.Close();
                    wresp = null;
                }
            }
            finally
            {
                wr = null;
            }
        }
        */

        private static Stream GetPostStream(string filePath, NameValueCollection formData, string boundary)
        {
            Stream postDataStream = new System.IO.MemoryStream();

            //adding form data
            string formDataHeaderTemplate = "--" + boundary + Environment.NewLine +
            "Content-Disposition: form-data; name=\"{0}\"" + Environment.NewLine + Environment.NewLine + "{1}" + Environment.NewLine;

            foreach (string key in formData.Keys)
            {
                byte[] formItemBytes = System.Text.Encoding.UTF8.GetBytes(string.Format(formDataHeaderTemplate,
                key, formData[key]));
                postDataStream.Write(formItemBytes, 0, formItemBytes.Length);
            }

            //adding file data
            FileInfo fileInfo = new FileInfo(filePath);

            string fileHeaderTemplate = Environment.NewLine + "--" + boundary + Environment.NewLine +
                "Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"" +
                Environment.NewLine + "Content-Type: " + MIMEUtil.GetMIMEType(fileInfo.Name) +
                Environment.NewLine + Environment.NewLine;

            byte[] fileHeaderBytes = System.Text.Encoding.UTF8.GetBytes(string.Format(fileHeaderTemplate,
            "file", fileInfo.Name));

            postDataStream.Write(fileHeaderBytes, 0, fileHeaderBytes.Length);

            FileStream fileStream = fileInfo.OpenRead();

            byte[] buffer = new byte[1024];

            int bytesRead = 0;

            while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) != 0)
            {
                postDataStream.Write(buffer, 0, bytesRead);
            }

            fileStream.Close();

            byte[] endBoundaryBytes = System.Text.Encoding.UTF8.GetBytes("--" + boundary + "--");
            postDataStream.Write(endBoundaryBytes, 0, endBoundaryBytes.Length);

            return postDataStream;
        }
        /*
           public ICollection<Test> GetTests(string query)
           {
               Console.WriteLine("[{0}] Executing Query: {1}", DateTime.Now, query);
               var requestDefect = WebRequest.Create(GetFullUrl(URL_GET_DEFECTS, domain, project, query));
               requestDefect.Headers.Set(HttpRequestHeader.Cookie, cookieInfo);

               try
               {
                   var responseDefect = requestDefect.GetResponse();
                   var reader = new StreamReader(responseDefect.GetResponseStream());
                   var result = reader.ReadToEnd();
                   reader.Close();

                   Console.WriteLine("[{0}] Executing Query Complete", DateTime.Now);
                   Console.WriteLine("[{0}] Parsing Results", DateTime.Now);
                   var defects = Test.Parse(result);
                   Console.WriteLine("[{0}] Parsing Complete", DateTime.Now);
               }
               catch (WebException ex)
               {
                   var stream = ex.Response.GetResponseStream();
                   var reader = new StreamReader(stream);

                   Console.WriteLine(reader.ReadToEnd());
               }
               return null;
           }
           */
        private string SerializeFilter(Dictionary<string, string> filter)
        {
            var query = new StringBuilder();
            foreach(var entry in filter)
            {
                if (query.Length > 0) query.Append(";");
                query.Append(entry.Key + "[" + entry.Value + "]");
            }
            return query.ToString();
        }
        
        public IEnumerable<QualityCenterDefectEntry> GetDefects(Dictionary<string, string> filter, bool withAudits = false)
        {
            var query = SerializeFilter(filter);
            var swStep = new Stopwatch();
            var swTotal = new Stopwatch();

            swTotal.Start();
            swStep.Start();
            _log.Info(string.Format("Executing Query: {0}", query));
            var requestDefect = WebRequest.Create(GetFullUrl(URL_GET_DEFECTS, domain, project, query));
            requestDefect.Headers.Set(HttpRequestHeader.Cookie, cookieInfo);

            IEnumerable<QualityCenterDefectEntry> results = new List<QualityCenterDefectEntry>();
            try
            {
                requestDefect.Timeout = 60*60*1000;
                var responseDefect = requestDefect.GetResponse();
                var reader = new StreamReader(responseDefect.GetResponseStream());
                var result = reader.ReadToEnd();
                reader.Close();

                swStep.Stop();
                _log.Info(String.Format("  Complete.. {0}", StopWatchUtil.GetElapsedTimeFormatted(swStep)));

                swStep.Restart();
                _log.Info("  Parsing Results...");
                results = QualityCenterDefectEntry.ParseMultiple(result);
                swStep.Stop();
                _log.Info(String.Format("  Complete.. {0}", StopWatchUtil.GetElapsedTimeFormatted(swStep)));
                _log.Info(String.Format("  {0} defects in query result.", results.Count()));
                swStep.Restart();

                if (withAudits)
                {
                    _log.Info("  Parsing Audit History...");
                    
                    foreach (var defectEntry in results)
                    {
                        defectEntry.Audits = GetDefectAudit(defectEntry.Id);
                    }
                }
                swStep.Stop();
                _log.Info(String.Format("  Complete.. {0}", StopWatchUtil.GetElapsedTimeFormatted(swStep)));
                _log.Info(String.Format("Complete.. {0}", StopWatchUtil.GetElapsedTimeFormatted(swTotal)));
            }
            catch (WebException wex)
            {
                using (var str = wex.Response.GetResponseStream())
                {
                    using (var rdr = new StreamReader(str))
                    {
                        var exception = new QualityCenterException(rdr.ReadToEnd());
                        _log.Error(exception.ToString());
                    }
                }
            }
            return results;
        }

        /*
        public DefectEntry GetDefect(int id)
        {
            var requestDefect = WebRequest.Create(GetFullUrl(URL_GET_DEFECT, domain, project, id));
            requestDefect.Headers.Set(HttpRequestHeader.Cookie, cookieInfo);
            var responseDefect = requestDefect.GetResponse();

            var reader = new StreamReader(responseDefect.GetResponseStream());

            var result = reader.ReadToEnd();



            reader.Close();

            return null;
        }
        */
    }
}
