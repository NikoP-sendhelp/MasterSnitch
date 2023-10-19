using Rhino;
using Rhino.Commands;
using Rhino.DocObjects;
using Rhino.Geometry;
using SnitchCommon.Building;
using SnitchCommon.Elements;
using SnitchDatabase;
using SnitchIFC;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using CO2Common;
using System.Data.Common;

namespace MasterSnitchPlugin
{
    public class MasterSnitchCommand : Command
    {
        public MasterSnitchCommand()
        {
            // Rhino only creates one instance of each command class defined in a
            // plug-in, so it is safe to store a reference in a static property.
            Instance = this;
        }

        ///<summary>The only instance of this command.</summary>
        public static MasterSnitchCommand Instance { get; private set; }

        ///<returns>The command name as it appears on the Rhino command line.</returns>
        public override string EnglishName => "MasterSnitchCommand";

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {

            //string filePath = "C:\\Users\\Nikop\\Downloads\\ifc_testing_model.ifc";
            string filePath = "C:\\Users\\Nikop\\Downloads\\testimalli2.ifc";
            Building building = FromIFCtoSnitch.ImportIFCLines(filePath);
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
                    attributes.SetUserString(property.Name, property.GetValue(column) != null ? property.GetValue(column).ToString():"null");
                }

                doc.Objects.AddLine(column.CenterLine, attributes);
            }

            doc.Views.Redraw();
            // ---
            return Result.Success;
        }
    }
}
