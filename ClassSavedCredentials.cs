using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using System.IO;
using System.Security.Cryptography;

namespace SystemMatrixAPIDemoConsoleApp
{
    class ClassSavedCredentials
    {
        static private string theFilename = "SystemMatrixAPIDemoConsoleAppSettings.json";
        static public string theDefaultProfileName = "[DEFAULT]";

        [JsonProperty()]
        public string Username { get; set; } = "Admin";
        public string Password { get; set; } = "Admin";
        public string Address { get; set; } = "localhost";
        public int? Port { get; set; } = 82;


        static public ClassSavedCredentials Load(bool doNotLoad = false)
        {
            ClassSavedCredentials theData;

            // Load the Profiles for diagnostics
            if (File.Exists(theFilename)
                && doNotLoad == false)
            {
                try
                {
                    using (StreamReader file = File.OpenText(theFilename))
                    {
                        JsonSerializer serializer = new JsonSerializer();
                        theData = (ClassSavedCredentials)serializer.Deserialize(file, typeof(ClassSavedCredentials));
                    }
                }
                catch
                {
                    theData = new ClassSavedCredentials();
                }
            }
            else
            {
                theData = new ClassSavedCredentials();
            }

            if (theData == null)
                theData = new ClassSavedCredentials();

            return theData;
        }

        static public void Save(ClassSavedCredentials theData)
        {
            using (StreamWriter file = File.CreateText(theFilename))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Formatting = Formatting.Indented;
                serializer.Serialize(file, theData);
            }
        }
    }
}
