﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Randomizer.Shared.Models
{

    public class TrackerBossState
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        public TrackerState TrackerState { get; set; }
        public string BossName { get; set; }
        public bool Defeated { get; set; }
    }

}
