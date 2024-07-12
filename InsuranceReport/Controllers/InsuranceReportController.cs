using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using InsuranceReport.DAL;
using InsuranceReport.Models;

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

           dob = InsuranceModule.Get_Birthday(nic).ToShortDateString();
            int age = InsuranceModule.GetAge(nic);
            

            return Json(new { nictype,gender,dob,age }, JsonRequestBehavior.AllowGet);

        }

        //public ActionResult ElderRecords()
        //{
        //    InsuranceReportModel model = new InsuranceReportModel();

        //    model.ElderRecords = InsuranceModule.get_older_employees();
        //    return View(model);
        //}

        public ActionResult ElderRecords()
        {
            InsuranceReportModel mod = new InsuranceReportModel();
            mod.IncorrectData = InsuranceModule.FindInvalidRecords();
            return View(mod);
        }

        public JsonResult MarkAsNOTSEN(List<string> list)
        {
            Boolean isSuccess = InsuranceModule.updateElderRecords(list);



            return Json(new { }, JsonRequestBehavior.AllowGet);
        }


    }
}