using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Configuration;
using System.Web.Http;
using System.Web;
using MySql.Data.MySqlClient;
using System.Web.Http.Cors;



namespace WebApplicationFinal.Controllers
{
    public class UserController : ApiController
    {
        fileEntities db = new fileEntities();
        public static MySqlConnection CreateConn()
        {
            string _conn = WebConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
            MySqlConnection conn = new MySqlConnection(_conn);
            return conn;
        }
        [HttpGet]
        public String TestConnection(int id)
        {
            var contex = new fileEntities();
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
        [HttpGet]
        [Route("addFile")]

        public int addFile()
        {
            using (fileEntities entities = new fileEntities())
            {
                var newFile = new file()
                {
                    name = "test",
                    type = 1,
                    url = "test",

                };
                entities.file.Add(newFile);
                entities.SaveChanges();
                var newId = newFile.id;
                Console.WriteLine(newId);
                return newId;
            }

        }

        [HttpPost]
        [Route("postFiles")]
        [EnableCors(origins:"*", headers:"*", methods:"*")]
        public List<String> postFile()
        {
            HttpFileCollection files = HttpContext.Current.Request.Files;
            List<String> filelist = new List<string>();
            foreach (string key in files.AllKeys)
            {
                HttpPostedFile file = files[key];
                if (string.IsNullOrEmpty(file.FileName) == false)
                {
                    filelist.Add(file.FileName);
                }
            }
            var len = filelist.Count;
            if(len < 1)
            {
                filelist.Add("Empty");
            }
            return filelist;
        }
    }
}
