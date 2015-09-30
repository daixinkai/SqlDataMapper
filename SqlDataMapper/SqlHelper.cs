//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace SqlDataMapper
//{
//   public static class SqlHelper
//    {
//        public static void Insert<T>( T entity)
//        {
//            var factory = FactoryCache.GetInsertSql(entity);
//            string sql = factory.Item1;
//            var args = factory.Item2.Select(o => SqlParameterRegister.Create(o.Item1, o.Item2)).ToArray();
//            var pid = ExecuteScalar(helper, sql, args);
//            pid = ConvertValue(pid, FactoryCache.GetPKType<T>());
//            var pkFactory = FactoryCache.GetSetPKFactory<T>();
//            pkFactory(entity, pid);
//        }

//        public static T ExecuteScalar<T>(this IHelper helper, string sql, params object[] args)
//        {
//            var val = ExecuteScalar(helper, sql, args);
//            return ConvertValue<T>(val);
//        }

//        public static object ExecuteScalar(this IHelper helper, string sql, params object[] args)
//        {
//            using (var connection = GetConnection())
//            {
//                using (var cmd = connection.CreateCommand())
//                {
//                    cmd.CommandText = BuildSql(sql);
//                    BuildParameter(cmd, args);
//                    connection.Open();
//                    return cmd.ExecuteScalar();
//                }
//            }
//        }

//        public static T ExecuteScalar<T>(this IHelper helper, string sql, SqlParameterRegister[] args)
//        {
//            var val = ExecuteScalar(helper, sql, args);
//            return ConvertValue<T>(val);
//        }

//        public static object ExecuteScalar(this IHelper helper, string sql, SqlParameterRegister[] args)
//        {
//            using (var connection = GetConnection())
//            {
//                using (var cmd = connection.CreateCommand())
//                {
//                    cmd.CommandText = BuildSql(sql);
//                    BuildParameter(cmd, args);
//                    connection.Open();
//                    return cmd.ExecuteScalar();
//                }
//            }
//        }

//        public static int Execute(this IHelper helper, string sql, params object[] args)
//        {
//            using (var connection = GetConnection())
//            {
//                using (var cmd = connection.CreateCommand())
//                {
//                    cmd.CommandText = BuildSql(sql);
//                    BuildParameter(cmd, args);
//                    connection.Open();
//                    return cmd.ExecuteNonQuery();
//                }
//            }
//        }



//        public static int ExecuteProcedure(this IHelper helper, string procName, params object[] args)
//        {
//            using (var connection = GetConnection())
//            {
//                using (var cmd = connection.CreateCommand())
//                {
//                    cmd.CommandText = procName;
//                    cmd.CommandType = CommandType.StoredProcedure;
//                    BuildParameter(cmd, args);
//                    connection.Open();
//                    return cmd.ExecuteNonQuery();
//                }
//            }
//        }

//        public static int ExecuteProcedure(this IHelper helper, string procName, SqlParameterRegister[] args)
//        {
//            using (var connection = GetConnection())
//            {
//                using (var cmd = connection.CreateCommand())
//                {
//                    cmd.CommandText = procName;
//                    cmd.CommandType = CommandType.StoredProcedure;
//                    var hasOutput = BuildParameter(cmd, args);
//                    connection.Open();
//                    int result = cmd.ExecuteNonQuery();
//                    if (hasOutput)
//                    {
//                        foreach (DbParameter parameter in cmd.Parameters)
//                        {
//                            if (parameter.Direction == ParameterDirection.Output)
//                            {
//                                args.FirstOrDefault(o => o.Name == parameter.ParameterName).Value = parameter.Value;
//                            }
//                        }
//                    }
//                    return result;
//                }
//            }
//        }

//        public static IEnumerable<T> Query<T>(this IHelper helper, string sql, params object[] args)
//        {
//            using (var connection = GetConnection())
//            {
//                using (var cmd = connection.CreateCommand())
//                {
//                    cmd.CommandText = BuildSql<T>(sql);
//                    BuildParameter(cmd, args);
//                    connection.Open();
//                    using (var reader = cmd.ExecuteReader())
//                    {
//                        if (!reader.HasRows)
//                        {
//                            yield break;
//                        }
//                        //var factory = FactoryCache.GetFactory<T>(cmd.CommandText, reader) as Func<IDataReader, T>;
//                        var factory = FactoryCache.GetFactory<T>(reader) as Func<IDataReader, T>;
//                        while (reader.Read())
//                        {
//                            yield return factory(reader);
//                        }
//                    }
//                }
//            }
//            yield break;
//        }

