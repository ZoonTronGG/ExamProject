using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Spatial;

namespace ExamProject.Models
{
    [Table("ExamTable")]
    public partial class Vacancies
    {
        [Key]
        public int VacancyId { get; set; }

        [StringLength(70)]
        public string Title { get; set; }

        [StringLength(100)]
        public string Description { get; set; }

        public DateTime? Date { get; set; }

        [StringLength(30)]
        public string Author { get; set; }

        public override string ToString()
        {
            return $"VacancyId: {VacancyId}. " +
                $"Title: {Title}\n" +
                $"Description: {Description}\n" +
                  $"Date: {Date}\n" +
                   $"Author: {Author}\n" +
                   "----------------\n";
        }

    }
}
