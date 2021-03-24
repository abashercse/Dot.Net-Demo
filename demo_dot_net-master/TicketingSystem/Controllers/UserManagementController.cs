using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Server.HttpSys;
using MySql.Data.MySqlClient;
using TicketingSystem.Models;

namespace TicketingSystem.Controllers
{
    public class UserManagementController : Controller
    {
        [HttpPost]
        public IActionResult UserMgt()
        {
            try
            {
                var UserInfo = HttpContext.Session.GetObjectFromJson<UserModel>("UserInfo");
                if (UserInfo == null)
                {
                    return RedirectToAction("Error", "Home");
                }
                ViewData["lblWelcome"] = "Welcome, " + HttpContext.Session.GetString("UserID");
                UserManagementViewModel model = new UserManagementViewModel
                {
                    userList = PopulateUserList(),
                    dhList = PopulateDhList(),
                    userDetails = new UserDetailsMasterModel
                    {
                        userName = "",
                        userId = "",
                        createdOn = "",
                        dhName = "Please select ..."
                    }
                };
                HttpContext.Session.SetObjectAsJson("USER_MGT", model);
                ViewData["Password"] = "";
                ViewData["txtUserId_Enabled"] = "enabled";
                ViewData["btnCreate_Enabled"] = "enabled";
                ViewData["btnUpdate_Enabled"] = "disabled";
                ViewData["btnDelete_Enabled"] = "disabled";
                return View(model);
            }
            catch (Exception e)
            {
                ViewData["lblMessage"] = "ERROR: " + e.Message;
                return View();
            }
        }

        private List<SelectListItem> PopulateUserList()
        {
            List<SelectListItem> itemList = new List<SelectListItem>();
            string sSQL;
            sSQL = "SELECT USER_NAME FROM TBL_USER_DH;";
            MySqlConnection connection = new MySqlConnection(CustUtility.CONN_STR_SQL);
            try
            {
                MySqlCommand command = new MySqlCommand(sSQL);
                command.Connection = connection;
                connection.Open();
                MySqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    SelectListItem item = new SelectListItem
                    {
                        Text = reader["USER_NAME"].ToString(),
                        Value = reader["USER_NAME"].ToString()
                    };
                    itemList.Add(item);
                }
            }
            catch (Exception e)
            {
                throw e;
            }
            finally
            {
                if (connection != null)
                {
                    connection.Close();
                }
                connection = null;
            }
            return itemList;
        }

