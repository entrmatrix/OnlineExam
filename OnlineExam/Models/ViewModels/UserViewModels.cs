using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PagedList;

namespace OnlineExam.Models
{
    public class UserSearchModel       
    { 
        public string Keyword { get; set; }

        public int TotalCount { get; set; }

        public int PageNumber { get; set; }

        public int PageSize { get; set; }

        public string UserId { get; set; }

        public bool CanUpdateRole { get; set; }

        public IPagedList<ApplicationUser> PagedUsers { get; set; }

        public IList<UserViewModel> ViewModelList { get; set; }
    }


    public class UserViewModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public DateTime  CreateDate { get; set; }

        public bool Boss { get; set; }
        public bool Admin { get; set; }

        public UserViewModel(ApplicationUser user)
        {
            Id = user.Id;
            Name = user.Name;
            Email = user.Email;
            CreateDate = user.CreateDate;
        }
    }
}