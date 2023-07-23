#include "../../libs/hacl/Hacl_Bignum256.hpp"
#include "../../src/electionguard/convert.hpp"
#include "../../src/electionguard/facades/bignum4096.hpp"
#include "../../src/electionguard/log.hpp"
#include "../../src/electionguard/utils.hpp"
#include "utils/byte_logger.hpp"

#include <doctest/doctest.h>
#include <electionguard/constants.h>

using namespace std;
using namespace electionguard;
using namespace electionguard::facades;
using namespace hacl;

// This test prints out the standard constants that are loaded into constants.h
// if you wish to use different primes then add a test like this one to calculate
// the constant values and add a conditional override in constants.h
TEST_CASE("Print Standard E.G 1.0 Constants")
{
    Log::debug("\n-------- Print Standard E.G. 1.0 Constants ---------\n");
    Log::debug("Endianness of your system:");
    if (is_little_endian()) {
        Log::debug("Little");
    } else {
        Log::debug("Big");
    }

    // Small Prime

    Log::debug("\n----- small_prime Q -----\n");
    auto q_hex = "FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF43";
    Log::debug(q_hex);

    auto q_bytes = hex_to_bytes(q_hex);
    ByteLogger::print("bytes:", q_bytes);

    // copy the byte array to a bignum representation
    Log::debug("uint64_t array");
    auto q_bignum = copy_bytes_to_bignum_64(q_bytes, MAX_Q_LEN);
    ByteLogger::print("", q_bignum);

    // hacl expects constants to be laid out this way
    Log::debug("uint64_t array (inverted hacl format)");
    auto q_hacl = ElementModQ::fromHex(q_hex, true);
    ByteLogger::print("", q_hacl->get(), MAX_Q_LEN);

    // 2^256 - Q + 1
    // this is specific to the limb offset in hacl
    uint64_t q_inverse_offset[MAX_Q_LEN_DOUBLE] = {};
    auto q_carry = Bignum256::sub(const_cast<uint64_t *>(MAX_256), q_hacl->get(), q_inverse_offset);
    q_carry = Bignum256::add(static_cast<uint64_t *>(q_inverse_offset),
                             const_cast<uint64_t *>(ONE_MOD_Q_ARRAY),
                             static_cast<uint64_t *>(q_inverse_offset));
    ByteLogger::print("hex inverse offset:", q_inverse_offset, MAX_Q_LEN);

    // Large Prime

    Log::debug("\n----- large_prime P -----\n");
    auto p_hex =
      "FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF93C467E37DB0C7A4D1BE3F810152"
      "CB56A1CECC3AF65CC0190C03DF34709AFFBD8E4B59FA03A9F0EED0649CCB621057D11056AE9132135A08E43B4673"
      "D74BAFEA58DEB878CC86D733DBE7BF38154B36CF8A96D1567899AAAE0C09D4C8B6B7B86FD2A1EA1DE62FF8643EC7"
      "C271827977225E6AC2F0BD61C746961542A3CE3BEA5DB54FE70E63E6D09F8FC28658E80567A47CFDE60EE741E5D8"
      "5A7BD46931CED8220365594964B839896FCAABCCC9B31959C083F22AD3EE591C32FAB2C7448F2A057DB2DB49EE52"
      "E0182741E53865F004CC8E704B7C5C40BF304C4D8C4F13EDF6047C555302D2238D8CE11DF2424F1B66C2C5D238D0"
      "744DB679AF2890487031F9C0AEA1C4BB6FE9554EE528FDF1B05E5B256223B2F09215F3719F9C7CCC69DDF172D0D6"
      "234217FCC0037F18B93EF5389130B7A661E5C26E54214068BBCAFEA32A67818BD3075AD1F5C7E9CC3D1737FB2817"
      "1BAF84DBB6612B7881C1A48E439CD03A92BF52225A2B38E6542E9F722BCE15A381B5753EA842763381CCAE83512B"
      "30511B32E5E8D80362149AD030AABA5F3A5798BB22AA7EC1B6D0F17903F4E22D840734AA85973F79A93FFB82A75C"
      "47C03D43D2F9CA02D03199BACEDDD4533A52566AFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF"
      "FFFFFFFFFFFF";
    Log::debug(p_hex);

    auto p_bytes = hex_to_bytes(p_hex);
    ByteLogger::print("bytes:", p_bytes);
    Log::debug("bytes_size:", p_bytes.size());

    Log::debug("uint64_t array");
    auto p_bignum = copy_bytes_to_bignum_64(p_bytes, MAX_P_LEN);
    ByteLogger::print("", p_bignum);

    Log::debug("uint64_t array (hacl inverted format)");
    auto p_hacl = ElementModP::fromHex(p_hex, true);
    ByteLogger::print("", p_hacl->get(), MAX_P_LEN);

    uint64_t p_inverse_offset[MAX_P_LEN_DOUBLE] = {};
    auto p_carry =
      Bignum4096::sub(const_cast<uint64_t *>(MAX_4096), p_hacl->get(), p_inverse_offset);
    p_carry = Bignum4096::add(static_cast<uint64_t *>(p_inverse_offset),
                              const_cast<uint64_t *>(ONE_MOD_P_ARRAY),
                              static_cast<uint64_t *>(p_inverse_offset));
    ByteLogger::print("hex inverse offset:", p_inverse_offset, MAX_P_LEN);

    // Cofactor

    Log::debug("\n----- cofactor R -----\n");
    auto r_hex =
      "0100000000000000000000000000000000000000000000000000000000000000BC93C467E37DB0C7A4D1BE3F8101"
      "52CB56A1CECC3AF65CC0190C03DF34709B8AF6A64C0CEDCF2D559DA9D97F095C3076C686037619148D2C86C31710"
      "2AFA2148031F04440AC0FF0C9A417A89212512E7607B2501DAA4D38A2C1410C4836149E2BDB8C8260E627C464696"
      "3EFFE9E16E495D48BD215C6D8EC9D1667657A2A1C8506F2113FFAD19A6B2BC7C45760456719183309F874BC9ACE5"
      "70FFDA877AA2B23A2D6F291C1554CA2EB12F12CD009B8B8734A64AD51EB893BD891750B85162241D908F0C970987"
      "9758E7E8233EAB3BF2D6AB53AFA32AA153AD6682E5A0648897C9BE18A0D50BECE030C3432336AD9163E33F8E7DAF"
      "498F14BB2852AFFA814841EB18DD5F0E89516D557776285C16071D211194EE1C3F34642036AB886E3EC28882CE40"
      "03DEA335B4D935BAE4B58235B9FB2BAB713C8F705A1C7DE42220209D6BBCACC467318601565272E4A63E38E24997"
      "54AE493AC1A8E83469EEF35CA27C271BC792EEE21156E617B922EA8F713C22CF282DC5D6385BB12868EB781278FA"
      "0AB2A8958FCCB5FFE2E5C361FC174420122B0163CA4A46308C8C46C91EA7457C136A7D9FD4A7F529FD4A7F529FD4"
      "A7F529FD4A7F529FD4A7F529FD4A7F529FD4A7F52A";
    Log::debug(r_hex);

    auto sanitized = sanitize_hex_string(r_hex);
    auto r_bytes = hex_to_bytes(sanitized);
    ByteLogger::print("bytes:", r_bytes);

    Log::debug("uint64_t array");
    // TODO: use Bignum4096/256 fo this operation and delete this method
    auto r_bignum = copy_bytes_to_bignum_64(r_bytes, MAX_P_LEN);
    ByteLogger::print("", r_bignum);

    Log::debug("uint64_t array (hacl inverted format)");
    auto r_hacl = ElementModP::fromHex(r_hex, true);
    ByteLogger::print("", r_hacl->get(), MAX_P_LEN);

    // Generator

    Log::debug("\n----- generator G -----\n");
    auto g_hex =
      "1D41E49C477E15EAEEF0C5E4AC08D4A46C268CD3424FC01D13769BDB43673218587BC86C4C1448D006A03699F3AB"
      "AE5FEB19E296F5D143CC5E4A3FC89088C9F4523D166EE3AE9D5FB03C0BDD77ADD5C017F6C55E2EC92C226FEF5C6C"
      "1DF2E7C36D90E7EAADE098241D3409983BCCD2B5379E9391FBC62F9F8D939D1208B160367C134264122189595EC8"
      "5C8CDBE5F9D307F46912C04932F8C16815A76B4682BD6BDC0ED52B00D8D30F59C731D5A7FFAE8165D53CF96649AA"
      "C2B743DA56F14F19DACC5236F29B1AB9F9BEFC69697293D5DEAD8B5BF5DE9BAB6DE67C45719E56344A3CBDF36098"
      "24B1B578E34EAEB6DD3190AB3571D6D671C512282C1DA7BD36B4251D2584FADEA80B9E141423074DD9B5FB83ACBD"
      "EAD4C87A58FFF517F977A83080370A3B0CF98A1BC2978C47AAC29611FD6C40E2F9875C35D50443A9AA3F49611DCD"
      "3A0D6FF3CB3FACF31471BDB61860B92C594D4E46569BB39FEEADFF1FD64C836A6D6DB85C6BA7241766B7AB56BF73"
      "9633B054147F7170921412E948D9E47402D15BB1C257318612C121C36B80EB8433C08E7D0B7149E3AB0A8735A92E"
      "DCE8FF943E28A2DCEACFCC69EC318909CB047BE1C5858844B5AD44F22EEB289E4CC554F7A5E2F3DEA026877FF928"
      "51816071CE028EB868D965CCB2D2295A8C55BD1C070B39B09AE06B37D29343B9D8997DC244C468B980970731736E"
      "E018BBADB987";
    Log::debug(g_hex);

    auto g_bytes = hex_to_bytes(g_hex);
    ByteLogger::print("bytes:", g_bytes);

    Log::debug("uint64_t array");
    auto g_bignum = copy_bytes_to_bignum_64(g_bytes, MAX_P_LEN);
    ByteLogger::print("", g_bignum);

    Log::debug("uint64_t array (hacl inverted format)");
    auto g_hacl = ElementModP::fromHex(g_hex, true);
    ByteLogger::print("", g_hacl->get(), MAX_P_LEN);
}

