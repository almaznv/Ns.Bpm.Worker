using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using System.Configuration;

namespace Ns.BpmOnline.Worker
{
    class ResponseAuthStatus
    {
        public int Code { get; set; }
        public string Message { get; set; }
        public object Exception { get; set; }
        public object PasswordChangeUrl { get; set; }
        public object RedirectUrl { get; set; }
    }

    public class BpmConnector
    {
        private string _bpmHost;
        private CookieContainer _authCookie = new CookieContainer();
        private ResponseAuthStatus _authStatus = null;

        private const string authServiceUri = @"/ServiceModel/AuthService.svc/Login";
        private const string processServiceUri = @"/0/ServiceModel/ProcessEngineService.svc/";
        private const string restServiceUri = @"/0/rest/";

        public BpmConnector(string bpmHost)
        {
            _bpmHost = bpmHost;
        }
        
        public bool TryLogin(string userName, string userPassword)
        {
            var authRequest = HttpWebRequest.Create(_bpmHost + authServiceUri) as HttpWebRequest;
            authRequest.Method = "POST";
            authRequest.ContentType = "application/json";
            authRequest.CookieContainer = _authCookie;

            using (var requesrStream = authRequest.GetRequestStream())        
            {
                using (var writer = new StreamWriter(requesrStream))
                {
                    writer.Write(@"{
                        ""UserName"":""" + userName + @""",
                        ""UserPassword"":""" + userPassword + @"""
                    }");
                }
            }

            using (var response = (HttpWebResponse)authRequest.GetResponse())
            {
                using (var reader = new StreamReader(response.GetResponseStream()))
                {
                    string responseText = reader.ReadToEnd();
                    _authStatus = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<ResponseAuthStatus>(responseText);
                }
            }

            if (_authStatus != null)
            {
                if (_authStatus.Code == 0)
                {
                    return true;
                }

            }
            return false;
        }

        public void RunProcess(string processName, Dictionary<string, string> parameters, bool isAsync = true)
        {
            string requestString = String.Format("{0}{1}{2}/Execute", _bpmHost, processServiceUri, processName);
            DoRequest(requestString, parameters, "GET", isAsync);
        }

        public void RunService(string method, string serviceUri, Dictionary<string, string> parameters, bool isAsync = true)
        {
            string requestString = String.Format("{0}{1}{2}", _bpmHost, restServiceUri, serviceUri);
            DoRequest(requestString, parameters, method, isAsync);
        }

        private void DoRequest(string requestUri, Dictionary<string, string> parameters, string method, bool isAsync = true)
        {
            string queryParameters = GetQueryString(parameters);
            if (method == "GET")
            {
                requestUri += queryParameters;
            }

            Logger.Log(String.Format("execute {0} request to: {1}", method, requestUri));

            HttpWebRequest request = HttpWebRequest.Create(requestUri) as HttpWebRequest;
            request.Method = method;
            request.CookieContainer = _authCookie;

            if (request.Method == "POST")
            {
                var data = Encoding.ASCII.GetBytes(queryParameters);
                request.ContentType = "application/x-www-form-urlencoded";
                request.ContentLength = data.Length;
                using (var stream = request.GetRequestStream())
                {
                    stream.Write(data, 0, data.Length);
                }
            }

            if (!isAsync)
            {
                request.GetResponse();
            } else
            {
                request.BeginGetResponse(null, null);
            }
        }

        private string GetQueryString(Dictionary<string, string> parameters)
        {
            List<string> listParams = new List<string> { };
            foreach (var param in parameters)
            {
                listParams.Add(String.Format("{0}={1}", param.Key, param.Value));
            }
            string queryParameters = String.Join("&", listParams.ToArray());
            return (queryParameters == String.Empty) ? String.Empty : "?" + queryParameters;
        }

    }
}
