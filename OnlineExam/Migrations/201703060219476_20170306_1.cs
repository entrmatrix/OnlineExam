namespace OnlineExam.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _20170306_1 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ExamQuestionOptions",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Text = c.String(),
                        Key = c.String(),
                        QuestionId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ExamQuestions", t => t.QuestionId, cascadeDelete: true)
                .Index(t => t.QuestionId);
            
            CreateTable(
                "dbo.ExamQuestions",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ExanId = c.Int(nullable: false),
                        Order = c.Int(nullable: false),
                        Content = c.String(),
                        CorrectAnswer = c.String(),
                        CorrectOption = c.String(),
                        Points = c.Double(),
                        Exam_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Exams", t => t.Exam_Id)
                .Index(t => t.Exam_Id);
            
            CreateTable(
                "dbo.Exams",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Title = c.String(),
                        ExamDate = c.DateTime(nullable: false),
                        Score = c.Double(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Options",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Text = c.String(),
                        QuestionId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Questions", t => t.QuestionId, cascadeDelete: true)
                .Index(t => t.QuestionId);
            
            CreateTable(
                "dbo.Questions",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Content = c.String(),
                        Answer = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Options", "QuestionId", "dbo.Questions");
            DropForeignKey("dbo.ExamQuestionOptions", "QuestionId", "dbo.ExamQuestions");
            DropForeignKey("dbo.ExamQuestions", "Exam_Id", "dbo.Exams");
            DropIndex("dbo.Options", new[] { "QuestionId" });
            DropIndex("dbo.ExamQuestions", new[] { "Exam_Id" });
            DropIndex("dbo.ExamQuestionOptions", new[] { "QuestionId" });
            DropTable("dbo.Questions");
            DropTable("dbo.Options");
            DropTable("dbo.Exams");
            DropTable("dbo.ExamQuestions");
            DropTable("dbo.ExamQuestionOptions");
        }
    }
}
