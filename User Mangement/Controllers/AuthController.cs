using BCrypt.Net;
using MailKit;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MimeKit;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Mail;
using System.Security.Claims;
using System.Text;
using User_Mangement.Data;
using User_Mangement.Models;

namespace User_Mangement.Controllers
{
    public class AuthController (UserAuthDbContext db,IConfiguration configuration) : Controller
    {
        public IActionResult LoginUser()
        {
            return View();
        }

        public IActionResult RegisterUser()
        {
            return View();
        }

        [HttpPost]
        public IActionResult RegisterUser(UserDetails obj)
        {
            if (obj == null || obj.FirstName == null || obj.LastName == null || obj.Gender == null || obj.EmailAddress == null)
            {
                ModelState.AddModelError("Fields", "this is required field");
                return View(obj);
            }
            var user = db.UserDetails.FirstOrDefault(x => x.EmailAddress == obj.EmailAddress);
            if (user!= null)
            {
                ModelState.AddModelError("User Exist", "User already Exist try logining");
                return View(obj);
            }
            db.UserDetails.Add(obj);

            db.SaveChanges();
            user = db.UserDetails.FirstOrDefault(x => x.EmailAddress == obj.EmailAddress);
            var token = GenrateToken(user.UserID);
            String Message = $"this is the link to verify yourself \r\n  https://localhost:7029/Auth/Password?token={token}";
            var status = GenrateMail(Message, user.EmailAddress);

            if (status.Equals("Error"))
            {
                ModelState.AddModelError("Failed_Mail", "Something went wrong try again");
                return View(obj);
            }
            ViewBag.Message = "Please check your email for verification " ;
            return View();
        }
        public String GenrateMail(String Message,string EmailID)
        {
            MimeMessage message = new MimeMessage();
            message.From.Add(new MailboxAddress("Tester", $"{configuration["Email:emailAddress"]}"));
            message.To.Add(MailboxAddress.Parse($"{EmailID}"));
            message.Body = new TextPart("plain")
            {
                Text = $@"{Message}
                        "
            };
            bool flag = false;
            MailKit.Net.Smtp.SmtpClient Client = new MailKit.Net.Smtp.SmtpClient();
            try
            {
                Client.Connect("smtp.gmail.com", 465, true);
                Client.Authenticate(configuration["Email:emailAddress"], configuration["Email:emailPassword"]);
                Client.Send(message);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                flag = true;
            }
            finally
            {
                Client.Disconnect(true);
                Client.Dispose();
            }
            if(flag)
            {
                return "Error";
            }
            return "ok";
        }


        public string GenrateToken(int userId)
        {
            var Token = Guid.NewGuid().ToString();
            db.VaildationTokens.Add(new VaildationToken
            {
                UserId = userId,
                Token = Token,
                ExpiryTime = DateTime.UtcNow.AddHours(1)    
            });
            db.SaveChanges();
            return Token;
        }

        [HttpPost]
        public IActionResult LoginUser(UserLoginDTO obj)
        {
            if (obj == null || obj.EmailId == null || obj.Password == null)
            {
                ModelState.AddModelError("Fields", "this is required field");
                return View(obj);
            }

            var user = db.UserDetails.FirstOrDefault(x => x.EmailAddress == obj.EmailId);
            if (user == null)
            {
                ModelState.AddModelError("User_not_found", "Either Email or password is wrong");
                return View(obj);
            }
            var passwordDetails = db.PasswordDetails.FirstOrDefault(x => x.UserId == user.UserID);
            bool verified = BCrypt.Net.BCrypt.Verify($"{obj.Password}", passwordDetails?.Password);
            if (!verified)
            {
                ModelState.AddModelError("User_not_found", "Either Email or password is wrong");
                return View(obj);
            }

            string JwtToken = CreateToken(user);

            Response.Cookies.Append("jwt", JwtToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict
            });

            return RedirectToAction("Users","Users");
        }

        public VaildationToken? GetVaildationToken(string Token)
        {
            return db.VaildationTokens.FirstOrDefault(x => x.Token == Token);
        }

