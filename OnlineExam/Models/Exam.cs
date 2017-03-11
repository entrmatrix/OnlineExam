using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace OnlineExam.Models
{
    public class Exam
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public DateTime ExamDate { get; set; }
        public double? Score { get; set; }

       
        public string UserId { get; set; }

        public string Name { get; set; }

        public virtual ICollection<ExamQuestion> ExamQuestions { get; set; }

       
        public virtual ApplicationUser User { get; set; }

    }

}