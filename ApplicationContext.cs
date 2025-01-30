using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace поиск_недостоверной_ТМ_по_корреляции
{
    internal class ApplicationContext: DbContext
    {
        public DbSet<telemetry> correlation_coefficients { get; set; } = null;

        public DbSet<telemetryValues> telemetry_values { get; set; }
        public DbSet<Slices> slices { get; set; }
        public DbSet<ActivePowerImbalance> active_power_imbalance { get; set; }
        public DbSet<ReactivePowerImbalance> reactive_power_imbalance { get; set; }


        public ApplicationContext()
        {
            Database.EnsureCreated();
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql("Host=localhost;Port = 5432;Database=БД_ИТ_диплом;Username=postgres;Password=HgdMoxN2");
        }
    }
}
