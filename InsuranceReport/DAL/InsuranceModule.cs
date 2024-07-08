using System;
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

        public static string Get_Birthday(string nic)
        {
            string nicType = CheckTheNICType(nic);
            if (nicType == "Invalid NIC")
            {
                return "Invalid NIC";
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
                dayOfYear = dayOfYear - 1;
            }
          

            DateTime birthDate = new DateTime(year, 1, 1).AddDays(dayOfYear - 1);
            return birthDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        }
    }
}
