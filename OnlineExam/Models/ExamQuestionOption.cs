using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OnlineExam.Models
{
    public class ExamQuestionOption
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public string Key { get; set; }   //  A/B/C


        public int QuestionId { get; set; }
        public virtual ExamQuestion Question { get; set; }

    }
}