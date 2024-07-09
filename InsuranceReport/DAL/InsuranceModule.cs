using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;

namespace InsuranceReport.DAL
{
    public class InsuranceModule
    {
        public static string CheckTheNICType(string nic)
        {
            if (nic.Length == 12)
            {
                return "New NIC";
            }
            else if (nic.Length == 10)
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

            return a >= 500 ? "Female" : "Male";
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

            if (CheckTheGender(nic) == "Female")
            {
                dayOfYear -= 500;
            }
            if(year % 4 != 0)
            {
                dayOfYear = dayOfYear -1;
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
                "SELECT FA_INDEX as ind, reg_Date as regDate, name, Birth_Day as dob, DATEDIFF(YEAR, Birth_Day, GETDATE()) AS age," +
                "NIC_No as nic, sex, Ins_Month FROM FinalApproval.dbo.FNAP2401 UNION ALL " +
                "SELECT se_index as ind, reg_date as regDate, surname as name, birth_day as dob, DATEDIFF(YEAR, Birth_Day, GETDATE()) AS age, " +
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
    }


}