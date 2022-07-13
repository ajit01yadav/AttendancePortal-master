using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using AttendancePortal.Common;

namespace AttendancePortal.Repository
{
    /// <summary>
    /// Authorized class
    /// </summary>
    public class Authorized
    {
        /// <summary>
        /// db connection string
        /// </summary>
        private string _dbConnectionString = string.Empty;

        /// <summary>
        /// Class constructor
        /// </summary>
        public Authorized()
        {
            _dbConnectionString = ConfigurationManager.ConnectionStrings["DbConnection"].ConnectionString;
        }

        /// <summary>
        /// Check windonws user name.
        /// </summary>
        /// <param name="_userName"></param>
        /// <returns></returns>
        public bool IsAuthorized(string _userName)
        {
            SqlConnection objConnection = null;
            try
            {
                objConnection = new SqlConnection(_dbConnectionString);
                string Query = String.Format("SELECT COUNT(*) FROM T_Employees WHERE UPPER([WindowsUsername])='{0}' AND StatusId=142", _userName.ToUpper());
                SqlCommand Cmd = new SqlCommand(Query, objConnection);
                objConnection.Open();
                int Result = (int)Cmd.ExecuteScalar();
                objConnection.Close();
                return Result > 0 ? true : false;
            }
            catch (Exception ex)
            {
                throw;
            }
            finally
            {
                if (objConnection.State == ConnectionState.Open)
                {
                    objConnection.Close();
                }
            }
        }

        /// <summary>
        /// Check employee's email id.
        /// </summary>
        /// <param name="_userName"></param>
        /// <param name="_emailID"></param>
        /// <returns></returns>
        public bool IsEmailIDValid(string _userName, string _emailID)
        {
            SqlConnection objConnection = null;
            try
            {
                objConnection = new SqlConnection(_dbConnectionString);
                string Query = String.Format("SELECT COUNT(*) FROM T_Employees WHERE UPPER([EmailId])='{0}' ANd UPPER([WindowsUsername])='{1}' AND StatusId=142", _emailID.ToUpper(), _userName.ToUpper());
                SqlCommand Cmd = new SqlCommand(Query, objConnection);
                objConnection.Open();
                int Result = (int)Cmd.ExecuteScalar();
                objConnection.Close();
                return Result > 0 ? true : false;
            }
            catch (Exception ex)
            {
                throw;
            }
            finally
            {
                if (objConnection.State == ConnectionState.Open)
                {
                    objConnection.Close();
                }
            }
        }

        /// <summary>
        /// Mark timing in the db.
        /// </summary>
        /// <param name="_userName"></param>
        public void MarkTimeinDB(string _userName)
        {
            SqlConnection sqlGetEmployeetimedetails = null;
            SqlConnection sqlConOutTimeType = null;
            try
            {
                string Loggeduserempcode = string.Empty;
                sqlConOutTimeType = new SqlConnection(_dbConnectionString);

                sqlConOutTimeType.Open();
                SqlCommand sqlCmd = new SqlCommand();
                sqlCmd.Connection = sqlConOutTimeType;

                sqlCmd.CommandType = CommandType.StoredProcedure;
                sqlCmd.CommandText = "usp_ace_GetEmployeeCode";
                sqlCmd.Parameters.AddWithValue("@NAME", _userName);

                SqlParameter Parameter = new SqlParameter("@EmployeeCode", SqlDbType.VarChar, 50);
                Parameter.Direction = ParameterDirection.Output;
                sqlCmd.Parameters.Add(Parameter);

                sqlCmd.ExecuteNonQuery();

                if (Parameter.Value != DBNull.Value)
                {
                    Loggeduserempcode = Parameter.Value.ToString().Trim();
                    string hostName = Dns.GetHostName();

                    int noOfRows = 0;

                    sqlGetEmployeetimedetails = new SqlConnection(_dbConnectionString);


                    sqlGetEmployeetimedetails.Open();
                    System.IFormatProvider format = new System.Globalization.CultureInfo("en-GB", true);

                    SqlCommand sqlCmddetails = new SqlCommand();
                    sqlCmddetails.Connection = sqlGetEmployeetimedetails;

                    sqlCmddetails.CommandType = CommandType.StoredProcedure;
                    sqlCmddetails.CommandText = "usp_insert_att_temp_tran";

                    sqlCmddetails.Parameters.AddWithValue("EmployeeCode", Loggeduserempcode);
                    sqlCmddetails.Parameters.AddWithValue("Updated_By", Dns.GetHostAddresses(hostName)[Dns.GetHostAddresses(hostName).Length - 1].ToString());
                    noOfRows = sqlCmddetails.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                throw;
            }
            finally
            {
                if (sqlConOutTimeType.State == ConnectionState.Open)
                {
                    sqlConOutTimeType.Close();
                }
                if (sqlGetEmployeetimedetails.State == ConnectionState.Open)
                {
                    sqlGetEmployeetimedetails.Close();
                }
            }
        }

        public void SaveAttendancePortalError(string ApplicationName, string ErrorMessage, string Method, string InnerException, string StackTrace, string Level, string UserName)
        {
            SqlConnection objConnection = null;
            SqlCommand objCommand = null;
            string ConnStr = string.Empty;
            try
            {
                objConnection = new SqlConnection(_dbConnectionString);
                objConnection.Open();

                objCommand = new SqlCommand("USP_InsertAttendancePortalError", objConnection);
                objCommand.CommandType = CommandType.StoredProcedure;
                SqlParameter[] sqlParam = new SqlParameter[7];

                sqlParam[0] = objCommand.Parameters.AddWithValue("@ApplicationName", ApplicationName);
                sqlParam[1] = objCommand.Parameters.AddWithValue("@ErrorMessage", ErrorMessage);
                sqlParam[2] = objCommand.Parameters.AddWithValue("@Method", Method);
                sqlParam[3] = objCommand.Parameters.AddWithValue("@InnerException", InnerException);
                sqlParam[4] = objCommand.Parameters.AddWithValue("@StackTrace", StackTrace);
                sqlParam[5] = objCommand.Parameters.AddWithValue("@Level", Level);
                sqlParam[6] = objCommand.Parameters.AddWithValue("@CreatedById", UserName);

                int result = objCommand.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw;
            }
            finally
            {
                if (objConnection.State == ConnectionState.Open)
                {
                    objConnection.Close();
                }
            }
        }
    }
}