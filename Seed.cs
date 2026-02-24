using System;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;

namespace Cubic_MapGenerator
{
    public class Seed
    {
        private readonly long baseSeed;
        private int callCount = 0;

        public Seed(string seed)
        {
            baseSeed = Middle16(Convert(seed));
        }
        public Seed()
        {
            string seed = null;
            baseSeed = Middle16(Convert(seed));
        }

        private BigInteger Convert(string input)
        {
            if(input == null)
            {
                Random rnd = new Random();
                int randomValue = rnd.Next();
                string randomSeed = randomValue.ToString();
                using (SHA256 sha = SHA256.Create())
                {
                    byte[] hashBytes = sha.ComputeHash(Encoding.UTF8.GetBytes(randomSeed));

                    byte[] positiveHash = new byte[hashBytes.Length + 1];
                    Array.Copy(hashBytes, positiveHash, hashBytes.Length);

                    return new BigInteger(positiveHash);
                }
            }
            else
            {
                using (SHA256 sha = SHA256.Create())
                {
                    byte[] hashBytes = sha.ComputeHash(Encoding.UTF8.GetBytes(input));

                    byte[] positiveHash = new byte[hashBytes.Length + 1];
                    Array.Copy(hashBytes, positiveHash, hashBytes.Length);

                    return new BigInteger(positiveHash);
                }
            }
            
        }

        private long Middle16(BigInteger value)
        {
            string s = BigInteger.Abs(value).ToString();
            int length = s.Length;

            if (length < 16)
                return long.Parse(s);

            int start = (length / 2) - 8;
            string mid = s.Substring(start, 16);

            return long.Parse(mid);
        }

        private long DeterministicHash(long seed, int index)
        {
            string combined = seed.ToString() + ":" + index.ToString();

            BigInteger hashed = Convert(combined);
            return Middle16(hashed);
        }

        public int ReturnIntValue(int min, int max)
        {
            if (min > max)
                throw new ArgumentException("min cannot be greater than max");

            long h = DeterministicHash(baseSeed, callCount++);
            long range = (max - (long)min) + 1;

            int result = (int)(Math.Abs(h) % range + min);
            return result;
        }

        public float ReturnFloatValue(float min, float max)
        {
            long h = DeterministicHash(baseSeed, callCount++);
            double t = (Math.Abs(h) % 1_000_000_000_000_0000L) / 1e16;

            return (float)(min + (max - min) * t);
        }

        public bool ReturnBoolValue()
        {
            long h = DeterministicHash(baseSeed, callCount++);
            return (h & 1) == 1;
        }
    }
}
