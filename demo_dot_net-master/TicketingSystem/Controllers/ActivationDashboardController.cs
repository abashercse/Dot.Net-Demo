using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using TicketingSystem.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Http;

namespace TicketingSystem.Controllers
{
    public class ActivationDashboardController : Controller
    {
        [HttpGet]   
        public IActionResult Dashboard()
        {
            try
            {
                var UserInfo = HttpContext.Session.GetObjectFromJson<UserModel>("UserInfo");
                if (UserInfo == null)
                {
                    return RedirectToAction("Error", "Home");
                }
                ViewData["lblWelcome"] = "Welcome, " + HttpContext.Session.GetString("UserID");
                ActivationDashboardModel activationDashboardModel = new ActivationDashboardModel
                {
                    txtExecutionDate = DateTime.Now.ToString("dd-MMM-yyyy"),
                    listTicketSummary = PopulateTicketSummary(UserInfo.txtUserGroup),
                    listReqMaster = PopulateActivationRequestList(UserInfo.txtUserGroup, 0, 30),
                    executor = PopulateExecutorList(UserInfo.txtUserGroup)
                };
                HttpContext.Session.SetObjectAsJson("ACT_DASH_BOARD_MODEL", activationDashboardModel);
                return View(activationDashboardModel);
            }
            catch (Exception e)
            {
                ViewData["lblMessage"] = "ERROR: " + e.Message;
                return View();
            }           
        }       
        [HttpPost]
        public IActionResult Dashboard(ActivationDashboardModel activationDashboardModel)
        {
            try
            {
                var UserInfo = HttpContext.Session.GetObjectFromJson<UserModel>("UserInfo");
                if (UserInfo == null)
                {
                    return RedirectToAction("Error", "Home");
                }
                ViewData["lblWelcome"] = "Welcome, " + HttpContext.Session.GetString("UserID");
                System.Diagnostics.Trace.WriteLine("############### City list from controller = " + activationDashboardModel.citylist+":Village list:"+ activationDashboardModel.villlist);

                activationDashboardModel.listTeamSummary = this.btnViewTeamSummary_Click(UserInfo.txtUserGroup, activationDashboardModel.txtExecutionDate, activationDashboardModel.selectedExecutor);
                var SessionActDashBoard = HttpContext.Session.GetObjectFromJson<ActivationDashboardModel>("ACT_DASH_BOARD_MODEL");

                activationDashboardModel.executor = SessionActDashBoard.executor;
                activationDashboardModel.listReqMaster = SessionActDashBoard.listReqMaster;
                activationDashboardModel.listTicketSummary = SessionActDashBoard.listTicketSummary;

                return View(activationDashboardModel);
            }
            catch (Exception e)
            {

                ViewData["lblMessage"] = "ERROR: " + e.Message;
                return View();
            }
        }
        private List<ActivationTeamSummaryModel> btnViewTeamSummary_Click(string sUserGroup,string txtExecutionDate,string selectedExecutor)
        {
            List<ActivationTeamSummaryModel> list = new List<ActivationTeamSummaryModel>();            
            MySqlConnection con = new MySqlConnection(CustUtility.CONN_STR_SQL);
            try
            {
                string sSQL;
                string sExecDateFrom = Convert.ToDateTime(txtExecutionDate).ToString("yyyy-MM-dd");
                string sExecDateTo = Convert.ToDateTime(txtExecutionDate).AddDays(1).ToString("yyyy-MM-dd");

                sSQL = "SELECT a.User_ID, IFNULL(b.TOTAL_ASSIGNED, 0) 'TOTAL_ASSIGNED', IFNULL(c.TOTAL_CLOSED, 0) 'TOTAL_CLOSED', ";
                sSQL += "      IFNULL(d.TOTAL_HOLD, 0) 'TOTAL_HOLD', IFNULL(e.TOTAL_REJECTED, 0) 'TOTAL_REJECTED' ";
                sSQL += "FROM (SELECT * FROM TBL_USER WHERE USER_GROUP = '" + sUserGroup + "') a ";
                sSQL += "LEFT OUTER JOIN ";
                sSQL += "( ";
                sSQL += "  SELECT TICKET_ASSIGN_USER, COUNT(*) 'TOTAL_ASSIGNED' FROM TBL_ACTIVATION_REQ_MASTER ";
                sSQL += "  WHERE TICKET_ASSIGN_DATE >= '" + sExecDateFrom + "' AND TICKET_ASSIGN_DATE < '" + sExecDateTo + "' ";
                sSQL += "  AND ACTIVATION_TEAM = '" + sUserGroup + "' ";
                sSQL += "  GROUP BY TICKET_ASSIGN_USER ";
                sSQL += ") b ON a.USER_ID = b.TICKET_ASSIGN_USER ";
                sSQL += "LEFT OUTER JOIN ";
                sSQL += "( ";
                sSQL += "  SELECT TICKET_CLOSE_USER, COUNT(*) 'TOTAL_CLOSED' FROM TBL_ACTIVATION_REQ_MASTER ";
                sSQL += "  WHERE TICKET_CLOSE_DATE >= '" + sExecDateFrom + "' AND TICKET_CLOSE_DATE < '" + sExecDateTo + "' ";
                sSQL += "  AND ACTIVATION_TEAM = '" + sUserGroup + "' ";
                sSQL += "  GROUP BY TICKET_CLOSE_USER ";
                sSQL += ") c ON a.USER_ID = c.TICKET_CLOSE_USER ";
                sSQL += "LEFT OUTER JOIN ";
                sSQL += "( ";
                sSQL += "  SELECT TICKET_HOLD_USER, COUNT(*) 'TOTAL_HOLD' FROM TBL_ACTIVATION_REQ_MASTER ";
                sSQL += "  WHERE TICKET_HOLD_DATE >= '" + sExecDateFrom + "' AND TICKET_HOLD_DATE < '" + sExecDateTo + "' ";
                sSQL += "  AND ACTIVATION_TEAM = '" + sUserGroup + "' ";
                sSQL += "  GROUP BY TICKET_HOLD_USER ";
                sSQL += ") d ON a.USER_ID =  d.TICKET_HOLD_USER ";
                sSQL += "LEFT OUTER JOIN ";
                sSQL += "( ";
                sSQL += "  SELECT TICKET_REJECT_USER, COUNT(*) 'TOTAL_REJECTED' FROM TBL_ACTIVATION_REQ_MASTER ";
                sSQL += "  WHERE TICKET_REJECT_DATE >= '" + sExecDateFrom + "' AND TICKET_REJECT_DATE < '" + sExecDateTo + "' ";
                sSQL += "  AND ACTIVATION_TEAM = '" + sUserGroup + "' ";
                sSQL += "  GROUP BY TICKET_REJECT_USER ";
                sSQL += ") e ON a.USER_ID =  e.TICKET_REJECT_USER ";
                if (!String.IsNullOrEmpty(selectedExecutor))
                {
                    sSQL += "WHERE a.USER_ID = '" + selectedExecutor + "'";
                }

                MySqlCommand cmd = new MySqlCommand(sSQL);
                cmd.Connection = con;
                con.Open();
                MySqlDataReader sdr = cmd.ExecuteReader();
                while (sdr.Read())
                {
                    ActivationTeamSummaryModel model = new ActivationTeamSummaryModel
                    { 
                        userId = sdr["USER_ID"].ToString(),
                        totalAssigned = sdr["TOTAL_ASSIGNED"].ToString(),
                        totalClosed = sdr["TOTAL_CLOSED"].ToString(),
                        totalHold = sdr["TOTAL_HOLD"].ToString(),                        
                        totalRejected = sdr["TOTAL_REJECTED"].ToString()
                    };
                    list.Add(model);
                }
            }
            catch(Exception e)
            {
                throw e;
            }
            finally
            {
                if (con != null)
                {
                    con.Close();
                }
                con = null;
            }
            return list;
        }
        private List<ActivationTicketSummaryModel> PopulateTicketSummary(string sUserGroup)
        {
            string sSQL, sReqDateFrom, sReqDateTo;
            sReqDateFrom = DateTime.Now.AddDays(-2).ToString("yyyy-MM-dd");
            sReqDateTo = DateTime.Now.AddDays(1).ToString("yyyy-MM-dd");

            sSQL = "SELECT DATE_FORMAT(a.REQUEST_DATE, '%d-%b-%Y') 'REQUEST_DATE', IFNULL(a.TOTAL_RECEIVED, 0) 'TOTAL_RECEIVED', ";
            sSQL += "       IFNULL(b.TOTAL_ASSIGNED, 0) 'TOTAL_ASSIGNED', IFNULL(c.TOTAL_CLOSED, 0) 'TOTAL_CLOSED', ";
            sSQL += "       IFNULL(d.TOTAL_HOLD, 0) 'TOTAL_HOLD', IFNULL(e.TOTAL_REJECTED, 0) 'TOTAL_REJECTED', ";
            sSQL += "       IFNULL(f.TOTAL_PENDING, 0) 'TOTAL_PENDING' ";
            sSQL += "FROM ";
            sSQL += "( ";
            sSQL += "  SELECT DATE(TICKET_CREATE_DATE) 'REQUEST_DATE', COUNT(*) 'TOTAL_RECEIVED' FROM TBL_ACTIVATION_REQ_MASTER ";
            sSQL += "  WHERE TICKET_CREATE_DATE >= '" + sReqDateFrom + "' AND TICKET_CREATE_DATE <= '" + sReqDateTo + "' ";
            sSQL += "  AND ACTIVATION_TEAM = '" + sUserGroup + "' ";
            sSQL += "  GROUP BY DATE(TICKET_CREATE_DATE) ";
            sSQL += ") a ";
            sSQL += "LEFT OUTER JOIN ";
            sSQL += "( ";
            sSQL += "  SELECT DATE(TICKET_CREATE_DATE) 'TICKET_CREATE_DATE', COUNT(*) 'TOTAL_ASSIGNED' FROM TBL_ACTIVATION_REQ_MASTER ";
            sSQL += "  WHERE TICKET_CREATE_DATE >= '" + sReqDateFrom + "' AND TICKET_CREATE_DATE <= '" + sReqDateTo + "' AND TICKET_STATUS = 'ASSIGNED' ";
            sSQL += "  AND ACTIVATION_TEAM = '" + sUserGroup + "' ";
            sSQL += "  GROUP BY DATE(TICKET_CREATE_DATE) ";
            sSQL += ") b ON a.REQUEST_DATE = b.TICKET_CREATE_DATE ";
            sSQL += "LEFT OUTER JOIN ";
            sSQL += "( ";
            sSQL += "  SELECT DATE(TICKET_CREATE_DATE) 'TICKET_CREATE_DATE', COUNT(*) 'TOTAL_CLOSED' FROM TBL_ACTIVATION_REQ_MASTER ";
            sSQL += "  WHERE TICKET_CREATE_DATE >= '" + sReqDateFrom + "' AND TICKET_CREATE_DATE <= '" + sReqDateTo + "' AND TICKET_STATUS = 'CLOSE' ";
            sSQL += "  AND ACTIVATION_TEAM = '" + sUserGroup + "' ";
            sSQL += "  GROUP BY DATE(TICKET_CREATE_DATE) ";
            sSQL += ") c ON a.REQUEST_DATE = c.TICKET_CREATE_DATE ";
            sSQL += "LEFT OUTER JOIN ";
            sSQL += "( ";
            sSQL += "  SELECT DATE(TICKET_CREATE_DATE) 'TICKET_CREATE_DATE', COUNT(*) 'TOTAL_HOLD' FROM TBL_ACTIVATION_REQ_MASTER ";
            sSQL += "  WHERE TICKET_CREATE_DATE >= '" + sReqDateFrom + "' AND TICKET_CREATE_DATE <= '" + sReqDateTo + "' AND TICKET_STATUS = 'HOLD' ";
            sSQL += "  AND ACTIVATION_TEAM = '" + sUserGroup + "' ";
            sSQL += "  GROUP BY DATE(TICKET_CREATE_DATE) ";
            sSQL += ") d ON a.REQUEST_DATE =  d.TICKET_CREATE_DATE ";
            sSQL += "LEFT OUTER JOIN ";
            sSQL += "( ";
            sSQL += "  SELECT DATE(TICKET_CREATE_DATE) 'TICKET_CREATE_DATE', COUNT(*) 'TOTAL_REJECTED' FROM TBL_ACTIVATION_REQ_MASTER ";
            sSQL += "  WHERE TICKET_CREATE_DATE >= '" + sReqDateFrom + "' AND TICKET_CREATE_DATE <= '" + sReqDateTo + "' AND TICKET_STATUS = 'REJECTED' ";
            sSQL += "  AND ACTIVATION_TEAM = '" + sUserGroup + "' ";
            sSQL += "  GROUP BY DATE(TICKET_CREATE_DATE) ";
            sSQL += ") e ON a.REQUEST_DATE =  e.TICKET_CREATE_DATE ";
            sSQL += "LEFT OUTER JOIN ";
            sSQL += "( ";
            sSQL += "  SELECT DATE(TICKET_CREATE_DATE) 'TICKET_CREATE_DATE', COUNT(*) 'TOTAL_PENDING' FROM TBL_ACTIVATION_REQ_MASTER ";
            sSQL += "  WHERE TICKET_CREATE_DATE >= '" + sReqDateFrom + "' AND TICKET_CREATE_DATE <= '" + sReqDateTo + "' AND TICKET_STATUS = 'OPEN' ";
            sSQL += "  AND ACTIVATION_TEAM = '" + sUserGroup + "' ";
            sSQL += "  GROUP BY DATE(TICKET_CREATE_DATE) ";
            sSQL += ") f ON a.REQUEST_DATE =  f.TICKET_CREATE_DATE ";
            sSQL += "ORDER BY a.REQUEST_DATE DESC ";

            List<ActivationTicketSummaryModel> list = new List<ActivationTicketSummaryModel>();

            MySqlConnection con = new MySqlConnection(CustUtility.CONN_STR_SQL);
            try
            {
                MySqlCommand cmd = new MySqlCommand(sSQL);
                cmd.Connection = con;
                con.Open();
                MySqlDataReader sdr = cmd.ExecuteReader();
                while (sdr.Read())
                {
                    ActivationTicketSummaryModel model = new ActivationTicketSummaryModel
                    {
                        requestDate = sdr["REQUEST_DATE"].ToString(),
                        totalAssigned = sdr["TOTAL_ASSIGNED"].ToString(),
                        totalClosed = sdr["TOTAL_CLOSED"].ToString(),
                        totalHold = sdr["TOTAL_HOLD"].ToString(),
                        totalPending = sdr["TOTAL_PENDING"].ToString(),
                        totalReceived = sdr["TOTAL_RECEIVED"].ToString(),
                        totalRejected = sdr["TOTAL_REJECTED"].ToString()
                    };
                    list.Add(model);
                }
            }
            catch (Exception e)
            {
                throw e;
            }
            finally
            {
                if (con != null)
                {
                    con.Close();
                }
                con = null;
            }
            return list;
        }
        private List<ActivationReqMasterModel> PopulateActivationRequestList(string sUserGroup,int limit,int pageSize)
        {            
            List<ActivationReqMasterModel> list = new List<ActivationReqMasterModel>();
            using (MySqlConnection con = new MySqlConnection(CustUtility.CONN_STR_SQL))
                try
                {
                    {
                        string sSQL;
                        sSQL = "SELECT TICKET_CREATE_DATE 'REQUEST_DATE', TICKET_NO 'TICKET_NO', ";
                        sSQL += "SERVICE_NAME 'SERVICE_NAME', PRODUCT_NAME 'PRODUCT_NAME', IS_URGENT 'URGENT', ";
                        sSQL += "TICKET_CREATE_USER 'REQUESTER', TICKET_STATUS 'TICKET_STATUS', TICKET_ASSIGN_USER 'ASSIGNED_USER' ";
                        sSQL += "FROM TBL_ACTIVATION_REQ_MASTER ";
                        sSQL += "WHERE ACTIVATION_TEAM = '" + sUserGroup + "' ";
                        sSQL += "ORDER BY TICKET_CREATE_DATE DESC ";
                        sSQL += "LIMIT "+limit+","+ pageSize;
                        using (MySqlCommand cmd = new MySqlCommand(sSQL))
                        {
                            cmd.Connection = con;
                            con.Open();
                            using (MySqlDataReader sdr = cmd.ExecuteReader())
                            {
                                while (sdr.Read())
                                {
                                    ActivationReqMasterModel reqMaster = new ActivationReqMasterModel 
                                    {
                                        ticketCreateDate = sdr["REQUEST_DATE"].ToString(),
                                        ticketNo = sdr["TICKET_NO"].ToString(),
                                        serviceName = sdr["SERVICE_NAME"].ToString(),
                                        productName = sdr["PRODUCT_NAME"].ToString(),
                                        isUrgent = sdr["URGENT"].ToString(),
                                        ticketCreateUser = sdr["REQUESTER"].ToString(),
                                        ticketStatus = sdr["TICKET_STATUS"].ToString(),
                                        ticketAssignUser = sdr["ASSIGNED_USER"].ToString(),
                                        alarm = ShowAlarm(sdr["REQUEST_DATE"], sdr["TICKET_STATUS"].ToString())
                                    };
                                    list.Add(reqMaster);
                                }
                            }
                            con.Close();
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    con.Close();
                }
            return list;
        }
        private static List<SelectListItem> PopulateExecutorList(string sUserGroup)
        {
            List<SelectListItem> items = new List<SelectListItem>();
            using (MySqlConnection con = new MySqlConnection(CustUtility.CONN_STR_SQL))
            {
                string sSQL = "SELECT USER_ID FROM TBL_USER WHERE USER_GROUP = '" + sUserGroup + "'";
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
                                Text = sdr["USER_ID"].ToString(),
                                Value = sdr["USER_ID"].ToString()
                            });
                        }
                    }
                    con.Close();
                }
            }
            return items;
        }
        protected string ShowAlarm(object oParamReqDate, string oParamTicketStatus)
        {
            string sTicketStatus = oParamTicketStatus;
            DateTime dRequestDate = (DateTime)oParamReqDate;
            TimeSpan tsTicketLifeTime = DateTime.Now.Subtract(dRequestDate);

            if ((sTicketStatus == "CLOSE") || (sTicketStatus == "REJECTED"))
            {
                return "bullet_white.png";
            }
            else
            {
                if (tsTicketLifeTime.TotalHours <= 18.0)
                {
                    return "bullet_green.png";
                }
                else if ((tsTicketLifeTime.TotalHours > 18.0) && (tsTicketLifeTime.TotalHours <= 24.0))
                {
                    return "bullet_yellow.png";
                }
                else if (tsTicketLifeTime.TotalHours > 24.0)
                {
                    return "bullet_red.png";
                }
                else
                {
                    return "bullet_white.png";
                }
            }
        }
        public IActionResult Btn_Management()
        {
            return RedirectToAction("Management", "ChannelManagement");
        }
    }
}