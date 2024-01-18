using Rhino;
using Rhino.Commands;
using Rhino.DocObjects;
using Rhino.Geometry;
using Rhino.Input;
using Rhino.Input.Custom;
using SnitchDB;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;

namespace SnitchTool
{
    public class SnitchToolCommand : Command
    {
        public SnitchToolCommand()
        {
            // Rhino only creates one instance of each command class defined in a
            // plug-in, so it is safe to store a refence in a static property.
            Instance = this;
        }

        ///<summary>The only instance of this command.</summary>
        public static SnitchToolCommand Instance { get; private set; }

        ///<returns>The command name as it appears on the Rhino command line.</returns>
        public override string EnglishName => "SnitchToolCommand";

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            SnitchADO ADO = new SnitchADO();
            string pathName = RhinoDoc.ActiveDoc.Path;
            string projectName = Path.GetFileNameWithoutExtension(pathName);

            foreach (RhinoObject rhinoObject in doc.Objects)
            {
                string rhinoObjectName = rhinoObject.Attributes.Name;
                double rhinoObjectCO2 = Double.Parse(rhinoObject.Attributes.GetUserString("CO2_Value"));
                string rhinoObjectProject = rhinoObject.Attributes.GetUserString("Project_name");
                double rhinoObjectLengthLoad = Double.Parse(rhinoObject.Attributes.GetUserString("LengthLoad"));

                List<Dictionary<string, string>> searchResult = new List<Dictionary<string, string>>();
                if (rhinoObjectName.Contains("Beam"))
                {
                    searchResult = ADO.GetBeamsWithLengthLoad(rhinoObjectLengthLoad, rhinoObjectProject, 5);
                }

                if (rhinoObjectName.Contains("Column"))
                {
                    searchResult = ADO.GetColumnsWithLengthLoad(rhinoObjectLengthLoad, rhinoObjectProject, 5);
                }

                if (int.Parse(searchResult[0]["AffectedRows"]) == 0)
                {
                    RhinoApp.WriteLine("No results were found for column to compare");
                    doc.Views.Redraw();
                    // ---
                    return Result.Failure;
                }

                List<double> resultElementsCO2Values = new List<double>();
                foreach (Dictionary<string, string> element in searchResult)
                {
                    if (element.ContainsKey("CO2"))
                    {
                        resultElementsCO2Values.Add(Double.Parse(element["CO2"]));
                    }
                }


                double resultAVGCO2Value = resultElementsCO2Values.Average();

                double comparisonValue = rhinoObjectCO2 / resultAVGCO2Value;

                int blueColor = 0;
                int redColor = 0;

                if (resultAVGCO2Value != 0 && rhinoObjectCO2 != 0)
                {
                    RhinoApp.WriteLine($"AVG: {resultAVGCO2Value} and element CO2: {rhinoObjectCO2}");
                }

                if (comparisonValue > 2)
                {
                    RhinoApp.WriteLine($"Element {rhinoObjectName} has significantly exceeded the CO2 value. Element CO2 value: {rhinoObjectCO2} and average CO2 value: {resultAVGCO2Value}");
                    comparisonValue = 2;
                }
                if ( comparisonValue > 1)
                {
                    redColor = (int)Math.Round(255 * (comparisonValue - 1));
                } else if (comparisonValue < 1)
                {
                    blueColor =(int)Math.Round(255 * (1 - comparisonValue));
                }



                rhinoObject.Attributes.ObjectColor = Color.FromArgb(255, redColor, 0, blueColor);
                rhinoObject.Attributes.ColorSource = ObjectColorSource.ColorFromObject;
                rhinoObject.CommitChanges();

            }

            doc.Views.Redraw();
            // ---
            return Result.Success;
        }
    }
}
