using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Project.Inputs
{
    public class DayTimes
    {
        [JsonProperty("day")]
        public string Day { get; set; }

        [JsonProperty("times")]
        public List<string> Times { get; set; }
    }
}
