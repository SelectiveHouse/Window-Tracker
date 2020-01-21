using DesktopTracker.Infra.Enums;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DesktopTracker.Infra.Models
{
    public class WindowData
    {
        public WindowData()
        {
            WindowId = Guid.NewGuid();
        }

        [Required]
        [Key]
        public Guid WindowId { get; private set; }

        public int WindowsIdentifier { get; set; }
        public string FullName { get; set; }
        public DateTime RecordedEnteredTime { get; set; }
        public DateTime? RecordedExitTime { get; set; }
        public DateTime? RecordedIdleTime { get; set; }
        public WindowState WindowState { get; set; }
        public bool Exited { get; set; } = false;

        [ForeignKey("UserData")]
        public Guid UserId { get; set; }
        public UserData UserData { get; set; }
    }
}
