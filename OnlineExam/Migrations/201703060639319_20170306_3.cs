namespace OnlineExam.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _20170306_3 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ExamQuestions", "UserAnswer", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.ExamQuestions", "UserAnswer");
        }
    }
}
