using Rhino;
using Rhino.Commands;
using Rhino.DocObjects;
using Rhino.Geometry;
using Rhino.Input;
using Rhino.Input.Custom;
using SnitchCommon.Building;
using SnitchCommon.Elements;
using SnitchDatabase;
using SnitchIFC;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace MasterSnitchPlugin
{
    public class MasterSnitchCommand : Command
    {
        public MasterSnitchCommand()
        {
            // Rhino only creates one instance of each command class defined in a
            // plug-in, so it is safe to store a refence in a static property.
            Instance = this;
        }

        ///<summary>The only instance of this command.</summary>
        public static MasterSnitchCommand Instance { get; private set; }

        ///<returns>The command name as it appears on the Rhino command line.</returns>
        public override string EnglishName => "MasterSnitchCommand";

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            SnitchADO ADO = new SnitchADO();
            string filePath = "C:\\Users\\Nikop\\Downloads\\ifc_testing_model.ifc";
            Building building = FromIFCtoSnitch.ImportIFC(filePath);
            building.SetFloorNumbersForBuildingElements();
            building.ConnectColumnsAndBeams();

            var attributes = new ObjectAttributes();


            building.CalculateBeamLoadBearingWidths();
            building.CalculateBeamLoads();
            building.CalculateColumnLoads();
            building.SetFloors();

            foreach (var beam in building.Beams.Values)
            {
                ADO.AddBeam(beam);
                Plane plane = doc.Views.ActiveView.ActiveViewport.ConstructionPlane();
                plane.Origin = beam.CenterPoint;
                doc.Objects.AddText(beam.Name, plane, 500, "Ariel", false, false);

                if (beam.FloorNo == 1)
                {
                    attributes.ObjectColor = Color.Red;
                    attributes.ColorSource = ObjectColorSource.ColorFromObject;
                }

                if (beam.FloorNo == 2)
                {
                    attributes.ObjectColor = Color.Green;
                    attributes.ColorSource = ObjectColorSource.ColorFromObject;
                }
                doc.Objects.AddLine(beam.CenterLine, attributes);

                string listOfColumns = string.Join(", ", beam.ConnectedColumns.Select(c => c.Name));
                RhinoApp.WriteLine($"{beam.Name} is connected to {listOfColumns}");
                RhinoApp.WriteLine($"{beam.Name} is at floor number: {beam.FloorNo}");
                RhinoApp.WriteLine($"{beam.Name} weight is: {beam.Weight}");
                RhinoApp.WriteLine($"{beam.Name} load is: {beam.Load} and loadbearingwidth is: {beam.LoadBearingWidth}");
                RhinoApp.WriteLine($"{beam.Name} Volume_concrete_m3 is: {beam.Volume_concrete_m3} and Mass_steel_m3 is: {beam.Mass_steel_m3}");
                RhinoApp.WriteLine("");
            }
            RhinoApp.WriteLine("");
            RhinoApp.WriteLine("");

            foreach (var column in building.Columns.Values)
            {
                ADO.AddColumn(column);
                // Plane plane = new Plane(column.CenterLine.PointAt(0.5), column.CenterLine.Direction);
                Plane plane = doc.Views.ActiveView.ActiveViewport.ConstructionPlane();
                plane.Origin = column.CenterPoint;
                doc.Objects.AddText(column.Name, plane, 500, "Ariel", false, false);

                if (column.FloorNo == 1)
                {
                    attributes.ObjectColor = Color.Red;
                    attributes.ColorSource = ObjectColorSource.ColorFromObject;
                }

                if (column.FloorNo == 2)
                {
                    attributes.ObjectColor = Color.Green;
                    attributes.ColorSource = ObjectColorSource.ColorFromObject;
                }
                doc.Objects.AddLine(column.CenterLine, attributes);
                if (column.AboveColumn != null)
                {
                    RhinoApp.WriteLine($"{column.Name} is below {column.AboveColumn.Name}");
                }
                RhinoApp.WriteLine($"{column.Name} is at floor number: {column.FloorNo}");
                RhinoApp.WriteLine($"{column.Name} load is: {column.Load}");
                RhinoApp.WriteLine($"{column.Name} mass is: {column.Weight}");
                RhinoApp.WriteLine($"{column.Name} Volume_concrete_m3 is: {column.Volume_concrete_m3} and Mass_steel_m3 is: {column.Mass_steel_m3}");
                RhinoApp.WriteLine("");
            }

            ADO.GetAllBeams();
            ADO.GetAllColumns();
            doc.Views.Redraw();
            // ---
            return Result.Success;
        }
    }
}
