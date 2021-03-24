using System;
using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TicketingSystem.Models;
using System.Security.Claims;
using MySql.Data.MySqlClient;
using System.Reflection.Emit;
using System.Security.Principal;
using System.Security.Cryptography;
using System.Text;


namespace TicketingSystem.Controllers
{
    public class HomeController : Controller
    {
        public int AccountLockedAttemped = CustUtility.AccountLockedAttemped;

        private readonly IHttpContextAccessor _httpContextAccessor;
        public HomeController(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        [HttpGet]
        public IActionResult Index()
        {
         /*   string userName;
            string domainName;
            string loginName;
            string runMode;

            ViewData["divNewPassConfirm"] = false;
            ViewData["divNewPass"] = false;
            ViewData["btnPasswordSave"] = false;

            runMode = CustUtility.RunMode;

            if(HttpContext.Session.GetString("LoginFailCount") == null)
            {
                HttpContext.Session.SetInt32("LoginFailCount", 0);
            }
            try
            {
                string user = HttpContext.Request.Query["user"];
                string ID = HttpContext.Request.Query["ID"];
                string type = HttpContext.Request.Query["type"];

                if (!String.IsNullOrEmpty(user) && !String.IsNullOrEmpty(ID) && !String.IsNullOrEmpty(type))
                {
                    domainName = type;
                    loginName = user;
                    HttpContext.Session.SetString("UserID", loginName);
                    HttpContext.Session.SetString("LoginStatus", "LOGGED-IN");
                    HttpContext.Session.SetString("LoginTime", DateTime.Now.ToLongDateString());

                    if (String.IsNullOrEmpty(HttpContext.Session.GetString("UserID")))
                    {
                        return RedirectToAction("Error", "Home");
                    }
                    else
                    {
                        //HttpContext.Session.SetString("UserInfo", null);
                        //to set user model in place of null
                        UserModel userModel = this.CountByUserId(loginName);
                        if (userModel != null)
                        {
                            HttpContext.Session.SetObjectAsJson("UserInfo", userModel);
                        }
                        return RedirectToAction("Dashboard", "ActivationDashboard");
                    }
                }
                else if (String.IsNullOrEmpty(user) && String.IsNullOrEmpty(ID) && !String.IsNullOrEmpty(type))
                {
                    if(type.ToLower() == "gpdh")
                    {
                        ViewData["rowDhLogin"] = true;
                    }
                }
                else
                {           

                    if (runMode.Equals("TEST"))
                    {
                        domainName = "GRAMEENPHONE";
                        loginName = "shameem";
                    }
                    else
                    {
                        //Request came from Grameenphone Domain user
                        userName = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
                        //userName = "TEST";
                        domainName = userName.Substring(0, userName.IndexOf("\\"));
                        loginName = userName.Substring(domainName.Length + 1, (userName.Length - domainName.Length - 1));
                    }
                    HttpContext.Session.SetString("UserID", loginName);
                    HttpContext.Session.SetString("LoginStatus", "LOGGED-IN");
                    HttpContext.Session.SetString("LoginTime", DateTime.Now.ToLongDateString());

                    if (String.IsNullOrEmpty(HttpContext.Session.GetString("UserID")))
                    {
                        return RedirectToAction("Error", "Home");
                    }
                    else
                    {
                        UserModel userModel = this.CountByUserId(loginName);
                        if(userModel != null && userModel.count == 1)
                        {
                            //ldap authenticate if user exist
                        //  LdapAuthentication authentication = new LdapAuthentication();
                          //  bool authStatus= authentication.validateUser("wipro.mohit","Lab123456");
                            //Console.WriteLine(authStatus);
                            HttpContext.Session.SetObjectAsJson("UserInfo", userModel);
                        //    var UserInfo = HttpContext.Session.GetObjectFromJson<UserModel>("UserInfo");
                            return RedirectToAction("Dashboard", "ActivationDashboard");
                        }
                        else
                        {
                            HttpContext.Session.SetObjectAsJson("UserInfo", null);
                            return RedirectToAction("MyRequest", "MyRequest");
                        }
                    }
                }
                
            }
            catch (Exception ex)
            {
                ViewData["lblMessage"] = "ERROR: " + ex.Message;
            }
            */
            return View();
        }
        
        [HttpPost]

        protected void btnLogin_Click(object sender, EventArgs e)
        {
            Console.WriteLine("TEST*****************");
            Console.WriteLine("TEST*****************");
           
        }
        [HttpPost]
        //public IActionResult logIn(UserModel userModel, string txtUserName)
        public IActionResult logIn(string txtUserName, string txtPassword, string txtNewPassword, string txtConfirmNewPass)
        {
           
            if (String.IsNullOrEmpty(txtUserName) || String.IsNullOrEmpty(txtPassword))
            {
                ViewData["lblMessage"] = "ERROR: " + "Wrong User Name / Password";
                return View("index");
                //return new ObjectResult("Wrong User Name / Password");
            }
             //For GP domain user
            UserModel userModelGp = this.CountByUserId(txtUserName);
            if (userModelGp != null && userModelGp.count == 1)
            {
                HttpContext.Session.SetString("UserID", txtUserName);
                HttpContext.Session.SetString("LoginStatus", "LOGGED-IN");
                HttpContext.Session.SetString("LoginTime", DateTime.Now.ToLongDateString());
                    
                HttpContext.Session.SetObjectAsJson("UserInfo", userModelGp);
                return RedirectToAction("Dashboard", "ActivationDashboard");
            }
            //end
           
            UserModel userModel = this.getUserDetails(txtUserName);
            if (userModel != null && userModel.count == 1) 
            {
                HttpContext.Session.SetObjectAsJson("UserInfo", userModel);
                if (!String.IsNullOrEmpty(userModel.isLocked))
                    {
                        if(userModel.isLocked.Equals("True"))
                         {
                            ViewData["lblMessage"] = "This account is locked! Please contact with System Admin";
                            return View("index");
                       // return new ObjectResult("This account is locked! Please contact with System Admin");
                         }
                    }
               // Guid userGuid = System.Guid.NewGuid();
                //string passwordSalt = userGuid.ToString();
                //string test= this.passwordEncryption(txtPassword + passwordSalt);
                //Console.WriteLine(test);
                string hashedPassword = this.passwordEncryption(txtPassword+ userModel.pwSalt);
                if (hashedPassword.Equals(userModel.txtPassword))
                {
                    if(!String.IsNullOrEmpty(userModel.pwResetFlag))
                    {
                        if(userModel.pwResetFlag.Equals("False"))
                            {
                            ViewData["lblMessage"] = "Please change your password before login!";
                            return View("index");
                            //return new ObjectResult("Please change your password before login!");
                        }
                    }

                    if (!String.IsNullOrEmpty(userModel.pwResetDate))
                    {
                        if (Convert.ToDateTime(userModel.pwResetDate) <= DateTime.Now.AddDays(-90))
                        {
                            ViewData["lblMessage"] = "Your Password has expired !";
                            return View("index");
                            //return new ObjectResult("Your Password has expired !");
                        }

                    }
                    HttpContext.Session.SetString("UserID", txtUserName);
                    HttpContext.Session.SetString("LoginStatus", "LOGGED-IN");
                    HttpContext.Session.SetString("LoginTime", DateTime.Now.ToLongDateString());
                    return RedirectToAction("Dashboard", "ActivationDashboard");

                }
                else
                {
                    //need to check login fail count and update table
                    ViewData["lblMessage"] = "Wrong User Name / Password !! Your account is locked. Contact with system admin";
                    return View("index");
                    
                }
            }
            else
            {
                ViewData["lblMessage"] = "Wrong User Name / Password";
                return View("index");
                //return new ObjectResult("Wrong User Name / Password");
            }
               
        }
        [HttpPost]
        public IActionResult Reset(UserModel userModel)
        {
            return RedirectToAction("Index", "Home");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        private UserModel CountByUserId(String loginName)
        {
            int rowCount = 0;
            UserModel userModel = null;
            MySqlConnection con = new MySqlConnection(CustUtility.CONN_STR_SQL);
            try
            {
                String sSQL = "SELECT USER_ID, USER_NAME,USER_GROUP,USER_TYPE FROM TBL_USER WHERE USER_ID = '" + loginName + "'";
                MySqlCommand cmd = new MySqlCommand(sSQL);

              /*  userModel = new UserModel
                {
                    txtUserId = "shameem",
                    txtUserName = "Md. Shameem Hassan",
                    txtUserGroup = "BS",
                    txtUserType = "Individual",
                    count = 1
                };
*/
                cmd.Connection = con;
                con.Open();
                MySqlDataReader sdr = cmd.ExecuteReader();
                while (sdr.Read())
                {
                    rowCount = rowCount + 1;
                    userModel = new UserModel
                    {
                        txtUserId = sdr["USER_ID"].ToString(),
                        txtUserName=sdr["USER_NAME"].ToString(),
                        txtUserGroup=sdr["USER_GROUP"].ToString(),
                        txtUserType=sdr["USER_TYPE"].ToString(),
                        count=rowCount
                    };
                    
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("error" + e.Message);

                ViewData["lblMessage"] = "ERROR: " + e.Message;
            }
            finally
            {
                if(con != null)
                {
                    con.Close();
                }
                con = null;
            }
            //System.Diagnostics.Trace.WriteLine("############### USER COUNT = " + userModel.count);
            return userModel;
        }


        private UserModel getUserDetails(String loginName)
        {
            int rowCount = 0;
            UserModel userModel = null;
            MySqlConnection con = new MySqlConnection(CustUtility.CONN_STR_SQL);
            try
            {
                String sSQL = "SELECT USER_NAME, USER_ID,IS_LOCKED,PW_RESET_FLAG,PW_RESET_DATE,PW_SALT ,USER_PASSWORD FROM TBL_USER_DH WHERE USER_ID = '" + loginName + "'";
                MySqlCommand cmd = new MySqlCommand(sSQL);

                cmd.Connection = con;
                con.Open();
                MySqlDataReader sdr = cmd.ExecuteReader();
                while (sdr.Read())
                {
                    rowCount = rowCount + 1;
                    userModel = new UserModel
                    {
                        txtUserName = sdr["USER_NAME"].ToString(),
                        txtUserId = sdr["USER_ID"].ToString(),
                        isLocked = sdr["IS_LOCKED"].ToString(),
                        pwResetFlag = sdr["PW_RESET_FLAG"].ToString(),
                        pwResetDate = sdr["PW_RESET_DATE"].ToString(),
                        pwSalt= sdr["PW_SALT"].ToString(),
                        txtPassword= sdr["USER_PASSWORD"].ToString(),
                        count = rowCount
                    };

                }
            }
            catch (Exception e)
            {
                Console.WriteLine("error" + e.Message);

                ViewData["lblMessage"] = "ERROR: " + e.Message;
            }
            finally
            {
                if (con != null)
                {
                    con.Close();
                }
                con = null;
            }
            
            return userModel;
        }

        public string passwordEncryption(string password)
        {
            using (var algorithm = SHA512.Create()) 
            {
                var hashedBytes = algorithm.ComputeHash(Encoding.UTF8.GetBytes(password));

                return BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
            }
        }

    }
}
