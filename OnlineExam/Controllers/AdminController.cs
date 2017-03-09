using Microsoft.AspNet.Identity.Owin;
using OnlineExam.Models;
using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace OnlineExam.Controllers
{
    [Authorize(Roles = "Boss,Admin")]
    public class AdminController : Controller
    {

        private ApplicationUserManager _userManager;
        private ApplicationRoleManager _roleManager;

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


        public async Task<ActionResult> Users(int? page)
        {
            var model = new UserSearchModel();

            var users = UserManager.Users;

            model.TotalCount = users.Count();

            int pageSize = 5;
            int pageNumber = (page ?? 1);

            model.PagedUsers = users.OrderBy(u=>u.Name).ToPagedList(pageNumber, pageSize);

            var viewModelList = new List<UserViewModel>();
            foreach (var user in model.PagedUsers)
            {
                var viewModel = new UserViewModel(user);

                viewModel.Boss = await UserManager.IsInRoleAsync(user.Id, "Boss");

                viewModel.Admin = await UserManager.IsInRoleAsync(user.Id, "Admin");

                viewModelList.Add(viewModel);
            }

            model.ViewModelList = viewModelList;



            return View(model);

        }
    }
}