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
using WebApplicationFinal.Util;
using System.Threading.Tasks;
using System.Transactions;


namespace WebApplicationFinal.Controllers
{
    [UserFilter]
    public class UserController : ApiController
    {

        FileEntitiesFinal db = new FileEntitiesFinal();
        public static MySqlConnection CreateConn()
        {
            string _conn = WebConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
            MySqlConnection conn = new MySqlConnection(_conn);
            return conn;
        }
        //[HttpGet]
        //public String TestConnection(int id)
        //{
        //    var contex = new FileEntitiesFinal();
        //    MySqlConnection conn = contex.Database.Connection as MySqlConnection;
        //    String order = "select * from user";
        //    String insert = "insert into school  values(1,'tj','sh' )";
        //    MySqlCommand command = new MySqlCommand(order, conn);
        //    conn.Open();
        //    MySqlDataReader reader = command.ExecuteReader();
        //    command.CommandText = insert;
        //    conn.Close();
        //    conn.Open();
        //    command.ExecuteNonQuery();
        //    conn.Clone();
        //    Console.WriteLine(reader);
        //    return reader.ToString();
        //    //return "OK";
        //}

        // Call this funtion to login
        [NoLogin]
        [HttpPost]
        [HttpGet]
        [Route("user/login")]
        public HttpResponseMessage login(int id, String password)
        {
            using (FileEntitiesFinal entity = new FileEntitiesFinal())
            {
                user currentUser = entity.user.Where(f => f.id == id).FirstOrDefault();
                if (currentUser == null)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "User is not registered");
                } else
                {
                    //TODO: Here should encrypt the password and compare!
                    if (password.Equals(currentUser.password))
                    {
                        HttpContext.Current.Session["id"] = currentUser.id;
                        return Request.CreateResponse(HttpStatusCode.OK, "Login successfully");
                    } else
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "User ID or password incorrect");
                    }
                }
                
            }
        }

        [NoLogin]
        [HttpPost]
        [HttpGet]
        public HttpResponseMessage register(int id, int schoolId, string password, string name) 
        {

            //TODO: validation the id format


            // check the exist user here
            if (isUserExist(id))
            {
                Request.CreateResponse(HttpStatusCode.BadRequest, "User Already Exist");
            }
            //TODO: Encrypt the password here
            user newUser = new user()
            {
                id = id,
                password = password,
                name = name,
                school_id = schoolId
            };
            Task task = registerUser(newUser);
            task.Wait();
            return Request.CreateResponse(HttpStatusCode.OK, "OK");
            
           
        }

        // check whether the user existed

        private bool isUserExist(int userId)
        {
            using (FileEntitiesFinal entity = new FileEntitiesFinal())
            {
                user tempUser = entity.user.Where(u => u.id == userId).FirstOrDefault();
                if (tempUser == null)
                {
                    return false;
                } else
                {
                    return true;
                }
            }
        }

        // insert new use transaction
        public async Task registerUser(user newUser)
        {
            using (TransactionScope scope = new TransactionScope())
            {
                using (FileEntitiesFinal entity = new FileEntitiesFinal())
                {
                    entity.user.Add(newUser);
                    entity.SaveChanges();
                }
                scope.Complete();
            }
        }

        
       

       
    }
}
