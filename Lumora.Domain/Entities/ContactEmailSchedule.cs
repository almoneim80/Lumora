namespace Lumora.Domain.Entities
{
    [Table("contact_email_schedule")]
    [SupportsChangeLog]
    public class ContactEmailSchedule : BaseEntity
    {
        /// <summary>
        /// Gets or sets reference to the contact table.
        /// </summary>
        [Required]
        public int ContactId { get; set; }

        [JsonIgnore]
        [ForeignKey("ContactId")]
        public virtual Contact? Contact { get; set; }

        /// <summary>
        /// Gets or sets reference to the EmailSchedule table.
        /// </summary>
        [Required]
        public int ScheduleId { get; set; }

        [JsonIgnore]
        [ForeignKey("ScheduleId")]
        public virtual EmailSchedule? Schedule { get; set; }

        /// <summary>
        /// Gets or sets the status of the completion of sending all emails assigned to the schedule.
        /// </summary>
        public ScheduleStatus Status { get; set; }
    }
}
