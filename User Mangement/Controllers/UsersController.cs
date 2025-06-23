using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using User_Mangement.Data;
using User_Mangement.Models;

namespace User_Mangement.Controllers
{
    public class UsersController(UserAuthDbContext db) : Controller
    {
        [Authorize]
        public IActionResult Users()
        {
            List<UserDetails> UserList = db.UserDetails.ToList();
            return View(UserList);
        }

        [Authorize]
        [HttpPost]
        public IActionResult Users(string userName)
        {
            var query = db.UserDetails.AsQueryable();

            if (!string.IsNullOrEmpty(userName))
            {
                query = query.Where(u =>
                    u.FirstName.Contains(userName) ||
                    u.LastName.Contains(userName) ||
                    (u.FirstName + " " + u.LastName).Contains(userName)
                );
            }
            List<UserDetails> userList = query.ToList();
            return View(userList);
        }
        [Authorize]

        public IActionResult Edit(int? UserId)
        {
            if (UserId == null)
            {
                return NotFound();
            }
            var user = db.UserDetails.Find(UserId);
            if (user == null)
            {
                return NotFound();
            }
            return View(user);

        }

        [HttpPost]
        [Authorize]

        public IActionResult Edit(UserDetails? obj)
        {
            if (obj == null || obj.FirstName == null || obj.LastName == null || obj.EmailAddress == null || obj.Gender== null || obj.UserID==0)
            {
                ModelState.AddModelError("fields", "All fields are required");
            }
            if (ModelState.IsValid)
            {
                var user = db.UserDetails.FirstOrDefault(x => x.UserID == obj.UserID);
                if (user == null) return NotFound();

                user.FirstName = obj.FirstName;
                user.LastName = obj.LastName;
                user.Gender = obj.Gender;
                user.DOB = obj.DOB;
                user.EmailAddress = obj.EmailAddress;

                db.SaveChanges();
                return RedirectToAction("Users");
            }
            return View(obj);
        }

        [Authorize]

        public IActionResult Details(int? UserId)
        {
            if (UserId == null)
            {
                return NotFound();
            }
            var user = db.UserDetails.Find(UserId);
            if (user == null)
            {
                return NotFound();
            }
            return View(user);

        }

    }
}
