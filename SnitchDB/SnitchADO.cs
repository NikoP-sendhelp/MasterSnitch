using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using Rhino;

namespace SnitchDB
{
    public class SnitchADO
    {
        public readonly string connectionString = "server=localhost;database=elementdb;uid=Np;pwd=159357Np;";

        public readonly string identifier = "Name = @name AND Project = @project AND Material = @material";
        public readonly string updateParam = "Load = @load, Length = @length, Width = @width, Height = @height, CO2 = @CO2";


        //public void SetOrUpdateBeam(Dictionary<string, string> beamDic)
        //{
        //    string query =
        //        "INSERT INTO `elementdb`.`beams` "
        //        + "(`Name`, `ResultantLoad`, `LengthLoad`, `Project`, `Material`, `Length`, `Width`, `Height`, `CO2`) "
        //        + "VALUES "
        //        + "(@name, @resultanLoad, @lengthLoad, @project, @material, @length, @width, @Height, @co2);";
        //    MySqlCommand command = new MySqlCommand(query);
        //    command.Parameters.AddWithValue("@name", beamDic["Name"]);
        //    command.Parameters.AddWithValue("@resultanLoad", Double.Parse(beamDic["Resultant_load"]));
        //    command.Parameters.AddWithValue("@lengthLoad", Double.Parse(beamDic["LengthLoad"]));
        //    command.Parameters.AddWithValue("@project", beamDic["Project_name"]);
        //    command.Parameters.AddWithValue("@material", beamDic["Material_name"]);
        //    command.Parameters.AddWithValue("@length", Double.Parse(beamDic["Length"]));
        //    command.Parameters.AddWithValue("@width", Double.Parse(beamDic["Width"]));
        //    command.Parameters.AddWithValue("@height", Double.Parse(beamDic["Height"]));
        //    command.Parameters.AddWithValue("@CO2", Double.Parse(beamDic["CO2_Value"]));

        //    ExecuteMySQLQuery("SETORINSERT", command);
        //}

        public List<Dictionary<string, string>> SetBeam(Dictionary<string, string> beamDic)
        {
            string query =
                "INSERT INTO elementdb.beams "
                + "(`Name`, `ResultantLoad`, `LengthLoad`, `Project`, `Material`, `Length`, `Width`, `Height`, `CO2`) "
                + "SELECT @name, @resultanLoad, @lengthLoad, @project, @material, @length, @width, @height, @CO2 "
                + "WHERE NOT EXISTS (SELECT * FROM elementdb.beams WHERE " + this.identifier + ");";

            MySqlCommand command = new MySqlCommand(query);
            command.Parameters.AddWithValue("@name", beamDic["Name"]);
            command.Parameters.AddWithValue("@resultanLoad", Double.Parse(beamDic["Resultant_load"]));
            command.Parameters.AddWithValue("@lengthLoad", Double.Parse(beamDic["LengthLoad"]));
            command.Parameters.AddWithValue("@project", beamDic["Project_name"]);
            command.Parameters.AddWithValue("@material", beamDic["Material_name"]);
            command.Parameters.AddWithValue("@length", Double.Parse(beamDic["Length"]));
            command.Parameters.AddWithValue("@width", Double.Parse(beamDic["Width"]));
            command.Parameters.AddWithValue("@height", Double.Parse(beamDic["Height"]));
            command.Parameters.AddWithValue("@CO2", Double.Parse(beamDic["CO2_Value"]));

            return ExecuteMySQLQuery("SET", command);
        }

        public List<Dictionary<string, string>> SetColumn(Dictionary<string, string> columnDic)
        {
            string query =
                "INSERT INTO elementdb.columns "
                + "(`Name`, `ResultantLoad`, `LengthLoad`, `Project`, `Material`, `SecondMaterial`, `Length`, `Width`, `Height`, `CO2`) "
                + "SELECT @name, @resultanLoad, @lengthLoad, @project, @material, @secondMaterial, @length, @width, @height, @CO2 "
                + "WHERE NOT EXISTS (SELECT * FROM elementdb.columns WHERE " + this.identifier + ");";
            MySqlCommand command = new MySqlCommand(query);
            command.Parameters.AddWithValue("@name", columnDic["Name"]);
            command.Parameters.AddWithValue("@resultanLoad", Double.Parse(columnDic["Resultant_load"]));
            command.Parameters.AddWithValue("@lengthLoad", Double.Parse(columnDic["LengthLoad"]));
            command.Parameters.AddWithValue("@project", columnDic["Project_name"]);
            command.Parameters.AddWithValue("@material", columnDic["Material_name"]);
            command.Parameters.AddWithValue("@secondMaterial", columnDic["Second_material_name"]);
            command.Parameters.AddWithValue("@length", Double.Parse(columnDic["Length"]));
            command.Parameters.AddWithValue("@width", Double.Parse(columnDic["Width"]));
            command.Parameters.AddWithValue("@height", Double.Parse(columnDic["Height"]));
            command.Parameters.AddWithValue("@CO2", Double.Parse(columnDic["CO2_Value"]));

            return ExecuteMySQLQuery("SET", command);
        }

