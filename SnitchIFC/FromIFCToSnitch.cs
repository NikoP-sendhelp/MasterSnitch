using Rhino.Geometry;
using System.Reflection;
using System.Text;
using Xbim.Ifc;
using SnitchCommon.Elements;
using SnitchCommon.Building;
using Xbim.Ifc2x3.GeometryResource;
using Xbim.Ifc2x3.SharedBldgElements;
using Xbim.Ifc2x3.GeometricConstraintResource;
using Xbim.Ifc2x3.Interfaces;
using Xbim.Ifc2x3.Kernel;
using Xbim.Ifc2x3.PropertyResource;
using System.Linq;
using System.IO;
using System.Collections.Generic;

namespace SnitchIFC
{
    public class FromIFCToSnich
    {

        // List of variables looked for in the inc model
        // Units unclear!
        private static List<string> wantedProperties = new List<string>
            {
                "Weight",
                "Height",
                "Length",
                "Width",
                "Volume",
                "Gross footprint area",
                "Area per tons",
                "Net surface area",
                "Bottom elevation",
            "Top elevation"
        };

        public static int iteration = 0;

        public static Building ImportIFCLines(string filePath)
        {
            iteration = 0;

            Building building = new Building();
            StringBuilder sb = new StringBuilder();


            using (var model = IfcStore.Open(filePath))
            {
                // Get project name
                string projectName = Path.GetFileNameWithoutExtension(filePath);

                var allElements = model.Instances.OfType<IIfcElement>().ToList();
                int c = 0;
                int b = 0;
                foreach (IIfcElement ifcElement in allElements)
                {
                    IfcLocalPlacement temp = (IfcLocalPlacement)ifcElement.ObjectPlacement;
                    IfcAxis2Placement3D placement = (IfcAxis2Placement3D)temp.RelativePlacement;

                    switch (ifcElement)
                    {
                        case IfcBeam _:
                            b++;
                            Beam beam = new Beam();
                            beam.SetName($"Beam {b}");
                            SetElementProperties(ifcElement, beam);
                            (Point3d bPt1, Point3d bPt2) = BeamCenterLineStartAndEndPoints(placement, beam.Length);
                            beam.CenterLine = new Line(bPt1, bPt2);
                            beam.CenterPoint = beam.CenterLine.PointAt(0.5);
                            beam.Project_name = projectName;
                            building.Beams.Add(beam.Guid, beam);
                            break;

                        case IfcColumn _:
                            c++;
                            Column column = new Column();
                            column.SetName($"Column {c}");
                            SetElementProperties(ifcElement, column);
                            (Point3d cPt1, Point3d cPt2) = ColumnCenterLineStartAndEndPoints(placement, column.Length);
                            column.CenterLine = new Line(cPt1, cPt2);
                            column.CenterPoint = column.CenterLine.PointAt(0.5);
                            column.Project_name = projectName;
                            building.Columns.Add(column.Guid, column);
                            break;
                        /*
                        case IfcSlab _:
                            Slab slab = new Slab();
                            setElementProperties(ifcElement, slab);
                            break;
                        */
                        default:
                            break;
                    }
                }
            }
            return building;
        }


        private static Point3d CartesianPointToPoint3d(IfcCartesianPoint location)
        {
            return new Point3d()
            {
                X = location.X,
                Y = location.Y,
                Z = location.Z
            };
        }

        private static (Point3d, Point3d) BeamCenterLineStartAndEndPoints(IfcAxis2Placement3D placement, double length)
        {
            Point3d startPoint = CartesianPointToPoint3d(placement.Location);

            Point3d endPoint = new Point3d()
            {
                X = placement.Location.X + placement.RefDirection.X * length,
                Y = placement.Location.Y + placement.RefDirection.Y * length,
                Z = placement.Location.Z + placement.RefDirection.Z * length
            };

            return (startPoint, endPoint);
        }

        private static (Point3d, Point3d) ColumnCenterLineStartAndEndPoints(IfcAxis2Placement3D placement, double length)
        {

            Point3d startPoint = CartesianPointToPoint3d(placement.Location);

            Point3d endPoint = new Point3d()
            {
                X = placement.Location.X + placement.Axis.X * length,
                Y = placement.Location.Y + placement.Axis.Y * length,
                Z = placement.Location.Z + placement.Axis.Z * length
            };

            return (startPoint, endPoint);
        }


        private static void SetElementProperties(IIfcElement ifcElement, object element)
        {

            var properties = ifcElement.IsDefinedBy
                            .OfType<IfcRelDefinesByProperties>()
                            .SelectMany(r => r.RelatingPropertyDefinition.PropertySetDefinitions)
                            .OfType<IfcPropertySet>()
                            .SelectMany(p => p.HasProperties);

            IIfcMaterial material = ifcElement.HasAssociations
                            .OfType<IIfcRelAssociatesMaterial>()
                            .Select(r => ((IIfcMaterial)r.RelatingMaterial)).First();

            PropertyInfo materialInfo = element.GetType().GetProperty("Material_name");
            materialInfo?.SetValue(element, material.Name.ToString());

            foreach (var property in properties)
            {
                if (wantedProperties.Contains(property.Name))
                {
                    var singleValue = (IfcPropertySingleValue)property;
                    string singleValueName = singleValue.Name.ToString().Replace(" ", "_");
                    if (singleValueName == "Weight")
                    {
                        singleValueName = "Mass";
                    }
                    PropertyInfo propertyInfo = element.GetType().GetProperty(singleValueName);
                    propertyInfo?.SetValue(element, singleValue.NominalValue.Value);
                }
            }

            if (ifcElement is IfcColumn)
            {
                iteration = 1;
            }
        }
    }
}
