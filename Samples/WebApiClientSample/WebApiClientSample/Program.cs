using Scada;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace WebApiClientSample
{
    /// <summary>
    /// Demonstrates how to use Rapid SCADA 6 web API.
    /// </summary>
    internal class Program
    {
        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

        private static string ReadContentAsString(HttpContent httpContent)
        {
            using Stream responseStream = httpContent.ReadAsStream();
            using StreamReader reader = new(responseStream, Encoding.UTF8);
            return reader.ReadToEnd();
        }

        private static void WriteResponse(HttpResponseMessage httpResponse, out string responseString)
        {
            Console.WriteLine(string.Format("Response status = {0} ({1})",
                (int)httpResponse.StatusCode, httpResponse.StatusCode));
            responseString = ReadContentAsString(httpResponse.Content);
            Console.WriteLine("Response content = " + responseString);
        }

        static void Main(string[] args)
        {
            const string RootPath = "http://localhost:8080/";
            Console.WriteLine("Web API Client Sample");

            CookieContainer cookies = new();
            HttpClientHandler handler = new() { CookieContainer = cookies };
            HttpClient httpClient = new(handler);

            // login
            Console.WriteLine("Login");
            Uri loginUri = new(RootPath + "Api/Auth/Login");
            HttpRequestMessage loginRequest = new(HttpMethod.Post, loginUri)
            {
                Content = JsonContent.Create(new 
                { 
                    Username = "admin",
                    Password = "scada"
                })
            };
            HttpResponseMessage loginResponse = httpClient.Send(loginRequest);
            WriteResponse(loginResponse, out string loginResponseString);
            loginRequest.Dispose();
            loginResponse.Dispose();

            // parse login result
            if (string.IsNullOrEmpty(loginResponseString))
            {
                Console.WriteLine("No response");
            }
            else if (JsonSerializer.Deserialize<SimpleResult>(loginResponseString, JsonOptions) is 
                SimpleResult loginResult)
            {
                Console.WriteLine("Ok = " + loginResult.Ok);
                Console.WriteLine("Msg = " + loginResult.Msg);
            }
            else
            {
                Console.WriteLine("Unable to parse login result");
            }

            // show cookies
            if (cookies.GetCookies(loginUri) is CookieCollection responseCookies &&
                responseCookies.Count > 0)
            {
                Console.WriteLine("Cookies:");

                foreach (Cookie cookie in responseCookies)
                {
                    Console.WriteLine(string.Format("{0} = {1}", cookie.Name, cookie.Value));
                }
            }
            else
            {
                Console.WriteLine("No cookies");
            }

            // get current data
            Console.WriteLine();
            Console.WriteLine("Get current data");
            Uri requestUri = new(RootPath + "Api/Main/GetCurData?cnlNums=101-105,110");
            HttpRequestMessage request = new(HttpMethod.Get, requestUri);
            HttpResponseMessage response = httpClient.Send(request);
            WriteResponse(response, out _);
            request.Dispose();
            response.Dispose();

            // logout
            Console.WriteLine();
            Console.WriteLine("Logout");
            Uri logoutUri = new(RootPath + "Api/Auth/Logout");
            HttpRequestMessage logoutRequest = new(HttpMethod.Post, logoutUri);
            HttpResponseMessage logoutResponse = httpClient.Send(logoutRequest);
            WriteResponse(logoutResponse, out _);
            logoutRequest.Dispose();
            logoutResponse.Dispose();

            handler.Dispose();
            httpClient.Dispose();
        }
    }
}
