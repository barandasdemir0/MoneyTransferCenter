using System;
using System.Collections.Generic;
using System.Text;

namespace MoneyTransferCenter.Domain.Entities;

public class RateLimitPolicySettings
{
    public int PermitLimit { get; set; }
    public int WindowMinutes { get; set; }
    public int SegmentsPerWindow { get; set; }
    public int QueueLimit { get; set; }
}
