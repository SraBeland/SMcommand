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

        [JsonIgnore]
        static private string key = "E546C8DF278CD5931069B522E695D4F2";


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

                        theData.Password = Encrypt.DecryptString(theData.Password, key);
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
                ClassSavedSettings c = (ClassSavedSettings) theData.MemberwiseClone();

                JsonSerializer serializer = new JsonSerializer();
                serializer.Formatting = Formatting.Indented;

                c.Password = Encrypt.EncryptString (theData.Password, key);
                serializer.Serialize(file, c);
            }
        }
    }
}
