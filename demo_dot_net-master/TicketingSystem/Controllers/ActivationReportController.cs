using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using TicketingSystem.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Http;
using System.IO;
using ClosedXML.Excel;

namespace TicketingSystem.Controllers
{
    public class ActivationReportController : Controller
    {
        [HttpPost]
        public IActionResult Report()
        {
            try
            {
                var UserInfo = HttpContext.Session.GetObjectFromJson<UserModel>("UserInfo");
                if (UserInfo == null)
                {
                    return RedirectToAction("Error", "Home");
                }
                ViewData["lblWelcome"] = "Welcome, " + HttpContext.Session.GetString("UserID");
                ActivationReportModel activationReportModel = new ActivationReportModel
                {
                    txtRequestDateFrom = "",
                    txtRequestDateTo = ""
                };
                return View(activationReportModel);
            }
            catch (Exception e)
            {
                ViewData["lblMessage"] = "ERROR: " + e.Message;
                return View();
            }
        }
        [HttpPost]
        public IActionResult ReportSearh(ActivationReportModel rptModel)
        {
            try
            {
                var UserInfo = HttpContext.Session.GetObjectFromJson<UserModel>("UserInfo");
                if (UserInfo == null)
                {
                    return RedirectToAction("Error", "Home");
                }
                ViewData["lblWelcome"] = "Welcome, " + HttpContext.Session.GetString("UserID");

                rptModel.list = this.btnShow_Click(UserInfo.txtUserGroup, rptModel.txtRequestDateFrom,rptModel.txtRequestDateTo);
                return View("Report", rptModel);
            }
            catch (Exception e)
            {

                ViewData["lblMessage"] = "ERROR: " + e.Message;
                return View("Report", rptModel);
            }
        }
        public IActionResult btnActivationDashboard_Click()
        {
            return RedirectToAction("Dashboard", "ActivationDashboard");
        }

