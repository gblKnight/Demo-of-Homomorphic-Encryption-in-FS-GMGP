using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Research.SEAL;
using System.Diagnostics;

namespace HEtest
{
    class User
    {
        SEALContext context;
        private double scale;
        private CKKSEncoder encoder;
        private Encryptor encryptor;

        public User(PublicKey publicKey)
        {
            using EncryptionParameters parms = new EncryptionParameters(SchemeType.CKKS);
            ulong polyModulusDegree = 8192;
            scale = Math.Pow(2.0, 40);
            parms.PolyModulusDegree = polyModulusDegree;
            parms.CoeffModulus = CoeffModulus.Create(polyModulusDegree, new int[] { 60, 40, 40, 60 });
            context = new SEALContext(parms);

            encoder = new CKKSEncoder(context);
            encryptor = new Encryptor(context, publicKey);
        }

        private void encryptedCoordinates(List<double> input, Ciphertext input_encrypted)
        {
            using Plaintext input_plain = new Plaintext();
            encoder.Encode(input, scale, input_plain);
            encryptor.Encrypt(input_plain, input_encrypted);
        }

        public void encryptedCoordinates(double input, Ciphertext input_encrypted)
        {
            List<double> inputList = new List<double> { input };
            encryptedCoordinates(inputList, input_encrypted);
        }

        public void changePublicKey(PublicKey publicKey)
        {
            encryptor = new Encryptor(context, publicKey);
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
    }
}
