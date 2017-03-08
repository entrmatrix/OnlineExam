namespace OnlineExam.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _20170308_2 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Exams", "UserId", c => c.String(maxLength: 128));
            CreateIndex("dbo.Exams", "UserId");
            AddForeignKey("dbo.Exams", "UserId", "dbo.AspNetUsers", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Exams", "UserId", "dbo.AspNetUsers");
            DropIndex("dbo.Exams", new[] { "UserId" });
            DropColumn("dbo.Exams", "UserId");
        }
    }
}
