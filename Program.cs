using CommandLine;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using PV_NuGet_SystemMatrixAPIAccessCore;
using PV_NuGet_SystemMatrixAPIAccessCore.SM_API;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;

namespace SMcommand
{
    // Define a class to receive parsed values

    internal class Program
    {
        private class Options
        {
            [Option('a', "address", Required = false, HelpText = "System Matrix IP address [example: localhost, 192.168.1.1]")] 
            public string address { get; set; } = null;

            [Option('u', "username", Required = false, HelpText = "System Matrix API username")]
            public string username { get; set; } = null;

            [Option('p', "password", Required = false, HelpText = "System Matrix API password")] 
            public string password { get; set; } = null;

            [Option('s', "save", Required = false, HelpText = "Save the settings (username, password etc)")]
            public bool saveCredentials { get; set; } = false;

            [Option('l', "listdisplays", Required = false, HelpText = "Prints all messages to standard output.")]
            public bool listDisplayIds { get; set; } = false;

            [Option('j', "savejson", Required = false, HelpText = "Saves the Monitoring data to Monitoring.json")]
            public bool saveJson { get; set; } = false;

            [Option('b', "brightness", Required = false, HelpText = "Sets all displays to a brightness [int] (-1 to revert to default value)")]
            public int? brightnessValue { get; set; } = null;

            [Option('z', "nofetch", Required = false, HelpText = "Prevents the Monitoring data from being retrieved - if other commands require the data it will still be retrieved.")]
            public bool SuppressMonitoringDataFetch { get; set; } = false;

            [Option('t', "starttestpattern", Required = false, HelpText = "Starts a test pattern [SolidRed, SolidGreen, SolidBlue, SolidWhite, CycleColors, LinesVertical, LinesDiagonal, GridColors, GridNumbered] [brightness]")]
            public IEnumerable<string> testPattern { get; set; } = null;

            [Option("stoptestpattern", Required = false, HelpText = "Stop test pattern")]
            public bool testPatternStop { get; set; } = false;

            [Option('d', "displays", Required = false, HelpText = "Display ID (GUID or Name) to control (do not include to send to all display)")]
            public IEnumerable<string> DisplayList { get; set; } = null;

            [Option("port", Required = false, HelpText = "The port System Matrix is running on [int] default is 82")]
            public int? port { get; set; } = null;

            [Option("refreshheader", Required = false, HelpText = "Send a header on all controllers.")]
            public bool? refreshHeader { get; set; } = null;

            [Option("poweron", Required = false, HelpText = "Power power supplies On.")]
            public bool powerOn { get; set; } = false;

            [Option("poweroff", Required = false, HelpText = "Power power supplies Off.")]
            public bool powerOff { get; set; } = false;

            [Option("powercycle", Required = false, HelpText = "Cycle Power power supplies.")]
            public bool powerCycle { get; set; } = false;

            [Option("enableoutput", Required = false, HelpText = "Enables output on all controllers.")]
            public bool enableOutput { get; set; } = false;

            [Option("disableoutput", Required = false, HelpText = "Disables output on all controllers.")]
            public bool disableOutput { get; set; } = false;

            [Option('q', "quite", Required = false, HelpText = "Reports only if errors are encountered.")]
            public bool quiteMode { get; set; } = false;
        }


        private static void Main(string[] args)
        {
            HttpClientHandler clientHandler = new HttpClientHandler();
            clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };
            int brightnessValueForTestPattern = 10;

            ClassSavedSettings theData = ClassSavedSettings.Load();

            // Pass the handler to httpclient(from you are calling api)
            HttpClient client = new HttpClient(clientHandler);
            SM_Monitoring theMonitoringData;

