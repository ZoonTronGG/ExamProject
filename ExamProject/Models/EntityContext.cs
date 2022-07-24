using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;

namespace ExamProject.Models
{
    public partial class EntityContext : DbContext
    {
        public EntityContext()
            : base("name=EntityContext")
        {
        }

        public virtual DbSet<Vacancies> Vacancies { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
        }
    }
}
