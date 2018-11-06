using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Web.Mvc;
using CFAStudentTracker.Models;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;

namespace CFAStudentTracker.Controllers
{
    //[Authorize(Roles = "Admin")]
    [Authorize(Roles = "Admin")]
    public class MembershipController : Controller
    {
        private MembershipEntities db = new MembershipEntities();
        private CFAEntities cfaDb = new CFAEntities();
        private Helpers help = new Helpers();
        private ApplicationUserManager _userManager;
        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        // GET: Membership
        public ActionResult Index()
        {

            var users = db.AspNetUsers.Include(p => p.AspNetRoles).OrderBy(p => p.UserName);
            return View(users);
        }

        // GET: Membership/Details/5
        public async Task<ActionResult> Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AspNetUser aspNetUser = await db.AspNetUsers.FindAsync(id);
            if (aspNetUser == null)
            {
                return HttpNotFound();
            }
            return View(aspNetUser);
        }

        public async Task<ActionResult> ResetPassword(string userId)
        {
            ApplicationDbContext context = new ApplicationDbContext();
            UserStore<ApplicationUser> store = new UserStore<ApplicationUser>(context);
            UserManager<ApplicationUser> UserManager = new UserManager<ApplicationUser>(store);
            string newPassword = "P@ssword1"; //"<PasswordAsTypedByUser>";
            string hashedNewPassword = UserManager.PasswordHasher.HashPassword(newPassword);
            ApplicationUser cUser = await store.FindByIdAsync(userId);
            await store.SetPasswordHashAsync(cUser, hashedNewPassword);
            await store.UpdateAsync(cUser);
            return RedirectToAction("Index");
        }

        // GET: Membership/Create
        public ActionResult Create()
        {
            return RedirectToAction("../Account/Register");
        }

        // POST: Membership/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "Id,Email,EmailConfirmed,PasswordHash,SecurityStamp,LockoutEndDateUtc,LockoutEnabled,AccessFailedCount,UserName")] AspNetUser aspNetUser, string role)
        {
            if (ModelState.IsValid)
            {
                db.AspNetUsers.Add(aspNetUser);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(aspNetUser);
        }

        // GET: Membership/Edit/5
        public ActionResult Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AspNetUser aspNetUser = db.AspNetUsers.Include(p => p.AspNetRoles).Where(p => p.Id == id).ToList()[0];
            if (aspNetUser == null)
            {
                return HttpNotFound();
            }
            ViewBag.Supervisors = db.AspNetUsers.Where(u => u.IsSupervisor && u.UserName != aspNetUser.UserName).ToSelectList(u => u.Id, u => u.UserName );
            ViewBag.Roles = help.GetRoles();
            
            return View(aspNetUser);
        }

        // POST: Membership/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "Id,Email,EmailConfirmed,PasswordHash,SecurityStamp,LockoutEndDateUtc,LockoutEnabled,AccessFailedCount,UserName,IsActive,IsSupervisor,SupervisorId")] AspNetUser aspNetUser, string Roles)
        {
            var e = db.AspNetUsers.Include(p => p.AspNetRoles).Where(p => p.Id == aspNetUser.Id).Select(p=>p.AspNetRoles).First();
            string[] x = new string[e.Count()];
            int i = 0;
            foreach (var item in e)
            {
                x[i] = item.Name;
                i++;
            }
            if (ModelState.IsValid)
            {
                UserManager.RemoveFromRoles(aspNetUser.Id,x);
                UserManager.AddToRole(aspNetUser.Id, Roles);
                aspNetUser.UserName = aspNetUser.UserName.ToUpper();
                aspNetUser.Supervisor = db.AspNetUsers.Find(aspNetUser.SupervisorId);
                db.Entry(aspNetUser).State = EntityState.Modified;
                await db.SaveChangesAsync();
                

                User user = await cfaDb.User.FindAsync(aspNetUser.UserName);
                user.IsSupervisor = aspNetUser.IsSupervisor;
                user.SupervisorName = aspNetUser.Supervisor?.UserName;
                cfaDb.Entry(user).State = EntityState.Modified;
                await cfaDb.SaveChangesAsync();

                return RedirectToAction("Index");
            }
            return View(aspNetUser);
        }

        // GET: Membership/Delete/5
        public async Task<ActionResult> Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AspNetUser aspNetUser = await db.AspNetUsers.FindAsync(id);
            if (aspNetUser == null)
            {
                return HttpNotFound();
            }
            return View(aspNetUser);
        }

        // POST: Membership/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(string id)
        {
            AspNetUser aspNetUser = await db.AspNetUsers.FindAsync(id);
            User user = await cfaDb.User.FindAsync(id);
            cfaDb.User.Remove(user);
            db.AspNetUsers.Remove(aspNetUser);

            await cfaDb.SaveChangesAsync();
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        public async Task<ActionResult> ActivateUser(string Id)
        {
            AspNetUser user = await db.AspNetUsers.FindAsync(Id);
            User cfaUser = await cfaDb.User.FindAsync(user.UserName);

            cfaUser.IsActive = true;
            user.IsActive = true;
            await cfaDb.SaveChangesAsync();
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        public async Task<ActionResult> DeactivateUser(string Id)
        {
            AspNetUser user = await db.AspNetUsers.FindAsync(Id);
            User cfaUser = await cfaDb.User.FindAsync(user.UserName);

            cfaUser.IsActive = false;
            user.IsActive = false;
            await cfaDb.SaveChangesAsync();
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
