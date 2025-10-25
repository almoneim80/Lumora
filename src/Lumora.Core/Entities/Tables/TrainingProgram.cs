namespace Lumora.Entities.Tables
{
    [SupportsElastic]
    [SupportsChangeLog]
    [Table("training_programs")]
    public class TrainingProgram : SharedData
    {
        [Searchable]
        public string? Name { get; set; }
        public string? Description { get; set; }

        [Searchable]
        public decimal? Price { get; set; }
        public decimal? Discount { get; set; }
        public string? Logo { get; set; }
        public bool HasCertificateExpiration { get; set; }
        public int CertificateValidityInMonth { get; set; }

        public List<string>? Audience { get; set; } = new();
        public List<string>? Requirements { get; set; } = new();
        public List<string>? Topics { get; set; } = new();
        public List<string>? Goals { get; set; } = new();
        public List<string>? Outcomes { get; set; } = new();
        public List<TrainerInfo>? Trainers { get; set; }

        public virtual ICollection<ProgramCourse> ProgramCourses { get; set; } = new List<ProgramCourse>();
        public virtual ICollection<TraineeProgress> TraineeProgresses { get; set; } = new List<TraineeProgress>();
        public virtual ICollection<ProgramEnrollment> ProgramEnrollments { get; set; } = new List<ProgramEnrollment>();
    }
}
