using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using InsuranceReport.DAL;

namespace InsuranceReport.Controllers
{
    public class InsuranceReportController : Controller
    {
        // GET: InsuranceReport
        public ActionResult Index()
        {
            return View();
        }

        public JsonResult checkNic(string nic)
        {
            string result = "null";

            result = InsuranceModule.CheckTheNICType(nic);

            return Json(new { result }, JsonRequestBehavior.AllowGet);

        }


    }
}