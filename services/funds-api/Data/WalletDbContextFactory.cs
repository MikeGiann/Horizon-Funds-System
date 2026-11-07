using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using FundsApi.Data;

namespace FundsApi.Data
{
    public class WalletDbContextFactory : IDesignTimeDbContextFactory<WalletDbContext>
    {
        //Database connection
        public WalletDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<WalletDbContext>();
            
            optionsBuilder.UseSqlServer("Server=localhost,1433;Database=WalletFundsDB;User Id=sa;Password=Batman751995!@#;TrustServerCertificate=True;");

            return new WalletDbContext(optionsBuilder.Options);
        }
    }
}