//        public static IEnumerable<T> Query<T>(this IHelper helper, string sql, SqlParameterRegister[] args)
//        {
//            using (var connection = GetConnection())
//            {
//                using (var cmd = connection.CreateCommand())
//                {
//                    cmd.CommandText = BuildSql<T>(sql);
//                    var hasOutput = BuildParameter(cmd, args);
//                    connection.Open();
//                    using (var reader = cmd.ExecuteReader())
//                    {
//                        if (reader.HasRows)
//                        {
//                            //var factory = FactoryCache.GetFactory<T>(cmd.CommandText, reader) as Func<IDataReader, T>;
//                            var factory = FactoryCache.GetFactory<T>(reader) as Func<IDataReader, T>;
//                            while (reader.Read())
//                            {
//                                yield return factory(reader);
//                            }
//                        }
//                    }
//                    if (hasOutput)
//                    {
//                        foreach (DbParameter parameter in cmd.Parameters)
//                        {
//                            if (parameter.Direction == ParameterDirection.Output)
//                            {
//                                args.FirstOrDefault(o => o.Name == parameter.ParameterName).Value = parameter.Value;
//                            }
//                        }
//                    }
//                }
//            }
//            yield break;
//        }
//        public static List<T> Fetch<T>(this IHelper helper, string sql, params object[] args)
//        {
//            return Query<T>(helper, sql, args).ToList();
//        }
//        public static List<T> Fetch<T>(this IHelper helper, string sql, SqlParameterRegister[] args)
//        {
//            return Query<T>(helper, sql, args).ToList();
//        }

//        public static T FirstOrDefault<T>(this IHelper helper, string sql, params object[] args)
//        {
//            return Query<T>(helper, sql, args).FirstOrDefault();
//        }

//        public static T FirstOrDefault<T>(this IHelper helper, string sql, SqlParameterRegister[] args)
//        {
//            return Query<T>(helper, sql, args).FirstOrDefault();
//        }

//        public static IEnumerable<T> QueryProcedure<T>(this IHelper helper, string procName, params object[] args)
//        {
//            using (var connection = GetConnection())
//            {
//                using (var cmd = connection.CreateCommand())
//                {
//                    cmd.CommandText = procName;
//                    cmd.CommandType = CommandType.StoredProcedure;
//                    BuildParameter(cmd, args);
//                    connection.Open();
//                    using (var reader = cmd.ExecuteReader())
//                    {
//                        if (reader.HasRows)
//                        {
//                            var factory = FactoryCache.GetFactory<T>(procName, reader) as Func<IDataReader, T>;
//                            while (reader.Read())
//                            {
//                                yield return factory(reader);
//                            }
//                        }
//                    }
//                }
//            }
//            yield break;
//        }

//        public static IEnumerable<T> QueryProcedure<T>(this IHelper helper, string procName, SqlParameterRegister[] args)
//        {
//            using (var connection = GetConnection())
//            {
//                using (var cmd = connection.CreateCommand())
//                {
//                    cmd.CommandText = procName;
//                    cmd.CommandType = CommandType.StoredProcedure;
//                    var hasOutput = BuildParameter(cmd, args);
//                    connection.Open();
//                    using (var reader = cmd.ExecuteReader())
//                    {
//                        if (reader.HasRows)
//                        {
//                            var factory = FactoryCache.GetFactory<T>(procName, reader) as Func<IDataReader, T>;
//                            while (reader.Read())
//                            {
//                                yield return factory(reader);
//                            }
//                        }
//                    }
//                    if (hasOutput)
//                    {
//                        foreach (DbParameter parameter in cmd.Parameters)
//                        {
//                            if (parameter.Direction == ParameterDirection.Output)
//                            {
//                                args.FirstOrDefault(o => o.Name == parameter.ParameterName).Value = parameter.Value;
//                            }
//                        }
//                    }
//                }
//            }
//            yield break;
//        }

//        public static List<T> FetchProcedure<T>(this IHelper helper, string sql, params object[] args)
//        {
//            return QueryProcedure<T>(helper, sql, args).ToList();
//        }
//        public static List<T> FetchProcedure<T>(this IHelper helper, string sql, SqlParameterRegister[] args)
//        {
//            return QueryProcedure<T>(helper, sql, args).ToList();
//        }

