#ifndef __ELECTIONGUARD_ELGAMAL_H_INCLUDED__
#define __ELECTIONGUARD_ELGAMAL_H_INCLUDED__

#include "export.h"
#include "group.h"
#include "status.h"

#ifdef __cplusplus
extern "C" {
#endif

#ifndef ElgamalKeyPair

struct eg_elgamal_keypair_s;
typedef struct eg_elgamal_keypair_s eg_elgamal_keypair_t;

// no constructors defined.  use `eg_elgamal_keypair_from_secret_new`

EG_API eg_electionguard_status_t eg_elgamal_keypair_from_secret_new(
  eg_element_mod_q_t *in_secret_key, eg_elgamal_keypair_t **out_handle);

EG_API eg_electionguard_status_t eg_elgamal_keypair_from_pair_new(
  eg_element_mod_q_t *in_secret_key, eg_element_mod_p_t *in_public_key,
  eg_elgamal_keypair_t **out_handle);

EG_API eg_electionguard_status_t eg_elgamal_keypair_free(eg_elgamal_keypair_t *handle);

EG_API eg_electionguard_status_t eg_elgamal_keypair_get_public_key(
  eg_elgamal_keypair_t *handle, eg_element_mod_p_t **out_public_key);

EG_API eg_electionguard_status_t eg_elgamal_keypair_get_secret_key(
  eg_elgamal_keypair_t *handle, eg_element_mod_q_t **out_secret_key);

#endif

#ifndef ElGamalCiphertext

struct eg_elgamal_ciphertext_s;

/**
* An "exponential ElGamal ciphertext" (i.e., with the plaintext in the exponent to allow for
* homomorphic addition). Create one with `elgamal_encrypt`. Add them with `elgamal_add`.
* Decrypt using one of the supplied instance methods.
*/
typedef struct eg_elgamal_ciphertext_s eg_elgamal_ciphertext_t;

EG_API eg_electionguard_status_t eg_elgamal_ciphertext_new(eg_element_mod_p_t *in_pad,
                                                           eg_element_mod_p_t *in_data,
                                                           eg_elgamal_ciphertext_t **out_handle);

EG_API eg_electionguard_status_t eg_elgamal_ciphertext_free(eg_elgamal_ciphertext_t *handle);

EG_API eg_electionguard_status_t eg_elgamal_ciphertext_get_pad(eg_elgamal_ciphertext_t *handle,
                                                               eg_element_mod_p_t **out_pad);
EG_API eg_electionguard_status_t eg_elgamal_ciphertext_get_data(eg_elgamal_ciphertext_t *handle,
                                                                eg_element_mod_p_t **out_data);

EG_API eg_electionguard_status_t eg_elgamal_ciphertext_crypto_hash(
  eg_elgamal_ciphertext_t *handle, eg_element_mod_q_t **out_crypto_hash);

/**
 * @brief Decrypts an ElGamal ciphertext with a "known product" (the blinding factor used in the encryption).
 * 
 * @param[in] handle The handle to the ElGamal ciphertext.
 * @param[in] in_product The known product (blinding factor).
 * @param[out] out_plaintext An exponentially encoded plaintext message.
 * @return EG_API 
 */
EG_API eg_electionguard_status_t eg_elgamal_ciphertext_decrypt_known_product(
  eg_elgamal_ciphertext_t *handle, eg_element_mod_p_t *in_product, uint64_t *out_plaintext);

/**
 * @brief Decrypt an ElGamal ciphertext using a known ElGamal secret key.
 * 
 * @param[in] handle The handle to the ElGamal ciphertext.
 * @param[in] in_secret_key The corresponding ElGamal secret key.
 * @param[out] out_plaintext An exponentially encoded plaintext message.
 * @return EG_API 
 */
EG_API eg_electionguard_status_t eg_elgamal_ciphertext_decrypt_with_secret(
  eg_elgamal_ciphertext_t *handle, eg_element_mod_q_t *in_secret_key, uint64_t *out_plaintext);

/**
 * @brief Decrypt an ElGamal ciphertext using a known nonce and the ElGamal public key.
 * 
 * @param[in] handle The handle to the ElGamal ciphertext.
 * @param[in] in_public_key The corresponding ElGamal public key.
 * @param[in] in_nonce The secret nonce used to create the ciphertext.
 * @param[out] out_plaintext An exponentially encoded plaintext message.
 * @return EG_API 
 */
EG_API eg_electionguard_status_t eg_elgamal_ciphertext_decrypt_known_nonce(
  eg_elgamal_ciphertext_t *handle, eg_element_mod_p_t *in_public_key, eg_element_mod_q_t *in_nonce,
  uint64_t *out_plaintext);

/**
 * @brief Partially Decrypts an ElGamal ciphertext with a known ElGamal secret key.
 *
 *       𝑀_i = 𝐴^𝑠𝑖 mod 𝑝 in the spec
 * 
 * @param[in] handle The handle to the ElGamal ciphertext.
 * @param[in] in_secret_key The corresponding ElGamal secret key.
 * @param[out] out_partial_decryption A partial decryption of the encrypted value.
 * @return EG_API 
 */
EG_API eg_electionguard_status_t eg_elgamal_ciphertext_partial_decrypt(
  eg_elgamal_ciphertext_t *handle, eg_element_mod_q_t *in_secret_key,
  eg_element_mod_p_t **out_partial_decryption);

#endif

#ifndef ElgamalEncrypt

/**
 * @brief Homomorphically accumulates one or more ElGamal ciphertexts by pairwise multiplication. 
 * The exponents of vote counters will add.
 * 
 * @param[in] in_ciphertexts 
 * @param[in] in_ciphertexts_size 
 * @param[out] out_ciphertext 
 * @return eg_electionguard_status_t 
 */
eg_electionguard_status_t eg_elgamal_add(eg_elgamal_ciphertext_t *in_ciphertexts[],
                                         uint64_t in_ciphertexts_size,
                                         eg_elgamal_ciphertext_t **out_ciphertext);

/**
 * Encrypts a message with a given random nonce and an ElGamal public key.
*
* @param[in] m Message to elgamal_encrypt; must be an integer in [0,Q).
* @param[in] nonce Randomly chosen nonce in [1,Q).
* @param[in] public_key ElGamal public key.
* @param[out] out_ciphertext the ciphertext result.  Caller is responsible for lifecycle
*/
EG_API eg_electionguard_status_t eg_elgamal_encrypt(uint64_t in_plaintext,
                                                    eg_element_mod_q_t *in_nonce,
                                                    eg_element_mod_p_t *in_public_key,
                                                    eg_elgamal_ciphertext_t **out_ciphertext);

#endif

#ifndef HashedElGamalCiphertext

struct eg_hashed_elgamal_ciphertext_s;

/**
* A "hashed ElGamal ciphertext" (i.e., with the plaintext in the exponent to allow for
* homomorphic addition). Create one with `hashed_elgamal_encrypt`. Add them with `elgamal_add`.
* Decrypt using one of the supplied instance methods.
*/
typedef struct eg_hashed_elgamal_ciphertext_s eg_hashed_elgamal_ciphertext_t;

// no constructors defined.  use `eg_hashed_elgamal_encrypt`

EG_API eg_electionguard_status_t
eg_hashed_elgamal_ciphertext_free(eg_hashed_elgamal_ciphertext_t *handle);

EG_API eg_electionguard_status_t eg_hashed_elgamal_ciphertext_get_pad(
  eg_hashed_elgamal_ciphertext_t *handle, eg_element_mod_p_t **out_pad);
EG_API eg_electionguard_status_t eg_hashed_elgamal_ciphertext_get_data(
  eg_hashed_elgamal_ciphertext_t *handle, char **out_data, uint64_t *out_size);
EG_API eg_electionguard_status_t eg_hashed_elgamal_ciphertext_get_mac(
  eg_hashed_elgamal_ciphertext_t *handle, char **out_data, uint64_t *out_size);

EG_API eg_electionguard_status_t eg_hashed_elgamal_ciphertext_crypto_hash(
  eg_hashed_elgamal_ciphertext_t *handle, eg_element_mod_q_t **out_crypto_hash);

eg_electionguard_status_t eg_hashed_elgamal_ciphertext_decrypt_with_secret(
  eg_hashed_elgamal_ciphertext_t *handle, eg_element_mod_q_t *in_secret_key,
  eg_element_mod_q_t *in_description_hash, bool in_look_for_padding, uint8_t **out_data,
  uint64_t *out_size);

#endif

#ifndef HashedElgamalEncrypt

/**
 * Encrypts a message with a given random nonce and an ElGamal public key.
*
* @param[in] m Message to elgamal_encrypt; must be an integer in [0,Q).
* @param[in] nonce Randomly chosen nonce in [1,Q).
* @param[in] public_key ElGamal public key.
* @param[out] out_ciphertext the ciphertext result.  Caller is responsible for lifecycle
*/
EG_API eg_electionguard_status_t
eg_hashed_elgamal_encrypt(uint8_t *in_plaintext, uint64_t in_length, eg_element_mod_q_t *in_nonce,
                          eg_element_mod_p_t *in_public_key, eg_element_mod_q_t *in_seed,
                          eg_hashed_elgamal_ciphertext_t **out_ciphertext);

#endif

#ifdef __cplusplus
}
#endif
#endif /* __ELECTIONGUARD_ELGAMAL_H_INCLUDED__ */
