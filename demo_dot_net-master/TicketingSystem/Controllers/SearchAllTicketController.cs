using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using TicketingSystem.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Cors;

namespace TicketingSystem.Controllers
{
    public class SearchAllTicketController : Controller
    {
        [HttpPost]
        public IActionResult AllTicket()
        {
            try
            {
                var UserInfo = HttpContext.Session.GetObjectFromJson<UserModel>("UserInfo");
                if (UserInfo == null)
                {
                    return RedirectToAction("Error", "Home");
                }
                ViewData["lblWelcome"] = "Welcome, " + HttpContext.Session.GetString("UserID");
                SearchAllTicketModel searchAllTicketModel = new SearchAllTicketModel
                {
                    executor = PopulateExecutorList(UserInfo.txtUserGroup),
                    channel = PopulateChannelList(UserInfo.txtUserGroup),
                    service = PopulateServiceList(UserInfo.txtUserGroup),
                    ticketStatus = CustUtility.PopulateTicketStatusList(),
                    urgent = CustUtility.PopulateUrgentList(),
                    product= new List<SelectListItem>()
                };
                HttpContext.Session.SetObjectAsJson("sAllTicketModel", searchAllTicketModel);
                return View(searchAllTicketModel);
            }
            catch (Exception e)
            {
                ViewData["lblMessage"] = "ERROR: " + e.Message;
                return View();
            }
        }
        [HttpPost]
        public IActionResult AllTicketSearch(SearchAllTicketModel searchAllTicketModel)
        {
            try
            {
                var UserInfo = HttpContext.Session.GetObjectFromJson<UserModel>("UserInfo");
                if (UserInfo == null)
                {
                    return RedirectToAction("Error", "Home");
                }
                ViewData["lblWelcome"] = "Welcome, " + HttpContext.Session.GetString("UserID");
                searchAllTicketModel.listReqMaster = this.btnSearch_Click(UserInfo.txtUserGroup, searchAllTicketModel.txtRequestDateFrom, searchAllTicketModel.txtRequestDateTo, searchAllTicketModel.ddlExecutor, searchAllTicketModel.ddlProduct, searchAllTicketModel.ddlChannelType, searchAllTicketModel.ddlService, searchAllTicketModel.ddlTicketStatus, searchAllTicketModel.ddlUrgent, searchAllTicketModel.txtTicketNo);
                var SessionSearchAllTicket = HttpContext.Session.GetObjectFromJson<SearchAllTicketModel>("sAllTicketModel");
                searchAllTicketModel.service = SessionSearchAllTicket.service;
                searchAllTicketModel.channel = SessionSearchAllTicket.channel;
                searchAllTicketModel.executor = SessionSearchAllTicket.executor;
                searchAllTicketModel.ticketStatus = SessionSearchAllTicket.ticketStatus;
                searchAllTicketModel.urgent = SessionSearchAllTicket.urgent;
                var varProductList = HttpContext.Session.GetObjectFromJson<List<SelectListItem>>("sProductList");
                if(varProductList != null && !String.IsNullOrEmpty(searchAllTicketModel.ddlService))
                {
                    searchAllTicketModel.product = varProductList;
                }
                else
                {
                    searchAllTicketModel.product = new List<SelectListItem>();
                }
                
                return View("AllTicket", searchAllTicketModel);
            }
            catch (Exception e)
            {
                
                ViewData["lblMessage"] = "ERROR: " + e.Message;
                return View("AllTicket", searchAllTicketModel);
            }
        }
        public IActionResult btnActivationDashboard_Click()
        {
            return RedirectToAction("Dashboard", "ActivationDashboard");
        }
        [HttpGet]
        [EnableCors("AllowOrigin")]
        public JsonResult GetProductList(String ServiceName)
        {
            List<SelectListItem> productList =PopulateProductList(ServiceName);
            HttpContext.Session.SetObjectAsJson("sProductList", productList);
           /* var SessionSearchAllTicket = HttpContext.Session.GetObjectFromJson<SearchAllTicketModel>("AllTicketModel");
            SessionSearchAllTicket.product = productList;
            HttpContext.Session.SetObjectAsJson("AllTicketModel", SessionSearchAllTicket);*/
            return Json(productList);
        }

