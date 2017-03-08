namespace OnlineExam.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _20170306_6 : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.ExamQuestions", "Exam_Id", "dbo.Exams");
            DropIndex("dbo.ExamQuestions", new[] { "Exam_Id" });
            RenameColumn(table: "dbo.ExamQuestions", name: "Exam_Id", newName: "ExamId");
            AlterColumn("dbo.ExamQuestions", "ExamId", c => c.Int(nullable: false));
            CreateIndex("dbo.ExamQuestions", "ExamId");
            AddForeignKey("dbo.ExamQuestions", "ExamId", "dbo.Exams", "Id", cascadeDelete: true);
            DropColumn("dbo.ExamQuestions", "ExanId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.ExamQuestions", "ExanId", c => c.Int(nullable: false));
            DropForeignKey("dbo.ExamQuestions", "ExamId", "dbo.Exams");
            DropIndex("dbo.ExamQuestions", new[] { "ExamId" });
            AlterColumn("dbo.ExamQuestions", "ExamId", c => c.Int());
            RenameColumn(table: "dbo.ExamQuestions", name: "ExamId", newName: "Exam_Id");
            CreateIndex("dbo.ExamQuestions", "Exam_Id");
            AddForeignKey("dbo.ExamQuestions", "Exam_Id", "dbo.Exams", "Id");
        }
    }
}
