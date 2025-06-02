using Domain.Models;
using Domain.Models.EnumMaster;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
//using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;
using MySqlConnector;
namespace Domain.Services
{
    public class WebUIDBContext : IDisposable
    {
        public MySqlConnection Connection { get; }

        public WebUIDBContext(string connectionString)
        {
            Connection = new MySqlConnection(connectionString);
        }
        public void Dispose() => Connection.Dispose();
    }
    public class WebUIDatabaseService
    {
        private readonly ILogger<WebUIDatabaseService> _logger;
        //internal WebUIDBContext _db { get; set; }
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;
        //public WebUIDatabaseService(IConfiguration configuration, WebUIDBContext db, ILogger<WebUIDatabaseService> logger)
        public WebUIDatabaseService(IConfiguration configuration, ILogger<WebUIDatabaseService> logger)
        {
            _logger = logger;
            _configuration = configuration;
            _connectionString = configuration["DatabaseSettings:MySql:WebUIDBConnectionString"];
        }
        //public WebUIDatabaseService(WebUIDBContext db, ILogger<WebUIDatabaseService> logger)
        //{
        //    _db = db;
        //    _logger = logger;
        //}
        public LapsePolicy GetLapsePolicyByCode(string code)
        {
            LapsePolicy lapsePolicy = null;
            using var connection = new MySqlConnection(_connectionString);
            try
            {
                DataSet ds = new DataSet();                
                if (connection.State != ConnectionState.Open && connection.State != ConnectionState.Connecting)
                {
                    connection.Open();
                }
                using var cmd = connection.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "bfl_Get_LapsePolicyByCode";
                cmd.Parameters.AddWithValue("P_Code", code);

                var adaptor = new MySqlDataAdapter(cmd);
                adaptor.Fill(ds);

                if (ds?.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    DataRow row = ds.Tables[0].Rows[0];
                    lapsePolicy = new LapsePolicy()
                    {
                        Id = Convert.ToInt32(row["id"]),
                        Name = row["PolicyName"].ToString(),
                        Code = row["PolicyCode"].ToString(),
                        DurationType = row["DurationType"].ToString(),
                        DurationValue = Convert.ToInt32(row["DurationValue"])
                    };
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                {
                    connection.Close();
                }
            }
            return lapsePolicy;
        }
        public int GetCustomerSegmentCount(string query)
        {
            int segmentCount = 0;
            using var connection = new MySqlConnection(_connectionString);
            try
            {
                DataSet ds = new DataSet();
                if (connection.State != ConnectionState.Open && connection.State != ConnectionState.Connecting)
                {
                    connection.Open();
                }
                using var cmd = connection.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "bfl_Get_CustomerSegmentCount";
                cmd.Parameters.AddWithValue("P_Query", query);

                var adaptor = new MySqlDataAdapter(cmd);
                adaptor.Fill(ds);

                if (ds?.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    DataRow row = ds.Tables[0].Rows[0];
                    segmentCount = Convert.ToInt32(row["Count"]);
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                {
                    connection.Close();
                }
            }
            return segmentCount;
        }
        public async Task<List<DBEnumValue>> GetDBEnumValuesAsync(string masterCode = null)
        {
            List<DBEnumValue> enumValues = new List<DBEnumValue>();
            using var connection = new MySqlConnection(_connectionString);
            #region ABC
            try
            {
                DataSet ds = new DataSet();
                using var cmd = connection.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "usp_BFL_GetEnumValues";
                cmd.Parameters.AddWithValue("P_MasterCode", masterCode);
                if(connection.State == ConnectionState.Closed)
                {
                    await connection.OpenAsync().ConfigureAwait(false);
                }
                enumValues = await ReadEnumMasterValueAsync(await ExecuteCommandAsync(cmd).ConfigureAwait(false)).ConfigureAwait(false);
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                {
                    await connection.CloseAsync().ConfigureAwait(false);
                }
            }
            return enumValues;
            #endregion
            async Task<DbDataReader> ExecuteCommandAsync(MySqlCommand cmd) => await cmd.ExecuteReaderAsync().ConfigureAwait(false);
        }
        public async Task<List<DBEnumValue>> GetMerchantDBEnumValues(string preFix, string category, string merchantGroupId, string merchantId, string source, int? isTripleReward = null,string? merchantType=null)
        {
            List<DBEnumValue> enumValues = new List<DBEnumValue>();
            using var connection = new MySqlConnection(_connectionString);
            #region ABC
            try
            {
                _logger.LogInformation($"category:{category} , merchantGroupId : {merchantGroupId}, merchantId : {merchantId}, source : {source}, merchantType : {merchantType},isTripleReward : {isTripleReward} ");
               

               
                DataSet ds = new DataSet();
                using var cmd = connection.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "usp_BFL_GetMerchantEnumValues";
                cmd.Parameters.AddWithValue("P_Category", String.IsNullOrEmpty(category) ? null : category);
                cmd.Parameters.AddWithValue("P_MerchantGroupId", String.IsNullOrEmpty(merchantGroupId) ? null : merchantGroupId);
                cmd.Parameters.AddWithValue("P_MerchantId", merchantId);
                cmd.Parameters.AddWithValue("P_IsTripleReward", isTripleReward);
                cmd.Parameters.AddWithValue("P_MerchantType", merchantType);
                cmd.Parameters.AddWithValue("P_Source", String.IsNullOrEmpty(source) ? null : source);
                if (connection.State == ConnectionState.Closed)
                {
                    await connection.OpenAsync().ConfigureAwait(false);
                }
                enumValues = await ReadEnumMasterValueAsync(await ExecuteCommandAsync(cmd).ConfigureAwait(false)).ConfigureAwait(false);
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                {
                    await connection.CloseAsync().ConfigureAwait(false);
                }
            }
            return enumValues;
            #endregion
            async Task<DbDataReader> ExecuteCommandAsync(MySqlCommand cmd) => await cmd.ExecuteReaderAsync().ConfigureAwait(false);
        }        
        public bool IsMerchantExist(string preFix, string segmentCodes, string merchantId,string merchantType)
        {
            var flag = false;
            using var connection = new MySqlConnection(_connectionString);
            try
            {
                DataSet ds = new DataSet();
                if (connection.State != ConnectionState.Open && connection.State != ConnectionState.Connecting)
                {
                    connection.Open();
                }
                using var cmd = connection.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "usp_BFL_MerchantSegmentValidation";
                cmd.Parameters.AddWithValue("P_SegmentCodes", segmentCodes);
                cmd.Parameters.AddWithValue("P_MerchantId", merchantId);
                cmd.Parameters.AddWithValue("P_MerchantType", merchantType);

                var adaptor = new MySqlDataAdapter(cmd);
                adaptor.Fill(ds);
                _logger.LogInformation($"{preFix} FROM Database : DS : { JsonConvert.SerializeObject(ds)}");
                if (ds?.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    DataRow row = ds.Tables[0].Rows[0];
                    var merchantStatus = Convert.ToInt32(row["MerchantStatus"]);
                    if(merchantStatus == 2)
                    {
                        flag = true;
                    }
                }
            }
            catch(Exception ex)
            {
                throw;
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                {
                    connection.Close();
                }
            }
            return flag;
        }        
        private async Task<List<DBEnumValue>> ReadEnumMasterValueAsync(DbDataReader reader)
        {
            var models = new List<DBEnumValue>();
            using (reader)
            {
                while (await reader.ReadAsync().ConfigureAwait(false))
                {
                    var model = new DBEnumValue()
                    {
                        MasterId = reader.IsDBNull("MasterId") ? 0 : reader.GetInt32("MasterId"),
                        MasterCode = reader.IsDBNull("MasterCode") ? string.Empty : reader.GetString("MasterCode"),
                        MasterName = reader.IsDBNull("MasterName") ? string.Empty : reader.GetString("MasterName"),

                        Id = reader.IsDBNull("Id") ? 0 : reader.GetInt32("Id"),
                        Code = reader.IsDBNull("Code") ? string.Empty : reader.GetString("Code"),
                        Name = reader.IsDBNull("Name") ? string.Empty : reader.GetString("Name"),

                        GroupMerchantCode = reader.IsDBNull("GroupMerchantCode") ? string.Empty : reader.GetString("GroupMerchantCode"),
                        GroupMerchantName = reader.IsDBNull("GroupMerchantName") ? string.Empty : reader.GetString("GroupMerchantName"),
                        BrandCode = reader.IsDBNull("BrandCode") ? string.Empty : reader.GetString("BrandCode")

                    };
                    models.Add(model);
                }
            }
            return models;
        }
        public async Task<List<DBEnumValue>> GetBillerDBEnumValues(string preFix, string category, string billerId)
        {
            List<DBEnumValue> responseModels = new List<DBEnumValue>();
            using var connection = new MySqlConnection(_connectionString);
            try
            {
                using var cmd = connection.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "usp_BFL_GetBillerEnumValues";
                if (connection.State == ConnectionState.Closed)
                {
                    await connection.OpenAsync().ConfigureAwait(false);
                }
                AttachParameters(cmd);
                responseModels = await ReadEnumMasterValueAsync(await cmd.ExecuteReaderAsync()).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error: {ex.Message}");
                throw;
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                {
                    await connection.CloseAsync().ConfigureAwait(false);
                }
            }

            _logger.LogInformation($"GetBillerDBEnumValues For category : {category}, billerId : {billerId},  Response : {JsonConvert.SerializeObject(responseModels)}");
            return responseModels;

            #region Local Methods  

            void AttachParameters(MySqlCommand cmd)
            {
                cmd.Parameters.AddWithValue("P_Category", category.Trim());
                cmd.Parameters.AddWithValue("P_BillerId", billerId.Trim());
            };
            #endregion
        }

        //vpa segment
        public int GetVPACustomerSegmentCount(string query)
        {
            int segmentVPACount = 0;
            using var connection = new MySqlConnection(_connectionString);
            try
            {
                DataSet ds = new DataSet();
                if (connection.State != ConnectionState.Open && connection.State != ConnectionState.Connecting)
                {
                    connection.Open();
                }
                using var cmd = connection.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "bfl_Get_VPACustomerSegmentCount";
                cmd.Parameters.AddWithValue("P_Query", query);

                var adaptor = new MySqlDataAdapter(cmd);
                adaptor.Fill(ds);

                if (ds?.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    DataRow row = ds.Tables[0].Rows[0];
                    segmentVPACount = Convert.ToInt32(row["Count"]);
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                {
                    connection.Close();
                }
            }
            return segmentVPACount;
        }
    }
}
