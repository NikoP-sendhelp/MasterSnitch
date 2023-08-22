using Newtonsoft.Json;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnitchCommon.Elements.Base
{
    public abstract class Element_base
    {

        //---------------------- CONSTRUCTORS ------------------------
        [JsonConstructor]
        public Element_base(bool dummy = false)
        {
        }
        public Element_base()
        {
            AssignProperties();
        }

        //----------------------- PROPERTIES -------------------------

        public Guid Guid { get; set; }
        public Line CenterLine { get; set; }
        public Point3d CenterPoint { get; set; }

        public string Name { get; set; }
        public double Volume_concrete_m3 { get; set; }
        public double Mass_steel_m3 { get; set; }
        public double Height { get; set; }
        public double Length { get; set; }
        public double Weight { get; set; }
        public double Mass { get; set; }
        public double Width { get; set; }
        public double Volume { get; set; }
        public double Gross_footprint_area { get; set; }
        public double Area_per_tons { get; set; }
        public double Net_surface_area { get; set; }
        public string Bottom_elevation { get; set; }
        public string Top_elevation { get; set; }

        public int FloorNo { get; set; }
        public double g { get; set; }

        //------------------------ METHODS ---------------------------
        private void AssignProperties()
        {
            this.Guid = Guid.NewGuid();
            this.g = 9.81;
        }

        //------------------------ SETTERS ---------------------------

        private void SetWeight()
        {
            this.Weight = this.Mass * this.g;
        }

        public void SetName(string Name)
        {
            this.Name = Name;
        }

        //------------------------ GETTERS ---------------------------

        public double GetWeight()
        {
            if (this.Weight != 0)
            {
                return this.Weight;
            }
            SetWeight();
            return this.Weight;
        }
    }
}
