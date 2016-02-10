using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WatsonSubtitle {
    public class Phrase {
        public List<Word> Words { get; set; }

        public Phrase() {
            Words = new List<Word>();
        }

        public override string ToString() {
            var lines = new List<string>();
            lines.Add("");

            foreach(var word in Words) {
                if(lines.Last().Length > 18) {
                    lines.Add("");
                }
                lines[lines.Count - 1] = lines.Last() + word.Text + " ";
            }

            var s = new StringBuilder();
            foreach(var line in lines) {
                s.AppendLine(line);
            }
            var str = s.ToString();
            return str.Substring(0, str.Length - 1);
        }

        public double GetStart() {
            if(Words.Count > 0) {
                return Words[0].StartTime;
            }
            return 0;
        }

         public double GetEnd() {
            if(Words.Count > 0) {
                return Words[Words.Count - 1].EndTime;
            }
            return 0;
        }
    }
}
