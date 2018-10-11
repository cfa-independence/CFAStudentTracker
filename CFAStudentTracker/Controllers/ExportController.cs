using CFAStudentTracker.Models;
using CFAStudentTracker.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using System.Data.Entity;
using System.Web;
using System.Data.OleDb;

namespace CFAStudentTracker.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ExportController : Controller
    {

        CFAEntities db = new CFAEntities();
        public ActionResult ExportErrors()
        {
            ViewBag.Queue = new SelectList(db.Queue, "QueueID", "QueueDescription");
            return View();
        }
        [HttpPost]
        public ActionResult ExportErrors(string Queue, [Bind(Include = "startDate,endDate,userName")] IndexFilter filter)
        {
            short queueID = short.Parse(Queue);
            var files = db.StudentFile.Include(s => s.Record);
            var q = from e in db.ProcessingError
                    join c in db.ErrorComplete
                        on e.ErrorComID equals c.ErrorComID
                    join et in db.ErrorType
                        on e.ErrorTypeID equals et.ErrorTypeID
                    join p in db.Processing
                        on e.ProcID equals p.ProcID
                    join r in db.Record
                        on p.RecordID equals r.RecordID
                    join t in db.FileType
                        on r.FileTypeID equals t.FileTypeID
                    join f in db.StudentFile
                        on r.FileID equals f.FileID
                    where p.QueueID == queueID && e.DateFound >= filter.startDate && e.DateFound <= filter.endDate
                    select new LookUp { e=e, p = p, r = r, f = f, t = t, et=et, c=c };
            var dbList = new System.Data.DataTable("DB");
            dbList.Columns.Add("Name");
            dbList.Columns.Add("SSN");
            dbList.Columns.Add("File Type");
            dbList.Columns.Add("Date Found");
            dbList.Columns.Add("Error Type");
            dbList.Columns.Add("Error Completed");
            dbList.Columns.Add("Complete Type");
            dbList.Columns.Add("Username");
            dbList.Columns.Add("Note");

            foreach (var item in q)
            {
                dbList.Rows.Add(item.f.FileName, item.f.FileSSN, item.t.TypeDescription, item.e.DateFound, item.et.Description, item.e.DateComplete, item.c.Description, item.p.Username, item.e.Note);
            }

            var grid = new GridView();
            grid.DataSource = dbList;
            grid.DataBind();

            Response.ClearContent();
            Response.Buffer = true;
            Response.AddHeader("content-disposition", "attachment; filename=ErrorList.xls");
            Response.ContentType = "application/ms-excel";

            Response.Charset = "";
            StringWriter sw = new StringWriter();
            HtmlTextWriter htw = new HtmlTextWriter(sw);

            grid.RenderControl(htw);

            Response.Output.Write(sw.ToString());
            Response.Flush();
            Response.End();

            return Redirect(Request.UrlReferrer.ToString());
        }

        public ActionResult ExportQueues()
        {
            ViewBag.Queue = new SelectList(db.Queue, "QueueID", "QueueDescription");
            return View();
        }
        [HttpPost]
        public ActionResult ExportQueues(string Queue, [Bind(Include = "startDate,endDate,userName")] IndexFilter filter)
        {
            short queueID = short.Parse(Queue);
            var files = db.StudentFile.Include(s => s.Record);
            var q = from p in db.Processing
                    join r in db.Record
                        on p.RecordID equals r.RecordID
                    join t in db.FileType
                        on r.FileTypeID equals t.FileTypeID
                    join f in db.StudentFile
                        on r.FileID equals f.FileID
                    where p.QueueID == queueID && p.ProcInQueue >= filter.startDate  && p.ProcInQueue <= filter.endDate
                    select new LookUp { p = p, r = r, f = f, t = t };
            var dbList = new System.Data.DataTable("DB");
            dbList.Columns.Add("ProcID");
            dbList.Columns.Add("Name");
            dbList.Columns.Add("SSN");
            dbList.Columns.Add("Type");
            dbList.Columns.Add("In Queue");
            dbList.Columns.Add("To User");
            dbList.Columns.Add("User Completed");
            dbList.Columns.Add("Username");

            foreach (var item in q)
            {
                dbList.Rows.Add(item.p.ProcID, item.f.FileName, item.f.FileSSN, item.t.TypeDescription, item.p.ProcInQueue, item.p.ProcToUser, item.p.ProcUserComplete, item.p.Username);
            }

            var grid = new GridView();
            grid.DataSource = dbList;
            grid.DataBind();

            Response.ClearContent();
            Response.Buffer = true;
            Response.AddHeader("content-disposition", "attachment; filename=DB.xls");
            Response.ContentType = "application/ms-excel";

            Response.Charset = "";
            StringWriter sw = new StringWriter();
            HtmlTextWriter htw = new HtmlTextWriter(sw);

            grid.RenderControl(htw);

            Response.Output.Write(sw.ToString());
            Response.Flush();
            Response.End();

            return Redirect(Request.UrlReferrer.ToString());
        }
    
    // GET: Export
    public ActionResult ExportAllQueues()
        {
            var files = db.StudentFile.Include(s => s.Record);
            var q = from p in db.Processing
                    join r in db.Record
                        on p.RecordID equals r.RecordID
                    join t in db.FileType
                        on r.FileTypeID equals t.FileTypeID
                    join f in db.StudentFile
                        on r.FileID equals f.FileID
                    select new LookUp { p = p, r = r, f = f, t = t };
            var dbList = new System.Data.DataTable("DB");
            dbList.Columns.Add("Name");
            dbList.Columns.Add("SSN");
            dbList.Columns.Add("Type");
            dbList.Columns.Add("In Queue");
            dbList.Columns.Add("To User");
            dbList.Columns.Add("User Completed");
            dbList.Columns.Add("Username");

            foreach (var item in q)
            {
                dbList.Rows.Add(item.f.FileName, item.f.FileSSN, item.t.TypeDescription, item.p.ProcInQueue, item.p.ProcToUser, item.p.ProcUserComplete, item.p.Username);
            }

            var grid = new GridView();
            grid.DataSource = dbList;
            grid.DataBind();

            Response.ClearContent();
            Response.Buffer = true;
            Response.AddHeader("content-disposition", "attachment; filename=DB.xls");
            Response.ContentType = "application/ms-excel";

            Response.Charset = "";
            StringWriter sw = new StringWriter();
            HtmlTextWriter htw = new HtmlTextWriter(sw);

            grid.RenderControl(htw);

            Response.Output.Write(sw.ToString());
            Response.Flush();
            Response.End();

            return Redirect(Request.UrlReferrer.ToString());
        }

        public ActionResult Index()
        {
            return View();
        }
        public ActionResult ImportTimeSheet()
        {
            
            return View();
        }

        [HttpPost]
        public async System.Threading.Tasks.Task<ActionResult> ImportTimeSheet(HttpPostedFileBase file)
        {

            DataSet EmployeeTimeSheet = new DataSet();
            if (Request.Files["file"].ContentLength > 0)
            {
                string fileExtension =
                                     System.IO.Path.GetExtension(Request.Files["file"].FileName);

                if (fileExtension == ".xls" || fileExtension == ".xlsx")
                {
                    string fileLocation = Server.MapPath("~/Content/") + Request.Files["file"].FileName;
                    if (System.IO.File.Exists(fileLocation))
                    {

                        System.IO.File.Delete(fileLocation);
                    }
                    Request.Files["file"].SaveAs(fileLocation);
                    string excelConnectionString = string.Empty;
                    excelConnectionString = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" +
                    fileLocation + ";Extended Properties=\"Excel 12.0;HDR=Yes;IMEX=2\"";
                    //connection String for xls file format.
                    if (fileExtension == ".xls")
                    {
                        excelConnectionString = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" +
                        fileLocation + ";Extended Properties=\"Excel 8.0;HDR=Yes;IMEX=2\"";
                    }
                    //connection String for xlsx file format.
                    else if (fileExtension == ".xlsx")
                    {
                        excelConnectionString = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" +
                        fileLocation + ";Extended Properties=\"Excel 12.0;HDR=Yes;IMEX=2\"";
                    }
                    //Create Connection to Excel work book and add oledb namespace
                    OleDbConnection excelConnection = new OleDbConnection(excelConnectionString);
                    excelConnection.Open();
                    DataTable dt = new DataTable();

                    dt = excelConnection.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);
                    if (dt == null)
                    {
                        return null;
                    }

                    String[] excelSheets = new String[dt.Rows.Count];
                    int t = 0;
                    //excel data saves in temp file here.
                    foreach (DataRow row in dt.Rows)
                    {
                        excelSheets[t] = row["TABLE_NAME"].ToString();
                        t++;
                    }
                    OleDbConnection excelConnection1 = new OleDbConnection(excelConnectionString);
                    //------------------------------------------------------
                    //SELECT column names below
                    //------------------------------------------------------
                    string query = string.Format("Select Date, Type, Norm, OT, OT2, User  from [{0}]", excelSheets[0]);
                    //------------------------------------------------------
                    //SELECT column names above
                    //------------------------------------------------------
                    using (OleDbDataAdapter dataAdapter = new OleDbDataAdapter(query, excelConnection1))
                    {
                        dataAdapter.Fill(EmployeeTimeSheet);
                    }

                }

                
                
                for (int i = 0; i < EmployeeTimeSheet.Tables[0].Rows.Count; i++)
                {

                    //checks for blank cells
                    if (!String.IsNullOrEmpty(EmployeeTimeSheet.Tables[0].Rows[i][0].ToString()))
                    {
                        //checks for actual time worked
                        if (EmployeeTimeSheet.Tables[0].Rows[i][1].ToString() == "[0]REG" || EmployeeTimeSheet.Tables[0].Rows[i][1].ToString() == "[12]WRKS")
                        {
                            var date = EmployeeTimeSheet.Tables[0].Rows[i][0].ToString();
                            var type = EmployeeTimeSheet.Tables[0].Rows[i][1].ToString();
                            var amount = double.Parse(EmployeeTimeSheet.Tables[0].Rows[i][2].ToString()) + double.Parse(EmployeeTimeSheet.Tables[0].Rows[i][3].ToString()) + double.Parse(EmployeeTimeSheet.Tables[0].Rows[i][4].ToString());
                            var user = EmployeeTimeSheet.Tables[0].Rows[i][5].ToString();
                            //makes sure not lon last row
                                //sums up duplicate rows for date                
                                while (i < EmployeeTimeSheet.Tables[0].Rows.Count - 1 && date == EmployeeTimeSheet.Tables[0].Rows[i + 1][0].ToString()
                                    && user == EmployeeTimeSheet.Tables[0].Rows[i + 1][5].ToString())
                                {
                                    //checks type again
                                    if (type == EmployeeTimeSheet.Tables[0].Rows[i + 1][1].ToString())
                                    {
                                        amount += double.Parse(EmployeeTimeSheet.Tables[0].Rows[i + 1][2].ToString())
                                            + double.Parse(EmployeeTimeSheet.Tables[0].Rows[i + 1][3].ToString())
                                            + double.Parse(EmployeeTimeSheet.Tables[0].Rows[i + 1][4].ToString());
                                    }
                                    //Checks next row to make sure there is not more than 2 dups.
                                    i++;
                                }

                            Hour hour = new Hour();
                            hour.HourDate = DateTime.Parse(date);
                            hour.HourAmount = (decimal)amount;
                            hour.User = db.User.Find(user);
                            //&& h.HourDate == hour.HourDate.Date
                            var find = db.Hour.Where(h => h.Username == hour.User.Username && h.HourDate == hour.HourDate.Date).ToList(); ;
                            if (find.Count ==0)
                            {
                                if (ModelState.IsValid)
                                {
                                    db.Hour.Add(hour);
                                    await db.SaveChangesAsync();
                                }
                            } else
                            {
                                Hour edit = find.ToList()[0];
                                if (edit.HourAmount != hour.HourAmount)
                                {
                                    edit.HourAmount = hour.HourAmount;
                                    edit.HourNotes +=" |Auto Updated from " + edit.HourAmount;
                                    if (ModelState.IsValid)
                                    {
                                        db.Entry(edit).State = EntityState.Modified;
                                        await db.SaveChangesAsync();
                                    }
                                }
                                
                            }
                            
                        }
                    }

                }
            }


            return Redirect(Request.UrlReferrer.ToString());
        }

        public ActionResult ImportRejects()
        {
            var b = false;
            ViewBag.Type = new SelectList(db.ErrorType, "ErrorTypeID", "Description");
            ViewBag.Queue = new SelectList(db.Queue, "QueueID", "QueueDescription");
            ViewBag.BoolCO = b;
            return View();
        }
        [HttpPost]
        public async System.Threading.Tasks.Task<ActionResult> ImportRejects(HttpPostedFileBase file, string Type, string Queue, string BoolCO)
        {

            DataSet Rejects = new DataSet();
            if (Request.Files["file"].ContentLength > 0)
            {
                string fileExtension =
                                     System.IO.Path.GetExtension(Request.Files["file"].FileName);

                if (fileExtension == ".xls" || fileExtension == ".xlsx")
                {
                    string fileLocation = Server.MapPath("~/Content/") + Request.Files["file"].FileName;
                    if (System.IO.File.Exists(fileLocation))
                    {

                        System.IO.File.Delete(fileLocation);
                    }
                    Request.Files["file"].SaveAs(fileLocation);
                    string excelConnectionString = string.Empty;
                    excelConnectionString = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" +
                    fileLocation + ";Extended Properties=\"Excel 12.0;HDR=Yes;IMEX=2\"";
                    //connection String for xls file format.
                    if (fileExtension == ".xls")
                    {
                        excelConnectionString = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" +
                        fileLocation + ";Extended Properties=\"Excel 8.0;HDR=Yes;IMEX=2\"";
                    }
                    //connection String for xlsx file format.
                    else if (fileExtension == ".xlsx")
                    {
                        excelConnectionString = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" +
                        fileLocation + ";Extended Properties=\"Excel 12.0;HDR=Yes;IMEX=2\"";
                    }
                    //Create Connection to Excel work book and add oledb namespace
                    OleDbConnection excelConnection = new OleDbConnection(excelConnectionString);
                    excelConnection.Open();
                    DataTable dt = new DataTable();

                    dt = excelConnection.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);
                    if (dt == null)
                    {
                        return null;
                    }

                    String[] excelSheets = new String[dt.Rows.Count];
                    int t = 0;
                    //excel data saves in temp file here.
                    foreach (DataRow row in dt.Rows)
                    {
                        excelSheets[t] = row["TABLE_NAME"].ToString();
                        t++;
                    }
                    OleDbConnection excelConnection1 = new OleDbConnection(excelConnectionString);
                    //------------------------------------------------------
                    //SELECT column names below
                    //------------------------------------------------------
                    string query = string.Format("Select SSN, Note  from [{0}]", excelSheets[0]);
                    //------------------------------------------------------
                    //SELECT column names above
                    //------------------------------------------------------
                    using (OleDbDataAdapter dataAdapter = new OleDbDataAdapter(query, excelConnection1))
                    {
                        dataAdapter.Fill(Rejects);
                    }

                }
                short queueID = short.Parse(Queue);
                IQueryable<LookUp> q;
                if (BoolCO == "false")
                {
                    q = from p in db.Processing
                        join r in db.Record
                            on p.RecordID equals r.RecordID
                        join t in db.FileType
                            on r.FileTypeID equals t.FileTypeID
                        join f in db.StudentFile
                            on r.FileID equals f.FileID
                        where p.ProcUserComplete != null && p.Username!= null && t.TypeDescription != "Crossover" && p.QueueID == queueID
                        orderby p.ProcUserComplete descending
                        select new LookUp { p = p, r = r, f = f, t = t };
                } else
                {
                    q = from p in db.Processing
                        join r in db.Record
                            on p.RecordID equals r.RecordID
                        join t in db.FileType
                            on r.FileTypeID equals t.FileTypeID
                        join f in db.StudentFile
                            on r.FileID equals f.FileID
                        where p.ProcUserComplete != null && p.Username != null && p.QueueID == queueID
                        orderby p.ProcUserComplete descending
                        select new LookUp { p = p, r = r, f = f, t = t };
                }
                
                var types = db.ErrorType;
                for (int i = 0; i < Rejects.Tables[0].Rows.Count; i++)
                {

                    //checks for blank cells
                    if (!String.IsNullOrEmpty(Rejects.Tables[0].Rows[i][0].ToString()))
                    {
                        var SSN = Rejects.Tables[0].Rows[i][0].ToString();
                        var Note = Rejects.Tables[0].Rows[i][1].ToString();
                        var error = new ProcessingError();
                        error.ErrorTypeID = byte.Parse(Type);
                        error.DateFound = DateTime.Now;
                        error.Note = Note;
                        var procList = q.Where(w => w.f.FileSSN == SSN).ToList();
                        
                        if (procList.Count > 0)
                        {
                            error.ProcID = procList[0].p.ProcID;
                            
                        } else
                        {
                            error.Note = "SSN: " + SSN + " was not found during Import | " + Note;
                        }
                        if (ModelState.IsValid)
                        {
                            db.ProcessingError.Add(error);
                            await db.SaveChangesAsync();
                        }

                    }

                }
            }


            return Redirect(Request.UrlReferrer.ToString());
        }
    }
}