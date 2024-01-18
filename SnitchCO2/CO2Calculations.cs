using SnitchCommon.Building;
using SnitchCommon.Elements;
using SnitchDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnitchCO2
{
    public class CO2Calculation
    {


        //---------------------- CONSTRUCTORS ------------------------
        public CO2Calculation(Building building)
        {
            this.ADO = new SnitchADO();
            this.building = building;
            this.SetCO2Values();
            this.CalculateElementCO2();
        }

        //----------------------- PROPERTIES -------------------------

        //[JsonIgnore]

        private SnitchADO ADO { get; set; }

        private Building building { get; set; }

        private Dictionary<string, double> MaterialCO2Values { get; set; }

        //------------------------ METHODS ---------------------------
        public void CalculateElementCO2()
        {
            foreach (Beam beam in this.building.Beams.Values)
            {
                string BMC = beam.GetMaterialCode();
                if (this.MaterialCO2Values.ContainsKey(BMC))
                {
                    beam.CO2_Value += this.MaterialCO2Values[BMC] * beam.Mass;
                }
            }

            foreach (Column column in building.Columns.Values)
            {
                string CMC = column.GetMaterialCode();

                if (this.MaterialCO2Values.ContainsKey(CMC))
                {
                    column.CO2_Value += this.MaterialCO2Values[CMC] * column.Mass;
                }
                string CSMC = column.GetSecondMaterialCode();
                if (CSMC != null)
                {
                    if (this.MaterialCO2Values.ContainsKey(CSMC))
                    {
                        column.CO2_Value += this.MaterialCO2Values[CMC] * column.ColumnPart.Mass;
                    }
                }
            }
        }


        //------------------------ SETTERS ---------------------------
        private void SetCO2Values()
        {
            this.MaterialCO2Values = this.ADO.GetCO2ClassAndEmission();
        }

        //------------------------ GETTERS ---------------------------

        public Dictionary<string, double> GetAllCO2Values()
        {
            return this.MaterialCO2Values;
        }
    }
}
