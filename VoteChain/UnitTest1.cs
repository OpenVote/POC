using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VoteChain.Data;
using System.Security.Cryptography;
using System.Text;
using System.Collections.Generic;
using System.Linq;

namespace VoteChain
{
    [TestClass]
    public class UnitTest1
    {
        List<Ballot> ballots = new List<Ballot>();
        [TestInitialize]
        public void InitializeVote()
        {
            var genesisBallot = new Ballot();
            var voterKey = makeRandomSigningKey();
            var ballot = new Ballot();
            ballot.Id = Guid.NewGuid();
            ballot.SubmissionDate = DateTime.UtcNow;
            ballot.BallotData = "{genesis for vote [put ID here]}";
            ballot.SignBallot(voterKey);
            ballot.SignSecret(voterKey, "this is a secret");
            ballot.LockBallotToChain(voterKey, Ballot.EmptyHash);
            ballots.Add(ballot);
        }
        [TestMethod]
        public void TestMethod1()
        {
            for (var i = 0; i < 1000; i++)
            {
                var voterKey = makeRandomSigningKey();
                var vote = "{President:'Dogbert'}";
                var ballot = new Ballot();
                ballot.Id = Guid.NewGuid();
                ballot.SubmissionDate = DateTime.UtcNow;
                ballot.BallotData = vote;
                ballot.SignBallot(voterKey);
                ballot.SignSecret(voterKey, "this is a secret");
                ballot.LockBallotToChain(voterKey, ballots.Last().BallotHash);
                ballots.Add(ballot);
                voterKey.Dispose();
            }
            var validation = Ballot.BallotChainIsValid(ballots);
            Assert.IsTrue(validation.ChainValid);
        }

        private ECSKey makeRandomSigningKey()
        {
            var p = new CngKeyCreationParameters
            {
                ExportPolicy = CngExportPolicies.AllowPlaintextExport
            };

            var cnk = CngKey.Create(CngAlgorithm.ECDsaP256, System.Guid.NewGuid().ToString(), p);
            var export = cnk.Export(CngKeyBlobFormat.EccPrivateBlob);
            cnk.Delete();
            cnk.Dispose();

            var signingKey = ECSKey.LoadCngKeyECCBlobExport(export);
            return signingKey;
        }
    }
}
