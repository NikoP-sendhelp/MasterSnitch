using Rhino;
using Rhino.Commands;
using Rhino.DocObjects;
using Rhino.DocObjects.Custom;
using Rhino.Geometry;
using Rhino.Input;
using Rhino.Input.Custom;
using SnitchDatabase;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace UploadToDB
{
    public class UploadToDBCommand : Command
    {
        public UploadToDBCommand()
        {
            // Rhino only creates one instance of each command class defined in a
            // plug-in, so it is safe to store a refence in a static property.
            Instance = this;
        }

        ///<summary>The only instance of this command.</summary>
        public static UploadToDBCommand Instance { get; private set; }

        ///<returns>The command name as it appears on the Rhino command line.</returns>
        public override string EnglishName => "UploadToDBCommand";

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            SnitchADO ADO = new SnitchADO();


            foreach (RhinoObject rhinoObject in doc.Objects)
            {
                NameValueCollection userData = rhinoObject.Attributes.GetUserStrings();
                Dictionary<string, string> elementDictionary = new Dictionary<string, string>();

                foreach (string key in userData)
                {
                    elementDictionary[key] = userData[key];
                }

                if (elementDictionary["Name"].Contains("Column"))
                {
                    ADO.AddOrUpdateColumn(elementDictionary);
                }
                if (elementDictionary["Name"].Contains("Beam"))
                {
                    ADO.AddOrUpdateBeam(elementDictionary);
                }
            }

            RhinoApp.WriteLine("Upload or update successful!");
            return Result.Success;
        }
    }
}
