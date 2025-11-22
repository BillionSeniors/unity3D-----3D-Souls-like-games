using System;
using System.Data;
using MySql.Data.MySqlClient;
using UnityEngine;
using System.Text;

public class SqlAccess
{
    public static MySqlConnection dbConnection;
    public SqlAccess(string host, string port, string username, string pwd, string database)
    {
        //连接数据库
        try
        {
            string connectionString = string.Format("server = {0};port={1};database = {2};user = {3};password = {4};", host, port, database, username, pwd);
            Debug.Log(connectionString);
            dbConnection = new MySqlConnection(connectionString);
            dbConnection.Open();
            Debug.Log("连接成功！");
        }
        catch (Exception e)
        {
            throw new Exception("连接失败！" + e.Message.ToString());
        }
    }

    //关闭连接
    public void Close()
    {
        if (dbConnection != null)
        {
            dbConnection.Close();
            dbConnection.Dispose();
            dbConnection = null;
        }
    }

    //查询
    public DataSet SelectWhere(string tableName, string[] items, string[] col, string[] operation, string[] values)
    {

        if (col.Length != operation.Length || operation.Length != values.Length)
            throw new Exception("col.Length != operation.Length != values.Length");

        StringBuilder query = new StringBuilder();
        query.Append("SELECT ");
        query.Append(items[0]);

        for (int i = 1; i < items.Length; ++i)
        {
            query.Append(", ");
            query.Append(items[i]);
        }

        query.Append(" FROM ");
        query.Append(tableName);
        query.Append(" WHERE 1=1");

        for (int i = 0; i < col.Length; ++i)
        {
            query.Append(" AND ");
            query.Append(col[i]);
            query.Append(operation[i]);
            query.Append("'");
            query.Append(values[0]);
            query.Append("' ");
        }
        Debug.Log(query.ToString());
        return ExecuteQuery(query.ToString());
    }

    //执行sql语句
    public static DataSet ExecuteQuery(string sqlString)
    {
        if (dbConnection.State == ConnectionState.Open)
        {
            DataSet ds = new DataSet();
            try
            {
                MySqlDataAdapter da = new MySqlDataAdapter(sqlString, dbConnection);
                da.Fill(ds);
            }
            catch (Exception ee)
            {
                throw new Exception("SQL:" + sqlString + "/n" + ee.Message.ToString());
            }
            finally
            {
            }
            return ds;
        }
        return null;
    }
}
