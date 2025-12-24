using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GCTWeb.Migrations
{
    /// <inheritdoc />
    public partial class AddDistributedCacheTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS AppDistributedCache (
                    Id VARCHAR(449) PRIMARY KEY,
                    Value LONGBLOB NOT NULL,
                    ExpiresAtTime DATETIME(6) NOT NULL,
                    SlidingExpirationInSeconds BIGINT NULL,
                    AbsoluteExpiration DATETIME(6) NULL,
                    INDEX Index_ExpiresAtTime (ExpiresAtTime)
                ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP TABLE IF EXISTS AppDistributedCache;");
        }
    }
}
