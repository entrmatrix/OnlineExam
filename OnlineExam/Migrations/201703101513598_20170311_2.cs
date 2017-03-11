namespace OnlineExam.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _20170311_2 : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Exams", "UserId", "dbo.AspNetUsers");
            DropIndex("dbo.Exams", new[] { "UserId" });
            AlterColumn("dbo.Exams", "UserId", c => c.String(maxLength: 128));
            CreateIndex("dbo.Exams", "UserId");
            AddForeignKey("dbo.Exams", "UserId", "dbo.AspNetUsers", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Exams", "UserId", "dbo.AspNetUsers");
            DropIndex("dbo.Exams", new[] { "UserId" });
            AlterColumn("dbo.Exams", "UserId", c => c.String(nullable: false, maxLength: 128));
            CreateIndex("dbo.Exams", "UserId");
            AddForeignKey("dbo.Exams", "UserId", "dbo.AspNetUsers", "Id", cascadeDelete: true);
        }
    }
}
