using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vera.Migrations
{
    /// <inheritdoc />
    public partial class AddIban : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "IBAN",
                table: "Users",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Skills",
                table: "Users",
                type: "TEXT",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "IBAN", "Password", "Skills" },
                values: new object[] { null, "AQAAAAIAAYagAAAAELggRWGw0Y5I3Qz4nqcZrX+yrTnAkRpu40a3YShF3ehPI55Ql/if1x79o5TavR+bTg==", "C#,.NET Core,Entity Framework,SQL Server,Docker,REST API" });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "IBAN", "Password", "Skills" },
                values: new object[] { null, "AQAAAAIAAYagAAAAEFO22SnDn9nTgM2/bi1t5L7ClLM43Qoq7+LaE6qbz9p/sLVTawy/iP2UMsAStzYiRg==", "Figma,Adobe XD,Illustrator,Photoshop,UI/UX,Wireframing" });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "IBAN", "Password", "Skills" },
                values: new object[] { null, "AQAAAAIAAYagAAAAEMW5P74VHokV9plAI/YTo4zW+ygGHoPeP/xxOdZF7PG8/ShcRtpw4djlXMQ8nfp9bw==", "React,Next.js,Node.js,MongoDB,TypeScript,GraphQL" });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "IBAN", "Password", "Skills" },
                values: new object[] { null, "AQAAAAIAAYagAAAAEHVCsnUxinidhJESKxAoISYv7S5d+n6O4Om91Xh40tKDiA1W2ev7jBACgHsjcq7wJA==", null });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "IBAN", "Password", "Skills" },
                values: new object[] { null, "AQAAAAIAAYagAAAAEO4DC/d4UENc52xjzvMVF4ncFaxD/SDONaUVBxKCLIkClx9Gukk5GHbyxaDmQYoYOg==", null });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "IBAN", "Password", "Skills" },
                values: new object[] { null, "AQAAAAIAAYagAAAAECwT89g/mqcSGzG+14LIGyZ97cfOHGb+/7HFN6dP0EvqTcE7avqS9ucfuM8Fe+uJIg==", null });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "IBAN", "Password", "Skills" },
                values: new object[] { null, "AQAAAAIAAYagAAAAEJAa1y+XAvkSwxhDGuOG5d0yavwv4esglLAKKIpRQJs3nOqrTXNne70qUPCXm/9XeQ==", "Flutter,Dart,Firebase,REST API,iOS,Android" });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "IBAN", "Password", "Skills" },
                values: new object[] { null, "AQAAAAIAAYagAAAAEDUWyPtxgzZt6HGZMvqB1o0rThVNvKnpdLx5Quy2Ohyjgi5nYwBgABzrS9pQ3cjnwQ==", "AWS,Azure,Docker,Kubernetes,CI/CD,Linux" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IBAN",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Skills",
                table: "Users");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "Password",
                value: "AQAAAAIAAYagAAAAEFIGkRyQ1hytcCmuXF/MnvwHMBjUfpOcGMWjpKXQfau2Cwc/0jvWwqzzmVDF2FVWKg==");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                column: "Password",
                value: "AQAAAAIAAYagAAAAEJa+N/WKPy4ij6yO9KyxUaJduuWDQOb9qiB/XpaSOdT+ofSal0HcQa69UtzMKTn/Fw==");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 3,
                column: "Password",
                value: "AQAAAAIAAYagAAAAEKSF/ZTgpU0Y00W/gJr1KK5SHNpsHA5EepCXB6XeEHi/tf70xqx78GWNH2UHtF6K5g==");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 4,
                column: "Password",
                value: "AQAAAAIAAYagAAAAEI0IMmlo7rrNW94vWsqAEEURRjCD2GjMrLXnKmlg3+ugXJORaXWfwu6ST9fUEl+Uaw==");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 5,
                column: "Password",
                value: "AQAAAAIAAYagAAAAEL0BkZ8Z+sPlZQUOSq/l8V9w5HNyCY/HKx/1P7dC6kX+b7Vf8Zmlfad+jX718Cuz/Q==");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 6,
                column: "Password",
                value: "AQAAAAIAAYagAAAAEKIwG+TblxK5YP22eEYsQpt4x2fjXPC9FO5y+1pBsyvxxnXSIGT2hLibHyj3iLRqLA==");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 7,
                column: "Password",
                value: "AQAAAAIAAYagAAAAEFA8yFaIjx/WPUknQEoFqLDXfBWwwOrltr4uJpFDrdSqMMVXrhPkJFniHuEeE7nmQQ==");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 8,
                column: "Password",
                value: "AQAAAAIAAYagAAAAED90Jj1V3RIEblseuxAg9tipjxo9tJII3BRBIe3EqsSJg3a18V1JoBMrsFXqN3zekw==");
        }
    }
}
