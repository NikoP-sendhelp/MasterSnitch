using Rhino;
using Rhino.Commands;
using Rhino.DocObjects;
using Rhino.Geometry;
using Rhino.Input;
using Rhino.Input.Custom;
using Rhino.UI.Controls;
using SnitchDatabase;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;

namespace MasterSnitchTool
{
    public class MasterSnitchTool : Command
    {
        public MasterSnitchTool()
        {
            // Rhino only creates one instance of each command class defined in a
            // plug-in, so it is safe to store a refence in a static property.
            Instance = this;
        }

        ///<summary>The only instance of this command.</summary>
        public static MasterSnitchTool Instance { get; private set; }

        ///<returns>The command name as it appears on the Rhino command line.</returns>
        public override string EnglishName => "MasterSnitchTool";

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            SnitchADO ADO = new SnitchADO();

            foreach (RhinoObject rhinoObject in doc.Objects)
            {
                string rhinoObjectName = rhinoObject.Attributes.Name;
                double rhinoObjectCO2 = Double.Parse(rhinoObject.Attributes.GetUserString("CO2_Value"));

                List<object> searchResult = new List<object>();
                if (rhinoObjectName.Contains("Beam"))
                {
                    double rhinoObjectForces = Double.Parse(rhinoObject.Attributes.GetUserString("LengthLoad"));
                    searchResult = ADO.GetBeamWithLoad(rhinoObjectForces);
                }

                if (rhinoObjectName.Contains("Column"))
                {
                    double rhinoObjectForces = Double.Parse(rhinoObject.Attributes.GetUserString("Resultant_load"));
                    searchResult = ADO.GetColumnWithLoad(rhinoObjectForces, 5);
                }

                if (searchResult != null && searchResult.Count != 0)
                {
                    List<double> test2 = new List<double>();
                    foreach (List<object> test3 in searchResult)
                    {
                        test2.Add((double)test3[8]);
                    }

                    double avg = test2.Average();

                    foreach (List<object> result in searchResult)
                    {
                        double resultCO2Value = (double)result[8];

                        if (rhinoObjectCO2 > 1.05 * resultCO2Value || rhinoObjectCO2 < 0.95 * resultCO2Value)
                        {
                            rhinoObject.Attributes.ObjectColor = Color.Red;
                            rhinoObject.Attributes.ColorSource = ObjectColorSource.ColorFromObject;
                            rhinoObject.CommitChanges();
                        }
                    }
                }
                else
                {
                    rhinoObject.Attributes.ObjectColor = Color.Orange;
                    rhinoObject.Attributes.ColorSource = ObjectColorSource.ColorFromObject;
                    rhinoObject.CommitChanges();
                }

            }

            doc.Views.Redraw();
            // ---
            return Result.Success;
        }
    }
}