TEST_CASE("Print Standard E.G 2.0 Constants")
{
    Log::debug("\n-------- Print Standard E.G. 2.0 Constants ---------\n");
    Log::debug("Endianness of your system:");
    if (is_little_endian()) {
        Log::debug("Little");
    } else {
        Log::debug("Big");
    }

    // Small Prime

    Log::debug("\n----- small_prime Q -----\n");
    auto q_hex = "FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF43";
    Log::debug(q_hex);

    auto q_bytes = hex_to_bytes(q_hex);
    ByteLogger::print("bytes:", q_bytes);

    // copy the byte array to a bignum representation
    Log::debug("uint64_t array");
    auto q_bignum = copy_bytes_to_bignum_64(q_bytes, MAX_Q_LEN);
    ByteLogger::print("", q_bignum);

    // hacl expects constants to be laid out this way
    Log::debug("uint64_t array (inverted hacl format)");
    auto q_hacl = ElementModQ::fromHex(q_hex, true);
    ByteLogger::print("", q_hacl->get(), MAX_Q_LEN);

    // 2^256 - Q + 1
    // this is specific to the limb offset in hacl
    uint64_t q_inverse_offset[MAX_Q_LEN_DOUBLE] = {};
    auto q_carry = Bignum256::sub(const_cast<uint64_t *>(MAX_256), q_hacl->get(), q_inverse_offset);
    q_carry = Bignum256::add(static_cast<uint64_t *>(q_inverse_offset),
                             const_cast<uint64_t *>(ONE_MOD_Q_ARRAY),
                             static_cast<uint64_t *>(q_inverse_offset));
    ByteLogger::print("hex inverse offset:", q_inverse_offset, MAX_Q_LEN);

    // Large Prime

    Log::debug("\n----- large_prime P -----\n");
    auto p_hex =
      "FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFB17217F7D1CF79ABC9E3B39803F2"
      "F6AF40F343267298B62D8A0D175B8BAAFA2BE7B876206DEBAC98559552FB4AFA1B10ED2EAE35C138214427573B29"
      "1169B8253E96CA16224AE8C51ACBDA11317C387EB9EA9BC3B136603B256FA0EC7657F74B72CE87B19D6548CAF5DF"
      "A6BD38303248655FA1872F20E3A2DA2D97C50F3FD5C607F4CA11FB5BFB90610D30F88FE551A2EE569D6DFC1EFA15"
      "7D2E23DE1400B39617460775DB8990E5C943E732B479CD33CCCC4E659393514C4C1A1E0BD1D6095D25669B333564"
      "A3376A9C7F8A5E148E82074DB6015CFE7AA30C480A5417350D2C955D5179B1E17B9DAE313CDB6C606CB1078F735D"
      "1B2DB31B5F50B5185064C18B4D162DB3B365853D7598A1951AE273EE5570B6C68F96983496D4E6D330AF889B44A0"
      "2554731CDC8EA17293D1228A4EF98D6F5177FBCF0755268A5C1F9538B98261AFFD446B1CA3CF5E9222B88C66D3C5"
      "422183EDC99421090BBB16FAF3D949F236E02B20CEE886B905C128D53D0BD2F9621363196AF503020060E4990839"
      "1A0C57339BA2BEBA7D052AC5B61CC4E9207CEF2F0CE2D7373958D762265890445744FB5F2DA4B751005892D35689"
      "0DEFE9CAD9B9D4B713E06162A2D8FDD0DF2FD608FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF"
      "FFFFFFFFFFFF";
    Log::debug(p_hex);

    auto p_bytes = hex_to_bytes(p_hex);
    ByteLogger::print("bytes:", p_bytes);
    Log::debug("bytes_size:", p_bytes.size());

    Log::debug("uint64_t array");
    auto p_bignum = copy_bytes_to_bignum_64(p_bytes, MAX_P_LEN);
    ByteLogger::print("", p_bignum);

    Log::debug("uint64_t array (hacl inverted format)");
    auto p_hacl = ElementModP::fromHex(p_hex, true);
    ByteLogger::print("", p_hacl->get(), MAX_P_LEN);

    uint64_t p_inverse_offset[MAX_P_LEN_DOUBLE] = {};
    auto p_carry =
      Bignum4096::sub(const_cast<uint64_t *>(MAX_4096), p_hacl->get(), p_inverse_offset);
    p_carry = Bignum4096::add(static_cast<uint64_t *>(p_inverse_offset),
                              const_cast<uint64_t *>(ONE_MOD_P_ARRAY),
                              static_cast<uint64_t *>(p_inverse_offset));
    ByteLogger::print("hex inverse offset:", p_inverse_offset, MAX_P_LEN);

    // Cofactor

    Log::debug("\n----- cofactor R -----\n");
    auto r_hex =
      "0100000000000000000000000000000000000000000000000000000000000000BCB17217F7D1CF79ABC9E3B39803"
      "F2F6AF40F343267298B62D8A0D175B8BAB857AE8F428165418806C62B0EA36355A3A73E0C741985BF6A0E3130179"
      "BF2F0B43E33AD862923861B8C9F768C4169519600BAD06093F964B27E02D86831231A9160DE48F4DA53D8AB5E69E"
      "386B694BEC1AE722D47579249D5424767C5C33B9151E07C5C11D106AC446D330B47DB59D352E47A53157DE044619"
      "00F6FE360DB897DF5316D87C94AE71DAD0BE84B647C4BCF818C23A2D4EBB53C702A5C8062D19F5E9B5033A94F7FF"
      "732F54129712869D97B8C96C412921A9D8679770F499A041C297CFF79D4C9149EB6CAF67B9EA3DC563D965F3AAD1"
      "377FF22DE9C3E62068DD0ED6151C37B4F74634C2BD09DA912FD599F4333A8D2CC005627DCA37BAD43E64A3963119"
      "C0BFE34810A21EE7CFC421D53398CBC7A95B3BF585E5A04B790E2FE1FE9BC264FDA8109F6454A082F5EFB2F37EA2"
      "37AA29DF320D6EA860C41A9054CCD24876C6253F667BFB0139B5531FF30189961202FD2B0D55A75272C7FD73343F"
      "7899BCA0B36A4C470A64A009244C84E77CEBC92417D5BB13BF18167D8033EB6C4DD7879FD4A7F529FD4A7F529FD4"
      "A7F529FD4A7F529FD4A7F529FD4A7F529FD4A7F52A";
    Log::debug(r_hex);

    auto sanitized = sanitize_hex_string(r_hex);
    auto r_bytes = hex_to_bytes(sanitized);
    ByteLogger::print("bytes:", r_bytes);

    Log::debug("uint64_t array");
    // TODO: use Bignum4096/256 fo this operation and delete this method
    auto r_bignum = copy_bytes_to_bignum_64(r_bytes, MAX_P_LEN);
    ByteLogger::print("", r_bignum);

    Log::debug("uint64_t array (hacl inverted format)");
    auto r_hacl = ElementModP::fromHex(r_hex, true);
    ByteLogger::print("", r_hacl->get(), MAX_P_LEN);

    // Generator

    Log::debug("\n----- generator G -----\n");
    auto g_hex =
      "36036FED214F3B50DC566D3A312FE4131FEE1C2BCE6D02EA39B477AC05F7F885F38CFE77A7E45ACF4029114C4D7A"
      "9BFE058BF2F995D2479D3DDA618FFD910D3C4236AB2CFDD783A5016F7465CF59BBF45D24A22F130F2D04FE93B2D5"
      "8BB9C1D1D27FC9A17D2AF49A779F3FFBDCA22900C14202EE6C99616034BE35CBCDD3E7BB7996ADFE534B63CCA41E"
      "21FF5DC778EBB1B86C53BFBE99987D7AEA0756237FB40922139F90A62F2AA8D9AD34DFF799E33C857A6468D001AC"
      "F3B681DB87DC4242755E2AC5A5027DB81984F033C4D178371F273DBB4FCEA1E628C23E52759BC7765728035CEA26"
      "B44C49A65666889820A45C33DD37EA4A1D00CB62305CD541BE1E8A92685A07012B1A20A746C3591A2DB3815000D2"
      "AACCFE43DC49E828C1ED7387466AFD8E4BF1935593B2A442EEC271C50AD39F733797A1EA11802A2557916534662A"
      "6B7E9A9E449A24C8CFF809E79A4D806EB681119330E6C57985E39B200B4893639FDFDEA49F76AD1ACD997EBA1365"
      "7541E79EC57437E504EDA9DD011061516C643FB30D6D58AFCCD28B73FEDA29EC12B01A5EB86399A593A9D5F450DE"
      "39CB92962C5EC6925348DB54D128FD99C14B457F883EC20112A75A6A0581D3D80A3B4EF09EC86F9552FFDA1653F1"
      "33AA2534983A6F31B0EE4697935A6B1EA2F75B85E7EBA151BA486094D68722B054633FEC51CA3F29B31E77E317B1"
      "78B6B9D8AE0F";
    Log::debug(g_hex);

    auto g_bytes = hex_to_bytes(g_hex);
    ByteLogger::print("bytes:", g_bytes);

    Log::debug("uint64_t array");
    auto g_bignum = copy_bytes_to_bignum_64(g_bytes, MAX_P_LEN);
    ByteLogger::print("", g_bignum);

    Log::debug("uint64_t array (hacl inverted format)");
    auto g_hacl = ElementModP::fromHex(g_hex, true);
    ByteLogger::print("", g_hacl->get(), MAX_P_LEN);
}

