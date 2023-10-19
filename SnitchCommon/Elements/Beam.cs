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

        public List<Column> ConnectedColumns { get; set; } = new List<Column>();
        public List<Beam> ConnectedBeams { get; set; } = new List<Beam>();

        public double LengthLoad {  get; set; }
        public double Live_load { get; set; }

        //------------------------ METHODS ---------------------------

        public double CalculateResultantLoad(double liveLoad)
        {
            this.Resultant_load = GetWeight();
            this.Resultant_load += ((LoadBearingWidth / 1000) * (this.Length / 1000)) * liveLoad; // convert mm to m
            this.Live_load = ((LoadBearingWidth / 1000) * (this.Length / 1000)) * liveLoad; // convert mm to m
            this.SetLengthLoad();
            return Resultant_load;
        }


        //------------------------ SETTERS ---------------------------
        private void SetLengthLoad()
        {
            this.LengthLoad = this.Resultant_load / this.Length;
        }

        //------------------------ GETTERS ---------------------------
    }
}
