using Microsoft.EntityFrameworkCore;
using VidaPolicial.Entities;

namespace VidaPolicial
{
    public class DatabaseContext : DbContext
    {
        public DbSet<Banimento> Banimentos { get; set; }
        public DbSet<Log> Logs { get; set; }
        public DbSet<Parametro> Parametros { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseMySql(Global.ConnectionString);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Banimento>().HasKey(x => x.Codigo);
            modelBuilder.Entity<Log>().HasKey(x => x.Codigo);
            modelBuilder.Entity<Log>().Property(x => x.Tipo).HasConversion(typeof(int));
            modelBuilder.Entity<Usuario>().HasKey(x => x.Codigo);
            modelBuilder.Entity<Parametro>().HasKey(x => x.Codigo);
            modelBuilder.Entity<Usuario>().Property(x => x.Staff).HasConversion(typeof(int));
        }
    }
}