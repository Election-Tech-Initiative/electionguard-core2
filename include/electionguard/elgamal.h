#ifndef __ELECTIONGUARD_ELGAMAL_H_INCLUDED__
#define __ELECTIONGUARD_ELGAMAL_H_INCLUDED__

#include "constants.h"
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
 * @brief Homomorphically accumulates one or more ElGamal ciphertexts by pairwise multiplication. 
 * The exponents of vote counters will add.
 * 
 * @param[in] handle 
 * @param[in] in_ciphertexts 
 * @param[in] in_ciphertexts_size 
 * @param[out] out_ciphertext 
 * @return eg_electionguard_status_t 
 */
EG_API eg_electionguard_status_t eg_elgamal_ciphertext_add_collection(
  eg_elgamal_ciphertext_t *handle, eg_elgamal_ciphertext_t *in_ciphertexts[],
  uint64_t in_ciphertexts_size, eg_elgamal_ciphertext_t **out_ciphertext);

/**
 * @brief Homomorphically accumulates two ElGamal ciphertexts by pairwise multiplication. 
 * The exponents of vote counters will add.
 * 
 * @param[in] handle
 * @param[in] in_ciphertext_b 
 * @param[out] out_ciphertext 
 * @return eg_electionguard_status_t 
 */
EG_API eg_electionguard_status_t
eg_elgamal_ciphertext_add(eg_elgamal_ciphertext_t *handle, eg_elgamal_ciphertext_t *in_ciphertext_b,
                          eg_elgamal_ciphertext_t **out_ciphertext);

/**
 * @brief Decrypts an ElGamal ciphertext with an "accumulation" (the product of partial decryptions).
 * 
 * @param[in] handle The handle to the ElGamal ciphertext.
 * @param[in] in_share_accumulation The accumulation of shares (∏𝑀𝑖).
 * @param[in] in_encryption_base The base value used in the encryption.
 * @param[out] out_plaintext An exponentially encoded plaintext message.
 * @return EG_API 
 */
EG_API eg_electionguard_status_t eg_elgamal_ciphertext_decrypt_accumulation(
  eg_elgamal_ciphertext_t *handle, eg_element_mod_p_t *in_share_accumulation,
  eg_element_mod_p_t *in_encryption_base, uint64_t *out_plaintext);

/**
 * @brief Decrypt an ElGamal ciphertext using a known ElGamal secret key.
 * 
 * @param[in] handle The handle to the ElGamal ciphertext.
 * @param[in] in_secret_key The corresponding ElGamal secret key.
 * @param[out] out_plaintext An exponentially encoded plaintext message.
 * @return EG_API 
 */
EG_API eg_electionguard_status_t eg_elgamal_ciphertext_decrypt_with_secret(
  eg_elgamal_ciphertext_t *handle, eg_element_mod_q_t *in_secret_key,
  eg_element_mod_p_t *in_encryption_base, uint64_t *out_plaintext);

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
EG_API eg_electionguard_status_t
eg_elgamal_add_collection(eg_elgamal_ciphertext_t *in_ciphertexts[], uint64_t in_ciphertexts_size,
                          eg_elgamal_ciphertext_t **out_ciphertext);

/**
 * @brief Homomorphically accumulates two ElGamal ciphertexts by pairwise multiplication. 
 * The exponents of vote counters will add.
 * 
 * @param[in] in_ciphertext_a 
 * @param[in] in_ciphertext_b 
 * @param[out] out_ciphertext 
 * @return eg_electionguard_status_t 
 */
