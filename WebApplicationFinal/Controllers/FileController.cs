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


namespace WebApplicationFinal.Controllers
{
    public class FileController : ApiController
    {
        [HttpPost]
        [Route("uploadFiles")]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public List<String> uploadFile()
        {
            HttpFileCollection files = HttpContext.Current.Request.Files;
            List<String> filelist = new List<string>();

            foreach (string key in files.AllKeys)
            {
                HttpPostedFile file = files[key];
                if (string.IsNullOrEmpty(file.FileName) == false)
                {
                    //  get the file parameter here
                    var request = HttpContext.Current.Request;
                    var type = request.QueryString["type"];
                    var permission = request.QueryString["permission"];
                    DateTime time = DateTime.Now;
                    // change the file to the byte array
                    byte[] data = null;
                    using (var binaryReader = new BinaryReader(file.InputStream))
                    {
                        data = binaryReader.ReadBytes((int)file.InputStream.Length);
                    }
                    
                    // TODO: read the user id from the session
                    var userId = HttpContext.Current.Session["id"];
                    // add a filter to block the unlogin user
                    if (userId == null)
                    {
                        userId = 2;
                    }
                    String fileName = file.FileName;

                    // check whether the file name already exist, if yes, replace a name automatically
                    if (isFileNameExisted(fileName)) {
                        // TODO: call a function and get a new name here
                        fileName = fileName + "_" + 1.ToString();
                    }
                    // call the upload function here and get the return url
                    UploadFile fileManager = new UploadFile();
                    String url = fileManager.uploadByteFile(data, fileName);
                    if (url == "upload failed")
                    {
                        continue;
                    }

                    file newFile = new file()
                    {
                        name = fileName,
                        user_id = (int)userId,
                        url = url, 
                        time = time,
                        download_times=0,
                        cost=0,
                        permission=Convert.ToInt32(permission),
                        status=1,
                        type=Convert.ToInt32(type),
                        size=data.Length/1024
                    };

                    // TODO： call the function to finished
                    Task task = StoreFileToDB(newFile);
                    task.Wait();
                    filelist.Add(file.FileName);
                }
            }
            var len = filelist.Count;
            if (len < 1)
            {
                filelist.Add("Empty");
            }
            return filelist;
        }
        
       
        private bool isFileNameExisted(String name)
        {
            using (FileEntitiesFinal entity = new FileEntitiesFinal())
            {
                file tempFile = entity.file.Where(f => f.name.Equals(name)).FirstOrDefault();
                if (tempFile == null)
                {
                    return false;
                } else
                {
                    return true;
                }
            }
        }
 

      

        // store the file into the database
        public async Task StoreFileToDB(file file)
        {
            // using transaction scope
            using (var scope = new TransactionScope())
            {

                using (FileEntitiesFinal entity = new FileEntitiesFinal())
                {
                    
                    entity.file.Add(file);
                    await entity.SaveChangesAsync();
                    scope.Complete();
                }
            }
        }




        [HttpGet]
        [Route("test")]
        public string test()
        {
            var name = HttpContext.Current.Request.QueryString["name"];
            return name;
        }



        [HttpGet]
        [Route("addFiles")]

        public user_share_file addFiles()
        {
            var newFile = new file()
            {
                name = "test222",
                type = 1,
                url = "test222",

            };

            using (FileEntitiesFinal entities = new FileEntitiesFinal())
            {

                entities.Entry<file>(newFile).State = System.Data.Entity.EntityState.Added;
                entities.SaveChanges();
                entities.Entry(newFile);
                user_share_file newShare = new user_share_file()
                {
                    user_id = 2,
                    file_id = newFile.id,
                    code="1234"
                };
                entities.user_share_file.Add(newShare);
                entities.SaveChanges();


                return newShare;
            }

        }
    }
}
