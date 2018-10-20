using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Specks
{
    class Score
    {
        public string Name { get; set; }
        public int Click { get; set; }
        public TimeSpan Time { get; set; }
        public override string ToString()
        {
            return String.Format("{0} ({1}, {2})", Name, Click, Time);
        }

        public Score(string line)
        {
            string[] tmp = line.Split(new []{' ','(',',',')'}, StringSplitOptions.RemoveEmptyEntries);
            Name = tmp[0];
            Click = Int32.Parse(tmp[1]);
            Time = TimeSpan.Parse(tmp[2]);
        }
        public Score() { }
    }
}
