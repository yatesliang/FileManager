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
using RandomCodeLib;

namespace WebApplicationFinal.Controllers
{
    
    [UserFilter]
    public class FileController : ApiController
    {

        [DllImport(@"C:\Users\wrl\Desktop\NET\DLL\FileCheck.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
        public static extern int checkName(string name, ref byte info);
        [DllImport(@"C:\Users\wrl\Desktop\NET\DLL\FileCheck.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
        public static extern int getFileType(string name);


        [HttpPost]
        [Route("file/upload")]
        public List<String> uploadFile()
        {
            HttpFileCollection files = HttpContext.Current.Request.Files;
            List<String> filelist = new List<string>();

            foreach (string key in files.AllKeys)
            {
                HttpPostedFile file = files[key];
                if (string.IsNullOrEmpty(file.FileName) == false)
                {

                    // check file name here
                    byte[] info = new byte[1024];
                    if (checkName(file.FileName, ref info[0]) == -1)
                    {
                        filelist.Add("File \'" + file.FileName + "\' name is not valid");
                        continue;
                    }
                    //  get the file parameter here
                    var request = HttpContext.Current.Request;
                    string type = "";
                    try
                    {
                        type = request.Params["type"];

                    }
                    catch(Exception e)
                    {
                        type = getFileType(file.FileName).ToString();
                    }


                    var permission = request.Params["permission"];
                    DateTime time = DateTime.Now;
                    // change the file to the byte array
                    byte[] data = null;
                    using (var binaryReader = new BinaryReader(file.InputStream))
                    {
                        data = binaryReader.ReadBytes((int)file.InputStream.Length);
                    }
                    
                    //read the user id from the session
                    var userId = HttpContext.Current.Session["id"];
                    // add a filter to block the unlogin user
                    // here is for test
                    //if (userId == null)
                    //{
                    //    userId = 2;
                    //}
                    String fileName = file.FileName;
                    String storeFilename = fileName;

                    fileName = userId.ToString() + "_" + fileName;
                    // check whether the file name already exist, if yes, replace a name automatically
                    if (isFileNameExisted(fileName)) {
                        // TODO: call a function and get a new name here
                        fileName = fileName + "_" + 1.ToString();
                        storeFilename = storeFilename + "_" + 1.ToString();
                    }
                    
                    // call the upload function here and get the return url
                    UploadFile fileManager = new UploadFile();
                    String url = fileManager.uploadByteFile(data, fileName);
                    if (url == "upload failed")
                    {
                        filelist.Add("File \'" + file.FileName + "\' upload fail");
                        continue;
                    }


                    file newFile = new file()
                    {
                        name = storeFilename,
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
                    addUpFileNumbs((int)userId);
                    filelist.Add("File \'"+file.FileName+"\' upload success");
                }
            }
            var len = filelist.Count;
            if (len < 1)
            {
                filelist.Add(" ");
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



        // download file
        [HttpGet]
        [NoLogin]
        [Route("file/download")]
        public String downloadFile(int fileId)
        {
            using(FileEntitiesFinal entity = new FileEntitiesFinal())
            {
                file tempFile = entity.file.Where(f => f.id == fileId).FirstOrDefault();
                if (tempFile == null)
                {
                    return "FileNotExist";
                } else
                {
                    addUpDownloadTimes(tempFile.id);
                    return tempFile.url.ToString()+"?attname=";

                }
            }
        }


        // 使用code快捷下载，不需要验证session
        [HttpGet]
        [HttpPost]
        [NoLogin]
        [Route("file/test")]
        public string testFile(string code)
        {
            FileEntitiesFinal entity = new FileEntitiesFinal();
            user_share_file temp =  entity.user_share_file.Where(f => f.code.Equals(code)).FirstOrDefault();
            return temp.code;
        }

        [HttpGet]
        [HttpPost]
        [NoLogin]
        [Route("file/fastDownload")]
        public HttpResponseMessage downloadByCode(string code)
        {

           file targetFile = getFileUrl(code);
            if (targetFile == null)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound, "No such file");
            }
            //TODO: get the download path
            Download download = new Download();
            string path = "";
            if (targetFile.size > 20 * 1024)
            {
                path = download.multiThreadDownload(targetFile.url, targetFile.name);
            } else
            {
                path = download.singleThreadDownload(targetFile.url, targetFile.name);

            }
            if (path == "")
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, "Fail to download");
            }
            string fileName = Path.GetFileName(path);
            FileStream fileStream = new FileStream(path, FileMode.Open);
            HttpResponseMessage response = null;
            try
            {
                response = Request.CreateResponse(HttpStatusCode.OK);
                response.Content = new StreamContent(fileStream);
                response.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");
                response.Content.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment")
                {
                    FileName = HttpUtility.UrlEncode(fileName)
                };
                response.Headers.Add("Access-Control-Expose-Header", "FileName");
                response.Headers.Add("FileName", HttpUtility.UrlEncode(fileName));
                addUpDownloadTimes(targetFile.id);
            }
            catch (Exception e)
            {
                response = Request.CreateResponse(HttpStatusCode.InternalServerError, "Fail to download");
            }
            //finally
            //{
            //    fileStream.Close();
            //}
            return response;
            
        }


        private file getFileUrl(string code)
        {
            file result = null;
            using (FileEntitiesFinal entity = new FileEntitiesFinal())
            {
                user_share_file tempFile = entity.user_share_file.Where(f => f.code.Equals(code)).FirstOrDefault();
                if (tempFile != null)
                {
                    result = tempFile.file;
                    
                } else
                {
                    result = null;
                }
            }

            return result;
        }





        [HttpGet]
        [Route("test")]
        public string test()
        {
            var name = HttpContext.Current.Request.QueryString["name"];
            return name;
        }


        [HttpGet]
        [Route("file/uploadTest")]

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

        [HttpGet]
        [Route("file/getUserFile")]
        public HttpResponseMessage getUserFiles()
        {
           // TODO: need to login first
            var userId = HttpContext.Current.Session["id"];
            if (userId == null)
            {
                return Request.CreateResponse(HttpStatusCode.NonAuthoritativeInformation);
            }
            FileEntitiesFinal entity = new FileEntitiesFinal();

            var fileList = from f in entity.file where f.user_id == (int)userId && f.status == 1 select new { f.name, f.id, f.download_times, f.description, f.size, f.permission, f.type, f.user_share_file.FirstOrDefault().code };

            List<dynamic> result = new List<dynamic>();
            foreach (var line in fileList)
            {
                result.Add(line);
            }
            HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK, result);

            return response;

        }


        [NoLogin]
        [HttpGet]
        [Route("file/getPublicFiles")]
        public HttpResponseMessage getPublicFiles()
        {
           
            using (FileEntitiesFinal entity = new FileEntitiesFinal())
            {
                var fileList = from f in entity.file where f.permission!=2 && f.status == 1 select new { f.name, f.id, f.download_times, f.description, f.size ,f.permission, f.type, f.user_share_file.FirstOrDefault().code};

                List<dynamic> result = new List<dynamic>();
                foreach (var line in fileList)
                {
                    result.Add(line);
                }
                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK, result);

                return response;

            }
        }






        [NoLogin]
        [HttpGet]
        [HttpPost]
        [Route("file/getPublicFilesType")]
        public HttpResponseMessage getFilesByType(int type)
        {
            using (FileEntitiesFinal entity = new FileEntitiesFinal())
            {
                var fileList = from f in entity.file where f.type == type && f.status == 1 select new { f.name, f.id, f.download_times, f.description, f.size, f.permission, f.type, f.user_share_file.FirstOrDefault().code };

                List<dynamic> result = new List<dynamic>();
                foreach (var line in fileList)
                {
                    result.Add(line);
                }
                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK, result);

                return response;

            }
        }


