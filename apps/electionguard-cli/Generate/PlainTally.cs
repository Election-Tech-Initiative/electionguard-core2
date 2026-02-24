using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ElectionGuard.CLI.Generate
{


    public class PlainTally
    {
        public List<Contest> contests { get; set; } = new();
        public string object_id { get; set; }
        public string style_id { get; set; }

        public void Clear()
        {
            foreach (Contest contest in contests)
            {
                contest.Clear();
            }
        }
        public static PlainTally operator +(PlainTally left, PlainTally right)
        {
            foreach (var contest in right.contests)
            {
                var l = left.contests.SingleOrDefault(c => c.object_id == contest.object_id);
                if (l != null)
                {
                    l += contest;
                }
                else
                {
                    left.contests.Add(contest);
                }
            }
            return left;
        }
    }

    public class Contest
    {
        public List<Ballot_Selections> ballot_selections { get; set; } = new();
        public string object_id { get; set; }

        public void Clear()
        {
            foreach (var selection in ballot_selections)
            {
                selection.Clear();
            }
        }

        public static Contest operator +(Contest left, Contest right)
        {
            foreach (var selection in right.ballot_selections)
            {
                var l = left.ballot_selections.SingleOrDefault(b => b.object_id == selection.object_id);
                if (l != null)
                {
                    l += selection;
                }
                else
                {
                    left.ballot_selections.Add(selection);
                }
            }
            return left;
        }
    }

    public class Ballot_Selections
    {
        public bool is_placeholder_selection { get; set; }
        public string object_id { get; set; }
        public int vote { get; set; }
        public string write_in { get; set; }

        public void Clear()
        {
            vote = 0;
        }

        public static Ballot_Selections operator +(Ballot_Selections left, Ballot_Selections right)
        {
            left.vote += right.vote;
            return left;
        }
    }
}
