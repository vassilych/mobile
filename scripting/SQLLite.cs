using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using Mono.Data.Sqlite;

namespace SplitAndMerge
{
    public class SQLLite
    {
        public static void Init()
        {
            //ParserFunction.RegisterFunction("SQLTableColumns", new SQLColumnsFunction());
            ParserFunction.RegisterFunction("SQLInit", new SqlLiteFunction(true));
            ParserFunction.RegisterFunction("SQLDBExists", new SqlLiteFunction(false));
            ParserFunction.RegisterFunction("SQLQuery", new SqlLiteQueryFunction());
            ParserFunction.RegisterFunction("SQLNonQuery", new SqlLiteNonQueryFunction());
            ParserFunction.RegisterFunction("SQLInsert", new SqlLiteInsertFunction());
        }
    }

    public class SqlLiteFunction : ParserFunction
    {
        public static string DBPath { get; set; }
        bool m_createSQL;

        public SqlLiteFunction(bool createSQL)
        {
            m_createSQL = createSQL;
        }

        protected override Variable Evaluate(ParsingScript script)
        {
            List<Variable> args = script.GetFunctionArgs();
            Utils.CheckArgs(args.Count, 1, m_name);

            string dbName = Utils.GetSafeString(args, 0);
            string dbPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.Personal),
                dbName);

            bool exists = File.Exists(dbPath);

