using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OnlineExam.Models
{
    public class ExamQuestion
    {
        public int Id { get; set; }
        public int ExamId { get; set; }

        public int Order { get; set; }
        public string Content { get; set; }
        public string CorrectAnswer { get; set; }
        public string CorrectOption { get; set; }  // A  or   1

        public string UserAnswer { get; set; }  // A  or   1
        public double? Points { get; set; } //  答錯-> 0 分



        public virtual Exam Exam { get; set; }

        public virtual ICollection<ExamQuestionOption> Options { get; set; }

    }
}