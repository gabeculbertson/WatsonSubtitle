using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WatsonSubtitle {
    public class Word {
        public double StartTime { get; set; }
        public double EndTime { get; set; }
        public string Text { get; set; }
        public double Confidence { get; set; }
    }
}
