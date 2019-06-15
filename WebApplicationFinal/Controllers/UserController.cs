using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.IO;
using System.Web.Configuration;
using System.Web.Http;
using System.Web;
using MySql.Data.MySqlClient;
using System.Web.Http.Cors;
using WebApplicationFinal.Models;
using System.Web.SessionState;

namespace WebApplicationFinal.Controllers
{
    public class UserController : ApiController
    {

        FileEntitiesFinal db = new FileEntitiesFinal();
        public static MySqlConnection CreateConn()
        {
            string _conn = WebConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
            MySqlConnection conn = new MySqlConnection(_conn);
            return conn;
        }
        [HttpGet]
        public String TestConnection(int id)
        {
            var contex = new FileEntitiesFinal();
            MySqlConnection conn = contex.Database.Connection as MySqlConnection;
            String order = "select * from user";
            String insert = "insert into school  values(1,'tj','sh' )";
            MySqlCommand command = new MySqlCommand(order, conn);
            conn.Open();
            MySqlDataReader reader = command.ExecuteReader();
            command.CommandText = insert;
            conn.Close();
            conn.Open();
            command.ExecuteNonQuery();
            conn.Clone();
            Console.WriteLine(reader);
            return reader.ToString();
            //return "OK";
        }
       

       
    }
}
