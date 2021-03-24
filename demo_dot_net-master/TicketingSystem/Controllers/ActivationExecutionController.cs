using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Mail;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MySql.Data.MySqlClient;
using TicketingSystem.Models;

namespace TicketingSystem.Controllers
{
    public class ActivationExecutionController : Controller
    {
        public ActionResult FileUpload()
        {
            IFormFile file = Request.Form.Files[0];
            var SessionActReq = HttpContext.Session.GetObjectFromJson<ActivationExecutionViewModel>("EXECUTION");
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
            var SessionActReq = HttpContext.Session.GetObjectFromJson<ActivationExecutionViewModel>("EXECUTION");
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
                        table += "<td><a id = \"_hplFileName\" href = \"/ActivationExecution/Download?fileName=" + file.Name + "\"> " + file.Name + " </a></td>";
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
            var SessionActReq = HttpContext.Session.GetObjectFromJson<ActivationExecutionViewModel>("EXECUTION");
            string TicketID = SessionActReq.ticketId;
            //string Files = CustUtility.FILE_UPLOAD_LOCATION + TicketID + "\\" + fileName;
            string Files = CustUtility.FILE_UPLOAD_LOCATION + TicketID + "/" + fileName;
            byte[] fileBytes = System.IO.File.ReadAllBytes(Files);
            System.IO.File.WriteAllBytes(Files, fileBytes);
            MemoryStream ms = new MemoryStream(fileBytes);
            return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
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
        public IActionResult btnSendMail(ActivationExecutionViewModel model)
        {
            ActivationExecutionViewModel SessionActReq = HttpContext.Session.GetObjectFromJson<ActivationExecutionViewModel>("EXECUTION");
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
            return View("Execution", SessionActReq);
        }
        [HttpGet]
        public IActionResult Execution(string reqticketno)
        {
            try
            {
                var UserInfo = HttpContext.Session.GetObjectFromJson<UserModel>("UserInfo");
                if (UserInfo == null)
                {
                    return RedirectToAction("Error", "Home");
                }
                ViewData["lblWelcome"] = "Welcome, " + HttpContext.Session.GetString("UserID");
                ActivationExecutionViewModel model = new ActivationExecutionViewModel
                {
                    ticketId = reqticketno,
                    executionMaster = FillFormFields(reqticketno),
                    msisdnList = FillFormMsisdn(reqticketno),
                    execType = PopulateExecutorType(),
                    ticketStatus = PopulateTicketStatus(),
                    executor = new List<SelectListItem>(),
                    executionHistories = FillFormExecutionHistory(reqticketno),
                    selectedExecType = ViewData["EXECUTION_TYPE"].ToString(),
                    selectedTicketStatus = ViewData["TICKET_STATUS"].ToString(),                    
                    selectedExecutor = (ViewData["TICKET_STATUS"].ToString().Equals("CLOSE")) ? ViewData["TICKET_CLOSE_USER"].ToString() 
                        : (ViewData["TICKET_STATUS"].ToString().Equals("ASSIGNED")) ? ViewData["TICKET_ASSIGN_USER"].ToString()
                        : (ViewData["TICKET_STATUS"].ToString().Equals("HOLD")) ? ViewData["TICKET_HOLD_USER"].ToString()
                        : (ViewData["TICKET_STATUS"].ToString().Equals("REJECTED")) ? ViewData["TICKET_REJECT_USER"].ToString()
                        : "Please select ...",
                    attachmentTable = GetDirectories(CustUtility.FILE_UPLOAD_LOCATION + reqticketno)
                };
                ViewData["EXECUTOR"] = model.selectedExecutor;
                HttpContext.Session.SetObjectAsJson("EXECUTION", model);
                return View(model);
            }
            catch (Exception e)
            {
                ViewData["lblMessage"] = "ERROR: " + e.Message;
                return View();
            }
        }

        private List<SelectListItem> PopulateExecutorList(string sUserGroup, bool sDisabled)
        {
            List<SelectListItem> listItems = new List<SelectListItem>();
            string sSQL;
            sSQL = "SELECT USER_ID FROM TBL_USER ";
            sSQL += "WHERE USER_GROUP = '" + sUserGroup + "' ";
            sSQL += "ORDER BY USER_ID ASC";

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
                        Text = reader["USER_ID"].ToString(),
                        Value = reader["USER_ID"].ToString()
                    };
                    listItems.Add(item);
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
            return listItems;
        }

