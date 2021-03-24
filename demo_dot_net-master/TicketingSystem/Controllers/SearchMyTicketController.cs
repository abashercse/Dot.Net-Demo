using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using TicketingSystem.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Http;
using MySqlX.XDevAPI;

namespace TicketingSystem.Controllers
{
    public class SearchMyTicketController : Controller
    {
        [HttpPost]
        public IActionResult MyTicket()
        {
            try
            {
                var UserInfo = HttpContext.Session.GetObjectFromJson<UserModel>("UserInfo");
                if (UserInfo == null)
                {
                    return RedirectToAction("Error", "Home");
                }
                ViewData["lblWelcome"] = "Welcome, " + HttpContext.Session.GetString("UserID");
                SearchMyTicketModel searchMyTicketModel = new SearchMyTicketModel
                {
                    ticketStatus = CustUtility.PopulateTicketStatusList()
                };
                return View(searchMyTicketModel);
            }
            catch (Exception e)
            {
                ViewData["lblMessage"] = "ERROR: " + e.Message;
                return View();
            }
        }
        [HttpPost]
        public IActionResult MyTicketSearch(SearchMyTicketModel mymodel)
        {
            try
            {
                var UserInfo = HttpContext.Session.GetObjectFromJson<UserModel>("UserInfo");
                if (UserInfo == null)
                {
                    return RedirectToAction("Error", "Home");
                }
                ViewData["lblWelcome"] = "Welcome, " + HttpContext.Session.GetString("UserID");
                mymodel.reqMstList = this.btnSearch_Click(mymodel.txtRequestDateTo,mymodel.txtRequestDateFrom,mymodel.ddlTicketStatus,mymodel.txtTicketNo);
                mymodel.ticketStatus = CustUtility.PopulateTicketStatusList();
                return View("MyTicket", mymodel);
            }
            catch (Exception e)
            {
                ViewData["lblMessage"] = "ERROR: " + e.Message;
                return View("MyTicket", mymodel);
            }
        }
        private List<ActivationReqMasterModel> btnSearch_Click(string txtExecutionDateTo, string txtExecutionDateFrom, string selectedStatus, string txtTicketNo)
        {
            List<ActivationReqMasterModel> list = new List<ActivationReqMasterModel>();
            MySqlConnection con = new MySqlConnection(CustUtility.CONN_STR_SQL);
                try
                {
                    string sSQL;
                    string sReqDateFrom, sReqDateTo;
                    string sSearchType = "OWN_TICKET";
                    string sUserID = HttpContext.Session.GetString("UserID");

                    sSQL = "SELECT TICKET_CREATE_DATE 'REQUEST_DATE', TICKET_NO 'TICKET_NO', ";
                    sSQL += "SERVICE_NAME 'SERVICE_NAME', TICKET_STATUS 'TICKET_STATUS', ";
                    sSQL += "TICKET_ASSIGN_USER 'ASSIGNED_USER', ACTIVATION_TEAM 'ACTIVATION_TEAM', ";
                    sSQL += "COMMENTS 'COMMENTS' ";
                    sSQL += "FROM TBL_ACTIVATION_REQ_MASTER ";
                    sSQL += "WHERE 1 = 1 ";
                    if (!String.IsNullOrEmpty(txtTicketNo))
                    {
                        sSQL += "AND TICKET_NO = '" + txtTicketNo + "' ";
                        sSearchType = "OTHERS_TICKET";
                    }
                    if (!String.IsNullOrEmpty(selectedStatus))
                    {
                        sSQL += "AND TICKET_STATUS = '" + selectedStatus + "' ";
                    sSearchType = "OWN_TICKET";
                    }
                    if (!String.IsNullOrEmpty(txtExecutionDateTo) && !String.IsNullOrEmpty(txtExecutionDateFrom))
                    {
                        sReqDateFrom = Convert.ToDateTime(txtExecutionDateFrom).ToString("yyyy-MM-dd");
                        sReqDateTo = Convert.ToDateTime(txtExecutionDateTo).AddDays(1).ToString("yyyy-MM-dd");
                        sSQL += "AND TICKET_CREATE_DATE >= '" + sReqDateFrom + "' AND TICKET_CREATE_DATE < '" + sReqDateTo + "' ";
                        sSearchType = "OWN_TICKET";
                    }
                    if (sSearchType == "OWN_TICKET")
                    {
                        sSQL += "AND TICKET_CREATE_USER = '" + sUserID + "' ";
                    }
                    sSQL += "ORDER BY TICKET_CREATE_DATE DESC";

                    MySqlCommand cmd = new MySqlCommand(sSQL);
                    cmd.Connection = con;
                    con.Open();
                    MySqlDataReader sdr = cmd.ExecuteReader();
                    while (sdr.Read())
                    {
                        ActivationReqMasterModel reqMaster = new ActivationReqMasterModel{
                            ticketCreateDate = sdr["REQUEST_DATE"].ToString(),
                            ticketNo = sdr["TICKET_NO"].ToString(),
                            serviceName = sdr["SERVICE_NAME"].ToString(),
                            ticketStatus = sdr["TICKET_STATUS"].ToString(),
                            ticketAssignUser = sdr["ASSIGNED_USER"].ToString(),
                            activationTeam= sdr["ACTIVATION_TEAM"].ToString(),
                            comments = sdr["COMMENTS"].ToString(),
                        };
                        list.Add(reqMaster);
                    }
                }
                catch (Exception ex)
                {
                    string s = ex.Message;
                    throw;
                }
                finally
                {
                    con.Close();
                }
            return list;
        }
        public IActionResult Btn_MyRequest()
        {
            return RedirectToAction("MyRequest", "MyRequest");
        }

    }
}