using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace VoteChain.Data
{
    public class Ballot
    {
        public Guid Id { get; set; }
        public byte[] SigningPublicKey { get; set; }
        public string BallotData { get; set; }
        public byte[] BallotSignature { get; set; }
        public byte[] SecretSignature { get; set; }
        public byte[] BallotHash { get; set; }
        public byte[] BallotHashSignature { get; set; }
        public byte[] PreviousBallotHash { get; set; }
        public DateTime SubmissionDate { get; set; }
        public DateTime LockDate { get; set; }
        public static byte[] EmptyHash { get
            {
                return new byte[32] { 0, 0, 0, 0, 0, 0, 0, 0,
                                      0, 0, 0, 0, 0, 0, 0, 0,
                                      0, 0, 0, 0, 0, 0, 0, 0,
                                      0, 0, 0, 0, 0, 0, 0, 0 };
            }
        }

        public byte[] GetBallotHashingData()
        {
            var ticksBytes = BitConverter.GetBytes(SubmissionDate.Ticks);
            var idBytes = Id.ToByteArray();
            var returnData = new byte[idBytes.Length + SigningPublicKey.Length + BallotSignature.Length + SecretSignature.Length + (PreviousBallotHash != null? PreviousBallotHash.Length:0) + ticksBytes.Length];
            System.Buffer.BlockCopy(idBytes, 0, returnData, 0, idBytes.Length);
            var loc = idBytes.Length;
            System.Buffer.BlockCopy(SigningPublicKey, 0, returnData, loc, SigningPublicKey.Length);
            loc += SigningPublicKey.Length;
            System.Buffer.BlockCopy(BallotSignature, 0, returnData, loc, BallotSignature.Length);
            loc += BallotSignature.Length;
            System.Buffer.BlockCopy(SecretSignature, 0, returnData, loc, SecretSignature.Length);
            loc += SecretSignature.Length;
            if (PreviousBallotHash != null)
            {
                System.Buffer.BlockCopy(PreviousBallotHash, 0, returnData, loc, PreviousBallotHash.Length);
                loc += PreviousBallotHash.Length;
            }
            System.Buffer.BlockCopy(ticksBytes, 0, returnData, loc, ticksBytes.Length);
            return returnData;
        }

        public bool BallotIsValid()
        {
            using(var key = ECSKey.LoadHexPublicKey(SigningPublicKey))
            {
                return key.Verify(UTF8Encoding.Unicode.GetBytes(BallotData), BallotSignature);
            }
        }
        public static ChainValidationResults BallotChainIsValid(List<Ballot> ballots)
        {
            var results = new ChainValidationResults();
            var eoc = false;
            var processedBallots = 0;
            byte[] nextBallotHash = Ballot.EmptyHash;
            while(eoc == false) {
                if(processedBallots == ballots.Count)
                {
                    eoc = true;
                    break;
                }
                else
                {
                    processedBallots++;
                }
                Ballot ballot = null;
                try
                {
                    ballot = ballots.Single(x => x.PreviousBallotHash.SequenceEqual(nextBallotHash));
                }
                catch(Exception ex)
                {
                    results.Messages.Add(ex.Message + " for ballot " + ballot.Id.ToString());
                }
                if (!ballot.BallotIsValid())
                {
                    results.Messages.Add("Ballot data has been tampered with " + ballot.Id.ToString());
                }
                if (!ballot.ballotHasValidHash())
                {
                    results.Messages.Add("Ballot chain is broken " + ballot.Id.ToString());
                }
                if(!ballot.PreviousBallotHash.SequenceEqual(Ballot.EmptyHash) && ballots.Count(x=> x.BallotHash == ballot.PreviousBallotHash) == 0)
                {
                    results.Messages.Add("Ballot chain is broken " + ballot.Id.ToString());
                }
                nextBallotHash = ballot.BallotHash;
            }
            return results;
        }

        private bool ballotHasValidHash()
        {
            using (var key = ECSKey.LoadHexPublicKey(SigningPublicKey))
            {
                SHA256Managed hashstring = new SHA256Managed();
                var ballotHash = hashstring.ComputeHash(GetBallotHashingData());
                if (ballotHash.SequenceEqual(BallotHash))
                {
                    return key.Verify(BallotHash, BallotHashSignature);
                }
                return false;
            }
        }

        public void LockBallotToChain(ECSKey key, byte[] previousBallotHash)
        {
            PreviousBallotHash = previousBallotHash;
            SHA256Managed hashstring = new SHA256Managed();
            BallotHash = hashstring.ComputeHash(GetBallotHashingData());
            BallotHashSignature = key.Sign(BallotHash);
            LockDate = DateTime.UtcNow;
        }

        public void SignBallot(ECSKey key)
        {
            BallotSignature = key.Sign(UTF8Encoding.Unicode.GetBytes(BallotData));
            SigningPublicKey = key.PublicKey;
        }

        public void SignSecret(ECSKey key, string secret)
        {
            SecretSignature = key.Sign(UTF8Encoding.Unicode.GetBytes(secret));
            SigningPublicKey = key.PublicKey;
        }
    }
}