//        public static IPageResult<T> Page<T>(this IHelper helper, int page, int pagesize, string sql, params object[] args)
//        {
//            sql = BuildSql<T>(sql);
//            string sqlCount, sqlPage;
//            BuildPageQueries<T>((page - 1) * pagesize, pagesize, sql, ref args, out sqlCount, out sqlPage);
//            int total = helper.ExecuteScalar<int>(sqlCount, args);
//            return helper.Query<T>(sqlPage, args).ToPage(page, pagesize, total);
//        }


//        public static bool Update<T>(this IHelper helper, T entity, string tableName, string primaryKeyName, IEnumerable<string> colunms)
//        {
//            var build = GetUpdateString(entity, tableName, primaryKeyName, colunms);
//            if (build == null)
//            {
//                return false;
//            }
//            using (var connection = GetConnection())
//            {
//                using (var cmd = connection.CreateCommand())
//                {
//                    cmd.CommandText = build.Item1;
//                    BuildParameter(cmd, build.Item2);
//                    connection.Open();
//                    return cmd.ExecuteNonQuery() > 0;
//                }
//            }
//        }

//        public static bool Update<T>(this IHelper helper, T entity, string tableName, string primaryKeyName, params string[] colunms)
//        {
//            var build = GetUpdateString(entity, tableName, primaryKeyName, (IEnumerable<string>)colunms);
//            if (build == null)
//            {
//                return false;
//            }
//            using (var connection = GetConnection())
//            {
//                using (var cmd = connection.CreateCommand())
//                {
//                    cmd.CommandText = build.Item1;
//                    BuildParameter(cmd, build.Item2);
//                    connection.Open();
//                    return cmd.ExecuteNonQuery() > 0;
//                }
//            }
//        }

//        public static bool Update<T>(this IHelper helper, T entity, string tableName, string primaryKeyName)
//        {
//            return Update(helper, entity, tableName, primaryKeyName, null);
//        }

//        public static bool Update<T>(this IHelper helper, T entity, string tableName)
//        {
//            return Update(helper, entity, tableName, null, null);
//        }

//        public static bool Update<T>(this IHelper helper, T entity)
//        {
//            return Update(helper, entity, null, null, null);
//        }

//        public static bool Update<T>(this IHelper helper, T entity, IEnumerable<string> colunms)
//        {
//            return Update(helper, entity, null, null, colunms);
//        }


//        #region Async

//        public async static Task InsertAsync<T>(this IHelper helper, T entity)
//        {
//            var factory = FactoryCache.GetInsertSql(entity);
//            string sql = factory.Item1;
//            var args = factory.Item2.Select(o => SqlParameterRegister.Create(o.Item1, o.Item2)).ToArray();
//            //这里是int
//            var pid = await ExecuteScalarAsync(helper, sql, args);
//            pid = ConvertValue(pid, FactoryCache.GetPKType<T>());
//            var pkFactory = FactoryCache.GetSetPKFactory<T>();
//            pkFactory(entity, pid);
//        }

//        public async static Task<T> ExecuteScalarAsync<T>(this IHelper helper, string sql, params object[] args)
//        {
//            var val = await ExecuteScalarAsync(helper, sql, args);
//            return ConvertValue<T>(val);
//        }

//        public async static Task<object> ExecuteScalarAsync(this IHelper helper, string sql, params object[] args)
//        {
//            using (var connection = GetConnection())
//            {
//                using (var cmd = connection.CreateCommand())
//                {
//                    cmd.CommandText = BuildSql(sql);
//                    BuildParameter(cmd, args);
//                    connection.Open();
//                    return await cmd.ExecuteScalarAsync();
//                }
//            }
//        }

//        public async static Task<T> ExecuteScalarAsync<T>(this IHelper helper, string sql, SqlParameterRegister[] args)
//        {
//            var val = await ExecuteScalarAsync(helper, sql, args);
//            return ConvertValue<T>(val);
//        }

//        public async static Task<object> ExecuteScalarAsync(this IHelper helper, string sql, SqlParameterRegister[] args)
//        {
//            using (var connection = GetConnection())
//            {
//                using (var cmd = connection.CreateCommand())
//                {
//                    cmd.CommandText = BuildSql(sql);
//                    BuildParameter(cmd, args);
//                    connection.Open();
//                    return await cmd.ExecuteScalarAsync();
//                }
//            }
//        }

//        public async static Task<int> ExecuteAsync(this IHelper helper, string sql, params object[] args)
//        {
//            using (var connection = GetConnection())
//            {
//                using (var cmd = connection.CreateCommand())
//                {
//                    cmd.CommandText = BuildSql(sql);
//                    BuildParameter(cmd, args);
//                    connection.Open();
//                    return await cmd.ExecuteNonQueryAsync();
//                }
//            }
//        }

