using SnitchCommon.Elements.Base;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnitchCommon.Elements
{
    public class Beam : Element_base
    {

        //---------------------- CONSTRUCTORS ------------------------


        //------------------------- FIELDS ---------------------------

        //----------------------- PROPERTIES -------------------------

        public double LoadBearingWidth { get; set; }

        public double Load { get; set; }

        public List<Column> ConnectedColumns { get; set; } = new List<Column>();

        //------------------------ METHODS ---------------------------

        public double CalculateLoad(double liveLoad)
        {
            this.Load = GetWeight();
            this.Load += ((LoadBearingWidth / 1000) * (this.Length / 1000)) * liveLoad; // convert mm to m

            return Load;
        }

        //------------------------ SETTERS ---------------------------

        //------------------------ GETTERS ---------------------------
    }
}
