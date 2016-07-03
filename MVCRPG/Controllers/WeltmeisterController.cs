using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MVCRPG.Controllers
{
    public class WeltmeisterController : Controller
    {
        #region Models
        class WMBrowseRequest
        {
            public string dir { get; set; }
            public string type { get; set; }
        }
        #endregion

        // GET: /Weltmeister/

        public ActionResult Index()
        {
            return View();
        }

        [AcceptVerbs("GET", "POST")]
        public JsonResult Glob()
        {
            List<string> globs = new List<string>();
            List<string> files = new List<string>();

            if (Request.Params["glob[]"] != null)
            {
                string glob = Request.Params["glob[]"];
                string path = Server.MapPath(glob.Substring(0, glob.IndexOf('*'))
                    );
                string pattern = string.Format("*{0}", Path.GetExtension(glob));

                files.AddRange(
                    new DirectoryInfo(path).GetFiles(pattern).Select(f => f.FullName.Replace(Request.PhysicalApplicationPath, string.Empty).Replace("\\", "/")).ToList()
                );
            }
            else if (Request.Params["glob[0]"] != null)
            {
                foreach (var key in Request.Params.AllKeys.Where(param => param.Contains("glob")))
                {
                    string glob = Request.Params[key];
                    string path = Path.GetFullPath(glob);
                    string pattern = string.Format("*{0}", Path.GetExtension(glob));
                    
                    // get all the files
                    files.AddRange(
                        new DirectoryInfo(glob).GetFiles(pattern).Select(f => f.FullName.Replace(Request.PhysicalApplicationPath, string.Empty).Replace("\\", "/")).ToList()
                    );
                }
            }

            List<string> newElemts = new List<string>();
            foreach (var item in files)
            {
                newElemts.Add(item.Replace("lib/",""));
            }

            return Json(newElemts, JsonRequestBehavior.AllowGet);
        }

        [AcceptVerbs("GET", "POST")]
        public JsonResult Save(string path, string data)
        {
            string serverPath = Server.MapPath(path);

            try
            {
                using (FileStream fs = new FileStream(serverPath, FileMode.Create, FileAccess.ReadWrite))
                {
                    using (StreamWriter writer = new StreamWriter(fs))
                    {
                        writer.Write(data);
                        writer.Flush();
                        writer.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                // failure
                return Json(new
                {
                    error = 1,
                    msg = ex.ToString()
                }, JsonRequestBehavior.AllowGet);
            }

            // success
            return Json(new { error = 0, msg = "" }, JsonRequestBehavior.AllowGet);
        }

        [AcceptVerbs("GET", "POST")]
        public JsonResult Browse(string dir, string type)
        {
            List<string> files = new List<string>();
            List<string> dirs = new List<string>();

            // map requested directory to server path 
            //string cleanPath = dir;
            //List<ValuesToValidate> validations = new List<ValuesToValidate>();
            //validations.Add(new ValuesToValidate() { valueValidated = "weltmeister/", valueReplaced = "../" });
            //validations.Add(new ValuesToValidate() { valueValidated = "lib/lib/", valueReplaced = "lib/" });


            //foreach (ValuesToValidate item in validations)
            //{
            //    if (dir.Contains(item.valueValidated))
            //    {
            //        cleanPath = dir.Replace(item.valueValidated, item.valueReplaced); break;
            //    }
            //}
            
            //var path = dir.Contains("weltmeister/") ? dir.Replace("weltmeister/", "../") :  dir;
            //var cleanPath = dir.Contains("/../") ? dir.Replace("/../", "../") : path;
            var finalPath = string.IsNullOrEmpty(dir) || dir.Contains("../") == false ? "../" + dir : dir;
            string serverPath = Server.MapPath(finalPath);

            // get directories
            Directory.GetDirectories(serverPath).ToList().ForEach((d) =>
            {
                dirs.Add(
                    d.Replace(Request.PhysicalApplicationPath, String.Empty).Replace("\\", "/")
                );
            });

            // type can be "", "images", or "scripts"
            List<string> fileTypes = new List<string>();
            if (string.IsNullOrWhiteSpace(type))
                fileTypes = new List<string>() {
                    "*.*"
                };
            else if (type.Equals("images"))
            {
                fileTypes = new List<string>() {
                    "*.png", "*.gif", "*.jpg", "*.jpeg"
                };
            }
            else if (type.Equals("scripts"))
            {
                fileTypes = new List<string>() {
                    "*.js"
                };
            }

            fileTypes.ForEach((pattern) =>
            {
                files.AddRange(
                    new DirectoryInfo(serverPath).GetFiles(pattern).Select(f => f.FullName.Replace(Request.PhysicalApplicationPath, string.Empty).Replace("\\", "/")).ToList()
                );
            });

            string parent = "";
            if (string.IsNullOrWhiteSpace(dir) == false)
            {
                parent = Directory.GetParent(Server.MapPath(dir)).FullName + "\\";
                parent = parent.Replace(Request.PhysicalApplicationPath, String.Empty);
                parent = parent.Replace("\\", "/");
            }
            if (parent == dir)
                parent = "";

            var data = new
            {
                parent = parent,
                dirs = dirs,
                files = files
            };

            return Json(data, JsonRequestBehavior.AllowGet);
        }
    }
}