            DBPath = dbPath;
            try
            {
                if (!exists && m_createSQL)
                {
                    SqliteConnection.CreateFile(dbPath);
                }

            }
            catch (Exception exc)
            {
                throw new ArgumentException("Couldn't create DB " + dbPath +
                                            " :" + exc.Message);
            }
            return new Variable(exists);
        }
    }

    public class SqlLiteNonQueryFunction : ParserFunction
    {
        protected override Variable Evaluate(ParsingScript script)
        {
            List<Variable> args = script.GetFunctionArgs();
            Utils.CheckArgs(args.Count, 1, m_name);

            if (string.IsNullOrWhiteSpace(SqlLiteFunction.DBPath))
            {
                throw new ArgumentException("DB has not been initialized.");
            }

            string query = Utils.GetSafeString(args, 0);
            int rowCount = 0;
            try
            {
                using (var connection = new SqliteConnection("Data Source=" + SqlLiteFunction.DBPath))
                {
                    connection.Open();
                    using (var cmd = connection.CreateCommand())
                    {
                        cmd.CommandText = query;
                        rowCount = cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception exc)
            {
                throw new ArgumentException("Couldn't run [" + query + "] on DB " + SqlLiteFunction.DBPath +
                                            " :" + exc.Message);
            }
            return new Variable(rowCount);
        }
    }

    public class SqlLiteQueryFunction : ParserFunction
    {
        static Dictionary<string, Dictionary<string, DbType>> s_columns = new Dictionary<string, Dictionary<string, DbType>>();

        protected override Variable Evaluate(ParsingScript script)
        {
            List<Variable> args = script.GetFunctionArgs();
            Utils.CheckArgs(args.Count, 1, m_name);

            if (string.IsNullOrWhiteSpace(SqlLiteFunction.DBPath))
            {
                throw new ArgumentException("DB has not been initialized.");
            }

            string query = Utils.GetSafeString(args, 0);
            string tableName = Utils.GetSafeString(args, 1);

            return GetData(query, tableName);
        }

        public static Dictionary<string, DbType> GetColumnData(string tableName)
        {
            Dictionary<string, DbType> tableData = null;
            if (s_columns.TryGetValue(tableName, out tableData))
            {
                return tableData;
            }

            var query = "select * from " + tableName + " where 1 = 2";
            GetData(query, tableName);
            if (s_columns.TryGetValue(tableName, out tableData))
            {
                return tableData;
            }

            return null;
        }

        public static Variable GetData(string query, string tableName = "")
        {
            DataTable table = new DataTable("results");
            Variable results = new Variable(Variable.VarType.ARRAY);
            try
            {
                using (var connection = new SqliteConnection("Data Source=" + SqlLiteFunction.DBPath))
                {
                    connection.Open();
                    using (var cmd = new SqliteCommand(query, connection))
                    {
                        SqliteDataAdapter dap = new SqliteDataAdapter(cmd);
                        dap.Fill(table);
                    }
                }
            }
            catch (Exception exc)
            {
                throw new ArgumentException("Couldn't run [" + query + "] on DB " + SqlLiteFunction.DBPath +
                                            " :" + exc.Message);
            }

            Dictionary<string, DbType> tableData = new Dictionary<string, DbType>();
            Variable headerRow = new Variable(Variable.VarType.ARRAY);
            for (int i = 0; i < table.Columns.Count; i++)
            {
                DataColumn col = table.Columns[i];
                headerRow.AddVariable(new Variable(col.ColumnName));
                if (!string.IsNullOrWhiteSpace(tableName))
                {
                    tableData[col.ColumnName.ToLower()] = StringToSqlDbType(col.DataType.Name);
                }
            }
            results.AddVariable(headerRow);

            if (!string.IsNullOrWhiteSpace(tableName))
            {
                s_columns[tableName] = tableData;
            }

            foreach (var rowObj in table.Rows)
            {
                DataRow row = rowObj as DataRow;
                Variable rowVar = new Variable(Variable.VarType.ARRAY);
                int i = 0;
                foreach (var item in row.ItemArray)
                {
                    DataColumn col = table.Columns[i++];
                    switch (col.DataType.Name)
                    {
                        case "Int32":
                            rowVar.AddVariable(new Variable((int)item));
                            break;
                        case "Int64":
                            rowVar.AddVariable(new Variable((long)item));
                            break;
                        case "Boolean":
                            rowVar.AddVariable(new Variable((bool)item));
                            break;
                        case "Single":
                            rowVar.AddVariable(new Variable((float)item));
                            break;
                        case "Double":
                            rowVar.AddVariable(new Variable((double)item));
                            break;
                        case "String":
                            rowVar.AddVariable(new Variable((string)item));
                            break;
                        case "DateTime":
                            rowVar.AddVariable(new Variable((DateTime)item));
                            break;
                        default:
                            throw new ArgumentException("Unknown type: " + col.DataType.Name);
                    }
                }

                results.AddVariable(rowVar);
            }

            return results;
        }

        public static DbType StringToSqlDbType(string strType)
        {
            switch (strType)
            {
                case "Int16": return DbType.Int16;
                case "Int32": return DbType.Int32;
                case "Int64": return DbType.Int64;
                case "String": return DbType.String;
                case "Double": return DbType.Double;
                case "Byte": return DbType.Byte;
                case "Boolean": return DbType.Boolean;
                case "DateTime": return DbType.DateTime;
                case "Single": return DbType.Single;
                case "Binary": return DbType.Binary;
                default:
                    throw new ArgumentException("Unknown type: " + strType);
            }
        }

        public static object SqlDbTypeToType(DbType dbType, Variable var)
        {
            switch (dbType)
            {
                case DbType.Int16:
                case DbType.Int32:
                case DbType.Int64:    return var.AsInt();
                case DbType.Double:
                case DbType.Single:   return var.AsDouble();
                case DbType.String:   return var.AsString();
                case DbType.Byte:
                case DbType.Boolean:  return var.AsBool();
                case DbType.DateTime: return var.AsDateTime();
            }
            return var.AsString();
        }
    }

    public class SqlLiteInsertFunction : ParserFunction
    {
        protected override Variable Evaluate(ParsingScript script)
        {
            List<Variable> args = script.GetFunctionArgs();
            Utils.CheckArgs(args.Count, 3, m_name);

            var tableName = Utils.GetSafeString(args, 0).Trim();
            var colsStr = Utils.GetSafeString(args, 1).Trim().ToLower();

            if (string.IsNullOrWhiteSpace(SqlLiteFunction.DBPath))
            {
                throw new ArgumentException("DB has not been initialized.");
            }

            var colData = SqlLiteQueryFunction.GetColumnData(tableName);
            if (colData == null || colData.Count == 0)
            {
                return new Variable("Error: table [" + tableName + "] doesn't exist.");
            }

            var queryStatement = "INSERT INTO " + tableName + " (" + colsStr + ") VALUES ("; //@a,@b,@c);"
            var cols = colsStr.Split(',');
            for (int i = 0; i < cols.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(cols[i]) || !colData.Keys.Contains(cols[i]))
                {
                    return new Variable("Error: column [" + cols[i] + "] doesn't exist.");
                }
                queryStatement += "@" + cols[i] + ",";
            }
            queryStatement = queryStatement.Remove(queryStatement.Length - 1) + ")"; ;

            var valsVariable = args[2];
            if (valsVariable.Type != Variable.VarType.ARRAY || valsVariable.Tuple.Count < cols.Length)
            {
                return new Variable("Error: not enough values (" + valsVariable.Tuple.Count +
                                    ") given for " + cols.Length + " columns.");
            }

            try
            {
                using (var connection = new SqliteConnection("Data Source=" + SqlLiteFunction.DBPath))
                {
                    connection.Open();
                    using (var cmd = new SqliteCommand(queryStatement, connection))
                    {
                        for (int i = 0; i < cols.Length; i++)
                        {
                            var varName = "@" + cols[i];
                            var varType = colData[cols[i]];
                            cmd.Parameters.Add(varName, varType);
                            cmd.Parameters[varName].Value = SqlLiteQueryFunction.SqlDbTypeToType(varType, valsVariable.Tuple[i]);
                        }

                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception exc)
            {
                throw new ArgumentException("Couldn't run [" + queryStatement + "] on DB " + SqlLiteFunction.DBPath +
                                            " :" + exc.Message);
            }

            return new Variable("Inserted new row.");
        }
    }
}