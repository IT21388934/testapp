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
            string nictype = "null";
            string gender = null;
            string dob = null;

            nictype = InsuranceModule.CheckTheNICType(nic);
            gender = InsuranceModule.CheckTheGender(nic);

            dob = InsuranceModule.Get_Birthday(nic);
            

            return Json(new { nictype,gender,dob }, JsonRequestBehavior.AllowGet);

        }


    }
}