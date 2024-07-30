using Models;
using SqlSugar;
using System.Data;
using System.Data.SqlClient;
using System.Linq.Expressions;

namespace KCL
{
    public class DBHelper
    {
        #region ■------------------ 单例

        private DBHelper() { }
        public readonly static DBHelper Instance = new DBHelper();

        #endregion

        #region ■------------------ 初始化

        private string _connString = ""; //数据库连接字符串

        public bool IsInit { get; set; }

        public SqlSugar.DbType MyDbType { get; private set; }

        public bool Init(SqlSugar.DbType dbType, string connString)
        {
            try
            {
                if (!IsInit)
                {
                    if (string.IsNullOrEmpty(connString))
                    {
                        throw new Exception("DBHelper 初始化失败，connString 为空");
                    }
                    MyDbType = dbType;
                    _connString = connString;

                    SqlSugarClient db = new SqlSugarClient(new ConnectionConfig()
                    {
                        ConnectionString = connString,
                        DbType = dbType,
                        IsAutoCloseConnection = true,
                        InitKeyType = InitKeyType.Attribute
                    });
                    //Print sql
                    db.Aop.OnLogExecuting = (sql, pars) =>
                    {
                        Console.WriteLine(sql + "\r\n" + db.Utilities.SerializeObject(pars.ToDictionary(it => it.ParameterName, it => it.Value)));
                        Console.WriteLine();
                    };

                    IsInit = true;
                    return true;
                }
                else
                {
                    throw new Exception("DBHelper 不允许重复初始化");
                }
            }
            catch (Exception ex)
            {
                throw new Exception("DBHelper 初始化失败：" + ex.Message);
            }
        }

        #endregion


        private SqlSugarClient DB
        {
            get
            {
                if (!IsInit)
                {
                    throw new Exception("DBHelper 未初始化");
                }

                SqlSugarClient db = new SqlSugarClient(new ConnectionConfig()
                {
                    ConnectionString = _connString,
                    DbType = MyDbType,
                    IsAutoCloseConnection = true,
                    InitKeyType = InitKeyType.Attribute
                });
                //Print sql
                db.Aop.OnLogExecuting = (sql, pars) =>
                {
                    Console.WriteLine(sql + "\r\n" + db.Utilities.SerializeObject(pars.ToDictionary(it => it.ParameterName, it => it.Value)));
                    Console.WriteLine();
                };
                db.Aop.OnError = (ex) =>
                {
                    throw ex;
                };
                return db;
            }
        }

        public void CreateClassFile()
        {
            //DB.DbFirst.IsCreateAttribute(true).CreateClassFile(@"I:\GitRepository\NET\2302-HaiYaTe\HYT.DataAccess\Models");
        }

        public void CreatTable()
        {
            //DB.CodeFirst.InitTables(typeof(tb_ActionLog));
            //DB.CodeFirst.InitTables(typeof(tb_DBBackup));
            //DB.CodeFirst.InitTables(typeof(tb_Doctor));
            //DB.CodeFirst.InitTables(typeof(tb_Patient));
            //DB.CodeFirst.InitTables(typeof(tb_RecordAssess));
            //DB.CodeFirst.InitTables(typeof(tb_RecordTrain));
            //DB.CodeFirst.InitTables(typeof(tb_Setting));
            //DB.CodeFirst.InitTables(typeof(tb_TrainRank));
            //DB.CodeFirst.InitTables(typeof(tb_Diagnosis));
            //DB.CodeFirst.InitTables(typeof(tb_TrainPlan));
        }

        #region ■------------------ 查询数据

        #region 〓〓〓〓〓〓〓 查询数量

        public int GetCount<T>() where T : new()
        {
            return DB.Queryable<T>().Count();
        }

        /// <summary>
        /// Where(it => it.Id == 1)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="whereString"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public int GetCountWhere<T>(Expression<Func<T, bool>> expression) where T : new()
        {
            return DB.Queryable<T>().Where(expression).Count();
        }

        /// <summary>
        /// Where("id=@id", new { id = 1 })
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="whereString"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public int GetCountWhere<T>(string whereString, object parameters = null) where T : new()
        {
            //Where("id=@id", new { id = 1 })
            return DB.Queryable<T>().Where(whereString, parameters).Count();
        }

