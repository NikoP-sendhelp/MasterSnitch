using Rhino.Geometry.Intersect;
using Rhino.Geometry;
using SnitchCommon.Elements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnitchCommon.Building
{
    public class Building
    {

        //---------------------- CONSTRUCTORS ------------------------
        public Building() => AssignProperties();

        //----------------------- PROPERTIES -------------------------

        public double DistributedLoad_live { get; set; }

        public double MOE { get; set; }

        public Dictionary<int, Floor> Floors { get; set; }

        public Dictionary<Guid, Beam> Beams { get; set; }
        public Dictionary<Guid, Column> Columns { get; set; }


        //------------------------ METHODS ---------------------------

        private void AssignProperties()
        {
            this.Beams = new Dictionary<Guid, Beam>();
            this.Columns = new Dictionary<Guid, Column>();
            this.Floors = new Dictionary<int, Floor>();

            this.DistributedLoad_live = 4000; // N/m2

            this.MOE = 200; // mm
        }

        public void CalculateBeamLoads()
        {
            foreach (Beam beam in this.Beams.Values)
            {
                beam.Resultant_load = beam.CalculateResultantLoad(this.DistributedLoad_live);
                if (beam.ConnectedBeams != null && beam.ConnectedColumns != null)
                {
                    foreach (Beam connectedBeam in beam.ConnectedBeams)
                    {
                        beam.Resultant_load += connectedBeam.GetWeight() / 2;
                    }
                }
            }
        }

        public void CalculateColumnLoads()
        {
            var sortedColumns = this.Columns.OrderByDescending(c => c.Value.FloorNo).Select(c => c.Value);

            foreach (Column column in sortedColumns)
            {
                column.CalculateResultantLoad();
            }
        }

        public bool CheckIfSteel(string ClassName)
        {
            return ClassName.IndexOf("steel", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        public void ConnectColumnsAndBeams()
        {
            List<Column> columns = this.Columns.Values.ToList();
            foreach (Column column in columns)
            {
                Point3d startPoint = column.CenterLine.To;
                foreach (Column column2 in columns)
                {
                    if (column == column2) { continue; }

                    Point3d endPoint = column2.CenterLine.From;
                    if (CheckIfSteel(column2.Material_name))
                    {
                        if (startPoint.DistanceTo(column2.CenterLine.To) <= this.MOE && column.CenterLine.From.DistanceTo(endPoint) <= this.MOE)
                        {
                            column.SetColumnPart(column2);
                            this.Columns.Remove(column2.Guid);
                            continue;
                        }
                    }

                    if (startPoint.DistanceTo(endPoint) <= this.MOE)
                    {
                        column.AboveColumn = column2;
                    }
                }

                foreach (Beam beam in this.Beams.Values)
                {
                    Point3d beamPoint1 = beam.CenterLine.To;
                    Point3d beamPoint2 = beam.CenterLine.From;
                    if ((startPoint.DistanceTo(beamPoint1) <= this.MOE) || (startPoint.DistanceTo(beamPoint2) <= this.MOE))
                    {
                        column.ConnectedBeams.Add(beam);
                        beam.ConnectedColumns.Add(column);
                    }
                }
            }

            // Connect Beams
            foreach (Beam beam in this.Beams.Values)
            {
                Line beamLine = beam.CenterLine;
                foreach (Beam beam2 in this.Beams.Values)
                {
                    Line beam2Line = beam2.CenterLine;
                    if (beam == beam2) { continue; }
                    if (Intersection.LineLine(beamLine, beam2Line, out _, out _, this.MOE, true))
                    {
                        beam.ConnectedBeams.Add(beam2);
                    }
                }
            }
        }

        public void CalculateBeamLoadBearingWidths()
        {
            Dictionary<int, List<Beam>> floorBeams = new Dictionary<int, List<Beam>>();
            foreach (var beam in Beams.Values)
            {
                if (floorBeams.ContainsKey(beam.FloorNo))
                    floorBeams[beam.FloorNo].Add(beam);
                else
                    floorBeams.Add(beam.FloorNo, new List<Beam>() { beam });
            }

            foreach (var beamsByFloor in floorBeams)
            {
                CalculateBeamLoadBearingWidthsByFloor(beamsByFloor.Value);
            }
        }

        private void CalculateBeamLoadBearingWidthsByFloor(List<Beam> beams)
        {
            List<Line> centerLines = beams.Select(b => b.CenterLine).ToList();
            foreach (var beam in beams)
            {
                Line centerLine = beam.CenterLine;

                Point3d centerPt = (centerLine.From + centerLine.To) / 2;
                Vector3d v1 = Vector3d.CrossProduct(centerLine.Direction, Vector3d.ZAxis);
                Vector3d v2 = Vector3d.CrossProduct(centerLine.Direction, -Vector3d.ZAxis);

                Line l1 = new Line(centerPt, v1 * 5);
                //Rhino.RhinoDoc.ActiveDoc.Objects.AddLine(l1);
                double minDistance1 = CalculateMinDistance(centerLines, centerPt, l1);

                Line l2 = new Line(centerPt, v2 * 5);
                //Rhino.RhinoDoc.ActiveDoc.Objects.AddLine(l2);
                double minDistance2 = CalculateMinDistance(centerLines, centerPt, l2);

                beam.LoadBearingWidth = minDistance1 / 2 + minDistance2 / 2;
            }
        }

        private static double CalculateMinDistance(List<Line> centerLines, Point3d centerPt, Line l1)
        {
            double minDistance = double.MaxValue;
            foreach (var line in centerLines)
            {
                if (!Intersection.LineLine(l1, line, out double a, out double b, 100, false))
                    continue;
                if (a <= 0 || a > 1 || b < 0 || b > 1)
                    continue;
                Point3d intersectionPt = l1.PointAt(a);
                double distance = (centerPt - intersectionPt).Length;
                if (minDistance > distance)
                    minDistance = distance;
            }
            if (minDistance == double.MaxValue)
                minDistance = 0;
            return minDistance;
        }

        //------------------------ SETTERS ---------------------------
        public void SetFloorNumbersForBuildingElements()
        {
            var sortedBeams = this.Beams.OrderBy(b => b.Value.CenterPoint.Z).Select(b => b.Value);
            var sortedColumns = this.Columns.OrderBy(c => c.Value.CenterLine.To.Z).Select(c => c.Value);
            int FloorNumber = 1;

            foreach (Column column in sortedColumns)
            {
                if (column.FloorNo != 0) { continue; }
                double columnZLevel = column.CenterLine.To.Z;

                foreach (Column column2 in sortedColumns)
                {
                    if (column == column2) { continue; }
                    double column2ZLevel = column2.CenterLine.To.Z;
                    if (Math.Abs(columnZLevel - column2ZLevel) <= this.MOE)
                    {
                        column.FloorNo = column2.FloorNo = FloorNumber;
                    }
                }

                foreach (Beam beam in sortedBeams)
                {
                    double beamZLevel = beam.CenterPoint.Z;
                    if (Math.Abs(columnZLevel - beamZLevel) <= this.MOE)
                    {
                        beam.FloorNo = column.FloorNo;
                    }
                }
                FloorNumber++;
            }
        }
        public void SetFloors()
        {
            foreach (Beam beam in this.Beams.Values)
            {
                if (!this.Floors.ContainsKey(beam.FloorNo))
                {
                    Floor newFloor = new Floor();
                    newFloor.Beams.Add(beam);
                    newFloor.FloorNumber = beam.FloorNo;
                    this.Floors.Add(beam.FloorNo, newFloor);
                    continue;
                }

                this.Floors[beam.FloorNo].Beams.Add(beam);
            }

            foreach (Column column in this.Columns.Values)
            {
                if (!this.Floors.ContainsKey(column.FloorNo))
                {
                    Floor newFloor = new Floor();
                    newFloor.Columns.Add(column);
                    newFloor.FloorNumber = column.FloorNo;
                    this.Floors.Add(column.FloorNo, newFloor);
                    continue;
                }

                this.Floors[column.FloorNo].Columns.Add(column);
            }

            foreach (Floor floor in this.Floors.Values)
            {
                floor.UpdateFloorLoad();
            }
        }
        //------------------------ GETTERS ---------------------------
    }
}
