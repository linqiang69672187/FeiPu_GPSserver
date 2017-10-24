using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace DbComponent
{
    public class SQLHelper
    {
        private static String connectionString = System.Configuration.ConfigurationSettings.AppSettings["m_connectionString"].ToString();

        #region ExecuteNonQuery封装

        public static int ExecuteNonQuery(CommandType cmdtype, string cmdText, params SqlParameter[] Parameters)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
         {
             conn.Open();
             using (SqlCommand cmd = new SqlCommand())
             {
                 cmd.CommandText = cmdText;
                 cmd.CommandType = cmdtype;
                 cmd.Connection = conn;
                 foreach (SqlParameter Parameter in Parameters)
                 {
                     cmd.Parameters.Add(Parameter);
                }
                 return cmd.ExecuteNonQuery();
              }
         }
          
        }
        #endregion

        #region ExecuteScalar封装
        public static object ExecuteScalar(CommandType cmdtype, string cmdText, params SqlParameter[] Parameters)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.CommandText = cmdText;
                    cmd.CommandType = cmdtype;
                    cmd.Connection = conn;

                    foreach (SqlParameter Parameter in Parameters)
                    {
                        cmd.Parameters.Add(Parameter);
                    }
                    return cmd.ExecuteScalar();
                }
            }

        }
        #endregion

        #region ExecuteScalar封装
        public static object ExecuteScalarStrProc(CommandType cmdtype, string StoredProcedureName, params SqlParameter[] Parameters)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand())
                {                    
                    cmd.Connection = conn;
                    cmd.CommandType = cmdtype;
                    cmd.CommandText = StoredProcedureName;

                    foreach (SqlParameter Parameter in Parameters)
                    {
                        cmd.Parameters.Add(Parameter);
                    }
                    return cmd.ExecuteScalar();
                }
            }
        }
        #endregion
        
        #region ExecuteRead封装
        public static DataTable ExecuteRead(CommandType cmdtype, string cmdText, int startRowIndex, int maximumRows,string tableName, params SqlParameter[] Parameters)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlDataAdapter dr = new SqlDataAdapter(cmdText, conn);
                DataSet ds = new DataSet();
                foreach (SqlParameter Parameter in Parameters)
                {
                      dr.SelectCommand.Parameters.Add(Parameter); 
                }
                dr.Fill(ds, startRowIndex, maximumRows, tableName);
                return ds.Tables[0];
            }
        }
        #endregion

        #region ExecuteReadStrProc封装
        public static DataTable ExecuteReadStrProc(CommandType cmdtype, string StoredProcedureName, int startRowIndex, int maximumRows, string tableName, params SqlParameter[] p)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlDataAdapter da = new SqlDataAdapter();
                DataSet ds = new DataSet();
                if (p.Length > 0)
                {
                    da.SelectCommand = new SqlCommand();
                    da.SelectCommand.Connection = conn;
                    da.SelectCommand.CommandText = StoredProcedureName;
                    da.SelectCommand.CommandType = cmdtype;
                    for (int i = 0; i < p.Length; i++)
                    {
                        da.SelectCommand.Parameters.Add(p[i]);
                    }
                    da.Fill(ds, startRowIndex, maximumRows, tableName);                    
                }
                return ds.Tables[0];                                 
            }
        }
        #endregion


        /// <summary>读取数据库所有表，不分页。存储过程或者SQL语句
   
        /// <example>For example:
        /// <code>
        /// 
        /// 
        /// </code>
        /// 返回 DATATABLE.
        /// </example>
        /// </summary>
        #region ExecuteReadStrProc封装，不分页
        public static DataTable ExecuteReadStrProc(CommandType cmdtype, string StoredProcedureName,string tableName, params SqlParameter[] p)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlDataAdapter da = new SqlDataAdapter();
                DataSet ds = new DataSet();
                if (p.Length > 0)
                {
                    da.SelectCommand = new SqlCommand();
                    da.SelectCommand.Connection = conn;
                    da.SelectCommand.CommandText = StoredProcedureName;
                    da.SelectCommand.CommandType = cmdtype;
                    for (int i = 0; i < p.Length; i++)
                    {
                        da.SelectCommand.Parameters.Add(p[i]);
                    }
                    da.Fill(ds, tableName);
                }
                return ds.Tables[0];
            }
        }
        #endregion

        #region ExecuteRead封装
        public static DataTable ExecuteRead(CommandType cmdtype, string cmdText, string tableName, params SqlParameter[] Parameters)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlDataAdapter dr = new SqlDataAdapter(cmdText,conn);
                DataSet ds = new DataSet();
                foreach (SqlParameter Parameter in Parameters)
                {
                    dr.SelectCommand.Parameters.Add(Parameter);
                }
                dr.Fill(ds, tableName);
                return ds.Tables[0];
             }
        }
        #endregion



    }
    
}
