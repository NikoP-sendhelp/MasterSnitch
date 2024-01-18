using SnitchCommon.Elements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnitchCommon.Building
{
    public class Floor
    {

        public List<Beam> Beams { get; set; }
        public List<Column> Columns { get; set; }

        public double FloorLoad { get; set; }

        public int FloorNumber { get; set; }

        public Floor()
        {
            this.Beams = new List<Beam>();
            this.Columns = new List<Column>();
        }

        public void UpdateFloorLoad()
        {
            this.FloorLoad = 0;
            foreach (Beam beam in this.Beams)
            {
                this.FloorLoad += beam.Resultant_load;
            }

            foreach (Column column in this.Columns)
            {
                this.FloorLoad += column.Resultant_load;
            }
        }

        public void CalculateFloorLoad()
        {
            foreach (var beam in this.Beams)
            {
                this.FloorLoad += beam.Resultant_load;
            }

            foreach (var column in this.Columns)
            {
                this.FloorLoad += column.Resultant_load;
            }
        }

        //public void ConnectBeamsToColumns()
        //{
        //    foreach (Column column in this.Columns)
        //    {
        //        foreach (Beam beam in this.Beams)
        //        {
        //            if ((column.CenterLine.To.DistanceTo(beam.CenterLine.To) <= 0.1) || (column.CenterLine.To.DistanceTo(beam.CenterLine.From) <= 0.1))
        //            {
        //                column.ConnectedBeams.Add(beam);
        //                beam.ConnectedColumns.Add(column);
        //            }
        //        }
        //    }
        //}
    }
}