//        public async static Task<int> ExecuteProcedureAsync(this IHelper helper, string procName, params object[] args)
//        {
//            using (var connection = GetConnection())
//            {
//                using (var cmd = connection.CreateCommand())
//                {
//                    cmd.CommandText = procName;
//                    cmd.CommandType = CommandType.StoredProcedure;
//                    BuildParameter(cmd, args);
//                    connection.Open();
//                    return await cmd.ExecuteNonQueryAsync();
//                }
//            }
//        }

//        public async static Task<int> ExecuteProcedureAsync(this IHelper helper, string procName, SqlParameterRegister[] args)
//        {
//            using (var connection = GetConnection())
//            {
//                using (var cmd = connection.CreateCommand())
//                {
//                    cmd.CommandText = procName;
//                    cmd.CommandType = CommandType.StoredProcedure;
//                    var hasOutput = BuildParameter(cmd, args);
//                    connection.Open();
//                    int result = await cmd.ExecuteNonQueryAsync();
//                    if (hasOutput)
//                    {
//                        foreach (DbParameter parameter in cmd.Parameters)
//                        {
//                            if (parameter.Direction == ParameterDirection.Output)
//                            {
//                                args.FirstOrDefault(o => o.Name == parameter.ParameterName).Value = parameter.Value;
//                            }
//                        }
//                    }
//                    return result;
//                }
//            }
//        }

//        public async static Task<IEnumerable<T>> QueryAsync<T>(this IHelper helper, string sql, params object[] args)
//        {
//            return await Task.Run(() => Query<T>(helper, sql, args));
//        }

//        public async static Task<IEnumerable<T>> QueryAsync<T>(this IHelper helper, string sql, SqlParameterRegister[] args)
//        {
//            return await Task.Run(() => Query<T>(helper, sql, args));
//        }

//        public async static Task<List<T>> FetchAsync<T>(this IHelper helper, string sql, params object[] args)
//        {
//            return await Task.Run(() => Fetch<T>(helper, sql, args));
//        }
//        public async static Task<List<T>> FetchAsync<T>(this IHelper helper, string sql, SqlParameterRegister[] args)
//        {
//            return await Task.Run(() => Fetch<T>(helper, sql, args));
//        }

//        public async static Task<T> FirstOrDefaultAsync<T>(this IHelper helper, string sql, params object[] args)
//        {
//            return (await QueryAsync<T>(helper, sql, args)).FirstOrDefault();
//        }

//        public async static Task<T> FirstOrDefaultAsync<T>(this IHelper helper, string sql, SqlParameterRegister[] args)
//        {
//            return (await QueryAsync<T>(helper, sql, args)).FirstOrDefault();
//        }

//        public async static Task<IEnumerable<T>> QueryProcedureAsync<T>(this IHelper helper, string sql, params object[] args)
//        {
//            return await Task.Run(() => QueryProcedure<T>(helper, sql, args));
//        }

//        public async static Task<IEnumerable<T>> QueryProcedureAsync<T>(this IHelper helper, string sql, params SqlParameterRegister[] args)
//        {
//            return await Task.Run(() => QueryProcedure<T>(helper, sql, args));
//        }

//        public async static Task<List<T>> FetchProcedureAsync<T>(this IHelper helper, string sql, params object[] args)
//        {
//            return await Task.Run(() => FetchProcedure<T>(helper, sql, args));
//        }
//        public async static Task<List<T>> FetchProcedureAsync<T>(this IHelper helper, string sql, SqlParameterRegister[] args)
//        {
//            return await Task.Run(() => FetchProcedure<T>(helper, sql, args));
//        }

//        public async static Task<IPageResult<T>> PageAsync<T>(this IHelper helper, int page, int pagesize, string sql, params object[] args)
//        {
//            return await Task.Run(() => Page<T>(helper, page, pagesize, sql, args));
//        }

//        public async static Task<bool> UpdateAsync<T>(this IHelper helper, T entity, string tableName, string primaryKeyName, IEnumerable<string> colunms)
//        {
//            return await Task.Run(() => Update(helper, entity, tableName, primaryKeyName, colunms));
//        }

