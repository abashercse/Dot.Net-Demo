using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TicketingSystem.Models
{
    public class CustUtility
    {
        //public const string CONN_STR_SQL = "Server=localhost;Database=conactdb;Uid=root;Pwd=admin";
        public const string CONN_STR_SQL = "Server=10.12.1.77;Port=30036;Database=conactdb;User Id = root; Password=wamik;ConnectionReset=True;pooling=true;minpoolsize=5;maxpoolsize=10;ConnectionLifeTime=30";
        public const string RunMode = "TEST";
        public const int AccountLockedAttemped = 5;
        //public const string CONN_STR_SQL = "Server=localhost;Port=3306;Database=conactdb;User Id = admin; Password=admin;ConnectionReset=True;pooling=true;minpoolsize=10;maxpoolsize=50;ConnectionLifeTime=30";
        public const int IDEAL_SESSION_TIMEOUT_MIN = 10;

        public const string MAIL_FROM_ADDRESS = "Employee@grameenphone.com";
        //public const string MAIL_SMTP_HOST = "192.168.207.211";
        public const string MAIL_SMTP_HOST = "10.12.1.76";
        //public const string FILE_UPLOAD_LOCATION = "D:\\Attachments\\";
        public const string FILE_UPLOAD_LOCATION = "/home/ocpadmin1/dot_net_attachments/";
        public const string ldapHost = "10.10.16.143";
        public const int ldapPort = 389;
        public const string ldapPassword = "Grameen@123";
        public const string ldapDn = "uid=admin,ou=system";
        public const string domainName = "ldap://10.10.20.76:389";

        public static List<SelectListItem> PopulateUrgentList()
        {
            List<SelectListItem> items = new List<SelectListItem>();
            items.Add(new SelectListItem { Text = "YES", Value = "YES" });
            items.Add(new SelectListItem { Text = "NO", Value = "NO" });
            return items;
        }
        public static List<SelectListItem> PopulateTicketStatusList()
        {
            List<SelectListItem> items = new List<SelectListItem>();
            items.Add(new SelectListItem { Text = "OPEN", Value = "OPEN" });
            items.Add(new SelectListItem { Text = "CLOSE", Value = "CLOSE" });
            items.Add(new SelectListItem { Text = "ASSIGNED", Value = "ASSIGNED" });
            items.Add(new SelectListItem { Text = "HOLD", Value = "HOLD" });
            items.Add(new SelectListItem { Text = "REJECTED", Value = "REJECTED" });
            return items;
        }

        public static string GeneratePassword()
        {
            /*string pass = System.Web.Security.Membership.GeneratePassword(8, 2);
            pass = RandomPassword.Generate(8, 10);*/
            string pass = "12345678";
            return pass;
        }

        public static string HashSHA1(string value)
        {
            System.Security.Cryptography.SHA256 sha1 = System.Security.Cryptography.SHA256.Create();

            byte[] inputBytes = Encoding.ASCII.GetBytes(value);
            byte[] hash = sha1.ComputeHash(inputBytes);

            //return Convert.ToBase64String(hash);
            //return System.Text.Encoding.UTF8.GetString(hash);

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("X2"));
            }
            return sb.ToString();
        }
    }
}
