using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DesktopTracker.Infra.Models
{
    public class DeviceData
    {
        public DeviceData()
        {
            DeviceId = Guid.NewGuid();
        }

        [Required]
        [Key]
        public Guid DeviceId { get; private set; }

        public TimeSpan? UpTime { get; set; }

        public bool SessionComplete { get; set; }

        [ForeignKey("UserData")]
        public Guid UserId { get; set; }
        public UserData UserData { get; set; }
    }
}
