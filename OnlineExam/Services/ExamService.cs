using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using OnlineExam.Models;

namespace OnlineExam.Services
{
    public class ExamService
    {
        private ApplicationDbContext context;

        public ExamService(ApplicationDbContext context)
        {
            this.context = context;
        }

        public IEnumerable<Exam> ExamRecords(string userId)
        {
            return context.Exams.Where(e => e.UserId == userId && e.Score.HasValue).OrderByDescending(e => e.ExamDate).ToList();
        }


        public Exam NewExam(int questionsCount, string userId, string title = "新測驗")
        {
            //建立新測驗

            var exam = new Exam()
            {
                UserId = userId,
                Title = title,
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

            return exam;
        }

        public Exam GetExam(int id)
        {
            return context.Exams.Find(id);
        }

        public Exam StoreExam(HttpRequestBase request)
        {
            //從使用者回傳的表單取得資料：測驗的Id
            int id = Convert.ToInt32(request["Id"]);

            //從資料庫取得該筆測驗資料
            var exam = GetExam(id);

            // 測驗的題目數
            int questionsCount = exam.ExamQuestions.Count;

            // 滿分100，計算每一題占多少分. 例如測驗有十題則每題10分
            var questionPoint = 100 / questionsCount;


            //核對每一題作答是否正確，正確該題得10分，錯誤則該題0分
            for (int i = 1; i <= questionsCount; i++)
            {
                //找出題號為i的題目
                var question = exam.ExamQuestions.Where(q => q.Order == i).FirstOrDefault();

                //從表單取得使用者這一題的作答   A or B or C
                string key = String.Format("answer-{0}", i);
                var userAnswer = request[key];

                //紀錄使用者這一題的作答
                question.UserAnswer = userAnswer;

                //如果使用者的答案正確則得分，錯誤則該題0分
                if (userAnswer == question.CorrectOption) question.Points = questionPoint;
                else question.Points = 0;
            }


            //計算測驗總分=每一題得分的加總
            exam.Score = exam.ExamQuestions.Sum(q => q.Points);

            //儲存測驗(更新測驗資料)
            context.SaveChanges();


            return exam;
        }

        public void DeleteNonUserExamRecords()
        {
            var records = context.Exams.Where(e => e.UserId == null).ToList();

            foreach (var item in records)
            {
                context.Exams.Remove(item);
            }

            context.SaveChanges();
        }

    }
}