TEST_CASE("Print Test Constants")
{
    Log::debug("\n-------- Print TEST Constants ---------\n");
    Log::debug("Endianness of your system:");
    if (is_little_endian()) {
        Log::debug("Little");
    } else {
        Log::debug("Big");
    }

    // Small Prime

    Log::debug("\n----- small_prime Q -----\n");
    auto q_hex = "FFF1";
    Log::debug(q_hex);

    auto q_bytes = hex_to_bytes(q_hex);
    ByteLogger::print("bytes:", q_bytes);

    Log::debug("uint64_t array");
    auto q_bignum = copy_bytes_to_bignum_64(q_bytes, MAX_Q_LEN);
    ByteLogger::print("", q_bignum);

    Log::debug("uint64_t array (inverted hacl format)");
    auto q_hacl = ElementModQ::fromHex(q_hex, true);
    ByteLogger::print("", q_hacl->get(), MAX_Q_LEN);

    // 2^256 - Q + 1
    uint64_t q_inverse_offset[MAX_Q_LEN_DOUBLE] = {};
    auto q_carry = Bignum256::sub(const_cast<uint64_t *>(MAX_256), q_hacl->get(), q_inverse_offset);
    q_carry = Bignum256::add(static_cast<uint64_t *>(q_inverse_offset),
                             const_cast<uint64_t *>(ONE_MOD_Q_ARRAY),
                             static_cast<uint64_t *>(q_inverse_offset));
    ByteLogger::print("hex inverse offset:", q_inverse_offset, MAX_Q_LEN);

    // Large Prime

    Log::debug("\n----- large_prime P -----\n");
    auto p_hex = "FFFFFFFFFFB43EA5";
    Log::debug(p_hex);

    auto p_bytes = hex_to_bytes(p_hex);
    ByteLogger::print("bytes:", p_bytes);

    Log::debug("uint64_t array");
    auto p_bignum = copy_bytes_to_bignum_64(p_bytes, MAX_P_LEN);
    ByteLogger::print("", p_bignum);

    Log::debug("uint64_t array (hacl inverted format)");
    auto p_hacl = ElementModP::fromHex(p_hex, true);
    ByteLogger::print("", p_hacl->get(), MAX_P_LEN);

    uint64_t p_inverse_offset[MAX_P_LEN_DOUBLE] = {};
    auto p_carry =
      Bignum4096::sub(const_cast<uint64_t *>(MAX_4096), p_hacl->get(), p_inverse_offset);
    p_carry = Bignum4096::add(static_cast<uint64_t *>(p_inverse_offset),
                              const_cast<uint64_t *>(ONE_MOD_P_ARRAY),
                              static_cast<uint64_t *>(p_inverse_offset));
    ByteLogger::print("hex inverse offset:", p_inverse_offset, MAX_P_LEN);

    // Cofactor

    Log::debug("\n----- cofactor R -----\n");
    auto r_hex = "01000F00E10CE4";
    Log::debug(r_hex);

    auto r_bytes = hex_to_bytes(r_hex);
    ByteLogger::print("bytes:", r_bytes);

    Log::debug("uint64_t array");
    auto r_bignum = copy_bytes_to_bignum_64(r_bytes, MAX_P_LEN);
    ByteLogger::print("", r_bignum);

    Log::debug("uint64_t array (hacl inverted format)");
    auto r_hacl = ElementModP::fromHex(r_hex, true);
    ByteLogger::print("", r_hacl->get(), MAX_P_LEN);

    // Generator

    Log::debug("\n----- generator G -----\n");
    auto g_hex = "D6982759F3D5107E";
    Log::debug(g_hex);

    auto g_bytes = hex_to_bytes(g_hex);
    ByteLogger::print("bytes:", g_bytes);

    Log::debug("uint64_t array");
    auto g_bignum = copy_bytes_to_bignum_64(g_bytes, MAX_P_LEN);
    ByteLogger::print("", g_bignum);

    Log::debug("uint64_t array (hacl inverted format)");
    auto g_hacl = ElementModP::fromHex(g_hex, true);
    ByteLogger::print("", g_hacl->get(), MAX_P_LEN);
}
