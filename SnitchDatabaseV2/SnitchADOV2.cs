using System.Data.SqlClient;
using System.Data;
using Rhino;
using SnitchCommon.Elements;
using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using System.IO.Packaging;
using System.Security.Cryptography.X509Certificates;
using System.Runtime.CompilerServices;
using Eto.Forms;

namespace SnitchDatabase
{
    public class SnitchADO
    {
        private readonly string identifier = "Name = @name AND Project = @project AND Material = @material";
        private readonly string updateParam = "Load = @load, Length = @length, Width = @width, Height = @height, CO2 = @CO2";

        public void AddOrUpdateBeam(Dictionary<string, string> beamDic)
        {
            string query = "IF EXISTS (SELECT * FROM Beams WHERE " + this.identifier + ") " +
                "BEGIN " +
                "UPDATE Beams SET " + this.updateParam + ", LengthLoad = @lengthLoad WHERE " + this.identifier + " " +
                "END " +
                "ELSE " +
                "BEGIN " +
                "INSERT INTO Beams (Name, Load, Project, Material, Length, Width, Height, CO2, LengthLoad, Liveload) VALUES (@name, @load, @project, @material, @length, @width, @height, @CO2, @lengthLoad, @liveload) " +
                "END";
            // string query = "INSERT INTO Beams (Name, Load, Project) VALUES (@name, @load, @project);";
            SqlCommand command = new SqlCommand(query);
            command.Parameters.AddWithValue("@name", beamDic["Name"]);
            command.Parameters.AddWithValue("@load", Double.Parse(beamDic["Resultant_load"]));
            command.Parameters.AddWithValue("@project", beamDic["Project_name"]);
            command.Parameters.AddWithValue("@material", beamDic["Material_name"]);
            command.Parameters.AddWithValue("@length", Double.Parse(beamDic["Length"]));
            command.Parameters.AddWithValue("@width", Double.Parse(beamDic["Width"]));
            command.Parameters.AddWithValue("@height", Double.Parse(beamDic["Height"]));
            command.Parameters.AddWithValue("@CO2", Double.Parse(beamDic["CO2_Value"]));
            command.Parameters.AddWithValue("@lengthLoad", Double.Parse(beamDic["LengthLoad"]));
            command.Parameters.AddWithValue("@liveload", Double.Parse(beamDic["Live_load"]));

            ExecuteQuery("SET", command);

        }

        public void AddOrUpdateColumn(Dictionary<string, string> columnDic)
        {
            string query = "IF EXISTS (SELECT * FROM Columns WHERE " + this.identifier + ") " +
                "BEGIN " +
                "UPDATE Columns SET " + this.updateParam + ", Second_Material = @secondMaterial WHERE " +this.identifier + " " +
                "END " +
                "ELSE " +
                "BEGIN " +
                "INSERT INTO Columns (Name, Load, Project, Material, Length, Width, Height, CO2, Second_Material) VALUES (@name, @load, @project, @material, @length, @width, @height, @CO2, @secondMaterial) " +
                "END";
            SqlCommand command = new SqlCommand(query);
            command.Parameters.AddWithValue("@name", columnDic["Name"]);
            command.Parameters.AddWithValue("@load", Double.Parse(columnDic["Resultant_load"]));
            command.Parameters.AddWithValue("@project", columnDic["Project_name"]);
            command.Parameters.AddWithValue("@material", columnDic["Material_name"]);
            command.Parameters.AddWithValue("@length", Double.Parse(columnDic["Length"]));
            command.Parameters.AddWithValue("@width", Double.Parse(columnDic["Width"]));
            command.Parameters.AddWithValue("@height", Double.Parse(columnDic["Height"]));
            command.Parameters.AddWithValue("@CO2", Double.Parse(columnDic["CO2_Value"]));
            command.Parameters.AddWithValue("@secondMaterial", columnDic["Second_material_name"]);
            ExecuteQuery("SET", command);

        }

        public List<object> GetAllBeams()
        {
            const string query = "SELECT * FROM Beams;";
            SqlCommand command = new SqlCommand(query);
            return ExecuteQuery("GET", command);
        }

        public List<object> GetAllColumns()
        {
            const string query = "SELECT * FROM Columns;";
            SqlCommand command = new SqlCommand(query);
            return ExecuteQuery("GET", command);
        }

        public List<object> GetAllProjectBeams(string projectName)
        {
            const string query = "SELECT * FROM Beams WHERE project = @project;";
            SqlCommand command = new SqlCommand(query);
            command.Parameters.AddWithValue("@project", projectName);
            return ExecuteQuery("GET", command);
        }

        public List<object> GetAllProjectColumns(string projectName)
        {
            const string query = "SELECT * FROM Columns WHERE project = @project;";
            SqlCommand command = new SqlCommand(query);
            command.Parameters.AddWithValue("@project", projectName);
            return ExecuteQuery("GET", command);
        }

        public Dictionary<string, double> GetC02Values()
        {
            const string query = "SELECT * FROM CO2;";
            SqlCommand command = new SqlCommand(query);
            List<object> AllCO2Values = ExecuteQuery("GET", command);

            Dictionary<string, double> CO2ValueDic = new Dictionary<string, double>();

            foreach (List<object> CO2Value in AllCO2Values)
            {
                CO2ValueDic.Add((string)CO2Value[1], (double)CO2Value[3]);
            }

            return CO2ValueDic;
        }

        public List<object> GetBeamWithLoad(double Load)
        {
            const string query = "SELECT * FROM BEAMS WHERE LengthLoad = @load";
            SqlCommand command = new SqlCommand(query);
            command.Parameters.AddWithValue("@load", Load);
            return ExecuteQuery("GET", command);
        }

        public List<object> GetColumnWithLoad(double load, double margin = 100)
        {
            string query = "SELECT * FROM Columns WHERE Load";

            double bottomMargin = load * (1 - (margin / 100));
            double topMargin = load * (1 + (margin / 100));

            const string marginQuery = " BETWEEN @bottomMargin AND @topMargin;";

            query += marginQuery;
            SqlCommand command = new SqlCommand(query);
            command.Parameters.AddWithValue("@bottomMargin", bottomMargin);
            command.Parameters.AddWithValue("@topMargin", topMargin);
            return ExecuteQuery("GET", command);
        }

        private List<object> ExecuteQuery(string operation, SqlCommand command)
        {
            List<object> temp = new List<object>();
            using (SqlConnection connection = new SqlConnection(SnitchDatabaseV2.Properties.Settings.Default.BuildingDatabaseConnectionString))
            {
                command.Connection = connection;

                try
                {
                        connection.Open();

                        switch (operation)
                        {
                            case "GET":
                                // Run the query by calling ExecuteReader().
                                SqlDataReader dataReader = command.ExecuteReader();
                                DataTable dataTable = new DataTable();
                                dataTable.Load(dataReader);


                                foreach (DataRow row in dataTable.Rows)
                                {
                                    temp.Add(row.ItemArray.ToList());
                                }


                            // Close the SqlDataReader.
                            dataReader.Close();
                            return temp;



                            case "SET":
                                command.ExecuteNonQuery();
                                return null;

                            case "DELETE":
                                return null;

                            case "UPDATE":
                                return null;

                            default:
                                return null;
                        }
                    }
                    catch (Exception ex)
                    {
                        RhinoApp.WriteLine($"FAILED: {ex.Message}");
                    return null;
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