        private List<SelectListItem> PopulateServiceList(string sUserGroup)
        {
            List<SelectListItem> items = new List<SelectListItem>();
            using (MySqlConnection con = new MySqlConnection(CustUtility.CONN_STR_SQL))
            {
                string sSQL = "SELECT ID,SERVICE_NAME FROM TBL_LIST_SERVICE WHERE ACTIVATION_TEAM = '" + sUserGroup + "'";
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
                                Text = sdr["SERVICE_NAME"].ToString(),
                                Value = sdr["SERVICE_NAME"].ToString()
                            });
                        }
                    }
                    con.Close();
                }
            }
            return items;
        }
        private List<SelectListItem> PopulateChannelList(string sUserGroup)
        {
            List<SelectListItem> items = new List<SelectListItem>();
            using (MySqlConnection con = new MySqlConnection(CustUtility.CONN_STR_SQL))
            {
                string sSQL = "SELECT ID,CHANNEL_TYPE FROM TBL_LIST_CHANNEL_TYPE WHERE ACTIVATION_TEAM = '" + sUserGroup + "'";
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
        private List<SelectListItem> PopulateExecutorList(string sUserGroup)
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
        private List<SelectListItem> PopulateProductList(string ServiceName)
        {
            List<SelectListItem> items = new List<SelectListItem>();
            using (MySqlConnection con = new MySqlConnection(CustUtility.CONN_STR_SQL))
            {
                string sSQL = "SELECT PRODUCT_NAME FROM TBL_LIST_PRODUCT ";
                sSQL += "WHERE SERVICE_NAME = '" + ServiceName + "'";
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
                                Text = sdr["PRODUCT_NAME"].ToString(),
                                Value = sdr["PRODUCT_NAME"].ToString()
                            });
                        }
                    }
                    con.Close();
                }
            }
            return items;
        }        
        private List<ActivationReqMasterModel> btnSearch_Click(string sUserGroup, string txtExecutionDateFrom, string txtExecutionDateTo, string selectedExecutor, string selectedProduct, string selectedChannel, string selectedService, string selectedStatus, string selectedUrgent, string txtTicketNo)
        {
            List<ActivationReqMasterModel> list = new List<ActivationReqMasterModel>();
            using (MySqlConnection con = new MySqlConnection(CustUtility.CONN_STR_SQL))
                try
                {
                    string sSQL;
                    string sReqDateFrom, sReqDateTo;
                    string sExecDateTo = Convert.ToDateTime(txtExecutionDateTo).AddDays(1).ToString("yyyy-MM-dd");
                    sSQL = "SELECT TICKET_CREATE_DATE 'REQUEST_DATE', TICKET_NO 'TICKET_NO', ";
                    sSQL += "SERVICE_NAME 'SERVICE_NAME', PRODUCT_NAME 'PRODUCT_NAME', IS_URGENT 'URGENT', ";
                    sSQL += "TICKET_CREATE_USER 'REQUESTER', TICKET_STATUS 'TICKET_STATUS', TICKET_ASSIGN_USER 'ASSIGNED_USER' ";
                    sSQL += "FROM TBL_ACTIVATION_REQ_MASTER ";
                    sSQL += "WHERE ACTIVATION_TEAM = '" + sUserGroup + "' ";
                    if (( !String.IsNullOrEmpty(txtExecutionDateFrom)) && (String.IsNullOrEmpty(txtExecutionDateTo)))
                    {
                        sReqDateFrom = Convert.ToDateTime(txtExecutionDateFrom.Trim()).ToString("yyyy-MM-dd");
                        sReqDateTo = Convert.ToDateTime(txtExecutionDateTo.Trim()).AddDays(1).ToString("yyyy-MM-dd");
                        sSQL += "AND TICKET_CREATE_DATE >= '" + sReqDateFrom + "' AND TICKET_CREATE_DATE < '" + sReqDateTo + "' ";
                    }
                    if (!String.IsNullOrEmpty(selectedService))
                    {
                        sSQL += "AND SERVICE_NAME = '" + selectedService + "' ";
                    }
                    if (!String.IsNullOrEmpty(selectedProduct))
                    {
                        sSQL += "AND PRODUCT_NAME = '" + selectedProduct + "' ";
                    }
                    if (!String.IsNullOrEmpty(selectedExecutor))
                    {
                        sSQL += "AND TICKET_ASSIGN_USER = '" + selectedExecutor + "' ";
                    }
                    if (!String.IsNullOrEmpty(selectedStatus))
                    {
                        sSQL += "AND TICKET_STATUS = '" + selectedStatus + "' ";
                    }
                    if (!String.IsNullOrEmpty(selectedChannel))
                    {
                        sSQL += "AND CHANNEL_TYPE = '" + selectedChannel + "' ";
                    }
                    if (!String.IsNullOrEmpty(selectedUrgent))
                    {
                        sSQL += "AND IS_URGENT = '" + selectedUrgent + "' ";
                    }
                    if (!String.IsNullOrEmpty(txtTicketNo))
                    {
                        sSQL += "AND TICKET_NO = '" + txtTicketNo + "' ";
                    }
                    sSQL += "ORDER BY TICKET_CREATE_DATE DESC";
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
                                    ticketStatus = sdr["TICKET_STATUS"].ToString(),
                                    ticketCreateUser = sdr["REQUESTER"].ToString(),
                                    ticketAssignUser = sdr["ASSIGNED_USER"].ToString()
                                };
                                list.Add(reqMaster);
                            }
                        }
                        con.Close();
                    }

                }
                catch (Exception )
                {
                    throw;
                }
                finally
                {
                    con.Close();
                }
            return list;
        }
    }
}
