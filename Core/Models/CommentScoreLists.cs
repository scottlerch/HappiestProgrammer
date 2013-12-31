using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HappiestProgrammer.Core.Models
{
    public class CommentScoreLists
    {
        public IEnumerable<CommentScore> Top { get; set; }

        public IEnumerable<CommentScore> Bottom { get; set; }
    }
}
