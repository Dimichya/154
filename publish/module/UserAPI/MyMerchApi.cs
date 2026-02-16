using Microsoft.AspNetCore.Mvc;
using System;
using IO = System.IO;
using System.Text.Json;
using System.Linq;
using UserAPI;
using Shared;
using Shared.Models.Base;

namespace Lampac.Controllers
{
    public class MyMerchApi : BaseController
    {
        [HttpGet]
        [Route("myuserapi")]
        public ActionResult Index(string email, bool remove, DateTime extend)
        {
            if (string.IsNullOrEmpty(email))
                return Content("email empty");

            var user = AppInit.conf.accsdb.users.FirstOrDefault(i => i.id == email || i.ids.Contains(email));
            if (user == null)
                return Content("user not found");

            if (remove)
            {
                user.ban = true;
                IO.File.WriteAllText($"{ModInit.folder_mod}/users.json", JsonSerializer.Serialize(AppInit.conf.accsdb.accounts));
                return Content("user remove");
            }
            else if (extend != default)
            {
                if (user != null)
                {
                    user.expires = extend;
                }
                else
                {
                    user = new AccsUser() { id = email, expires = extend };
                    AppInit.conf.accsdb.users.Add(user);
                }

                IO.File.WriteAllText($"{ModInit.folder_mod}/users.json", JsonSerializer.Serialize(AppInit.conf.accsdb.users));
            }

            return Json(user);
        }
    }
}
