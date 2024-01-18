using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Rhino.Geometry;

namespace SnitchCommon.Elements
{
    public abstract class ElementBase
    {
        //---------------------- CONSTRUCTORS ------------------------
        public ElementBase()
        {
            AssignProperties();
        }

        //----------------------- PROPERTIES -------------------------

        public Guid Guid { get; set; }
        public Line CenterLine { get; set; }
        public Point3d CenterPoint { get; set; }

        public string Name { get; set; }
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
        public double Resultant_load { get; set; }
        public string Material_name { get; set; }

        public double LengthLoad { get; set; }

        public string Project_name { get; set; }

        public double CO2_Value { get; set; }

        public int FloorNo { get; set; }
        public double g { get; set; }

        //------------------------ METHODS ---------------------------
        private void AssignProperties()
        {
            this.Guid = Guid.NewGuid();
            this.g = 9.81;
            this.CO2_Value = 0;
        }

        //------------------------ SETTERS ---------------------------

        private void SetWeight()
        {
            this.Weight = 1.15 * this.Mass * this.g; // ULS condition
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

        public string GetMaterialCode()
        {
            int index = this.Material_name.IndexOf('/') + 1;
            return this.Material_name.Substring(index);
        }
    }
}
