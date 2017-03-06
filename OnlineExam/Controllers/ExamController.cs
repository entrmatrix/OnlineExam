using OnlineExam.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Xml;

namespace OnlineExam.Controllers
{

    public class ExamController : Controller
    {

        private ApplicationDbContext context = new ApplicationDbContext();

        // GET: Exam
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult New()
        {
            //設定測驗有多少題目
            int questionsCount = 10;

            using (var context = new ApplicationDbContext())
            {

                //建立新測驗

                var exam = new Exam()
                {
                    Title = "新測驗",
                    ExamDate = DateTime.Now,
                    ExamQuestions = new List<ExamQuestion>()
                };


                string[] keys = { "A", "B", "C", "D" };


                //向題庫隨機取得10題
                Random rnd = new Random();
                var questions = context.Questions.ToList().OrderBy(q => rnd.Next()).Take(questionsCount).ToList();

                for (int i = 0; i < questions.Count; i++)
                {
                    var examQuestion = new ExamQuestion
                    {
                        //題目內容
                        Content = questions[i].Content,

                        //記錄正確答案記錄
                        CorrectAnswer = questions[i].Answer,

                        //加上題號
                        Order = i + 1,
                    };

                    var questionOptions = new List<ExamQuestionOption>();

                    questionOptions.Add(new ExamQuestionOption
                    {
                        //這是正確的選項
                        Text = examQuestion.CorrectAnswer
                    });

                    foreach (var item in questions[i].Options)
                    {
                        var examOption = new ExamQuestionOption
                        {
                            Text = item.Text
                        };

                        // 加入其他錯誤的選項
                        questionOptions.Add(examOption);
                    }

                    //將選項重新隨機排列
                    questionOptions = questionOptions.OrderBy(x => rnd.Next()).ToList();

                    for (int j = 0; j < questionOptions.Count; j++)
                    {
                        // 幫選項加上"Key" = A or B or C
                        questionOptions[j].Key = keys[j];
                    }

                    examQuestion.Options = questionOptions;

                    //記錄題目的正確選項
                    examQuestion.CorrectOption = examQuestion.Options
                                                    .Where(o => o.Text == examQuestion.CorrectAnswer)
                                                    .FirstOrDefault().Key;


                    exam.ExamQuestions.Add(examQuestion);

                }

                //將測驗儲存於資料庫
                context.Exams.Add(exam);
                context.SaveChanges();


                //將每一題的正確答案、正確選項隱藏
                foreach (var question in exam.ExamQuestions)
                {
                    question.CorrectAnswer = "";
                    question.CorrectOption = "";
                }


                //將測驗傳給使用者
                return View(exam);

            }


        }

        [HttpPost]
        public ActionResult Store()
        {
            int id = Convert.ToInt32(Request["Id"]);


            var exam = context.Exams.Find(id);

            int questionsCount = exam.ExamQuestions.Count;
            var questionPoint = 100 / questionsCount;

            for (int i = 1; i <= questionsCount; i++)
            {
                var question = exam.ExamQuestions.Where(q => q.Order == i).FirstOrDefault();

                string key = String.Format("answer-{0}", i);
                var userAnswer = Request[key];

                question.UserAnswer = userAnswer;

                if (userAnswer == question.CorrectOption) question.Points = questionPoint;
                else question.Points = 0;
            }

            exam.Score = exam.ExamQuestions.Sum(q => q.Points);

            context.SaveChanges();

            return View("New", exam);




        }


    }

}