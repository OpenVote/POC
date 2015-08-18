using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace VoteChain.Data
{
    public class KeyPair
    {
        public byte[] PublicKey { get; set; }
        public byte[] PrivateKey { get; set; }
    }
    public class ECSKey : IDisposable
    {
        public static byte[] ECC_PRIVATE_KEY_START = new byte[] { 69, 67, 83, 50, 32, 0, 0, 0 };
        public static byte[] ECC_PUBLIC_KEY_START = new byte[] { 69, 67, 83, 49, 32, 0, 0, 0 };
        public byte[] PrivateKey
        {
            get
            {
                return _Key.PrivateKey;
            }
        }
        public byte[] PublicKey
        {
            get
            {
                return _Key.PublicKey;
            }
        }

        public byte[] ExportPrivateKey(bool makeECCBlob)
        {
            var returnData = _Key.PublicKey.ToList();
            returnData.AddRange(_Key.PrivateKey);
            if (makeECCBlob)
            {
                //Remove 04 that leads all uncompressed public keys
                returnData.RemoveAt(0);
                //Add ECCs blob headers
                returnData.InsertRange(0, ECSKey.ECC_PRIVATE_KEY_START);
            }
            return returnData.ToArray();
        }
        public byte[] ExportPublicKey(bool makeECCBlob)
        {
            if (!makeECCBlob)
            {
                return _Key.PublicKey;
            }
            else
            {
                //Remove 04 that leads all public keys
                var data = _Key.PublicKey.Skip(1).ToList();
                //Add ECC blob headers
                data.InsertRange(0, ECSKey.ECC_PUBLIC_KEY_START);
                return data.ToArray();
            }
        }


        KeyPair _Key;
        CngKey _cngKey;
        public static ECSKey LoadCngKeyECCBlobExport(byte[] key)
        {
            return new ECSKey(key);
        }

        public static ECSKey LoadHexPrivateKey(byte[] key)
        {
            var keyTest = key.Take(8);
            if (keyTest.SequenceEqual(ECC_PRIVATE_KEY_START))
            {
                return new ECSKey(key);
            }
            else if (keyTest.SequenceEqual(ECC_PUBLIC_KEY_START))
            {
                throw new ArgumentException("Key is invalid (uncompressed public and private keys are required material)");
            }
            else if (key.Length != 97)
            {
                throw new ArgumentException("Key is invalid (uncompressed public and private keys are required material)");
            }
            else
            {
                var keyData = key.Skip(1).Take(96).ToList();
                keyData.InsertRange(0, ECC_PRIVATE_KEY_START);
                return new ECSKey(keyData.ToArray());
            }
        }
        public static ECSKey LoadHexPublicKey(byte[] key)
        {
            var keyTest = key.Take(8);
            if (keyTest.SequenceEqual(ECC_PUBLIC_KEY_START))
            {
                return new ECSKey(key);
            }
            else if (keyTest.SequenceEqual(ECC_PRIVATE_KEY_START))
            {
                throw new ArgumentException("Key is invalid (uncompressed public key is required material)");
            }
            else if (key.Length != 65)
            {
                throw new ArgumentException("Key is invalid (uncompressed public key is required material)");
            }
            else
            {
                var keyData = key.Skip(1).Take(64).ToList();
                keyData.InsertRange(0, ECC_PUBLIC_KEY_START);
                return new ECSKey(keyData.ToArray());
            }
        }

        private ECSKey(byte[] key)
        {
            _Key = new KeyPair();

            if (key.Take(8).SequenceEqual(ECC_PRIVATE_KEY_START))
            {
                var publicKeyList = key.Skip(8).Take(64).ToList();
                publicKeyList.Insert(0, 04);
                _Key.PublicKey = publicKeyList.ToArray();
                _Key.PrivateKey = key.Skip(72).Take(32).ToArray();
                _cngKey = CngKey.Import(key, CngKeyBlobFormat.EccPrivateBlob);
            }
            else
            {
                var publicKeyList = key.Skip(8).Take(64).ToList();
                publicKeyList.Insert(0, 04);
                _Key.PublicKey = publicKeyList.ToArray();
                _cngKey = CngKey.Import(key, CngKeyBlobFormat.EccPublicBlob);
            }
        }

        public byte[] Sign(byte[] hash)
        {
            using (ECDsaCng sig = new ECDsaCng(_cngKey))
            {
                sig.HashAlgorithm = CngAlgorithm.ECDsaP256;
                return sig.SignHash(hash);
            }
        }

        internal bool Verify(byte[] hash, byte[] signature)
        {
            using (ECDsaCng sig = new ECDsaCng(_cngKey))
            {
                sig.HashAlgorithm = CngAlgorithm.ECDsaP256;
                return sig.VerifyHash(hash, signature);
            }
        }

        public void Dispose()
        {
            _cngKey.Dispose();
        }
    }
}
