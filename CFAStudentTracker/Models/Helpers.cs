using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Office.Interop.Excel;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using System.Threading.Tasks;


namespace CFAStudentTracker.Models
{
    public class Helpers
    {
        private IdentityDbContext ident = new IdentityDbContext();
        private RoleStore<IdentityRole> roleStore;
        private RoleManager<IdentityRole> roleManager;
        private UserStore<ApplicationUser> userStore;
        private UserManager<ApplicationUser> userManager;
        private CFAEntities db = new CFAEntities();
        
        public Helpers()
        {
            roleStore = new RoleStore<IdentityRole>(ident);
            roleManager = new RoleManager<IdentityRole>(roleStore);
            userStore = new UserStore<ApplicationUser>(ident);
            userManager = new UserManager<ApplicationUser>(userStore);
    }
        public async Task<long> GetNextFile(long id, string user)
        {
            var q = db.Queue.Find(id);
            //getnext file
            var studentsInQueue = db.Processing.Include(p => p.Record).OrderBy(p => p.ProcID).Where(p => p.QueueID == id && p.ProcToUser == null && p.ProcUserComplete == null);
            if (studentsInQueue.Count() == 0)
            {
                return -1;
            }
            var priorityStudents = studentsInQueue.OrderBy(p => p.ProcID).Where(p => p.Record.ProcPriority);
            Processing getFile;
            if (priorityStudents.Count() > 0)
            {
                getFile = priorityStudents.First();
                getFile.ProcToUser = DateTime.Now;
                getFile.Username = user;
                db.Entry(getFile).State = EntityState.Modified;
                await db.SaveChangesAsync();
            }
            else
            {
                var QueueOrder = q.QueueOrderID;
                switch (QueueOrder)
                {
                    case 1:
                        getFile = studentsInQueue.First();
                        getFile.ProcToUser = DateTime.Now;
                        getFile.Username = user;
                        db.Entry(getFile).State = EntityState.Modified;
                        await db.SaveChangesAsync();

                        break;
                    case 2:
                        IList<QueuePriority> queueFileTypeOrder = db.QueuePriority.OrderBy(p => p.QPOrder).Where(p => p.QueueID == id).ToList();
                        bool foundfile = false;
                        foreach (var item in queueFileTypeOrder)
                        {
                            var filetypecount = studentsInQueue.Where(p => p.Record.FileTypeID == item.FileTypeID);
                            if (filetypecount.Count() > 0)
                            {
                                foundfile = true;
                                getFile = filetypecount.First();
                                getFile.ProcToUser = DateTime.Now;
                                getFile.Username = user;
                                db.Entry(getFile).State = EntityState.Modified;
                                await db.SaveChangesAsync();

                                break;
                            }
                        }
                        if (!foundfile)
                        {
                            getFile = studentsInQueue.First();
                            getFile.ProcToUser = DateTime.Now;
                            getFile.Username = user;
                            db.Entry(getFile).State = EntityState.Modified;
                            await db.SaveChangesAsync();

                        }
                        break;
                    case 3:
                        var DODstudents = studentsInQueue.OrderBy(p => p.Record.DOD);
                        if (DODstudents.Count() == 0)
                        {
                            getFile = studentsInQueue.First();
                            getFile.ProcToUser = DateTime.Now;
                            getFile.Username = user;
                            db.Entry(getFile).State = EntityState.Modified;
                            await db.SaveChangesAsync();
                            break;
                        }
                        getFile = DODstudents.First();
                        getFile.ProcToUser = DateTime.Now;
                        getFile.Username = user;
                        db.Entry(getFile).State = EntityState.Modified;
                        await db.SaveChangesAsync();

                        break;
                    default:

                        break;
                }
            }

            return 1;
        }
        public double WeightedFileAmount(DateTime start, DateTime end, string username)
        {
            
            var processed = db.Processing.Include(p => p.Record).Where(p => p.Username == username && p.ProcUserComplete >= start && p.ProcUserComplete <= end);
            var queues = processed.Select(p => p.QueueID).Distinct();
            double totalWeightedFiles = 0;
            foreach (var q in queues)
            {
                var proFiltered = processed.Where(p => p.QueueID == q);
                IQueryable<StringAndInt> rr = from pro in proFiltered
                                              group pro by pro.Record.FileType.FileTypeID into proGroup
                                              orderby proGroup.Key
                                              select new StringAndInt { MyString = proGroup.Key.ToString(), MyInt = proGroup.Count() };
                var re = rr.ToList();

                foreach (var item in re)
                {
                    short qid = short.Parse(item.MyString);
                    var w = (double)db.WeightedFile.Where(p => p.QueueID == q && p.FileTypeID == qid).First().WeightedAmount;
                    totalWeightedFiles += item.MyInt * w;
                }
            }



            return totalWeightedFiles;

            
            
        }
        public List<SelectListItem> GetRoles()
        {
            var i = roleManager.Roles;
            List<SelectListItem> roles = new List<SelectListItem>();
            foreach (var x in i)
            {
                roles.Add(new SelectListItem { Text = x.Name, Value = x.Name });
            }
            return roles;
        }

        public void SetRole(string UserID, string Role)
        {
            userManager.AddToRole(UserID, Role);
        }

        public long FindStudentFileID(string ssn)
        {
            if (ssn.Length == 9)
            {
                ssn.Insert(2, "-");
                ssn.Insert(5, "-");
            }
            if (ssn.Length > 11)
            {
                return -1;
            }
            var result = db.StudentFile.Where(s => s.FileSSN == ssn);
            if (result.ToList().Count == 0)
            {
                return -1;
            } else
            {
                return result.ToList()[0].FileID;
            }
        }

        
    }

}