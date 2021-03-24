using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using TicketingSystem.Models;

namespace TicketingSystem.Controllers
{
    public class MyRequestController : Controller
    {
        [HttpPost]
        public IActionResult MyRequest()
        {
            try
            {
                var UserInfo = HttpContext.Session.GetObjectFromJson<UserModel>("UserInfo");
                if (UserInfo == null)
                {
                    return RedirectToAction("Error", "Home");
                }
                string UserID = HttpContext.Session.GetString("UserID");
                ViewData["lblWelcome"] = "Welcome, " + UserID;
                MyRequestViewModel model = new MyRequestViewModel
                {
                    linkCaptions = PopulateTicketStatusHyperLinks(UserID),
                    listDetails = PopulateActivationRequestList("ALL", UserID)
                };
                HttpContext.Session.SetObjectAsJson("MY_REQUEST_MODEL", model);
                return View(model);
            }
            catch (Exception e)
            {
                ViewData["lblMessage"] = "ERROR: " + e.Message;
                return View();
            }

        }

        [HttpGet]
        public IActionResult MyRequest(MyRequestViewModel model, string status)
        {
            try
            {
                var UserInfo = HttpContext.Session.GetObjectFromJson<UserModel>("UserInfo");
                if (UserInfo == null)
                {
                    return RedirectToAction("Error", "Home");
                }
                string UserID = HttpContext.Session.GetString("UserID");
                ViewData["lblWelcome"] = "Welcome, " + UserID;
                string sStatus = string.IsNullOrEmpty(status) ? "ALL" : status;
                model = new MyRequestViewModel
                {
                    linkCaptions = PopulateTicketStatusHyperLinks(UserID),
                    listDetails = PopulateActivationRequestList(sStatus, UserID)
                };
                HttpContext.Session.SetObjectAsJson("MY_REQUEST_MODEL", model);
                return View(model);
            }
            catch (Exception e)
            {
                ViewData["lblMessage"] = "ERROR: " + e.Message;
                return View();
            }

        }

        private RequestLinkMasterModel PopulateTicketStatusHyperLinks(string sUserID)
        {
            RequestLinkMasterModel masterModel = new RequestLinkMasterModel();
            string sSQL = "SELECT IFNULL(COUNT(*), 0) AS TOTAL_COUNT, ";
            sSQL += "IFNULL(COUNT(CASE TICKET_STATUS WHEN 'OPEN' THEN 1 END), 0) AS OPEN_COUNT, ";
            sSQL += "IFNULL(COUNT(CASE TICKET_STATUS WHEN 'CLOSE' THEN 1 END), 0) AS CLOSE_COUNT, ";
            sSQL += "IFNULL(COUNT(CASE TICKET_STATUS WHEN 'ASSIGNED' THEN 1 END), 0) AS ASSIGNED_COUNT, ";
            sSQL += "IFNULL(COUNT(CASE TICKET_STATUS WHEN 'HOLD' THEN 1 END), 0) AS HOLD_COUNT, ";
            sSQL += "IFNULL(COUNT(CASE TICKET_STATUS WHEN 'REJECTED' THEN 1 END), 0) AS REJECTED_COUNT ";
            sSQL += "FROM TBL_ACTIVATION_REQ_MASTER WHERE TICKET_CREATE_USER = '" + sUserID + "';";

            MySqlConnection connection = new MySqlConnection(CustUtility.CONN_STR_SQL);
            try
            {
                MySqlCommand command = new MySqlCommand(sSQL);
                command.Connection = connection;
                connection.Open();
                MySqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    masterModel.reqTotalLinkCaption = "Your Total Request count is " + reader["TOTAL_COUNT"].ToString();
                    masterModel.reqOpenLinkCaption = "You have " + reader["OPEN_COUNT"].ToString() + " Open Requests";
                    masterModel.reqCloseLinkCaption = "You have " + reader["CLOSE_COUNT"].ToString() + " Close Requests";
                    masterModel.reqAssignLinkCaption = "You have " + reader["ASSIGNED_COUNT"].ToString() + " Assigned Requests";
                    masterModel.reqHoldLinkCaption = "You have " + reader["HOLD_COUNT"].ToString() + " Hold Requests";
                    masterModel.reqRejectLinkCaption = "You have " + reader["REJECTED_COUNT"].ToString() + " Rejected Requests";
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
            return masterModel;
        }

        private List<RequestDetailsMaster> PopulateActivationRequestList(string sTicketStatus, string sUserID)
        {
            List<RequestDetailsMaster> requests = new List<RequestDetailsMaster>();
            string sSQL;
            sSQL = "SELECT TICKET_CREATE_DATE 'REQUEST_DATE', TICKET_NO 'TICKET_NO', ";
            sSQL += "SERVICE_NAME 'SERVICE_NAME', TICKET_STATUS 'TICKET_STATUS', ";
            sSQL += "TICKET_ASSIGN_USER 'TICKET_ASSIGN_USER', ACTIVATION_TEAM 'ACTIVATION_TEAM', ";
            sSQL += "COMMENTS 'COMMENTS' ";
            sSQL += "FROM TBL_ACTIVATION_REQ_MASTER ";
            sSQL += "WHERE TICKET_CREATE_USER = '" + sUserID + "' ";
            if (sTicketStatus != "ALL")
            {
                sSQL += "AND TICKET_STATUS = '" + sTicketStatus + "' ";
            }
            sSQL += "ORDER BY TICKET_CREATE_DATE DESC";
            MySqlConnection connection = new MySqlConnection(CustUtility.CONN_STR_SQL);
            try
            {
                MySqlCommand command = new MySqlCommand(sSQL);
                command.Connection = connection;
                connection.Open();
                MySqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    RequestDetailsMaster details = new RequestDetailsMaster
                    {
                        requestDate = reader["REQUEST_DATE"].ToString(),
                        ticketId = reader["TICKET_NO"].ToString(),
                        serviceName = reader["SERVICE_NAME"].ToString(),
                        status = reader["TICKET_STATUS"].ToString(),
                        ticketAssignedUser = reader["TICKET_ASSIGN_USER"].ToString(),
                        responsibleTeam = reader["ACTIVATION_TEAM"].ToString(),
                        comments = reader["COMMENTS"].ToString()
                    };
                    requests.Add(details);
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
            return requests;
        }

        public IActionResult Btn_ActRequest()
        {
            return RedirectToAction("ActRequest", "ActivationRequest");
        }

        public IActionResult Btn_MyTicket()
        {
            return RedirectToAction("MyTicket", "SearchMyTicket");
        }

        public IActionResult Btn_ActivationDashboard()
        {
            return RedirectToAction("Dashboard", "ActivationDashboard");
        }
    }
}