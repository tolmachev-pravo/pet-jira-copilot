using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pet.Jira.Infrastructure.Migrations
{
    public partial class Add_Unique_Index_To_Users_Username : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
CREATE TEMP TABLE __UserDedup AS
SELECT duplicateUser.Id AS DuplicateUserId,
       keeperUser.Id AS KeeperUserId
FROM Users duplicateUser
JOIN Users keeperUser
    ON keeperUser.Username = duplicateUser.Username
WHERE duplicateUser.rowid <> keeperUser.rowid
  AND keeperUser.rowid = (
      SELECT MIN(candidateUser.rowid)
      FROM Users candidateUser
      WHERE candidateUser.Username = duplicateUser.Username
  );");

            migrationBuilder.Sql(@"
UPDATE UserNotifications
SET UserId = (
    SELECT KeeperUserId
    FROM __UserDedup
    WHERE DuplicateUserId = UserNotifications.UserId
)
WHERE EXISTS (
    SELECT 1
    FROM __UserDedup
    WHERE DuplicateUserId = UserNotifications.UserId
);");

            migrationBuilder.Sql(@"
DELETE FROM UserCalendarConnections
WHERE Id IN (
    SELECT duplicateConnection.Id
    FROM UserCalendarConnections duplicateConnection
    JOIN __UserDedup duplicateMap
        ON duplicateMap.DuplicateUserId = duplicateConnection.UserId
    WHERE EXISTS (
        SELECT 1
        FROM UserCalendarConnections keeperConnection
        WHERE keeperConnection.UserId = duplicateMap.KeeperUserId
          AND keeperConnection.Provider = duplicateConnection.Provider
    )
);");

            migrationBuilder.Sql(@"
UPDATE UserCalendarConnections
SET UserId = (
    SELECT KeeperUserId
    FROM __UserDedup
    WHERE DuplicateUserId = UserCalendarConnections.UserId
)
WHERE EXISTS (
    SELECT 1
    FROM __UserDedup
    WHERE DuplicateUserId = UserCalendarConnections.UserId
);");

            migrationBuilder.Sql(@"
DELETE FROM Users
WHERE Id IN (
    SELECT DuplicateUserId
    FROM __UserDedup
);");

            migrationBuilder.Sql("DROP TABLE __UserDedup;");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Username",
                table: "Users",
                column: "Username",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_Username",
                table: "Users");
        }
    }
}
