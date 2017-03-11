using OnlineExam.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Xml;
using PagedList;
using OnlineExam.Services;
using Microsoft.AspNet.Identity;

namespace OnlineExam.Controllers
{
    [Authorize]
    public class ExamController : Controller
    {

        private ApplicationDbContext context ;
        private ExamService examService ;

        public ExamController()
        {
            this.context = new ApplicationDbContext();
            this.examService = new ExamService(context);
        }

        string UserId
        {
            get
            {
                return User.Identity.GetUserId();
            }

        }


        // GET: Exam
        public ActionResult Index(int? page)
        {
            var records = examService.ExamRecords(UserId);

            //檢查是否無資料
            if (records.IsNullOrEmpty())
            {
                ViewBag.Count = 0;
                ViewBag.Average = -1;

                return View();
            }

            ViewBag.Count = records.Count();

            var average= records.Average(e => e.Score);

            ViewBag.Average = Math.Round((double)average, 2, MidpointRounding.AwayFromZero);

            int pageSize = 5;
            int pageNumber = (page ??  1 );

            var model = records.ToPagedList(pageNumber, pageSize);

            return View(model);
        }

        public ActionResult New()
        {
            //設定測驗有多少題目
            int questionsCount = 10;
            
            //從Service取得新測驗          
            var exam = examService.NewExam(questionsCount, UserId);

            //將測驗傳給使用者
            return View(exam);

        }

        [HttpPost]
        public ActionResult Store()
        {
            var exam = examService.StoreExam(Request);

            //將測驗結果回傳給使用者

            return PartialView("New", exam);


        }

        public ActionResult Details(int id)
        {
            var model = examService.GetExam(id);
            if (model == null) return HttpNotFound();
            if(model.Score==null) return HttpNotFound();

            return View(model);
        }


    }

}