using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Mail;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MySql.Data.MySqlClient;
using TicketingSystem.Models;

namespace TicketingSystem.Controllers
{
    public class ActivationRequestController : Controller
    {
        public IActionResult ActRequest()
        {
            try
            {
                var UserInfo = HttpContext.Session.GetObjectFromJson<UserModel>("UserInfo");
                if (UserInfo == null)
                {
                    return RedirectToAction("Error", "Home");
                }
                ViewData["lblWelcome"] = "Welcome, " + HttpContext.Session.GetString("UserID");
                Random random = new Random();
                int nRandom = random.Next(0, 99);
                string TICKET = DateTime.Now.ToString("yyyyMMddHHmmss") + nRandom.ToString().PadLeft(2, '0');
                ActivationRequestViewModel model = new ActivationRequestViewModel
                {                    
                    ticketId = TICKET,
                    ddlActivationTeam = PopulateActivationTeam(),
                    ddlService = new List<SelectListItem>(),
                    ddlChannelType = new List<SelectListItem>(),
                    ddlProduct = new List<SelectListItem>(),
                    ddlChannelName = new List<SelectListItem>(),
                    msisdnSims = new List<MsisdnSimMasterModel>
                    {
                        new MsisdnSimMasterModel { msisdn = "", simNo = "" },
                        new MsisdnSimMasterModel { msisdn = "", simNo = "" },
                        new MsisdnSimMasterModel { msisdn = "", simNo = "" },
                        new MsisdnSimMasterModel { msisdn = "", simNo = "" },
                        new MsisdnSimMasterModel { msisdn = "", simNo = "" },
                        new MsisdnSimMasterModel { msisdn = "", simNo = "" },
                        new MsisdnSimMasterModel { msisdn = "", simNo = "" },
                        new MsisdnSimMasterModel { msisdn = "", simNo = "" },
                        new MsisdnSimMasterModel { msisdn = "", simNo = "" },
                        new MsisdnSimMasterModel { msisdn = "", simNo = "" }
                    },
                    attachmentTable = GetDirectories(CustUtility.FILE_UPLOAD_LOCATION + TICKET)
                };
                HttpContext.Session.SetObjectAsJson("ACT_REQ", model);
                ViewData["btnSave_Enabled"] = "enabled";
                return View(model);
            }
            catch (Exception e)
            {
                ViewData["lblMessage"] = "ERROR: " + e.Message;
                return View();
            }
        }

        public ActionResult FileUpload()
        {
            IFormFile file = Request.Form.Files[0];
            var SessionActReq = HttpContext.Session.GetObjectFromJson<ActivationRequestViewModel>("ACT_REQ");
            string TicketID = SessionActReq.ticketId;
            string fileUploadPath = CustUtility.FILE_UPLOAD_LOCATION + TicketID;
            if (!Directory.Exists(fileUploadPath))
            {
                Directory.CreateDirectory(fileUploadPath);
            }
            string fullPath = Path.Combine(fileUploadPath, file.FileName);
            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                file.CopyToAsync(stream);
            }
            return this.Content(GetDirectories(fileUploadPath));
        }

        public ActionResult RemoveFile(IEnumerable<string> checkedFiles)
        {
            var SessionActReq = HttpContext.Session.GetObjectFromJson<ActivationRequestViewModel>("ACT_REQ");
            string TicketID = SessionActReq.ticketId;
            foreach (string fileNm in checkedFiles)
            {
                //string fullPath = CustUtility.FILE_UPLOAD_LOCATION + TicketID + "\\" + fileNm;
                string fullPath = CustUtility.FILE_UPLOAD_LOCATION + TicketID + "/" + fileNm;
                if (System.IO.File.Exists(fullPath))
                {
                    System.IO.File.Delete(fullPath);
                }

            }

            return this.Content(GetDirectories(CustUtility.FILE_UPLOAD_LOCATION + TicketID));
        }

