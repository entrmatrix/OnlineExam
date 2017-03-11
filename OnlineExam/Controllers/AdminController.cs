using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using OnlineExam.Models;
using OnlineExam.Services;

using System.Collections.Generic;
using PagedList;

namespace OnlineExam.Controllers
{
    [Authorize(Roles = "Boss,Admin")]
    public class AdminController : Controller
    {
        private ApplicationDbContext context;
        private ExamService examService;

        private ApplicationUserManager _userManager;
        private ApplicationRoleManager _roleManager;

        public AdminController()
        {
            this.context = new ApplicationDbContext();
            this.examService = new ExamService(context);
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

        bool CanDelete(ApplicationUser user)
        {
            //Boss (老闆)不能刪除
            if (UserManager.IsInRole(user.Id, "Boss")) return false;

            //Admin(管理者)只有老闆有權刪除
            if (UserManager.IsInRole(user.Id, "Admin"))
            {
                var currentUserId = User.Identity.GetUserId();
                if (!UserManager.IsInRole(currentUserId, "Boss")) return false;
            }

            return true;

        }

        bool CanUpdateRole
        {
            get
            {
                var currentUserId = User.Identity.GetUserId();
                return UserManager.IsInRole(currentUserId, "Boss");

            }
        }
            
      
        

        public ActionResult Users(int? page , string keyword="")
        {
            var model = new UserSearchModel();
            model.PageNumber = page ?? 1;
            model.Keyword = keyword;

            return Users(model);

        }
       
        [HttpPost]
        public ActionResult Users(UserSearchModel model)
        {
            string keyword = model.Keyword;

            model.CanUpdateRole = this.CanUpdateRole;

            if (model.PageNumber < 1) model.PageNumber = 1;
            model.PageSize = 1;

            var users = UserManager.Users;

            if (!String.IsNullOrEmpty(keyword))
            {
                users = users.Where(u => u.Name.Contains(keyword) || u.Email.Contains(keyword));
                                           
            }

            model.TotalCount = users.Count();

            if (model.TotalCount == 0)
            {
                if (Request.IsAjaxRequest())
                {
                    return PartialView("Users", model);
                }

                return View("Users", model);
            }

            users = users.OrderByDescending(u => u.CreateDate);

            int maxPage = users.ToList().MaxPage(model.PageSize);
            if (model.PageNumber > maxPage) model.PageNumber = maxPage;

            model.PagedUsers = users.ToPagedList(model.PageNumber, model.PageSize);

            if (model.PageNumber > model.PagedUsers.PageCount)
            {
                model.PageNumber = model.PagedUsers.PageCount;
            }


            var viewModelList = new List<UserViewModel>();
            foreach (var user in model.PagedUsers)
            {
                var viewModel = new UserViewModel(user);

                viewModel.Boss = UserManager.IsInRole(user.Id, "Boss");

                viewModel.Admin = UserManager.IsInRole(user.Id, "Admin");

                viewModelList.Add(viewModel);
            }

            model.ViewModelList = viewModelList;

            if (Request.IsAjaxRequest())
            {
                return PartialView("Users" , model);
            }

            return View("Users", model);

        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteUser(UserSearchModel model)
        {

            var id = model.UserId;
            var user = UserManager.FindById(id);
            if (user == null) throw new Exception("");          

            if (!CanDelete(user))
            {
                return Json(new { Success = false, Msg = "您的權限不足" }, JsonRequestBehavior.AllowGet);
            }

            //清除User所有測驗
            user.Exams.Clear();

            var result= UserManager.Delete(user);
            if (!result.Succeeded) throw new Exception("");

            //刪除孤兒測驗
            examService.DeleteNonUserExamRecords();

            return Json(new { Success = true, Msg = "" }, JsonRequestBehavior.AllowGet);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateUserRole()
        {
            if (!CanUpdateRole) throw new Exception();

            var id = Request["id"];
            bool add = Convert.ToBoolean(Request["add"]);

            IdentityResult result;

            if (add)
            {
                result= UserManager.AddToRole(id, "Admin");
            }
            else
            {
                result= UserManager.RemoveFromRole(id, "Admin");
            }

            if (!result.Succeeded)
            {
                return Json(new { Success = false, Msg = "更新角色失敗" }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { Success = true, Msg = "" }, JsonRequestBehavior.AllowGet);

        }




        public ActionResult AddAdmin()
        {
            var john = UserManager.FindByEmail("john@gmail.com");

            UserManager.AddToRole(john.Id, "Admin");

            return Content("Done");
            
        }


        public ActionResult SeedUsers()
        {
            var users = new List<ApplicationUser>();

            var john = new ApplicationUser
            {
                Name="John",
                Email="john@gmail.com",
                UserName = "john@gmail.com",
                CreateDate =new DateTime(2017,2,24),
                Gender=true,
            };

          


            users.Add(john);

            var johnny = new ApplicationUser
            {
                Name = "johnny",
                Email = "johnny@gmail.com",
                UserName = "johnny@gmail.com",
                CreateDate = new DateTime(2017, 3, 3),
                Gender = true,
            };

            users.Add(johnny);


            var carry = new ApplicationUser
            {
                Name = "Marry",
                Email = "carry@gmail.com",
                UserName = "carry@gmail.com",
                CreateDate = new DateTime(2017, 2, 11),
                Gender = true,
            };

            users.Add(carry);

            var mike = new ApplicationUser
            {
                Name = "Mike",
                Email = "mike@gmail.com",
                UserName = "mike@gmail.com",
                CreateDate = new DateTime(2017, 1, 29),
                Gender = true,
            };

            users.Add(mike);

            foreach (var user in users)
            {
                var exist = UserManager.FindByEmail(user.Email);
                if (exist == null)
                {
                    UserManager.Create(user);
                }
            }

            john = UserManager.FindByEmail("john@gmail.com");
            john.Exams = new List<Exam>();

            john.Exams.Add(new Exam() { ExamDate = DateTime.Now, Title = "test" });
            john.Exams.Add(new Exam() { ExamDate = DateTime.Now, Title = "test1" });
            john.Exams.Add(new Exam() { ExamDate = DateTime.Now, Title = "test2" });

            UserManager.Update(john);




            return Content("Done");

        }

    }
}