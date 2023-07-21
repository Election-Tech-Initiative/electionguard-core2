#ifndef __ELECTIONGUARD_CPP_HASH_H_INCLUDED__
#define __ELECTIONGUARD_CPP_HASH_H_INCLUDED__

#include "export.h"
#include "group.h"
#include "status.h"

#ifdef __cplusplus
extern "C" {
#endif

struct eg_crypto_hashable_type_s;
typedef struct eg_crypto_hashable_type_s eg_crypto_hashable_type_t;

/**
 * @brief HP = H(HV ;00,p,q,g). Parameter Hash 3.1.2
 */
EG_API const char *eg_hash_get_prefix_parameter_hash();

/**
 * @brief HM = H(HP;01,manifest). Manifest Hash 3.1.4
 */
EG_API const char *eg_hash_get_prefix_manifest_hash();

/**
 * @brief HB = (HP;02,n,k,date,info,HM). Election Base Hash 3.1.5
 */
EG_API const char *eg_hash_get_prefix_base_hash();

/**
 * @brief  H = (HP ; 10, i, j, Ki,j , hi,j ). Guardin Share proof challenge 3.2.2
 */
EG_API const char *eg_hash_get_prefix_guardian_share_challenge();

/**
 * @brief ki,l = H(HP;11,i,l,Kl,αi,l,βi,l). (14) Guardain Share Encryption Secret Key 3.2.2
 */
EG_API const char *eg_hash_get_prefix_guardian_share_secret();

/**
 * @brief HE = H(HB;12,K,K1,0,K1,1,...,K1,k−1,K2,0,...,Kn,k−2,Kn,k−1). Extended Base Hash 3.2.3
 */
EG_API const char *eg_hash_get_prefix_extended_hash();

/**
 * @brief ξi,j = H(HE;20,ξB,Λi,λj). encryption nonce 3.3.2
 */
EG_API const char *eg_hash_get_prefix_selection_nonce();

/**
 * @brief c = H(HE;21,K,α,β,a0,b0,a1,b1,...,aL,bL). Ballot Selection Encryption Proof (ranged) 3.3.5
 * c = H(HE;21,K,α,β,a0,b0,a1,b1). Ballot Selection Encryption Proof (unselected) 3.3.5
 * c = H(HE;21,K,α,β,a0,b0,a1,b1). Ballot Selection Encryption Proof (selected) 3.3.5
 */
EG_API const char *eg_hash_get_prefix_selection_proof();

/**
 * @brief ξ = H (HE ; 20, ξB , Λ, ”contest data”). Ballot Contest Data Nonce 3.3.6
 */
EG_API const char *eg_hash_get_prefix_contest_data_nonce();

/**
 * @brief k = H(HE;22,K,α,β). Ballot ContestData Secret Key 3.3.6
 */
EG_API const char *eg_hash_get_prefix_contest_data_secret();

/**
 * @brief c = H(HE;21,K,α ̄,β ̄,a0,b0,a1,b1,...,aL,bL). Ballot Contest Limit Encryption Proof 3.3.8
 */
EG_API const char *eg_hash_get_prefix_contest_proof();

/**
 * @brief χl = H(HE;23,Λl,K,α1,β1,α2,β2 ...,αm,βm). Contest Hash 3.4.1
 */
EG_API const char *eg_hash_get_prefix_contest_hash();

/**
 * @brief H(B) = H(HE;24,χ1,χ2,...,χmB ,Baux). Confirmation Code 3.4.2
 */
EG_API const char *eg_hash_get_prefix_ballot_code();

/**
 * @brief H0 = H(HE;24,Baux,0), Ballot Chaining for Fixed Device 3.4.3
 * H = H(HE;24,Baux), Ballot chaining closure
 */
EG_API const char *eg_hash_get_prefix_ballot_chain();

/**
 * @brief c = H(HE;30,K,A,B,a,b,M). Ballot Selection Decryption Proof 3.6.3
 * c = H(HE;30,K,α,β,a,b,M). Challenge Ballot Selection Decryption Proof 3.6.5
 */
EG_API const char *eg_hash_get_prefix_decrypt_selection_proof();

/**
 * @brief c = H(HE;31,K,C0,C1,C2,a,b,β). Ballot Contest Decryption of Contest Data 3.6.4
 */
EG_API const char *eg_hash_get_prefix_decrypt_contest_data();

// EG_API eg_electionguard_status_t eg_hash_elems(const char *prefix,
//                                                eg_crypto_hashable_type_t *in_data[],
//                                                uint64_t in_data_size,
//                                                eg_element_mod_q_t **out_handle);

EG_API eg_electionguard_status_t eg_hash_elems_string(const char *a,
                                                      eg_element_mod_q_t **out_handle);

EG_API eg_electionguard_status_t eg_hash_elems_strings(const char **a, uint64_t in_length,
                                                       eg_element_mod_q_t **out_handle);

EG_API eg_electionguard_status_t eg_hash_elems_int(const uint64_t a,
                                                   eg_element_mod_q_t **out_handle);

EG_API eg_electionguard_status_t eg_hash_elems_string_int(char *a, uint64_t b,
                                                          eg_element_mod_q_t **out_handle);

EG_API eg_electionguard_status_t eg_hash_elems_int_int(uint64_t a, uint64_t b,
                                                       eg_element_mod_q_t **out_handle);

EG_API eg_electionguard_status_t eg_hash_elems_string_int_modp(char *a, uint64_t b,
                                                               eg_element_mod_p_t *c,
                                                               eg_element_mod_q_t **out_handle);

EG_API eg_electionguard_status_t eg_hash_elems_string_int_modq(char *a, uint64_t b,
                                                               eg_element_mod_q_t *c,
                                                               eg_element_mod_q_t **out_handle);

EG_API eg_electionguard_status_t eg_hash_elems_modp_modp(eg_element_mod_p_t *a,
                                                         eg_element_mod_p_t *b,
                                                         eg_element_mod_q_t **out_handle);

EG_API eg_electionguard_status_t eg_hash_elems_modp_modq(eg_element_mod_p_t *a,
                                                         eg_element_mod_q_t *b,
                                                         eg_element_mod_q_t **out_handle);

EG_API eg_electionguard_status_t eg_hash_elems_modq_modq(eg_element_mod_q_t *a,
                                                         eg_element_mod_q_t *b,
                                                         eg_element_mod_q_t **out_handle);

EG_API eg_electionguard_status_t eg_hash_elems_modq_modp(eg_element_mod_q_t *a,
                                                         eg_element_mod_p_t *b,
                                                         eg_element_mod_q_t **out_handle);

EG_API eg_electionguard_status_t eg_hash_elems_array(eg_element_mod_p_t *in_data[],
                                                     uint64_t in_data_size,
                                                     eg_element_mod_q_t **out_handle);

#ifdef __cplusplus
}
#endif
#endif /* __ELECTIONGUARD_CPP_HASH_H_INCLUDED__ */
