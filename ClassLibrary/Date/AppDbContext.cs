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

            // Document_resource -> Document (many-to-one), без коллекции у Document
            modelBuilder.Entity<Document_resource>()
                .HasOne(dr => dr.Document)
                .WithMany() // у Document нет навигации-коллекции
                .OnDelete(DeleteBehavior.Cascade); // при удалении документа удалятся строки

            //modelBuilder.Entity<CurrencyTick>()
            //    .HasIndex(ct => new { ct.Pair, ct.Timestamp });

            //modelBuilder.Entity<CurrencyPair>()
            //    .HasIndex(cp => cp.Symbol)
            //    .IsUnique();
            modelBuilder.Entity<Condition>().HasData(
                   new Condition { Id = 1, Name = "Подписан",    Code = "Подписан" },

                   new Condition { Id = 2, Name = "Отозван",     Code = "Отозван" },
                   new Condition { Id = 3, Name = "Не подписан", Code = "Не подписан" },
                   new Condition { Id = 4, Name = "В наличии",   Code = "В наличии" },
                   new Condition { Id = 5, Name = "Закончился",  Code = "Закончился" },
                   new Condition { Id = 6, Name = "Закупка",     Code = "Закупка" },
                   new Condition { Id = 7, Name = "Активный",    Code = "Активный" },
                   new Condition { Id = 8, Name = "Новый",       Code = "Новый" },
                   new Condition { Id = 9, Name = "Готов",       Code = "Готов" },
                   new Condition { Id = 10, Name = "Архив",      Code = "Архив" }
            );
            modelBuilder.Entity<TypeDoc>().HasData(
                   new TypeDoc { Id = 1, Name = "Поступление", Code = "Поступление" },
                   new TypeDoc { Id = 2, Name = "Отгрузка",    Code = "Отгрузка" }
            );

            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        }
    }
}
