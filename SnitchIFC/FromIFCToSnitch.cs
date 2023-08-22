using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xbim.Ifc;
using Xbim.Ifc4.Interfaces;
using Xbim.Ifc2x3.GeometryResource;
using Xbim.Ifc.Extensions;
using Xbim.Ifc2x3.SharedBldgElements;
using Xbim.Ifc2x3.GeometricModelResource;
using Xbim.Ifc2x3.GeometricConstraintResource;
using Rhino.Geometry;
using SnitchCommon.Building;
using SnitchCommon.Elements;
using System.Xml.Linq;
using System.Reflection;
using Rhino;
using Xbim.Common.Geometry;

namespace SnitchIFC
{
    public static class FromIFCtoSnitch
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

        public static Building ImportIFC(string filePath)
        {

            Building building = new Building();
            StringBuilder sb = new StringBuilder();



            using (var model = IfcStore.Open(filePath))
            {
                // Check if project exists
                string projectName = Path.GetFileNameWithoutExtension(filePath);

                // Create list of all elements (Empty for now)
                var allElements = new Dictionary<string, List<IIfcElement>>();

                allElements["Beams"] = model.Instances.OfType<IIfcBeam>().Cast<IIfcElement>().ToList();
                allElements["Columns"] = model.Instances.OfType<IIfcColumn>().Cast<IIfcElement>().ToList();
                allElements["Slabs"] = model.Instances.OfType<IIfcSlab>().Cast<IIfcElement>().ToList();
                allElements["Walls"] = model.Instances.OfType<IIfcWall>().Cast<IIfcElement>().ToList();

                var test2 = model.Instances.OfType<IIfcElement>().ToList();
                int c = 0;
                int b = 0;
                foreach (IIfcElement ifcElement in test2)
                {
                    IfcLocalPlacement temp = (IfcLocalPlacement)ifcElement.ObjectPlacement;
                    IfcAxis2Placement3D placement = (IfcAxis2Placement3D)temp.RelativePlacement;

                    switch (ifcElement)
                    {
                        case IfcBeam _:
                            b++;
                            Beam beam = new Beam();
                            beam.SetName($"Beam {b}");
                            setElementProperties(ifcElement, beam);
                            (Point3d bPt1, Point3d bPt2) = BeamCenterLineStartAndEndPoints(placement, beam.Length);
                            beam.CenterLine = new Line(bPt1, bPt2);
                            beam.CenterPoint = beam.CenterLine.PointAt(0.5);
                            building.Beams.Add(beam.Guid, beam);
                            break;

                        case IfcColumn _:
                            c++;
                            Column column = new Column();
                            column.SetName($"Column {c}");
                            setElementProperties(ifcElement, column);
                            (Point3d cPt1, Point3d cPt2) = ColumnCenterLineStartAndEndPoints(placement, column.Length);
                            column.CenterLine = new Line(cPt1, cPt2);
                            column.CenterPoint = column.CenterLine.PointAt(0.5);
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


        private static void setElementProperties(IIfcElement ifcElement, object element)
        {
            var properties = ifcElement.IsDefinedBy
                            .Where(r => r.RelatingPropertyDefinition is IIfcPropertySet)
                            .SelectMany(r => ((IIfcPropertySet)r.RelatingPropertyDefinition).HasProperties)
                            .OfType<IIfcPropertySingleValue>();

            foreach (var property in properties)
            {
                if (wantedProperties.Contains(property.Name))
                {
                    string propertyName = property.Name.ToString().Replace(" ", "_");
                    if (propertyName == "Weight")
                    {
                        propertyName = "Mass";
                    }
                    PropertyInfo propertyInfo = element.GetType().GetProperty(propertyName);
                    propertyInfo?.SetValue(element, property.NominalValue.Value);
                }
            }
        }
    }
}
