using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Research.SEAL;
using System.Diagnostics;

namespace HEtest
{
    class VS
    {
        private SEALContext context;
        private RelinKeys relinKeys;
        private Evaluator evaluator;

        private Ciphertext px_encrypted;
        private Ciphertext py_encrypted;

        public VS(RelinKeys relinKeys)
        {
            using EncryptionParameters parms = new EncryptionParameters(SchemeType.CKKS);
            ulong polyModulusDegree = 8192;
            parms.PolyModulusDegree = polyModulusDegree;
            parms.CoeffModulus = CoeffModulus.Create(polyModulusDegree, new int[] { 60, 40, 40, 60 });
            context = new SEALContext(parms);
            evaluator = new Evaluator(context);
            this.relinKeys = relinKeys;
        }

        public bool computeDistance(Ciphertext rx_encrypted, Ciphertext ry_encrypted, 
                                    Ciphertext encrypted_result)
        {
            using Ciphertext dist_x2_encrypted = new Ciphertext();
            using Ciphertext dist_y2_encrypted = new Ciphertext();
            evaluator.Sub(px_encrypted, rx_encrypted, dist_x2_encrypted);
            evaluator.SquareInplace(dist_x2_encrypted);
            evaluator.RelinearizeInplace(dist_x2_encrypted, relinKeys);
            evaluator.RescaleToNextInplace(dist_x2_encrypted);
            evaluator.Sub(py_encrypted, ry_encrypted, dist_y2_encrypted);
            evaluator.SquareInplace(dist_y2_encrypted);
            evaluator.RelinearizeInplace(dist_y2_encrypted, relinKeys);
            evaluator.RescaleToNextInplace(dist_y2_encrypted);
            evaluator.Add(dist_x2_encrypted, dist_y2_encrypted, encrypted_result);
            return true;
        }

        public bool verifyDistanceFromCS(CS cs, Ciphertext encrypted_result)
        {
            return cs.verifyLoginInformation(encrypted_result);
        }

        public string showContent(bool validationResult)
        {
            if (validationResult)
                return "\nLogin successful! Welcome!\n";
            else
                return "\nLogin failed! Validation code error!\n";
        }

        public void changeRelinKeys(RelinKeys relinKeys)
        {
            this.relinKeys = relinKeys;
        }

        public void setPasswordPoint(Ciphertext px, Ciphertext py)
        {
            this.px_encrypted = px;
            this.py_encrypted = py;
        }

        public double showPasswordPointLongitude(CS cs)
        {
            return cs.decryptCiphertext(px_encrypted);
        }

        public double showPasswordPointLatitude(CS cs)
        {
            return cs.decryptCiphertext(py_encrypted);
        }
    }
}
