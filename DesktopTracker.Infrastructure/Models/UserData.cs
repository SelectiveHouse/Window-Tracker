using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DesktopTracker.Infra.Models
{
    public class UserData
    {
        public UserData()
        {
            UserId = Guid.NewGuid();
        }

        [Required]
        [Key]
        public Guid UserId { get; private set; }

        public string UserName { get; set; }

        public ICollection<WindowData> WindowDatas { get; set; }
        public ICollection<DeviceData> DeviceDatas { get; set; }
    }
}