//        public async static Task<bool> UpdateAsync<T>(this IHelper helper, T entity, string tableName, string primaryKeyName, params string[] colunms)
//        {
//            return await Task.Run(() => Update(helper, entity, tableName, primaryKeyName, colunms));
//        }
//        public async static Task<bool> UpdateAsync<T>(this IHelper helper, T entity, string tableName, string primaryKeyName)
//        {
//            return await Task.Run(() => Update(helper, entity, tableName, primaryKeyName));
//        }

//        public async static Task<bool> UpdateAsync<T>(this IHelper helper, T entity, string tableName)
//        {
//            return await Task.Run(() => Update(helper, entity, tableName));
//        }

//        public async static Task<bool> UpdateAsync<T>(this IHelper helper, T entity)
//        {
//            return await Task.Run(() => Update(helper, entity));
//        }

//        public async static Task<bool> UpdateAsync<T>(this IHelper helper, T entity, IEnumerable<string> colunms)
//        {
//            return await Task.Run(() => Update(helper, entity, null, null, colunms));
//        }


//        #endregion
//        #region MyRegion
//        public static async Task<T> GetEntityByPrimaryAsync<T>(this IHelper helper, string primaryName, long primaryValue) where T : new()
//        {
//            return await Task.Run(() => helper.GetEntityByPrimary<T>(primaryName, primaryValue));
//        }

//        public static async Task<int> UpdateEntityAsync<T>(this IHelper helper, Dictionary<string, object> args) where T : new()
//        {
//            return await Task.Run(() => helper.UpdateEntity<T>(args));
//        }
//        #endregion

//        #region PrivateMethod

//        static T ConvertValue<T>(object val)
//        {
//            Type u = Nullable.GetUnderlyingType(typeof(T));
//            if (u != null && val == null)
//                return default(T);
//            return (T)Convert.ChangeType(val, u == null ? typeof(T) : u);
//        }

//        static object ConvertValue(object val, Type type)
//        {
//            Type u = Nullable.GetUnderlyingType(type);
//            if (u != null && val == null)
//                return null;
//            return Convert.ChangeType(val, u == null ? type : u);
//        }

//        static void BuildPageQueries<T>(long skip, long take, string sql, ref object[] args, out string sqlCount, out string sqlPage)
//        {
//            PagingHelper.SQLParts parts;
//            if (!PagingHelper.SplitSQL(sql, out parts))
//                throw new Exception("Unable to parse SQL statement for paged query");
//            sqlPage = BuildPageQuery(skip, take, parts, ref args);
//            sqlCount = parts.sqlCount;
//        }


//        static string BuildSql(string sql)
//        {
//            //sql = sql.Replace("@", "@" + _parameterName);
//            return sql;
//        }
//        static string BuildSql<T>(string sql)
//        {
//            if (sql.Trim().ToLower().StartsWith("where"))
//            {
//                sql = ColumnsCache.BuildSelectColumns<T>() + " " + sql;
//            }
//            //sql = sql.Replace("@", "@" + _parameterName);
//            return sql;
//        }

//        static void BuildParameter(IDbCommand cmd, object[] args)
//        {
//            if (args == null)
//            {
//                return;
//            }
//            int index = 0;
//            foreach (var item in args)
//            {
//                var par = cmd.CreateParameter();
//                //par.ParameterName = _parameterName + index++;
//                par.ParameterName = "" + index++;
//                par.Value = item;
//                if (item == null)
//                {
//                    par.Value = DBNull.Value;
//                }
//                cmd.Parameters.Add(par);
//            }
//        }


//        static bool BuildParameter(IDbCommand cmd, SqlParameterRegister[] args)
//        {
//            if (args == null || args.Length == 0)
//            {
//                return false;
//            }

//            bool b = false;

//            foreach (var item in args)
//            {
//                var par = cmd.CreateParameter();
//                par.ParameterName = item.Name;
//                par.Value = item.Value;
//                if (item.Output)
//                {
//                    par.Direction = ParameterDirection.Output;
//                    b = true;
//                }
//                if (item.Value == null)
//                {
//                    par.Value = DBNull.Value;
//                }
//                cmd.Parameters.Add(par);
//            }

//            return b;
//        }


//        static DbConnection GetConnection()
//        {
//            return new SqlConnection(SQLHelper.strCon);
//        }

