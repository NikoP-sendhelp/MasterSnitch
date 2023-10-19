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
            this.Resultant_load = GetWeight();

            foreach (Beam beam in this.ConnectedBeams)
            {
                this.Resultant_load += beam.Resultant_load / beam.ConnectedColumns.Count();
            }
            if (this.AboveColumn != null)
            {
                this.Resultant_load += this.AboveColumn.Resultant_load;
            }

            if (SteelWeight != 0)
            {
                this.Resultant_load += this.SteelWeight * this.g;
            }
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

        //------------------------ GETTERS ---------------------------

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