        protected string GetDirectories(string sFilePath)
        {
            // List<FileData> list = new List<FileData>();
            string table = "";
            try
            {
                DirectoryInfo dir = new DirectoryInfo(sFilePath);
                if (dir.Exists == false)
                {
                    return table;
                }
                FileInfo[] files = dir.GetFiles();
                if (files.Length > 0)
                {
                    int i = 0;
                    table = "<table cellspacing = \"0\" rules = \"all\" border = \"1\" id = \"grvDirList\" style = \"border-collapse:collapse;\">";
                    table += "<tr style = \"background-color:Gray;font-weight:bold;height:15px;\">";
                    table += "<th> Select </th><th> Sl No.</th><th> File Name </th><th> File Size </th></tr>";

                    foreach (FileInfo file in files)
                    {
                        table += "<tr><td><input id =\"chkSelectFile\" type =\"checkbox\" name =\"chkSelectFile\" value=\"" + file.Name + "\"/></ td >";
                        table += "<td> " + i + " </td>";
                        table += "<td><a id = \"_hplFileName\" href = \"/ActivationRequest/Download?fileName=" + file.Name + "\"> " + file.Name + " </a></td>";
                        table += "<td> " + GetSizeInMB(file.Length) + " MB" + " </td></tr>";
                        i++;
                        /*drow = dt.NewRow();
                        drow["FILENAME"] = file.Name;
                        drow["FILEPATH"] = file.FullName;
                        drow["FILESIZE"] = GetSizeInMB(file.Length) + " MB";
                        dt.Rows.Add(drow);*/

                    }
                    table += "</table>";
                }

            }
            catch (Exception ex)
            {
                /* this.lblMessage.Text = DateTime.Now.ToString() + " ## GetDirectories ## " + ex.Message;*/
                throw ex;
            }
            return table;
        }

        private string GetSizeInMB(long sizeInBytes)
        {
            decimal decisize = 0;
            string strsize = "";

            decisize = Convert.ToDecimal(sizeInBytes);
            decisize = decisize / 1048576;
            strsize = decisize.ToString("0.00");

            return strsize;
        }

        public ActionResult Download(string fileName)
        {
            var SessionActReq = HttpContext.Session.GetObjectFromJson<ActivationRequestViewModel>("ACT_REQ");
            string TicketID = SessionActReq.ticketId;
            //string Files = CustUtility.FILE_UPLOAD_LOCATION + TicketID + "\\" + fileName;
            string Files = CustUtility.FILE_UPLOAD_LOCATION + TicketID + "/" + fileName;
            byte[] fileBytes = System.IO.File.ReadAllBytes(Files);
            System.IO.File.WriteAllBytes(Files, fileBytes);
            MemoryStream ms = new MemoryStream(fileBytes);
            return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
        }

        public JsonResult GetServiceForTeamChange(string selectedTeam)
        {
            List<SelectListItem> serviceList = PopulateServiceList(selectedTeam);
            return Json(serviceList);
        }

        public JsonResult GetChannelForTeamChange(string selectedTeam)
        {
            List<SelectListItem> channelList = PopulateChannelTypeList(selectedTeam);
            return Json(channelList);
        }

        public JsonResult GetProductForServiceChange(string selectedService)
        {
            List<SelectListItem> productList = PopulateProductList(selectedService);
            return Json(productList);
        }

        public JsonResult GetChannelNameForType(string selectedChannel)
        {
            List<SelectListItem> channelNameList = PopulateChannelNames(selectedChannel);
            return Json(channelNameList);
        }

        private List<SelectListItem> PopulateActivationTeam()
        {
            return new List<SelectListItem>
            {
                new SelectListItem {Text = "CONSUMER", Value = "CONSUMER"},
                new SelectListItem {Text = "BS", Value = "BS"}
            };
        }

