using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Collections.Generic;

namespace InsuranceReport.DAL
{
    public class InsuranceModule
    {
        public static string CheckTheNICType(string nic)
        {
            if (nic.Length == 12 && nic != "000000000000")
            {
                return "New NIC";
            }
            else if (nic.Length == 10 && (nic[nic.Length - 1] == 'V' || nic[nic.Length - 1] == 'X'))

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

        public static DataTable get_older_employees()
        {
            DataTable dt = new DataTable();
            string sql = "SELECT * FROM( " +
                "SELECT FA_INDEX as ind, reg_Date as regDate, name, Birth_Day as dob, DATEDIFF(YEAR, Birth_Day, reg_Date) AS age," +
                "NIC_No as nic, sex, Ins_Month FROM FinalApproval.dbo.FNAP2401 UNION ALL " +
                "SELECT se_index as ind, reg_date as regDate, surname as name, birth_day as dob,DATEDIFF(YEAR, Birth_Day, reg_Date) AS age, " +
                "nic_no as nic, sex, ins_month " +
                "FROM Self.dbo.self2024 ) AS a " +
                "where a.age >= 65 and Ins_Month is null order by regDate";

            using (var conn_uat = new SqlConnection(ConfigurationManager.ConnectionStrings["connx"].ConnectionString))
            {

                conn_uat.Open();

                SqlCommand command = new SqlCommand(sql, conn_uat);

                SqlDataAdapter da = new SqlDataAdapter(command);
                DataSet ds = new DataSet();
                da.Fill(ds);
                dt = ds.Tables[0];

            };

            return dt;
        }

        public static DataTable FindInvalidRecords()
        {
            DataTable records = get_older_employees();
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
                    else
                    {
                        DataRow newRow = dt.NewRow();
                        newRow.ItemArray = row.ItemArray;
                        newRow["ErrorType"] = "Correct Data";
                        dt.Rows.Add(newRow);
                    }
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

        public static Boolean updateElderRecords(List<string> list)
        {
            string sql = "UPDATE Self.dbo.self2024 SET ins_month = 'NOTSEN' WHERE DATEDIFF(YEAR, birth_day, reg_Date) >= 65 " +
                "AND ins_month IS NULL";

            string sql2 = "UPDATE FinalApproval.dbo.FNAP2401 SET Ins_Month = 'NOTSEN' " +
                "WHERE DATEDIFF(YEAR, Birth_Day, reg_Date) >= 65 AND Ins_Month IS NULL ";

            //foreach(string item in list)
            //{
            //   string letter = 
            //}




            return false;
        }
    }
}
