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
            

            nictype = CheckTheNICType(nic);
            gender = CheckTheGender(nic);

           dob = Get_Birthday(nic).ToShortDateString();
            int age = GetAge(nic);
            

            return Json(new { nictype,gender,dob,age }, JsonRequestBehavior.AllowGet);

        }

        public static string CheckTheNICType(string nic)
        {
            if (nic.Length == 12 && nic != "000000000000")
            {
                return "New NIC";
            }
            else if (nic.Length == 10 && (char.ToUpper(nic[nic.Length - 1]) == 'V' || char.ToUpper(nic[nic.Length - 1]) == 'X'))


            {
                return "Old NIC";
            }
            else
            {
                return "Invalid NIC";
            }
        }

        public static string CheckTheGender(string nic)
        {
            string nicType = CheckTheNICType(nic);
            if (nicType == "Invalid NIC")
            {
                return "Invalid NIC";
            }

            int a;
            if (nicType == "Old NIC")
            {
                a = int.Parse(nic.Substring(2, 3));
            }
            else // New NIC
            {
                a = int.Parse(nic.Substring(4, 3));
            }

            return a >= 500 ? "F" : "M";
        }

        public static DateTime Get_Birthday(string nic)
        {
            string nicType = CheckTheNICType(nic);
            if (nicType == "Invalid NIC")
            {
                throw new ArgumentException("Invalid NIC");
            }

            int year;
            int dayOfYear;
            if (nicType == "New NIC")
            {
                year = int.Parse(nic.Substring(0, 4));
                dayOfYear = int.Parse(nic.Substring(4, 3));
            }
            else // Old NIC
            {
                year = int.Parse(nic.Substring(0, 2));
                year += (year < 30) ? 2000 : 1900; // Adjust century
                dayOfYear = int.Parse(nic.Substring(2, 3));
            }

            if (CheckTheGender(nic) == "F")
            {
                dayOfYear -= 500;
            }

            // Check for leap year adjustment
            if (DateTime.IsLeapYear(year))
            {
                if (dayOfYear > 366 || dayOfYear < 1)
                {
                    throw new ArgumentOutOfRangeException("dayOfYear", "Day of year is out of range for the given year.");
                }
            }
            else
            {
                if (dayOfYear > 365 || dayOfYear < 1)
                {

                    throw new ArgumentOutOfRangeException("dayOfYear", "Day of year is out of range for the given year.");
                }

                if (dayOfYear > 60)
                {
                    dayOfYear = dayOfYear - 1;
                }
            }

            DateTime birthDate = new DateTime(year, 1, 1).AddDays(dayOfYear - 1);
            return birthDate;
        }

        public static int GetAge(string nic)
        {
            DateTime birthDate = Get_Birthday(nic);
            DateTime today = DateTime.Today;
            int age = today.Year - birthDate.Year;

            // If the birthday has not occurred yet this year, subtract one year from the age
            if (birthDate > today.AddYears(-age))
            {
                age--;
            }

            return age;
        }

        public ActionResult ElderRecords()
        {
            InsuranceReportModel mod = new InsuranceReportModel();
            DataTable records = InsuranceModule.get_older_employees();
            mod.IncorrectData = FindInvalidRecords(records);
            return View(mod);
        }

        public ActionResult ElRecords()
        {
            InsuranceReportModel mod = new InsuranceReportModel();
            DataTable records = InsuranceModule.get_employees_age_lower_65();
            mod.IncorrectData = FindInvalidRecords(records);
            return View(mod);
        }


        public ActionResult PeoplesInurance()
        {
            InsuranceReportModel mod = new InsuranceReportModel();
            DataTable records = InsuranceModule.get_People_Insurance_Records();
            mod.IncorrectData = FindInvalidRecords(records);
            return View(mod);
        }


        public static DataTable FindInvalidRecords(DataTable records)
        {
            DataTable dt = records.Clone(); // Create a new DataTable with the same structure as records
            dt.Columns.Add("ErrorType", typeof(string)); // Add a new column for the error type

            foreach (DataRow row in records.Rows)
            {
                try
                {
                    string nic = row["nic"].ToString().Trim();
                    DateTime storedDob = DateTime.Parse(row["dob"].ToString());
                    string storedGender = row["sex"].ToString();

                    string nicType = CheckTheNICType(nic);
                    if (nicType == "Invalid NIC")
                    {
                        DataRow newRow = dt.NewRow();
                        newRow.ItemArray = row.ItemArray;
                        newRow["ErrorType"] = "Invalid NIC";
                        dt.Rows.Add(newRow);
                        continue;
                    }

                    DateTime calculatedDob = Get_Birthday(nic);
                    string calculatedGender = CheckTheGender(nic);

                    if (storedDob != calculatedDob && !storedGender.Equals(calculatedGender, StringComparison.OrdinalIgnoreCase))
                    {
                        DataRow newRow = dt.NewRow();
                        newRow.ItemArray = row.ItemArray;
                        newRow["ErrorType"] = "Wrong Birthday, Wrong Gender";
                        dt.Rows.Add(newRow);
                    }
                    else if (storedDob != calculatedDob)
                    {
                        DataRow newRow = dt.NewRow();
                        newRow.ItemArray = row.ItemArray;
                        newRow["ErrorType"] = "Wrong Birthday";
                        dt.Rows.Add(newRow);
                    }
                    else if (!storedGender.Equals(calculatedGender, StringComparison.OrdinalIgnoreCase))
                    {
                        DataRow newRow = dt.NewRow();
                        newRow.ItemArray = row.ItemArray;
                        newRow["ErrorType"] = "Wrong Gender";
                        dt.Rows.Add(newRow);
                    }
                    // Skip adding rows with "Correct Data"
                }
                catch (Exception ex)
                {
                    // Handle or log the exception as needed
                    // For now, just continue to the next record
                    continue;
                }
            }

            return dt;
        }




        public JsonResult MarkAsNOTSEN(List<string> list)
        {
            Boolean isSuccess = InsuranceModule.updateElderRecords(list);



            return Json(new { }, JsonRequestBehavior.AllowGet);
        }


    }
}