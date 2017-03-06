namespace OnlineExam.Migrations
{
    using Models;
    using System;
    using System.Collections.Generic;
    using System.Data.Entity.Migrations;
    using System.Linq;
    using System.Xml;

    internal sealed class Configuration : DbMigrationsConfiguration<OnlineExam.Models.ApplicationDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(OnlineExam.Models.ApplicationDbContext context)
        {
           
            CreateQuestions(context);


            //  This method will be called after migrating to the latest version.

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method 
            //  to avoid creating duplicate seed data. E.g.
            //
            //    context.People.AddOrUpdate(
            //      p => p.FullName,
            //      new Person { FullName = "Andrew Peters" },
            //      new Person { FullName = "Brice Lambson" },
            //      new Person { FullName = "Rowan Miller" }
            //    );
            //
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
        void AddQuestions(XmlNode node , OnlineExam.Models.ApplicationDbContext context)
        {
            var nodeContent = node.SelectSingleNode("Content");
            string content = nodeContent.InnerText;

            var exsit = context.Questions.Where(q => q.Content == content).FirstOrDefault();
            if (exsit != null) return;

            var nodeAnswer = node.SelectSingleNode("Answer");
            string answer = nodeAnswer.InnerText;
            if (String.IsNullOrEmpty(answer)) return;

          var options = GetOptions(node,context);

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
