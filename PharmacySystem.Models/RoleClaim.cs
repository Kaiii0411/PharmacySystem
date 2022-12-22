﻿using System;
using System.Collections.Generic;

namespace PharmacySystem.Models
{
    public partial class RoleClaim
    {
        public int Id { get; set; }
        public long RoleId { get; set; }
        public string? ClaimType { get; set; }
        public string? ClaimValue { get; set; }

        public virtual Role Role { get; set; } = null!;
    }
}