        public IActionResult Excel(ActivationReportModel rptModel)
        {
            var UserInfo = HttpContext.Session.GetObjectFromJson<UserModel>("UserInfo");
            if (UserInfo == null)
            {
                return RedirectToAction("Error", "Home");
            }
            List<ActivationReqMasterModel> list = this.btnShow_Click(UserInfo.txtUserGroup, rptModel.txtRequestDateFrom, rptModel.txtRequestDateTo);

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("ActivationReport");
                var currentRow = 1;
                worksheet.Cell(currentRow, 1).Value = "TICKET_NO";
                worksheet.Cell(currentRow, 2).Value = "CHANNEL_TYPE";
                worksheet.Cell(currentRow, 3).Value = "CHANNEL_NAME";
                worksheet.Cell(currentRow, 4).Value = "SERVICE_NAME";
                worksheet.Cell(currentRow, 5).Value = "PRODUCT_NAME";
                worksheet.Cell(currentRow, 6).Value = "TICKET_CREATE_DATE";
                worksheet.Cell(currentRow, 7).Value = "TICKET_ASSIGN_DATE";
                worksheet.Cell(currentRow, 8).Value = "TICKET_CLOSE_DATE";
                worksheet.Cell(currentRow, 9).Value = "TOTAL_EXECUTION_TIME";
                worksheet.Cell(currentRow, 10).Value = "TOTAL_WAITING_TIME";
                worksheet.Cell(currentRow, 11).Value = "TICKET_STATUS";
                worksheet.Cell(currentRow, 12).Value = "TICKET_ASSIGN_USER";
                worksheet.Cell(currentRow, 13).Value = "TOTAL_QUANTITY";
                worksheet.Cell(currentRow, 14).Value = "EXEC_QUANTITY";
                worksheet.Cell(currentRow, 15).Value = "EXEC_USER";
                foreach (var rpt in list)
                {
                    currentRow++;
                    worksheet.Cell(currentRow, 1).Value = rpt.ticketNo;
                    worksheet.Cell(currentRow, 2).Value = rpt.channelType;
                    worksheet.Cell(currentRow, 3).Value = rpt.channelName;
                    worksheet.Cell(currentRow, 4).Value = rpt.serviceName;
                    worksheet.Cell(currentRow, 5).Value = rpt.productName;
                    worksheet.Cell(currentRow, 6).Value = rpt.ticketCreateDate;
                    worksheet.Cell(currentRow, 7).Value = rpt.ticketAssignDate;
                    worksheet.Cell(currentRow, 7).Value = rpt.ticketCloseDate;
                    worksheet.Cell(currentRow, 9).Value = rpt.totalExecutionTime;
                    worksheet.Cell(currentRow, 10).Value = rpt.totalWaitingTime;
                    worksheet.Cell(currentRow, 11).Value = rpt.ticketStatus;
                    worksheet.Cell(currentRow, 12).Value = rpt.ticketAssignUser;
                    worksheet.Cell(currentRow, 13).Value = rpt.totalQuantity;
                    worksheet.Cell(currentRow, 14).Value = rpt.execQuantity;
                    worksheet.Cell(currentRow, 15).Value = rpt.execUser;
                }

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();

                    return File(
                        content,
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        "ActivationReport_" + DateTime.Now.ToString("ddMMyyyyHHmmss") + ".xls");
                }
            }
        }


        private List<ActivationReqMasterModel> btnShow_Click(string sUserGroup, string txtRequestDateFrom, string txtRequestDateTo)
        {
            List<ActivationReqMasterModel> list = new List<ActivationReqMasterModel>();
            using (MySqlConnection con = new MySqlConnection(CustUtility.CONN_STR_SQL))
                try
                {
                    string sSQL;
                    string sReqDateFrom, sReqDateTo;
                    sSQL = "SELECT a.TICKET_NO, a.CHANNEL_TYPE, a.CHANNEL_NAME, a.SERVICE_NAME, a.PRODUCT_NAME, ";
                    sSQL += "a.TICKET_CREATE_DATE, a.TICKET_ASSIGN_DATE, a.TICKET_CLOSE_DATE, ";
                    sSQL += "TIME_FORMAT(TIMEDIFF(a.TICKET_CLOSE_DATE, a.TICKET_CREATE_DATE), '%H:%i:%s') AS 'TOTAL_EXECUTION_TIME', ";
                    sSQL += "TIME_FORMAT(TIMEDIFF(a.TICKET_ASSIGN_DATE, a.TICKET_CREATE_DATE), '%H:%i:%s') AS 'TOTAL_WAITING_TIME', ";
                    sSQL += "a.TICKET_STATUS, a.TICKET_ASSIGN_USER, a.TOTAL_QUANTITY, b.EXEC_QUANTITY, b.EXEC_USER ";
                    sSQL += "FROM ";
                    sSQL += "( ";
                    sSQL += "  SELECT * ";
                    sSQL += "  FROM TBL_ACTIVATION_REQ_MASTER ";
                    sSQL += "  WHERE ACTIVATION_TEAM = '" + sUserGroup + "' ";
                    if (!String.IsNullOrEmpty(txtRequestDateFrom) && !String.IsNullOrEmpty(txtRequestDateTo))
                    {
                        sReqDateFrom = Convert.ToDateTime(txtRequestDateFrom).ToString("yyyy-MM-dd");
                        sReqDateTo = Convert.ToDateTime(txtRequestDateTo).AddDays(1).ToString("yyyy-MM-dd");
                        sSQL += "  AND TICKET_CREATE_DATE >= '" + sReqDateFrom + "' AND TICKET_CREATE_DATE < '" + sReqDateTo + "' ";
                    }
                    sSQL += ") a ";
                    sSQL += "LEFT OUTER JOIN ";
                    sSQL += "( ";
                    sSQL += "  SELECT TICKET_NO, EXEC_USER, SUM(EXEC_QUANTITY) AS 'EXEC_QUANTITY' ";
                    sSQL += "  FROM TBL_ACTIVATION_REQ_EXEC ";
                    sSQL += "  GROUP BY TICKET_NO, EXEC_USER ";
                    sSQL += ") b ";
                    sSQL += "ON a.TICKET_NO = b.TICKET_NO ";
                    sSQL += "ORDER BY a.TICKET_CLOSE_DATE DESC ";
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
                                    ticketNo = sdr["TICKET_NO"].ToString(),
                                    channelType = sdr["CHANNEL_TYPE"].ToString(),
                                    channelName = sdr["CHANNEL_NAME"].ToString(),
                                    serviceName = sdr["SERVICE_NAME"].ToString(),
                                    productName = sdr["PRODUCT_NAME"].ToString(),
                                    ticketCreateDate = sdr["TICKET_CREATE_DATE"].ToString(),
                                    ticketAssignDate = sdr["TICKET_ASSIGN_DATE"].ToString(),
                                    ticketCloseDate = sdr["TICKET_CLOSE_DATE"].ToString(),
                                    totalExecutionTime = sdr["TOTAL_EXECUTION_TIME"].ToString(),
                                    totalWaitingTime = sdr["TOTAL_WAITING_TIME"].ToString(),
                                    ticketStatus = sdr["TICKET_STATUS"].ToString(),
                                    ticketAssignUser = sdr["TICKET_ASSIGN_USER"].ToString(),
                                    totalQuantity = sdr["TOTAL_QUANTITY"].ToString(),
                                    execQuantity = sdr["EXEC_QUANTITY"].ToString(),
                                    execUser = sdr["EXEC_USER"].ToString()
                                };
                                list.Add(reqMaster);
                            }
                        }
                        con.Close();
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
    }
}