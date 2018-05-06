using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using System.Threading;

namespace sqlitetest
{
    class Program
    {
        static SQLiteConnection m_dbConnection;
        public static Mutex mutex = new Mutex();
        static void Main(string[] args)
        {
            /*使用SqLite创建数据库，并插入数据。
            SQLiteConnection conn = null;

            string dbPath = "Data Source =" + Environment.CurrentDirectory + "/test.db";
            conn = new SQLiteConnection(dbPath);//创建数据库实例，指定文件位置  
            conn.Open();//打开数据库，若文件不存在会自动创建  

            string sql = "CREATE TABLE IF NOT EXISTS student(id integer, name varchar(20), sex varchar(2));";//建表语句  
            SQLiteCommand cmdCreateTable = new SQLiteCommand(sql, conn);
            cmdCreateTable.ExecuteNonQuery();//如果表不存在，创建数据表  

            SQLiteCommand cmdInsert = new SQLiteCommand(conn);
            cmdInsert.CommandText = "INSERT INTO student VALUES(1, '小张', '男')";//插入几条数据  
            cmdInsert.ExecuteNonQuery();
            cmdInsert.CommandText = "INSERT INTO student VALUES(2, '小毛', '女')";
            cmdInsert.ExecuteNonQuery();
            cmdInsert.CommandText = "INSERT INTO student VALUES(3, '小文', '男')";
            cmdInsert.ExecuteNonQuery();

            conn.Close();
            */
            Thread read = new Thread(new ThreadStart(Read));
            Thread write = new Thread(new ThreadStart(Write));
            write.Start();
            read.Start();
        }
        //连接sqlite数据库
        public static void connectToDatabase()
        {
            string dbPath = "Data Source =" + Environment.CurrentDirectory + "/test.db";
            m_dbConnection = new SQLiteConnection(dbPath);
            m_dbConnection.Open();
        }
        //从数据库中读取数据
        public static void Read()
        {
            Console.WriteLine( "读线程正在等待 the mutex");
            //申请
            mutex.WaitOne();
            Console.WriteLine("读线程申请到 the mutex");
            connectToDatabase();
            string readsql = "select * from student order by id";
            SQLiteCommand readcommand = new SQLiteCommand(readsql, m_dbConnection);
            SQLiteDataReader reader = readcommand.ExecuteReader();
            while (reader.Read())
            {
                //读线程每次从数据库读取一条记录显示到console上
                Console.WriteLine("ID: " + reader["id"] + "\tName: " + reader["name"] + "\tSex: " + reader["sex"]);
                //删除该条记录
                string deletesql = "delete  from student where id=" + reader["id"];
                SQLiteCommand deletecommand = new SQLiteCommand(deletesql, m_dbConnection);
                deletecommand.ExecuteNonQuery();
                Console.WriteLine("已删除ID为：" + reader["id"] + "的记录");
            }
               Console.ReadLine();
               m_dbConnection.Close();
               // 释放
               mutex.ReleaseMutex();
        }
        //向数据库中写入数据
        public static void Write()
        {
            Console.WriteLine("写线程正在等待 the mutex");
            //申请
            mutex.WaitOne();
            Console.WriteLine("写线程申请到 the mutex");
            connectToDatabase();
            SQLiteCommand inscom = new SQLiteCommand(m_dbConnection);
            inscom.CommandText = "INSERT INTO student VALUES(1, '小张', '男')";//插入几条数据  
            inscom.ExecuteNonQuery();
            Console.WriteLine("已写入：1, '小张', '男'");
            inscom.CommandText = "INSERT INTO student VALUES(2, '小毛', '女')";
            inscom.ExecuteNonQuery();
            Console.WriteLine("已写入：2, '小毛', '女'");
            inscom.CommandText = "INSERT INTO student VALUES(3, '小文', '男')";
            inscom.ExecuteNonQuery();
            Console.WriteLine("已写入：3, '小文', '男'");
            Console.WriteLine("数据写入完毕");
            m_dbConnection.Close();
            // 释放
            mutex.ReleaseMutex();
        }
    }
}

