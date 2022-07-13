using System;
using System.Configuration;
using System.IO;
using System.Net;
using System.Text;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AttendancePortal.Common
{
    public static class CommonClass
    {
        /// <summary>
        /// Authorize user.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string AuthorizeUser(string data)
        {
            return GetAuthorizationUrl(data).ToString();
        }

        /// <summary>
        /// Encode parameter value.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string EncodeBase64(this string value)
        {
            var valueBytes = Encoding.UTF8.GetBytes(value);
            return Convert.ToBase64String(valueBytes);
        }

        /// <summary>
        /// Decode parameter value.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string DecodeBase64(this string value)
        {
            var valueBytes = System.Convert.FromBase64String(value);
            return Encoding.UTF8.GetString(valueBytes);
        }

        /// <summary>
        /// Get Authorization url
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private static string GetAuthorizationUrl(string data)
        {
            try
            {
                string _Url = ConfigurationManager.AppSettings["Url"];
                string _clientId = ConfigurationManager.AppSettings["ClientId"];
                string _redirectUrl = ConfigurationManager.AppSettings["RedirectUrl"];
                string _scopes = ConfigurationManager.AppSettings["Scopes"];
                string _accountDomain = ConfigurationManager.AppSettings["AccountDomain"];

                StringBuilder UrlBuilder = new StringBuilder(_Url);
                UrlBuilder.Append("client_id=" + _clientId);
                UrlBuilder.Append("&redirect_uri=" + _redirectUrl);
                UrlBuilder.Append("&response_type=" + "code");
                UrlBuilder.Append("&scope=" + _scopes);
                UrlBuilder.Append("&access_type=" + "offline");
                UrlBuilder.Append("&state=" + EncodeBase64(data));
                UrlBuilder.Append("&hd=" + _accountDomain);
                return UrlBuilder.ToString();
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        /// <summary>
        /// Exchange Authorization Code
        /// </summary>
        /// <param name="UserName"></param>
        /// <param name="code"></param>
        /// <param name="accessToken"></param>
        /// <returns></returns>
        public static string ExchangeAuthorizationCode(string UserName, string code, out string accessToken)
        {
            try
            {
                accessToken = string.Empty;

                string ClientSecret = ConfigurationManager.AppSettings["ClientSecret"];
                string ClientId = ConfigurationManager.AppSettings["ClientId"];
                string RedirectUrl = ConfigurationManager.AppSettings["RedirectUrl"];

                var Content = "code=" + code +
                    "&client_id=" + ClientId +
                    "&client_secret=" + ClientSecret +
                    "&redirect_uri=" + RedirectUrl +
                    "&grant_type=authorization_code";

                var request = WebRequest.Create("https://accounts.google.com/o/oauth2/token");
                request.Method = "POST";
                byte[] byteArray = Encoding.UTF8.GetBytes(Content);
                request.ContentType = "application/x-www-form-urlencoded";
                request.ContentLength = byteArray.Length;

                using (Stream dataStream = request.GetRequestStream())
                {
                    dataStream.Write(byteArray, 0, byteArray.Length);
                    dataStream.Close();
                }

                var Response = (HttpWebResponse)request.GetResponse();
                Stream responseDataStream = Response.GetResponseStream();
                StreamReader reader = new StreamReader(responseDataStream);
                string ResponseData = reader.ReadToEnd();
                reader.Close();
                responseDataStream.Close();
                Response.Close();
                if (Response.StatusCode == HttpStatusCode.OK)
                {
                    var ReturnedToken = JsonConvert.DeserializeObject<Token>(ResponseData);

                    accessToken = ReturnedToken.access_token;
                    return ReturnedToken.refresh_token;
                }
                else
                {
                    return string.Empty;
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        /// <summary>
        /// Fetch employee email id.
        /// </summary>
        /// <param name="accessToken"></param>
        /// <returns></returns>
        public static string FetchEmailId(string accessToken)
        {
            try
            {
                var EmailRequest = ConfigurationManager.AppSettings["EmailRequest"] + "&access_token=" + accessToken;
                var Request = WebRequest.Create(EmailRequest);
                var Response = (HttpWebResponse)Request.GetResponse();
                var DataStream = Response.GetResponseStream();
                var Reader = new StreamReader(DataStream);
                var JsonString = Reader.ReadToEnd();
                Reader.Close();
                DataStream.Close();
                Response.Close();

                dynamic json = JValue.Parse(JsonString);
                return json.email;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }

    /// <summary>
    /// Token class
    /// </summary>
    public class Token
    {
        public string access_token { get; set; }
        
        public string refresh_token { get; set; }
    }
}