        private List<SelectListItem> PopulateDhList()
        {
            List<SelectListItem> itemList = new List<SelectListItem>();
            string sSQL;
            sSQL = "SELECT CHANNEL_NAME FROM TBL_LIST_CHANNEL WHERE CHANNEL_TYPE='Distribution House' ORDER BY CHANNEL_NAME;";
            MySqlConnection connection = new MySqlConnection(CustUtility.CONN_STR_SQL);
            try
            {
                MySqlCommand command = new MySqlCommand(sSQL);
                command.Connection = connection;
                connection.Open();
                MySqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    SelectListItem item = new SelectListItem
                    {
                        Text = reader["CHANNEL_NAME"].ToString(),
                        Value = reader["CHANNEL_NAME"].ToString()
                    };
                    itemList.Add(item);
                }
            }
            catch (Exception e)
            {
                throw e;
            }
            finally
            {
                if (connection != null)
                {
                    connection.Close();
                }
                connection = null;
            }
            return itemList;
        }

        [HttpPost]
        public ActionResult UserChange(UserManagementViewModel model)
        {
            ViewData["txtUserId_Enabled"] = "disabled";
            ViewData["btnCreate_Enabled"] = "disabled";
            ViewData["btnUpdate_Enabled"] = "enabled";
            ViewData["btnDelete_Enabled"] = "enabled";
            try
            {
                var UserInfo = HttpContext.Session.GetObjectFromJson<UserModel>("UserInfo");
                if (UserInfo == null)
                {
                    return RedirectToAction("Error", "Home");
                }
                ViewData["lblWelcome"] = "Welcome, " + HttpContext.Session.GetString("UserID");
                var SessionUserMgt = HttpContext.Session.GetObjectFromJson<UserManagementViewModel>("USER_MGT");
                model.userList = SessionUserMgt.userList;
                model.userDetails = GetUserDetails(model.selectedUser);
                model.dhList = SessionUserMgt.dhList;
                HttpContext.Session.SetObjectAsJson("USER_MGT", model);
                ViewData["Password"] = "";
                return View("UserMgt", model);
            }
            catch (Exception e)
            {
                ViewData["lblMessage"] = "ERROR: " + e.Message;
                return View();
            }
        }

        private UserDetailsMasterModel GetUserDetails(string UserID)
        {
            UserDetailsMasterModel master = new UserDetailsMasterModel();
            string sSQL;
            sSQL = "SELECT * FROM TBL_USER_DH ";
            sSQL += "WHERE USER_NAME = '" + UserID + "';";
            MySqlConnection connection = new MySqlConnection(CustUtility.CONN_STR_SQL);
            try
            {
                MySqlCommand command = new MySqlCommand(sSQL);
                command.Connection = connection;
                connection.Open();
                MySqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    master.userName = reader["USER_NAME"].ToString();
                    master.userId = reader["USER_ID"].ToString();
                    master.createdOn = Convert.ToDateTime(reader["CREATED_ON"].ToString()).ToString("dd - MMM - yyyy hh: mm:ss tt");
                    master.dhName = reader["DH_NAME"].ToString();
                }
            }
            catch (Exception e)
            {
                throw e;
            }
            finally
            {
                if (connection != null)
                {
                    connection.Close();
                }
                connection = null;
            }
            return master;
        }

        [HttpPost]
        public IActionResult BtnReset_Click(UserManagementViewModel model)
        {
            try
            {
                var UserInfo = HttpContext.Session.GetObjectFromJson<UserModel>("UserInfo");
                if (UserInfo == null)
                {
                    return RedirectToAction("Error", "Home");
                }
                ViewData["lblWelcome"] = "Welcome, " + HttpContext.Session.GetString("UserID");
                model.userList = PopulateUserList();
                model.dhList = PopulateDhList();
                model.userDetails = new UserDetailsMasterModel
                {
                    userName = "",
                    userId = "",
                    createdOn = "",
                    dhName = "Please select ..."
                };
                model.selectedUser = "";
                model.selectedDh = "";
                HttpContext.Session.SetObjectAsJson("USER_MGT", model);
                ViewData["Password"] = "";
                ViewData["txtUserId_Enabled"] = "enabled";
                ViewData["btnCreate_Enabled"] = "enabled";
                ViewData["btnUpdate_Enabled"] = "disabled";
                ViewData["btnDelete_Enabled"] = "disabled";
                return View("UserMgt", model);
            }
            catch (Exception e)
            {
                ViewData["lblMessage"] = "ERROR: " + e.Message;
                return View("UserMgt", model);
            }
        }

        [HttpPost]
        public IActionResult BtnUpdate_Click(UserManagementViewModel model, string txtUserName, string txtUserId)
        {
            string UpdatedBy = HttpContext.Session.GetString("UserID");
            ViewData["Password"] = "";
            ViewData["txtUserId_Enabled"] = "enabled";
            ViewData["btnCreate_Enabled"] = "disabled";
            ViewData["btnUpdate_Enabled"] = "enabled";
            ViewData["btnDelete_Enabled"] = "enabled";
            var SessionUserMgt = HttpContext.Session.GetObjectFromJson<UserManagementViewModel>("USER_MGT");
            if (string.IsNullOrEmpty(txtUserName) && string.IsNullOrEmpty(txtUserId))
            {
                ViewData["lblMessage"] = "ERROR: " + "UserName and UserID is required!!";
                model.userList = SessionUserMgt.userList;
                model.dhList = SessionUserMgt.dhList;
                model.userDetails = SessionUserMgt.userDetails;
                return View("UserMgt", model);
            }

            if (string.IsNullOrEmpty(model.selectedDh) && !string.IsNullOrEmpty(txtUserName))
            {
                try
                {
                    using (MySqlConnection connection = new MySqlConnection(CustUtility.CONN_STR_SQL))
                    {
                        connection.Open();
                        using (MySqlTransaction transaction = connection.BeginTransaction())
                        {
                            string sSQL;
                            sSQL = "UPDATE TBL_USER_DH ";
                            sSQL += "SET ";
                            sSQL += "DH_NAME = @DHName , USER_NAME= @UserName , UPDATED_BY ='" + UpdatedBy + "' , UPDATED_ON='" + DateTime.Now.ToString("yyyy-MM-dd H:mm:ss") + "' ";
                            sSQL += "WHERE USER_NAME='" + model.selectedUser + "'";
                            int nResult = 0;

                            using (MySqlCommand command = new MySqlCommand(sSQL))
                            {
                                command.Connection = connection;
                                command.Parameters.AddWithValue("@DHName", model.selectedDh);
                                command.Parameters.AddWithValue("@UserName", txtUserName);
                                command.Transaction = transaction;
                                nResult = command.ExecuteNonQuery();
                            }
                            transaction.Commit();
                        }
                        connection.Close();
                    }
                }
                catch (Exception e)
                {
                    ViewData["lblMessage"] = "ERROR: " + e.Message;
                }
                ViewData["lblMessage"] = "MESSAGE: " + "User information updated successfully.";
                model.userList = PopulateUserList();
                model.dhList = PopulateDhList();
                model.userDetails = GetUserDetails(txtUserName);
                return View("UserMgt", model);
            }
            else
            {
                ViewData["lblMessage"] = "ERROR: " + "User Name and Distribution House required!";
                model.userList = SessionUserMgt.userList;
                model.dhList = SessionUserMgt.dhList;
                model.userDetails = SessionUserMgt.userDetails;
                return View("UserMgt", model);
            }
        }

        [HttpPost]
        public IActionResult BtnCreate_Click(UserManagementViewModel model, string txtUserName, string txtUserId)
        {
            string CreatedBy = HttpContext.Session.GetString("UserID");
            ViewData["Password"] = "";
            ViewData["txtUserId_Enabled"] = "enabled";
            ViewData["btnCreate_Enabled"] = "enabled";
            ViewData["btnUpdate_Enabled"] = "disabled";
            ViewData["btnDelete_Enabled"] = "disabled";
            var SessionUserMgt = HttpContext.Session.GetObjectFromJson<UserManagementViewModel>("USER_MGT");
            if (string.IsNullOrEmpty(txtUserName) && string.IsNullOrEmpty(txtUserId))
            {
                ViewData["lblMessage"] = "ERROR: " + "UserName and UserID is required!!";
                ViewData["Password"] = "";
                model.userList = SessionUserMgt.userList;
                model.dhList = SessionUserMgt.dhList;
                model.userDetails = SessionUserMgt.userDetails;
                return View("UserMgt", model);
            }
            try
            {
                using (MySqlConnection connection = new MySqlConnection(CustUtility.CONN_STR_SQL))
                {
                    if (!IsUserExist(txtUserId))
                    {
                        string password = CustUtility.GeneratePassword();
                        Guid usetGuid = System.Guid.NewGuid();
                        string passwordSalt = usetGuid.ToString();
                        string newHashedPassword = CustUtility.HashSHA1(password + passwordSalt);
                        connection.Open();

                        using (MySqlTransaction transaction = connection.BeginTransaction())
                        {
                            string sSQL;
                            sSQL = "INSERT INTO TBL_USER_DH(DH_NAME,USER_NAME,USER_ID,USER_PASSWORD,PW_SALT,PW_RESET_FLAG,UPDATED_BY,UPDATED_ON,CREATED_BY,CREATED_ON)";
                            sSQL += "VALUES(@DHName,@UserName,@UserID,@Password,@pw_salt,0,'" + CreatedBy + "','" + DateTime.Now.ToString("yyyy-MM-dd H:mm:ss") + "','" + CreatedBy + "','" + DateTime.Now.ToString("yyyy-MM-dd H:mm:ss") + "')";
                            int nResult = 0;

                            using (MySqlCommand command = new MySqlCommand(sSQL))
                            {
                                command.Connection = connection;
                                command.Parameters.AddWithValue("@DHName", model.selectedDh);
                                command.Parameters.AddWithValue("@UserName", txtUserName);
                                command.Parameters.AddWithValue("@UserID", txtUserId);
                                command.Parameters.AddWithValue("@Password", newHashedPassword);
                                command.Parameters.AddWithValue("@pw_salt", passwordSalt);
                                command.Transaction = transaction;
                                nResult = command.ExecuteNonQuery();
                            }

                            transaction.Commit();
                        }
                        connection.Close();
                        ViewData["Password"] = "Password: " + password;
                        ViewData["lblMessage"] = "MESSAGE: " + "User create successfully.";
                    }
                    else
                    {
                        ViewData["Password"] = "";
                        ViewData["lblMessage"] = "ERROR: " + "This User is already exist";
                    }
                }
            }
            catch (Exception e)
            {
                ViewData["lblMessage"] = "ERROR: " + e.Message;
            }
            model.userList = PopulateUserList();
            model.userDetails = new UserDetailsMasterModel
            {
                userName = "",
                userId = "",
                createdOn = "",
                dhName = "Please select ..."
            };
            model.dhList = PopulateDhList();
            return View("UserMgt", model);
        }

        [HttpPost]
        public IActionResult BtnDelete_Click(UserManagementViewModel model)
        {
            string UpdatedBy = HttpContext.Session.GetString("UserID");
            ViewData["Password"] = "";
            ViewData["txtUserId_Enabled"] = "enabled";
            ViewData["btnCreate_Enabled"] = "enabled";
            ViewData["btnUpdate_Enabled"] = "disabled";
            ViewData["btnDelete_Enabled"] = "disabled";
            try
            {
                using (MySqlConnection connection = new MySqlConnection(CustUtility.CONN_STR_SQL))
                {
                    if (!string.IsNullOrEmpty(model.selectedUser))
                    {
                        connection.Open();
                        using (MySqlTransaction transaction = connection.BeginTransaction())
                        {
                            string sSQL;
                            sSQL = "DELETE FROM TBL_USER_DH ";
                            sSQL += "WHERE USER_NAME='" + model.selectedUser + "'";
                            int nResult = 0;
                            using (MySqlCommand command = new MySqlCommand(sSQL))
                            {
                                command.Connection = connection;
                                command.Transaction = transaction;
                                nResult = command.ExecuteNonQuery();
                            }
                            transaction.Commit();

                        }
                        connection.Close();
                        ViewData["lblMessage"] = "MESSAGE: " + "User: " + model.selectedUser + " deleted successfully.";
                    }
                    else
                    {
                        ViewData["lblMessage"] = "ERROR: " + "Select User from dropdown list!";
                    }
                }
            }
            catch (Exception e)
            {
                ViewData["lblMessage"] = "ERROR: " + e.Message;
            }
            model.userList = PopulateUserList();
            model.userDetails = new UserDetailsMasterModel
            {
                userName = "",
                userId = "",
                createdOn = "",
                dhName = "Please select ..."
            };
            model.dhList = PopulateDhList();
            model.selectedUser = "";
            model.selectedDh = "";
            return View("UserMgt", model);
        }

        private bool IsUserExist(string userId)
        {
            string sSQL;
            sSQL = "SELECT ID FROM TBL_USER_DH ";
            sSQL += "WHERE USER_ID = '" + userId + "'";

            int count = 0;
            MySqlConnection connection = new MySqlConnection(CustUtility.CONN_STR_SQL);
            try
            {
                MySqlCommand command = new MySqlCommand(sSQL);
                command.Connection = connection;
                connection.Open();
                MySqlDataReader reader = command.ExecuteReader();
                if (reader.Read())
                {
                    count++;
                }
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                if (connection != null)
                {
                    connection.Close();
                }
                connection = null;
            }
            if (count >= 1)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public IActionResult Btn_MyRequest()
        {
            return RedirectToAction("MyRequest", "MyRequest");
        }

        public IActionResult Btn_Dashboard()
        {
            return RedirectToAction("Dashboard", "ActivationDashboard");
        }

        public IActionResult Btn_Management()
        {
            return RedirectToAction("Management", "ChannelManagement");
        }
    }
}