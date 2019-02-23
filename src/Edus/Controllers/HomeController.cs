using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Edus.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        //系统首页
        public ActionResult Index()
        {
            return View();
        }

        //没找到
        public ActionResult NotFound()
        {
            return View("NotFound");
        }

        //发生错误
        public ActionResult Error()
        {
            //其实也并不是发生错误，在测试阶段，有可能是bug造成，而在用户使用阶段，则很可能是用户恶意破坏造成，所以直接抛错
            return View("Error");
        }
    }
}