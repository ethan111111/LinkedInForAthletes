﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace recruitingWebApp.Migrations
{
    /// <inheritdoc />
    public partial class postcontenttypecol : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ContentType",
                table: "Posts",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ContentType",
                table: "Posts");
        }
    }
}
