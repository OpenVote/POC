using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VoteChain.Data
{
    public class Vote
    {
        public Guid Id { get; set; }
        public string Description { get; set; }
        public Guid GenesisBallotId { get; set; }
    }
}
