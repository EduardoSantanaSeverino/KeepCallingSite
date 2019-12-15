using System;
using System.IO;
using System.Net;
using Microsoft.Extensions.Configuration;

namespace KeepCallingSiteV2
{
    public class Program
    {
        private const string DELIM = " | ";
        // dotnet publish --self-contained -r rhel.6-x64 -c Release /p:PublishTrimmed=true

        private static IConfiguration Configuration { get; set; }

        public static void Main(string[] args)
        {
            try
            {
                var builder = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
                
                Configuration = builder.Build();
                MainApp();
            }
            catch (Exception ex)
            {
                SaveLog("Message" + DELIM + ex.Message);
            }
        }
        
        private static void MainApp()
        {

            var siteUrl = Configuration.GetSection("SiteUrl").Value;
            var sample = Configuration.GetSection("Sample").Value;
            int.TryParse(sample, out var sampleInt);

            if (string.IsNullOrEmpty(siteUrl))
            {
                SaveLog(CreateMessageToLog("The Site Url Is Empty", "0001", "", "", ""));
                return;
            }
            var webSiteText = GetHtml(Configuration.GetSection("SiteUrl").Value + GetParameters(sampleInt));
            SaveLog(CreateMessageToLog("Success", "0002", OnlyFirstChars(webSiteText, sampleInt), "", siteUrl));
        }

        private static string GetParameters(int from)
        {
            var date = DateTime.Now;
            var retVal = $"?From={from}&currentDateTime={date:u}";
            return retVal;
        }

        private static string OnlyFirstChars(string input, int l = 100)
        {
            if (string.IsNullOrEmpty(input))
            {
                return "";
            }
            if (input.Length > l)
            {
                return input.Substring(0, l);
            }
            return input;
        }

        private static string GetHtml(string urlLink)
        {
            string retVal = "";
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(urlLink);
            HttpWebResponse respose = (HttpWebResponse)request.GetResponse();
            StreamReader sr = new StreamReader(respose.GetResponseStream());
            retVal = sr.ReadToEnd();
            return retVal;
        }

        private static void SaveLog(string message)
        {
            var streamWriter = new StreamWriter("Log.txt", true);
            streamWriter.WriteLine(DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss") + DELIM + message + DELIM + "{ENDLINE}");
            streamWriter.Close();
        }

        private static string CreateMessageToLog(string message, string code, string output, string error, string file)
        {
            return message + DELIM + code + (string.IsNullOrEmpty(output) ? "" : DELIM + output) + (string.IsNullOrEmpty(error) ? "" : DELIM + "Error:" + error) + (string.IsNullOrEmpty(file) ? "" : DELIM + file);
        }
    }
}