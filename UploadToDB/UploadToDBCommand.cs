using Rhino;
using Rhino.Commands;
using Rhino.DocObjects;
using Rhino.Geometry;
using Rhino.Input;
using Rhino.Input.Custom;
using SnitchDB;
using System;
using System.Collections.Generic;
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

            int beamAffectedRows = 0;
            int columnAffectedRows = 0;

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
                    Dictionary<string, string> columnResult = ADO.SetColumn(elementDictionary)[0];
                    columnAffectedRows += int.Parse(columnResult["AffectedRows"]); // Get Affected row info
                }

                if (elementDictionary["Name"].Contains("Beam"))
                {
                    Dictionary<string, string> beamResult = ADO.SetBeam(elementDictionary)[0];
                    beamAffectedRows += int.Parse(beamResult["AffectedRows"]); // Get Affected row info

                }
            }

            RhinoApp.WriteLine($"Query changed {beamAffectedRows} beam rows and {columnAffectedRows} column rows");

            return Result.Success;
        }
    }
}
