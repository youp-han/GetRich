using System;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.IO;

namespace Lotto.Core
{
    public class CoreAPICall
    {
        public string CallAPI(string Url)
        {

            string result = null;
            try
            {
                
                WebRequest request = WebRequest.Create(Url);

                request.Method = "POST";
                request.ContentType = "application/json";

                //ignore SSL
                ServicePointManager.ServerCertificateValidationCallback = delegate (
                    Object obj, X509Certificate certificate, X509Chain chain,
                    SslPolicyErrors errors)
                {
                    return (true);
                };
                ServicePointManager.Expect100Continue = false;
                //ignore SSL End


                Stream dataStream = request.GetRequestStream();
                dataStream.Close();

                WebResponse response = request.GetResponse();
                dataStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(dataStream);
                result = reader.ReadToEnd();
                reader.Close();
                dataStream.Close();
                response.Close();

            }
            catch (Exception e)
            {
                throw e;
            }

            return result;
        }
    }
}