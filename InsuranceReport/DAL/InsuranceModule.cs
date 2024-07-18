using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace InsuranceReport.DAL
{
    public class InsuranceModule
    {

        //private static readonly int currentMonth = DateTime.Now.Month;
        private static readonly int currentMonth = 7;
        //get the employeee deatailes how is the age is grater than 65 at the registration date.
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

        public static DataTable get_employees_age_lower_65()
        {
            int month = currentMonth - 1;
            DataTable dt = new DataTable();
            string sql = "select * from( " +
                "SELECT se_index as ind, reg_date as regDate, surname as name, birth_day as dob, " +
                "DATEDIFF(YEAR, Birth_Day, reg_Date) AS age, nic_no as nic, sex, ins_month, status " +
                "FROM[rumeshTest].[dbo].self2024New2 " +
                "UNION ALL " +
                "SELECT FA_INDEX as ind, reg_date as regDate, Name as name, birth_day as dob, " +
                "DATEDIFF(YEAR, Birth_Day, reg_Date) AS age, nic_no as nic, sex, ins_month, status " +
                "FROM[rumeshTest].[dbo].[FNAP2401New2] ) as a " +
                "where DATEDIFF(YEAR, a.dob, a.regDate) < 65  and(a.status = 'R' or a.status is null)" +
                " and month(a.regDate) <= "+month+ " " +
                "and a.ins_month is null order by regDate desc ";

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

        public static DataTable get_People_Insurance_Records()
        {
            int month = currentMonth - 1;
            DataTable dt = new DataTable();
            string sql = "select ind,regDate,name,dob,age,nic,sex from( SELECT se_index as ind, reg_date as regDate, surname as name, birth_day as dob, " +
                "DATEDIFF(YEAR, Birth_Day, reg_Date) AS age, nic_no as nic, sex, ins_month, status, jobcate as jobCate, " +
                "centre_code as cenCode " +
                "FROM[rumeshTest].[dbo].self2024New2 UNION ALL " +
                "SELECT FA_INDEX as ind, reg_date as regDate, Name as name, birth_day as dob, " +
                "DATEDIFF(YEAR, Birth_Day, reg_Date) AS age, nic_no as nic, sex, ins_month, status, JobCate as jobCate, centre_code as cenCode " +
                "FROM[rumeshTest].[dbo].[FNAP2401New2] ) as a " +
                "INNER JOIN[Utility].[dbo].[JOB_UNIT] ju on a.jobCate = ju.code " +
                "where DATEDIFF(YEAR, a.dob, a.regDate) < 65  and(a.status = 'R' or a.status is null) " +
                "and month(a.regDate) <= "+month+" " +
                "and a.ins_month is null and a.sex = 'M' or(a.sex = 'F' and ju.SECTOR = 'N') or a.cenCode = 'T' " +
                "order by regDate desc ";

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
