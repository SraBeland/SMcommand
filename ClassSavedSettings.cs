using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using System.IO;
using System.Security.Cryptography;

namespace SMcommand
{
    class ClassSavedSettings
    {
        static private string theFilename = "SMcommandSettings.json";
        static public string theDefaultProfileName = "[DEFAULT]";

        [JsonProperty()]
        public string Username { get; set; } = "Admin"; 
        public string Password { get; set; } = "Password";
        public string Address { get; set; } = "localhost"; 
        public int? Port { get; set; } = 82;


        static public ClassSavedSettings Load(bool doNotLoad = false) 
        {
            ClassSavedSettings theData;

            // Load the Profiles for diagnostics
            if (File.Exists(theFilename)
                && doNotLoad == false)
            {
                try
                {
                    using (StreamReader file = File.OpenText(theFilename))
                    {
                        JsonSerializer serializer = new JsonSerializer();
                        theData = (ClassSavedSettings)serializer.Deserialize(file, typeof(ClassSavedSettings));
                    }
                }
                catch
                {
                    theData = new ClassSavedSettings();
                }
            }
            else
            {
                theData = new ClassSavedSettings();
            }

            if (theData == null)
                theData = new ClassSavedSettings();

            return theData;
        }

        static public void Save(ClassSavedSettings theData)
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
