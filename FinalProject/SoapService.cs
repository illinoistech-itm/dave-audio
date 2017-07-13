using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace FinalProject
{
    public class SoapService
    {

        public async Task<String[]> GetUserInfo(String cardNum)
        {
            Debug.WriteLine("Getting user info for " + cardNum);
            //Send SOAP request for the 6 digit card number passed as a parameter
            String result = await SendSoapRequest(cardNum);
            if (result != null)
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(result);
                String userData = "";

                XmlNodeList myNodes = xmlDoc.GetElementsByTagName("PCSGetbyCardNumResult");
                foreach (XmlNode childrenNode in myNodes)
                {
                    userData = childrenNode.InnerText;
                }

                if (userData.Equals("Not Found"))
                {
                    Debug.WriteLine("User not found.");
                    return null;
                }
                else
                {
                    String[] splitUserData = userData.Split(new char[] { ',' });
                    return splitUserData;
                }

            }
            else
            {
                Debug.WriteLine("Something went wrong when calling the SOAP API.");
                return null;
            }
        }
        private async Task<String> SendSoapRequest(String cardNum)
        {
            //Generate xml string for the desired SOAP service
            string soap = @"<?xml version=""1.0"" encoding=""utf-8""?>
            <soap:Envelope xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd= ""http://www.w3.org/2001/XMLSchema"" xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"" >         
                <soap:Body>
                    <PCSGetbyCardNum xmlns = ""https://mercury.roger.iit.edu/wsr/auxauthws.asmx?wsdl"">           
                        <cardNumber>" + cardNum + @"</cardNumber>
                    </PCSGetbyCardNum>
                </soap:Body>
            </soap:Envelope>";

            HttpWebRequest req = (HttpWebRequest)WebRequest.Create("https://mercury.roger.iit.edu/wsr/auxauthws.asmx?op=PCSGetbyCardNum");
            req.ContentType = " text/xml; charset=utf-8";
            req.Method = "POST";
            //Set credentials for Basic authentication
            string credentials = "";
            byte[] bytes = Encoding.ASCII.GetBytes(credentials);
            string base64 = Convert.ToBase64String(bytes);
            //Set basi authenticaion
            string authorization = String.Concat("Basic ", base64);
            req.Headers["Authorization"] = authorization;

            try
            {
                //Get stream to send requests
                var requestStream = await req.GetRequestStreamAsync();
                byte[] data = Encoding.ASCII.GetBytes(soap);
                //Write request to stream
                requestStream.Write(data, 0, data.Length);
                requestStream.Dispose();
                //Get response from service
                HttpWebResponse response = (HttpWebResponse)await req.GetResponseAsync();
                Stream responseStream = response.GetResponseStream();
                //Read response Stream
                StreamReader readStream = new StreamReader(responseStream, Encoding.UTF8);
                String responsebody = readStream.ReadToEnd();

                return responsebody;
            }
            catch (WebException wex)
            {
                var aux = new StreamReader(wex.Response.GetResponseStream()).ReadToEnd();
                Debug.WriteLine(aux);
                return null;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                return null;
            }

        }
    }
}
