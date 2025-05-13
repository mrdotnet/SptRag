using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using SptRag.Admin.Client;

namespace SptRag.Admin.Server.Data
{
    public partial class SptRagDbContext : DbContext
    {
        public SptRagDbContext()
        {
        }

        public SptRagDbContext(DbContextOptions<SptRagDbContext> options) : base(options)
        {
        }

        partial void OnModelBuilding(ModelBuilder builder);

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            this.OnModelBuilding(builder);
        }

        public DbSet<Client.Document> Documents { get; set; }

        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            configurationBuilder.Conventions.Add(_ => new BlankTriggerAddingConvention());
        }
    }
}