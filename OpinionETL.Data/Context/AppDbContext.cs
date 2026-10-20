using Microsoft.EntityFrameworkCore;
using OpinionETL.Core.Models;
using System.Security.Cryptography;

namespace OpinionETL.Data.Context
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
    {
        public DbSet<Category> Category { get; set; }
        public DbSet<Product> Product { get; set; }
        public DbSet<SourceType> Source_Type { get; set; }
        public DbSet<DataSource> Data_Source { get; set; }
        public DbSet<Opinion> Opinion { get; set; }
        public DbSet<Client> Client { get; set; }
        public DbSet<Classification> Classification { get; set; }
        public DbSet<StageOpinion> Stage_Opinion { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}
