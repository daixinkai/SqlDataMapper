#define Net45
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlDataMapper
{
    public class Database : IDisposable
    {

        public Database(string connectionStringName)
        {
            var connectionStringSetting = ConfigurationManager.ConnectionStrings[connectionStringName];
            if (connectionStringSetting == null)
            {

            }
            if (!string.IsNullOrEmpty(connectionStringSetting.ProviderName))
            {
                _providerName = connectionStringSetting.ProviderName;
            }
            _connectionString = connectionStringSetting.ConnectionString;
            InitFactory();
        }

        public Database(string connectionString, string providerName)
        {
            if (!string.IsNullOrEmpty(providerName))
            {
                _providerName = providerName;
            }
            _connectionString = connectionString;
            InitFactory();
        }


        ~Database()
        {
            if (!_disposed)
            {
                Dispose();
            }
        }


        void InitFactory()
        {
            this._factory = DbProviderFactories.GetFactory(this._providerName);
        }

        #region Fields
        string _connectionString;

        string _providerName = "System.Data.SqlClient";

        DbProviderFactory _factory;

        TransactionUnit _transaction;

        DbConnection _connection;

        #endregion


        #region Query
        /// <summary>
        /// 查询
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public IEnumerable<T> Query<T>(string sql, params object[] args)
        {
            return Query<T>(sql, BuildParameter(args));
        }
        /// <summary>
        /// 查询,如果需要返回输出参数,使用此重载
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public IEnumerable<T> Query<T>(string sql,  DbParameterRegister[] args)
        {
            return Query<T>(sql, CommandType.Text, args);
        }
#if Net45
        /// <summary>
        /// 查询
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public async Task<IEnumerable<T>> QueryAsync<T>(string sql, params object[] args)
        {
            return await Task.Run(() => Query<T>(sql, args));
        }
        /// <summary>
        /// 查询
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public async Task<IEnumerable<T>> QueryAsync<T>(string sql,  DbParameterRegister[] args)
        {
            return await Task.Run(() => Query<T>(sql, args));
        }
#endif

        #region Procedure
        /// <summary>
        /// 查询存储过程
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="procedureName"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public IEnumerable<T> QueryProcedure<T>(string procedureName, DbParameterRegister[] args)
        {
            return Query<T>(procedureName, CommandType.StoredProcedure, args);
        }
#if Net45
        /// <summary>
        /// 查询存储过程
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="procedureName"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public async Task<IEnumerable<T>> QueryProcedureAsync<T>(string procedureName, DbParameterRegister[] args)
        {
            return await Task.Run(() => QueryProcedure<T>(procedureName, args));
        }

#endif

        #endregion

        #endregion

        #region Execute
        public int Execute(string sql, params object[] args)
        {
            return Execute(sql, BuildParameter(args));
        }
        public int Execute(string sql, DbParameterRegister[] args)
        {
            return Execute(sql, CommandType.Text, args);
        }
#if Net45
        public async Task<int> ExecuteAsync(string sql, params object[] args)
        {
            return await ExecuteAsync(sql, BuildParameter(args));
        }
        public async Task<int> ExecuteAsync(string sql,  DbParameterRegister[] args)
        {
            return await ExecuteAsync(sql, CommandType.Text, args);
        }

#endif

        #region Procedure
        public int ExecuteProcedure(string sql, params object[] args)
        {
            return ExecuteProcedure(sql, BuildParameter(args));
        }
        public int ExecuteProcedure(string sql, DbParameterRegister[] args)
        {
            return Execute(sql, CommandType.StoredProcedure, args);
        }
        #endregion
#if Net45
        public async Task<int> ExecuteProcedureAsync(string sql, params object[] args)
        {
            return await ExecuteProcedureAsync(sql, BuildParameter(args));
        }
        public async Task<int> ExecuteProcedureAsync(string sql,  DbParameterRegister[] args)
        {
            return await ExecuteAsync(sql, CommandType.StoredProcedure, args);
        }

#endif

        #endregion

        #region ExecuteScalar
        public T ExecuteScalar<T>(string sql, params object[] args)
        {
            return ExecuteScalar<T>(sql, BuildParameter(args));
        }
        public T ExecuteScalar<T>(string sql, DbParameterRegister[] args)
        {
            return ExecuteScalar<T>(sql, CommandType.Text, args);
        }
#if Net45
        public async Task<T> ExecuteScalarAsync<T>(string sql, params object[] args)
        {
            return await ExecuteScalarAsync<T>(sql, BuildParameter(args));
        }
        public async Task<T> ExecuteScalarAsync<T>(string sql, DbParameterRegister[] args)
        {
            return await ExecuteScalarAsync<T>(sql, CommandType.Text, args);
        }

#endif

        #region Procedure
        public T ExecuteScalarProcedure<T>(string sql, params object[] args)
        {
            return ExecuteScalarProcedure<T>(sql, BuildParameter(args));
        }
        public T ExecuteScalarProcedure<T>(string sql, DbParameterRegister[] args)
        {
            return ExecuteScalar<T>(sql, CommandType.StoredProcedure, args);
        }
        #endregion
#if Net45
        public async Task<T> ExecuteProcedureAsync<T>(string sql, params object[] args)
        {
            return await ExecuteProcedureAsync<T>(sql, BuildParameter(args));
        }
        public async Task<T> ExecuteProcedureAsync<T>(string sql, DbParameterRegister[] args)
        {
            return await ExecuteScalarAsync<T>(sql, CommandType.StoredProcedure, args);
        }

#endif
        #endregion

        #region Fetch
        public List<T> Fetch<T>(string sql, params object[] args)
        {
            return Query<T>(sql, args).ToList();
        }

        public List<T> Fetch<T>(string sql,  DbParameterRegister[] args)
        {
            return Query<T>(sql, args).ToList();
        }
#if Net45
        public async Task<List<T>> FetchAsync<T>(string sql, params object[] args)
        {
            return await FetchAsync<T>(sql, BuildParameter(args));
        }

        public async Task<List<T>> FetchAsync<T>(string sql,  DbParameterRegister[] args)
        {
            return await Task.Run(() => Fetch<T>(sql, args));
        }

        #region Procedure
        /// <summary>
        /// 查询存储过程
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="procedureName"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public List<T> FetchProcedure<T>(string procedureName,  DbParameterRegister[] args)
        {
            return QueryProcedure<T>(procedureName, args).ToList();
        }
#if Net45
        /// <summary>
        /// 查询存储过程
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="procedureName"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public async Task<List<T>> FetchProcedureAsync<T>(string procedureName,  DbParameterRegister[] args)
        {
            return await Task.Run(() => FetchProcedure<T>(procedureName, args));
        }

#endif

        #endregion

#endif
        #endregion

        /// <summary>
        /// 捕获错误,返回一个值,是否继续
        /// </summary>
        /// <param name="ex"></param>
        /// <returns>true-忽略错误,继续执行;false-抛出异常</returns>
        public virtual bool OnException(Exception ex)
        {
            return true;
        }

        /// <summary>
        /// 当启动事务的时候触发
        /// </summary>
        public virtual void OnBeginTransaction()
        {

        }

        /// <summary>
        /// 当结束事务的时候触发
        /// </summary>
        public virtual void OnEndTransaction()
        {

        }

        #region PrivateMethod
        #region Do

        IEnumerable<T> Query<T>(string sql, CommandType type, DbParameterRegister[] args)
        {
            var connection = GetConnection();
            using (var command = connection.CreateCommand())
            {
                command.CommandType = type;
                if (type == CommandType.StoredProcedure)
                {
                    command.CommandText = sql;
                }
                else
                {
                    command.CommandText = BuildSql<T>(sql);
                }
                var hasOutput = BuildParameter(command, args);
                OpenConnection(connection);
                using (var reader = command.ExecuteReader())
                {
                    if (!reader.HasRows)
                    {
                        if (hasOutput)
                        {
                            SetOutputValue(command, args);
                        }
                        CloseConnection(connection);
                        yield break;
                    }
                    var factory = MapperFactory.GetFactory<T>(reader) as Func<IDataReader, T>;
                    while (reader.Read())
                    {
                        T obj = default(T);
                        try
                        {
                            obj = factory(reader);
                        }
                        catch (Exception ex)
                        {
                            if (OnException(ex))
                            {
                                continue;
                            }
                            CloseConnection(connection);
                            throw;
                        }
                        yield return obj;
                    }
                    if (hasOutput)
                    {
                        SetOutputValue(command, args);
                    }
                }
            }
            CloseConnection(connection);
            yield break;
        }
        int Execute(string sql, CommandType type, DbParameterRegister[] args)
        {
            var connection = GetConnection();
            using (var command = connection.CreateCommand())
            {
                command.CommandType = type;
                command.CommandText = sql;
                var hasOutput = BuildParameter(command, args);
                OpenConnection(connection);
                try
                {
                    return command.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    if (!OnException(ex))
                    {
                        throw;
                    }
                    return 0;
                }
                finally
                {
                    if (hasOutput)
                    {
                        SetOutputValue(command, args);
                    }
                    CloseConnection(connection);
                }
            }
        }

        T ExecuteScalar<T>(string sql, CommandType type, DbParameterRegister[] args)
        {
            var connection = GetConnection();
            using (var command = connection.CreateCommand())
            {
                command.CommandType = type;
                command.CommandText = sql;
                var hasOutput = BuildParameter(command, args);
                OpenConnection(connection);
                try
                {
                    var result = command.ExecuteScalar();
                    if (result == null || result == DBNull.Value)
                    {
                        return default(T);
                    }
                    return (T)result;
                }
                catch (Exception ex)
                {
                    if (!OnException(ex))
                    {
                        throw;
                    }
                    return default(T);
                }
                finally
                {
                    if (hasOutput)
                    {
                        SetOutputValue(command, args);
                    }
                    CloseConnection(connection);
                }
            }
        }

        async Task<int> ExecuteAsync(string sql, CommandType type, DbParameterRegister[] args)
        {
            var connection = GetConnection();
            using (var command = connection.CreateCommand())
            {
                command.CommandType = type;
                command.CommandText = sql;
                var hasOutput = BuildParameter(command, args);
                await OpenConnectionAsync(connection);
                try
                {
                    return await command.ExecuteNonQueryAsync();
                }
                catch (Exception ex)
                {
                    if (!OnException(ex))
                    {
                        throw;
                    }
                    return 0;
                }
                finally
                {
                    if (hasOutput)
                    {
                        SetOutputValue(command, args);
                    }
                    CloseConnection(connection);
                }
            }
        }

        async Task<T> ExecuteScalarAsync<T>(string sql, CommandType type, DbParameterRegister[] args)
        {
            var connection = GetConnection();
            using (var command = connection.CreateCommand())
            {
                command.CommandType = type;
                command.CommandText = sql;
                var hasOutput = BuildParameter(command, args);
                await OpenConnectionAsync(connection);
                try
                {
                    var result = await command.ExecuteScalarAsync();
                    if (result == null || result == DBNull.Value)
                    {
                        return default(T);
                    }
                    return (T)result;
                }
                catch (Exception ex)
                {
                    if (!OnException(ex))
                    {
                        throw;
                    }
                    return default(T);
                }
                finally
                {
                    if (hasOutput)
                    {
                        SetOutputValue(command, args);
                    }
                    CloseConnection(connection);
                }
            }
        }

        #endregion


        string BuildSql<T>(string sql)
        {
            Type type = typeof(T);
            if (string.IsNullOrEmpty(sql))
            {
                return ColumnsCache.BuildSelectColumns(type);
            }

            if (sql.TrimStart().StartsWith("where", StringComparison.OrdinalIgnoreCase))
            {
                return "Select " + ColumnsCache.GetColumns(type) + " From " + type.Name;
            }
            return sql;
        }
        void BuildParameter(IDbCommand command, object[] args)
        {
            if (args == null || args.Length == 0)
            {
                return;
            }
            int index = 0;
            foreach (object arg in args)
            {
                var parameter = command.CreateParameter();
                parameter.ParameterName = (index++).ToString();
                parameter.Value = arg;
                command.Parameters.Add(parameter);
            }

        }

        bool BuildParameter(IDbCommand command, DbParameterRegister[] args)
        {
            if (args == null || args.Length == 0)
            {
                return false;
            }
            bool hasOutput = false;
            foreach (var arg in args)
            {
                var parameter = command.CreateParameter();
                parameter.ParameterName = arg.Name;
                parameter.Value = arg.Value;
                if (arg.Output)
                {
                    parameter.Direction = ParameterDirection.Output;
                }
                command.Parameters.Add(parameter);
            }

            return hasOutput;
        }

        DbParameterRegister[] BuildParameter(object[] args)
        {
            if (args == null || args.Length == 0)
            {
                return null;
            }
            int index = 0;
            return args.Select(o => DbParameterRegister.Create((index++).ToString(), o)).ToArray();
        }

        void SetOutputValue(IDbCommand command, DbParameterRegister[] args)
        {
            foreach (var item in args.Where(o => o.Output))
            {
                item.Value = command.Parameters.Cast<IDbDataParameter>().FirstOrDefault(o => o.ParameterName == item.Name);
            }
        }

        DbConnection GetConnection()
        {
            if (_connection != null)
            {
                if (_connection.State == (ConnectionState.Connecting | ConnectionState.Open | ConnectionState.Broken))
                {
                    _connection.Close();
                    return _connection;
                }
                return _connection;
            }
            _connection = _factory.CreateConnection();
            _connection.ConnectionString = this._connectionString;
            return _connection;
        }
        #endregion

        bool _disposed;

        #region OpenAndCloseConnection

        void OpenConnection(DbConnection connection)
        {
            if (connection.State == ConnectionState.Open)
            {
                return;
            }
            connection.Open();
        }

        async Task OpenConnectionAsync(DbConnection connection)
        {
            if (connection.State == ConnectionState.Open)
            {
                return;
            }
            await connection.OpenAsync();
        }

        void CloseConnection(DbConnection connection)
        {
            if (_transaction != null)
            {
                return;
            }
            if (connection.State != ConnectionState.Closed)
            {
                connection.Close();
            }
        }
        #endregion

        public void Dispose()
        {
            if (_transaction != null)
            {
                _transaction.Rollback();
                _transaction.Dispose();
                _transaction = null;
            }
            if (_connection != null)
            {
                if (_connection.State != ConnectionState.Closed)
                {
                    _connection.Close();
                }
                _connection.Dispose();
                _connection = null;
            }
            _disposed = true;
        }


        #region Transaction

        /// <summary>
        /// 启动事务
        /// </summary>
        public void BeginTransaction()
        {
            if (_transaction != null)
            {
                _transaction.Dispose();
            }

            var connection = GetConnection();

            OpenConnection(connection);

            _transaction = new TransactionUnit(connection);
            OnBeginTransaction();
        }

        /// <summary>
        /// 启动事务
        /// </summary>
        public void BeginTransaction(IsolationLevel level)
        {
            if (_transaction != null)
            {
                _transaction.Dispose();
            }
            var connection = GetConnection();
            OpenConnection(connection);
            _transaction = new TransactionUnit(connection, level);
            OnBeginTransaction();
        }

        public void CompleteTransaction()
        {
            if (_transaction != null)
            {
                _transaction.Commit();
                _transaction = null;
            }
        }
        public void AbortTransaction()
        {
            if (_transaction != null)
            {
                _transaction.Rollback();
                _transaction = null;
            }
            OnEndTransaction();
        }

        /// <summary>
        /// 事务单元
        /// </summary>
        class TransactionUnit : IDisposable
        {
            public TransactionUnit(DbConnection connection)
            {
                _connection = connection;
                Init();
            }

            public TransactionUnit(DbConnection connection, IsolationLevel level)
            {
                _connection = connection;
                Init(level);
            }

            void Init()
            {
                _transaction = _connection.BeginTransaction();
            }
            void Init(IsolationLevel level)
            {
                _transaction = _connection.BeginTransaction(level);
            }

            DbConnection _connection;

            DbTransaction _transaction;

            public void Commit()
            {
                _transaction.Commit();
                _disposed = true;
            }

            public void Dispose()
            {
                if (!_disposed)
                {
                    Rollback();
                }
            }

            bool _disposed;

            public void Rollback()
            {
                _transaction.Rollback();
                _disposed = true;
            }
        }



        #endregion




    }
}
