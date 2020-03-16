﻿using Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using static Dapper.SqlMapper;

namespace ApiBoilerPlateMyTest.Data
{
    public abstract class DbFactoryBase
    {
        private readonly IConfiguration _config;
        private readonly TestDBContext _context;


        internal string DbConnectionString => _config.GetConnectionString("SQLDBConnectionString");

        public DbFactoryBase(IConfiguration config, TestDBContext context)
        {
            _config = config;
            _context = context;
        }

        internal IDbConnection DbConnection => new SqlConnection(_config.GetConnectionString("SQLDBConnectionString"));

        //public virtual async Task<IEnumerable<T>> DbQueryAsync<T>(string sql, object parameters = null)
        //{
        //    using (IDbConnection dbCon = DbConnection)
        //    {
        //        if (parameters == null)
        //            return await dbCon.QueryAsync<T>(sql);

        //        return await dbCon.QueryAsync<T>(sql, parameters);
        //    }
        //}

        public virtual async Task<IEnumerable<T>> DbQueryAsync<T>(string sql, object parameters = null)
        {
            using (IDbConnection dbCon = DbConnection)
            {
                if (parameters == null)
                    return await dbCon.QueryAsync<T>(sql);

                return await dbCon.QueryAsync<T>(sql, parameters);
            }
        }
        public virtual async Task<T> DbQuerySingleAsync<T>(string sql, object parameters)
        {
            using (IDbConnection dbCon = DbConnection)
            {
                return await dbCon.QueryFirstOrDefaultAsync<T>(sql, parameters);
            }
        }

        public virtual async Task<bool> DbExecuteAsync<T>(string sql, object parameters)
        {
            using (IDbConnection dbCon = DbConnection)
            {
                return await dbCon.ExecuteAsync(sql, parameters) > 0;
            }
        }

        public virtual async Task<bool> DbExecuteScalarAsync(string sql, object parameters)
        {
            using (IDbConnection dbCon = DbConnection)
            {
                return await dbCon.ExecuteScalarAsync<bool>(sql, parameters);
            }
        }

        public virtual async Task<T> DbExecuteScalarDynamicAsync<T>(string sql, object parameters = null)
        {
            using (IDbConnection dbCon = DbConnection)
            {
                if (parameters == null)
                    return await dbCon.ExecuteScalarAsync<T>(sql);

                return await dbCon.ExecuteScalarAsync<T>(sql, parameters);
            }
        }

        public virtual async Task<(IEnumerable<T> Data, int RecordCount)> DbQueryMultipleAsync<T>(string sql, object parameters = null)
        {
            IEnumerable<T> data = null;
            int totalRecords = 0;

            using (IDbConnection dbCon = DbConnection)
            {
                using (GridReader results = await dbCon.QueryMultipleAsync(sql, parameters))
                {
                    data = await results.ReadAsync<T>();
                    totalRecords = await results.ReadSingleAsync<int>();
                }
            }

            return (data, totalRecords);
        }
    }
}
