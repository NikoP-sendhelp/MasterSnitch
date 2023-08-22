using System.Data.SqlClient;
using System.Data;
using Rhino;
using SnitchCommon.Elements;

namespace SnitchDatabase
{
    public class SnitchADO
    {
        public void AddBeam(Beam beam)
        {
            string query = $"INSERT INTO Beams(Name, Load) VALUES ({beam.Name}, {beam.Load})";
            ExecuteQuery("SET", query);

        }

        public void AddColumn(Column column)
        {
            string query = $"INSERT INTO Columns(Name, Load) VALUES ({column.Name}, {column.Load})";
            ExecuteQuery("SET", query);

        }

        public void GetAllBeams()
        {
            const string query = "SELECT * FROM Beam";
            ExecuteQuery("GET", query);
        }

        public void GetAllColumns()
        {
            const string query = "SELECT * FROM Column";
            ExecuteQuery("GET", query);
        }

        private void ExecuteQuery(string operation, string query)
        {
            using (SqlConnection connection = new SqlConnection(SnitchDatabaseV2.Properties.Settings.Default.BuildingDatabaseConnectionString))
            {
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    try
                    {
                        connection.Open();


                        switch (operation)
                        {
                            case "GET":
                                // Run the query by calling ExecuteReader().
                                using (SqlDataReader dataReader = command.ExecuteReader())
                                {
                                    // Create a data table to hold the retrieved data.
                                    DataTable dataTable = new DataTable();

                                    // Load the data from SqlDataReader into the data table.
                                    dataTable.Load(dataReader);

                                    foreach (DataRow row in dataReader)
                                    {
                                        RhinoApp.WriteLine(row.ToString());
                                    }
                                    // Close the SqlDataReader.
                                    dataReader.Close();
                                }
                                break;

                            case "SET":
                                command.ExecuteNonQuery();
                                break;

                            case "DELETE":
                                break;

                            case "UPDATE":
                                break;

                            default:
                                break;
                        }
                    }
                    catch
                    {
                        RhinoApp.WriteLine("FAILED");
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
}
