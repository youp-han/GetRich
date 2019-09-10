using System;
using System.IO;
using System.Text;
using System.Web.Mvc;

namespace Lotto.Controllers
{
    public class BaseController : Controller
    {
        #region Custom JsonResult

        private object CustomJsonObject(bool success, string msg, object data)
        {
            return new { result = success ? "success" : "failure", msg = msg, data = data };
        }

        protected JsonResult Json(bool success, string msg, object data)
        {
            return Json(CustomJsonObject(success, msg, data));
        }

        protected JsonResult Json(bool success, string msg, object data, JsonRequestBehavior behavior)
        {
            return Json(CustomJsonObject(success, msg, data), behavior);
        }

        protected JsonResult Json(bool success, string msg, object data, string contentType)
        {
            return Json(CustomJsonObject(success, msg, data), contentType);
        }

        protected JsonResult Json(bool success, string msg, object data, string contentType, Encoding contentEncoding)
        {
            return Json(CustomJsonObject(success, msg, data), contentType, contentEncoding);
        }

        protected JsonResult Json(bool success, string msg, object data, string contentType, JsonRequestBehavior behavior)
        {
            return Json(CustomJsonObject(success, msg, data), contentType, behavior);
        }

        protected JsonResult Json(bool success, string msg, object data, string contentType, Encoding contentEncoding, JsonRequestBehavior behavior)
        {
            return Json(CustomJsonObject(success, msg, data), contentType, contentEncoding, behavior);
        }

        #endregion

        #region JsonResult

        protected internal JsonResult Json(object data)
        {
            return this.Json(data, (string)null, (Encoding)null, JsonRequestBehavior.DenyGet);
        }

        protected internal JsonResult Json(object data, string contentType)
        {
            return this.Json(data, contentType, (Encoding)null, JsonRequestBehavior.DenyGet);
        }

        protected internal virtual JsonResult Json(object data, string contentType, Encoding contentEncoding)
        {
            return this.Json(data, contentType, contentEncoding, JsonRequestBehavior.DenyGet);
        }

        protected internal JsonResult Json(object data, JsonRequestBehavior behavior)
        {
            return this.Json(data, (string)null, (Encoding)null, behavior);
        }

        protected internal JsonResult Json(object data, string contentType, JsonRequestBehavior behavior)
        {
            return this.Json(data, contentType, (Encoding)null, behavior);
        }

        protected internal virtual JsonResult Json(object data, string contentType, Encoding contentEncoding, JsonRequestBehavior behavior)
        {
            return new JsonResult()
            {
                Data = data,
                ContentType = contentType,
                ContentEncoding = contentEncoding,
                JsonRequestBehavior = behavior
            };
        }


        #endregion

        #region Log-Local

        public void LogToLocalDirectory(string resultMsg)
        {

            string LogPath = Server.MapPath("~/Connection_Log/");
            if (!Directory.Exists(LogPath))
            {
                Directory.CreateDirectory(LogPath);
            }
            string logFileName = Path.Combine(LogPath, string.Concat("Lotto_Connection_Error_Log", "_", DateTime.Now.ToShortDateString(), ".log"));
            LogContents(logFileName, "Error: " + resultMsg);
        }



        public void LogContents(string path, string logMessage)
        {
            using (StreamWriter w = System.IO.File.AppendText(path))
            {
                w.Write("\r\nLog Entry : ");
                w.WriteLine("{0} {1}", DateTime.Now.ToLongTimeString(), DateTime.Now.ToLongDateString());
                w.WriteLine("-------------------------------");
                w.WriteLine("{0}", logMessage);
                w.WriteLine("-------------------------------");
            }
        }

        #endregion
    }
}