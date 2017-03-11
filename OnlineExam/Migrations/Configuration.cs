namespace OnlineExam.Migrations
{
    using Models;
    using System;
    using System.Collections.Generic;
    using System.Data.Entity.Migrations;
    using System.Linq;
    using System.Xml;
    using Microsoft.AspNet.Identity.EntityFramework;
    using Microsoft.AspNet.Identity;
    using OnlineExam;
    using System.Text;

    internal sealed class Configuration : DbMigrationsConfiguration<OnlineExam.Models.ApplicationDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
            
        }

        protected override void Seed(OnlineExam.Models.ApplicationDbContext context)
        {

            var roleStore = new RoleStore<IdentityRole>(context);
            var roleManager = new ApplicationRoleManager(roleStore);

            string roleName = "Boss";
            var role = roleManager.FindByName(roleName);
            if (role == null) roleManager.CreateRole(roleName);

            roleName = "Admin";
            role = roleManager.FindByName(roleName);
            if (role == null) roleManager.CreateRole(roleName);


            var userStore = new UserStore<ApplicationUser>(context);
            var userManager = new ApplicationUserManager(userStore);

          

            string email = "traders.com.tw@gmail.com";
            var exist = userManager.FindByEmail(email);
            if (exist == null)
            {
                string password = "000000";
                var boss = new ApplicationUser
                {
                    Name="何金水",
                    Email = email,
                    UserName = email,
                    CreateDate = DateTime.Now,
                    Gender = true,
                };
                var result = userManager.Create(boss, password);
                if (result.Succeeded)
                {
                    userManager.AddToRole(boss.Id, "Boss");
                }
            }
            else
            {
                userManager.AddToRole(exist.Id, "Boss");
            }


            CreateQuestions(context);


        }




        void CreateQuestions(OnlineExam.Models.ApplicationDbContext context)
        {
            var doc = new XmlDocument();

            var resourceName = "OnlineExam.App_Data.Questions.xml";
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            var stream = assembly.GetManifestResourceStream(resourceName);


            doc.Load(stream);



            string rootName = "Questions";
            XmlNode root = doc.DocumentElement;

            if (root.Name != rootName) throw new Exception("wrong root Name. root name should be: " + rootName);


            foreach (XmlNode node in root.ChildNodes)
            {
                AddQuestions(node, context);
            }

            context.SaveChanges();

        }
        void AddQuestions(XmlNode node, OnlineExam.Models.ApplicationDbContext context)
        {
            var nodeContent = node.SelectSingleNode("Content");
            string content = nodeContent.InnerText;

            var exsit = context.Questions.Where(q => q.Content == content).FirstOrDefault();
            if (exsit != null) return;

            var nodeAnswer = node.SelectSingleNode("Answer");
            string answer = nodeAnswer.InnerText;
            if (String.IsNullOrEmpty(answer)) return;

            var options = GetOptions(node, context);

            var question = new Question
            {
                Content = content,
                Answer = answer,
                Options = options
            };

            context.Questions.Add(question);

        }

        List<Option> GetOptions(XmlNode node, OnlineExam.Models.ApplicationDbContext context)
        {
            var optionList = new List<Option>();
            var nodeOptions = node.SelectSingleNode("Options");

            foreach (XmlNode optionNode in nodeOptions.ChildNodes)
            {
                var optionText = optionNode.InnerText;
                if (!String.IsNullOrEmpty(optionText))
                {
                    var option = new Option()
                    {
                        Text = optionText
                    };
                    optionList.Add(option);
                }

            }

            return optionList;
        }
    }
}
