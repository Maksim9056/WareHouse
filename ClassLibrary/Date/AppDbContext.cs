using ClassLibrary.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary.Date
{
    public  class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
        public DbSet<Resource> Resource { get; set; }
        public DbSet<Unit> Unit { get; set; }
        public DbSet<Client> Client { get; set; }
        public DbSet<Balance> Balance { get; set; }
        public DbSet<Document> Document { get; set; }
        public DbSet<Document_resource> Document_resource { get; set; }
        public DbSet<Condition> Condition { get; set; }
        public DbSet<TypeDoc> TypeDoc { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            //modelBuilder.Entity<CurrencyTick>()
            //    .HasIndex(ct => new { ct.Pair, ct.Timestamp });

            //modelBuilder.Entity<CurrencyPair>()
            //    .HasIndex(cp => cp.Symbol)
            //    .IsUnique();
            modelBuilder.Entity<Condition>().HasData(
                   new Condition { Id = 1, Name = "Подписан" },

                   new Condition { Id = 2, Name = "Отозван" },
                   new Condition { Id = 3, Name = "Не подписан" },
                   new Condition { Id = 4, Name = "В наличии" },
                   new Condition { Id = 5, Name = "Закончился" },
                   new Condition { Id = 6, Name = "Закупка" },
                   new Condition { Id = 7, Name = "Активный" },
                   new Condition { Id = 8, Name = "Новый" },
                   new Condition { Id = 9, Name = "Готов" },
                   new Condition { Id = 10, Name = "Архив" }
            );
            modelBuilder.Entity<TypeDoc>().HasData(
                   new TypeDoc { Id = 1, Name = "Поступление" },
                   new TypeDoc { Id = 2, Name = "Отгрузка" }
            );

            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        }
    }
}
