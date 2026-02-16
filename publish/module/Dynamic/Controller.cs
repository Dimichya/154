using Microsoft.AspNetCore.Mvc;
using Shared;

namespace Dynamic
{
    public class DynamicMvc : BaseController
    {
        [HttpGet]
        [Route("hellodynamic")]
        public ActionResult Index()
        {
            return Content("dynamic controller");
        }
    }
}
