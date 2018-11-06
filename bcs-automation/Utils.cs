using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Net.Security;

namespace bcs_automation
{
    public class Requests
    {
        private CookieContainer cookies = new CookieContainer();
        public string GET(string url)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            request.Accept = "*/*";
            request.ContentType = "application/json";
            request.CookieContainer = cookies;

            //ServicePointManager.ServerCertificateValidationCallback = new
            //RemoteCertificateValidationCallback
            //(
            //   delegate { return true; }
            //);

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }
        public string PUT(string url, string put_data)
        {
            var request = (HttpWebRequest)WebRequest.Create(url);
            var data = Encoding.ASCII.GetBytes(put_data);
            request.Method = "PUT";
            request.ContentType = "application/json";
            request.ContentLength = data.Length;
            request.CookieContainer = cookies;
            request.Accept = "*/*";

            //ServicePointManager.ServerCertificateValidationCallback = new
            //RemoteCertificateValidationCallback
            //(
            //   delegate { return true; }
            //);

            using (var stream = request.GetRequestStream())
            {
                stream.Write(data, 0, data.Length);
            }

            HttpWebResponse response = null;
            response = (HttpWebResponse)request.GetResponse();
            string s = new StreamReader(response.GetResponseStream()).ReadToEnd();
            return s;
        }
    }
}