        private List<SelectListItem> PopulateServiceList(string activationTeam)
        {
            List<SelectListItem> listItems = new List<SelectListItem>();
            using (MySqlConnection connection = new MySqlConnection(CustUtility.CONN_STR_SQL))
            {
                string sSQL;
                sSQL = "SELECT SERVICE_NAME FROM TBL_LIST_SERVICE ";
                sSQL += "WHERE ACTIVATION_TEAM = '" + activationTeam + "' ";
                sSQL += "ORDER BY SERVICE_NAME ASC";
                using (MySqlCommand command = new MySqlCommand(sSQL))
                {
                    command.Connection = connection;
                    connection.Open();
                    MySqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        SelectListItem item = new SelectListItem
                        {
                            Text = reader["SERVICE_NAME"].ToString(),
                            Value = reader["SERVICE_NAME"].ToString()
                        };
                        listItems.Add(item);
                    }
                    connection.Close();
                }
            }
            return listItems;
        }

        private List<SelectListItem> PopulateChannelTypeList(string activationTeam)
        {
            List<SelectListItem> listItems = new List<SelectListItem>();
            using (MySqlConnection connection = new MySqlConnection(CustUtility.CONN_STR_SQL))
            {
                string sSQL;
                sSQL = "SELECT CHANNEL_TYPE FROM TBL_LIST_CHANNEL_TYPE ";
                sSQL += "WHERE ACTIVATION_TEAM = '" + activationTeam + "' ";
                sSQL += "ORDER BY CHANNEL_TYPE ASC";
                using (MySqlCommand command = new MySqlCommand(sSQL))
                {
                    command.Connection = connection;
                    connection.Open();
                    MySqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        SelectListItem item = new SelectListItem
                        {
                            Text = reader["CHANNEL_TYPE"].ToString(),
                            Value = reader["CHANNEL_TYPE"].ToString()
                        };
                        listItems.Add(item);
                    }
                }
                connection.Close();
            }
            return listItems;
        }

        private List<SelectListItem> PopulateProductList(string selectedService)
        {
            List<SelectListItem> listItems = new List<SelectListItem>();
            using (MySqlConnection connection = new MySqlConnection(CustUtility.CONN_STR_SQL))
            {
                string sSQL;
                sSQL = "SELECT PRODUCT_NAME, ID FROM TBL_LIST_PRODUCT ";
                sSQL += "WHERE SERVICE_NAME = '" + selectedService + "' ";
                sSQL += "ORDER BY PRODUCT_NAME ASC";
                using (MySqlCommand command = new MySqlCommand(sSQL))
                {
                    command.Connection = connection;
                    connection.Open();
                    MySqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        SelectListItem item = new SelectListItem
                        {
                            Text = reader["PRODUCT_NAME"].ToString(),
                            Value = reader["ID"].ToString()
                        };
                        listItems.Add(item);
                    }
                }
                connection.Close();
            }
            return listItems;
        }

        private List<SelectListItem> PopulateChannelNames(string selectedChannelType)
        {
            List<SelectListItem> listItems = new List<SelectListItem>();
            using (MySqlConnection connection = new MySqlConnection(CustUtility.CONN_STR_SQL))
            {
                string sSQL;
                sSQL = "SELECT CHANNEL_NAME, ID FROM TBL_LIST_CHANNEL ";
                sSQL += "WHERE CHANNEL_TYPE = '" + selectedChannelType + "' ";
                sSQL += "ORDER BY CHANNEL_NAME ASC";
                using (MySqlCommand command = new MySqlCommand(sSQL))
                {
                    command.Connection = connection;
                    connection.Open();
                    MySqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        SelectListItem item = new SelectListItem
                        {
                            Text = reader["CHANNEL_NAME"].ToString(),
                            Value = reader["ID"].ToString()
                        };
                        listItems.Add(item);
                    }
                }
                connection.Close();
            }
            return listItems;
        }

        [HttpPost]
        public IActionResult btnReturn_Click()
        {
            return RedirectToAction("Dashboard", "ActivationDashboard");
        }

        [HttpPost]
        public IActionResult btnSave_Click(ActivationRequestViewModel model, string txtTotalQuantity, string txtRemarks, bool chkUrgent,
            string[] txtMSISDN, string[] txtSIM)
        {
            var UserInfo = HttpContext.Session.GetObjectFromJson<UserModel>("UserInfo");
            if (UserInfo == null)
            {
                return RedirectToAction("Error", "Home");
            }
            ViewData["lblWelcome"] = "Welcome, " + HttpContext.Session.GetString("UserID");
            var SessionActReq = HttpContext.Session.GetObjectFromJson<ActivationRequestViewModel>("ACT_REQ");
            string TicketID = SessionActReq.ticketId;
            if (string.IsNullOrEmpty(model.selectedActivationTeam))
            {
                ModelState.AddModelError("Error", "Please select Ticket Status Other than OPEN.");
                model = ReturnDefaultModel();
                return View("ActRequest", model);
            }
            if (string.IsNullOrEmpty(model.selectedService))
            {
                ModelState.AddModelError("Error", "You must select Service Name.");
                model = ReturnDefaultModel();
                return View("ActRequest", model);
            }
            if (string.IsNullOrEmpty(model.selectedProduct))
            {
                ModelState.AddModelError("Error", "You must select Product Name.");
                model = ReturnDefaultModel();
                return View("ActRequest", model);
            }
            if (string.IsNullOrEmpty(model.selectedChannel))
            {
                ModelState.AddModelError("Error", "You must select Channel Type.");
                model = ReturnDefaultModel();
                return View("ActRequest", model);
            }
            if (string.IsNullOrEmpty(model.selectedChannelName))
            {
                ModelState.AddModelError("Error", "You must select Channel Name.");
                model = ReturnDefaultModel();
                return View("ActRequest", model);
            }
            if (string.IsNullOrEmpty(txtTotalQuantity) || txtTotalQuantity.Trim().Equals("0"))
            {
                ModelState.AddModelError("Error", "You must specify Total Quantity.");
                model = ReturnDefaultModel();
                return View("ActRequest", model);
            }
            try
            {
                using (MySqlConnection con = new MySqlConnection(CustUtility.CONN_STR_SQL))
                {
                    string sSQL;
                    string sTicketNo = TicketID;
                    string sActivationTeam = model.selectedActivationTeam;
                    string sServiceName = model.selectedService;
                    string sProductName = model.selectedProduct;
                    string sChannelType = model.selectedChannel;
                    string sChannelName = model.selectedChannelName;
                    string sTotalQuantity = txtTotalQuantity;
                    string sIsUrgent = (chkUrgent) ? "YES" : "NO";
                    string sRemarks = txtRemarks;
                    string sTicketCreateDate = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
                    string sTicketCreateUser = HttpContext.Session.GetString("UserID");
                    string sTicketStatus = "OPEN";
                    string sExecQuantity = "0";
                    string sRemQuantity = txtTotalQuantity;
                    int nResult = 0;
                    con.Open();

                    using (MySqlTransaction transaction = con.BeginTransaction())
                    {
                        sSQL = "INSERT INTO TBL_ACTIVATION_REQ_MASTER(TICKET_NO, ACTIVATION_TEAM, SERVICE_NAME, PRODUCT_NAME, ";
                        sSQL += "CHANNEL_TYPE, CHANNEL_NAME, TOTAL_QUANTITY, REMARKS, TICKET_CREATE_DATE, TICKET_CREATE_USER, ";
                        sSQL += "IS_URGENT, TICKET_STATUS, EXEC_QUANTITY, REM_QUANTITY) ";
                        sSQL += "VALUES('" + sTicketNo + "', '" + sActivationTeam + "', '" + sServiceName + "', '" + sProductName + "', '";
                        sSQL += sChannelType + "', '" + sChannelName + "', '" + sTotalQuantity + "', '" + sRemarks + "', '" + sTicketCreateDate + "', '" + sTicketCreateUser + "', '";
                        sSQL += sIsUrgent + "', '" + sTicketStatus + "', '" + sExecQuantity + "', '" + sRemQuantity + "')";

                        using (MySqlCommand command = new MySqlCommand(sSQL))
                        {
                            command.Connection = con;
                            command.Transaction = transaction;
                            nResult = command.ExecuteNonQuery();
                        }

                        sSQL = "INSERT INTO TBL_ACTIVATION_REQ_HISTORY(TICKET_NO, CURRENT_STATUS, CURRENT_STATUS_DATE, CURRENT_STATUS_USER) ";
                        sSQL += "VALUES('" + sTicketNo + "', '" + sTicketStatus + "', '" + sTicketCreateDate + "', '" + sTicketCreateUser + "')";

                        using (MySqlCommand command = new MySqlCommand(sSQL))
                        {
                            command.Connection = con;
                            command.Transaction = transaction;
                            nResult = command.ExecuteNonQuery();
                        }

                        for (int i = 0; i < 10; i++)
                        {
                            string sMobileNo = txtMSISDN[i];
                            string sSimNo = txtSIM[i];
                            string sSeqNo = Convert.ToString(i + 1);
                            if (!string.IsNullOrEmpty(sMobileNo) && !string.IsNullOrEmpty(sSimNo))
                            {
                                sSQL = "INSERT INTO TBL_ACTIVATION_REQ_DETAIL(TICKET_NO, SEQ_NO, MOBILE_NO, SIM_NO) ";
                                sSQL += "VALUES('" + sTicketNo + "', " + sSeqNo + ", '" + sMobileNo + "', '" + sSimNo + "')";
                                using (MySqlCommand command = new MySqlCommand(sSQL))
                                {
                                    command.Connection = con;
                                    command.Transaction = transaction;
                                    nResult = command.ExecuteNonQuery();
                                }
                            }
                        }
                        transaction.Commit();
                    }
                    con.Close();
                }
            }
            catch (Exception e)
            {
                ViewData["lblMessage"] = "ERROR: " + e.Message;
            }
            ViewData["lblMessage"] = "MESSAGE: " + "Your activation request[Ticket# " + TicketID + "] has been submitted successfully.";
            ViewData["btnSave_Enabled"] = "disabled";
            var SessionActReq1 = HttpContext.Session.GetObjectFromJson<ActivationRequestViewModel>("ACT_REQ");
            model.ddlActivationTeam = SessionActReq1.ddlActivationTeam;
            model.ddlService = SessionActReq1.ddlService;
            model.ddlChannelType = SessionActReq1.ddlChannelType;
            model.ddlProduct = SessionActReq1.ddlProduct;
            model.ddlChannelName = SessionActReq1.ddlChannelName;
            model.msisdnSims = SessionActReq1.msisdnSims;
            model.attachmentTable = GetDirectories(CustUtility.FILE_UPLOAD_LOCATION + TicketID);
            return View("ActRequest", model);
        }

        private ActivationRequestViewModel ReturnDefaultModel()
        {
            var SessionVal = HttpContext.Session.GetObjectFromJson<ActivationRequestViewModel>("ACT_REQ");
            Console.WriteLine("Ticket ID: " + SessionVal.ticketId + "ATTACHMENT RAW STRING:" + GetDirectories(CustUtility.FILE_UPLOAD_LOCATION + SessionVal.ticketId));
            ActivationRequestViewModel viewModel = new ActivationRequestViewModel
            {
                ticketId = SessionVal.ticketId,
                ddlActivationTeam = SessionVal.ddlActivationTeam,
                ddlService = SessionVal.ddlService,
                ddlChannelType = SessionVal.ddlChannelType,
                ddlProduct = SessionVal.ddlProduct,
                ddlChannelName = SessionVal.ddlChannelName,
                msisdnSims = SessionVal.msisdnSims,
                selectedActivationTeam = SessionVal.selectedActivationTeam,
                selectedChannel = SessionVal.selectedChannel,
                selectedProduct = SessionVal.selectedProduct,
                selectedService = SessionVal.selectedService,
                selectedChannelName = SessionVal.selectedChannelName,
                attachmentTable = GetDirectories(CustUtility.FILE_UPLOAD_LOCATION + SessionVal.ticketId)
            };
            return viewModel;
        }

        public ActionResult btnAttachment(IEnumerable<string> checkedFiles)
        {
            string fileNames="";
            foreach (string fileNm in checkedFiles)
            {
                if (!String.IsNullOrEmpty(fileNm))
                {
                    if (String.IsNullOrEmpty(fileNames))
                    {
                        fileNames = fileNm;
                    }
                    else
                    {
                        fileNames = fileNames + "|" + fileNm;
                    }
                }
            }
            string divAttachment="<span id = \"lblMailAttachment\" style = \"width:350px\">"+ fileNames + "</ span><input type =\"hidden\" name=\"txtMailAttachment\" value = \"" +fileNames+ "\" id =\"mailAttachment\"/>";
            return this.Content(divAttachment);
        }

        public IActionResult btnSendMail(ActivationRequestViewModel model)
        {
            ActivationRequestViewModel SessionActReq = HttpContext.Session.GetObjectFromJson<ActivationRequestViewModel>("ACT_REQ");
            string TicketID = SessionActReq.ticketId;

            string sAttachmentFolder, sAttachmentFileList;
            SmtpClient sc = new SmtpClient();
            MailMessage mm = new MailMessage();

            try
            {
                if (String.IsNullOrEmpty(model.txtMailTo))
                {
                   // string lblMessage= "No recipients were specified for this mail.";
                }

                mm = new MailMessage();
                mm.From = new MailAddress(CustUtility.MAIL_FROM_ADDRESS);
                mm.Subject = model.txtMailSubject;
                mm.Body = "Ticket Ref: " + TicketID + "\n\n" + model.txtMailBody;
                mm.IsBodyHtml = false;

                if (!String.IsNullOrEmpty(model.txtMailTo))
                {
                    mm.To.Add(model.txtMailTo);
                }
                if (!String.IsNullOrEmpty(model.txtMailCC))
                {
                    mm.CC.Add(model.txtMailCC.Trim());
                }
                if (!String.IsNullOrEmpty(model.txtMailAttachment))
                {
                    //sAttachmentFolder = CustUtility.FILE_UPLOAD_LOCATION + TicketID + "\\";
                    sAttachmentFolder = CustUtility.FILE_UPLOAD_LOCATION + TicketID + "/";
                    DirectoryInfo dir = new DirectoryInfo(sAttachmentFolder);
                    if (dir.Exists == true)
                    {
                        /*
                        FileInfo[] files = dir.GetFiles();
                        foreach (FileInfo file in files)
                        {
                            Attachment at = new Attachment(file.FullName);
                            mm.Attachments.Add(at);
                        }
                        */

                        sAttachmentFileList = model.txtMailAttachment.Trim();
                        foreach (string sFileName in sAttachmentFileList.Split('|'))
                        {
                            Attachment at = new Attachment(sAttachmentFolder + sFileName.Trim());
                            mm.Attachments.Add(at);
                        }
                    }
                }

                sc.Host = CustUtility.MAIL_SMTP_HOST;
                sc.Port = 25;
                sc.Send(mm);
            }
            catch (Exception ex)
            {
               // string lblMessage = ex.Message;
                //ViewData["lblMessage"] = "ERROR: " + ex.Message;
                throw ex;
            }
            ViewData["btnSave_Enabled"] = "enabled";
            return View("ActRequest", SessionActReq);
        }
    }
}