        #endregion

        #region 〓〓〓〓〓〓〓 单个查询

        /// <summary>
        /// 通过ID搜索
        /// </summary>
        /// <returns></returns>
        public T GetOneByID<T>(string id) where T : class
        {
            var list = DB.Queryable<T>().Where("ID=@id", new { ID = id }).ToList();
            if (list.Count > 0)
            {
                return list[0];
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 获取最新的
        /// </summary>
        /// <returns></returns>
        public T GetWhereFirst<T>(Expression<Func<T, bool>> expression) where T : class, new()
        {
            return  DB.Queryable<T>(). Where(expression).First();
        }

        #endregion

        #region 〓〓〓〓〓〓〓 集合查询

        public List<T> GetAll<T>(string orderby = "ID asc") where T : new()
        {
            return DB.Queryable<T>().OrderBy(orderby).ToList();
        }

        /// <summary>
        /// 【推荐】分页获取：排序、筛选
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="current"></param>
        /// <param name="pageSize"></param>
        /// <param name="expression"></param>
        /// <param name="count"></param>
        /// <param name="OrderBy">ID DESC</param>
        /// <returns></returns>
        public List<T> GetPageWhere<T>(ref int current, int pageSize, Expression<Func<T, bool>> expression, ref int count, string OrderBy = "ID DESC") where T : new()
        {
            var result = DB.Queryable<T>().Where(expression).OrderBy(OrderBy).ToPageList(current, pageSize, ref count);

            //当前页查不到数据，自动往前一页
            if (count > 0 && result.Count <= 0 && current > 0)
            {
                current--;
                result = DB.Queryable<T>().Where(expression).OrderBy(OrderBy).ToPageList(current, pageSize, ref count);
            }

            return result;
        }

        /// <summary>
        /// 【推荐】分页获取：排序、筛选、忽略列
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="current"></param>
        /// <param name="pageSize"></param>
        /// <param name="expression"></param>
        /// <param name="count"></param>
        /// <param name="OrderBy">ID DESC</param>
        /// <returns></returns>
        public List<T> GetPageWhereIgnore<T>(ref int current, int pageSize, Expression<Func<T, bool>> expression, Expression<Func<T, object>> ignore, ref int count, string OrderBy = "ID DESC") where T : new()
        {
            var result = DB.Queryable<T>().Where(expression).OrderBy(OrderBy).IgnoreColumns(ignore).ToPageList(current, pageSize, ref count);

            //当前页查不到数据，自动往前一页
            if (count > 0 && result.Count <= 0 && current > 0)
            {
                current--;
                result = DB.Queryable<T>().Where(expression).OrderBy(OrderBy).ToPageList(current, pageSize, ref count);
            }

            return result;
        }

        /// <summary>
        /// 【废弃】分页获取 有筛选 无排序 Where("id=@id", new { id = 1 })  不推荐使用
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="current"></param>
        /// <param name="pageSize"></param>
        /// <param name="whereString"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public List<T> GetPageWhere<T>(ref int current, int pageSize, string whereString, object parameters = null) where T : new()
        {
            int count = 0;
            var result = DB.Queryable<T>().Where(whereString, parameters).ToPageList(current, pageSize, ref count);
            //当前页查不到数据，自动往前一页
            if (count > 0 && result.Count <= 0 && current > 0)
            {
                current--;
                result = DB.Queryable<T>().Where(whereString, parameters).ToPageList(current, pageSize, ref count);
            }

            return result;
        }

        /// <summary>
        /// 【废弃】分页获取：sql 语句、进行排序筛选  不推荐使用
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="current"></param>
        /// <param name="pageSize"></param>
        /// <param name="sql"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public List<T> GetPageWhere<T>(ref int current, int pageSize, string sql, ref int count) where T : class, new()
        {
            var result = DB.SqlQueryable<T>(sql).ToPageList(current, pageSize, ref count);

            //有数据，但当前页查不到数据，自动往前一页
            while (count > 0 && result.Count <= 0 && current > 0)
            {
                current--;
                result = DB.SqlQueryable<T>(sql).ToPageList(current, pageSize, ref count);
            }

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="where"></param>
        /// <param name="select"></param>
        /// <returns></returns>
        public List<TResult> WhereOrderSelect<T, TResult>(Expression<Func<T, bool>> where, string order,Expression<Func<T, TResult>> select) where T : new() where TResult : new()
        {
            return DB.Queryable<T>().Where(where).OrderBy(order). Select<TResult>(select).ToList();
        }

        /// <summary>
        /// Where(it => it.Id == 1)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="whereString"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public List<T> GetWhere<T>(Expression<Func<T, bool>> expression, string orderby = "ID asc") where T : new()
        {
            return DB.Queryable<T>().Where(expression).OrderBy(orderby).ToList();
        }

        /// <summary>
        /// Where(it => it.Id == 1)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="whereString"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public List<T> GetWhereOrderTop<T>(Expression<Func<T, bool>> expression, int count, string orderby = "ID asc") where T : new()
        {
            return DB.Queryable<T>().Where(expression).OrderBy(orderby).Take(count).ToList();
        }

        /// <summary>
        /// Where("id=@id", new { id = 1 })
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="whereString"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public List<T> GetWhere<T>(string whereString, object parameters = null) where T : new()
        {
            return DB.Queryable<T>().Where(whereString, parameters).ToList();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <returns></returns>
        public List<T> GetWhere<T>(string sql) where T : class, new()
        {
            return DB.SqlQueryable<T>(sql).ToList();
        }


        public List<T> GetWhere<T>(int current, int pageSize, string OrderBy, Expression<Func<T, bool>> expression, ref int count) where T : new()
        {
            return DB.Queryable<T>().Where(expression).OrderBy(OrderBy).ToPageList(current, pageSize, ref count);
        }

        public class tb_Test
        {
            public int ID { get; set; }
            public string Name { get; set; }
        }

        public List<T> GetWhereOrderIgnoreNew<T>(int count, Expression<Func<T, bool>> expression, Expression<Func<T, object>> ignore, string orderBy = "ID DESC") where T : new()
        {
            if (ignore == null)
            {
                return DB.Queryable<T>().Where(expression).OrderBy(orderBy).Take(count).ToList();
            }
            else
            {
                return DB.Queryable<T>().Where(expression).IgnoreColumns(ignore).OrderBy(orderBy).Take(count).ToList();
            }
        }

        #endregion



        #endregion

        #region ■------------------ 插入数据
        /// <summary>
        /// 插入数据 是否成功 没有Identity
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public bool InsertList<T>(List<T> o) where T : class, new()
        {
            return DB.Insertable<T>(o).ExecuteCommand() > 0;
        }



        /// <summary>
        /// 插入数据 是否成功 没有Identity
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public bool Insert<T>(object o) where T : class, new()
        {
            return DB.Insertable<T>(o).ExecuteCommand() > 0;
        }

        /// <summary>
        /// 插入数据 是否成功 有Identity
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public int InsertReturnIdentity<T>(object o) where T : class, new()
        {
            return DB.Insertable<T>(o).ExecuteReturnIdentity();
        }

        #endregion

        #region ■------------------ 更新数据

        /// <summary>
        /// 更新数据
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public bool Update<T>(object o) where T : class, new()
        {
            return DB.Updateable<T>(o).ExecuteCommand() > 0;
        }

        /// <summary>
        /// 更新数据 指定更新列
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public bool UpdateColumns<T>(object o, params string[] columns) where T : class, new()
        {
            return DB.Updateable<T>(o).UpdateColumns(columns).ExecuteCommand() > 0;
        }

        /// <summary>
        /// 更新数据 指定忽略列
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public bool UpdateIgnores<T>(object o, object columns) where T : class, new()
        {
            return DB.Updateable<T>(o).IgnoreColumns(it => columns).ExecuteCommand() > 0;
        }

        #endregion

        #region ■------------------ 删除数据

        /// <summary>
        /// 通过主键删除
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool DeleteByID<T>(string id) where T : class, new()
        {
            return DB.Deleteable<T>().In(id).ExecuteCommandHasChange();
        }

        /// <summary>
        /// 删除数据
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public bool Delete<T>(object o) where T : class, new()
        {
            return DB.Deleteable<T>(o).ExecuteCommand() > 0;
        }

        /// <summary>
        /// 删除数据 根据表达式
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public bool DeleteWhere<T>(Expression<Func<T, bool>> where) where T : class, new()
        {
            return DB.Deleteable<T>().Where(where).ExecuteCommand() > 0;
        }

        /// <summary>
        /// 删除所有数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="o"></param>
        /// <returns></returns>
        public bool DeleteAll<T>() where T : class, new()
        {
            return DB.Deleteable<T>().ExecuteCommand() > 0;
        }


        #endregion

        #region ■------------------ 备份还原

        /// <summary>
        /// 备份数据库
        /// </summary>
        /// <param name="dbName"></param>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public bool Backup(string filePath)
        {
            SqlConnection connection = new SqlConnection();
            connection.ConnectionString = _connString;
            connection.Open();

            string dbName = connection.Database;

            SqlCommand command = new SqlCommand();
            command.Connection = connection;
            command.CommandText = string.Format("backup database {0} to disk = '{1}'", dbName, filePath);

            try
            {
                command.ExecuteNonQuery();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
            finally
            {
                connection.Close();
            }
        }

        /// <summary>
        /// 还原数据库
        /// </summary>
        /// <param name="dbName"></param>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public bool Restore(string filePath)
        {
            SqlConnection connection = new SqlConnection();

            var startIndex = _connString.IndexOf("Catalog");
            string dbName = _connString.Substring(startIndex + 8);
            var endIndex = dbName.IndexOf(";");
            dbName = dbName.Substring(0, endIndex);
            connection.ConnectionString = _connString.Replace(dbName, "master");
            //connection.ConnectionString = cs;
            connection.Open();


            SqlCommand command = new SqlCommand();
            command.Connection = connection;
            command.CommandText = string.Format("select spid from sysprocesses,sysdatabases where sysprocesses.dbid=sysdatabases.dbid and sysdatabases.Name='{0}'", dbName);

            // 获取当前所有连接进程
            List<short> list = new List<short>();
            try
            {
                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    list.Add(reader.GetInt16(0));
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                return false;
            }
            finally
            {
                connection.Close();
            }

            // 杀死当前所有连接进程

            for (int i = 0; i < list.Count; i++)
            {
                try
                {
                    connection.Open();
                    command = new SqlCommand(string.Format("kill {0}", list[i].ToString()), connection);
                    command.ExecuteNonQuery();
                    connection.Close();
                }
                catch (Exception ex)
                {

                }
                finally
                {
                    connection.Close();
                }
            }



            // 还原数据库
            //connection.ConnectionString = cs;
            connection.Open();
            command.CommandText = string.Format("restore database {0} from disk = '{1}' with replace", dbName, filePath);
            try
            {
                command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                return false;
            }
            finally
            {
                connection.Close();
            }
            return true;
        }

        public void DeleteFile(string file)
        {
            try
            {
                var startIndex = _connString.IndexOf("Catalog");
                string dbName = _connString.Substring(startIndex + 8);
                var endIndex = dbName.IndexOf(";");
                dbName = dbName.Substring(0, endIndex);

                using (var conn = new SqlConnection(_connString.Replace(dbName, "master")))
                {
                    using (var cmd = conn.CreateCommand())
                    {
                        conn.Open();
                        cmd.CommandType = CommandType.Text;
                        cmd.CommandText = "EXEC sp_configure 'show advanced options', 1;RECONFIGURE;EXEC sp_configure 'xp_cmdshell', 1;RECONFIGURE; ";
                        cmd.ExecuteNonQuery();
                        cmd.CommandText = $"exec   master..xp_cmdshell   'del   {file}'  ";
                        cmd.ExecuteNonQuery();
                        cmd.CommandText = $"EXEC sp_configure 'show advanced options', 1;RECONFIGURE;EXEC sp_configure 'xp_cmdshell', 0;RECONFIGURE;-- ";
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }

   



        #endregion
    }
}