//        /// <summary>
//        /// build the page sql
//        /// </summary>
//        /// <param name="skip"></param>
//        /// <param name="take"></param>
//        /// <param name="parts"></param>
//        /// <param name="args"></param>
//        /// <returns></returns>
//        static string BuildPageQuery(long skip, long take, PagingHelper.SQLParts parts, ref object[] args)
//        {
//            parts.sqlSelectRemoved = PagingHelper.rxOrderBy.Replace(parts.sqlSelectRemoved, "", 1);
//            if (PagingHelper.rxDistinct.IsMatch(parts.sqlSelectRemoved))
//            {
//                parts.sqlSelectRemoved = "peta_inner.* FROM (SELECT " + parts.sqlSelectRemoved + ") peta_inner";
//            }
//            var sqlPage = string.Format("SELECT * FROM (SELECT ROW_NUMBER() OVER ({0}) peta_rn, {1}) peta_paged WHERE peta_rn>@{2} AND peta_rn<=@{3}",
//                                    parts.sqlOrderBy == null ? "ORDER BY (SELECT NULL)" : parts.sqlOrderBy, parts.sqlSelectRemoved, args.Length, args.Length + 1);
//            args = args.Concat(new object[] { skip, skip + take }).ToArray();
//            return sqlPage;
//        }


//        static Tuple<string, object[]> GetUpdateString(object entity, string tableName, string primaryKeyName)
//        {
//            Type type = entity.GetType();
//            if (string.IsNullOrEmpty(tableName))
//            {
//                tableName = type.Name;
//            }
//            if (string.IsNullOrEmpty(primaryKeyName))
//            {
//                primaryKeyName = "ID";
//            }
//            StringBuilder sqlBuilder = new StringBuilder();
//            ArrayList args = new ArrayList();
//            int index = 0;
//            PropertyInfo p = null;
//            foreach (var item in type.GetProperties())
//            {
//                if (item.GetCustomAttributes(typeof(ColumnIgnoreAttribute), true).Length > 0)
//                {
//                    continue;
//                }
//                if (item.Name.Equals(primaryKeyName, StringComparison.OrdinalIgnoreCase))
//                {
//                    p = item;
//                    continue;
//                }
//                sqlBuilder.AppendFormat("[{0}]=@{1},", item.Name, (index++));
//                object value = item.GetValue(entity);
//                args.Add(value);
//            }
//            if (p == null || args.Count == 0)
//            {
//                return null;
//            }
//            string sql = string.Format("Update {0} Set {1} Where {2}=@{3}", tableName, sqlBuilder.ToString().TrimEnd(','), p.Name, index);
//            args.Add(p.GetValue(entity));
//            return new Tuple<string, object[]>(sql, args.ToArray());
//        }

//        static Tuple<string, object[]> GetUpdateString(object entity, string tableName, string primaryKeyName, IEnumerable<string> columns)
//        {
//            if (columns == null)
//            {
//                return GetUpdateString(entity, tableName, primaryKeyName);
//            }
//            Type type = entity.GetType();
//            if (string.IsNullOrEmpty(tableName))
//            {
//                tableName = type.Name;
//            }
//            if (string.IsNullOrEmpty(primaryKeyName))
//            {
//                primaryKeyName = "ID";
//            }
//            StringBuilder sqlBuilder = new StringBuilder();
//            ArrayList args = new ArrayList();
//            int index = 0;
//            var ps = type.GetProperties();
//            PropertyInfo p = ps.FirstOrDefault(o => o.Name.Equals(primaryKeyName, StringComparison.OrdinalIgnoreCase));
//            if (p == null)
//            {
//                return null;
//            }

//            foreach (var column in columns)
//            {
//                var item = ps.FirstOrDefault(o => o.Name.Equals(column, StringComparison.OrdinalIgnoreCase));
//                if (item == null)
//                {
//                    continue;
//                }
//                sqlBuilder.AppendFormat("[{0}]=@{1},", item.Name, (index++));
//                object value = item.GetValue(entity);
//                args.Add(value);
//            }

//            string sql = string.Format("Update {0} Set {1} Where {2}=@{3}", tableName, sqlBuilder.ToString().TrimEnd(','), p.Name, index);
//            args.Add(p.GetValue(entity));
//            return new Tuple<string, object[]>(sql, args.ToArray());
//        }

//        #endregion
//    }


//    public class SqlParameterRegister
//    {
//        public string Name { get; set; }

//        public object Value { get; set; }

//        public bool Output { get; set; }


//        public static SqlParameterRegister Create(string name, object value)
//        {
//            return Create(name, value, false);
//        }

//        public static SqlParameterRegister Create(string name, object value, bool outPut)
//        {
//            return new SqlParameterRegister()
//            {
//                Name = name,
//                Output = outPut,
//                Value = value
//            };
//        }

//    }
//}
//}
