using CommandLine;
using Newtonsoft.Json;
using PV_NuGet_SystemMatrixAPIAccessCore;
using PV_NuGet_SystemMatrixAPIAccessCore.SM_API;
using System;
using System.IO;
using System.Net.Http;
using System.Text;

namespace SystemMatrixAPIDemoConsoleApp
{
    // Define a class to receive parsed values

    internal class Program
    {
        private class Options
        {
            [Option('a', "Address", Required = false, HelpText = "System Matrix IP address [example: localhost, 192.168.1.1]")] 
            public string address { get; set; } = null;

            [Option('t', "Port", Required = false, HelpText = "The port System Matrix is running on [int] default is 82")]
            public int? port { get; set; } = null;

            [Option('u', "Username", Required = false, HelpText = "System Matrix API username")]
            public string username { get; set; } = null;

            [Option('p', "Password", Required = false, HelpText = "System Matrix API password")] 
            public string password { get; set; } = null;

            [Option('s', "SaveSettings", Required = false, HelpText = "Save the settings (username, password etc)")]
            public bool saveCredentials { get; set; } = false;

            [Option('l', "ListDisplaysID", Required = false, HelpText = "Prints all messages to standard output.")]
            public bool listDisplayIds { get; set; } = false;

            [Option('j', "SaveJSON", Required = false, HelpText = "Saves the Monitoring data to Monitoring.json")]
            public bool saveJson { get; set; } = false;

            [Option('b', "BrightnessValue", Required = false, HelpText = "Sets all displays to a brightness [int] (-1 to revert to default value)")]
            public int? brightnessValue { get; set; } = null;

            [Option("NoDataFetch", Required = false, HelpText = "Prevents the Monitoring data from being retrieved - if other commands require the data it will still be retrieved.")]
            public bool SuppressMonitoringDataFetch { get; set; } = false;

            [Option('i', "SelectDisplay", Required = false, HelpText = "Display ID to control (do not include to send to all display)")]
            public string selectedDisplayID { get; set; } = null;
        }

        private static void Main(string[] args)
        {
            HttpClientHandler clientHandler = new HttpClientHandler();
            clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };

            ClassSavedCredentials theData = ClassSavedCredentials.Load();

            // Pass the handler to httpclient(from you are calling api)
            HttpClient client = new HttpClient(clientHandler);

            Parser.Default.ParseArguments<Options>(args)
                   .WithParsed<Options>(o =>
                   {
                       theData.Address ??= o.address;
                       theData.Port ??= o.port;
                       theData.Username ??= o.username;
                       theData.Password ??= o.password;

                       if (o.saveCredentials)
                       {
                           Console.WriteLine("Saving settings");
                           ClassSavedCredentials.Save(theData);
                       }

                       // I some commands require the monitoring data override it.
                       if (o.listDisplayIds == true || o.saveJson == true)
                           o.SuppressMonitoringDataFetch = false;

                       var jsonRawDataAsString = "";
                       var byteData = Encoding.UTF8.GetBytes($"{theData.Username}:{theData.Password}");
                       string _authorization = Convert.ToBase64String(byteData);

                       client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", _authorization);
                       client.Timeout = TimeSpan.FromSeconds(15);

                       Console.WriteLine($"{theData.Username}:{theData.Password}");

                       bool connectFailed = false;

                       if (o.SuppressMonitoringDataFetch == false)
                       {
                           string uRL_Monitoring = "https://" + theData.Address + ":" + theData.Port + "/api/monitoring";
                           Console.WriteLine("URL:'" + uRL_Monitoring + "'");
                           try
                           {
                               Console.WriteLine("Retrieving Monitoring information...");
                               HttpResponseMessage response = client.GetAsync(uRL_Monitoring).Result;

                               var myjsonRawDataAsString = response.Content.ReadAsStringAsync().Result;
                               jsonRawDataAsString = (string)myjsonRawDataAsString.Clone();

                               PV_NuGet_SystemMatrixAPIAccessCore.SM_API.Root theJsonConvertedToObject = JsonConvert.DeserializeObject<PV_NuGet_SystemMatrixAPIAccessCore.SM_API.Root>(myjsonRawDataAsString);
                               string jsonFormatted = JsonConvert.SerializeObject(theJsonConvertedToObject, Newtonsoft.Json.Formatting.Indented);

                               if (o.saveJson)
                               {
                                   Console.WriteLine("Saving Monitoring.json");
                                   using (System.IO.StreamWriter file = new StreamWriter(@"Monitoring.json"))
                                   {
                                       file.Write(jsonFormatted);
                                   }
                               }

                               SM_Monitoring theMonitoringData = new SM_Monitoring(o.address);

                               SM_API_Call theSM_API = new SM_API_Call();
                               theMonitoringData.JsonPayload.httpCallResultDetail = "OK";
                               theMonitoringData.JsonPayload.httpCallJsonRaw = myjsonRawDataAsString;
                               theMonitoringData.JsonPayload.httpCallJsonResultFormatted = jsonFormatted;
                               theMonitoringData.JsonPayload.httpCallCommunicationError = false;
                               theMonitoringData.JsonPayload.SetData(theJsonConvertedToObject);
                               theMonitoringData.Ingest_SystemMatrix_JSON_Information(theMonitoringData.JsonPayload, "NA", false);

                               if (o.listDisplayIds)
                               {
                                   Console.WriteLine("List Displays IDs");
                                   foreach (var i in theMonitoringData.theData.Displays)
                                   {
                                       Console.WriteLine("Display : " + i.Name + " is GUID: " + i.Id);
                                   }
                               }
                           }
                           catch (Exception e)
                           {
                               connectFailed = true;

                               Console.WriteLine("Connectrion Failed!");
                               Console.WriteLine("Exception:");
                               Console.WriteLine(e.Message);
                               Console.WriteLine("Note: --help for how to use this app");
                           }
                       }

                       // If a monitoring get faile don't bother to do anything else.
                       if (connectFailed == false)
                       {
                           if (o.brightnessValue != null)
                           {
                               string uRL_SetBrightness = null;

                               if (o.selectedDisplayID == null) // Global
                               {
                                   if (o.brightnessValue == -1) // Set to default
                                   {
                                       uRL_SetBrightness = "https://" + theData.Address + ":" + theData.Port + "/api/global/commands/defaultBrightness";
                                   }
                                   else
                                   {
                                       uRL_SetBrightness = "https://" + theData.Address + ":" + theData.Port + "/api/global/commands/brightness/" + o.brightnessValue.ToString();
                                   }
                               }
                               else // Display ID selected
                               {
                                   uRL_SetBrightness = "https://" + theData.Address + ":" + theData.Port + "/api/displays/" + o.selectedDisplayID + "/commands/brightness/" + o.brightnessValue.ToString();
                               }

                               Console.WriteLine("Sending brightness command");
                               Console.WriteLine("URL:'" + uRL_SetBrightness + "'");

                               try
                               {
                                   HttpResponseMessage response = client.GetAsync(uRL_SetBrightness).Result;
                                   Console.WriteLine("Result:" + response.IsSuccessStatusCode + ", Status:" + response.StatusCode);
                               }
                               catch
                               {
                                   Console.WriteLine("Brightness command failed!");
                               }
                           }
                       }
                   });
        }
    }
}