using SnitchCommon.Elements.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnitchCommon.Elements
{
    public class Column : Element_base
    {
        //---------------------- CONSTRUCTORS ------------------------

        //------------------------- FIELDS ---------------------------

        //----------------------- PROPERTIES -------------------------

        public double LoadBearingArea { get; set; }
        public double NormalForce { get; set; }

        public double FloorLoadFactor { get; set; }

        public double Load { get; set; }

        public Column AboveColumn { get; set; }

        public List<Beam> ConnectedBeams { get; set; } = new List<Beam>();

        //------------------------ METHODS ---------------------------

        public void CalculateLoad()
        {
            this.Load = GetWeight();

            foreach (Beam beam in this.ConnectedBeams)
            {
                this.Load += beam.Load / beam.ConnectedColumns.Count();
            }
            if (this.AboveColumn != null)
            {
                this.Load += this.AboveColumn.Load;
            }
        }

        //------------------------ SETTERS ---------------------------

        //------------------------ GETTERS ---------------------------
    }
}
