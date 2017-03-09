using System;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using OnlineExam.Models;
using OnlineExam.Services;
using System.Configuration;
using SendGrid;
using SendGrid.Helpers.Mail;
using Microsoft.AspNet.Identity.EntityFramework;

namespace OnlineExam.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        private ApplicationRoleManager _roleManager;

        private SendGridEmailService emailService;

        public AccountController()
        {
            string sendGridApiKey = ConfigurationManager.AppSettings["sendGridApiKey"];
            string sender = "service@online-exam.com";
            string senderName = ConfigurationManager.AppSettings["siteName"];
            this.emailService = new SendGridEmailService(sendGridApiKey, sender, senderName);
        }

        public AccountController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set
            {
                _signInManager = value;
            }
        }

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

        public ApplicationRoleManager RoleManager
        {
            get
            {
                return _roleManager ?? HttpContext.GetOwinContext().Get<ApplicationRoleManager>();
            }
            private set
            {
                _roleManager = value;
            }
        }


        //
        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            //RoleManager.Create(new IdentityRole { Name = "Boss" });

            //RoleManager.CreateRole("Boss");
            //RoleManager.CreateRole("Admin");


            var user = UserManager.FindByEmail("traders.com.tw@gmail.com");

            





            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        //
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid) return View(model);

            // 這不會計算為帳戶鎖定的登入失敗
            // 若要啟用密碼失敗來觸發帳戶鎖定，請變更為 shouldLockout: true
            var result = await SignInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, shouldLockout: false);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(returnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:

                    //需要Email驗證
                    var user = await UserManager.FindByNameAsync(model.Email);
                    return View("NeedRegisterConfirm", user);

                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "登入嘗試失試。");
                    return View(model);
            }
        }

       

        //
        // GET: /Account/Register
        [AllowAnonymous]
        public ActionResult Register()
        {
            var model = new RegisterViewModel();
            model.Gender = true;
            return View(model);
        }

        //
        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
                user.Name = model.Name;
                user.Gender = model.Gender;
                user.CreateDate = DateTime.Now;

                var result = await UserManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    //發送Email認證信
                    await SendRegisterConfirmEmail(user);

                    return View("RegisterInstruction", user);
                }
                AddErrors(result);
            }

            // 如果執行到這裡，發生某項失敗，則重新顯示表單
            return View(model);
        }

        async Task SendRegisterConfirmEmail(ApplicationUser user)
        {
            string siteName = ConfigurationManager.AppSettings["siteName"];
            string title = String.Format("{0} 會員註冊Email認證信", siteName);
            string subject = title;
            string nickName = user.Name;

            string emailTemplate = System.IO.File.ReadAllText(Server.MapPath("~/Views/Shared/EmailTemplates/RegisterMail.html"));

            string userId = user.Id;
            string code = UserManager.GenerateEmailConfirmationToken(userId);
            var validateUrl = Url.Action("RegisterConfirm", "Account", new { user = userId, code = code }, protocol: Request.Url.Scheme);

            string htmlBody = emailTemplate.Replace("{{TitleText}}", title).Replace("{{UserName}}", nickName)
                                                        .Replace("{{ValidateUrl}}", validateUrl);

            string textTemplate = System.IO.File.ReadAllText(Server.MapPath("~/Views/Shared/EmailTemplates/RegisterMailText.txt"));

            string textBody = textTemplate.Replace("{{TitleText}}", title).Replace("{{UserName}}", nickName)
                                                        .Replace("{{ValidateUrl}}", validateUrl);

            string to = user.Email;

            await emailService.SendAsync(subject, to, htmlBody, textBody);
        }

        [AllowAnonymous]
        public async Task<ActionResult> RegisterConfirm(string user, string code)
        {
            string userId = user;

            var newUser = await UserManager.FindByIdAsync(userId);
            if (newUser == null)
            {
                return HttpNotFound();
            }

            if (!newUser.EmailConfirmed)
            {
                await UserManager.ConfirmEmailAsync(userId, code);
            }

            return View(newUser);

        }


        //使用者申請重發驗證信
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<ActionResult> SendRegisterConfirmMail(ApplicationUser model)
        {
            var user = UserManager.FindById(model.Id);

            //發送email驗證信
            await SendRegisterConfirmEmail(user);

            return View("RegisterInstruction", user);
        }
        //
        // GET: /Account/ForgotPassword
        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        //
        // POST: /Account/ForgotPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await UserManager.FindByNameAsync(model.Email);
                if (user == null || !(await UserManager.IsEmailConfirmedAsync(user.Id)))
                {
                    user = new ApplicationUser()
                    {
                        Email = model.Email,
                        Name = ""
                    };
                   
                    // 不顯示使用者不存在或未受確認
                    return View("ForgotPasswordInstruction", user);
                }

                await SendResetPasswordEmail(user);

                return View("ForgotPasswordInstruction", user);
              
            }

            // 如果執行到這裡，發生某項失敗，則重新顯示表單
            return View(model);
        }

        private async Task SendResetPasswordEmail(ApplicationUser user)
        {
            string siteName = ConfigurationManager.AppSettings["siteName"];
            
            string subject = String.Format("{0} 忘記密碼回函", siteName);
            string title = String.Format("{0} 忘記密碼小幫手", siteName);
            string nickName = user.Name;

            string userId = user.Id;
            string code = await UserManager.GeneratePasswordResetTokenAsync(userId);
            var validateUrl = Url.Action("ResetPassword", "Account", new { user = userId, code = code }, protocol: Request.Url.Scheme);

            string mailHtmlTemplate = System.IO.File.ReadAllText(Server.MapPath("~/Views/Shared/EmailTemplates/PasswordMail.html"));

            string htmlBody = mailHtmlTemplate.Replace("{{TitleText}}", title).Replace("{{UserName}}", nickName)
                                                        .Replace("{{ValidateUrl}}", validateUrl);

            string textTemplate = System.IO.File.ReadAllText(Server.MapPath("~/Views/Shared/EmailTemplates/PasswordMailText.txt"));

            string textBody = textTemplate.Replace("{{TitleText}}", title).Replace("{{UserName}}", nickName)
                                                        .Replace("{{ValidateUrl}}", validateUrl);

            string to = user.Email;

            await emailService.SendAsync(subject, to, htmlBody, textBody);


        }

        //
        // GET: /Account/ForgotPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        //
        // GET: /Account/ResetPassword
        [AllowAnonymous]
        public async Task<ActionResult>  ResetPassword(string user, string code)
        {
            var theUser = await UserManager.FindByIdAsync(user);
            if (theUser == null) return HttpNotFound();

            var model = new ResetPasswordViewModel()
            {
                Code = code
            };

            return View(model);
           
        }

        //
        // POST: /Account/ResetPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await UserManager.FindByNameAsync(model.Email);
            if (user == null)
            {
                // 不顯示使用者不存在
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            var result = await UserManager.ResetPasswordAsync(user.Id, model.Code, model.Password);
            if (result.Succeeded)
            {
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            AddErrors(result);
            return View();
        }

        //
        // GET: /Account/ResetPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        //
        // POST: /Account/ExternalLogin
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            // 要求重新導向至外部登入提供者
            return new ChallengeResult(provider, Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl }));
        }

       

        //
        // GET: /Account/ExternalLoginCallback
        [AllowAnonymous]
        public async Task<ActionResult> ExternalLoginCallback(string returnUrl)
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();
            if (loginInfo == null)
            {
                return RedirectToAction("Login");
            }

            // 若使用者已經有登入資料，請使用此外部登入提供者登入使用者
            var result = await SignInManager.ExternalSignInAsync(loginInfo, isPersistent: false);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(returnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = false });
                case SignInStatus.Failure:
                default:
                    // 若使用者沒有帳戶，請提示使用者建立帳戶
                    ViewBag.ReturnUrl = returnUrl;
                    ViewBag.LoginProvider = loginInfo.Login.LoginProvider;
                    return View("ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel { Email = loginInfo.Email });
            }
        }

        //
        // POST: /Account/ExternalLoginConfirmation
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model, string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Manage");
            }

            if (ModelState.IsValid)
            {
                // 從外部登入提供者處取得使用者資訊
                var info = await AuthenticationManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    return View("ExternalLoginFailure");
                }
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
                var result = await UserManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    result = await UserManager.AddLoginAsync(user.Id, info.Login);
                    if (result.Succeeded)
                    {
                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                        return RedirectToLocal(returnUrl);
                    }
                }
                AddErrors(result);
            }

            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        //
        // POST: /Account/LogOff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return RedirectToAction("Index", "Home");
        }

        //
        // GET: /Account/ExternalLoginFailure
        [AllowAnonymous]
        public ActionResult ExternalLoginFailure()
        {
            return View();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_userManager != null)
                {
                    _userManager.Dispose();
                    _userManager = null;
                }

                if (_signInManager != null)
                {
                    _signInManager.Dispose();
                    _signInManager = null;
                }
            }

            base.Dispose(disposing);
        }

        #region Helper
        // 新增外部登入時用來當做 XSRF 保護
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }

        internal class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri)
                : this(provider, redirectUri, null)
            {
            }

            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties { RedirectUri = RedirectUri };
                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] = UserId;
                }
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }
        #endregion
    }
}