EG_API eg_electionguard_status_t eg_elgamal_add(eg_elgamal_ciphertext_t *in_ciphertext_a,
                                                eg_elgamal_ciphertext_t *in_ciphertext_b,
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

EG_API eg_electionguard_status_t eg_hashed_elgamal_ciphertext_new(
  eg_element_mod_p_t *in_pad, uint8_t *in_data, uint64_t in_data_length, uint8_t *in_mac,
  uint64_t in_mac_length, eg_hashed_elgamal_ciphertext_t **out_handle);
EG_API eg_electionguard_status_t
eg_hashed_elgamal_ciphertext_free(eg_hashed_elgamal_ciphertext_t *handle);

EG_API eg_electionguard_status_t eg_hashed_elgamal_ciphertext_get_pad(
  eg_hashed_elgamal_ciphertext_t *handle, eg_element_mod_p_t **out_pad);
EG_API eg_electionguard_status_t eg_hashed_elgamal_ciphertext_get_data(
  eg_hashed_elgamal_ciphertext_t *handle, uint8_t **out_data, uint64_t *out_size);
EG_API eg_electionguard_status_t eg_hashed_elgamal_ciphertext_get_mac(
  eg_hashed_elgamal_ciphertext_t *handle, uint8_t **out_data, uint64_t *out_size);

EG_API eg_electionguard_status_t eg_hashed_elgamal_ciphertext_crypto_hash(
  eg_hashed_elgamal_ciphertext_t *handle, eg_element_mod_q_t **out_crypto_hash);

/**
 * @brief Decrypts ciphertext with the Auxiliary Encryption method (as specified in the
 * ElectionGuard specification) given a random nonce, an ElGamal public key,
 * and an encryption seed. The encrypt may be called to look for padding to
 * verify and remove, in this case the plaintext will be smaller than
 * the ciphertext, or not to look for padding in which case the
 * plaintext will be the same size as the ciphertext.
 * 
 * @param[in] handle 
 * @param[in] in_public_key 
 * @param[in] in_secret_key 
 * @param[in] in_hash_prefix A prefix value for the hash used to create the session key.
 * @param[in] in_encryption_seed 
 * @param[in] in_look_for_padding 
 * @param[out] out_data 
 * @param[out] out_size 
 * @return EG_API 
 */
EG_API eg_electionguard_status_t eg_hashed_elgamal_ciphertext_decrypt_with_secret(
  eg_hashed_elgamal_ciphertext_t *handle, eg_element_mod_p_t *in_public_key,
  eg_element_mod_q_t *in_secret_key, char *in_hash_prefix, eg_element_mod_q_t *in_encryption_seed,
  bool in_look_for_padding, uint8_t **out_data, uint64_t *out_size);

/**
 * @brief Partially Decrypts an ElGamal ciphertext with a known ElGamal secret key.
 *
 *       𝑀_i = C0^𝑠𝑖 mod 𝑝 in the spec
 * 
 * @param[in] handle The handle to the ElGamal ciphertext.
 * @param[in] in_secret_key The corresponding ElGamal secret key.
 * @param[out] out_partial_decryption A partial decryption of the encrypted value.
 * @return EG_API 
 */
EG_API eg_electionguard_status_t eg_hashed_elgamal_ciphertext_partial_decrypt(
  eg_hashed_elgamal_ciphertext_t *handle, eg_element_mod_q_t *in_secret_key,
  eg_element_mod_p_t **out_partial_decryption);

#endif

#ifndef HashedElgamalEncrypt

/**
* Encrypts a message with the Auxiliary Encryption method (as specified in the
* ElectionGuard specification) given a random nonce, an ElGamal public key,
* and an encryption seed. 
*
* @param[in] in_message Message to elgamal_encrypt.
* @param[in] in_length Length of the message.
* @param[in] in_nonce Randomly chosen nonce in [1,Q).
* @param[in] in_hash_prefix Prefix to use in the hash function.
* @param[in] in_public_key ElGamal public key.
* @param[in] in_seed Encryption seed to use in key generation.
* @param[in] in_max_len Indicates the maximum length of plaintext,
*                       must be one of the `HASHED_CIPHERTEXT_PADDED_DATA_SIZE`
*                       enumeration values.
* @param[in] in_allow_truncation Truncates data to the max_len if set to true.
* @param[in] in_use_precompute Whether to use precomputed values for the encryption.
* @param[out] out_ciphertext the ciphertext result.  Caller is responsible for lifecycle.
*/
EG_API eg_electionguard_status_t eg_hashed_elgamal_encrypt(
  uint8_t *in_message, uint64_t in_length, eg_element_mod_q_t *in_nonce, char *in_hash_prefix,
  eg_element_mod_p_t *in_public_key, eg_element_mod_q_t *in_seed,
  enum HASHED_CIPHERTEXT_PADDED_DATA_SIZE in_max_len, bool in_allow_truncation,
  bool in_use_precompute, eg_hashed_elgamal_ciphertext_t **out_ciphertext);

/**
* Encrypts a message with the Auxiliary Encryption method (as specified in the
* ElectionGuard specification) given a random nonce, an ElGamal public key,
* and an encryption seed. 
*
* @param[in] in_message Message to elgamal_encrypt.
* @param[in] in_length Length of the message.
* @param[in] in_nonce Randomly chosen nonce in [1,Q).
* @param[in] in_hash_prefix Prefix to use in the hash function.
* @param[in] in_public_key ElGamal public key.
* @param[in] in_seed Encryption seed to use in key generation.
* @param[in] in_use_precompute Whether to use precomputed values for the encryption.
* @param[out] out_ciphertext the ciphertext result.  Caller is responsible for lifecycle.
*/
EG_API eg_electionguard_status_t eg_hashed_elgamal_encrypt_no_pdding(
  uint8_t *in_message, uint64_t in_length, eg_element_mod_q_t *in_nonce, char *in_hash_prefix,
  eg_element_mod_p_t *in_public_key, eg_element_mod_q_t *in_seed, bool in_use_precompute,
  eg_hashed_elgamal_ciphertext_t **out_ciphertext);

#endif

#ifdef __cplusplus
}
#endif
#endif /* __ELECTIONGUARD_ELGAMAL_H_INCLUDED__ */