            Parser.Default.ParseArguments<Options>(args)
                   .WithParsed<Options>(o =>
                   {
                       if (o.address != null)
                            theData.Address = o.address;

                       if (o.port != null)
                            theData.Port = o.port;

                       if (o.username != null)
                            theData.Username = o.username;

                       if (o.password != null)
                           theData.Password = o.password;

                       if (o.brightnessValue != null)
                           brightnessValueForTestPattern = (int) o.brightnessValue;

                       if (brightnessValueForTestPattern == -1)
                           brightnessValueForTestPattern = 10;

                       if (o.saveCredentials)
                       {
                           Console.WriteLine("Saving settings");
                           ClassSavedSettings.Save(theData);
                       }

                       theMonitoringData = new SM_Monitoring(o.address);

                       // I some commands require the monitoring data override it.
                       if (o.listDisplayIds == true || o.saveJson == true || o.DisplayList != null)
                           o.SuppressMonitoringDataFetch = false;

                       var jsonRawDataAsString = "";
                       var byteData = Encoding.UTF8.GetBytes($"{theData.Username}:{theData.Password}");
                       string _authorization = Convert.ToBase64String(byteData);

                       client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", _authorization);
                       client.Timeout = TimeSpan.FromSeconds(15);

                       //Console.WriteLine($"{theData.Username}:{theData.Password}");

                       bool connectFailed = false;

                       if (o.SuppressMonitoringDataFetch == false)
                       {
                           string uRL_Monitoring = "https://" + theData.Address + ":" + theData.Port + "/api/monitoring";

                           if (! o.quiteMode)
                           {
                               Console.WriteLine("Retrieving Monitoring information...");
                               Console.WriteLine("URL:'" + uRL_Monitoring + "'");
                           }

                           try
                           { 
                               HttpResponseMessage response = client.GetAsync(uRL_Monitoring).Result;

                               var myjsonRawDataAsString = response.Content.ReadAsStringAsync().Result;
                               jsonRawDataAsString = (string)myjsonRawDataAsString.Clone();

                               PV_NuGet_SystemMatrixAPIAccessCore.SM_API.Root theJsonConvertedToObject = JsonConvert.DeserializeObject<PV_NuGet_SystemMatrixAPIAccessCore.SM_API.Root>(myjsonRawDataAsString);
                               string jsonFormatted = JsonConvert.SerializeObject(theJsonConvertedToObject, Newtonsoft.Json.Formatting.Indented);

                               if (o.saveJson)
                               {
                                   if (!o.quiteMode)
                                       Console.WriteLine("Saving Monitoring.json");

                                   using (System.IO.StreamWriter file = new StreamWriter(@"Monitoring.json"))
                                   {
                                       file.Write(jsonFormatted);
                                   }
                               }


                               SM_API_Call theSM_API = new SM_API_Call();
                               theMonitoringData.JsonPayload.httpCallResultDetail = "OK";
                               theMonitoringData.JsonPayload.httpCallJsonRaw = myjsonRawDataAsString;
                               theMonitoringData.JsonPayload.httpCallJsonResultFormatted = jsonFormatted;
                               theMonitoringData.JsonPayload.httpCallCommunicationError = false;
                               theMonitoringData.JsonPayload.SetData(theJsonConvertedToObject);
                               theMonitoringData.Ingest_SystemMatrix_JSON_Information(theMonitoringData.JsonPayload, "NA", false);

                               if (o.listDisplayIds)
                               {
                                   Console.WriteLine("Displays");
                                   foreach (var i in theMonitoringData.theData.Displays)
                                   {
                                       Console.WriteLine("Display: '" + i.Name + "' GUID: " + i.Id);
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


                       List<displayToSendTo> theDisplayToSendTo = new List<displayToSendTo>();

                       foreach (string d in o.DisplayList)
                       {
                            bool isGuid = false;
                            Guid passedInGuid = Guid.Empty;

                            if (Guid.TryParse(d, out passedInGuid))
                            {
                                isGuid = true;
                            }

                           bool foundIt = false;
                            foreach (var i in theMonitoringData.theData.Displays)
                            {
                                if (isGuid)
                                {
                                   if (i.Id == passedInGuid)
                                   {
                                       theDisplayToSendTo.Add(new displayToSendTo (i.Id, i.Name));
                                       foundIt = true;
                                   }
                                }
                                else
                                {
                                   if (d == i.Name)
                                   {
                                       theDisplayToSendTo.Add(new displayToSendTo(i.Id, i.Name));
                                       foundIt = true;
                                   }
                               }
                           }

                            if (foundIt == false)
                            {
                                Console.WriteLine("Error: The selected display [" + d + "] ID is not found.");
                                return;
                            }
                       }


                       // If a monitoring get faile don't bother to do anything else.
                       if (connectFailed == false)
                       {
                           if (o.brightnessValue != null)
                           {
                               string uRL_SetBrightness = null;
                               bool validBrightness = false;

                               if (o.brightnessValue >= 0 && o.brightnessValue <= 100)
                                   validBrightness = true;
                               else
                               {
                                   Console.WriteLine("Error: Test Pattern brightness value not valid: " + o.brightnessValue.ToString());
                                }

                               if (validBrightness == true)
                               {
                                   ClassSystemMatrixPayloadOutputLevels payload = new ClassSystemMatrixPayloadOutputLevels();
                                   if (theDisplayToSendTo.Count == 0) // Global
                                   {
                                       if (o.brightnessValue == -1) // Set to default
                                           PostIt("[ALL]", client, "https://" + theData.Address + ":" + theData.Port + "/api/global/commands/defaultbrightness", o.quiteMode);
                                       else
                                           PostIt("[ALL]", client, "https://" + theData.Address + ":" + theData.Port + "/api/global/commands/brightness/" + o.brightnessValue.ToString(), o.quiteMode);
                                   }
                                   else // Display ID selected
                                   {
                                       foreach (displayToSendTo g in theDisplayToSendTo)
                                       {
                                           if (o.brightnessValue == -1) // Set to default
                                               PostIt(g.displayName, client, uRL_SetBrightness = "https://" + theData.Address + ":" + theData.Port + "/api/displays/" + g.displayGuid + "/commands/defaultbrightness", o.quiteMode);
                                           else
                                               PostIt(g.displayName, client, uRL_SetBrightness = "https://" + theData.Address + ":" + theData.Port + "/api/displays/" + g.displayGuid + "/commands/brightness/" + o.brightnessValue.ToString(), o.quiteMode);
                                       }
                                   }
                               }
                           }


                           if (o.powerOff == true)
                           {
                               if (theDisplayToSendTo.Count == 0) // Global
                                   PostIt("[ALL]", client, "https://" + theData.Address + ":" + theData.Port + "/api/global/commands/powerOff", o.quiteMode);
                               else
                               {
                                   foreach (displayToSendTo g in theDisplayToSendTo)
                                       PostIt(g.displayName, client, "https://" + theData.Address + ":" + theData.Port + "/api/displays/" + g.displayGuid + "/commands/powerOff", o.quiteMode);
                               }
                           }

                           if (o.powerOn == true)
                           {
                               if (theDisplayToSendTo.Count == 0) // Global
                                   PostIt("[ALL]", client, "https://" + theData.Address + ":" + theData.Port + "/api/global/commands/powerOn", o.quiteMode);
                               else
                               {
                                   foreach (displayToSendTo g in theDisplayToSendTo)
                                       PostIt(g.displayName, client, "https://" + theData.Address + ":" + theData.Port + "/api/displays/" + g.displayGuid + "/commands/powerOn", o.quiteMode);
                               }
                           }

                           if (o.powerCycle == true)
                           {
                               if (theDisplayToSendTo.Count == 0) // Global
                                   PostIt("[ALL]", client, "https://" + theData.Address + ":" + theData.Port + "/api/global/commands/powerCycle", o.quiteMode);
                               else
                               {
                                   foreach (displayToSendTo g in theDisplayToSendTo)
                                       PostIt(g.displayName, client, "https://" + theData.Address + ":" + theData.Port + "/api/displays/" + g.displayGuid + "/commands/powerCycle", o.quiteMode);
                               }
                           }

                           if (o.enableOutput == true)
                           {
                               if (theDisplayToSendTo.Count == 0) // Global
                                   PostIt("[ALL]", client, "https://" + theData.Address + ":" + theData.Port + "/api/global/commands/enableOutput", o.quiteMode);
                               else
                               {
                                   foreach (displayToSendTo g in theDisplayToSendTo)
                                       PostIt(g.displayName, client, "https://" + theData.Address + ":" + theData.Port + "/api/displays/" + g.displayGuid + "/commands/enableOutput", o.quiteMode);
                               }
                           }

                           if (o.disableOutput == true)
                           {
                               if (theDisplayToSendTo.Count == 0) // Global
                                   PostIt("[ALL]", client, "https://" + theData.Address + ":" + theData.Port + "/api/global/commands/disableOutput", o.quiteMode);
                               else
                               {
                                   foreach (displayToSendTo g in theDisplayToSendTo)
                                       PostIt(g.displayName, client, "https://" + theData.Address + ":" + theData.Port + "/api/displays/" + g.displayGuid + "/commands/disableOutput", o.quiteMode);
                               }
                           }

                           if (o.refreshHeader == true)
                           {
                               if (theDisplayToSendTo.Count == 0) // Global
                                   PostIt("[ALL]", client, "https://" + theData.Address + ":" + theData.Port + "/api/global/commands/refreshHeader", o.quiteMode);
                               else
                               {
                                   foreach (displayToSendTo g in theDisplayToSendTo)
                                       PostIt(g.displayName, client, "https://" + theData.Address + ":" + theData.Port + "/api/displays/" + g.displayGuid + "/commands/sendHeader", o.quiteMode);
                               }
                           }

                           List<string> TestpatternOptions = new List<string>();
                           if (o.testPattern != null)
                           {
                               foreach (object s in o.testPattern)
                               {
                                   TestpatternOptions.Add((string)s);
                                   Console.WriteLine("Parameter:" + (string)s);
                               }
                           }


                           if (o.testPatternStop)
                           {
                               ClassSystemMatrixPayloadTestPattern thePatternPayload = new ClassSystemMatrixPayloadTestPattern();
                               thePatternPayload.Action = SystemMatrixCommandPayload_Action.Stop;
                               thePatternPayload.Brightness = brightnessValueForTestPattern;

                               var postPayload = new StringContent(thePatternPayload.Json(), Encoding.UTF8, "application/json");

                               if (theDisplayToSendTo.Count == 0) // Global
                               {
                                   foreach (var i in theMonitoringData.theData.Displays)
                                       PostIt(i.Name, client, "https://" + theData.Address + ":" + theData.Port + "/api/displays/" + i.Id + "/commands/testpatterns", o.quiteMode, postPayload);
                               }
                               else
                               {
                                   foreach (displayToSendTo g in theDisplayToSendTo)
                                       PostIt(g.displayName, client, "https://" + theData.Address + ":" + theData.Port + "/api/displays/" + g.displayGuid + "/commands/testpatterns", o.quiteMode, postPayload);
                               }
                           }


                           if (TestpatternOptions.Count > 0)
                           {
                               ClassSystemMatrixPayloadTestPattern thePatternPayload = new ClassSystemMatrixPayloadTestPattern();
                               thePatternPayload.Action = SystemMatrixCommandPayload_Action.Start;
                               thePatternPayload.Brightness = brightnessValueForTestPattern;
                               bool validTestPattern = true;
                               bool validBrightness = false;


                               if (TestpatternOptions.Count == 2)
                               {
                                   switch (TestpatternOptions[0].ToLower())
                                   {
                                       case "solidred": thePatternPayload.TestPattern = SystemMatrixCommandPayload_TestPattern.SolidRed; break;
                                       case "solidgreen": thePatternPayload.TestPattern = SystemMatrixCommandPayload_TestPattern.SolidGreen; break;
                                       case "solidblue": thePatternPayload.TestPattern = SystemMatrixCommandPayload_TestPattern.SolidBlue; break;
                                       case "solidwhite": thePatternPayload.TestPattern = SystemMatrixCommandPayload_TestPattern.SolidWhite; break;
                                       case "cyclecolors": thePatternPayload.TestPattern = SystemMatrixCommandPayload_TestPattern.CycleColors; break;
                                       case "linesvertical": thePatternPayload.TestPattern = SystemMatrixCommandPayload_TestPattern.LinesVertical; break;
                                       case "linesdiagonal": thePatternPayload.TestPattern = SystemMatrixCommandPayload_TestPattern.LinesDiagonal; break;
                                       case "gridcolors": thePatternPayload.TestPattern = SystemMatrixCommandPayload_TestPattern.GridColors; break;
                                       case "gridnumbered": thePatternPayload.TestPattern = SystemMatrixCommandPayload_TestPattern.GridNumbered; break;

                                       default:
                                           {
                                               validTestPattern = false;
                                               Console.WriteLine("Error: Test pattern - not valid pattern: '" + o.testPattern + "'");
                                           }; break;
                                   }

                                   try
                                   {
                                       thePatternPayload.Brightness = Convert.ToDouble(TestpatternOptions[1]);

                                       if (thePatternPayload.Brightness >= 0 && thePatternPayload.Brightness <= 100)
                                       {
                                           Console.WriteLine("Test pattern brightness: " + thePatternPayload.Brightness);
                                           validBrightness = true;
                                       }
                                       else
                                       {
                                           Console.WriteLine("Error: Test Pattern brightness value not valid: " + thePatternPayload.Brightness.ToString());
                                       }
                                   }
                                   catch
                                   {
                                       Console.WriteLine("Error: Test pattern Brightness not correct.");
                                   }


                                   if (validTestPattern == true && validBrightness == true)
                                   {
                                       var postPayload = new StringContent(thePatternPayload.Json(), Encoding.UTF8, "application/json");

                                       if (theDisplayToSendTo.Count == 0) // Global
                                       {
                                           foreach (var i in theMonitoringData.theData.Displays)
                                               PostIt(i.Name, client, "https://" + theData.Address + ":" + theData.Port + "/api/displays/" + i.Id + "/commands/testpatterns", o.quiteMode, postPayload);
                                       }
                                       else
                                       {
                                           foreach (displayToSendTo g in theDisplayToSendTo)
                                               PostIt(g.displayName, client, "https://" + theData.Address + ":" + theData.Port + "/api/displays/" + g.displayGuid + "/commands/testpatterns", o.quiteMode, postPayload);
                                       }
                                   }
                               }
                               else
                               {
                                   Console.WriteLine("Test Pattern missing required parameters. Required [pattern] [brightness] # Parameters provided:" + TestpatternOptions.Count);
                               }
                           }
                       }
                   });
        }

        class displayToSendTo
        {
            public string displayName;
            public Guid displayGuid;
            public double brightness = 10;

            public displayToSendTo (Guid g, string n)
            {
                displayName = n;
                displayGuid = g;
            }

            public displayToSendTo(SM_Display d)
            {
                displayName = d.Name;
                displayGuid = d.Id;
            }
        }


        static bool PostIt(string displayName, HttpClient client, string url, bool quiteMode, StringContent postPayload = null)
        {
            if (!quiteMode)
            {
                Console.WriteLine("Send to display:" + displayName + "   URL:'" + url + "'");

                if (postPayload != null)
                    Console.WriteLine("Payload:'" + postPayload.ReadAsStringAsync().Result + "'");
            }

            try
            {
                HttpResponseMessage response = client.PostAsync(url, postPayload).Result;

                if (!quiteMode)
                    Console.WriteLine("Result:" + response.IsSuccessStatusCode + ", Status:" + response.StatusCode);

                return false;
            }
            catch
            {
                Console.WriteLine("Command failed!");
                return false;
            }
        }




        public class ClassSystemMatrixPayloadOutputLevels
        {
            public double brightness { get; set; }
            public double gamma { get; set; }
            public double red { get; set; }
            public double green { get; set; }
            public double blue { get; set; }

            public ClassSystemMatrixPayloadOutputLevels()
            {
                brightness = 5;
                gamma = 2.2;
                red = 80;
                green = 80;
                blue = 80;
            }


            public string Json()
            {
                string body = "";

                try
                {
                    body = JsonConvert.SerializeObject(this);
                }
                catch
                {
                    body = "";
                }

                return body;
            }
        }

        public enum SystemMatrixCommandPayload_Action { Start, Stop };
        public enum SystemMatrixCommandPayload_TestPattern { SolidRed, SolidGreen, SolidBlue, SolidWhite, CycleColors, LinesVertical, LinesDiagonal, GridColors, GridNumbered };

        public class ClassSystemMatrixPayloadTestPattern
        {
            [JsonConverter(typeof(StringEnumConverter))]
            public SystemMatrixCommandPayload_Action Action { get; set; }
            public double Brightness { get; set; }
            [JsonConverter(typeof(StringEnumConverter))]
            public SystemMatrixCommandPayload_TestPattern TestPattern { get; set; }

            public ClassSystemMatrixPayloadTestPattern()
            {
                Action = SystemMatrixCommandPayload_Action.Start;
                Brightness = 50;
                TestPattern = SystemMatrixCommandPayload_TestPattern.CycleColors;
            }

            public ClassSystemMatrixPayloadTestPattern(SystemMatrixCommandPayload_Action InAction, SystemMatrixCommandPayload_TestPattern InTestPattern, double InBrightness)
            {
                Action = InAction;
                Brightness = InBrightness;
                TestPattern = InTestPattern;
            }


            public string Json()
            {
                string body = "";

                try
                {
                    body = JsonConvert.SerializeObject(this);
                }
                catch
                {
                    body = "";
                }

                return body;
            }
        }



    }
}