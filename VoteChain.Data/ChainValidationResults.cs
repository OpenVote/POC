using System.Collections.Generic;

namespace VoteChain.Data
{
    public class ChainValidationResults
    {
        public List<string> Messages = new List<string>();
        public bool ChainValid
        {
            get
            {
                return Messages.Count == 0;
            }
        }
    }
}