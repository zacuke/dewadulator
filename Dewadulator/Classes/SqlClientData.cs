using System;
using System.IO;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Collections.Generic;

namespace Dewadulator.Classes
{

    public class SqlClientData
    {
        private enum SQLTypes
        {
            DataTable,
            DataSet,
            Scalar,
            NonQuery
        }

        public string sqlConn;

        public SqlClientData()
        {
            sqlConn = "Data Source=testsql; Database=GoldPointSystems;Integrated Security=true;";
        }

        public SqlClientData(string _sqlConn)
        {
            sqlConn = _sqlConn;
        }

        public DataTable TextTable(string sqlText, params object[] paramaters)
        {
            return (DataTable)InternalQuery(sqlText, SQLTypes.DataTable, CommandType.Text, paramaters);
        }

        public DataTable ProcTable(string procName, params object[] paramaters)
        {
            return (DataTable)InternalQuery(procName, SQLTypes.DataTable, CommandType.StoredProcedure, paramaters);
        }

        public DataSet ProcDataSet(string procName, params object[] paramaters)
        {
            return (DataSet)InternalQuery(procName, SQLTypes.DataSet, CommandType.StoredProcedure, paramaters);
        }

        public DataSet TextDataSet(string sqlText, params object[] paramaters)
        {
            return (DataSet)InternalQuery(sqlText, SQLTypes.DataSet, CommandType.Text, paramaters);
        }

        public object ProcScalar(string procName, params object[] paramaters)
        {
            return InternalQuery(procName, SQLTypes.Scalar, CommandType.StoredProcedure, paramaters);
        }

        public object TextScalar(string sqlText, params object[] paramaters)
        {
            return InternalQuery(sqlText, SQLTypes.Scalar, CommandType.Text, paramaters);
        }

        public void ProcNonQuery(string procName, params object[] paramaters)
        {
            InternalQuery(procName, SQLTypes.NonQuery, CommandType.StoredProcedure, paramaters);
        }

        public DataTable ProcTable(string procName, List<string> paramaters)
        {
            return (DataTable)InternalQuery(procName, SQLTypes.DataTable, CommandType.StoredProcedure, paramaters.ToArray());
        }

        public void TextNonQuery(string sqlText, List<string> paramaters)
        {
            InternalQuery(sqlText, SQLTypes.NonQuery, CommandType.Text, paramaters.ToArray());
        }

        public void TextNonQuery(string sqlText, params object[] paramaters)
        {
            InternalQuery(sqlText, SQLTypes.NonQuery, CommandType.Text, paramaters);
        }

        private object InternalQuery(string sqlText, SQLTypes sqlType, CommandType commandTypes, object[] myParams)
        {
            object functionReturnValue = null;
            SqlDataAdapter _dataAdapter;
            SqlConnection myConnection = new SqlConnection(sqlConn);

            //  using (SqlConnection connection = (SqlConnection)((SqlConnectionInfo)base.GetLocalInstance().obj()).CreateConnectionObject())


            try
            {


                //create a new sqlite Command object
                SqlCommand _myCommand = new SqlCommand(sqlText, myConnection);

                //Set up the command type
                switch (commandTypes)
                {
                    case CommandType.StoredProcedure:
                        _myCommand.CommandType = CommandType.StoredProcedure;
                        break;
                    case CommandType.Text:
                        _myCommand.CommandType = CommandType.Text;
                        break;
                }

                //Add the paramaters if there are any.
                if (myParams.Length > 0)
                {
                    for (int xInt = 0; xInt <= myParams.Length - 1; xInt += 2)
                    {
                        _myCommand.Parameters.AddWithValue(myParams[xInt].ToString(), myParams[xInt + 1]);
                    }
                }

                //Open the connection if needed.
                if (myConnection.State == ConnectionState.Closed)
                {
                    myConnection.Open();
                }

                //Set up the Sql Types
                switch (sqlType)
                {
                    case SQLTypes.DataSet:
                        var ds = new DataSet();
                        _dataAdapter = new SqlDataAdapter(_myCommand);
                        _dataAdapter.Fill(ds);
                        functionReturnValue = ds;
                        break;
                    case SQLTypes.DataTable:
                        var dt = new DataTable();
                        _dataAdapter = new SqlDataAdapter(_myCommand);
                        _dataAdapter.Fill(dt);
                        functionReturnValue = dt;
                        break;
                    case SQLTypes.NonQuery:
                        _myCommand.ExecuteNonQuery();
                        functionReturnValue = null;
                        break;
                    case SQLTypes.Scalar:
                        functionReturnValue = _myCommand.ExecuteScalar();
                        break;
                    default:
                        functionReturnValue = null;
                        break;

                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw;
            }
            finally
            {
                myConnection.Close();
            }

            return functionReturnValue;
        }
    }
}
