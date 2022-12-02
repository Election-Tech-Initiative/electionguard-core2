using System;
using System.Collections.Generic;
using System.Linq;

namespace ElectionGuard.ElectionSetup
{
    /// <summary>
    /// A polynomial defined by coefficients
    ///
    /// The 0-index coefficient is used for a secret key which can be
    /// discovered by a quorum of n guardians corresponding to n coefficients.
    /// </summary>
    public class ElectionPolynomial
    {
        public ElectionPolynomial(List<Coefficient> coefficients)
        {
            Coefficients = coefficients;
        }

        public List<Coefficient> Coefficients;

        /// <summary>
        /// Access the list of public keys generated from secret coefficient
        /// </summary>
        public List<PublicCommitment> GetCommitments()
        {
            return Coefficients
                .Select(i => i.Commitment)
                .ToList();
        }

        /// <summary>
        /// Access the list of proof of possession of the private key for the secret coefficient
        /// </summary>
        public List<SchnorrProof> GetProofs()
        {
            return Coefficients
                .Select(i => i.Proof)
                .ToList();
        }

        /// <summary>
        /// Generates a polynomial for sharing election keys
        /// </summary>
        /// <param name="numberOfCoefficients">Number of coefficients of polynomial</param>
        /// <param name="nonce">An optional nonce parameter that may be provided (useful for testing)</param>
        /// <returns>Polynomial used to share election keys</returns>
        public static ElectionPolynomial GeneratePolynomial(int numberOfCoefficients, ElementModQ nonce)
        {
            List<Coefficient> coefficients = new List<Coefficient>();

            for (int i = 0; i < numberOfCoefficients; i++)
            {
                // todo: the hard stuff in C++ goes here
                throw new NotImplementedException();

                //    // Note: the nonce value is not safe. It is designed for testing only.
                //    // This method should be called without the nonce in production.
                //    ElementModQ value;
                //    if (nonce != null)
                //        value = add_q(nonce, i);
                //    else
                //        value = rand_q();

                //    var commitment = g_pow_p(value)
                //    var proof = make_schnorr_proof(
                //        ElGamalKeyPair(value, commitment), rand_q()
                //    )  # TODO Alternate schnoor proof method that doesn't need KeyPair
                //    var coefficient = new Coefficient(value, commitment, proof)
                //    coefficients.append(coefficient)
            }

            return new ElectionPolynomial(coefficients);
        }
    }
}