using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SqlDataMapper.Tests
{
    [TestClass]
    public class UnitTest1
    {

        ~UnitTest1()
        {
            if (_database != null)
            {
                _database.Dispose();
            }
        }
        Database _database;
        Database Database
        {
            get
            {
                if (_database == null)
                {
                    string connectionString = "server=127.0.0.1;User Id=sa;password=1234;Persist Security Info=True;database=testdatabase;Allow Zero Datetime=True;";
                    string providerName = "MySql.Data.MySqlClient";
                    _database = new Database(connectionString, providerName);
                }
                return _database;
            }
        }

        [TestMethod]
        public void TestMethod1()
        {
            //Database.BeginTransaction();
            var list = Database.Fetch<TestTable>("");
            //Database.CompleteTransaction();
        }


        public class TestTable
        {
            public int ID { get; set; }

            public int State { get; set; }
        }

    }
}
