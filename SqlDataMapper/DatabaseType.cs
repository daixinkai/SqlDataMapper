using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlDataMapper
{
    abstract class DatabaseType
    {
        /// <summary>
        /// Returns the prefix used to delimit parameters in SQL query strings.
        /// </summary>
        /// <param name="ConnectionString"></param>
        /// <returns></returns>
        public virtual string GetParameterPrefix(string ConnectionString)
        {
            return "@";
        }

        /// <summary>
        /// Converts a supplied C# object value into a value suitable for passing to the database
        /// </summary>
        /// <param name="value">The value to convert</param>
        /// <returns>The converted value</returns>
        public virtual object MapParameterValue(object value)
        {
            // Cast bools to integer
            if (value.GetType() == typeof(bool))
            {
                return ((bool)value) ? 1 : 0;
            }

            // Leave it
            return value;
        }

        /// <summary>
        /// Called immediately before a command is executed, allowing for modification of the IDbCommand before it's passed to the database provider
        /// </summary>
        /// <param name="cmd"></param>
        public virtual void PreExecute(IDbCommand cmd)
        {
        }

        /// <summary>
        /// Builds an SQL query suitable for performing page based queries to the database
        /// </summary>
        /// <param name="skip">The number of rows that should be skipped by the query</param>
        /// <param name="take">The number of rows that should be retruend by the query</param>
        /// <param name="parts">The original SQL query after being parsed into it's component parts</param>
        /// <param name="args">Arguments to any embedded parameters in the SQL query</param>
        /// <returns>The final SQL query that should be executed.</returns>
        public virtual string BuildPageQuery(long skip, long take, PagingHelper.SQLParts parts, ref object[] args)
        {
            var sql = string.Format("{0}\nLIMIT @{1} OFFSET @{2}", parts.sql, args.Length, args.Length + 1);
            args = args.Concat(new object[] { take, skip }).ToArray();
            return sql;
        }

        /// <summary>
        /// Returns an SQL Statement that can check for the existance of a row in the database.
        /// </summary>
        /// <returns></returns>
        public virtual string GetExistsSql()
        {
            return "SELECT COUNT(1) FROM {0} WHERE {1}";
        }

        /// <summary>
        /// Escape a tablename into a suitable format for the associated database provider.
        /// </summary>
        /// <param name="tableName">The name of the table (as specified by the client program, or as attributes on the associated POCO class.</param>
        /// <returns>The escaped table name</returns>
        public virtual string EscapeTableName(string tableName)
        {
            // Assume table names with "dot" are already escaped
            return tableName.IndexOf('.') >= 0 ? tableName : EscapeSqlIdentifier(tableName);
        }

        /// <summary>
        /// Escape and arbitary SQL identifier into a format suitable for the associated database provider
        /// </summary>
        /// <param name="str">The SQL identifier to be escaped</param>
        /// <returns>The escaped identifier</returns>
        public virtual string EscapeSqlIdentifier(string str)
        {
            return string.Format("[{0}]", str);
        }

        ///// <summary>
        ///// Return an SQL expression that can be used to populate the primary key column of an auto-increment column.
        ///// </summary>
        ///// <param name="ti">Table info describing the table</param>
        ///// <returns>An SQL expressions</returns>
        ///// <remarks>See the Oracle database type for an example of how this method is used.</remarks>
        //public virtual string GetAutoIncrementExpression(TableInfo ti)
        //{
        //    return null;
        //}

        /// <summary>
        /// Returns an SQL expression that can be used to specify the return value of auto incremented columns.
        /// </summary>
        /// <param name="primaryKeyName">The primary key of the row being inserted.</param>
        /// <returns>An expression describing how to return the new primary key value</returns>
        /// <remarks>See the SQLServer database provider for an example of how this method is used.</remarks>
        public virtual string GetInsertOutputClause(string primaryKeyName)
        {
            return string.Empty;
        }

        public virtual void BuildInsert(IDbCommand cmd)
        {
            cmd.CommandText += ";\nSELECT @@IDENTITY AS NewID;";
        }


        /// <summary>
        /// Look at the type and provider name being used and instantiate a suitable DatabaseType instance.
        /// </summary>
        /// <param name="TypeName"></param>
        /// <param name="ProviderName"></param>
        /// <returns></returns>
        public static DatabaseType Resolve(string TypeName, string ProviderName)
        {
            // Try using type name first (more reliable)
            if (TypeName.StartsWith("MySql"))
                return Singleton<MySqlDatabaseType>.Instance;
            if (TypeName.StartsWith("SqlCe"))
                return Singleton<SqlServerCEDatabaseType>.Instance;
            if (TypeName.StartsWith("Npgsql") || TypeName.StartsWith("PgSql"))
                return Singleton<PostgreSQLDatabaseType>.Instance;
            if (TypeName.StartsWith("Oracle"))
                return Singleton<OracleDatabaseType>.Instance;
            if (TypeName.StartsWith("SQLite"))
                return Singleton<SQLiteDatabaseType>.Instance;
            if (TypeName.StartsWith("System.Data.SqlClient."))
                return Singleton<SqlServerDatabaseType>.Instance;

            // Try again with provider name
            if (ProviderName.IndexOf("MySql", StringComparison.InvariantCultureIgnoreCase) >= 0)
                return Singleton<MySqlDatabaseType>.Instance;
            if (ProviderName.IndexOf("SqlServerCe", StringComparison.InvariantCultureIgnoreCase) >= 0)
                return Singleton<SqlServerCEDatabaseType>.Instance;
            if (ProviderName.IndexOf("pgsql", StringComparison.InvariantCultureIgnoreCase) >= 0)
                return Singleton<PostgreSQLDatabaseType>.Instance;
            if (ProviderName.IndexOf("Oracle", StringComparison.InvariantCultureIgnoreCase) >= 0)
                return Singleton<OracleDatabaseType>.Instance;
            if (ProviderName.IndexOf("SQLite", StringComparison.InvariantCultureIgnoreCase) >= 0)
                return Singleton<SQLiteDatabaseType>.Instance;

            // Assume SQL Server
            return Singleton<SqlServerDatabaseType>.Instance;
        }

    }


    class MySqlDatabaseType : DatabaseType
    {
        public override string GetParameterPrefix(string ConnectionString)
        {
            if (ConnectionString != null && ConnectionString.IndexOf("Allow User Variables=true") >= 0)
                return "?";
            else
                return "@";
        }

        public override string EscapeSqlIdentifier(string str)
        {
            return string.Format("`{0}`", str);
        }

        public override string GetExistsSql()
        {
            return "SELECT EXISTS (SELECT 1 FROM {0} WHERE {1})";
        }
    }

    class OracleDatabaseType : DatabaseType
    {
        public override string GetParameterPrefix(string ConnectionString)
        {
            return ":";
        }

        public override void PreExecute(IDbCommand cmd)
        {
            cmd.GetType().GetProperty("BindByName").SetValue(cmd, true, null);
        }

        public override string BuildPageQuery(long skip, long take, PagingHelper.SQLParts parts, ref object[] args)
        {
            if (parts.sqlSelectRemoved.StartsWith("*"))
                throw new Exception("Query must alias '*' when performing a paged query.\neg. select t.* from table t order by t.id");

            // Same deal as SQL Server
            return Singleton<SqlServerDatabaseType>.Instance.BuildPageQuery(skip, take, parts, ref args);
        }

        public override string EscapeSqlIdentifier(string str)
        {
            return string.Format("\"{0}\"", str.ToUpperInvariant());
        }

        //public override void ExecuteInsert( IDbCommand cmd, string PrimaryKeyName)
        //{
        //    if (PrimaryKeyName != null)
        //    {
        //        cmd.CommandText += string.Format(" returning {0} into :newid", EscapeSqlIdentifier(PrimaryKeyName));
        //        var param = cmd.CreateParameter();
        //        param.ParameterName = ":newid";
        //        param.Value = DBNull.Value;
        //        param.Direction = ParameterDirection.ReturnValue;
        //        param.DbType = DbType.Int64;
        //        cmd.Parameters.Add(param);
        //    }
        //    else
        //    {
        //        db.ExecuteNonQueryHelper(cmd);
        //        return -1;
        //    }
        //}

    }

    class PostgreSQLDatabaseType : DatabaseType
    {
        public override object MapParameterValue(object value)
        {
            // Don't map bools to ints in PostgreSQL
            if (value.GetType() == typeof(bool))
                return value;

            return base.MapParameterValue(value);
        }

        public override string EscapeSqlIdentifier(string str)
        {
            return string.Format("\"{0}\"", str);
        }

        //public override object ExecuteInsert(Database db, System.Data.IDbCommand cmd, string PrimaryKeyName)
        //{
        //    if (PrimaryKeyName != null)
        //    {
        //        cmd.CommandText += string.Format("returning {0} as NewID", EscapeSqlIdentifier(PrimaryKeyName));
        //        return db.ExecuteScalarHelper(cmd);
        //    }
        //    else
        //    {
        //        db.ExecuteNonQueryHelper(cmd);
        //        return -1;
        //    }
        //}
    }

    class SQLiteDatabaseType : DatabaseType
    {
        public override object MapParameterValue(object value)
        {
            if (value.GetType() == typeof(uint))
                return (long)((uint)value);

            return base.MapParameterValue(value);
        }

        //public override object ExecuteInsert(Database db, System.Data.IDbCommand cmd, string PrimaryKeyName)
        //{
        //    if (PrimaryKeyName != null)
        //    {
        //        cmd.CommandText += ";\nSELECT last_insert_rowid();";
        //        return db.ExecuteScalarHelper(cmd);
        //    }
        //    else
        //    {
        //        db.ExecuteNonQueryHelper(cmd);
        //        return -1;
        //    }
        //}

        public override string GetExistsSql()
        {
            return "SELECT EXISTS (SELECT 1 FROM {0} WHERE {1})";
        }

    }

    class SqlServerCEDatabaseType : DatabaseType
    {
        public override string BuildPageQuery(long skip, long take, PagingHelper.SQLParts parts, ref object[] args)
        {
            var sqlPage = string.Format("{0}\nOFFSET @{1} ROWS FETCH NEXT @{2} ROWS ONLY", parts.sql, args.Length, args.Length + 1);
            args = args.Concat(new object[] { skip, take }).ToArray();
            return sqlPage;
        }

        //public override object ExecuteInsert(Database db, System.Data.IDbCommand cmd, string PrimaryKeyName)
        //{
        //    db.ExecuteNonQueryHelper(cmd);
        //    return db.ExecuteScalar<object>("SELECT @@@IDENTITY AS NewID;");
        //}

    }

    class SqlServerDatabaseType : DatabaseType
    {
        public override string BuildPageQuery(long skip, long take, PagingHelper.SQLParts parts, ref object[] args)
        {
            parts.sqlSelectRemoved = PagingHelper.rxOrderBy.Replace(parts.sqlSelectRemoved, "", 1);
            if (PagingHelper.rxDistinct.IsMatch(parts.sqlSelectRemoved))
            {
                parts.sqlSelectRemoved = "peta_inner.* FROM (SELECT " + parts.sqlSelectRemoved + ") peta_inner";
            }
            var sqlPage = string.Format("SELECT * FROM (SELECT ROW_NUMBER() OVER ({0}) peta_rn, {1}) peta_paged WHERE peta_rn>@{2} AND peta_rn<=@{3}",
                                    parts.sqlOrderBy == null ? "ORDER BY (SELECT NULL)" : parts.sqlOrderBy, parts.sqlSelectRemoved, args.Length, args.Length + 1);
            args = args.Concat(new object[] { skip, skip + take }).ToArray();

            return sqlPage;
        }

        //public override object ExecuteInsert(Database db, System.Data.IDbCommand cmd, string PrimaryKeyName)
        //{
        //    return db.ExecuteScalarHelper(cmd);
        //}

        public override string GetExistsSql()
        {
            return "IF EXISTS (SELECT 1 FROM {0} WHERE {1}) SELECT 1 ELSE SELECT 0";
        }

        public override string GetInsertOutputClause(string primaryKeyName)
        {
            return String.Format(" OUTPUT INSERTED.[{0}]", primaryKeyName);
        }
    }
}
