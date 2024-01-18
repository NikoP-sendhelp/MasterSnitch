using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnitchCommon.Elements
{
    public class Column : ElementBase
    {

        //---------------------- CONSTRUCTORS ------------------------

        //------------------------- FIELDS ---------------------------

        //----------------------- PROPERTIES -------------------------

        public Column AboveColumn { get; set; }

        public List<Beam> ConnectedBeams { get; set; } = new List<Beam>();

        public double SteelWeight { get; set; }
        public double ConcreteWeight { get; set; }

        public double SteelMass { get; set; }

        public Column ColumnPart { get; set; }

        public string Second_material_name { get; set; } = "null";

        //------------------------ METHODS ---------------------------

        public void CalculateResultantLoad()
        {
            this.Resultant_load = GetWeight(); // ULS condition set in base

            if (this.ConnectedBeams != null)
            {
                foreach (Beam beam in this.ConnectedBeams)
                {
                    this.Resultant_load += beam.Resultant_load / beam.ConnectedColumns.Count();
                }
            }

            if (this.AboveColumn != null)
            {
                this.Resultant_load += this.AboveColumn.Resultant_load;
            }

            if (SteelWeight != 0)
            {
                this.Resultant_load += this.SteelWeight * this.g;
            }

            this.Resultant_load = 1.15 * this.Resultant_load; // ULS condition
            this.SetLengthLoad();
        }

        //------------------------ SETTERS ---------------------------

        public void SetColumnPart(Column column)
        {
            this.ColumnPart = column;
            this.Second_material_name = column.Material_name;
            this.Weight += column.GetWeight();
            if (column.Height > this.Height)
            {
                this.Height = column.Height;
            }
            if (column.Length > this.Length)
            {
                this.Length = column.Length;
                this.CenterLine = column.CenterLine;
            }
            if (column.Width > this.Width)
            {
                this.Width = column.Width;
            }
        }

        private void SetSteelAndConcreteWeight()
        {
            if (this.Name.StartsWith("CONCRETE"))
            {
                this.ConcreteWeight = this.Weight;

                if (this.ColumnPart != null && this.ColumnPart.Name.StartsWith("STEEL"))
                {
                    this.SteelWeight = this.ColumnPart.GetWeight();
                }
            }
        }

        private void SetLengthLoad()
        {
            this.LengthLoad = this.Resultant_load / this.Length;
        }

        //------------------------ GETTERS ---------------------------

        // Helper not used!
        public double GetSteelMass()
        {
            return 0;
        }

        public string GetSecondMaterialCode()
        {
            if (this.Second_material_name != null)
            {
                int index = this.Second_material_name.IndexOf('/') + 1;
                return this.Second_material_name.Substring(index);
            }

            return null;
        }
    }
}
