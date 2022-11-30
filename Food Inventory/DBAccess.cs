using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Data;
using System.Data.OleDb;

public class DBAccess
{
    public static OleDbConnection connection = new OleDbConnection();
    private static OleDbCommand command = new OleDbCommand();
    private static OleDbDataReader DbReader;
    private static OleDbDataAdapter adapter = new OleDbDataAdapter();
    public OleDbTransaction DbTran;


    private string connectionString = @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=";
    // default password for access database is Talent
    private string protectedDatabaseRequirements = ";Jet OLEDB:Database Password=Talent";
    public string ConnectionString { get => connectionString; set => connectionString = value; }

    public DBAccess(string dataSource)
    {

        this.connectionString = ConnectionString + dataSource + protectedDatabaseRequirements;
    }


    public void createConn()
    {
        try
        {
            if (connection.State != ConnectionState.Open)
            {
                connection.ConnectionString = ConnectionString;
                connection.Open();
            }
        }
        catch (OleDbException ex)
        {
            throw ex;
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }


    public void closeConn()
    {
        if (connection.State == ConnectionState.Open)
        {
            connection.Close();
        }

    }


    public int executeDataAdapter(DataTable tblName, string strSelectOleDb)
    {
        try
        {
            if (connection.State == 0)
            {
                createConn();
            }

            adapter.SelectCommand.CommandText = strSelectOleDb;
            adapter.SelectCommand.CommandType = CommandType.Text;
            OleDbCommandBuilder DbCommandBuilder = new OleDbCommandBuilder(adapter);


            string insert = DbCommandBuilder.GetInsertCommand().CommandText.ToString();
            string update = DbCommandBuilder.GetUpdateCommand().CommandText.ToString();
            string delete = DbCommandBuilder.GetDeleteCommand().CommandText.ToString();


            return adapter.Update(tblName);
        }
        catch (OleDbException ex)
        {
            throw ex;
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }


    public void readDatathroughAdapter(string query, DataTable tblName)
    {
        try
        {
            if (connection.State == ConnectionState.Closed)
            {
                createConn();
            }

            command.Connection = connection;
            command.CommandText = query;
            command.CommandType = CommandType.Text;

            adapter = new OleDbDataAdapter(command);
            adapter.Fill(tblName);
        }
        catch (OleDbException ex)
        {
            throw ex;
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }


    public OleDbDataReader readDatathroughReader(string query)
    {
        //DataReader used to sequentially read data from a data source
        OleDbDataReader reader;

        try
        {
            if (connection.State == ConnectionState.Closed)
            {
                createConn();
            }

            command.Connection = connection;
            command.CommandText = query;
            command.CommandType = CommandType.Text;

            reader = command.ExecuteReader();
            return reader;
        }
        catch (OleDbException ex)
        {
            throw ex;
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }


    public int executeQuery(OleDbCommand dbCommand)
    {
        try
        {
            if (connection.State == 0)
            {
                createConn();
            }

            dbCommand.Connection = connection;
            dbCommand.CommandType = CommandType.Text;


            return dbCommand.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }
}

