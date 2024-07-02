using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace InsuranceReport.DAL
{
    public class InsuranceModule
    {

        public static string CheckTheNICType(string nic)
        {

            string NicType = null;

            if(nic.Length == 12)
            {
                NicType = "New Nic";
            } else if(nic.Length == 10)
            {
                NicType = "Old NIC";
            }

            return NicType;

        }

        public static string CheckTheGender(string nic)
        {

            string gender = null;

            string nicType = CheckTheNICType(nic);


            return gender;


        }
    }
}