        public List<Dictionary<string, string>> GetAllBeams()
        {
            const string query = "SELECT * FROM elementdb.beams;";
            MySqlCommand command = new MySqlCommand(query);
            return ExecuteMySQLQuery("GET", command);
        }

        public List<Dictionary<string, string>> GetAllColumns()
        {
            const string query = "SELECT * FROM elementdb.columns;";
            MySqlCommand command = new MySqlCommand(query);
            return ExecuteMySQLQuery("GET", command);
        }

        public List<Dictionary<string, string>> GetAllCO2()
        {
            const string query = "SELECT * FROM elementdb.co2data;";
            MySqlCommand command = new MySqlCommand(query);
            return ExecuteMySQLQuery("GET", command);
        }

        public List<Dictionary<string, string>> GetBeamsWithLengthLoad(double lengthLoad, string projectName, double margin = 100)
        {
            double bottomMargin = lengthLoad * (1 - (margin / 100));
            double topMargin = lengthLoad * (1 + (margin / 100));

            const string marginQuery = " BETWEEN @bottomMargin AND @topMargin;";

            const string query = "SELECT * FROM elementdb.beams WHERE Project = @projectName AND LengthLoad" + marginQuery;
            MySqlCommand command = new MySqlCommand(query);
            command.Parameters.AddWithValue("@bottomMargin", bottomMargin);
            command.Parameters.AddWithValue("@topMargin", topMargin);
            command.Parameters.AddWithValue("@projectName", projectName);
            return ExecuteMySQLQuery("GET", command);
        }
        public List<Dictionary<string, string>> GetColumnsWithLengthLoad(double lengthLoad, string projectName, double margin = 100)
        {
            double bottomMargin = lengthLoad * (1 - (margin / 100));
            double topMargin = lengthLoad * (1 + (margin / 100));

            const string marginQuery = " BETWEEN @bottomMargin AND @topMargin;";

            const string query = "SELECT * FROM elementdb.columns WHERE Project = @projectName AND LengthLoad" + marginQuery;
            MySqlCommand command = new MySqlCommand(query);
            command.Parameters.AddWithValue("@bottomMargin", bottomMargin);
            command.Parameters.AddWithValue("@topMargin", topMargin);
            command.Parameters.AddWithValue("@projectName", projectName);
            return ExecuteMySQLQuery("GET", command);
        }

        public Dictionary<string, double> GetCO2ClassAndEmission()
        {
            Dictionary<string, double> CO2ClassAndEmission = new Dictionary<string, double>();

            List<Dictionary<string, string>> temp = GetAllCO2();

            foreach (Dictionary<string, string> tempItem in temp)
            {
                if (tempItem.ContainsKey("AffectedRows"))
                {
                    continue;
                }
                CO2ClassAndEmission.Add(tempItem["MaterialClass"], Double.Parse(tempItem["Emission"]));

            }

            return CO2ClassAndEmission;
        }

        public List<Dictionary<string, string>> ExecuteMySQLQuery(string operation, MySqlCommand command)
        {
            List<Dictionary<string, string>> results = new List<Dictionary<string, string>>()
            {
                new Dictionary<string, string>()
                {
                    {"AffectedRows", "0"}
                }
            };

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                command.Connection = connection;

                try
                {
                    connection.Open();

                    switch (operation)
                    {
                        case "GET":
                            // Run the query by calling ExecuteReader().
                            using (MySqlDataReader dataReader = command.ExecuteReader())
                            {
                                // results[0]["AffectedRows"] = dataReader.FieldCount.ToString();
                                int rowAmount = 0;
                                while (dataReader.Read())
                                {
                                    Dictionary<string, string> data = new Dictionary<string, string>();
                                    rowAmount++;
                                    for (int i = 0; i < dataReader.FieldCount; i++)
                                    {
                                        data.Add(dataReader.GetName(i), dataReader.GetString(i));
                                    }
                                    results.Add(data);
                                }
                                results[0]["AffectedRows"] = rowAmount.ToString();
                            }
                            return results;

                        case "SET":
                            int rowsAffected = command.ExecuteNonQuery();
                            results[0]["AffectedRows"] = rowsAffected.ToString();
                            return results;

                        case "DELETE":
                            return results;

                        case "UPDATE":
                            return results;

                        default:
                            return results;
                    }
                }
                catch (Exception ex)
                {
                    RhinoApp.WriteLine($"FAILED: {ex.Message}");
                    return results;
                }
                finally
                {
                    // Close the connection.
                    connection.Close();
                }

            }

        }
    }
}
