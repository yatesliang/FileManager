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
using System.Transactions;
using System.Threading.Tasks;
using System.Runtime.ConstrainedExecution;
using FileManager;
using System.Text;
using WebApplicationFinal.Util;
using System.Runtime.InteropServices;
using Newtonsoft.Json;

namespace WebApplicationFinal.Controllers
{
    [UserFilter]
    public class ForumController : ApiController
    {


        [Route("forum/post")]
        [HttpGet]
        [HttpPost]
        public HttpResponseMessage postQuestion()
        {
            Console.WriteLine("in");
            try
            {
                var userId = HttpContext.Current.Session["id"];
                //var questionId = HttpContext.Current.Request.QueryString["questionId"];
                var title = HttpContext.Current.Request.Params["title"];
                var content = HttpContext.Current.Request.Params["content"];
                DateTime time = DateTime.Now;
                file_request newRequest = new file_request()
                {
                    title = (string)title,
                    description = (string)content,
                    post_time = time,
                    status = 1,
                    user_id = (int)userId
                };
                using (FileEntitiesFinal entity = new FileEntitiesFinal())
                {
                    entity.file_request.Add(newRequest);
                    entity.SaveChangesAsync();
                    return Request.CreateResponse(HttpStatusCode.OK, "Success");
                }
            } 
            catch(Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, "Fail to post");
            }
            

        }

        [Route("forum/getQuestionList")]
        [HttpGet]
        [HttpPost]
        public HttpResponseMessage getRequestList()
        {
            using (FileEntitiesFinal entity = new FileEntitiesFinal())
            {
                var questionList = from q in entity.file_request where q.status == 1 select new { q.title, q.user.name,  q.id ,time=q.post_time};

                List < dynamic > result = new List<dynamic>();
                foreach (var line in questionList)
                {
                    result.Add(line);
                }
                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK, result);

                return response;

            }
        }

        [Route("forum/getQuestionDetail")]
        [HttpGet]
        [HttpPost]
        public HttpResponseMessage getDetail(int id)
        {
            using (FileEntitiesFinal entity = new FileEntitiesFinal())
            {
                var question = from q in entity.file_request where q.status == 1 && q.id==id select new { q.title, q.user.name, q.id, time = q.post_time, content=q.description };

                List<dynamic> result = new List<dynamic>();
                foreach (var line in question)
                {
                    result.Add(line);
                }
                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK, result);

                return response;

            }
        }


        [Route("forum/getAnswerList")]
        [HttpGet]
        [HttpPost]
        public HttpResponseMessage getAnswerList(int id)
        {
            using (FileEntitiesFinal entity = new FileEntitiesFinal())
            {
                var answers = from a in entity.answer where a.file_request_id == id select new { a.content, a.user.name};

                List<dynamic> result = new List<dynamic>();
                foreach (var line in answers)
                {
                    result.Add(line);
                }
                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK, result);

                return response;

            }
        }


        [Route("forum/answer")]
        [HttpGet]
        [HttpPost]
        public HttpResponseMessage postAnswer(int questionId)
        {
            try
            {
                var userId = HttpContext.Current.Session["id"];
                //var questionId = HttpContext.Current.Request.QueryString["questionId"];
                //var title = HttpContext.Current.Request.Params["title"];
                var content = HttpContext.Current.Request.Params["content"];
                DateTime time = DateTime.Now;
                answer newAnswer = new answer
                {
                    content = (string)content,
                    user_id = (int)userId,
                    file_request_id = questionId,
                    answer_time = time
                };
                using (FileEntitiesFinal entity = new FileEntitiesFinal())
                {
                    file_request request = entity.file_request.Where(r => r.id == questionId).FirstOrDefault();
                    if (request == null)
                    {
                        return Request.CreateResponse(HttpStatusCode.NotFound, "No such question");
                    }


                    entity.answer.Add(newAnswer);
                    entity.SaveChangesAsync();
                    return Request.CreateResponse(HttpStatusCode.OK, "Success");
                }
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, "Fail to post");
            }


        }




        [Route("forum/getMyQuestion")]
        [HttpGet]
        [HttpPost]
        public HttpResponseMessage getMyQuestion()
        {
            var userId = HttpContext.Current.Session["id"];
            int id = (int)userId;
            using (FileEntitiesFinal entity = new FileEntitiesFinal())
            {
                var question = from q in entity.file_request where q.user_id == id select new { q.title, q.user.name, q.id, time = q.post_time, content = q.description };

                List<dynamic> result = new List<dynamic>();
                foreach (var line in question)
                {
                    result.Add(line);
                }
                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK, result);

                return response;

            }
        }

        [Route("forum/getMyAnswer")]
        [HttpGet]
        [HttpPost]
        public HttpResponseMessage getMyAnswer()
        {
            var userId = HttpContext.Current.Session["id"];
            int id = (int)userId;
            using (FileEntitiesFinal entity = new FileEntitiesFinal())
            {
                var answers = from a in entity.answer where a.user_id == id select new { a.content, title = a.file_request.title, time= a.file_request.post_time,questionId = a.file_request_id };

                List<dynamic> result = new List<dynamic>();
                foreach (var line in answers)
                {
                    result.Add(line);
                }
                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK, result);

                return response;

            }
        }


        [HttpGet]
        [HttpPost]
        [Route("forum/searchQuestion")]
        public HttpResponseMessage searchUserFile(string key)
        {
            using (FileEntitiesFinal entity = new FileEntitiesFinal())
            {
                var question = from q in entity.file_request where q.title.Contains(key) select new { q.title, q.user.name, q.id, time = q.post_time, content = q.description };

                List<dynamic> result = new List<dynamic>();
                foreach (var line in question)
                {
                    result.Add(line);
                }
                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK, result);

                return response;

            }
        }





    }
}