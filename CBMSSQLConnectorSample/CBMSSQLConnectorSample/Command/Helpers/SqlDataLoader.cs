using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using MG.CB.Command.DataHandler.Argument;
using MG.CB.Command.DataHandler.Argument.Interfaces;
using MG.CB.Command.Interfaces;
using MG.CB.Metadata.DataModel.Dataware.Interfaces;

namespace CBTestConnector.Command.Helpers
{
    /// <summary> Utility methods used to fill the <see cref="IResultSetLoader"/>. </summary>
    public static class SqlDataLoader
    {
        public  static void LoadData(IResultSetLoader loader, IExecutionContext context, ICollection<IColumnArgument> columns,
            string connectionString, string queryString)
        {
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (SqlCommand sqlCommand = new SqlCommand(queryString, conn))
                {
                    //Retrieves a list of the variables (i.e. @ parameters).
                    List<Variable> variables = context.GetAllVariables().ToList();
                    variables.ForEach(v =>
                    {
                        sqlCommand.Parameters.Add(new SqlParameter(v.Name, context.GetVariableValue(v)));
                    });
                    using (var reader = sqlCommand.ExecuteReader())
                    {
                        //Opens an ITableResultLoader object for writing. 
                        using (ITableResultLoader tableLoader = loader.OpenTableResultLoader(columns))
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read() &&
                                       !tableLoader.CancellationToken.IsCancellationRequested)
                                {
                                    //Create a new ITableResultRow instance
                                    ITableResultRow row = tableLoader.NewRow();
                                    //Gets the values in the row, one per column.
                                    reader.GetValues(row.GetValues());
                                    //Adds a row to the ITableResultLoader.
                                    tableLoader.Add(row);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}