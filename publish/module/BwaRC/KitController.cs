using BwaRC;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Shared;
using Shared.Engine;
using System;
using System.Web;
using IO = System.IO;

namespace Lampac.Controllers.BwaRC
{
    public class KitController : Controller
    {
        [Route("/kit")]
        public ActionResult KitIndex([FromForm]string json, string user_id)
        {
            string init_file = $"{ModInit.folder_mod}/raw/{CrypTo.md5(user_id?.ToLower()?.Trim())}";

            if (!string.IsNullOrEmpty(json))
			{
                if (string.IsNullOrEmpty(user_id))
                    return Content("error", "application/json; charset=utf-8");

                try
                {
                    JsonConvert.DeserializeObject<AppInit>(json);
                }
                catch (Exception ex) { return Json(new { error = true, ex = ex.Message }); }

                IO.File.WriteAllText(init_file, json);
                return Content(json, "application/json; charset=utf-8");
            }
			else
			{
				if (string.IsNullOrEmpty(user_id))
                    return Content(IO.File.ReadAllText($"{ModInit.folder_mod}/html/auth.html"), "text/html; charset=utf-8");

                string html = IO.File.ReadAllText($"{ModInit.folder_mod}/html/settings.html");
                string conf = IO.File.Exists(init_file) ? IO.File.ReadAllText(init_file) : string.Empty;
				return Content(html.Replace("{conf}", conf).Replace("{user_id}", HttpUtility.UrlEncode(user_id)), "text/html; charset=utf-8");
			}
        }
    }
}