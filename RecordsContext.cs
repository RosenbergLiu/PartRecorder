using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PartRecorder
{
    public class RecordsContext:DbContext
    {
            public DbSet<Record> Records { get; set; }

            protected override void OnConfiguring(
                DbContextOptionsBuilder optionsBuilder)
            {
                optionsBuilder.UseSqlite(
                    "Data Source=records.db");
            }
    }
}