        [NoLogin]
        [HttpGet]
        [HttpPost]
        [Route("file/getUserFileType")]
        public HttpResponseMessage getUserFileType(int type)
        {
            var userId = HttpContext.Current.Session["id"];
            using (FileEntitiesFinal entity = new FileEntitiesFinal())
            {
                var fileList = from f in entity.file where f.user_id == (int)userId && f.type == type && f.status == 1 select new { f.name, f.id, f.download_times, f.description, f.size, f.permission, f.type, f.user_share_file.FirstOrDefault().code };

                List<dynamic> result = new List<dynamic>();
                foreach (var line in fileList)
                {
                    result.Add(line);
                }
                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK, result);

                return response;

            }
        }



        [NoLogin]
        [HttpGet]
        [HttpPost]
        [Route("file/searchPublicFiles")]
        public HttpResponseMessage searchFile(string key)
        {
            using (FileEntitiesFinal entity = new FileEntitiesFinal())
            {
                var fileList = from f in entity.file where f.name.Contains(key) && f.status == 1 select new { f.name, f.id, f.download_times, f.description, f.size, f.permission, f.type, f.user_share_file.FirstOrDefault().code };

                List<dynamic> result = new List<dynamic>();
                foreach (var line in fileList)
                {
                    result.Add(line);
                }
                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK, result);

                return response;

            }
        }



        [HttpGet]
        [HttpPost]
        [Route("file/searchUserFile")]
        public HttpResponseMessage searchUserFile(string key)
        {
            var userId = HttpContext.Current.Session["id"];
            using (FileEntitiesFinal entity = new FileEntitiesFinal())
            {
                var fileList = from f in entity.file where f.name.Contains(key) && f.status == 1 && f.user_id == (int)userId select new { f.name, f.id, f.download_times, f.description, f.size, f.permission, f.type, f.user_share_file.FirstOrDefault().code };

                List<dynamic> result = new List<dynamic>();
                foreach (var line in fileList)
                {
                    result.Add(line);
                }
                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK, result);

                return response;

            }
        }

        [NoLogin]
        [HttpGet]
        [Route("file/delete")]
        public HttpResponseMessage deleteFile(string id)
        {
            //string id = HttpContext.Current.Request.Params["fileId"].ToString();
           
            int fileId = 0;
            HttpResponseMessage response = null;
            
            //response.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue()
            //{
            //    MaxAge = TimeSpan.FromMinutes(20)
            //};
            try
            {
                fileId = Convert.ToInt32(id);
            } catch (Exception e)
            {
                response = Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid File");
       
                return response;

            }
            var userId = HttpContext.Current.Session["id"];
            using (FileEntitiesFinal entity = new FileEntitiesFinal())
            {
                var files = from f in entity.file where f.id == fileId select f;
                file tempFile = files.FirstOrDefault();
                if (tempFile.user_id != (int)userId)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Operation deny");
                }
                if (tempFile.status == 100)
                {
                    
                    response = Request.CreateResponse(HttpStatusCode.BadRequest, "File had been deleted, don't delete it again.");
                    return response;
                }
                tempFile.status = 100;
                entity.Entry<file>(tempFile).State = System.Data.Entity.EntityState.Modified;
                entity.SaveChanges();
                
                string message = string.Format("Delete file {0} successfully!", tempFile.name.ToString());
                //response.Content = new StringContent(message, Encoding.Unicode);
                response = Request.CreateResponse(HttpStatusCode.OK, message);
                return response;

            }
        }

        [HttpGet]
        [Route("file/getDownloadCode")]
        public HttpResponseMessage getDownloadCode(int fileId)
        {
            
            using (FileEntitiesFinal entity  = new FileEntitiesFinal())
            {
                var userId = HttpContext.Current.Session["id"];
                string code = "";
                user_share_file user_Share_File = entity.user_share_file.Where(u => u.file_id == fileId && u.user_id == (int)userId).FirstOrDefault();
                if (entity.file.Where(f=>f.id == fileId).FirstOrDefault() == null)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, "No such file");
                }
                RandomCodeLib.CodeClass codeClass = new CodeClass();
                if (user_Share_File == null)
                {
                    codeClass.getRandomCode(6, out code);
                    while (isCodeExist(code))
                    {
                        codeClass.getRandomCode(6, out code);
                        if (code.Length < 6)
                        {
                            return Request.CreateResponse(HttpStatusCode.InternalServerError, "File to get the code");
                        }
                    }
                    user_Share_File = new user_share_file { user_id = (int)userId, file_id = fileId, code = code };
                    entity.user_share_file.Add(user_Share_File);
                    entity.SaveChangesAsync();
                   
                }
                else
                {
                    code = user_Share_File.code;
                }
                return Request.CreateResponse(HttpStatusCode.OK, code);
            }
        }


        public bool isCodeExist(string code)
        {
            using(FileEntitiesFinal entity = new FileEntitiesFinal())
            {
                user_share_file user_Share_File = entity.user_share_file.Where(f => f.code == code).FirstOrDefault();
                return user_Share_File != null;
            }
        }

        //private string GetRandomString(int length)
        //{
        //    const string key = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
        //    if (length < 1)
        //    {
        //        return string.Empty;
        //    }
        //        Random random = new Random();
        //        byte[] buffer = new byte[8];
        //        ulong bit = 21;
        //        ulong result = 0;
        //        int index = 0;
        //        StringBuilder stringBuilder = new StringBuilder((length / 5 + 1) * 5);
        //        while (stringBuilder.Length <length)
        //        {
        //            random.NextBytes(buffer);
        //            buffer[5] = buffer[6] = buffer[7] = 0x00;
        //            result = BitConverter.ToUInt64(buffer, 0);
        //            while (result >0 && stringBuilder.Length <length)
        //            {
        //                index = (int)(bit & result);
        //                stringBuilder.Append(key[index]);
        //                result = result >> 5;
        //            }
        //        }
        //        return stringBuilder.ToString();
        // }

        private async void addUpDownloadTimes(int fileId)
        {
            using (TransactionScope scope = new TransactionScope())
            {
                 using (FileEntitiesFinal entity = new FileEntitiesFinal())
                 {
                    file tempFile = entity.file.Where(f => f.id == fileId).FirstOrDefault();
                    if (tempFile == null)
                    {
                        return;
                    }
                    else
                    {
                        tempFile.download_times++;
                        entity.file.Attach(tempFile);
                        entity.Entry(tempFile).State = System.Data.Entity.EntityState.Modified;
                        await entity.SaveChangesAsync();
                    }
                 }
                scope.Complete();
                
            }
             
        }

        private async void addUpFileNumbs(int userId)
        {
            using (TransactionScope scope = new TransactionScope())
            {
                using (FileEntitiesFinal entity = new FileEntitiesFinal())
                {
                    user tempUser = entity.user.Where(u => u.id == userId).FirstOrDefault();
                    if (tempUser == null)
                    {
                        return;

                    }
                    tempUser.file_nums++;
                    entity.user.Attach(tempUser);
                    entity.Entry(tempUser).State = System.Data.Entity.EntityState.Modified;
                    await entity.SaveChangesAsync();
                }
            }
            
        }
        



    }
}
