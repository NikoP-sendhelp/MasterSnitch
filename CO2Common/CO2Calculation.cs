using Newtonsoft.Json;
using SnitchCommon.Building;
using SnitchCommon.Elements;
using SnitchDatabase;
using System.Collections.Generic;

namespace CO2Common
{
    public class CO2Calculation
    {

        //---------------------- CONSTRUCTORS ------------------------
        [JsonConstructor]
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
            this.MaterialCO2Values = this.ADO.GetC02Values();
        }

        //------------------------ GETTERS ---------------------------

        public Dictionary<string, double> GetAllCO2Values()
        {
            return this.MaterialCO2Values;
        }
    }
}
