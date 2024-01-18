using Rhino;
using Rhino.Commands;
using Rhino.DocObjects;
using Rhino.Geometry;
using Rhino.Input;
using Rhino.Input.Custom;
using SnitchCO2;
using SnitchCommon.Building;
using SnitchDB;
using SnitchIFC;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MasterSnitch
{
    public class MasterCommon : Command
    {
        public MasterCommon()
        {
            // Rhino only creates one instance of each command class defined in a
            // plug-in, so it is safe to store a refence in a static property.
            Instance = this;
        }

        ///<summary>The only instance of this command.</summary>
        public static MasterCommon Instance { get; private set; }

        ///<returns>The command name as it appears on the Rhino command line.</returns>
        public override string EnglishName => "MasterCommon";

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {//string filePath = "C:\\Users\\Nikop\\Downloads\\ifc_testing_model.ifc";
            string filePath = "C:\\Users\\Nikop\\Downloads\\testimalli2.ifc";
            Building building = FromIFCToSnich.ImportIFCLines(filePath);
            building.SetFloorNumbersForBuildingElements();
            building.ConnectColumnsAndBeams();


            building.CalculateBeamLoadBearingWidths();
            building.CalculateBeamLoads();
            building.CalculateColumnLoads();
            building.SetFloors();

            CO2Calculation CO2Calculations = new CO2Calculation(building);

            foreach (var beam in building.Beams.Values)
            {
                var attributes = new ObjectAttributes();
                Plane plane = doc.Views.ActiveView.ActiveViewport.ConstructionPlane();
                plane.Origin = beam.CenterPoint;

                attributes.Name = beam.Name;
                List<string> connectedColumns = beam.ConnectedColumns.Select(c => c.Name).ToList();
                List<string> connectedBeams = beam.ConnectedBeams.Select(b => b.Name).ToList();



                foreach (var property in beam.GetType().GetProperties())
                {
                    if (property.Name == "ConnectedBeams" || property.Name == "ConnectedColumns")
                    {
                        List<string> connectedLines = connectedColumns.Concat(connectedBeams).ToList();
                        attributes.SetUserString("ConnectedLines", string.Join(",", connectedLines));
                        continue;
                    }
                    attributes.SetUserString(property.Name, property.GetValue(beam) != null ? property.GetValue(beam).ToString() : "null");
                }

                doc.Objects.AddLine(beam.CenterLine, attributes);
            }

            foreach (var column in building.Columns.Values)
            {
                var attributes = new ObjectAttributes();
                Plane plane = doc.Views.ActiveView.ActiveViewport.ConstructionPlane();
                plane.Origin = column.CenterPoint;

                attributes.Name = column.Name;
                // attributes.SetUserString("ConnectedLines", string.Join(",", connectedLines));
                //doc.Objects.AddText(column.Name, plane, 500, "Ariel", false, false);

                foreach (var property in column.GetType().GetProperties())
                {
                    if (property.Name == "ConnectedBeams")
                    {
                        List<string> connectedLines = column.ConnectedBeams.Select(beam => beam.Name).ToList();
                        attributes.SetUserString("ConnectedLines", string.Join(",", connectedLines));
                        continue;
                    }
                    attributes.SetUserString(property.Name, property.GetValue(column) != null ? property.GetValue(column).ToString() : "null");
                }

                doc.Objects.AddLine(column.CenterLine, attributes);
            }

            doc.Views.Redraw();
            return Result.Success;
        }
    }
}
