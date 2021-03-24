using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MySql.Data.MySqlClient;
using TicketingSystem.Models;



namespace TicketingSystem.Controllers
{
    public class ChannelManagementController : Controller
    {
       
       public IActionResult Management()
        {
            try
            {
                var UserInfo = HttpContext.Session.GetObjectFromJson<UserModel>("UserInfo");
                if (UserInfo == null)
                {
                    return RedirectToAction("Error", "Home");
                }
                ViewData["lblWelcome"] = "Welcome, " + HttpContext.Session.GetString("UserID");
                ChannelManagementModel channelManagementModel = new ChannelManagementModel
                {

                    channelType = PopulateChanneltypeList(),
                 //   channelName = PopulateChannelnameList()
                 channelName=new List<SelectListItem>()

                };
                HttpContext.Session.SetObjectAsJson("ACT_CHANNEL_MODEL", channelManagementModel);
                return View(channelManagementModel);
            }
            catch (Exception e)
            {
                ViewData["lblMessage"] = "ERROR: " + e.Message;
                return View();
            }
        }
        public IActionResult ChanneltypeChange(string Channeltype)
        {
            try
            {
                var UserInfo = HttpContext.Session.GetObjectFromJson<UserModel>("UserInfo");
                if (UserInfo == null)
                {
                    return RedirectToAction("Error", "Home");
                }
                ViewData["lblWelcome"] = "Welcome, " + HttpContext.Session.GetString("UserID");
             /*   //ChannelManagementModel channelManagementModel = new ChannelManagementModel
                {

                    channelType = PopulateChanneltypeList(),
                    ddlChannelType= Channeltype,
                    channelName = PopulateChannelnameList(Channeltype)

                };*/
                var SessionChannelMgt = HttpContext.Session.GetObjectFromJson<ChannelManagementModel>("ACT_CHANNEL_MODEL");
                ChannelManagementModel model = new ChannelManagementModel
                {
                    channelType = SessionChannelMgt.channelType,
                    channelName = PopulateChannelnameList(Channeltype),
                    ddlChannelType= Channeltype
                };
                HttpContext.Session.SetObjectAsJson("CHANNEL_MGT", model);
                return View("Management",model);

            }
            catch (Exception e)
            {
                ViewData["lblMessage"] = "ERROR: " + e.Message;
                return View();
            }
        }
       
        private static List<SelectListItem> PopulateChanneltypeList()
        {
            List<SelectListItem> items = new List<SelectListItem>();
            using (MySqlConnection con = new MySqlConnection(CustUtility.CONN_STR_SQL))
            {
                string sSQL = "SELECT DISTINCT CHANNEL_TYPE FROM TBL_LIST_CHANNEL ORDER BY CHANNEL_TYPE";
                using (MySqlCommand cmd = new MySqlCommand(sSQL))
                {
                    cmd.Connection = con;
                    con.Open();
                    using (MySqlDataReader sdr = cmd.ExecuteReader())
                    {
                        while (sdr.Read())
                        {
                            items.Add(new SelectListItem
                            {
                                Text = sdr["CHANNEL_TYPE"].ToString(),
                                Value = sdr["CHANNEL_TYPE"].ToString()
                            });
                        }
                    }
                    con.Close();
                }
            }
            return items;
        }
        private static List<SelectListItem> PopulateChannelnameList(string channeltype)
        {
            List<SelectListItem> items = new List<SelectListItem>();
            using (MySqlConnection con = new MySqlConnection(CustUtility.CONN_STR_SQL))
            {
                string sSQL;
                sSQL = "SELECT * FROM TBL_LIST_CHANNEL ";
                sSQL += "WHERE CHANNEL_TYPE = '" + channeltype + "' ORDER BY CHANNEL_NAME";
                using (MySqlCommand cmd = new MySqlCommand(sSQL))
                {
                    cmd.Connection = con;
                    con.Open();
                    using (MySqlDataReader sdr = cmd.ExecuteReader())
                    {
                        while (sdr.Read())
                        {
                            items.Add(new SelectListItem
                            {
                                Text = sdr["CHANNEL_NAME"].ToString(),
                                Value = sdr["ID"].ToString()
                            });
                        }
                    }
                    con.Close();
                }
            }
            return items;
        }

