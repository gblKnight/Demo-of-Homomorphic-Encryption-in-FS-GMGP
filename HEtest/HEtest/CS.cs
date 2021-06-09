using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Research.SEAL;
using System.Diagnostics;

namespace HEtest
{
    class CS
    {
        private EncryptionParameters parms;
        private SEALContext context;
        private PublicKey publicKey;
        private SecretKey secretKey;
        private RelinKeys relinKeys;
        private CKKSEncoder encoder;
        private Decryptor decryptor;
        private double maxToleranceDistance;


        public CS()
        {
            maxToleranceDistance = 100;

            parms = new EncryptionParameters(SchemeType.CKKS);
            ulong polyModulusDegree = 8192;
            parms.PolyModulusDegree = polyModulusDegree;
            parms.CoeffModulus = CoeffModulus.Create(polyModulusDegree, new int[] { 60, 40, 40, 60 });
            context = new SEALContext(parms);

            using KeyGenerator keygen = new KeyGenerator(context);
            secretKey = keygen.SecretKey;
            keygen.CreatePublicKey(out PublicKey publicKey);
            keygen.CreateRelinKeys(out RelinKeys relinKeys);
            this.publicKey = publicKey;
            this.relinKeys = relinKeys;

            encoder = new CKKSEncoder(context);
            decryptor = new Decryptor(context, secretKey);
        }

        public bool verifyLoginInformation(Ciphertext encrypted_result)
        {
            using Plaintext plain_result = new Plaintext();
            decryptor.Decrypt(encrypted_result, plain_result);
            List<double> result = new List<double>();
            encoder.Decode(plain_result, result);
            for (int i = 0; i < result.Count; ++i)
            {
                if (result[i] * 100 > maxToleranceDistance)
                {
                    return false;
                }
            }
            return true;
        }

        public PublicKey sendPublicKey()
        {
            return publicKey;
        }

        public RelinKeys sendRelinKeys()
        {
            return relinKeys;
        }
        public SecretKey sendSecretKey()
        {
            return secretKey;
        }


        private static string showList(List<double> l, int length)
        {
            string listPrinter = "";
            for (int i = 0; i < length; ++i)
            {
                listPrinter += (l[i].ToString() + "  ");
            }
            //l.ForEach(e => listPrinter += (e.ToString() + "  "));
            return listPrinter;
        }

        public string showCiphertext(Ciphertext ciphertext)
        {
            using Plaintext plaintext = new Plaintext();
            decryptor.Decrypt(ciphertext, plaintext);
            List<double> text = new List<double>();
            encoder.Decode(plaintext, text);
            return showList(text, 1);
        }

        public double decryptCiphertext(Ciphertext ciphertext)
        {
            using Plaintext plaintext = new Plaintext();
            decryptor.Decrypt(ciphertext, plaintext);
            List<double> text = new List<double>();
            encoder.Decode(plaintext, text);
            return text[0];
        }
    }
}