        public IActionResult Password(String Token)
        {
            var entry = GetVaildationToken(Token);
            if (entry == null || entry.ExpiryTime <= DateTime.UtcNow)
            {
                ViewBag.Message = "Link has Expried";
                return View();
            }
            return View(new PasswordDetailsDTO{Token=Token});
        }
        [HttpPost]
        public IActionResult Password(PasswordDetailsDTO obj)
        {
            if (obj == null || obj.Password!=obj.ConfirmPassword || obj.Password==null || obj.Token==null)
            {
                ModelState.AddModelError("InvalidPassword", "Password and confirm Password doesn't match");
                return View(obj);
            }

            var entry = GetVaildationToken(obj.Token);
            if (entry == null)
            {
                ViewBag.Message = "Link has Expried";
                return View(obj);
            }
            var hashPassword = BCrypt.Net.BCrypt.HashPassword(obj.Password);
            db.PasswordDetails.Add(new PasswordDetails { Password = hashPassword, UserId = entry.UserId});
            db.VaildationTokens.Remove(entry);
            db.SaveChanges();
            return RedirectToAction("Success");
        }

        public IActionResult Success() {
            ViewBag.Message = "User Password has been set ";
            return View();
           }

        public IActionResult RetrivePassword() {
            return View();
        }
        [HttpPost]
        public IActionResult RetrivePassword(RetrivePasswordDTO? obj) { 
            if(obj==null || obj.EmailAddress == null)
            {
                ModelState.AddModelError("Email required", "Email field is required");
            }
            var user = db.UserDetails.FirstOrDefault(x=>x.EmailAddress==obj.EmailAddress);
            if (user == null)
            {
                ModelState.AddModelError("Notfound", "Email was not found");
            }
            if (ModelState.IsValid)
            {
                String token = GenrateToken(user.UserID);
                String Message = $"this is the link to verify yourself \r\n  https://localhost:7029/Auth/ForgetPassword?token={token}";
                var status = GenrateMail(Message, user.EmailAddress);

                if (status.Equals("Error"))
                {
                    ModelState.AddModelError("Failed_Mail", "Something went wrong try again");
                    return View(obj);
                }
                ViewBag.Message = "Please check your email for verification ";
                return View();
            }
            return View(obj);

        }

        public IActionResult ForgetPassword(String Token)
        {
            var entry = GetVaildationToken(Token);
            if (entry == null || entry.ExpiryTime <= DateTime.UtcNow)
            {
                ViewBag.Message = "Link has Expried";
                return View();
            }
            return View(new PasswordDetailsDTO { Token = Token });
        }

        [HttpPost]
        public IActionResult ForgetPassword(PasswordDetailsDTO obj)
        {
            if (obj == null || obj.Password != obj.ConfirmPassword || obj.Password == null || obj.Token == null)
            {
                ModelState.AddModelError("InvalidPassword", "Password and confirm Password doesn't match");
                return View(obj);
            }
            var entry = GetVaildationToken(obj.Token);
            if (entry == null)
            {
                ViewBag.Message = "Link has Expried";
                return View(obj);
            }
            var hashPassword = BCrypt.Net.BCrypt.HashPassword(obj.Password);
            var PasswordEntry =db.PasswordDetails.FirstOrDefault(x => x.UserId == entry.UserId);
            if (PasswordEntry == null)
            {
                return NotFound();
            }
            PasswordEntry.Password = hashPassword;
            db.VaildationTokens.Remove(entry);
            db.SaveChanges();
            return RedirectToAction("Success");
        }

        private string CreateToken(UserDetails user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name ,user.EmailAddress),
                new Claim(ClaimTypes.NameIdentifier ,user.UserID.ToString()),
            };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(configuration.GetValue<String>("AppSettings:Token")!));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512);

            var tokenDescriptor = new JwtSecurityToken(
                issuer: configuration.GetValue<string>("AppSettings:Issuer"),
                audience: configuration.GetValue<string>("AppSettings:Audience"),
                claims: claims,
                expires: DateTime.UtcNow.AddDays(1),
                signingCredentials: creds
                );

            return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
        }

    }
}