        [HttpPost]
        public IActionResult btnCreate_Click(ChannelManagementModel model)
        {
            MySqlConnection con = new MySqlConnection(CustUtility.CONN_STR_SQL);

            var SessionChannelMgt = HttpContext.Session.GetObjectFromJson<ChannelManagementModel>("ACT_CHANNEL_MODEL");
            ChannelManagementModel model1 = new ChannelManagementModel
            {
                channelType = SessionChannelMgt.channelType,
                channelName = SessionChannelMgt.channelName,
                ddlChannelType = model.ddlChannelType,
                ddlChannelName=model.ddlChannelName
            };
            HttpContext.Session.SetObjectAsJson("CHANNEL_MGT", model1);
            //return View("Management", model1);
            try
            {
                if ((!string.IsNullOrEmpty(model.ddlChannelType)) && (!string.IsNullOrEmpty(model.ddlChannelName)))
                {
                    string sSQL;
                    string channelType = model.ddlChannelType;
                    string channelName = model.ddlChannelName;
                    sSQL = "INSERT INTO TBL_LIST_CHANNEL(CHANNEL_NAME,CHANNEL_TYPE)";
                    sSQL += "VALUES('" + channelName + "','" + channelType + "')";

                    MySqlCommand cmd = new MySqlCommand(sSQL);
                    cmd.Connection = con;
                    con.Open();
                    MySqlTransaction transaction = con.BeginTransaction();
                   // con.Open();
                    int nResult = cmd.ExecuteNonQuery();
                    transaction.Commit();
                    ViewData["lblMessage"] = "MESSAGE: " + "New channel created successfully.";


                }
                else
                {
                    ViewData["lblMessage"] = "ERROR: " + "New Channel Name and Channel Type Required! ";
                }
            }
            catch (Exception e)
            {
                ViewData["lblMessage"] = "ERROR: " + e.Message;
                return View("Management",model1 );
            }
            finally
            {
                if (con != null)
                {
                    con.Close();
                }
                con = null;
            }
            return View("Management", model1);
        }

        [HttpPost]
        public IActionResult BtnReset_Click()
        {
            return RedirectToAction("Management", "ChannelManagement");
        }


        [HttpPost]
        public IActionResult btnUpdate_Click(ChannelManagementModel model, string txtNewChannelName)

        {
            MySqlConnection con = new MySqlConnection(CustUtility.CONN_STR_SQL);

            var SessionChannelMgt = HttpContext.Session.GetObjectFromJson<ChannelManagementModel>("ACT_CHANNEL_MODEL");
            ChannelManagementModel model2 = new ChannelManagementModel
            {
                channelType = SessionChannelMgt.channelType,
                channelName = SessionChannelMgt.channelName,
                ddlChannelType = model.ddlChannelType,
                ddlChannelName = model.ddlChannelName
            };
            HttpContext.Session.SetObjectAsJson("CHANNEL_MGT", model2);
            try
            {
                if ((!string.IsNullOrEmpty(model.ddlChannelType)) && (!string.IsNullOrEmpty(model.ddlChannelName) && (!string.IsNullOrEmpty(txtNewChannelName))))
                {
                    string sSQL;
                    string channelType = model.ddlChannelType;
                    string channelName = model.ddlChannelName;
                    // var SessionUserMgt = HttpContext.Session.GetObjectFromJson<ChannelManagementModel>("CHANNEL_MGT");
                    sSQL = "UPDATE TBL_LIST_CHANNEL ";
                    sSQL += "SET ";
                    sSQL += "CHANNEL_NAME = @CH_NAME , CHANNEL_TYPE= @CH_TYPE ";
                    sSQL += "WHERE ID='" + channelName + "'";
                    // int nResult = 0;

                    // MySqlConnection con = new MySqlConnection(CustUtility.CONN_STR_SQL);
                   // MySqlCommand command = new MySqlCommand(sSQL);
                    MySqlCommand cmd = new MySqlCommand(sSQL);
                    cmd.Connection = con;
                    con.Open();
                    MySqlTransaction transaction = con.BeginTransaction();
                   // cmd.Transaction = transaction;
                    cmd.Parameters.AddWithValue("@CH_NAME", txtNewChannelName);
                    cmd.Parameters.AddWithValue("@CH_TYPE", channelType);
                   // con.Open();
                   // MySqlTransaction transaction = con.BeginTransaction();
                    //cmd.Transaction = transaction;
                    int nResult = cmd.ExecuteNonQuery();
                    transaction.Commit();
                    ViewData["lblMessage"] = "MESSAGE: " + "Channel Name and Channel Type updated successfully.";

                }
                else
                {
                    ViewData["lblMessage"] = "ERROR: " + "Channel Name and Channel Type required!";
                }
                // return View("Management", model2);

            }
            catch (Exception e)
            {
                ViewData["lblMessage"] = "ERROR: " + e.Message;
                return View("Management", model2);
            }
            finally
            {
                if (con != null)
                {
                    con.Close();
                }
                con = null;
            }

            //  ViewData["lblMessage"] = "MESSAGE: " + "Channel Name and Channel Type updated successfully.";
            return View("Management", model2);
        }

