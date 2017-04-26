using log4net;
using Newtonsoft.Json;
using QualityCenterMiner.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace QualityCenterMiner.Utils
{
    class HubClientUtil
    {
        private static readonly ILog _log = LogManager.GetLogger(typeof(HubClientUtil));

        private static readonly string CONTENT_TYPE = "application/vnd.api+json";
        private readonly string _host = null;
        private string _token = null;

        public HubClientUtil(string host)
        {
            _host = host;
        }

        public void Authenticate(string email, string password)
        {
            var endPoint = "auth/login";
            var payload = new { email = "a@a.com", password = "c" };

            _token = Send<HubSession>(endPoint, "POST", null, JsonConvert.SerializeObject(payload)).Token;
        }

        public T Send<T>(string endPoint, string method, string parameters, string body)
        {
            var url = _host + endPoint + parameters;
            _log.Debug("[" + method + "] " + url);
            _log.Debug("Body: " + body);
            var request = (HttpWebRequest)WebRequest.Create(_host + endPoint + parameters);

            request.Method = method;
            request.ContentLength = 0;
            request.ContentType = CONTENT_TYPE;
            if (_token != null) request.Headers["Authorization"] = _token;

            if (!string.IsNullOrEmpty(body) && method == "POST")
            {
                var encoding = new UTF8Encoding();
                var bytes = Encoding.GetEncoding("iso-8859-1").GetBytes(body);
                request.ContentLength = bytes.Length;

                using (var writeStream = request.GetRequestStream())
                {
                    writeStream.Write(bytes, 0, bytes.Length);
                }
            }

            using (var response = (HttpWebResponse)request.GetResponse())
            {
                var responseValue = string.Empty;

                if (response.StatusCode != HttpStatusCode.OK)
                {
                    var message = String.Format("Request failed. Received HTTP {0}", response.StatusCode);
                    throw new ApplicationException(message);
                }

                // grab the response
                using (var responseStream = response.GetResponseStream())
                {
                    if (responseStream != null)
                        using (var reader = new StreamReader(responseStream))
                        {
                            responseValue = reader.ReadToEnd();
                        }
                }

                //_log.Info(JsonConvert.DeserializeObject<T>(responseValue).Token);
                return JsonConvert.DeserializeObject<T>(responseValue);
            }
        }
    }
}