        private List<SelectListItem> PopulateExecutorType()
        {
            return new List<SelectListItem>
            {
                new SelectListItem { Text = "Single", Value = "Single" },
                new SelectListItem { Text = "Batch", Value = "Batch" }
            };
        }

        private List<SelectListItem> PopulateTicketStatus()
        {
            return new List<SelectListItem>
            {
                new SelectListItem { Text = "OPEN", Value = "OPEN" },
                new SelectListItem { Text = "CLOSE", Value = "CLOSE" },
                new SelectListItem { Text = "ASSIGNED", Value = "ASSIGNED" },
                new SelectListItem { Text = "HOLD", Value = "HOLD" },
                new SelectListItem { Text = "REJECTED", Value = "REJECTED" }
            };
        }

        private ExecutionMasterModel FillFormFields(string ticketNo)
        {
            ExecutionMasterModel master = new ExecutionMasterModel();
            string sSQL;
            sSQL = "SELECT * FROM TBL_ACTIVATION_REQ_MASTER ";
            sSQL += "WHERE TICKET_NO = '" + ticketNo + "';";

            MySqlConnection connection = new MySqlConnection(CustUtility.CONN_STR_SQL);
            try
            {
                MySqlCommand command = new MySqlCommand(sSQL);
                command.Connection = connection;
                connection.Open();
                MySqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    master.ticketId = reader["TICKET_NO"].ToString();
                    master.activationTeam = reader["ACTIVATION_TEAM"].ToString();
                    master.requestedBy = reader["TICKET_CREATE_USER"].ToString();
                    master.requestedDate = Convert.ToDateTime(reader["TICKET_CREATE_Date"].ToString()).ToString("dd-MMM-yyyy hh:mm:ss tt");
                    master.serviceName = reader["SERVICE_NAME"].ToString();
                    master.productName = reader["PRODUCT_NAME"].ToString();
                    master.channelType = reader["CHANNEL_TYPE"].ToString();
                    master.channelName = reader["CHANNEL_NAME"].ToString();
                    master.totalQuantity = reader["TOTAL_QUANTITY"].ToString();
                    master.checkUrgent = (reader["IS_URGENT"].ToString() == "YES") ? "checked" : "unchecked";
                    master.remarks = reader["REMARKS"].ToString();
                    master.comments = reader["COMMENTS"].ToString();
                    master.remainingQuantity = reader["REM_QUANTITY"].ToString();
                    master.executionQuantity = (reader["EXEC_QUANTITY"].ToString() == "0") ? "" : reader["EXEC_QUANTITY"].ToString();
                    master.execType = reader["EXEC_TYPE"].ToString();
                    ViewData["EXECUTION_TYPE"] = master.execType;
                    master.ticketStatus = reader["TICKET_STATUS"].ToString();
                    ViewData["TICKET_STATUS"] = master.ticketStatus;
                    switch (reader["TICKET_STATUS"].ToString())
                    {
                        case "CLOSE":
                            master.execUser = reader["TICKET_CLOSE_USER"].ToString();
                            ViewData["TICKET_CLOSE_USER"] = master.execUser;
                            break;
                        case "ASSIGNED":
                            master.execUser = reader["TICKET_ASSIGN_USER"].ToString();
                            ViewData["TICKET_ASSIGN_USER"] = master.execUser;
                            break;
                        case "HOLD":
                            master.execUser = reader["TICKET_HOLD_USER"].ToString();
                            ViewData["TICKET_HOLD_USER"] = master.execUser;
                            break;
                        case "REJECTED":
                            master.execUser = reader["TICKET_REJECT_USER"].ToString();
                            ViewData["TICKET_REJECT_USER"] = master.execUser;
                            break;  
                    }                    
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
            ViewData["btnSave_Enabled"] = "enabled";
            return master;
        }

        private List<MsisdnSimMasterModel> FillFormMsisdn(string ticketNo)
        {
            List<MsisdnSimMasterModel> msisdnSimList = new List<MsisdnSimMasterModel>();
            string sSQL;
            sSQL = "SELECT MOBILE_NO, SIM_NO FROM TBL_ACTIVATION_REQ_DETAIL ";
            sSQL += "WHERE TICKET_NO = '" + ticketNo + "' ";
            sSQL += "ORDER BY SEQ_NO ASC;";

            MySqlConnection connection = new MySqlConnection(CustUtility.CONN_STR_SQL);
            try
            {
                MySqlCommand command = new MySqlCommand(sSQL);
                command.Connection = connection;
                connection.Open();
                MySqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    MsisdnSimMasterModel msisdnSim = new MsisdnSimMasterModel
                    {
                        msisdn = reader["MOBILE_NO"].ToString(),
                        simNo = reader["SIM_NO"].ToString()
                    };
                    msisdnSimList.Add(msisdnSim);
                }
                for (int i = msisdnSimList.Count - 1; i < 10; i++)
                {
                    MsisdnSimMasterModel msisdnSimMaster = new MsisdnSimMasterModel
                    {
                        msisdn = "",
                        simNo = ""
                    };
                    msisdnSimList.Add(msisdnSimMaster);
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
            return msisdnSimList;
        }

        private List<ExecutionHistory> FillFormExecutionHistory(string ticketNo)
        {
            List<ExecutionHistory> histories = new List<ExecutionHistory>();
            string sSQL;
            sSQL = "SELECT EXEC_DATE,EXEC_QUANTITY,EXEC_USER FROM TBL_ACTIVATION_REQ_EXEC ";
            sSQL += "WHERE TICKET_NO = '" + ticketNo + "' ";
            sSQL += "ORDER BY EXEC_DATE DESC";

            MySqlConnection connection = new MySqlConnection(CustUtility.CONN_STR_SQL);
            try
            {
                MySqlCommand command = new MySqlCommand(sSQL);
                command.Connection = connection;
                connection.Open();
                MySqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    ExecutionHistory history = new ExecutionHistory
                    {
                        execDate = Convert.ToDateTime(reader["EXEC_DATE"].ToString()).ToString("dd-MMM-yyyy hh:mm:ss tt"),
                        execQty = reader["EXEC_QUANTITY"].ToString(),
                        execUser = reader["EXEC_USER"].ToString()
                    };
                    histories.Add(history);
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
            return histories;
        }

        public JsonResult GetExecutorForStatusChange(string ticketStatus)
        {
            var UserInfo = HttpContext.Session.GetObjectFromJson<UserModel>("UserInfo");
            List<SelectListItem> executorList = PopulateExecutorList(UserInfo.txtUserGroup, false);
            return Json(executorList);
        }

        [HttpPost]
        public IActionResult btnSave_Click(ActivationExecutionViewModel model,
            string txtExecutionQuantity, string txtRemainingQuantity, string txtComments)
        {
            var SessionActReq = HttpContext.Session.GetObjectFromJson<ActivationExecutionViewModel>("EXECUTION");
            string reqTicketNo = SessionActReq.ticketId;
            ExecutionMasterModel existingTicketDetails = FillFormFields(reqTicketNo);
            string chosenTicketStatus = string.IsNullOrEmpty(model.selectedTicketStatus) ? SessionActReq.executionMaster.ticketStatus : model.selectedTicketStatus;
            string chosenExecutor = string.IsNullOrEmpty(model.selectedExecutor) ? SessionActReq.executionMaster.execUser : model.selectedExecutor;
            if (string.IsNullOrEmpty(existingTicketDetails.ticketStatus))
            {
                existingTicketDetails = SessionActReq.executionMaster;
            }
            if (chosenTicketStatus.Equals("OPEN"))
            {
                ModelState.AddModelError("Error", "Please select Ticket Status Other than OPEN.");
                model = ReturnDefaultModel();
                return View("Execution", model);
            }
            if (chosenTicketStatus.Equals("ASSIGNED") && chosenExecutor.Equals("Please select ..."))
            {
                ModelState.AddModelError("Error", "Please select an Executor to Assign this ticket.");
                model = ReturnDefaultModel();
                return View("Execution", model);
            }
            if (chosenTicketStatus.Equals("ASSIGNED") && !existingTicketDetails.ticketStatus.Equals("OPEN"))
            {
                if (!chosenTicketStatus.Equals("ASSIGNED"))
                {
                    ModelState.AddModelError("Error", "You cannot Assign this ticket to any Executor as it is not OPEN.");
                    model = ReturnDefaultModel();
                    return View("Execution", model);
                }
            }
            if (chosenTicketStatus.Equals("REJECTED") && !existingTicketDetails.ticketStatus.Equals("ASSIGNED"))
            {
                ModelState.AddModelError("Error", "You must first Assign this ticket to some Executor.");
                model = ReturnDefaultModel();
                return View("Execution", model);
            }
            if (chosenTicketStatus.Equals("HOLD") && !existingTicketDetails.ticketStatus.Equals("ASSIGNED") &&
                !existingTicketDetails.ticketStatus.Equals("HOLD"))
            {
                ModelState.AddModelError("Error", "You must first Hold or Assign this ticket to some Executor.");
                model = ReturnDefaultModel();
                return View("Execution", model);
            }
            if (chosenTicketStatus.Equals("CLOSE") && !existingTicketDetails.ticketStatus.Equals("ASSIGNED") &&
                !existingTicketDetails.ticketStatus.Equals("HOLD"))
            {
                ModelState.AddModelError("Error", "You must first Hold or Assign this ticket to some Executor.");
                model = ReturnDefaultModel();
                return View("Execution", model);
            }
            if (chosenTicketStatus.Equals("ASSIGNED") && existingTicketDetails.ticketStatus.Equals("ASSIGNED"))
            {
                if (txtExecutionQuantity.Equals(""))
                {
                    ModelState.AddModelError("Error", "Given Execution Quantity is invalid (Blank).");
                    model = ReturnDefaultModel();
                    return View("Execution", model);
                }
                else if (Convert.ToInt32(txtExecutionQuantity) <= 0)
                {
                    ModelState.AddModelError("Error", "Given Execution Quantity is invalid (Zero Or Negative).");
                    model = ReturnDefaultModel();
                    return View("Execution", model);
                }
                else if (Convert.ToInt32(txtExecutionQuantity) > Convert.ToInt32(txtRemainingQuantity))
                {
                    ModelState.AddModelError("Error", "Given Execution Quantity is more than the Remaining Quantity.");
                    model = ReturnDefaultModel();
                    return View("Execution", model);
                }
            }
            if (chosenTicketStatus.Equals("HOLD") && existingTicketDetails.ticketStatus.Equals("HOLD"))
            {
                if (txtExecutionQuantity.Equals(""))
                {
                    ModelState.AddModelError("Error", "Given Execution Quantity is invalid (Blank).");
                    model = ReturnDefaultModel();
                    return View("Execution", model);
                }
                else if (Convert.ToInt32(txtExecutionQuantity) <= 0)
                {
                    ModelState.AddModelError("Error", "Given Execution Quantity is invalid (Zero Or Negative).");
                    model = ReturnDefaultModel();
                    return View("Execution", model);
                }
                else if (Convert.ToInt32(txtExecutionQuantity) > Convert.ToInt32(txtRemainingQuantity))
                {
                    ModelState.AddModelError("Error", "Given Execution Quantity is more than the Remaining Quantity.");
                    model = ReturnDefaultModel();
                    return View("Execution", model);
                }
            }
            if (chosenTicketStatus.Equals("CLOSE") && !txtRemainingQuantity.Equals("0"))
            {
                ModelState.AddModelError("Error", "Execution Quantity should be Blank for CLOSE status.");
                model = ReturnDefaultModel();
                return View("Execution", model);
            }
            if (chosenTicketStatus.Equals("REJECTED") && !txtExecutionQuantity.Equals("0"))
            {
                ModelState.AddModelError("Error", "Execution Quantity should be Blank for REJECTED status.");
                model = ReturnDefaultModel();
                return View("Execution", model);
            }

            string sSQL;
            string sTicketNo = reqTicketNo;
            string sTicketStatus = chosenTicketStatus;
            string sComments = txtComments;
            string sExecType = existingTicketDetails.execType;
            string sExecDate = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
            string sExecUser = chosenExecutor;
            string sExecQuantity = (txtExecutionQuantity.Trim() == "") ? "0" : txtExecutionQuantity.Trim();
            string sRemQuantity;
            if (Convert.ToInt32(txtRemainingQuantity) != 0) {
                sRemQuantity = Convert.ToString(Convert.ToInt32(txtRemainingQuantity) - Convert.ToInt32(sExecQuantity));
            } else {
                sRemQuantity = "0";
            }            
            string sCurrentExecUser = HttpContext.Session.GetString("UserID");
            bool bTicketStatusChanged = (sTicketStatus != existingTicketDetails.ticketStatus) ? true : false;
            int nResult = 0;

            sSQL = "UPDATE TBL_ACTIVATION_REQ_MASTER ";
            sSQL += "SET ";
            sSQL += "COMMENTS = '" + sComments + "', ";
            sSQL += "EXEC_TYPE = '" + sExecType + "', ";
            if (sExecQuantity != "0")
            {
                sSQL += "EXEC_QUANTITY = '" + sExecQuantity + "', ";
                sSQL += "REM_QUANTITY = '" + sRemQuantity + "', ";
            }
            sSQL += "TICKET_STATUS = '" + sTicketStatus + "' ";
            if ((sTicketStatus == "ASSIGNED") && (bTicketStatusChanged == true))
            {
                sSQL += ", ";
                sSQL += "TICKET_ASSIGN_DATE = '" + sExecDate + "', ";
                sSQL += "TICKET_ASSIGN_USER = '" + sExecUser + "' ";
            }
            else if ((sTicketStatus == "HOLD") && (bTicketStatusChanged == true))
            {
                sSQL += ", ";
                sSQL += "TICKET_HOLD_DATE = '" + sExecDate + "', ";
                sSQL += "TICKET_HOLD_USER = '" + sExecUser + "' ";
            }
            else if ((sTicketStatus == "REJECTED") && (bTicketStatusChanged == true))
            {
                sSQL += ", ";
                sSQL += "TICKET_REJECT_DATE = '" + sExecDate + "', ";
                sSQL += "TICKET_REJECT_USER = '" + sExecUser + "' ";
            }
            else if ((sTicketStatus == "CLOSE") && (bTicketStatusChanged == true))
            {
                sSQL += ", ";
                sSQL += "TICKET_CLOSE_DATE = '" + sExecDate + "', ";
                sSQL += "TICKET_CLOSE_USER = '" + sExecUser + "' ";
            }
            sSQL += "WHERE TICKET_NO = '" + sTicketNo + "'";

            MySqlConnection connection = new MySqlConnection(CustUtility.CONN_STR_SQL);
            try
            {
                MySqlCommand command = new MySqlCommand(sSQL);
                command.Connection = connection;
                connection.Open();
                MySqlTransaction transaction = connection.BeginTransaction();
                command.Transaction = transaction;
                nResult = command.ExecuteNonQuery();

                if (bTicketStatusChanged == true)
                {
                    sSQL = "INSERT INTO TBL_ACTIVATION_REQ_HISTORY(TICKET_NO, CURRENT_STATUS, CURRENT_STATUS_DATE, CURRENT_STATUS_USER) ";
                    sSQL += "VALUES('" + sTicketNo + "', '" + sTicketStatus + "', '" + sExecDate + "', '" + sExecUser + "')";
                    command = new MySqlCommand(sSQL);
                    command.Connection = connection;
                    command.Transaction = transaction;
                    nResult = command.ExecuteNonQuery();
                }

                if (Convert.ToInt32(sExecQuantity) != 0 && sTicketStatus.Equals("ASSIGNED"))
                {
                    sSQL = "INSERT INTO TBL_ACTIVATION_REQ_EXEC(TICKET_NO, EXEC_TYPE, EXEC_QUANTITY, EXEC_USER, EXEC_DATE)";
                    sSQL += "VALUES('" + sTicketNo + "', '" + sExecType + "', '" + sExecQuantity + "', '" + sCurrentExecUser + "', '" + sExecDate + "')";
                    command = new MySqlCommand(sSQL);
                    command.Connection = connection;
                    command.Transaction = transaction;
                    nResult = command.ExecuteNonQuery();
                }
                transaction.Commit();
            }
            catch (Exception e)
            {
                ViewData["lblMessage"] = "ERROR: " + e.Message;
            }
            finally
            {
                if (connection != null)
                {
                    connection.Close();
                }
                connection = null;
            }

            var UserInfo = HttpContext.Session.GetObjectFromJson<UserModel>("UserInfo");
            if (UserInfo == null)
            {
                return RedirectToAction("Error", "Home");
            }
            ViewData["lblWelcome"] = "Welcome, " + HttpContext.Session.GetString("UserID");
            ViewData["TICKET_STATUS"] = sTicketStatus;
            ViewData["EXECUTION_TYPE"] = sExecType;

            ActivationExecutionViewModel updatedModel = new ActivationExecutionViewModel
            {
                ticketId = sTicketNo,
                executor = PopulateExecutorList(UserInfo.txtUserGroup, true),
                executionMaster = FillFormFields(sTicketNo),
                msisdnList = FillFormMsisdn(sTicketNo),
                executionHistories = FillFormExecutionHistory(sTicketNo),
                ticketStatus = PopulateTicketStatus(),
                execType = PopulateExecutorType(),
                selectedExecType = ViewData["EXECUTION_TYPE"].ToString(),
                selectedTicketStatus = ViewData["TICKET_STATUS"].ToString(),
                selectedExecutor = (ViewData["TICKET_STATUS"].ToString().Equals("CLOSE")) ? ViewData["TICKET_CLOSE_USER"].ToString() 
                    : (ViewData["TICKET_STATUS"].ToString().Equals("ASSIGNED")) ? ViewData["TICKET_ASSIGN_USER"].ToString()
                    : (ViewData["TICKET_STATUS"].ToString().Equals("HOLD")) ? ViewData["TICKET_HOLD_USER"].ToString()
                    : (ViewData["TICKET_STATUS"].ToString().Equals("REJECTED")) ? ViewData["TICKET_REJECT_USER"].ToString()
                    : "Please select ...",
                attachmentTable = GetDirectories(CustUtility.FILE_UPLOAD_LOCATION + sTicketNo)
            };
            ViewData["EXECUTOR"] = sExecUser;
            try
            {
                if (chosenTicketStatus.Equals("CLOSE"))
                {
                    ViewData["btnSave_Enabled"] = "disabled";
                    SendCloseMail(reqTicketNo, updatedModel.executionMaster.requestedBy);
                }
                else if (chosenTicketStatus.Equals("REJECTED"))
                {
                    ViewData["btnSave_Enabled"] = "disabled";
                    SendRejectedMail(reqTicketNo, updatedModel.executionMaster.requestedBy);
                }
                else if (chosenTicketStatus.Equals("HOLD"))
                {
                    if (existingTicketDetails.ticketStatus.Equals("ASSIGNED"))
                    {
                        SendHoldMail(reqTicketNo, updatedModel.executionMaster.requestedBy);
                    }
                }
                ModelState.AddModelError("Error", "Execution information [Ticket# " + sTicketNo + "] updated successfully.");
                HttpContext.Session.SetObjectAsJson("EXECUTION", updatedModel);
                return View("Execution", updatedModel);
            }
            catch (Exception e)
            {
                ViewData["lblMessage"] = "ERROR: " + e.Message;
                ModelState.AddModelError("Error", "Execution information [Ticket# " + sTicketNo + "] updated successfully.");
                HttpContext.Session.SetObjectAsJson("EXECUTION", updatedModel);
                return View("Execution", updatedModel);
            }
        }

        private ActivationExecutionViewModel ReturnDefaultModel()
        {
            var SessionVal = HttpContext.Session.GetObjectFromJson<ActivationExecutionViewModel>("EXECUTION");
            ActivationExecutionViewModel viewModel = new ActivationExecutionViewModel
            {
                ticketId = SessionVal.ticketId,
                executor = SessionVal.executor,
                executionMaster = SessionVal.executionMaster,
                msisdnList = SessionVal.msisdnList,
                execType = SessionVal.execType,
                ticketStatus = SessionVal.ticketStatus,
                executionHistories = SessionVal.executionHistories,
                selectedExecutor = SessionVal.selectedExecutor,
                selectedExecType = SessionVal.selectedExecType,
                selectedTicketStatus = SessionVal.selectedTicketStatus,
                attachmentTable = GetDirectories(CustUtility.FILE_UPLOAD_LOCATION + SessionVal.ticketId)
            };
            return viewModel;
        }

        private void SendCloseMail(string ticketNo, string requestedBy)
        {
            SmtpClient smtp = new SmtpClient();
            MailMessage mail = new MailMessage();

            try
            {
                mail.From = new MailAddress(CustUtility.MAIL_FROM_ADDRESS);
                mail.Subject = "Activation Ticket# " + ticketNo + " Resolved";
                mail.Body = "Ticket Ref: " + ticketNo + "\n\n" + "Your Activation Ticket has been Resolved.";
                mail.IsBodyHtml = false;
                mail.To.Add(requestedBy + "@grameenphone.com");

                smtp.Host = CustUtility.MAIL_SMTP_HOST;
                smtp.Port = 25;
                smtp.Send(mail);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        private void SendHoldMail(string ticketNo, string requestedBy)
        {
            SmtpClient smtp = new SmtpClient();
            MailMessage mail = new MailMessage();

            try
            {
                mail.From = new MailAddress(CustUtility.MAIL_FROM_ADDRESS);
                mail.Subject = "Activation Ticket# " + ticketNo + " on Hold";
                mail.Body = "Ticket Ref: " + ticketNo + "\n\n" + "Your Activation Ticket is on Hold. Please see the Executor Comments field for details.";
                mail.IsBodyHtml = false;
                mail.To.Add(requestedBy + "@grameenphone.com");

                smtp.Host = CustUtility.MAIL_SMTP_HOST;
                smtp.Port = 25;
                smtp.Send(mail);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        private void SendRejectedMail(string ticketNo, string requestedBy)
        {
            SmtpClient smtp = new SmtpClient();
            MailMessage mail = new MailMessage();

            try
            {
                mail.From = new MailAddress(CustUtility.MAIL_FROM_ADDRESS);
                mail.Subject = "Rejection of Activation Ticket# " + ticketNo;
                mail.Body = "Ticket Ref: " + ticketNo + "\n\n" + "Sorry! Your Activation Ticket has been Rejected. Please see the Executor Comments field for clarification.";
                mail.IsBodyHtml = false;
                mail.To.Add(requestedBy + "@grameenphone.com");

                smtp.Host = CustUtility.MAIL_SMTP_HOST;
                smtp.Port = 25;
                smtp.Send(mail);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public IActionResult btnReturn_Click()
        {
            return RedirectToAction("Dashboard", "ActivationDashboard");
        }
    }
}