        [HttpPost]
        public IActionResult btnDelete_Click(ChannelManagementModel model)
        {
            MySqlConnection con = new MySqlConnection(CustUtility.CONN_STR_SQL);

            var SessionChannelMgt = HttpContext.Session.GetObjectFromJson<ChannelManagementModel>("ACT_CHANNEL_MODEL");
            ChannelManagementModel model3 = new ChannelManagementModel

           /* var SessionChannelMgt = HttpContext.Session.GetObjectFromJson<ChannelManagementModel>("ACT_CHANNEL_MODEL");
            ChannelManagementModel model3 = new ChannelManagementModel*/
            {
                channelType = SessionChannelMgt.channelType,
                channelName = SessionChannelMgt.channelName,
                ddlChannelType = model.ddlChannelType,
                ddlChannelName = model.ddlChannelName
            };
            HttpContext.Session.SetObjectAsJson("CHANNEL_MGT", model3);
            try
            {
                if (!string.IsNullOrEmpty(model.ddlChannelName))
                {
                    string sSQL;
                    string channelType = model.ddlChannelType;
                    string channelName = model.ddlChannelName;
                    //  var SessionUserMgt = HttpContext.Session.GetObjectFromJson<ChannelManagementModel>("CHANNEL_MGT");
                    sSQL = "DELETE FROM TBL_LIST_CHANNEL ";
                    sSQL += "WHERE ID='" + channelName + "'";
                    //  int nResult = 0;
                    //MySqlConnection con = new MySqlConnection(CustUtility.CONN_STR_SQL);
                    MySqlCommand cmd = new MySqlCommand(sSQL);
                    cmd.Connection = con;
                    //command.Parameters.AddWithValue("@CH_NAME", txtNewChannelName);
                    //command.Parameters.AddWithValue("@CH_TYPE", channelType);
                    con.Open();
                    MySqlTransaction transaction = con.BeginTransaction();
                    //command.Transaction = transaction;
                    int nResult = cmd.ExecuteNonQuery();
                    transaction.Commit();
                    ViewData["lblMessage"] = "MESSAGE: " + "Channel Name and Channel Type deleted successfully.";

                }
                else
                {
                    ViewData["lblMessage"] = "ERROR: " + "Channel Name and Channel Type required!";
                }
            }
            catch (Exception e)
                {
                    ViewData["lblMessage"] = "ERROR: " + e.Message;
                    return View("Management", model3);
                }
                finally
                {
                    if (con != null)
                    {
                        con.Close();
                    }
                    con = null;
                }
                return View("Management", model3);

            }
      public IActionResult btnReset_Click()
        {
            return Management();
        }

        public IActionResult Btn_MyRequest()
        {
            return RedirectToAction("MyRequest", "MyRequest");
        }

        public IActionResult Btn_Dashboard()
        {
            return RedirectToAction("Dashboard", "ActivationDashboard");
        }

        public IActionResult Btn_UserMgt()
        {
            return RedirectToAction("UserMgt", "UserManagement");
        }
        
    }

}