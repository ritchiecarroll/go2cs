namespace go.modernc.org;

using @unsafe = unsafe_package;

partial class libc_package {

internal static @unsafe.Pointer _ᴛ1ʗ;

public static partial void Y_Exit(ж<TLS> tls, int32 ec);

public static partial int32 /*r*/ Y_IO_feof_unlocked(ж<TLS> tls, uintptr f);

public static partial int32 /*r*/ Y_IO_ferror_unlocked(ж<TLS> tls, uintptr f);

public static partial int32 /*r*/ Y_IO_getc(ж<TLS> tls, uintptr f1);

public static partial int32 /*r*/ Y_IO_getc_unlocked(ж<TLS> tls, uintptr f);

public static partial int32 /*r*/ Y_IO_putc(ж<TLS> tls, int32 c1, uintptr f1);

public static partial int32 /*r*/ Y_IO_putc_unlocked(ж<TLS> tls, int32 c, uintptr f);

public static partial uintptr /*r*/ Y___errno_location(ж<TLS> tls);

public static partial int32 /*_2*/ Y__aio_close(ж<TLS> tls, int32 fd);

public static partial uintptr /*r*/ Y__asctime_r(ж<TLS> tls, uintptr tm, uintptr buf);

public static partial void Y__assert_fail(ж<TLS> tls, uintptr expr, uintptr @file, int32 line, uintptr func1);

public static partial int32 /*_3*/ Y__atomic_compare_exchangeInt16(ж<TLS> t, uintptr ptr, uintptr expected, uintptr desired, int32 weak, int32 success, int32 failure);

public static partial int32 /*_3*/ Y__atomic_compare_exchangeInt32(ж<TLS> t, uintptr ptr, uintptr expected, uintptr desired, int32 weak, int32 success, int32 failure);

public static partial int32 /*_3*/ Y__atomic_compare_exchangeInt64(ж<TLS> t, uintptr ptr, uintptr expected, uintptr desired, int32 weak, int32 success, int32 failure);

public static partial int32 /*_3*/ Y__atomic_compare_exchangeInt8(ж<TLS> t, uintptr ptr, uintptr expected, uintptr desired, int32 weak, int32 success, int32 failure);

public static partial int32 /*_3*/ Y__atomic_compare_exchangeUint16(ж<TLS> t, uintptr ptr, uintptr expected, uintptr desired, int32 weak, int32 success, int32 failure);

public static partial int32 /*_3*/ Y__atomic_compare_exchangeUint32(ж<TLS> t, uintptr ptr, uintptr expected, uintptr desired, int32 weak, int32 success, int32 failure);

public static partial int32 /*_3*/ Y__atomic_compare_exchangeUint64(ж<TLS> t, uintptr ptr, uintptr expected, uintptr desired, int32 weak, int32 success, int32 failure);

public static partial int32 /*_3*/ Y__atomic_compare_exchangeUint8(ж<TLS> t, uintptr ptr, uintptr expected, uintptr desired, int32 weak, int32 success, int32 failure);

public static partial void Y__atomic_exchangeInt16(ж<TLS> t, uintptr ptr, uintptr val, uintptr ret, int32 _);

public static partial void Y__atomic_exchangeInt32(ж<TLS> t, uintptr ptr, uintptr val, uintptr ret, int32 _);

public static partial void Y__atomic_exchangeInt64(ж<TLS> t, uintptr ptr, uintptr val, uintptr ret, int32 _);

public static partial void Y__atomic_exchangeInt8(ж<TLS> t, uintptr ptr, uintptr val, uintptr ret, int32 _);

public static partial void Y__atomic_exchangeUint16(ж<TLS> t, uintptr ptr, uintptr val, uintptr ret, int32 _);

public static partial void Y__atomic_exchangeUint32(ж<TLS> t, uintptr ptr, uintptr val, uintptr ret, int32 _);

public static partial void Y__atomic_exchangeUint64(ж<TLS> t, uintptr ptr, uintptr val, uintptr ret, int32 _);

public static partial void Y__atomic_exchangeUint8(ж<TLS> t, uintptr ptr, uintptr val, uintptr ret, int32 _);

public static partial int16 /*r*/ Y__atomic_fetch_addInt16(ж<TLS> t, uintptr ptr, int16 val, int32 _);

public static partial int32 /*r*/ Y__atomic_fetch_addInt32(ж<TLS> t, uintptr ptr, int32 val, int32 _);

public static partial int64 /*r*/ Y__atomic_fetch_addInt64(ж<TLS> t, uintptr ptr, int64 val, int32 _);

public static partial int8 /*r*/ Y__atomic_fetch_addInt8(ж<TLS> t, uintptr ptr, int8 val, int32 _);

public static partial uint16 /*r*/ Y__atomic_fetch_addUint16(ж<TLS> t, uintptr ptr, uint16 val, int32 _);

public static partial uint32 /*r*/ Y__atomic_fetch_addUint32(ж<TLS> t, uintptr ptr, uint32 val, int32 _);

public static partial uint64 /*r*/ Y__atomic_fetch_addUint64(ж<TLS> t, uintptr ptr, uint64 val, int32 _);

public static partial uint8 /*r*/ Y__atomic_fetch_addUint8(ж<TLS> t, uintptr ptr, uint8 val, int32 _);

public static partial int16 /*r*/ Y__atomic_fetch_andInt16(ж<TLS> t, uintptr ptr, int16 val, int32 _);

public static partial int32 /*r*/ Y__atomic_fetch_andInt32(ж<TLS> t, uintptr ptr, int32 val, int32 _);

public static partial int64 /*r*/ Y__atomic_fetch_andInt64(ж<TLS> t, uintptr ptr, int64 val, int32 _);

public static partial int8 /*r*/ Y__atomic_fetch_andInt8(ж<TLS> t, uintptr ptr, int8 val, int32 _);

public static partial uint16 /*r*/ Y__atomic_fetch_andUint16(ж<TLS> t, uintptr ptr, uint16 val, int32 _);

public static partial uint32 /*r*/ Y__atomic_fetch_andUint32(ж<TLS> t, uintptr ptr, uint32 val, int32 _);

public static partial uint64 /*r*/ Y__atomic_fetch_andUint64(ж<TLS> t, uintptr ptr, uint64 val, int32 _);

public static partial uint8 /*r*/ Y__atomic_fetch_andUint8(ж<TLS> t, uintptr ptr, uint8 val, int32 _);

public static partial int16 /*r*/ Y__atomic_fetch_orInt16(ж<TLS> t, uintptr ptr, int16 val, int32 _);

public static partial int32 /*r*/ Y__atomic_fetch_orInt32(ж<TLS> t, uintptr ptr, int32 val, int32 _);

public static partial int64 /*r*/ Y__atomic_fetch_orInt64(ж<TLS> t, uintptr ptr, int64 val, int32 _);

public static partial int8 /*r*/ Y__atomic_fetch_orInt8(ж<TLS> t, uintptr ptr, int8 val, int32 _);

public static partial uint16 /*r*/ Y__atomic_fetch_orUint16(ж<TLS> t, uintptr ptr, uint16 val, int32 _);

public static partial uint32 /*r*/ Y__atomic_fetch_orUint32(ж<TLS> t, uintptr ptr, uint32 val, int32 _);

public static partial uint64 /*r*/ Y__atomic_fetch_orUint64(ж<TLS> t, uintptr ptr, uint64 val, int32 _);

public static partial uint8 /*r*/ Y__atomic_fetch_orUint8(ж<TLS> t, uintptr ptr, uint8 val, int32 _);

public static partial int16 /*r*/ Y__atomic_fetch_subInt16(ж<TLS> t, uintptr ptr, int16 val, int32 _);

public static partial int32 /*r*/ Y__atomic_fetch_subInt32(ж<TLS> t, uintptr ptr, int32 val, int32 _);

public static partial int64 /*r*/ Y__atomic_fetch_subInt64(ж<TLS> t, uintptr ptr, int64 val, int32 _);

public static partial int8 /*r*/ Y__atomic_fetch_subInt8(ж<TLS> t, uintptr ptr, int8 val, int32 _);

public static partial uint16 /*r*/ Y__atomic_fetch_subUint16(ж<TLS> t, uintptr ptr, uint16 val, int32 _);

public static partial uint32 /*r*/ Y__atomic_fetch_subUint32(ж<TLS> t, uintptr ptr, uint32 val, int32 _);

public static partial uint64 /*r*/ Y__atomic_fetch_subUint64(ж<TLS> t, uintptr ptr, uint64 val, int32 _);

public static partial uint8 /*r*/ Y__atomic_fetch_subUint8(ж<TLS> t, uintptr ptr, uint8 val, int32 _);

public static partial int16 /*r*/ Y__atomic_fetch_xorInt16(ж<TLS> t, uintptr ptr, int16 val, int32 _);

public static partial int32 /*r*/ Y__atomic_fetch_xorInt32(ж<TLS> t, uintptr ptr, int32 val, int32 _);

public static partial int64 /*r*/ Y__atomic_fetch_xorInt64(ж<TLS> t, uintptr ptr, int64 val, int32 _);

public static partial int8 /*r*/ Y__atomic_fetch_xorInt8(ж<TLS> t, uintptr ptr, int8 val, int32 _);

public static partial uint16 /*r*/ Y__atomic_fetch_xorUint16(ж<TLS> t, uintptr ptr, uint16 val, int32 _);

public static partial uint32 /*r*/ Y__atomic_fetch_xorUint32(ж<TLS> t, uintptr ptr, uint32 val, int32 _);

public static partial uint64 /*r*/ Y__atomic_fetch_xorUint64(ж<TLS> t, uintptr ptr, uint64 val, int32 _);

public static partial uint8 /*r*/ Y__atomic_fetch_xorUint8(ж<TLS> t, uintptr ptr, uint8 val, int32 _);

public static partial void Y__atomic_loadInt16(ж<TLS> t, uintptr ptr, uintptr ret, int32 memorder);

public static partial void Y__atomic_loadInt32(ж<TLS> t, uintptr ptr, uintptr ret, int32 memorder);

public static partial void Y__atomic_loadInt64(ж<TLS> t, uintptr ptr, uintptr ret, int32 memorder);

public static partial void Y__atomic_loadInt8(ж<TLS> t, uintptr ptr, uintptr ret, int32 memorder);

public static partial void Y__atomic_loadUint16(ж<TLS> t, uintptr ptr, uintptr ret, int32 memorder);

public static partial void Y__atomic_loadUint32(ж<TLS> t, uintptr ptr, uintptr ret, int32 memorder);

public static partial void Y__atomic_loadUint64(ж<TLS> t, uintptr ptr, uintptr ret, int32 memorder);

public static partial void Y__atomic_loadUint8(ж<TLS> t, uintptr ptr, uintptr ret, int32 memorder);

public static partial void Y__atomic_storeInt16(ж<TLS> t, uintptr ptr, uintptr val, int32 memorder);

public static partial void Y__atomic_storeInt32(ж<TLS> t, uintptr ptr, uintptr val, int32 memorder);

public static partial void Y__atomic_storeInt64(ж<TLS> t, uintptr ptr, uintptr val, int32 memorder);

public static partial void Y__atomic_storeInt8(ж<TLS> t, uintptr ptr, uintptr val, int32 memorder);

public static partial void Y__atomic_storeUint16(ж<TLS> t, uintptr ptr, uintptr val, int32 memorder);

public static partial void Y__atomic_storeUint32(ж<TLS> t, uintptr ptr, uintptr val, int32 memorder);

public static partial void Y__atomic_storeUint64(ж<TLS> t, uintptr ptr, uintptr val, int32 memorder);

public static partial void Y__atomic_storeUint8(ж<TLS> t, uintptr ptr, uintptr val, int32 memorder);

public static partial void Y__block_all_sigs(ж<TLS> tls, uintptr set);

public static partial void Y__block_app_sigs(ж<TLS> tls, uintptr set);

public static partial uintptr /*r*/ Y__builtin___memcpy_chk(ж<TLS> t, uintptr dest, uintptr src, Tsize_t n, Tsize_t os);

public static partial uintptr /*_3*/ Y__builtin___memmove_chk(ж<TLS> t, uintptr dest, uintptr src, Tsize_t n, Tsize_t os);

public static partial uintptr /*_4*/ Y__builtin___memset_chk(ж<TLS> t, uintptr s, int32 c, Tsize_t n, Tsize_t os);

public static partial int32 /*r*/ Y__builtin___snprintf_chk(ж<TLS> t, uintptr str, Tsize_t maxlen, int32 flag, Tsize_t os, uintptr format, uintptr args);

public static partial int32 /*r*/ Y__builtin___sprintf_chk(ж<TLS> t, uintptr s, int32 flag, Tsize_t os, uintptr format, uintptr args);

public static partial uintptr /*r*/ Y__builtin___strcat_chk(ж<TLS> t, uintptr dest, uintptr src, Tsize_t os);

public static partial uintptr /*_3*/ Y__builtin___strcpy_chk(ж<TLS> t, uintptr dest, uintptr src, Tsize_t os);

public static partial uintptr /*r*/ Y__builtin___strncpy_chk(ж<TLS> t, uintptr dest, uintptr src, Tsize_t n, Tsize_t os);

public static partial int32 /*r*/ Y__builtin___vsnprintf_chk(ж<TLS> t, uintptr str, Tsize_t maxlen, int32 flag, Tsize_t os, uintptr format, uintptr args);

public static partial void Y__builtin_abort(ж<TLS> t);

public static partial int32 /*_2*/ Y__builtin_abs(ж<TLS> t, int32 j);

public static partial int32 /*_3*/ Y__builtin_add_overflowInt64(ж<TLS> t, int64 a, int64 b, uintptr res);

public static partial int32 /*_3*/ Y__builtin_add_overflowUint32(ж<TLS> t, uint32 a, uint32 b, uintptr res);

public static partial int32 /*_3*/ Y__builtin_add_overflowUint64(ж<TLS> t, uint64 a, uint64 b, uintptr res);

public static partial uintptr /*_2*/ Y__builtin_alloca(ж<TLS> tls, Tsize_t size);

public static partial uint16 /*_2*/ Y__builtin_bswap16(ж<TLS> t, uint16 x);

public static partial uint32 /*_2*/ Y__builtin_bswap32(ж<TLS> t, uint32 x);

public static partial uint64 /*_2*/ Y__builtin_bswap64(ж<TLS> t, uint64 x);

public static partial void Y__builtin_bzero(ж<TLS> t, uintptr s, Tsize_t n);

public static partial int32 /*_2*/ Y__builtin_clz(ж<TLS> t, uint32 n);

public static partial int32 /*_2*/ Y__builtin_clzl(ж<TLS> t, @ulong n);

public static partial int32 /*_2*/ Y__builtin_clzll(ж<TLS> t, uint64 n);

public static partial float64 /*_2*/ Y__builtin_copysign(ж<TLS> t, float64 x, float64 y);

public static partial float32 /*_2*/ Y__builtin_copysignf(ж<TLS> t, float32 x, float32 y);

public static partial float64 /*_2*/ Y__builtin_copysignl(ж<TLS> t, float64 x, float64 y);

public static partial int32 /*_2*/ Y__builtin_ctz(ж<TLS> t, uint32 n);

public static partial int32 /*_2*/ Y__builtin_ctzl(ж<TLS> tls, @ulong x);

public static partial void Y__builtin_exit(ж<TLS> t, int32 status);

public static partial @long /*_2*/ Y__builtin_expect(ж<TLS> t, @long exp, @long c);

public static partial float64 /*_2*/ Y__builtin_fabs(ж<TLS> t, float64 x);

public static partial float32 /*_2*/ Y__builtin_fabsf(ж<TLS> t, float32 x);

public static partial float64 /*_2*/ Y__builtin_fabsl(ж<TLS> t, float64 x);

public static partial int32 /*r*/ Y__builtin_ffs(ж<TLS> tls, int32 i);

public static partial float64 /*r*/ Y__builtin_fma(ж<TLS> tls, float64 x, float64 y, float64 z);

public static partial float64 /*r*/ Y__builtin_fmax(ж<TLS> tls, float64 x, float64 y);

public static partial float64 /*r*/ Y__builtin_fmin(ж<TLS> tls, float64 x, float64 y);

public static partial void Y__builtin_free(ж<TLS> t, uintptr ptr);

public static partial int32 /*_3*/ Y__builtin_getentropy(ж<TLS> t, uintptr buf, Tsize_t n);

public static partial float64 /*_1*/ Y__builtin_huge_val(ж<TLS> t);

public static partial float32 /*_1*/ Y__builtin_huge_valf(ж<TLS> t);

public static partial float64 /*r*/ Y__builtin_hypot(ж<TLS> tls, float64 x, float64 y);

public static partial float64 /*_1*/ Y__builtin_inf(ж<TLS> t);

public static partial float32 /*_1*/ Y__builtin_inff(ж<TLS> tls);

public static partial float64 /*_1*/ Y__builtin_infl(ж<TLS> t);

public static partial int32 /*r*/ Y__builtin_isblank(ж<TLS> tls, int32 c);

public static partial int32 /*_2*/ Y__builtin_isnan(ж<TLS> t, float64 x);

public static partial int32 /*_2*/ Y__builtin_isnanf(ж<TLS> t, float32 x);

public static partial int32 /*_2*/ Y__builtin_isnanl(ж<TLS> t, float64 x);

public static partial int32 /*r*/ Y__builtin_isprint(ж<TLS> tls, int32 c);

public static partial int32 /*_2*/ Y__builtin_isunordered(ж<TLS> t, float64 a, float64 b);

public static partial int64 /*_2*/ Y__builtin_llabs(ж<TLS> tls, int64 a);

public static partial float64 /*_2*/ Y__builtin_log2(ж<TLS> t, float64 x);

public static partial @long /*r*/ Y__builtin_lrint(ж<TLS> tls, float64 x);

public static partial @long /*r*/ Y__builtin_lrintf(ж<TLS> tls, float32 x);

public static partial @long /*r*/ Y__builtin_lround(ж<TLS> tls, float64 x);

public static partial uintptr /*_2*/ Y__builtin_malloc(ж<TLS> t, Tsize_t size);

public static partial int32 /*_3*/ Y__builtin_memcmp(ж<TLS> t, uintptr s1, uintptr s2, Tsize_t n);

public static partial uintptr /*r*/ Y__builtin_memcpy(ж<TLS> t, uintptr dest, uintptr src, Tsize_t n);

public static partial uintptr /*_4*/ Y__builtin_memset(ж<TLS> t, uintptr s, int32 c, Tsize_t n);

public static partial uintptr /*_5*/ Y__builtin_mmap(ж<TLS> t, uintptr addr, Tsize_t length, int32 prot, int32 flags, int32 fd, Toff_t offset);

public static partial int32 /*_3*/ Y__builtin_mul_overflowInt64(ж<TLS> t, int64 a, int64 b, uintptr res);

public static partial int32 /*_3*/ Y__builtin_mul_overflowUint128(ж<TLS> t, Uint128 a, Uint128 b, uintptr res);

public static partial int32 /*_3*/ Y__builtin_mul_overflowUint64(ж<TLS> t, uint64 a, uint64 b, uintptr res);

public static partial float64 /*_2*/ Y__builtin_nan(ж<TLS> t, uintptr s);

public static partial float32 /*_2*/ Y__builtin_nanf(ж<TLS> tls, uintptr s);

public static partial float64 /*_2*/ Y__builtin_nanl(ж<TLS> t, uintptr s);

public static partial Tsize_t /*_3*/ Y__builtin_object_size(ж<TLS> t, uintptr p, int32 typ);

public static partial int32 /*_2*/ Y__builtin_popcount(ж<TLS> t, uint32 x);

public static partial int32 /*_2*/ Y__builtin_popcountl(ж<TLS> t, @ulong x);

public static partial int32 /*_2*/ Y__builtin_popcountll(ж<TLS> t, uint64 x);

public static partial void Y__builtin_prefetch(ж<TLS> t, uintptr addr, uintptr args);

public static partial int32 /*r*/ Y__builtin_printf(ж<TLS> tls, uintptr fmt, uintptr va);

public static partial float32 /*r*/ Y__builtin_rintf(ж<TLS> tls, float32 x);

public static partial float64 /*r*/ Y__builtin_round(ж<TLS> tls, float64 x);

public static partial float32 /*r*/ Y__builtin_roundf(ж<TLS> tls, float32 x);

public static partial int32 /*_4*/ Y__builtin_snprintf(ж<TLS> t, uintptr str, Tsize_t size, uintptr format, uintptr args);

public static partial int32 /*r*/ Y__builtin_sprintf(ж<TLS> t, uintptr str, uintptr format, uintptr args);

public static partial uintptr /*_3*/ Y__builtin_strchr(ж<TLS> t, uintptr s, int32 c);

public static partial int32 /*_2*/ Y__builtin_strcmp(ж<TLS> t, uintptr s1, uintptr s2);

public static partial uintptr /*_2*/ Y__builtin_strcpy(ж<TLS> t, uintptr dest, uintptr src);

public static partial Tsize_t /*_2*/ Y__builtin_strlen(ж<TLS> t, uintptr s);

public static partial int32 /*_3*/ Y__builtin_sub_overflowInt64(ж<TLS> t, int64 a, int64 b, uintptr res);

public static partial void Y__builtin_trap(ж<TLS> t);

public static partial float64 /*r*/ Y__builtin_trunc(ж<TLS> tls, float64 x);

public static partial void Y__builtin_unreachable(ж<TLS> t);

public static partial int32 /*_4*/ Y__builtin_vsnprintf(ж<TLS> t, uintptr str, Tsize_t size, uintptr format, uintptr va);

public static partial int32 /*_4*/ Y__c11_atomic_compare_exchange_strongInt16(ж<TLS> t, uintptr ptr, uintptr expected, int16 desired, int32 success, int32 failure);

public static partial int32 /*_3*/ Y__c11_atomic_compare_exchange_strongInt32(ж<TLS> t, uintptr ptr, uintptr expected, int32 desired, int32 success, int32 failure);

public static partial int32 /*_4*/ Y__c11_atomic_compare_exchange_strongInt64(ж<TLS> t, uintptr ptr, uintptr expected, int64 desired, int32 success, int32 failure);

public static partial int32 /*_4*/ Y__c11_atomic_compare_exchange_strongInt8(ж<TLS> t, uintptr ptr, uintptr expected, int8 desired, int32 success, int32 failure);

public static partial int32 /*_4*/ Y__c11_atomic_compare_exchange_strongUint16(ж<TLS> t, uintptr ptr, uintptr expected, uint16 desired, int32 success, int32 failure);

public static partial int32 /*_4*/ Y__c11_atomic_compare_exchange_strongUint32(ж<TLS> t, uintptr ptr, uintptr expected, uint32 desired, int32 success, int32 failure);

public static partial int32 /*_4*/ Y__c11_atomic_compare_exchange_strongUint64(ж<TLS> t, uintptr ptr, uintptr expected, uint64 desired, int32 success, int32 failure);

public static partial int32 /*_4*/ Y__c11_atomic_compare_exchange_strongUint8(ж<TLS> t, uintptr ptr, uintptr expected, uint8 desired, int32 success, int32 failure);

public static partial int16 /*r*/ Y__c11_atomic_exchangeInt16(ж<TLS> t, uintptr ptr, int16 val, int32 _);

public static partial int32 /*r*/ Y__c11_atomic_exchangeInt32(ж<TLS> t, uintptr ptr, int32 val, int32 _);

public static partial int64 /*r*/ Y__c11_atomic_exchangeInt64(ж<TLS> t, uintptr ptr, int64 val, int32 _);

public static partial int8 /*r*/ Y__c11_atomic_exchangeInt8(ж<TLS> t, uintptr ptr, int8 val, int32 _);

public static partial uint16 /*r*/ Y__c11_atomic_exchangeUint16(ж<TLS> t, uintptr ptr, uint16 val, int32 _);

public static partial uint32 /*r*/ Y__c11_atomic_exchangeUint32(ж<TLS> t, uintptr ptr, uint32 val, int32 _);

public static partial uint64 /*r*/ Y__c11_atomic_exchangeUint64(ж<TLS> t, uintptr ptr, uint64 val, int32 _);

public static partial uint8 /*r*/ Y__c11_atomic_exchangeUint8(ж<TLS> t, uintptr ptr, uint8 val, int32 _);

public static partial int16 /*r*/ Y__c11_atomic_fetch_addInt16(ж<TLS> t, uintptr ptr, int16 val, int32 _);

public static partial int32 /*r*/ Y__c11_atomic_fetch_addInt32(ж<TLS> t, uintptr ptr, int32 val, int32 _);

public static partial int64 /*r*/ Y__c11_atomic_fetch_addInt64(ж<TLS> t, uintptr ptr, int64 val, int32 _);

public static partial int8 /*r*/ Y__c11_atomic_fetch_addInt8(ж<TLS> t, uintptr ptr, int8 val, int32 _);

public static partial uint16 /*r*/ Y__c11_atomic_fetch_addUint16(ж<TLS> t, uintptr ptr, uint16 val, int32 _);

public static partial uint32 /*r*/ Y__c11_atomic_fetch_addUint32(ж<TLS> t, uintptr ptr, uint32 val, int32 _);

public static partial uint64 /*r*/ Y__c11_atomic_fetch_addUint64(ж<TLS> t, uintptr ptr, uint64 val, int32 _);

public static partial uint8 /*r*/ Y__c11_atomic_fetch_addUint8(ж<TLS> t, uintptr ptr, uint8 val, int32 _);

public static partial int16 /*r*/ Y__c11_atomic_fetch_andInt16(ж<TLS> t, uintptr ptr, int16 val, int32 _);

public static partial int32 /*r*/ Y__c11_atomic_fetch_andInt32(ж<TLS> t, uintptr ptr, int32 val, int32 _);

public static partial int64 /*r*/ Y__c11_atomic_fetch_andInt64(ж<TLS> t, uintptr ptr, int64 val, int32 _);

public static partial int8 /*r*/ Y__c11_atomic_fetch_andInt8(ж<TLS> t, uintptr ptr, int8 val, int32 _);

public static partial uint16 /*r*/ Y__c11_atomic_fetch_andUint16(ж<TLS> t, uintptr ptr, uint16 val, int32 _);

public static partial uint32 /*r*/ Y__c11_atomic_fetch_andUint32(ж<TLS> t, uintptr ptr, uint32 val, int32 _);

public static partial uint64 /*r*/ Y__c11_atomic_fetch_andUint64(ж<TLS> t, uintptr ptr, uint64 val, int32 _);

public static partial uint8 /*r*/ Y__c11_atomic_fetch_andUint8(ж<TLS> t, uintptr ptr, uint8 val, int32 _);

public static partial int16 /*r*/ Y__c11_atomic_fetch_orInt16(ж<TLS> t, uintptr ptr, int16 val, int32 _);

public static partial int32 /*r*/ Y__c11_atomic_fetch_orInt32(ж<TLS> t, uintptr ptr, int32 val, int32 _);

public static partial int64 /*r*/ Y__c11_atomic_fetch_orInt64(ж<TLS> t, uintptr ptr, int64 val, int32 _);

public static partial int8 /*r*/ Y__c11_atomic_fetch_orInt8(ж<TLS> t, uintptr ptr, int8 val, int32 _);

public static partial uint16 /*r*/ Y__c11_atomic_fetch_orUint16(ж<TLS> t, uintptr ptr, uint16 val, int32 _);

public static partial uint32 /*r*/ Y__c11_atomic_fetch_orUint32(ж<TLS> t, uintptr ptr, uint32 val, int32 _);

public static partial uint64 /*r*/ Y__c11_atomic_fetch_orUint64(ж<TLS> t, uintptr ptr, uint64 val, int32 _);

public static partial uint8 /*r*/ Y__c11_atomic_fetch_orUint8(ж<TLS> t, uintptr ptr, uint8 val, int32 _);

public static partial int16 /*r*/ Y__c11_atomic_fetch_subInt16(ж<TLS> t, uintptr ptr, int16 val, int32 _);

public static partial int32 /*r*/ Y__c11_atomic_fetch_subInt32(ж<TLS> t, uintptr ptr, int32 val, int32 _);

public static partial int64 /*r*/ Y__c11_atomic_fetch_subInt64(ж<TLS> t, uintptr ptr, int64 val, int32 _);

public static partial int8 /*r*/ Y__c11_atomic_fetch_subInt8(ж<TLS> t, uintptr ptr, int8 val, int32 _);

public static partial uint16 /*r*/ Y__c11_atomic_fetch_subUint16(ж<TLS> t, uintptr ptr, uint16 val, int32 _);

public static partial uint32 /*r*/ Y__c11_atomic_fetch_subUint32(ж<TLS> t, uintptr ptr, uint32 val, int32 _);

public static partial uint64 /*r*/ Y__c11_atomic_fetch_subUint64(ж<TLS> t, uintptr ptr, uint64 val, int32 _);

public static partial uint8 /*r*/ Y__c11_atomic_fetch_subUint8(ж<TLS> t, uintptr ptr, uint8 val, int32 _);

public static partial int16 /*r*/ Y__c11_atomic_fetch_xorInt16(ж<TLS> t, uintptr ptr, int16 val, int32 _);

public static partial int32 /*r*/ Y__c11_atomic_fetch_xorInt32(ж<TLS> t, uintptr ptr, int32 val, int32 _);

public static partial int64 /*r*/ Y__c11_atomic_fetch_xorInt64(ж<TLS> t, uintptr ptr, int64 val, int32 _);

public static partial int8 /*r*/ Y__c11_atomic_fetch_xorInt8(ж<TLS> t, uintptr ptr, int8 val, int32 _);

public static partial uint16 /*r*/ Y__c11_atomic_fetch_xorUint16(ж<TLS> t, uintptr ptr, uint16 val, int32 _);

public static partial uint32 /*r*/ Y__c11_atomic_fetch_xorUint32(ж<TLS> t, uintptr ptr, uint32 val, int32 _);

public static partial uint64 /*r*/ Y__c11_atomic_fetch_xorUint64(ж<TLS> t, uintptr ptr, uint64 val, int32 _);

public static partial uint8 /*r*/ Y__c11_atomic_fetch_xorUint8(ж<TLS> t, uintptr ptr, uint8 val, int32 _);

public static partial int16 /*r*/ Y__c11_atomic_loadInt16(ж<TLS> t, uintptr ptr, int32 memorder);

public static partial int32 /*r*/ Y__c11_atomic_loadInt32(ж<TLS> t, uintptr ptr, int32 memorder);

public static partial int64 /*r*/ Y__c11_atomic_loadInt64(ж<TLS> t, uintptr ptr, int32 memorder);

public static partial int8 /*r*/ Y__c11_atomic_loadInt8(ж<TLS> t, uintptr ptr, int32 memorder);

public static partial uint16 /*r*/ Y__c11_atomic_loadUint16(ж<TLS> t, uintptr ptr, int32 memorder);

public static partial uint32 /*r*/ Y__c11_atomic_loadUint32(ж<TLS> t, uintptr ptr, int32 memorder);

public static partial uint64 /*r*/ Y__c11_atomic_loadUint64(ж<TLS> t, uintptr ptr, int32 memorder);

public static partial uint8 /*r*/ Y__c11_atomic_loadUint8(ж<TLS> t, uintptr ptr, int32 memorder);

public static partial void Y__c11_atomic_storeInt16(ж<TLS> t, uintptr ptr, int16 val, int32 memorder);

public static partial void Y__c11_atomic_storeInt32(ж<TLS> t, uintptr ptr, int32 val, int32 memorder);

public static partial void Y__c11_atomic_storeInt64(ж<TLS> t, uintptr ptr, int64 val, int32 memorder);

public static partial void Y__c11_atomic_storeInt8(ж<TLS> t, uintptr ptr, int8 val, int32 memorder);

public static partial void Y__c11_atomic_storeUint16(ж<TLS> t, uintptr ptr, uint16 val, int32 memorder);

public static partial void Y__c11_atomic_storeUint32(ж<TLS> t, uintptr ptr, uint32 val, int32 memorder);

public static partial void Y__c11_atomic_storeUint64(ж<TLS> t, uintptr ptr, uint64 val, int32 memorder);

public static partial void Y__c11_atomic_storeUint8(ж<TLS> t, uintptr ptr, uint8 val, int32 memorder);

public static partial void Y__ccgo_dmesg(ж<TLS> t, uintptr fmt, uintptr va);

public static partial int32 /*_2*/ Y__ccgo_getMutexType(ж<TLS> tls, uintptr m);

public static partial uintptr /*_1*/ Y__ccgo_in6addr_anyp(ж<TLS> t);

public static partial int32 /*_2*/ Y__ccgo_pthreadAttrGetDetachState(ж<TLS> tls, uintptr a);

public static partial int32 /*_2*/ Y__ccgo_pthreadMutexattrGettype(ж<TLS> tls, uintptr a);

public static partial void Y__ccgo_sqlite3_log(ж<TLS> t, int32 iErrCode, uintptr zFormat, uintptr args);

public static partial int32 /*r1*/ Y__clock_gettime(ж<TLS> tls, Tclockid_t clk, uintptr ts);

public static partial int32 /*r*/ Y__clock_nanosleep(ж<TLS> tls, Tclockid_t clk, int32 flags, uintptr req, uintptr rem);

public static partial uintptr /*_2*/ Y__cmsg_nxthdr(ж<TLS> t, uintptr msgh, uintptr cmsg);

public static partial void Y__convert_scm_timestamps(ж<TLS> tls, uintptr msg, Tsocklen_t csize);

public static partial float64 /*r1*/ Y__cos(ж<TLS> tls, float64 x, float64 y);

public static partial float32 /*r1*/ Y__cosdf(ж<TLS> tls, float64 x);

public static partial uintptr /*r*/ Y__crypt_blowfish(ж<TLS> tls, uintptr key, uintptr setting, uintptr output);

public static partial uintptr /*r*/ Y__crypt_des(ж<TLS> tls, uintptr key, uintptr setting, uintptr output);

public static partial uintptr /*r*/ Y__crypt_md5(ж<TLS> tls, uintptr key, uintptr setting, uintptr output);

public static partial uintptr /*r*/ Y__crypt_r(ж<TLS> tls, uintptr key, uintptr salt, uintptr data);

public static partial uintptr /*r*/ Y__crypt_sha256(ж<TLS> tls, uintptr key, uintptr setting, uintptr output);

public static partial uintptr /*r*/ Y__crypt_sha512(ж<TLS> tls, uintptr key, uintptr setting, uintptr output);

public static partial uintptr /*r*/ Y__ctype_b_loc(ж<TLS> tls);

public static partial Tsize_t /*r*/ Y__ctype_get_mb_cur_max(ж<TLS> tls);

public static partial uintptr /*r*/ Y__ctype_tolower_loc(ж<TLS> tls);

public static partial uintptr /*r*/ Y__ctype_toupper_loc(ж<TLS> tls);

public static partial void Y__des_setkey(ж<TLS> tls, uintptr key, uintptr ekey);

public static partial int32 /*r*/ Y__dn_expand(ж<TLS> tls, uintptr @base, uintptr end, uintptr src, uintptr dest, int32 space);

public static partial int32 /*r1*/ Y__dns_parse(ж<TLS> tls, uintptr r, int32 rlen, uintptr __ccgo_fp_callback, uintptr ctx);

internal static partial int32 /*_7*/ __ccgo_abi0___dns_parse_2(ж<TLS> _0, uintptr _1, int32 _2, uintptr _3, int32 _4, uintptr _5, int32 _6, uintptr __ccgo_fp);

internal static void __ccgo_abiInternal___dns_parse_2(ref TLS tls, uintptr dest, uintptr abi0CodePtr) {
    ref var f = ref heap<Func<ж<TLS>, uintptr, int32, uintptr, int32, uintptr, int32, int32>>((ж<TLS> _0, uintptr _1, int32 _2, uintptr _3, int32 _4, uintptr _5, int32 _6) => {
        return __ccgo_abi0___dns_parse_2(_0, _1, _2, _3, _4, _5, _6, abi0CodePtr);
    }, out var Ꮡf);
    @unsafe.Pointer.StoreArray<uintptr>((@unsafe.Pointer)dest, 2, @unsafe.Pointer.LoadArray<uintptr>((@unsafe.Pointer)((uintptr)GoFuncCookie.Of(f)), 2));
}

public static partial void Y__do_des(ж<TLS> tls, Tuint32_t l_in, Tuint32_t r_in, uintptr l_out, uintptr r_out, Tuint32_t count, Tuint32_t saltbits, uintptr ekey);

public static partial void Y__do_orphaned_stdio_locks(ж<TLS> tls);

public static partial int32 /*r1*/ Y__dup3(ж<TLS> tls, int32 old, int32 new1, int32 flags);

public static partial Tlocale_t /*r*/ Y__duplocale(ж<TLS> tls, Tlocale_t old);

public static partial void Y__env_rm_add(ж<TLS> tls, uintptr old, uintptr new1);

public static partial uintptr /*r*/ Y__errno_location(ж<TLS> tls);

public static partial int32 /*r*/ Y__execvpe(ж<TLS> tls, uintptr @file, uintptr argv, uintptr envp);

public static partial float64 /*r*/ Y__expo2(ж<TLS> tls, float64 x, float64 sign);

public static partial float32 /*r*/ Y__expo2f(ж<TLS> tls, float32 x, float32 sign);

public static partial Tsize_t /*r*/ Y__fbufsize(ж<TLS> tls, uintptr f);

public static partial int32 /*r*/ Y__fclose_ca(ж<TLS> tls, uintptr f);

public static partial uintptr /*r*/ Y__fdopen(ж<TLS> tls, int32 fd, uintptr mode);

public static partial int32 /*r1*/ Y__fesetround(ж<TLS> tls, int32 r);

public static partial Twint_t /*r*/ Y__fgetwc_unlocked(ж<TLS> tls, uintptr f);

public static partial int32 /*r*/ Y__flbf(ж<TLS> tls, uintptr f);

public static partial float64 /*r*/ Y__floatscan(ж<TLS> tls, uintptr f, int32 prec, int32 pok);

public static partial int32 /*r*/ Y__fmodeflags(ж<TLS> tls, uintptr mode);

public static partial uintptr /*r*/ Y__fopen_rb_ca(ж<TLS> tls, uintptr filename, uintptr f, uintptr buf, Tsize_t len1);

public static partial int32 /*r*/ Y__fpclassify(ж<TLS> tls, float64 x);

public static partial int32 /*r*/ Y__fpclassifyf(ж<TLS> tls, float32 x);

public static partial int32 /*r*/ Y__fpclassifyl(ж<TLS> tls, float64 x);

public static partial Tsize_t /*r*/ Y__fpending(ж<TLS> tls, uintptr f);

public static partial int32 /*r*/ Y__fpurge(ж<TLS> tls, uintptr f);

public static partial Twint_t /*r*/ Y__fputwc_unlocked(ж<TLS> tls, Twchar_t c, uintptr f);

public static partial int32 /*r*/ Y__freadable(ж<TLS> tls, uintptr f);

public static partial Tsize_t /*r*/ Y__freadahead(ж<TLS> tls, uintptr f);

public static partial int32 /*r*/ Y__freading(ж<TLS> tls, uintptr f);

public static partial uintptr /*r*/ Y__freadptr(ж<TLS> tls, uintptr f, uintptr sizep);

public static partial void Y__freadptrinc(ж<TLS> tls, uintptr f, Tsize_t inc);

public static partial void Y__freelocale(ж<TLS> tls, Tlocale_t l);

public static partial int32 /*r*/ Y__fseeko(ж<TLS> tls, uintptr f, Toff_t off, int32 whence);

public static partial int32 /*r*/ Y__fseeko_unlocked(ж<TLS> tls, uintptr f, Toff_t off, int32 whence);

public static partial void Y__fseterr(ж<TLS> tls, uintptr f);

public static partial int32 /*r*/ Y__fsetlocking(ж<TLS> tls, uintptr f, int32 type1);

public static partial int32 /*r*/ Y__fstat(ж<TLS> tls, int32 fd, uintptr st);

public static partial int32 /*r*/ Y__fstatat(ж<TLS> tls, int32 fd, uintptr path, uintptr st, int32 flag);

public static partial Toff_t /*r*/ Y__ftello(ж<TLS> tls, uintptr f);

public static partial Toff_t /*r*/ Y__ftello_unlocked(ж<TLS> tls, uintptr f);

public static partial void Y__funcs_on_quick_exit(ж<TLS> tls);

public static partial int32 /*r*/ Y__futimesat(ж<TLS> tls, int32 dirfd, uintptr pathname, uintptr times);

public static partial int32 /*r*/ Y__fwritable(ж<TLS> tls, uintptr f);

public static partial Tsize_t /*r*/ Y__fwritex(ж<TLS> tls, uintptr s, Tsize_t l, uintptr f);

public static partial int32 /*r*/ Y__fwriting(ж<TLS> tls, uintptr f);

public static partial int32 /*r*/ Y__fxstat(ж<TLS> tls, int32 ver, int32 fd, uintptr buf);

public static partial int32 /*r*/ Y__fxstatat(ж<TLS> tls, int32 ver, int32 fd, uintptr path, uintptr buf, int32 flag);

public static partial void Y__get_handler_set(ж<TLS> tls, uintptr set);

public static partial uintptr /*r*/ Y__get_locale(ж<TLS> tls, int32 cat, uintptr val);

public static partial int32 /*r*/ Y__get_resolv_conf(ж<TLS> tls, uintptr conf, uintptr search, Tsize_t search_sz);

public static partial uint64 /*r*/ Y__getauxval(ж<TLS> tls, uint64 item);

public static partial Tssize_t /*r*/ Y__getdelim(ж<TLS> tls, uintptr s, uintptr n, int32 delim, uintptr f);

public static partial int32 /*r*/ Y__getgr_a(ж<TLS> tls, uintptr name, Tgid_t gid, uintptr gr, uintptr buf, uintptr size, uintptr mem, uintptr nmem, uintptr res);

public static partial int32 /*r*/ Y__getgrent_a(ж<TLS> tls, uintptr f, uintptr gr, uintptr line, uintptr size, uintptr mem, uintptr nmem, uintptr res);

public static partial void Y__getopt_msg(ж<TLS> tls, uintptr a, uintptr b, uintptr c, Tsize_t l);

public static partial int32 /*r*/ Y__getpw_a(ж<TLS> tls, uintptr name, Tuid_t uid, uintptr pw, uintptr buf, uintptr size, uintptr res);

public static partial int32 /*r*/ Y__getpwent_a(ж<TLS> tls, uintptr f, uintptr pw, uintptr line, uintptr size, uintptr res);

public static partial uintptr /*r*/ Y__gettextdomain(ж<TLS> tls);

public static partial uintptr /*r*/ Y__gmtime_r(ж<TLS> tls, uintptr t, uintptr tm);

public static partial uintptr /*r*/ Y__h_errno_location(ж<TLS> tls);

public static partial int32 /*r*/ Y__inet_aton(ж<TLS> tls, uintptr s0, uintptr dest);

public static partial void Y__init_ssp(ж<TLS> tls, uintptr entropy);

public static partial uint64 /*r*/ Y__intscan(ж<TLS> tls, uintptr f, uint32 @base, int32 pok, uint64 lim);

public static partial int32 /*r*/ Y__isalnum_l(ж<TLS> tls, int32 c, Tlocale_t l);

public static partial int32 /*r*/ Y__isalpha_l(ж<TLS> tls, int32 c, Tlocale_t l);

public static partial int32 /*r*/ Y__isblank_l(ж<TLS> tls, int32 c, Tlocale_t l);

public static partial int32 /*r*/ Y__iscntrl_l(ж<TLS> tls, int32 c, Tlocale_t l);

public static partial int32 /*r*/ Y__isdigit_l(ж<TLS> tls, int32 c, Tlocale_t l);

public static partial int32 /*_2*/ Y__isfinite(ж<TLS> tls, float64 d);

public static partial int32 /*_2*/ Y__isfinitef(ж<TLS> tls, float32 f);

public static partial int32 /*_2*/ Y__isfinitel(ж<TLS> tls, float64 d);

public static partial int32 /*r*/ Y__isgraph_l(ж<TLS> tls, int32 c, Tlocale_t l);

public static partial int32 /*r*/ Y__islower_l(ж<TLS> tls, int32 c, Tlocale_t l);

public static partial int32 /*_2*/ Y__isnan(ж<TLS> t, float64 x);

public static partial int32 /*_2*/ Y__isnanf(ж<TLS> t, float32 arg);

public static partial int32 /*_2*/ Y__isnanl(ж<TLS> t, float64 arg);

public static partial int32 /*r*/ Y__isoc99_fscanf(ж<TLS> tls, uintptr f, uintptr fmt, uintptr va);

public static partial int32 /*r*/ Y__isoc99_fwscanf(ж<TLS> tls, uintptr f, uintptr fmt, uintptr va);

public static partial int32 /*r*/ Y__isoc99_scanf(ж<TLS> tls, uintptr fmt, uintptr va);

public static partial int32 /*r*/ Y__isoc99_sscanf(ж<TLS> tls, uintptr s, uintptr fmt, uintptr va);

public static partial int32 /*r*/ Y__isoc99_swscanf(ж<TLS> tls, uintptr s, uintptr fmt, uintptr va);

public static partial int32 /*r*/ Y__isoc99_vfscanf(ж<TLS> tls, uintptr f, uintptr fmt, Tva_list ap);

public static partial int32 /*r*/ Y__isoc99_vfwscanf(ж<TLS> tls, uintptr f, uintptr fmt, Tva_list ap);

public static partial int32 /*r*/ Y__isoc99_vscanf(ж<TLS> tls, uintptr fmt, Tva_list ap);

public static partial int32 /*r*/ Y__isoc99_vsscanf(ж<TLS> tls, uintptr s, uintptr fmt, Tva_list ap);

public static partial int32 /*r*/ Y__isoc99_vswscanf(ж<TLS> tls, uintptr s, uintptr fmt, Tva_list ap);

public static partial int32 /*r*/ Y__isoc99_vwscanf(ж<TLS> tls, uintptr fmt, Tva_list ap);

public static partial int32 /*r*/ Y__isoc99_wscanf(ж<TLS> tls, uintptr fmt, uintptr va);

public static partial int32 /*r*/ Y__isprint_l(ж<TLS> tls, int32 c, Tlocale_t l);

public static partial int32 /*r*/ Y__ispunct_l(ж<TLS> tls, int32 c, Tlocale_t l);

public static partial int32 /*r*/ Y__isspace_l(ж<TLS> tls, int32 c, Tlocale_t l);

public static partial int32 /*r*/ Y__isupper_l(ж<TLS> tls, int32 c, Tlocale_t l);

public static partial int32 /*r*/ Y__iswalnum_l(ж<TLS> tls, Twint_t c, Tlocale_t l);

public static partial int32 /*r*/ Y__iswalpha_l(ж<TLS> tls, Twint_t c, Tlocale_t l);

public static partial int32 /*r*/ Y__iswblank_l(ж<TLS> tls, Twint_t c, Tlocale_t l);

public static partial int32 /*r*/ Y__iswcntrl_l(ж<TLS> tls, Twint_t c, Tlocale_t l);

public static partial int32 /*r*/ Y__iswctype_l(ж<TLS> tls, Twint_t c, Twctype_t t, Tlocale_t l);

public static partial int32 /*r*/ Y__iswdigit_l(ж<TLS> tls, Twint_t c, Tlocale_t l);

public static partial int32 /*r*/ Y__iswgraph_l(ж<TLS> tls, Twint_t c, Tlocale_t l);

public static partial int32 /*r*/ Y__iswlower_l(ж<TLS> tls, Twint_t c, Tlocale_t l);

public static partial int32 /*r*/ Y__iswprint_l(ж<TLS> tls, Twint_t c, Tlocale_t l);

public static partial int32 /*r*/ Y__iswpunct_l(ж<TLS> tls, Twint_t c, Tlocale_t l);

public static partial int32 /*r*/ Y__iswspace_l(ж<TLS> tls, Twint_t c, Tlocale_t l);

public static partial int32 /*r*/ Y__iswupper_l(ж<TLS> tls, Twint_t c, Tlocale_t l);

public static partial int32 /*r*/ Y__iswxdigit_l(ж<TLS> tls, Twint_t c, Tlocale_t l);

public static partial int32 /*r*/ Y__isxdigit_l(ж<TLS> tls, int32 c, Tlocale_t l);

public static partial uintptr /*r*/ Y__lctrans(ж<TLS> tls, uintptr msg, uintptr lm);

public static partial uintptr /*r*/ Y__lctrans_cur(ж<TLS> tls, uintptr msg);

public static partial uintptr /*r*/ Y__lctrans_impl(ж<TLS> tls, uintptr msg, uintptr lm);

public static partial complex128 /*r*/ Y__ldexp_cexp(ж<TLS> tls, complex128 z, int32 expt);

public static partial complex64 /*r*/ Y__ldexp_cexpf(ж<TLS> tls, complex64 z, int32 expt);

public static partial float64 /*r1*/ Y__lgamma_r(ж<TLS> tls, float64 x, uintptr signgamp);

public static partial float32 /*r1*/ Y__lgammaf_r(ж<TLS> tls, float32 x, uintptr signgamp);

public static partial float64 /*r*/ Y__lgammal_r(ж<TLS> tls, float64 x, uintptr sg);

public static partial int32 /*r*/ Y__libc_current_sigrtmax(ж<TLS> tls);

public static partial int32 /*r*/ Y__libc_current_sigrtmin(ж<TLS> tls);

public static partial int32 /*r1*/ Y__libc_sigaction(ж<TLS> tls, int32 sig, uintptr sa, uintptr old);

public static partial int32 /*r*/ Y__loc_is_allocated(ж<TLS> tls, Tlocale_t loc);

public static partial uintptr /*r*/ Y__localtime_r(ж<TLS> tls, uintptr t, uintptr tm);

public static partial int32 /*_2*/ Y__lockfile(ж<TLS> tls, uintptr @file);

public static partial int32 /*r*/ Y__lookup_ipliteral(ж<TLS> tls, uintptr buf, uintptr name, int32 family);

public static partial int32 /*r*/ Y__lookup_name(ж<TLS> tls, uintptr buf, uintptr canon, uintptr name, int32 family, int32 flags);

public static partial int32 /*r*/ Y__lookup_serv(ж<TLS> tls, uintptr buf, uintptr name, int32 proto, int32 socktype, int32 flags);

public static partial Toff_t /*r*/ Y__lseek(ж<TLS> tls, int32 fd, Toff_t offset, int32 whence);

public static partial int32 /*r*/ Y__lsysinfo(ж<TLS> tls, uintptr info);

public static partial int32 /*r*/ Y__lxstat(ж<TLS> tls, int32 ver, uintptr path, uintptr buf);

public static partial int32 /*r*/ Y__madvise(ж<TLS> tls, uintptr addr, Tsize_t len1, int32 advice);

public static partial uintptr /*r*/ Y__map_file(ж<TLS> tls, uintptr pathname, uintptr size);

public static partial float64 /*r*/ Y__math_divzero(ж<TLS> tls, Tuint32_t sign);

public static partial float32 /*r*/ Y__math_divzerof(ж<TLS> tls, Tuint32_t sign);

public static partial float64 /*r*/ Y__math_invalid(ж<TLS> tls, float64 x);

public static partial float32 /*r*/ Y__math_invalidf(ж<TLS> tls, float32 x);

public static partial float64 /*r*/ Y__math_oflow(ж<TLS> tls, Tuint32_t sign);

public static partial float32 /*r*/ Y__math_oflowf(ж<TLS> tls, Tuint32_t sign);

public static partial float64 /*r*/ Y__math_uflow(ж<TLS> tls, Tuint32_t sign);

public static partial float32 /*r*/ Y__math_uflowf(ж<TLS> tls, Tuint32_t sign);

public static partial float64 /*r*/ Y__math_xflow(ж<TLS> tls, Tuint32_t sign, float64 y2);

public static partial float32 /*r*/ Y__math_xflowf(ж<TLS> tls, Tuint32_t sign, float32 y2);

public static partial uintptr /*r*/ Y__memrchr(ж<TLS> tls, uintptr m, int32 c, Tsize_t n);

public static partial int32 /*r*/ Y__mkostemps(ж<TLS> tls, uintptr template, int32 len1, int32 flags);

public static partial uintptr /*r*/ Y__mmap(ж<TLS> tls, uintptr start, Tsize_t len1, int32 prot, int32 flags, int32 fd, Toff_t off);

public static partial uintptr /*r*/ Y__mo_lookup(ж<TLS> tls, uintptr p, Tsize_t size, uintptr s);

public static partial int32 /*r*/ Y__month_to_secs(ж<TLS> tls, int32 month, int32 is_leap);

public static partial int32 /*r*/ Y__mprotect(ж<TLS> tls, uintptr addr, Tsize_t len1, int32 prot);

public static partial uintptr /*r*/ Y__mremap(ж<TLS> tls, uintptr old_addr, Tsize_t old_len, Tsize_t new_len, int32 flags, uintptr va);

public static partial int32 /*r*/ Y__munmap(ж<TLS> tls, uintptr start, Tsize_t len1);

public static partial Tlocale_t /*r*/ Y__newlocale(ж<TLS> tls, int32 mask, uintptr name, Tlocale_t loc);

public static partial uintptr /*r*/ Y__nl_langinfo(ж<TLS> tls, Tnl_item item);

public static partial uintptr /*r*/ Y__nl_langinfo_l(ж<TLS> tls, Tnl_item item, Tlocale_t loc);

public static partial uintptr /*r*/ Y__nscd_query(ж<TLS> tls, Tint32_t req, uintptr key, uintptr buf, Tsize_t len1, uintptr swap);

public static partial uintptr /*r*/ Y__ofl_add(ж<TLS> tls, uintptr f);

public static partial uintptr /*r*/ Y__ofl_lock(ж<TLS> tls);

public static partial void Y__ofl_unlock(ж<TLS> tls);

public static partial int32 /*r*/ Y__overflow(ж<TLS> tls, uintptr f, int32 _c);

public static partial uint64 /*r*/ Y__pleval(ж<TLS> tls, uintptr s, uint64 n);

public static partial int32 /*r*/ Y__posix_getopt(ж<TLS> tls, int32 argc, uintptr argv, uintptr optstring);

public static partial void Y__procfdname(ж<TLS> tls, uintptr buf, uint32 fd);

public static partial int32 /*r*/ Y__ptsname_r(ж<TLS> tls, int32 fd, uintptr buf, Tsize_t len1);

public static partial int32 /*r1*/ Y__putenv(ж<TLS> tls, uintptr s, Tsize_t l, uintptr r);

public static partial void Y__qsort_r(ж<TLS> tls, uintptr @base, Tsize_t nel, Tsize_t width, Tcmpfun __ccgo_fp_cmp, uintptr arg);

internal static partial int32 /*_4*/ __ccgo_abi0___qsort_r_3(ж<TLS> _0, uintptr _1, uintptr _2, uintptr _3, uintptr __ccgo_fp);

internal static void __ccgo_abiInternal___qsort_r_3(ref TLS tls, uintptr dest, uintptr abi0CodePtr) {
    ref var f = ref heap<Func<ж<TLS>, uintptr, uintptr, uintptr, int32>>((ж<TLS> _0, uintptr _1, uintptr _2, uintptr _3) => {
        return __ccgo_abi0___qsort_r_3(_0, _1, _2, _3, abi0CodePtr);
    }, out var Ꮡf);
    @unsafe.Pointer.StoreArray<uintptr>((@unsafe.Pointer)dest, 2, @unsafe.Pointer.LoadArray<uintptr>((@unsafe.Pointer)((uintptr)GoFuncCookie.Of(f)), 2));
}

public static partial Tuint64_t /*r*/ Y__rand48_step(ж<TLS> tls, uintptr xi, uintptr lc);

public static partial void Y__register_locked_file(ж<TLS> tls, uintptr f, Tpthread_t self);

public static partial int32 /*r1*/ Y__rem_pio2(ж<TLS> tls, float64 x, uintptr y);

public static partial int32 /*r*/ Y__rem_pio2_large(ж<TLS> tls, uintptr x, uintptr y, int32 e0, int32 nx, int32 prec);

public static partial int32 /*r*/ Y__rem_pio2f(ж<TLS> tls, float32 x, uintptr y);

public static partial int32 /*r*/ Y__res_mkquery(ж<TLS> tls, int32 op, uintptr dname, int32 @class, int32 type1, uintptr data, int32 datalen, uintptr newrr, uintptr buf, int32 buflen);

public static partial int32 /*r*/ Y__res_msend(ж<TLS> tls, int32 nqueries, uintptr queries, uintptr qlens, uintptr answers, uintptr alens, int32 asize);

public static partial int32 /*r1*/ Y__res_msend_rc(ж<TLS> tls, int32 nqueries, uintptr queries, uintptr qlens, uintptr answers, uintptr alens, int32 asize, uintptr conf);

public static partial int32 /*r1*/ Y__res_send(ж<TLS> tls, uintptr _msg, int32 _msglen, uintptr _answer, int32 _anslen);

public static partial uintptr /*r*/ Y__res_state(ж<TLS> tls);

public static partial void Y__reset_tls(ж<TLS> tls);

public static partial void Y__restore(ж<TLS> tls);

public static partial void Y__restore_rt(ж<TLS> tls);

public static partial void Y__restore_sigs(ж<TLS> tls, uintptr set);

public static partial int32 /*r1*/ Y__rtnetlink_enumerate(ж<TLS> tls, int32 link_af, int32 addr_af, uintptr __ccgo_fp_cb, uintptr ctx);

internal static partial int32 /*_3*/ __ccgo_abi0___rtnetlink_enumerate_2(ж<TLS> _0, uintptr _1, uintptr _2, uintptr __ccgo_fp);

internal static void __ccgo_abiInternal___rtnetlink_enumerate_2(ref TLS tls, uintptr dest, uintptr abi0CodePtr) {
    ref var f = ref heap<Func<ж<TLS>, uintptr, uintptr, int32>>((ж<TLS> _0, uintptr _1, uintptr _2) => {
        return __ccgo_abi0___rtnetlink_enumerate_2(_0, _1, _2, abi0CodePtr);
    }, out var Ꮡf);
    @unsafe.Pointer.StoreArray<uintptr>((@unsafe.Pointer)dest, 2, @unsafe.Pointer.LoadArray<uintptr>((@unsafe.Pointer)((uintptr)GoFuncCookie.Of(f)), 2));
}

public static partial int32 /*r*/ Y__secs_to_tm(ж<TLS> tls, int64 t, uintptr tm);

public static partial void Y__secs_to_zone(ж<TLS> tls, int64 t, int32 local, uintptr isdst, uintptr offset, uintptr oppoff, uintptr zonename);

public static partial int32 /*r*/ Y__setxid(ж<TLS> tls, int32 nr, int32 id, int32 eid, int32 sid);

public static partial int32 /*r*/ Y__shgetc(ж<TLS> tls, uintptr f);

public static partial void Y__shlim(ж<TLS> tls, uintptr f, Toff_t lim);

public static partial uintptr /*r*/ Y__shm_mapname(ж<TLS> tls, uintptr name, uintptr buf);

public static partial int32 /*r1*/ Y__sigaction(ж<TLS> tls, int32 sig, uintptr sa, uintptr old);

public static partial int32 /*r*/ Y__signbit(ж<TLS> tls, float64 x);

public static partial int32 /*r*/ Y__signbitf(ж<TLS> tls, float32 x);

public static partial int32 /*r*/ Y__signbitl(ж<TLS> tls, float64 x);

public static partial int32 /*r*/ Y__sigsetjmp_tail(ж<TLS> tls, uintptr jb, int32 ret);

public static partial float64 /*r1*/ Y__sin(ж<TLS> tls, float64 x, float64 y, int32 iy);

public static partial float32 /*r1*/ Y__sindf(ж<TLS> tls, float64 x);

public static partial void Y__stack_chk_fail(ж<TLS> tls);

public static partial void Y__stack_chk_fail_local(ж<TLS> tls);

public static partial int32 /*r*/ Y__stdio_close(ж<TLS> tls, uintptr f);

public static partial void Y__stdio_exit(ж<TLS> tls);

public static partial void Y__stdio_exit_needed(ж<TLS> tls);

public static partial Tsize_t /*r*/ Y__stdio_read(ж<TLS> tls, uintptr f, uintptr buf, Tsize_t len1);

public static partial Toff_t /*r*/ Y__stdio_seek(ж<TLS> tls, uintptr f, Toff_t off, int32 whence);

public static partial Tsize_t /*r*/ Y__stdio_write(ж<TLS> tls, uintptr f, uintptr buf, Tsize_t len1);

public static partial Tsize_t /*r*/ Y__stdout_write(ж<TLS> tls, uintptr f, uintptr buf, Tsize_t len1);

public static partial uintptr /*r*/ Y__stpcpy(ж<TLS> tls, uintptr d, uintptr s);

public static partial uintptr /*r*/ Y__stpncpy(ж<TLS> tls, uintptr d, uintptr s, Tsize_t n);

public static partial int32 /*r1*/ Y__strcasecmp_l(ж<TLS> tls, uintptr l, uintptr r, Tlocale_t loc);

public static partial uintptr /*r*/ Y__strchrnul(ж<TLS> tls, uintptr s, int32 c);

public static partial int32 /*r1*/ Y__strcoll_l(ж<TLS> tls, uintptr l, uintptr r, Tlocale_t loc);

public static partial uintptr /*r*/ Y__strerror_l(ж<TLS> tls, int32 e, Tlocale_t loc);

public static partial uintptr /*r*/ Y__strftime_fmt_1(ж<TLS> tls, uintptr s, uintptr l, int32 f, uintptr tm, Tlocale_t loc, int32 pad);

public static partial Tsize_t /*r*/ Y__strftime_l(ж<TLS> tls, uintptr s, Tsize_t n, uintptr f, uintptr tm, Tlocale_t loc);

public static partial int32 /*r1*/ Y__strncasecmp_l(ж<TLS> tls, uintptr l, uintptr r, Tsize_t n, Tlocale_t loc);

public static partial float64 /*r*/ Y__strtod_l(ж<TLS> tls, uintptr s, uintptr p, Tlocale_t l);

public static partial float32 /*r*/ Y__strtof_l(ж<TLS> tls, uintptr s, uintptr p, Tlocale_t l);

public static partial Tintmax_t /*r*/ Y__strtoimax_internal(ж<TLS> tls, uintptr s, uintptr p, int32 @base);

public static partial int64 /*r*/ Y__strtol_internal(ж<TLS> tls, uintptr s, uintptr p, int32 @base);

public static partial float64 /*r*/ Y__strtold_l(ж<TLS> tls, uintptr s, uintptr p, Tlocale_t l);

public static partial int64 /*r*/ Y__strtoll_internal(ж<TLS> tls, uintptr s, uintptr p, int32 @base);

public static partial uint64 /*r*/ Y__strtoul_internal(ж<TLS> tls, uintptr s, uintptr p, int32 @base);

public static partial uint64 /*r*/ Y__strtoull_internal(ж<TLS> tls, uintptr s, uintptr p, int32 @base);

public static partial Tuintmax_t /*r*/ Y__strtoumax_internal(ж<TLS> tls, uintptr s, uintptr p, int32 @base);

public static partial Tsize_t /*r*/ Y__strxfrm_l(ж<TLS> tls, uintptr dest, uintptr src, Tsize_t n, Tlocale_t loc);

public static partial void Y__sync_synchronize(ж<TLS> t);

public static partial int16 /*r*/ Y__sync_val_compare_and_swapInt16(ж<TLS> t, uintptr ptr, int16 oldval, int16 newval);

public static partial int32 /*r*/ Y__sync_val_compare_and_swapInt32(ж<TLS> t, uintptr ptr, int32 oldval, int32 newval);

public static partial int64 /*r*/ Y__sync_val_compare_and_swapInt64(ж<TLS> t, uintptr ptr, int64 oldval, int64 newval);

public static partial int8 /*r*/ Y__sync_val_compare_and_swapInt8(ж<TLS> t, uintptr ptr, int8 oldval, int8 newval);

public static partial uint16 /*r*/ Y__sync_val_compare_and_swapUint16(ж<TLS> t, uintptr ptr, uint16 oldval, uint16 newval);

public static partial uint32 /*r*/ Y__sync_val_compare_and_swapUint32(ж<TLS> t, uintptr ptr, uint32 oldval, uint32 newval);

public static partial uint64 /*r*/ Y__sync_val_compare_and_swapUint64(ж<TLS> t, uintptr ptr, uint64 oldval, uint64 newval);

public static partial uint8 /*r*/ Y__sync_val_compare_and_swapUint8(ж<TLS> t, uintptr ptr, uint8 oldval, uint8 newval);

public static partial @long /*_2*/ Y__syscall0(ж<TLS> tls, @long n);

public static partial @long /*_2*/ Y__syscall1(ж<TLS> tls, @long n, @long a1);

public static partial @long /*_2*/ Y__syscall2(ж<TLS> tls, @long n, @long a1, @long a2);

public static partial @long /*_2*/ Y__syscall3(ж<TLS> tls, @long n, @long a1, @long a2, @long a3);

public static partial @long /*_2*/ Y__syscall4(ж<TLS> tls, @long n, @long a1, @long a2, @long a3, @long a4);

public static partial @long /*_2*/ Y__syscall5(ж<TLS> tls, @long n, @long a1, @long a2, @long a3, @long a4, @long a5);

public static partial @long /*_2*/ Y__syscall6(ж<TLS> tls, @long n, @long a1, @long a2, @long a3, @long a4, @long a5, @long a6);

public static partial int64 /*r1*/ Y__syscall_ret(ж<TLS> tls, uint64 r);

public static partial float64 /*r1*/ Y__tan(ж<TLS> tls, float64 x, float64 y, int32 odd);

public static partial float32 /*r1*/ Y__tandf(ж<TLS> tls, float64 x, int32 odd);

public static partial int64 /*r*/ Y__tm_to_secs(ж<TLS> tls, uintptr tm);

public static partial uintptr /*r*/ Y__tm_to_tzname(ж<TLS> tls, uintptr tm);

public static partial int32 /*r*/ Y__tolower_l(ж<TLS> tls, int32 c, Tlocale_t l);

public static partial int32 /*r*/ Y__toread(ж<TLS> tls, uintptr f);

public static partial void Y__toread_needs_stdio_exit(ж<TLS> tls);

public static partial int32 /*r*/ Y__toupper_l(ж<TLS> tls, int32 c, Tlocale_t l);

public static partial Twint_t /*r*/ Y__towctrans_l(ж<TLS> tls, Twint_t c, Twctrans_t t, Tlocale_t l);

public static partial Twint_t /*r*/ Y__towlower_l(ж<TLS> tls, Twint_t c, Tlocale_t l);

public static partial int32 /*r*/ Y__towrite(ж<TLS> tls, uintptr f);

public static partial void Y__towrite_needs_stdio_exit(ж<TLS> tls);

public static partial Twint_t /*r*/ Y__towupper_l(ж<TLS> tls, Twint_t c, Tlocale_t l);

public static partial uintptr /*r*/ Y__tre_mem_alloc_impl(ж<TLS> tls, Ttre_mem_t mem, int32 provided, uintptr provided_block, int32 zero, Tsize_t size);

public static partial void Y__tre_mem_destroy(ж<TLS> tls, Ttre_mem_t mem);

public static partial Ttre_mem_t /*r*/ Y__tre_mem_new_impl(ж<TLS> tls, int32 provided, uintptr provided_block);

public static partial int32 /*r*/ Y__tsearch_balance(ж<TLS> tls, uintptr p);

public static partial int32 /*r*/ Y__uflow(ж<TLS> tls, uintptr f);

public static partial void Y__unlist_locked_file(ж<TLS> tls, uintptr f);

public static partial void Y__unlockfile(ж<TLS> tls, uintptr @file);

public static partial Tlocale_t /*r*/ Y__uselocale(ж<TLS> tls, Tlocale_t new1);

public static partial void Y__vm_wait(ж<TLS> tls);

public static partial int32 /*r1*/ Y__wcscoll_l(ж<TLS> tls, uintptr l, uintptr r, Tlocale_t locale);

public static partial Tsize_t /*r*/ Y__wcsftime_l(ж<TLS> tls, uintptr s, Tsize_t n, uintptr f, uintptr tm, Tlocale_t loc);

public static partial Tsize_t /*r*/ Y__wcsxfrm_l(ж<TLS> tls, uintptr dest, uintptr src, Tsize_t n, Tlocale_t loc);

public static partial Twctrans_t /*r*/ Y__wctrans_l(ж<TLS> tls, uintptr s, Tlocale_t l);

public static partial Twctype_t /*r*/ Y__wctype_l(ж<TLS> tls, uintptr s, Tlocale_t l);

public static partial int32 /*r*/ Y__xmknod(ж<TLS> tls, int32 ver, uintptr path, Tmode_t mode, uintptr dev);

public static partial int32 /*r*/ Y__xmknodat(ж<TLS> tls, int32 ver, int32 fd, uintptr path, Tmode_t mode, uintptr dev);

public static partial uintptr /*r*/ Y__xpg_basename(ж<TLS> tls, uintptr s);

public static partial int32 /*r*/ Y__xpg_strerror_r(ж<TLS> tls, int32 err, uintptr buf, Tsize_t buflen);

public static partial int32 /*r*/ Y__xstat(ж<TLS> tls, int32 ver, uintptr path, uintptr buf);

public static partial int64 /*r*/ Y__year_to_secs(ж<TLS> tls, int64 year, uintptr is_leap);

public static partial void Y_exit(ж<TLS> tls, int32 status);

public static partial void Y_flushlbf(ж<TLS> tls);

public static partial void Y_longjmp(ж<TLS> t, uintptr env, int32 val);

public static partial int32 /*_4*/ Y_obstack_begin(ж<TLS> t, uintptr obstack, int32 size, int32 alignment, uintptr chunkfun, uintptr freefun);

public static partial int32 /*_3*/ Y_obstack_newchunk(ж<TLS> t, uintptr obstack, int32 length);

public static partial void Y_pthread_cleanup_pop(ж<TLS> tls, uintptr _, int32 run);

public static partial void Y_pthread_cleanup_push(ж<TLS> tls, uintptr _, uintptr f, uintptr x);

public static partial int32 /*_2*/ Y_setjmp(ж<TLS> t, uintptr env);

public static partial int64 /*r*/ Ya64l(ж<TLS> tls, uintptr s);

public static partial void Yabort(ж<TLS> tls);

public static partial int32 /*r*/ Yabs(ж<TLS> tls, int32 a);

public static partial int32 /*r1*/ Yaccept(ж<TLS> tls, int32 fd, uintptr addr, uintptr len1);

public static partial int32 /*r1*/ Yaccept4(ж<TLS> tls, int32 fd, uintptr addr, uintptr len1, int32 flg);

public static partial int32 /*r*/ Yaccess(ж<TLS> tls, uintptr filename, int32 amode);

public static partial int32 /*r*/ Yacct(ж<TLS> tls, uintptr filename);

public static partial float64 /*r*/ Yacos(ж<TLS> tls, float64 x);

public static partial float32 /*r*/ Yacosf(ж<TLS> tls, float32 x);

public static partial float64 /*r*/ Yacosh(ж<TLS> tls, float64 x);

public static partial float32 /*r*/ Yacoshf(ж<TLS> tls, float32 x);

public static partial float64 /*r*/ Yacoshl(ж<TLS> tls, float64 x);

public static partial float64 /*r*/ Yacosl(ж<TLS> tls, float64 x);

public static partial int32 /*r*/ Yaddmntent(ж<TLS> tls, uintptr f, uintptr mnt);

public static partial int32 /*r*/ Yadjtime(ж<TLS> tls, uintptr @in, uintptr @out);

public static partial int32 /*r*/ Yadjtimex(ж<TLS> tls, uintptr tx);

public static partial uint32 /*r*/ Yalarm(ж<TLS> tls, uint32 seconds);

public static partial uintptr /*_2*/ Yalloca(ж<TLS> tls, Tsize_t size);

public static partial int32 /*r*/ Yalphasort(ж<TLS> tls, uintptr a, uintptr b);

public static partial int32 /*r*/ Yarch_prctl(ж<TLS> tls, int32 code, uint64 addr);

public static partial uintptr /*r*/ Yasctime(ж<TLS> tls, uintptr tm);

public static partial uintptr /*r*/ Yasctime_r(ж<TLS> tls, uintptr tm, uintptr buf);

public static partial float64 /*r1*/ Yasin(ж<TLS> tls, float64 x);

public static partial float32 /*r*/ Yasinf(ж<TLS> tls, float32 x);

public static partial float64 /*r*/ Yasinh(ж<TLS> tls, float64 x3);

public static partial float32 /*r*/ Yasinhf(ж<TLS> tls, float32 x3);

public static partial float64 /*r*/ Yasinhl(ж<TLS> tls, float64 x);

public static partial float64 /*r*/ Yasinl(ж<TLS> tls, float64 x);

public static partial int32 /*r*/ Yasprintf(ж<TLS> tls, uintptr s, uintptr fmt, uintptr va);

public static partial int32 /*r1*/ Yat_quick_exit(ж<TLS> tls, uintptr __ccgo_fp_func);

internal static partial void __ccgo_abi0_at_quick_exit_0(ж<TLS> _0, uintptr __ccgo_fp);

internal static void __ccgo_abiInternal_at_quick_exit_0(ref TLS tls, uintptr dest, uintptr abi0CodePtr) {
    ref var f = ref heap<Action<ж<TLS>>>((ж<TLS> _0) => {
        __ccgo_abi0_at_quick_exit_0(_0, abi0CodePtr);
    }, out var Ꮡf);
    @unsafe.Pointer.StoreArray<uintptr>((@unsafe.Pointer)dest, 2, @unsafe.Pointer.LoadArray<uintptr>((@unsafe.Pointer)((uintptr)GoFuncCookie.Of(f)), 2));
}

public static partial float64 /*r*/ Yatan(ж<TLS> tls, float64 x3);

public static partial float64 /*r*/ Yatan2(ж<TLS> tls, float64 y, float64 x);

public static partial float32 /*r*/ Yatan2f(ж<TLS> tls, float32 y, float32 x);

public static partial float64 /*r*/ Yatan2l(ж<TLS> tls, float64 y, float64 x);

public static partial float32 /*r*/ Yatanf(ж<TLS> tls, float32 x3);

public static partial float64 /*r*/ Yatanh(ж<TLS> tls, float64 x3);

public static partial float32 /*r*/ Yatanhf(ж<TLS> tls, float32 x3);

public static partial float64 /*r*/ Yatanhl(ж<TLS> tls, float64 x);

public static partial float64 /*r*/ Yatanl(ж<TLS> tls, float64 x);

public static partial int32 /*r*/ Yatexit(ж<TLS> tls, uintptr func_);

public static partial float64 /*r*/ Yatof(ж<TLS> tls, uintptr s);

public static partial int32 /*r*/ Yatoi(ж<TLS> tls, uintptr s);

public static partial int64 /*r*/ Yatol(ж<TLS> tls, uintptr s);

public static partial int64 /*r*/ Yatoll(ж<TLS> tls, uintptr s);

public static partial int32 /*_3*/ Ybacktrace(ж<TLS> t, uintptr buf, int32 size);

public static partial void Ybacktrace_symbols_fd(ж<TLS> t, uintptr buffer, int32 size, int32 fd);

public static partial uintptr /*r*/ Ybasename(ж<TLS> tls, uintptr s);

public static partial int32 /*r*/ Ybcmp(ж<TLS> tls, uintptr s1, uintptr s2, Tsize_t n);

public static partial void Ybcopy(ж<TLS> tls, uintptr s1, uintptr s2, Tsize_t n);

public static partial int32 /*r1*/ Ybind(ж<TLS> tls, int32 fd, uintptr addr, Tsocklen_t len1);

public static partial uintptr /*r*/ Ybind_textdomain_codeset(ж<TLS> tls, uintptr domainname, uintptr codeset);

public static partial uintptr /*r*/ Ybindtextdomain(ж<TLS> tls, uintptr domainname, uintptr dirname);

public static partial int32 /*r*/ Ybrk(ж<TLS> tls, uintptr end);

public static partial uintptr /*r*/ Ybsearch(ж<TLS> tls, uintptr key, uintptr @base, Tsize_t nel, Tsize_t width, uintptr __ccgo_fp_cmp);

internal static partial int32 /*_3*/ __ccgo_abi0_bsearch_4(ж<TLS> _0, uintptr _1, uintptr _2, uintptr __ccgo_fp);

internal static void __ccgo_abiInternal_bsearch_4(ref TLS tls, uintptr dest, uintptr abi0CodePtr) {
    ref var f = ref heap<Func<ж<TLS>, uintptr, uintptr, int32>>((ж<TLS> _0, uintptr _1, uintptr _2) => {
        return __ccgo_abi0_bsearch_4(_0, _1, _2, abi0CodePtr);
    }, out var Ꮡf);
    @unsafe.Pointer.StoreArray<uintptr>((@unsafe.Pointer)dest, 2, @unsafe.Pointer.LoadArray<uintptr>((@unsafe.Pointer)((uintptr)GoFuncCookie.Of(f)), 2));
}

public static partial Twint_t /*r*/ Ybtowc(ж<TLS> tls, int32 c);

public static partial void Ybzero(ж<TLS> tls, uintptr s, Tsize_t n);

public static partial Tsize_t /*r*/ Yc16rtomb(ж<TLS> tls, uintptr s, Tchar16_t c16, uintptr ps);

public static partial Tsize_t /*r*/ Yc32rtomb(ж<TLS> tls, uintptr s, Tchar32_t c32, uintptr ps);

public static partial float64 /*r*/ Ycabs(ж<TLS> tls, complex128 z);

public static partial float32 /*r*/ Ycabsf(ж<TLS> tls, complex64 z);

public static partial float64 /*r*/ Ycabsl(ж<TLS> tls, complex128 z);

public static partial complex128 /*r*/ Ycacos(ж<TLS> tls, complex128 z);

public static partial complex64 /*r*/ Ycacosf(ж<TLS> tls, complex64 z);

public static partial complex128 /*r*/ Ycacosh(ж<TLS> tls, complex128 z);

public static partial complex64 /*r*/ Ycacoshf(ж<TLS> tls, complex64 z);

public static partial complex128 /*r*/ Ycacoshl(ж<TLS> tls, complex128 z);

public static partial complex128 /*r*/ Ycacosl(ж<TLS> tls, complex128 z);

public static partial uintptr /*r*/ Ycalloc(ж<TLS> tls, Tsize_t m, Tsize_t n);

public static partial int32 /*r*/ Ycapget(ж<TLS> tls, uintptr a, uintptr b);

public static partial int32 /*r*/ Ycapset(ж<TLS> tls, uintptr a, uintptr b);

public static partial float64 /*r*/ Ycarg(ж<TLS> tls, complex128 z);

public static partial float32 /*r*/ Ycargf(ж<TLS> tls, complex64 z);

public static partial float64 /*r*/ Ycargl(ж<TLS> tls, complex128 z);

public static partial complex128 /*r1*/ Ycasin(ж<TLS> tls, complex128 z);

public static partial complex64 /*r1*/ Ycasinf(ж<TLS> tls, complex64 z);

public static partial complex128 /*r*/ Ycasinh(ж<TLS> tls, complex128 z);

public static partial complex64 /*r*/ Ycasinhf(ж<TLS> tls, complex64 z);

public static partial complex128 /*r*/ Ycasinhl(ж<TLS> tls, complex128 z);

public static partial complex128 /*r*/ Ycasinl(ж<TLS> tls, complex128 z);

public static partial complex128 /*r*/ Ycatan(ж<TLS> tls, complex128 z);

public static partial complex64 /*r*/ Ycatanf(ж<TLS> tls, complex64 z);

public static partial complex128 /*r*/ Ycatanh(ж<TLS> tls, complex128 z);

public static partial complex64 /*r*/ Ycatanhf(ж<TLS> tls, complex64 z);

public static partial complex128 /*r*/ Ycatanhl(ж<TLS> tls, complex128 z);

public static partial complex128 /*r*/ Ycatanl(ж<TLS> tls, complex128 z);

public static partial int32 /*r*/ Ycatclose(ж<TLS> tls, Tnl_catd catd);

public static partial uintptr /*r*/ Ycatgets(ж<TLS> tls, Tnl_catd catd, int32 set_id, int32 msg_id, uintptr s);

public static partial Tnl_catd /*r*/ Ycatopen(ж<TLS> tls, uintptr name, int32 oflag);

public static partial float64 /*r1*/ Ycbrt(ж<TLS> tls, float64 x);

public static partial float32 /*r1*/ Ycbrtf(ж<TLS> tls, float32 x);

public static partial float64 /*r*/ Ycbrtl(ж<TLS> tls, float64 x);

public static partial complex128 /*r*/ Yccos(ж<TLS> tls, complex128 z);

public static partial complex64 /*r*/ Yccosf(ж<TLS> tls, complex64 z);

public static partial complex128 /*r*/ Yccosh(ж<TLS> tls, complex128 z);

public static partial complex64 /*r*/ Yccoshf(ж<TLS> tls, complex64 z);

public static partial complex128 /*r*/ Yccoshl(ж<TLS> tls, complex128 z);

public static partial complex128 /*r*/ Yccosl(ж<TLS> tls, complex128 z);

public static partial float64 /*r*/ Yceil(ж<TLS> tls, float64 x3);

public static partial float32 /*r*/ Yceilf(ж<TLS> tls, float32 x3);

public static partial float64 /*r*/ Yceill(ж<TLS> tls, float64 x);

public static partial complex128 /*r*/ Ycexp(ж<TLS> tls, complex128 z);

public static partial complex64 /*r*/ Ycexpf(ж<TLS> tls, complex64 z);

public static partial complex128 /*r*/ Ycexpl(ж<TLS> tls, complex128 z);

public static partial Tspeed_t /*r*/ Ycfgetispeed(ж<TLS> tls, uintptr tio);

public static partial Tspeed_t /*r*/ Ycfgetospeed(ж<TLS> tls, uintptr tio);

public static partial void Ycfmakeraw(ж<TLS> tls, uintptr t);

public static partial int32 /*r*/ Ycfsetispeed(ж<TLS> tls, uintptr tio, Tspeed_t speed);

public static partial int32 /*r*/ Ycfsetospeed(ж<TLS> tls, uintptr tio, Tspeed_t speed);

public static partial int32 /*r*/ Ycfsetspeed(ж<TLS> tls, uintptr tio, Tspeed_t speed);

public static partial int32 /*r*/ Ychdir(ж<TLS> tls, uintptr path);

public static partial int32 /*r*/ Ychmod(ж<TLS> tls, uintptr path, Tmode_t mode);

public static partial int32 /*r*/ Ychown(ж<TLS> tls, uintptr path, Tuid_t uid, Tgid_t gid);

public static partial int32 /*r*/ Ychroot(ж<TLS> tls, uintptr path);

public static partial float64 /*r*/ Ycimag(ж<TLS> tls, complex128 z);

public static partial float32 /*r*/ Ycimagf(ж<TLS> tls, complex64 z);

public static partial float64 /*r*/ Ycimagl(ж<TLS> tls, complex128 z);

public static partial int32 /*r*/ Yclearenv(ж<TLS> tls);

public static partial void Yclearerr(ж<TLS> tls, uintptr f);

public static partial void Yclearerr_unlocked(ж<TLS> tls, uintptr f);

public static partial Tclock_t /*r*/ Yclock(ж<TLS> tls);

public static partial int32 /*r1*/ Yclock_adjtime(ж<TLS> tls, Tclockid_t clock_id, uintptr utx);

public static partial int32 /*r*/ Yclock_getcpuclockid(ж<TLS> tls, Tpid_t pid, uintptr clk);

public static partial int32 /*r*/ Yclock_getres(ж<TLS> tls, Tclockid_t clk, uintptr ts);

public static partial int32 /*r*/ Yclock_gettime(ж<TLS> tls, Tclockid_t clk, uintptr ts);

public static partial int32 /*r*/ Yclock_nanosleep(ж<TLS> tls, Tclockid_t clk, int32 flags, uintptr req, uintptr rem);

public static partial int32 /*r*/ Yclock_settime(ж<TLS> tls, Tclockid_t clk, uintptr ts);

public static partial complex128 /*r1*/ Yclog(ж<TLS> tls, complex128 z);

public static partial complex64 /*r1*/ Yclogf(ж<TLS> tls, complex64 z);

public static partial complex128 /*r*/ Yclogl(ж<TLS> tls, complex128 z);

public static partial int32 /*r1*/ Yclose(ж<TLS> tls, int32 fd);

public static partial int32 /*r*/ Yclosedir(ж<TLS> tls, uintptr dir);

public static partial void Ycloselog(ж<TLS> tls);

public static partial Tsize_t /*r*/ Yconfstr(ж<TLS> tls, int32 name, uintptr buf, Tsize_t len1);

public static partial complex128 /*r*/ Yconj(ж<TLS> tls, complex128 z);

public static partial complex64 /*r*/ Yconjf(ж<TLS> tls, complex64 z);

public static partial complex128 /*r*/ Yconjl(ж<TLS> tls, complex128 z);

public static partial int32 /*r1*/ Yconnect(ж<TLS> tls, int32 fd, uintptr addr, Tsocklen_t len1);

public static partial Tssize_t /*r*/ Ycopy_file_range(ж<TLS> tls, int32 fd_in, uintptr off_in, int32 fd_out, uintptr off_out, Tsize_t len1, uint32 flags);

public static partial float64 /*r*/ Ycopysign(ж<TLS> tls, float64 x, float64 y);

public static partial float32 /*r*/ Ycopysignf(ж<TLS> tls, float32 x, float32 y);

public static partial float64 /*r*/ Ycopysignl(ж<TLS> tls, float64 x, float64 y);

public static partial float64 /*r*/ Ycos(ж<TLS> tls, float64 x3);

public static partial float32 /*r*/ Ycosf(ж<TLS> tls, float32 x3);

public static partial float64 /*r*/ Ycosh(ж<TLS> tls, float64 x3);

public static partial float32 /*r*/ Ycoshf(ж<TLS> tls, float32 x3);

public static partial float64 /*r*/ Ycoshl(ж<TLS> tls, float64 x);

public static partial float64 /*r*/ Ycosl(ж<TLS> tls, float64 x);

public static partial complex128 /*r*/ Ycpow(ж<TLS> tls, complex128 z, complex128 c);

public static partial complex64 /*r*/ Ycpowf(ж<TLS> tls, complex64 z, complex64 c);

public static partial complex128 /*r*/ Ycpowl(ж<TLS> tls, complex128 z, complex128 c);

public static partial complex128 /*r*/ Ycproj(ж<TLS> tls, complex128 z);

public static partial complex64 /*r*/ Ycprojf(ж<TLS> tls, complex64 z);

public static partial complex128 /*r*/ Ycprojl(ж<TLS> tls, complex128 z);

public static partial float64 /*r*/ Ycreal(ж<TLS> tls, complex128 z);

public static partial float32 /*r*/ Ycrealf(ж<TLS> tls, complex64 z);

public static partial float64 /*r*/ Ycreall(ж<TLS> tls, complex128 z);

public static partial int32 /*r*/ Ycreat(ж<TLS> tls, uintptr filename, Tmode_t mode);

public static partial uintptr /*r*/ Ycrypt(ж<TLS> tls, uintptr key, uintptr salt);

public static partial uintptr /*r*/ Ycrypt_r(ж<TLS> tls, uintptr key, uintptr salt, uintptr data);

public static partial complex128 /*r*/ Ycsin(ж<TLS> tls, complex128 z);

public static partial complex64 /*r*/ Ycsinf(ж<TLS> tls, complex64 z);

public static partial complex128 /*r*/ Ycsinh(ж<TLS> tls, complex128 z);

public static partial complex64 /*r*/ Ycsinhf(ж<TLS> tls, complex64 z);

public static partial complex128 /*r*/ Ycsinhl(ж<TLS> tls, complex128 z);

public static partial complex128 /*r*/ Ycsinl(ж<TLS> tls, complex128 z);

public static partial complex128 /*r*/ Ycsqrt(ж<TLS> tls, complex128 z);

public static partial complex64 /*r*/ Ycsqrtf(ж<TLS> tls, complex64 z);

public static partial complex128 /*r*/ Ycsqrtl(ж<TLS> tls, complex128 z);

public static partial complex128 /*r*/ Yctan(ж<TLS> tls, complex128 z);

public static partial complex64 /*r*/ Yctanf(ж<TLS> tls, complex64 z);

public static partial complex128 /*r*/ Yctanh(ж<TLS> tls, complex128 z);

public static partial complex64 /*r*/ Yctanhf(ж<TLS> tls, complex64 z);

public static partial complex128 /*r*/ Yctanhl(ж<TLS> tls, complex128 z);

public static partial complex128 /*r*/ Yctanl(ж<TLS> tls, complex128 z);

public static partial uintptr /*r*/ Yctermid(ж<TLS> tls, uintptr s);

public static partial uintptr /*r*/ Yctime(ж<TLS> tls, uintptr t);

public static partial uintptr /*r*/ Yctime_r(ж<TLS> tls, uintptr t, uintptr buf);

public static partial uintptr /*r*/ Ycuserid(ж<TLS> tls, uintptr buf);

public static partial uintptr /*r*/ Ydcgettext(ж<TLS> tls, uintptr domainname, uintptr msgid, int32 category);

public static partial uintptr /*r1*/ Ydcngettext(ж<TLS> tls, uintptr domainname, uintptr msgid1, uintptr msgid2, uint64 n, int32 category);

public static partial int32 /*r*/ Ydelete_module(ж<TLS> tls, uintptr a, uint32 b);

public static partial uintptr /*r*/ Ydgettext(ж<TLS> tls, uintptr domainname, uintptr msgid);

public static partial float64 /*r*/ Ydifftime(ж<TLS> tls, Ttime_t t1, Ttime_t t0);

public static partial int32 /*r*/ Ydirfd(ж<TLS> tls, uintptr d);

public static partial uintptr /*r*/ Ydirname(ж<TLS> tls, uintptr s);

public static partial Tdiv_t /*r*/ Ydiv(ж<TLS> tls, int32 num, int32 den);

public static partial int32 /*_2*/ Ydlclose(ж<TLS> t, uintptr handle);

public static partial uintptr /*_1*/ Ydlerror(ж<TLS> t);

public static partial uintptr /*_3*/ Ydlopen(ж<TLS> t, uintptr filename, int32 flags);

public static partial uintptr /*_2*/ Ydlsym(ж<TLS> t, uintptr handle, uintptr symbol);

public static partial int32 /*r*/ Ydn_comp(ж<TLS> tls, uintptr src, uintptr dst, int32 space, uintptr dnptrs, uintptr lastdnptr);

public static partial int32 /*r*/ Ydn_expand(ж<TLS> tls, uintptr @base, uintptr end, uintptr src, uintptr dest, int32 space);

public static partial int32 /*r*/ Ydn_skipname(ж<TLS> tls, uintptr s, uintptr end);

public static partial uintptr /*r*/ Ydngettext(ж<TLS> tls, uintptr domainname, uintptr msgid1, uintptr msgid2, uint64 n);

public static partial int32 /*r*/ Ydprintf(ж<TLS> tls, int32 fd, uintptr fmt, uintptr va);

public static partial float64 /*r*/ Ydrand48(ж<TLS> tls);

public static partial float64 /*r*/ Ydrem(ж<TLS> tls, float64 x, float64 y);

public static partial float32 /*r*/ Ydremf(ж<TLS> tls, float32 x, float32 y);

public static partial int32 /*r*/ Ydup(ж<TLS> tls, int32 fd);

public static partial int32 /*r1*/ Ydup2(ж<TLS> tls, int32 old, int32 new1);

public static partial int32 /*r*/ Ydup3(ж<TLS> tls, int32 old, int32 new1, int32 flags);

public static partial Tlocale_t /*r*/ Yduplocale(ж<TLS> tls, Tlocale_t old);

public static partial int32 /*r*/ Yeaccess(ж<TLS> tls, uintptr filename, int32 amode);

public static partial uintptr /*r*/ Yecvt(ж<TLS> tls, float64 x, int32 n, uintptr dp, uintptr sign);

public static partial void Yencrypt(ж<TLS> tls, uintptr block, int32 edflag);

public static partial void Yendgrent(ж<TLS> tls);

public static partial void Yendhostent(ж<TLS> tls);

public static partial int32 /*r*/ Yendmntent(ж<TLS> tls, uintptr f);

public static partial void Yendnetent(ж<TLS> tls);

public static partial void Yendprotoent(ж<TLS> tls);

public static partial void Yendpwent(ж<TLS> tls);

public static partial void Yendservent(ж<TLS> tls);

public static partial void Yendspent(ж<TLS> tls);

public static partial void Yendusershell(ж<TLS> tls);

public static partial void Yendutent(ж<TLS> tls);

public static partial void Yendutxent(ж<TLS> tls);

public static partial int32 /*r*/ Yepoll_create(ж<TLS> tls, int32 size);

public static partial int32 /*r1*/ Yepoll_create1(ж<TLS> tls, int32 flags);

public static partial int32 /*r*/ Yepoll_ctl(ж<TLS> tls, int32 fd, int32 op, int32 fd2, uintptr ev);

public static partial int32 /*r1*/ Yepoll_pwait(ж<TLS> tls, int32 fd, uintptr ev, int32 cnt, int32 to, uintptr sigs);

public static partial int32 /*r*/ Yepoll_wait(ж<TLS> tls, int32 fd, uintptr ev, int32 cnt, int32 to);

public static partial float64 /*r*/ Yerand48(ж<TLS> tls, uintptr s);

public static partial float64 /*r1*/ Yerf(ж<TLS> tls, float64 x);

public static partial float64 /*r1*/ Yerfc(ж<TLS> tls, float64 x);

public static partial float32 /*r1*/ Yerfcf(ж<TLS> tls, float32 x);

public static partial float64 /*r*/ Yerfcl(ж<TLS> tls, float64 x);

public static partial float32 /*r1*/ Yerff(ж<TLS> tls, float32 x);

public static partial float64 /*r*/ Yerfl(ж<TLS> tls, float64 x);

public static partial void Yerr(ж<TLS> tls, int32 status, uintptr fmt, uintptr va);

public static partial void Yerrx(ж<TLS> tls, int32 status, uintptr fmt, uintptr va);

public static partial uintptr /*r*/ Yether_aton(ж<TLS> tls, uintptr x);

public static partial uintptr /*r*/ Yether_aton_r(ж<TLS> tls, uintptr x, uintptr p_a);

public static partial int32 /*r*/ Yether_hostton(ж<TLS> tls, uintptr hostname, uintptr e);

public static partial int32 /*r*/ Yether_line(ж<TLS> tls, uintptr l, uintptr e, uintptr hostname);

public static partial uintptr /*r*/ Yether_ntoa(ж<TLS> tls, uintptr p_a);

public static partial uintptr /*r*/ Yether_ntoa_r(ж<TLS> tls, uintptr p_a, uintptr x);

public static partial int32 /*r*/ Yether_ntohost(ж<TLS> tls, uintptr hostname, uintptr e);

public static partial int32 /*r*/ Yeuidaccess(ж<TLS> tls, uintptr filename, int32 amode);

public static partial int32 /*r1*/ Yeventfd(ж<TLS> tls, uint32 count, int32 flags);

public static partial int32 /*r*/ Yeventfd_read(ж<TLS> tls, int32 fd, uintptr value);

public static partial int32 /*r*/ Yeventfd_write(ж<TLS> tls, int32 fd, Teventfd_t _value);

public static partial int32 /*r*/ Yexecl(ж<TLS> tls, uintptr path, uintptr argv0, uintptr va);

public static partial int32 /*r*/ Yexecle(ж<TLS> tls, uintptr path, uintptr argv0, uintptr va);

public static partial int32 /*r*/ Yexeclp(ж<TLS> tls, uintptr @file, uintptr argv0, uintptr va);

public static partial int32 /*r*/ Yexecv(ж<TLS> tls, uintptr path, uintptr argv);

public static partial int32 /*r*/ Yexecve(ж<TLS> tls, uintptr path, uintptr argv, uintptr envp);

public static partial int32 /*r*/ Yexecvp(ж<TLS> tls, uintptr @file, uintptr argv);

public static partial int32 /*r*/ Yexecvpe(ж<TLS> tls, uintptr @file, uintptr argv, uintptr envp);

public static partial void Yexit(ж<TLS> tls, int32 code);

public static partial float64 /*r1*/ Yexp(ж<TLS> tls, float64 x1);

public static partial float64 /*r*/ Yexp10(ж<TLS> tls, float64 x);

public static partial float32 /*r*/ Yexp10f(ж<TLS> tls, float32 x);

public static partial float64 /*r*/ Yexp10l(ж<TLS> tls, float64 x);

public static partial float64 /*r1*/ Yexp2(ж<TLS> tls, float64 x1);

public static partial float32 /*r1*/ Yexp2f(ж<TLS> tls, float32 x2);

public static partial float64 /*r*/ Yexp2l(ж<TLS> tls, float64 x);

public static partial float32 /*r1*/ Yexpf(ж<TLS> tls, float32 x2);

public static partial float64 /*r*/ Yexpl(ж<TLS> tls, float64 x);

public static partial void Yexplicit_bzero(ж<TLS> tls, uintptr d, Tsize_t n);

public static partial float64 /*r*/ Yexpm1(ж<TLS> tls, float64 x3);

public static partial float32 /*r*/ Yexpm1f(ж<TLS> tls, float32 x3);

public static partial float64 /*r*/ Yexpm1l(ж<TLS> tls, float64 x);

public static partial float64 /*r*/ Yfabs(ж<TLS> tls, float64 x);

public static partial float32 /*r*/ Yfabsf(ж<TLS> tls, float32 x);

public static partial float64 /*r*/ Yfabsl(ж<TLS> tls, float64 x);

public static partial int32 /*r*/ Yfaccessat(ж<TLS> tls, int32 fd, uintptr filename, int32 amode, int32 flag);

public static partial int32 /*r*/ Yfallocate(ж<TLS> tls, int32 fd, int32 mode, Toff_t @base, Toff_t len1);

public static partial int32 /*r*/ Yfanotify_init(ж<TLS> tls, uint32 flags, uint32 event_f_flags);

public static partial int32 /*r*/ Yfanotify_mark(ж<TLS> tls, int32 fanotify_fd, uint32 flags, uint64 mask, int32 dfd, uintptr pathname);

public static partial int32 /*r*/ Yfchdir(ж<TLS> tls, int32 fd);

public static partial int32 /*r*/ Yfchmod(ж<TLS> tls, int32 fd, Tmode_t mode);

public static partial int32 /*r*/ Yfchmodat(ж<TLS> tls, int32 fd, uintptr path, Tmode_t mode, int32 flag);

public static partial int32 /*r*/ Yfchown(ж<TLS> tls, int32 fd, Tuid_t uid, Tgid_t gid);

public static partial int32 /*r*/ Yfchownat(ж<TLS> tls, int32 fd, uintptr path, Tuid_t uid, Tgid_t gid, int32 flag);

public static partial int32 /*r1*/ Yfclose(ж<TLS> tls, uintptr f);

public static partial int32 /*r*/ Yfcntl(ж<TLS> tls, int32 fd, int32 cmd, uintptr va);

public static partial int32 /*r*/ Yfcntl64(ж<TLS> tls, int32 fd, int32 cmd, uintptr va);

public static partial uintptr /*r*/ Yfcvt(ж<TLS> tls, float64 x, int32 n, uintptr dp, uintptr sign);

public static partial int32 /*r*/ Yfdatasync(ж<TLS> tls, int32 fd);

public static partial float64 /*r*/ Yfdim(ж<TLS> tls, float64 x, float64 y);

public static partial float32 /*r*/ Yfdimf(ж<TLS> tls, float32 x, float32 y);

public static partial float64 /*r*/ Yfdiml(ж<TLS> tls, float64 x, float64 y);

public static partial uintptr /*r*/ Yfdopen(ж<TLS> tls, int32 fd, uintptr mode);

public static partial uintptr /*r*/ Yfdopendir(ж<TLS> tls, int32 fd);

public static partial int32 /*r*/ Yfeclearexcept(ж<TLS> tls, int32 mask);

public static partial int32 /*r*/ Yfegetenv(ж<TLS> tls, uintptr envp);

public static partial int32 /*r*/ Yfegetround(ж<TLS> tls);

public static partial int32 /*r*/ Yfeof(ж<TLS> tls, uintptr f);

public static partial int32 /*r*/ Yfeof_unlocked(ж<TLS> tls, uintptr f);

public static partial int32 /*r*/ Yferaiseexcept(ж<TLS> tls, int32 mask);

public static partial int32 /*r*/ Yferror(ж<TLS> tls, uintptr f);

public static partial int32 /*r*/ Yferror_unlocked(ж<TLS> tls, uintptr f);

public static partial int32 /*r*/ Yfesetenv(ж<TLS> tls, uintptr envp);

public static partial int32 /*r*/ Yfetestexcept(ж<TLS> tls, int32 mask);

public static partial int32 /*r1*/ Yfexecve(ж<TLS> tls, int32 fd, uintptr argv, uintptr envp);

public static partial int32 /*r1*/ Yfflush(ж<TLS> tls, uintptr f);

public static partial int32 /*r*/ Yfflush_unlocked(ж<TLS> tls, uintptr f);

public static partial int32 /*r*/ Yffs(ж<TLS> tls, int32 i);

public static partial int32 /*r*/ Yffsl(ж<TLS> tls, int64 i);

public static partial int32 /*r*/ Yffsll(ж<TLS> tls, int64 i);

public static partial int32 /*r*/ Yfgetc(ж<TLS> tls, uintptr f1);

public static partial int32 /*r*/ Yfgetc_unlocked(ж<TLS> tls, uintptr f);

public static partial uintptr /*r*/ Yfgetgrent(ж<TLS> tls, uintptr f);

public static partial uintptr /*r*/ Yfgetln(ж<TLS> tls, uintptr f, uintptr plen);

public static partial int32 /*r*/ Yfgetpos(ж<TLS> tls, uintptr f, uintptr pos);

public static partial uintptr /*r*/ Yfgetpwent(ж<TLS> tls, uintptr f);

public static partial uintptr /*r*/ Yfgets(ж<TLS> tls, uintptr s, int32 n, uintptr f);

public static partial uintptr /*r*/ Yfgets_unlocked(ж<TLS> tls, uintptr s, int32 n, uintptr f);

public static partial Twint_t /*r*/ Yfgetwc(ж<TLS> tls, uintptr f);

public static partial Twint_t /*r*/ Yfgetwc_unlocked(ж<TLS> tls, uintptr f);

public static partial uintptr /*r*/ Yfgetws(ж<TLS> tls, uintptr s, int32 n, uintptr f);

public static partial uintptr /*r*/ Yfgetws_unlocked(ж<TLS> tls, uintptr s, int32 n, uintptr f);

public static partial Tssize_t /*r*/ Yfgetxattr(ж<TLS> tls, int32 filedes, uintptr name, uintptr value, Tsize_t size);

public static partial int32 /*r*/ Yfileno(ж<TLS> tls, uintptr f);

public static partial int32 /*r*/ Yfileno_unlocked(ж<TLS> tls, uintptr f);

public static partial int32 /*r*/ Yfinite(ж<TLS> tls, float64 x);

public static partial int32 /*r*/ Yfinitef(ж<TLS> tls, float32 x);

public static partial Tssize_t /*r*/ Yflistxattr(ж<TLS> tls, int32 filedes, uintptr list, Tsize_t size);

public static partial int32 /*r*/ Yflock(ж<TLS> tls, int32 fd, int32 op);

public static partial void Yflockfile(ж<TLS> tls, uintptr f);

public static partial float64 /*r*/ Yfloor(ж<TLS> tls, float64 x3);

public static partial float32 /*r*/ Yfloorf(ж<TLS> tls, float32 x3);

public static partial float64 /*r*/ Yfloorl(ж<TLS> tls, float64 x);

public static partial float64 /*r1*/ Yfma(ж<TLS> tls, float64 x1, float64 y, float64 z);

public static partial float64 /*r*/ Yfmal(ж<TLS> tls, float64 x, float64 y, float64 z);

public static partial float64 /*r*/ Yfmax(ж<TLS> tls, float64 x, float64 y);

public static partial float32 /*r*/ Yfmaxf(ж<TLS> tls, float32 x, float32 y);

public static partial float64 /*r*/ Yfmaxl(ж<TLS> tls, float64 x, float64 y);

public static partial uintptr /*r*/ Yfmemopen(ж<TLS> tls, uintptr buf, Tsize_t size, uintptr mode);

public static partial float64 /*r*/ Yfmin(ж<TLS> tls, float64 x, float64 y);

public static partial float32 /*r*/ Yfminf(ж<TLS> tls, float32 x, float32 y);

public static partial float64 /*r*/ Yfminl(ж<TLS> tls, float64 x, float64 y);

public static partial float64 /*r*/ Yfmod(ж<TLS> tls, float64 x, float64 y);

public static partial float32 /*r*/ Yfmodf(ж<TLS> tls, float32 x, float32 y);

public static partial float64 /*r*/ Yfmodl(ж<TLS> tls, float64 x, float64 y);

public static partial int32 /*r*/ Yfmtmsg(ж<TLS> tls, int64 classification, uintptr label, int32 severity, uintptr text, uintptr action, uintptr tag);

public static partial int32 /*r*/ Yfnmatch(ж<TLS> tls, uintptr pat, uintptr str, int32 flags);

public static partial uintptr /*r*/ Yfopen(ж<TLS> tls, uintptr filename, uintptr mode);

public static partial uintptr /*r*/ Yfopen64(ж<TLS> tls, uintptr filename, uintptr mode);

public static partial uintptr /*r*/ Yfopencookie(ж<TLS> tls, uintptr cookie, uintptr mode, Tcookie_io_functions_t iofuncs);

public static partial int32 /*_1*/ Yfork(ж<TLS> t);

public static partial int64 /*r*/ Yfpathconf(ж<TLS> tls, int32 fd, int32 name);

public static partial int32 /*r*/ Yfprintf(ж<TLS> tls, uintptr f, uintptr fmt, uintptr va);

public static partial int32 /*r*/ Yfpurge(ж<TLS> tls, uintptr f);

public static partial int32 /*r*/ Yfputc(ж<TLS> tls, int32 c1, uintptr f1);

public static partial int32 /*r*/ Yfputc_unlocked(ж<TLS> tls, int32 c, uintptr f);

public static partial int32 /*r*/ Yfputs(ж<TLS> tls, uintptr s, uintptr f);

public static partial int32 /*r*/ Yfputs_unlocked(ж<TLS> tls, uintptr s, uintptr f);

public static partial Twint_t /*r*/ Yfputwc(ж<TLS> tls, Twchar_t c, uintptr f);

public static partial Twint_t /*r*/ Yfputwc_unlocked(ж<TLS> tls, Twchar_t c, uintptr f);

public static partial int32 /*r*/ Yfputws(ж<TLS> tls, uintptr _ws, uintptr f);

public static partial int32 /*r*/ Yfputws_unlocked(ж<TLS> tls, uintptr _ws, uintptr f);

public static partial Tsize_t /*r*/ Yfread(ж<TLS> tls, uintptr destv, Tsize_t size, Tsize_t nmemb, uintptr f);

public static partial Tsize_t /*r*/ Yfread_unlocked(ж<TLS> tls, uintptr destv, Tsize_t size, Tsize_t nmemb, uintptr f);

public static partial void Yfree(ж<TLS> tls, uintptr p);

public static partial void Yfreeaddrinfo(ж<TLS> tls, uintptr p);

public static partial void Yfreeifaddrs(ж<TLS> tls, uintptr ifp);

public static partial void Yfreelocale(ж<TLS> tls, Tlocale_t l);

public static partial int32 /*r*/ Yfremovexattr(ж<TLS> tls, int32 fd, uintptr name);

public static partial uintptr /*r*/ Yfreopen(ж<TLS> tls, uintptr filename, uintptr mode, uintptr f);

public static partial float64 /*r*/ Yfrexp(ж<TLS> tls, float64 x, uintptr e);

public static partial float32 /*r*/ Yfrexpf(ж<TLS> tls, float32 x, uintptr e);

public static partial float64 /*r*/ Yfrexpl(ж<TLS> tls, float64 x, uintptr e);

public static partial int32 /*r*/ Yfscanf(ж<TLS> tls, uintptr f, uintptr fmt, uintptr va);

public static partial int32 /*r*/ Yfseek(ж<TLS> tls, uintptr f, int64 off, int32 whence);

public static partial int32 /*r*/ Yfseeko(ж<TLS> tls, uintptr f, Toff_t off, int32 whence);

public static partial int32 /*r*/ Yfsetpos(ж<TLS> tls, uintptr f, uintptr pos);

public static partial int32 /*r*/ Yfsetxattr(ж<TLS> tls, int32 filedes, uintptr name, uintptr value, Tsize_t size, int32 flags);

public static partial int32 /*r*/ Yfstat(ж<TLS> tls, int32 fd, uintptr st);

public static partial int32 /*r*/ Yfstat64(ж<TLS> tls, int32 fd, uintptr st);

public static partial int32 /*r*/ Yfstatat(ж<TLS> tls, int32 fd, uintptr path, uintptr st, int32 flag);

public static partial int32 /*r*/ Yfstatfs(ж<TLS> tls, int32 fd, uintptr buf);

public static partial int32 /*r*/ Yfstatvfs(ж<TLS> tls, int32 fd, uintptr buf);

public static partial int32 /*r*/ Yfsync(ж<TLS> tls, int32 fd);

public static partial int64 /*r*/ Yftell(ж<TLS> tls, uintptr f);

public static partial Toff_t /*r*/ Yftello(ж<TLS> tls, uintptr f);

public static partial int32 /*r*/ Yftime(ж<TLS> tls, uintptr tp);

public static partial Tkey_t /*r*/ Yftok(ж<TLS> tls, uintptr path, int32 id);

public static partial int32 /*r*/ Yftruncate(ж<TLS> tls, int32 fd, Toff_t length);

public static partial int32 /*r*/ Yftruncate64(ж<TLS> tls, int32 fd, Toff_t length);

public static partial int32 /*r*/ Yftrylockfile(ж<TLS> tls, uintptr f);

public static partial int32 /*_2*/ Yfts64_close(ж<TLS> t, uintptr ftsp);

public static partial uintptr /*_4*/ Yfts64_open(ж<TLS> t, uintptr path_argv, int32 options, uintptr compar);

public static partial uintptr /*_2*/ Yfts64_read(ж<TLS> t, uintptr ftsp);

public static partial int32 /*_2*/ Yfts_close(ж<TLS> t, uintptr ftsp);

public static partial uintptr /*_4*/ Yfts_open(ж<TLS> t, uintptr path_argv, int32 options, uintptr compar);

public static partial uintptr /*_2*/ Yfts_read(ж<TLS> t, uintptr ftsp);

public static partial int32 /*r*/ Yftw(ж<TLS> tls, uintptr path, uintptr __ccgo_fp_fn, int32 fd_limit);

internal static partial int32 /*_4*/ __ccgo_abi0_ftw_1(ж<TLS> _0, uintptr _1, uintptr _2, int32 _3, uintptr __ccgo_fp);

internal static void __ccgo_abiInternal_ftw_1(ref TLS tls, uintptr dest, uintptr abi0CodePtr) {
    ref var f = ref heap<Func<ж<TLS>, uintptr, uintptr, int32, int32>>((ж<TLS> _0, uintptr _1, uintptr _2, int32 _3) => {
        return __ccgo_abi0_ftw_1(_0, _1, _2, _3, abi0CodePtr);
    }, out var Ꮡf);
    @unsafe.Pointer.StoreArray<uintptr>((@unsafe.Pointer)dest, 2, @unsafe.Pointer.LoadArray<uintptr>((@unsafe.Pointer)((uintptr)GoFuncCookie.Of(f)), 2));
}

public static partial void Yfunlockfile(ж<TLS> tls, uintptr f);

public static partial int32 /*r*/ Yfutimens(ж<TLS> tls, int32 fd, uintptr times);

public static partial int32 /*r*/ Yfutimes(ж<TLS> tls, int32 fd, uintptr tv);

public static partial int32 /*r*/ Yfutimesat(ж<TLS> tls, int32 dirfd, uintptr pathname, uintptr times);

public static partial int32 /*r*/ Yfwide(ж<TLS> tls, uintptr f, int32 mode);

public static partial int32 /*r*/ Yfwprintf(ж<TLS> tls, uintptr f, uintptr fmt, uintptr va);

public static partial Tsize_t /*r*/ Yfwrite(ж<TLS> tls, uintptr src, Tsize_t size, Tsize_t nmemb, uintptr f);

public static partial Tsize_t /*r*/ Yfwrite_unlocked(ж<TLS> tls, uintptr src, Tsize_t size, Tsize_t nmemb, uintptr f);

public static partial int32 /*r*/ Yfwscanf(ж<TLS> tls, uintptr f, uintptr fmt, uintptr va);

public static partial uintptr /*r*/ Ygai_strerror(ж<TLS> tls, int32 ecode);

public static partial uintptr /*r*/ Ygcvt(ж<TLS> tls, float64 x, int32 n, uintptr b);

public static partial int64 /*r*/ Yget_avphys_pages(ж<TLS> tls);

public static partial uintptr /*r*/ Yget_current_dir_name(ж<TLS> tls);

public static partial int32 /*r*/ Yget_nprocs(ж<TLS> tls);

public static partial int32 /*r*/ Yget_nprocs_conf(ж<TLS> tls);

public static partial int64 /*r*/ Yget_phys_pages(ж<TLS> tls);

public static partial int32 /*r1*/ Ygetaddrinfo(ж<TLS> tls, uintptr host, uintptr serv, uintptr hint, uintptr res);

public static partial uint64 /*r*/ Ygetauxval(ж<TLS> tls, uint64 item);

public static partial int32 /*r*/ Ygetc(ж<TLS> tls, uintptr f1);

public static partial int32 /*r*/ Ygetc_unlocked(ж<TLS> tls, uintptr f);

public static partial int32 /*r*/ Ygetchar(ж<TLS> tls);

public static partial int32 /*r*/ Ygetchar_unlocked(ж<TLS> tls);

public static partial uintptr /*r*/ Ygetcwd(ж<TLS> tls, uintptr buf, Tsize_t size);

public static partial uintptr /*r*/ Ygetdate(ж<TLS> tls, uintptr s);

public static partial Tssize_t /*r*/ Ygetdelim(ж<TLS> tls, uintptr s, uintptr n, int32 delim, uintptr f);

public static partial int32 /*r*/ Ygetdents(ж<TLS> tls, int32 fd, uintptr buf, Tsize_t len1);

public static partial int32 /*r*/ Ygetdomainname(ж<TLS> tls, uintptr name, Tsize_t len1);

public static partial int32 /*r*/ Ygetdtablesize(ж<TLS> tls);

public static partial Tgid_t /*r*/ Ygetegid(ж<TLS> tls);

public static partial int32 /*r*/ Ygetentropy(ж<TLS> tls, uintptr buffer, Tsize_t len1);

public static partial uintptr /*r*/ Ygetenv(ж<TLS> tls, uintptr name);

public static partial Tuid_t /*r*/ Ygeteuid(ж<TLS> tls);

public static partial Tgid_t /*r*/ Ygetgid(ж<TLS> tls);

public static partial uintptr /*r*/ Ygetgrent(ж<TLS> tls);

public static partial uintptr /*r*/ Ygetgrgid(ж<TLS> tls, Tgid_t gid);

public static partial int32 /*r*/ Ygetgrgid_r(ж<TLS> tls, Tgid_t gid, uintptr gr, uintptr buf, Tsize_t size, uintptr res);

public static partial uintptr /*r*/ Ygetgrnam(ж<TLS> tls, uintptr name);

public static partial int32 /*r*/ Ygetgrnam_r(ж<TLS> tls, uintptr name, uintptr gr, uintptr buf, Tsize_t size, uintptr res);

public static partial int32 /*r*/ Ygetgrouplist(ж<TLS> tls, uintptr user, Tgid_t gid, uintptr groups, uintptr ngroups);

public static partial int32 /*r*/ Ygetgroups(ж<TLS> tls, int32 count, uintptr list);

public static partial uintptr /*r*/ Ygethostbyaddr(ж<TLS> tls, uintptr a, Tsocklen_t l, int32 af);

public static partial int32 /*r*/ Ygethostbyaddr_r(ж<TLS> tls, uintptr a, Tsocklen_t l, int32 af, uintptr h, uintptr buf, Tsize_t buflen, uintptr res, uintptr err);

public static partial uintptr /*r*/ Ygethostbyname(ж<TLS> tls, uintptr name);

public static partial uintptr /*r*/ Ygethostbyname2(ж<TLS> tls, uintptr name, int32 af);

public static partial int32 /*r*/ Ygethostbyname2_r(ж<TLS> tls, uintptr name, int32 af, uintptr h, uintptr buf, Tsize_t buflen, uintptr res, uintptr err);

public static partial int32 /*r*/ Ygethostbyname_r(ж<TLS> tls, uintptr name, uintptr h, uintptr buf, Tsize_t buflen, uintptr res, uintptr err);

public static partial uintptr /*r*/ Ygethostent(ж<TLS> tls);

public static partial int64 /*r*/ Ygethostid(ж<TLS> tls);

public static partial int32 /*r*/ Ygethostname(ж<TLS> tls, uintptr name, Tsize_t len1);

public static partial int32 /*r1*/ Ygetifaddrs(ж<TLS> tls, uintptr ifap);

public static partial int32 /*r1*/ Ygetitimer(ж<TLS> tls, int32 which, uintptr old);

public static partial Tssize_t /*r*/ Ygetline(ж<TLS> tls, uintptr s, uintptr n, uintptr f);

public static partial int32 /*r*/ Ygetloadavg(ж<TLS> tls, uintptr a, int32 n);

public static partial uintptr /*r*/ Ygetlogin(ж<TLS> tls);

public static partial int32 /*r*/ Ygetlogin_r(ж<TLS> tls, uintptr name, Tsize_t size);

public static partial uintptr /*r*/ Ygetmntent(ж<TLS> tls, uintptr f);

public static partial uintptr /*r*/ Ygetmntent_r(ж<TLS> tls, uintptr f, uintptr mnt, uintptr linebuf, int32 buflen);

public static partial int32 /*r*/ Ygetnameinfo(ж<TLS> tls, uintptr sa, Tsocklen_t sl, uintptr node, Tsocklen_t nodelen, uintptr serv, Tsocklen_t servlen, int32 flags);

public static partial uintptr /*r*/ Ygetnetbyaddr(ж<TLS> tls, Tuint32_t net, int32 type1);

public static partial uintptr /*r*/ Ygetnetbyname(ж<TLS> tls, uintptr name);

public static partial uintptr /*r*/ Ygetnetent(ж<TLS> tls);

public static partial int32 /*r*/ Ygetopt(ж<TLS> tls, int32 argc, uintptr argv, uintptr optstring);

public static partial int32 /*r*/ Ygetopt_long(ж<TLS> tls, int32 argc, uintptr argv, uintptr optstring, uintptr longopts, uintptr idx);

public static partial int32 /*r*/ Ygetopt_long_only(ж<TLS> tls, int32 argc, uintptr argv, uintptr optstring, uintptr longopts, uintptr idx);

public static partial int32 /*r*/ Ygetpagesize(ж<TLS> tls);

public static partial uintptr /*r*/ Ygetpass(ж<TLS> tls, uintptr prompt);

public static partial int32 /*r1*/ Ygetpeername(ж<TLS> tls, int32 fd, uintptr addr, uintptr len1);

public static partial Tpid_t /*r*/ Ygetpgid(ж<TLS> tls, Tpid_t pid);

public static partial Tpid_t /*r*/ Ygetpgrp(ж<TLS> tls);

public static partial Tpid_t /*r*/ Ygetpid(ж<TLS> tls);

public static partial Tpid_t /*r*/ Ygetppid(ж<TLS> tls);

public static partial int32 /*r*/ Ygetpriority(ж<TLS> tls, int32 which, Tid_t who);

public static partial uintptr /*r*/ Ygetprotobyname(ж<TLS> tls, uintptr name);

public static partial uintptr /*r*/ Ygetprotobynumber(ж<TLS> tls, int32 num);

public static partial uintptr /*r*/ Ygetprotoent(ж<TLS> tls);

public static partial uintptr /*r*/ Ygetpwent(ж<TLS> tls);

public static partial uintptr /*r*/ Ygetpwnam(ж<TLS> tls, uintptr name);

public static partial int32 /*r*/ Ygetpwnam_r(ж<TLS> tls, uintptr name, uintptr pw, uintptr buf, Tsize_t size, uintptr res);

public static partial uintptr /*r*/ Ygetpwuid(ж<TLS> tls, Tuid_t uid);

public static partial int32 /*r*/ Ygetpwuid_r(ж<TLS> tls, Tuid_t uid, uintptr pw, uintptr buf, Tsize_t size, uintptr res);

public static partial Tssize_t /*r*/ Ygetrandom(ж<TLS> tls, uintptr buf, Tsize_t buflen, uint32 flags);

public static partial int32 /*r*/ Ygetresgid(ж<TLS> tls, uintptr rgid, uintptr egid, uintptr sgid);

public static partial int32 /*r*/ Ygetresuid(ж<TLS> tls, uintptr ruid, uintptr euid, uintptr suid);

public static partial int32 /*r*/ Ygetrlimit(ж<TLS> tls, int32 resource, uintptr rlim);

public static partial int32 /*r*/ Ygetrlimit64(ж<TLS> tls, int32 resource, uintptr rlim);

public static partial int32 /*r1*/ Ygetrusage(ж<TLS> tls, int32 who, uintptr ru);

public static partial uintptr /*r*/ Ygets(ж<TLS> tls, uintptr s);

public static partial uintptr /*r*/ Ygetservbyname(ж<TLS> tls, uintptr name, uintptr prots);

public static partial int32 /*r*/ Ygetservbyname_r(ж<TLS> tls, uintptr name, uintptr prots, uintptr se, uintptr buf, Tsize_t buflen, uintptr res);

public static partial uintptr /*r*/ Ygetservent(ж<TLS> tls);

public static partial Tpid_t /*r*/ Ygetsid(ж<TLS> tls, Tpid_t pid);

public static partial int32 /*r1*/ Ygetsockname(ж<TLS> tls, int32 fd, uintptr addr, uintptr len1);

public static partial int32 /*r2*/ Ygetsockopt(ж<TLS> tls, int32 fd, int32 level, int32 optname, uintptr optval, uintptr optlen);

public static partial uintptr /*r*/ Ygetspent(ж<TLS> tls);

public static partial int32 /*r*/ Ygetsubopt(ж<TLS> tls, uintptr opt, uintptr keys, uintptr val);

public static partial uintptr /*r*/ Ygettext(ж<TLS> tls, uintptr msgid);

public static partial int32 /*r*/ Ygettimeofday(ж<TLS> tls, uintptr tv, uintptr tz);

public static partial Tuid_t /*r*/ Ygetuid(ж<TLS> tls);

public static partial uintptr /*r*/ Ygetusershell(ж<TLS> tls);

public static partial uintptr /*r*/ Ygetutent(ж<TLS> tls);

public static partial uintptr /*r*/ Ygetutid(ж<TLS> tls, uintptr ut);

public static partial uintptr /*r*/ Ygetutline(ж<TLS> tls, uintptr ut);

public static partial uintptr /*r*/ Ygetutxent(ж<TLS> tls);

public static partial uintptr /*r*/ Ygetutxid(ж<TLS> tls, uintptr ut);

public static partial uintptr /*r*/ Ygetutxline(ж<TLS> tls, uintptr ut);

public static partial int32 /*r*/ Ygetw(ж<TLS> tls, uintptr f);

public static partial Twint_t /*r*/ Ygetwc(ж<TLS> tls, uintptr f);

public static partial Twint_t /*r*/ Ygetwc_unlocked(ж<TLS> tls, uintptr f);

public static partial Twint_t /*r*/ Ygetwchar(ж<TLS> tls);

public static partial Twint_t /*r*/ Ygetwchar_unlocked(ж<TLS> tls);

public static partial Tssize_t /*r*/ Ygetxattr(ж<TLS> tls, uintptr path, uintptr name, uintptr value, Tsize_t size);

public static partial int32 /*r*/ Yglob(ж<TLS> tls, uintptr pat, int32 flags, uintptr __ccgo_fp_errfunc, uintptr g_);

internal static partial int32 /*_3*/ __ccgo_abi0_glob_2(ж<TLS> _0, uintptr _1, int32 _2, uintptr __ccgo_fp);

internal static void __ccgo_abiInternal_glob_2(ref TLS tls, uintptr dest, uintptr abi0CodePtr) {
    ref var f = ref heap<Func<ж<TLS>, uintptr, int32, int32>>((ж<TLS> _0, uintptr _1, int32 _2) => {
        return __ccgo_abi0_glob_2(_0, _1, _2, abi0CodePtr);
    }, out var Ꮡf);
    @unsafe.Pointer.StoreArray<uintptr>((@unsafe.Pointer)dest, 2, @unsafe.Pointer.LoadArray<uintptr>((@unsafe.Pointer)((uintptr)GoFuncCookie.Of(f)), 2));
}

public static partial void Yglobfree(ж<TLS> tls, uintptr g_);

public static partial uintptr /*r*/ Ygmtime(ж<TLS> tls, uintptr t);

public static partial uintptr /*r*/ Ygmtime_r(ж<TLS> tls, uintptr t, uintptr tm);

public static partial int32 /*r*/ Ygrantpt(ж<TLS> tls, int32 fd);

public static partial uintptr /*r*/ Yhasmntopt(ж<TLS> tls, uintptr mnt, uintptr opt);

public static partial int32 /*r*/ Yhcreate(ж<TLS> tls, Tsize_t nel);

public static partial void Yhdestroy(ж<TLS> tls);

public static partial void Yherror(ж<TLS> tls, uintptr msg);

public static partial uintptr /*r*/ Yhsearch(ж<TLS> tls, TENTRY item, TACTION action);

public static partial uintptr /*r*/ Yhstrerror(ж<TLS> tls, int32 ecode);

public static partial Tuint32_t /*r*/ Yhtonl(ж<TLS> tls, Tuint32_t n);

public static partial Tuint16_t /*r*/ Yhtons(ж<TLS> tls, Tuint16_t n);

public static partial float64 /*r*/ Yhypot(ж<TLS> tls, float64 x, float64 y);

public static partial float32 /*r*/ Yhypotf(ж<TLS> tls, float32 x, float32 y);

public static partial float64 /*r*/ Yhypotl(ж<TLS> tls, float64 x, float64 y);

public static partial Tsize_t /*r*/ Yiconv(ж<TLS> tls, Ticonv_t cd, uintptr @in, uintptr inb, uintptr @out, uintptr outb);

public static partial int32 /*r*/ Yiconv_close(ж<TLS> tls, Ticonv_t cd);

public static partial Ticonv_t /*r*/ Yiconv_open(ж<TLS> tls, uintptr to, uintptr from);

public static partial void Yif_freenameindex(ж<TLS> tls, uintptr idx);

public static partial uintptr /*r1*/ Yif_indextoname(ж<TLS> tls, uint32 index, uintptr name);

public static partial uintptr /*r*/ Yif_nameindex(ж<TLS> tls);

public static partial uint32 /*r1*/ Yif_nametoindex(ж<TLS> tls, uintptr name);

public static partial int32 /*r*/ Yilogb(ж<TLS> tls, float64 x3);

public static partial int32 /*r*/ Yilogbf(ж<TLS> tls, float32 x3);

public static partial int32 /*r*/ Yilogbl(ж<TLS> tls, float64 x);

public static partial Tintmax_t /*r*/ Yimaxabs(ж<TLS> tls, Tintmax_t a);

public static partial Timaxdiv_t /*r*/ Yimaxdiv(ж<TLS> tls, Tintmax_t num, Tintmax_t den);

public static partial uintptr /*r*/ Yindex(ж<TLS> tls, uintptr s, int32 c);

public static partial Tin_addr_t /*r*/ Yinet_addr(ж<TLS> tls, uintptr p);

public static partial int32 /*r*/ Yinet_aton(ж<TLS> tls, uintptr s0, uintptr dest);

public static partial Tin_addr_t /*r*/ Yinet_lnaof(ж<TLS> tls, Tin_addr @in);

public static partial Tin_addr /*r*/ Yinet_makeaddr(ж<TLS> tls, Tin_addr_t n, Tin_addr_t h);

public static partial Tin_addr_t /*r*/ Yinet_netof(ж<TLS> tls, Tin_addr @in);

public static partial Tin_addr_t /*r*/ Yinet_network(ж<TLS> tls, uintptr p);

public static partial uintptr /*r*/ Yinet_ntoa(ж<TLS> tls, Tin_addr _in);

public static partial uintptr /*r*/ Yinet_ntop(ж<TLS> tls, int32 af, uintptr a0, uintptr s, Tsocklen_t l);

public static partial int32 /*r*/ Yinet_pton(ж<TLS> tls, int32 af, uintptr s, uintptr a0);

public static partial int32 /*r*/ Yinit_module(ж<TLS> tls, uintptr a, uint64 b, uintptr c);

public static partial uintptr /*r*/ Yinitstate(ж<TLS> tls, uint32 seed, uintptr state, Tsize_t size);

public static partial int32 /*_5*/ Yinitstate_r(ж<TLS> t, uint32 seed, uintptr statebuf, Tsize_t statelen, uintptr buf);

public static partial int32 /*r*/ Yinotify_add_watch(ж<TLS> tls, int32 fd, uintptr pathname, Tuint32_t mask);

public static partial int32 /*r*/ Yinotify_init(ж<TLS> tls);

public static partial int32 /*r1*/ Yinotify_init1(ж<TLS> tls, int32 flags);

public static partial int32 /*r*/ Yinotify_rm_watch(ж<TLS> tls, int32 fd, int32 wd);

public static partial void Yinsque(ж<TLS> tls, uintptr element, uintptr pred);

public static partial int32 /*r1*/ Yioctl(ж<TLS> tls, int32 fd, int32 req, uintptr va);

public static partial int32 /*r*/ Yioperm(ж<TLS> tls, uint64 from, uint64 num, int32 turn_on);

public static partial int32 /*r*/ Yiopl(ж<TLS> tls, int32 level);

public static partial int32 /*r*/ Yisalnum(ж<TLS> tls, int32 c);

public static partial int32 /*r*/ Yisalnum_l(ж<TLS> tls, int32 c, Tlocale_t l);

public static partial int32 /*r*/ Yisalpha(ж<TLS> tls, int32 c);

public static partial int32 /*r*/ Yisalpha_l(ж<TLS> tls, int32 c, Tlocale_t l);

public static partial int32 /*r*/ Yisascii(ж<TLS> tls, int32 c);

public static partial int32 /*r*/ Yisastream(ж<TLS> tls, int32 fd);

public static partial int32 /*r1*/ Yisatty(ж<TLS> tls, int32 fd);

public static partial int32 /*r*/ Yisblank(ж<TLS> tls, int32 c);

public static partial int32 /*r*/ Yisblank_l(ж<TLS> tls, int32 c, Tlocale_t l);

public static partial int32 /*r*/ Yiscntrl(ж<TLS> tls, int32 c);

public static partial int32 /*r*/ Yiscntrl_l(ж<TLS> tls, int32 c, Tlocale_t l);

public static partial int32 /*r*/ Yisdigit(ж<TLS> tls, int32 c);

public static partial int32 /*r*/ Yisdigit_l(ж<TLS> tls, int32 c, Tlocale_t l);

public static partial int32 /*r*/ Yisgraph(ж<TLS> tls, int32 c);

public static partial int32 /*r*/ Yisgraph_l(ж<TLS> tls, int32 c, Tlocale_t l);

public static partial int32 /*r*/ Yislower(ж<TLS> tls, int32 c);

public static partial int32 /*r*/ Yislower_l(ж<TLS> tls, int32 c, Tlocale_t l);

public static partial int32 /*_2*/ Yisnan(ж<TLS> t, float64 x);

public static partial int32 /*_2*/ Yisnanf(ж<TLS> t, float32 arg);

public static partial int32 /*_2*/ Yisnanl(ж<TLS> t, float64 arg);

public static partial int32 /*r*/ Yisprint(ж<TLS> tls, int32 c);

public static partial int32 /*r*/ Yisprint_l(ж<TLS> tls, int32 c, Tlocale_t l);

public static partial int32 /*r*/ Yispunct(ж<TLS> tls, int32 c);

public static partial int32 /*r*/ Yispunct_l(ж<TLS> tls, int32 c, Tlocale_t l);

public static partial int32 /*r*/ Yissetugid(ж<TLS> tls);

public static partial int32 /*r*/ Yisspace(ж<TLS> tls, int32 c);

public static partial int32 /*r*/ Yisspace_l(ж<TLS> tls, int32 c, Tlocale_t l);

public static partial int32 /*r*/ Yisupper(ж<TLS> tls, int32 c);

public static partial int32 /*r*/ Yisupper_l(ж<TLS> tls, int32 c, Tlocale_t l);

public static partial int32 /*r*/ Yiswalnum(ж<TLS> tls, Twint_t wc);

public static partial int32 /*r*/ Yiswalnum_l(ж<TLS> tls, Twint_t c, Tlocale_t l);

public static partial int32 /*r*/ Yiswalpha(ж<TLS> tls, Twint_t wc);

public static partial int32 /*r*/ Yiswalpha_l(ж<TLS> tls, Twint_t c, Tlocale_t l);

public static partial int32 /*r*/ Yiswblank(ж<TLS> tls, Twint_t wc);

public static partial int32 /*r*/ Yiswblank_l(ж<TLS> tls, Twint_t c, Tlocale_t l);

public static partial int32 /*r*/ Yiswcntrl(ж<TLS> tls, Twint_t wc);

public static partial int32 /*r*/ Yiswcntrl_l(ж<TLS> tls, Twint_t c, Tlocale_t l);

public static partial int32 /*r*/ Yiswctype(ж<TLS> tls, Twint_t wc, Twctype_t type1);

public static partial int32 /*r*/ Yiswctype_l(ж<TLS> tls, Twint_t c, Twctype_t t, Tlocale_t l);

public static partial int32 /*r*/ Yiswdigit(ж<TLS> tls, Twint_t wc);

public static partial int32 /*r*/ Yiswdigit_l(ж<TLS> tls, Twint_t c, Tlocale_t l);

public static partial int32 /*r*/ Yiswgraph(ж<TLS> tls, Twint_t wc);

public static partial int32 /*r*/ Yiswgraph_l(ж<TLS> tls, Twint_t c, Tlocale_t l);

public static partial int32 /*r*/ Yiswlower(ж<TLS> tls, Twint_t wc);

public static partial int32 /*r*/ Yiswlower_l(ж<TLS> tls, Twint_t c, Tlocale_t l);

public static partial int32 /*r*/ Yiswprint(ж<TLS> tls, Twint_t wc);

public static partial int32 /*r*/ Yiswprint_l(ж<TLS> tls, Twint_t c, Tlocale_t l);

public static partial int32 /*r*/ Yiswpunct(ж<TLS> tls, Twint_t wc);

public static partial int32 /*r*/ Yiswpunct_l(ж<TLS> tls, Twint_t c, Tlocale_t l);

public static partial int32 /*r*/ Yiswspace(ж<TLS> tls, Twint_t wc);

public static partial int32 /*r*/ Yiswspace_l(ж<TLS> tls, Twint_t c, Tlocale_t l);

public static partial int32 /*r*/ Yiswupper(ж<TLS> tls, Twint_t wc);

public static partial int32 /*r*/ Yiswupper_l(ж<TLS> tls, Twint_t c, Tlocale_t l);

public static partial int32 /*r*/ Yiswxdigit(ж<TLS> tls, Twint_t wc);

public static partial int32 /*r*/ Yiswxdigit_l(ж<TLS> tls, Twint_t c, Tlocale_t l);

public static partial int32 /*r*/ Yisxdigit(ж<TLS> tls, int32 c);

public static partial int32 /*r*/ Yisxdigit_l(ж<TLS> tls, int32 c, Tlocale_t l);

public static partial float64 /*r1*/ Yj0(ж<TLS> tls, float64 x);

public static partial float32 /*r1*/ Yj0f(ж<TLS> tls, float32 x);

public static partial float64 /*r1*/ Yj1(ж<TLS> tls, float64 x);

public static partial float32 /*r1*/ Yj1f(ж<TLS> tls, float32 x);

public static partial float64 /*r*/ Yjn(ж<TLS> tls, int32 n, float64 x);

public static partial float32 /*r*/ Yjnf(ж<TLS> tls, int32 n, float32 x);

public static partial int64 /*r*/ Yjrand48(ж<TLS> tls, uintptr s);

public static partial int32 /*r*/ Ykill(ж<TLS> tls, Tpid_t pid, int32 sig);

public static partial int32 /*r*/ Ykillpg(ж<TLS> tls, Tpid_t pgid, int32 sig);

public static partial int32 /*r*/ Yklogctl(ж<TLS> tls, int32 type1, uintptr buf, int32 len1);

public static partial uintptr /*r*/ Yl64a(ж<TLS> tls, int64 x0);

public static partial int64 /*r*/ Ylabs(ж<TLS> tls, int64 a);

public static partial int32 /*r*/ Ylchmod(ж<TLS> tls, uintptr path, Tmode_t mode);

public static partial int32 /*r*/ Ylchown(ж<TLS> tls, uintptr path, Tuid_t uid, Tgid_t gid);

public static partial int32 /*r*/ Ylckpwdf(ж<TLS> tls);

public static partial void Ylcong48(ж<TLS> tls, uintptr p);

public static partial float64 /*r*/ Yldexp(ж<TLS> tls, float64 x, int32 n);

public static partial float32 /*r*/ Yldexpf(ж<TLS> tls, float32 x, int32 n);

public static partial float64 /*r*/ Yldexpl(ж<TLS> tls, float64 x, int32 n);

public static partial Tldiv_t /*r*/ Yldiv(ж<TLS> tls, int64 num, int64 den);

public static partial uintptr /*r*/ Ylfind(ж<TLS> tls, uintptr key, uintptr @base, uintptr nelp, Tsize_t width, uintptr __ccgo_fp_compar);

internal static partial int32 /*_3*/ __ccgo_abi0_lfind_4(ж<TLS> _0, uintptr _1, uintptr _2, uintptr __ccgo_fp);

internal static void __ccgo_abiInternal_lfind_4(ref TLS tls, uintptr dest, uintptr abi0CodePtr) {
    ref var f = ref heap<Func<ж<TLS>, uintptr, uintptr, int32>>((ж<TLS> _0, uintptr _1, uintptr _2) => {
        return __ccgo_abi0_lfind_4(_0, _1, _2, abi0CodePtr);
    }, out var Ꮡf);
    @unsafe.Pointer.StoreArray<uintptr>((@unsafe.Pointer)dest, 2, @unsafe.Pointer.LoadArray<uintptr>((@unsafe.Pointer)((uintptr)GoFuncCookie.Of(f)), 2));
}

public static partial float64 /*r*/ Ylgamma(ж<TLS> tls, float64 x);

public static partial float64 /*r*/ Ylgamma_r(ж<TLS> tls, float64 x, uintptr signgamp);

public static partial float32 /*r*/ Ylgammaf(ж<TLS> tls, float32 x);

public static partial float32 /*r*/ Ylgammaf_r(ж<TLS> tls, float32 x, uintptr signgamp);

public static partial float64 /*r*/ Ylgammal(ж<TLS> tls, float64 x);

public static partial float64 /*r*/ Ylgammal_r(ж<TLS> tls, float64 x, uintptr sg);

public static partial Tssize_t /*r*/ Ylgetxattr(ж<TLS> tls, uintptr path, uintptr name, uintptr value, Tsize_t size);

public static partial int32 /*r*/ Ylink(ж<TLS> tls, uintptr existing, uintptr new1);

public static partial int32 /*r*/ Ylinkat(ж<TLS> tls, int32 fd1, uintptr existing, int32 fd2, uintptr new1, int32 flag);

public static partial int32 /*r1*/ Ylisten(ж<TLS> tls, int32 fd, int32 backlog);

public static partial Tssize_t /*r*/ Ylistxattr(ж<TLS> tls, uintptr path, uintptr list, Tsize_t size);

public static partial int64 /*r*/ Yllabs(ж<TLS> tls, int64 a);

public static partial Tlldiv_t /*r*/ Ylldiv(ж<TLS> tls, int64 num, int64 den);

public static partial Tssize_t /*r*/ Yllistxattr(ж<TLS> tls, uintptr path, uintptr list, Tsize_t size);

public static partial int64 /*r*/ Yllrint(ж<TLS> tls, float64 x);

public static partial int64 /*r*/ Yllrintf(ж<TLS> tls, float32 x);

public static partial int64 /*r*/ Yllrintl(ж<TLS> tls, float64 x);

public static partial int64 /*r*/ Yllround(ж<TLS> tls, float64 x);

public static partial int64 /*r*/ Yllroundf(ж<TLS> tls, float32 x);

public static partial int64 /*r*/ Yllroundl(ж<TLS> tls, float64 x);

public static partial uintptr /*r*/ Ylocaleconv(ж<TLS> tls);

public static partial uintptr /*r*/ Ylocaltime(ж<TLS> tls, uintptr t);

public static partial uintptr /*r*/ Ylocaltime_r(ж<TLS> tls, uintptr t, uintptr tm);

public static partial int32 /*r*/ Ylockf(ж<TLS> tls, int32 fd, int32 op, Toff_t size);

public static partial float64 /*r1*/ Ylog(ж<TLS> tls, float64 x1);

public static partial float64 /*r*/ Ylog10(ж<TLS> tls, float64 x);

public static partial float32 /*r*/ Ylog10f(ж<TLS> tls, float32 x);

public static partial float64 /*r*/ Ylog10l(ж<TLS> tls, float64 x);

public static partial float64 /*r*/ Ylog1p(ж<TLS> tls, float64 x3);

public static partial float32 /*r*/ Ylog1pf(ж<TLS> tls, float32 x3);

public static partial float64 /*r*/ Ylog1pl(ж<TLS> tls, float64 x);

public static partial float64 /*r1*/ Ylog2(ж<TLS> tls, float64 x1);

public static partial float32 /*r1*/ Ylog2f(ж<TLS> tls, float32 x1);

public static partial float64 /*r*/ Ylog2l(ж<TLS> tls, float64 x);

public static partial float64 /*r*/ Ylogb(ж<TLS> tls, float64 x);

public static partial float32 /*r*/ Ylogbf(ж<TLS> tls, float32 x);

public static partial float64 /*r*/ Ylogbl(ж<TLS> tls, float64 x);

public static partial float32 /*r1*/ Ylogf(ж<TLS> tls, float32 x1);

public static partial int32 /*r*/ Ylogin_tty(ж<TLS> tls, int32 fd);

public static partial float64 /*r*/ Ylogl(ж<TLS> tls, float64 x);

public static partial void Ylongjmp(ж<TLS> t, uintptr env, int32 val);

public static partial int64 /*r*/ Ylrand48(ж<TLS> tls);

public static partial int32 /*r*/ Ylremovexattr(ж<TLS> tls, uintptr path, uintptr name);

public static partial int64 /*r*/ Ylrint(ж<TLS> tls, float64 x);

public static partial int64 /*r*/ Ylrintf(ж<TLS> tls, float32 x);

public static partial int64 /*r*/ Ylrintl(ж<TLS> tls, float64 x);

public static partial int64 /*r*/ Ylround(ж<TLS> tls, float64 x);

public static partial int64 /*r*/ Ylroundf(ж<TLS> tls, float32 x);

public static partial int64 /*r*/ Ylroundl(ж<TLS> tls, float64 x);

public static partial uintptr /*r*/ Ylsearch(ж<TLS> tls, uintptr key, uintptr @base, uintptr nelp, Tsize_t width, uintptr __ccgo_fp_compar);

internal static partial int32 /*_3*/ __ccgo_abi0_lsearch_4(ж<TLS> _0, uintptr _1, uintptr _2, uintptr __ccgo_fp);

internal static void __ccgo_abiInternal_lsearch_4(ref TLS tls, uintptr dest, uintptr abi0CodePtr) {
    ref var f = ref heap<Func<ж<TLS>, uintptr, uintptr, int32>>((ж<TLS> _0, uintptr _1, uintptr _2) => {
        return __ccgo_abi0_lsearch_4(_0, _1, _2, abi0CodePtr);
    }, out var Ꮡf);
    @unsafe.Pointer.StoreArray<uintptr>((@unsafe.Pointer)dest, 2, @unsafe.Pointer.LoadArray<uintptr>((@unsafe.Pointer)((uintptr)GoFuncCookie.Of(f)), 2));
}

public static partial Toff_t /*r*/ Ylseek(ж<TLS> tls, int32 fd, Toff_t offset, int32 whence);

public static partial Toff_t /*r*/ Ylseek64(ж<TLS> tls, int32 fd, Toff_t offset, int32 whence);

public static partial int32 /*r*/ Ylsetxattr(ж<TLS> tls, uintptr path, uintptr name, uintptr value, Tsize_t size, int32 flags);

public static partial int32 /*r*/ Ylstat(ж<TLS> tls, uintptr path, uintptr buf);

public static partial int32 /*r*/ Ylstat64(ж<TLS> tls, uintptr path, uintptr buf);

public static partial int32 /*r*/ Ylutimes(ж<TLS> tls, uintptr filename, uintptr tv);

public static partial int32 /*r*/ Ymadvise(ж<TLS> tls, uintptr addr, Tsize_t len1, int32 advice);

public static partial uintptr /*r*/ Ymalloc(ж<TLS> tls, Tsize_t n);

public static partial Tsize_t /*r*/ Ymalloc_usable_size(ж<TLS> tls, uintptr p);

public static partial int32 /*r*/ Ymblen(ж<TLS> tls, uintptr s, Tsize_t n);

public static partial Tsize_t /*r*/ Ymbrlen(ж<TLS> tls, uintptr s, Tsize_t n, uintptr st);

public static partial Tsize_t /*r*/ Ymbrtoc16(ж<TLS> tls, uintptr pc16, uintptr s, Tsize_t n, uintptr ps);

public static partial Tsize_t /*r*/ Ymbrtoc32(ж<TLS> tls, uintptr pc32, uintptr s, Tsize_t n, uintptr ps);

public static partial Tsize_t /*r*/ Ymbrtowc(ж<TLS> tls, uintptr wc, uintptr src, Tsize_t n, uintptr st);

public static partial int32 /*r*/ Ymbsinit(ж<TLS> tls, uintptr st);

public static partial Tsize_t /*r*/ Ymbsnrtowcs(ж<TLS> tls, uintptr wcs, uintptr src, Tsize_t n, Tsize_t wn, uintptr st);

public static partial Tsize_t /*r*/ Ymbsrtowcs(ж<TLS> tls, uintptr ws, uintptr src, Tsize_t wn, uintptr st);

public static partial Tsize_t /*r*/ Ymbstowcs(ж<TLS> tls, uintptr ws, uintptr _s, Tsize_t wn);

public static partial int32 /*r*/ Ymbtowc(ж<TLS> tls, uintptr wc, uintptr src, Tsize_t n);

public static partial uintptr /*r*/ Ymemccpy(ж<TLS> tls, uintptr dest, uintptr src, int32 c, Tsize_t n);

public static partial uintptr /*r*/ Ymemchr(ж<TLS> tls, uintptr src, int32 c, Tsize_t n);

public static partial int32 /*r1*/ Ymemcmp(ж<TLS> tls, uintptr vl, uintptr vr, Tsize_t n);

public static partial uintptr /*r*/ Ymemcpy(ж<TLS> tls, uintptr dest, uintptr src, Tsize_t n);

public static partial int32 /*r*/ Ymemfd_create(ж<TLS> tls, uintptr name, uint32 flags);

public static partial uintptr /*r*/ Ymemmem(ж<TLS> tls, uintptr h0, Tsize_t k, uintptr n0, Tsize_t l);

public static partial uintptr /*r*/ Ymemmove(ж<TLS> tls, uintptr dest, uintptr src, Tsize_t n);

public static partial uintptr /*r*/ Ymempcpy(ж<TLS> tls, uintptr dest, uintptr src, Tsize_t n);

public static partial uintptr /*r*/ Ymemrchr(ж<TLS> tls, uintptr m, int32 c, Tsize_t n);

public static partial uintptr /*r*/ Ymemset(ж<TLS> tls, uintptr dest, int32 c, Tsize_t n);

public static partial int32 /*r*/ Ymincore(ж<TLS> tls, uintptr addr, Tsize_t len1, uintptr vec);

public static partial int32 /*r*/ Ymkdir(ж<TLS> tls, uintptr path, Tmode_t mode);

public static partial int32 /*r*/ Ymkdirat(ж<TLS> tls, int32 fd, uintptr path, Tmode_t mode);

public static partial uintptr /*r*/ Ymkdtemp(ж<TLS> tls, uintptr template);

public static partial int32 /*r*/ Ymkfifo(ж<TLS> tls, uintptr path, Tmode_t mode);

public static partial int32 /*r*/ Ymkfifoat(ж<TLS> tls, int32 fd, uintptr path, Tmode_t mode);

public static partial int32 /*r*/ Ymknod(ж<TLS> tls, uintptr path, Tmode_t mode, Tdev_t dev);

public static partial int32 /*r*/ Ymknodat(ж<TLS> tls, int32 fd, uintptr path, Tmode_t mode, Tdev_t dev);

public static partial int32 /*r*/ Ymkostemp(ж<TLS> tls, uintptr template, int32 flags);

public static partial int32 /*r*/ Ymkostemps(ж<TLS> tls, uintptr template, int32 len1, int32 flags);

public static partial int32 /*r*/ Ymkstemp(ж<TLS> tls, uintptr template);

public static partial int32 /*r*/ Ymkstemp64(ж<TLS> tls, uintptr template);

public static partial int32 /*r*/ Ymkstemps(ж<TLS> tls, uintptr template, int32 len1);

public static partial int32 /*r*/ Ymkstemps64(ж<TLS> tls, uintptr template, int32 len1);

public static partial uintptr /*r*/ Ymktemp(ж<TLS> tls, uintptr template);

public static partial Ttime_t /*r*/ Ymktime(ж<TLS> tls, uintptr tm);

public static partial int32 /*r*/ Ymlock(ж<TLS> tls, uintptr addr, Tsize_t len1);

public static partial int32 /*r*/ Ymlock2(ж<TLS> tls, uintptr addr, Tsize_t len1, uint32 flags);

public static partial int32 /*r*/ Ymlockall(ж<TLS> tls, int32 flags);

public static partial uintptr /*r*/ Ymmap(ж<TLS> tls, uintptr start, Tsize_t len1, int32 prot, int32 flags, int32 fd, Toff_t off);

public static partial uintptr /*r*/ Ymmap64(ж<TLS> tls, uintptr start, Tsize_t len1, int32 prot, int32 flags, int32 fd, Toff_t off);

public static partial float64 /*r*/ Ymodf(ж<TLS> tls, float64 x, uintptr iptr);

public static partial float32 /*r*/ Ymodff(ж<TLS> tls, float32 x, uintptr iptr);

public static partial float64 /*r1*/ Ymodfl(ж<TLS> tls, float64 x, uintptr iptr);

public static partial int32 /*r*/ Ymount(ж<TLS> tls, uintptr special, uintptr dir, uintptr fstype, uint64 flags, uintptr data);

public static partial int32 /*r*/ Ymprotect(ж<TLS> tls, uintptr addr, Tsize_t len1, int32 prot);

public static partial int64 /*r*/ Ymrand48(ж<TLS> tls);

public static partial uintptr /*r*/ Ymremap(ж<TLS> tls, uintptr old_addr, Tsize_t old_len, Tsize_t new_len, int32 flags, uintptr va);

public static partial int32 /*r1*/ Ymsgctl(ж<TLS> tls, int32 q, int32 cmd, uintptr buf);

public static partial int32 /*r*/ Ymsgget(ж<TLS> tls, Tkey_t k, int32 flag);

public static partial Tssize_t /*r*/ Ymsgrcv(ж<TLS> tls, int32 q, uintptr m, Tsize_t len1, int64 type1, int32 flag);

public static partial int32 /*r*/ Ymsgsnd(ж<TLS> tls, int32 q, uintptr m, Tsize_t len1, int32 flag);

public static partial int32 /*r*/ Ymsync(ж<TLS> tls, uintptr start, Tsize_t len1, int32 flags);

public static partial int32 /*r*/ Ymunlock(ж<TLS> tls, uintptr addr, Tsize_t len1);

public static partial int32 /*r*/ Ymunlockall(ж<TLS> tls);

public static partial int32 /*r*/ Ymunmap(ж<TLS> tls, uintptr start, Tsize_t len1);

public static partial int32 /*r*/ Yname_to_handle_at(ж<TLS> tls, int32 dirfd, uintptr pathname, uintptr handle, uintptr mount_id, int32 flags);

public static partial float64 /*r*/ Ynan(ж<TLS> tls, uintptr s);

public static partial float32 /*r*/ Ynanf(ж<TLS> tls, uintptr s);

public static partial float64 /*r*/ Ynanl(ж<TLS> tls, uintptr s);

public static partial int32 /*r*/ Ynanosleep(ж<TLS> tls, uintptr req, uintptr rem);

public static partial Tlocale_t /*r*/ Ynewlocale(ж<TLS> tls, int32 mask, uintptr name, Tlocale_t loc);

public static partial float64 /*r*/ Ynextafter(ж<TLS> tls, float64 x3, float64 y3);

public static partial float32 /*r*/ Ynextafterf(ж<TLS> tls, float32 x3, float32 y3);

public static partial float64 /*r*/ Ynextafterl(ж<TLS> tls, float64 x, float64 y);

public static partial float64 /*r*/ Ynexttoward(ж<TLS> tls, float64 x, float64 y);

public static partial float32 /*r*/ Ynexttowardf(ж<TLS> tls, float32 x3, float64 y3);

public static partial float64 /*r*/ Ynexttowardl(ж<TLS> tls, float64 x, float64 y);

public static partial int32 /*r1*/ Ynftw(ж<TLS> tls, uintptr path, uintptr __ccgo_fp_fn, int32 fd_limit, int32 flags);

internal static partial int32 /*_5*/ __ccgo_abi0_nftw_1(ж<TLS> _0, uintptr _1, uintptr _2, int32 _3, uintptr _4, uintptr __ccgo_fp);

internal static void __ccgo_abiInternal_nftw_1(ref TLS tls, uintptr dest, uintptr abi0CodePtr) {
    ref var f = ref heap<Func<ж<TLS>, uintptr, uintptr, int32, uintptr, int32>>((ж<TLS> _0, uintptr _1, uintptr _2, int32 _3, uintptr _4) => {
        return __ccgo_abi0_nftw_1(_0, _1, _2, _3, _4, abi0CodePtr);
    }, out var Ꮡf);
    @unsafe.Pointer.StoreArray<uintptr>((@unsafe.Pointer)dest, 2, @unsafe.Pointer.LoadArray<uintptr>((@unsafe.Pointer)((uintptr)GoFuncCookie.Of(f)), 2));
}

public static partial uintptr /*r*/ Yngettext(ж<TLS> tls, uintptr msgid1, uintptr msgid2, uint64 n);

public static partial int32 /*r*/ Ynice(ж<TLS> tls, int32 inc);

public static partial uintptr /*r*/ Ynl_langinfo(ж<TLS> tls, Tnl_item item);

public static partial uintptr /*r*/ Ynl_langinfo_l(ж<TLS> tls, Tnl_item item, Tlocale_t loc);

public static partial int64 /*r*/ Ynrand48(ж<TLS> tls, uintptr s);

public static partial uint32 /*r*/ Yns_get16(ж<TLS> tls, uintptr cp);

public static partial uint64 /*r*/ Yns_get32(ж<TLS> tls, uintptr cp);

public static partial int32 /*r1*/ Yns_initparse(ж<TLS> tls, uintptr msg, int32 msglen, uintptr handle);

public static partial int32 /*r1*/ Yns_name_uncompress(ж<TLS> tls, uintptr msg, uintptr eom, uintptr src, uintptr dst, Tsize_t dstsiz);

public static partial int32 /*r1*/ Yns_parserr(ж<TLS> tls, uintptr handle, Tns_sect section, int32 rrnum, uintptr rr);

public static partial void Yns_put16(ж<TLS> tls, uint32 s, uintptr cp);

public static partial void Yns_put32(ж<TLS> tls, uint64 l, uintptr cp);

public static partial int32 /*r1*/ Yns_skiprr(ж<TLS> tls, uintptr ptr, uintptr eom, Tns_sect section, int32 count);

public static partial Tuint32_t /*r*/ Yntohl(ж<TLS> tls, Tuint32_t n);

public static partial Tuint16_t /*r*/ Yntohs(ж<TLS> tls, Tuint16_t n);

public static partial void Yobstack_free(ж<TLS> t, uintptr obstack, uintptr obj);

public static partial int32 /*_2*/ Yobstack_vprintf(ж<TLS> t, uintptr obstack, uintptr template, uintptr va);

public static partial int32 /*r*/ Yopen(ж<TLS> tls, uintptr filename, int32 flags, uintptr va);

public static partial int32 /*r*/ Yopen64(ж<TLS> tls, uintptr filename, int32 flags, uintptr va);

public static partial int32 /*r*/ Yopen_by_handle_at(ж<TLS> tls, int32 mount_fd, uintptr handle, int32 flags);

public static partial uintptr /*r*/ Yopen_memstream(ж<TLS> tls, uintptr bufp, uintptr sizep);

public static partial uintptr /*r*/ Yopen_wmemstream(ж<TLS> tls, uintptr bufp, uintptr sizep);

public static partial int32 /*r*/ Yopenat(ж<TLS> tls, int32 fd, uintptr filename, int32 flags, uintptr va);

public static partial uintptr /*r*/ Yopendir(ж<TLS> tls, uintptr name);

public static partial void Yopenlog(ж<TLS> tls, uintptr ident, int32 opt, int32 facility);

public static partial int32 /*r*/ Yopenpty(ж<TLS> tls, uintptr pm, uintptr ps, uintptr name, uintptr tio, uintptr ws);

public static partial int64 /*r*/ Ypathconf(ж<TLS> tls, uintptr path, int32 name);

public static partial int32 /*r*/ Ypause(ж<TLS> tls);

public static partial int32 /*r1*/ Ypclose(ж<TLS> tls, uintptr f);

public static partial void Yperror(ж<TLS> tls, uintptr msg);

public static partial int32 /*r*/ Ypersonality(ж<TLS> tls, uint64 persona);

public static partial int32 /*r*/ Ypipe(ж<TLS> tls, uintptr fd);

public static partial int32 /*r*/ Ypipe2(ж<TLS> tls, uintptr fd, int32 flag);

public static partial int32 /*r*/ Ypivot_root(ж<TLS> tls, uintptr new1, uintptr old);

public static partial int32 /*r*/ Ypoll(ж<TLS> tls, uintptr fds, Tnfds_t n, int32 timeout);

public static partial uintptr /*_2*/ Ypopen(ж<TLS> t, uintptr command, uintptr type1);

public static partial int32 /*r*/ Yposix_close(ж<TLS> tls, int32 fd, int32 flags);

public static partial int32 /*r*/ Yposix_fadvise(ж<TLS> tls, int32 fd, Toff_t @base, Toff_t len1, int32 advice);

public static partial int32 /*r*/ Yposix_fallocate(ж<TLS> tls, int32 fd, Toff_t @base, Toff_t len1);

public static partial int32 /*r*/ Yposix_madvise(ж<TLS> tls, uintptr addr, Tsize_t len1, int32 advice);

public static partial int32 /*r1*/ Yposix_openpt(ж<TLS> tls, int32 flags);

public static partial int32 /*r*/ Yposix_spawn_file_actions_addchdir_np(ж<TLS> tls, uintptr fa, uintptr path);

public static partial int32 /*r*/ Yposix_spawn_file_actions_addclose(ж<TLS> tls, uintptr fa, int32 fd);

public static partial int32 /*r*/ Yposix_spawn_file_actions_adddup2(ж<TLS> tls, uintptr fa, int32 srcfd, int32 fd);

public static partial int32 /*r*/ Yposix_spawn_file_actions_addfchdir_np(ж<TLS> tls, uintptr fa, int32 fd);

public static partial int32 /*r*/ Yposix_spawn_file_actions_addopen(ж<TLS> tls, uintptr fa, int32 fd, uintptr path, int32 flags, Tmode_t mode);

public static partial int32 /*r*/ Yposix_spawn_file_actions_destroy(ж<TLS> tls, uintptr fa);

public static partial int32 /*r*/ Yposix_spawn_file_actions_init(ж<TLS> tls, uintptr fa);

public static partial int32 /*r*/ Yposix_spawnattr_destroy(ж<TLS> tls, uintptr attr);

public static partial int32 /*r*/ Yposix_spawnattr_getflags(ж<TLS> tls, uintptr attr, uintptr flags);

public static partial int32 /*r*/ Yposix_spawnattr_getpgroup(ж<TLS> tls, uintptr attr, uintptr pgrp);

public static partial int32 /*r*/ Yposix_spawnattr_getschedparam(ж<TLS> tls, uintptr attr, uintptr schedparam);

public static partial int32 /*r*/ Yposix_spawnattr_getschedpolicy(ж<TLS> tls, uintptr attr, uintptr policy);

public static partial int32 /*r*/ Yposix_spawnattr_getsigdefault(ж<TLS> tls, uintptr attr, uintptr def);

public static partial int32 /*r*/ Yposix_spawnattr_getsigmask(ж<TLS> tls, uintptr attr, uintptr mask);

public static partial int32 /*r*/ Yposix_spawnattr_init(ж<TLS> tls, uintptr attr);

public static partial int32 /*r*/ Yposix_spawnattr_setflags(ж<TLS> tls, uintptr attr, int16 flags);

public static partial int32 /*r*/ Yposix_spawnattr_setpgroup(ж<TLS> tls, uintptr attr, Tpid_t pgrp);

public static partial int32 /*r*/ Yposix_spawnattr_setschedparam(ж<TLS> tls, uintptr attr, uintptr schedparam);

public static partial int32 /*r*/ Yposix_spawnattr_setschedpolicy(ж<TLS> tls, uintptr attr, int32 policy);

public static partial int32 /*r*/ Yposix_spawnattr_setsigdefault(ж<TLS> tls, uintptr attr, uintptr def);

public static partial int32 /*r*/ Yposix_spawnattr_setsigmask(ж<TLS> tls, uintptr attr, uintptr mask);

public static partial float64 /*r*/ Ypow(ж<TLS> tls, float64 x1, float64 y1);

public static partial float64 /*r*/ Ypow10(ж<TLS> tls, float64 x);

public static partial float32 /*r*/ Ypow10f(ж<TLS> tls, float32 x);

public static partial float64 /*r*/ Ypow10l(ж<TLS> tls, float64 x);

public static partial float32 /*r*/ Ypowf(ж<TLS> tls, float32 x1, float32 y1);

public static partial float64 /*r*/ Ypowl(ж<TLS> tls, float64 x, float64 y);

public static partial int32 /*r*/ Yppoll(ж<TLS> tls, uintptr fds, Tnfds_t n, uintptr to, uintptr mask);

public static partial int32 /*r*/ Yprctl(ж<TLS> tls, int32 op, uintptr va);

public static partial Tssize_t /*r*/ Ypread(ж<TLS> tls, int32 fd, uintptr buf, Tsize_t size, Toff_t ofs);

public static partial Tssize_t /*r*/ Ypreadv(ж<TLS> tls, int32 fd, uintptr iov, int32 count, Toff_t ofs);

public static partial Tssize_t /*r*/ Ypreadv2(ж<TLS> tls, int32 fd, uintptr iov, int32 count, Toff_t ofs, int32 flags);

public static partial int32 /*r*/ Yprintf(ж<TLS> tls, uintptr fmt, uintptr va);

public static partial int32 /*r1*/ Yprlimit(ж<TLS> tls, Tpid_t pid, int32 resource, uintptr new_limit, uintptr old_limit);

public static partial Tssize_t /*r*/ Yprocess_vm_readv(ж<TLS> tls, Tpid_t pid, uintptr lvec, uint64 liovcnt, uintptr rvec, uint64 riovcnt, uint64 flags);

public static partial Tssize_t /*r*/ Yprocess_vm_writev(ж<TLS> tls, Tpid_t pid, uintptr lvec, uint64 liovcnt, uintptr rvec, uint64 riovcnt, uint64 flags);

public static partial int32 /*r*/ Ypselect(ж<TLS> tls, int32 n, uintptr rfds, uintptr wfds, uintptr efds, uintptr ts, uintptr mask);

public static partial void Ypsiginfo(ж<TLS> tls, uintptr si, uintptr msg);

public static partial void Ypsignal(ж<TLS> tls, int32 sig, uintptr msg);

public static partial int32 /*_2*/ Ypthread_atfork(ж<TLS> tls, uintptr prepare, uintptr parent, uintptr child);

public static partial int32 /*_2*/ Ypthread_attr_destroy(ж<TLS> tls, uintptr a);

public static partial int32 /*_3*/ Ypthread_attr_getdetachstate(ж<TLS> tls, uintptr a, uintptr state);

public static partial int32 /*_2*/ Ypthread_attr_init(ж<TLS> tls, uintptr a);

public static partial int32 /*r*/ Ypthread_attr_setdetachstate(ж<TLS> tls, uintptr a, int32 state);

public static partial int32 /*_3*/ Ypthread_attr_setscope(ж<TLS> tls, uintptr a, int32 scope);

public static partial int32 /*_3*/ Ypthread_attr_setstacksize(ж<TLS> tls, uintptr a, Tsize_t stacksite);

public static partial void Ypthread_cleanup_pop(ж<TLS> tls, int32 run);

public static partial void Ypthread_cleanup_push(ж<TLS> tls, uintptr f, uintptr x);

public static partial int32 /*_2*/ Ypthread_cond_broadcast(ж<TLS> tls, uintptr c);

public static partial int32 /*_2*/ Ypthread_cond_destroy(ж<TLS> tls, uintptr c);

public static partial int32 /*_2*/ Ypthread_cond_init(ж<TLS> tls, uintptr c, uintptr a);

public static partial int32 /*_2*/ Ypthread_cond_signal(ж<TLS> tls, uintptr c);

public static partial int32 /*r*/ Ypthread_cond_timedwait(ж<TLS> tls, uintptr c, uintptr m, uintptr ts);

public static partial int32 /*_2*/ Ypthread_cond_wait(ж<TLS> tls, uintptr c, uintptr m);

public static partial int32 /*_2*/ Ypthread_create(ж<TLS> tls, uintptr res, uintptr attrp, uintptr entry, uintptr arg);

public static partial int32 /*_2*/ Ypthread_detach(ж<TLS> tls, uintptr t);

public static partial int32 /*_2*/ Ypthread_equal(ж<TLS> tls, uintptr t, uintptr u);

public static partial void Ypthread_exit(ж<TLS> tls, uintptr result);

public static partial uintptr /*_2*/ Ypthread_getspecific(ж<TLS> tls, Tpthread_key_t k);

public static partial int32 /*r*/ Ypthread_join(ж<TLS> tls, Tpthread_t t, uintptr res);

public static partial int32 /*_3*/ Ypthread_key_create(ж<TLS> tls, uintptr k, uintptr dtor);

public static partial int32 /*_2*/ Ypthread_key_delete(ж<TLS> tls, Tpthread_key_t k);

public static partial int32 /*_2*/ Ypthread_mutex_destroy(ж<TLS> tls, uintptr m);

public static partial int32 /*_2*/ Ypthread_mutex_init(ж<TLS> tls, uintptr m, uintptr a);

public static partial int32 /*_2*/ Ypthread_mutex_lock(ж<TLS> tls, uintptr m);

public static partial int32 /*_2*/ Ypthread_mutex_trylock(ж<TLS> tls, uintptr m);

public static partial int32 /*_2*/ Ypthread_mutex_unlock(ж<TLS> tls, uintptr m);

public static partial int32 /*_2*/ Ypthread_mutexattr_destroy(ж<TLS> tls, uintptr a);

public static partial int32 /*_2*/ Ypthread_mutexattr_init(ж<TLS> tls, uintptr a);

public static partial int32 /*_3*/ Ypthread_mutexattr_settype(ж<TLS> tls, uintptr a, int32 typ);

public static partial uintptr /*_1*/ Ypthread_self(ж<TLS> tls);

public static partial int32 /*_3*/ Ypthread_setcancelstate(ж<TLS> tls, int32 @new, uintptr old);

public static partial int32 /*_3*/ Ypthread_setspecific(ж<TLS> tls, Tpthread_key_t k, uintptr x);

public static partial int32 /*_3*/ Ypthread_sigmask(ж<TLS> tls, int32 now, uintptr set, uintptr old);

public static partial int64 /*r*/ Yptrace(ж<TLS> tls, int32 req, uintptr va);

public static partial uintptr /*r*/ Yptsname(ж<TLS> tls, int32 fd);

public static partial int32 /*r*/ Yptsname_r(ж<TLS> tls, int32 fd, uintptr buf, Tsize_t len1);

public static partial int32 /*r*/ Yputc(ж<TLS> tls, int32 c1, uintptr f1);

public static partial int32 /*r*/ Yputc_unlocked(ж<TLS> tls, int32 c, uintptr f);

public static partial int32 /*r*/ Yputchar(ж<TLS> tls, int32 c1);

public static partial int32 /*r*/ Yputchar_unlocked(ж<TLS> tls, int32 c);

public static partial int32 /*r*/ Yputenv(ж<TLS> tls, uintptr s);

public static partial int32 /*r1*/ Yputgrent(ж<TLS> tls, uintptr gr, uintptr f);

public static partial int32 /*r*/ Yputpwent(ж<TLS> tls, uintptr pw, uintptr f);

public static partial int32 /*r1*/ Yputs(ж<TLS> tls, uintptr s);

public static partial int32 /*r*/ Yputspent(ж<TLS> tls, uintptr sp, uintptr f);

public static partial uintptr /*r*/ Ypututline(ж<TLS> tls, uintptr ut);

public static partial uintptr /*r*/ Ypututxline(ж<TLS> tls, uintptr ut);

public static partial int32 /*r*/ Yputw(ж<TLS> tls, int32 _x, uintptr f);

public static partial Twint_t /*r*/ Yputwc(ж<TLS> tls, Twchar_t c, uintptr f);

public static partial Twint_t /*r*/ Yputwc_unlocked(ж<TLS> tls, Twchar_t c, uintptr f);

public static partial Twint_t /*r*/ Yputwchar(ж<TLS> tls, Twchar_t c);

public static partial Twint_t /*r*/ Yputwchar_unlocked(ж<TLS> tls, Twchar_t c);

public static partial Tssize_t /*r*/ Ypwrite(ж<TLS> tls, int32 fd, uintptr buf, Tsize_t size, Toff_t ofs);

public static partial Tssize_t /*r*/ Ypwritev(ж<TLS> tls, int32 fd, uintptr iov, int32 count, Toff_t ofs);

public static partial Tssize_t /*r*/ Ypwritev2(ж<TLS> tls, int32 fd, uintptr iov, int32 count, Toff_t ofs, int32 flags);

public static partial void Yqsort(ж<TLS> tls, uintptr @base, Tsize_t nel, Tsize_t width, Tcmpfun __ccgo_fp_cmp);

internal static partial int32 /*_3*/ __ccgo_abi0_qsort_3(ж<TLS> _0, uintptr _1, uintptr _2, uintptr __ccgo_fp);

internal static void __ccgo_abiInternal_qsort_3(ref TLS tls, uintptr dest, uintptr abi0CodePtr) {
    ref var f = ref heap<Func<ж<TLS>, uintptr, uintptr, int32>>((ж<TLS> _0, uintptr _1, uintptr _2) => {
        return __ccgo_abi0_qsort_3(_0, _1, _2, abi0CodePtr);
    }, out var Ꮡf);
    @unsafe.Pointer.StoreArray<uintptr>((@unsafe.Pointer)dest, 2, @unsafe.Pointer.LoadArray<uintptr>((@unsafe.Pointer)((uintptr)GoFuncCookie.Of(f)), 2));
}

public static partial void Yqsort_r(ж<TLS> tls, uintptr @base, Tsize_t nel, Tsize_t width, Tcmpfun __ccgo_fp_cmp, uintptr arg);

internal static partial int32 /*_4*/ __ccgo_abi0_qsort_r_3(ж<TLS> _0, uintptr _1, uintptr _2, uintptr _3, uintptr __ccgo_fp);

internal static void __ccgo_abiInternal_qsort_r_3(ref TLS tls, uintptr dest, uintptr abi0CodePtr) {
    ref var f = ref heap<Func<ж<TLS>, uintptr, uintptr, uintptr, int32>>((ж<TLS> _0, uintptr _1, uintptr _2, uintptr _3) => {
        return __ccgo_abi0_qsort_r_3(_0, _1, _2, _3, abi0CodePtr);
    }, out var Ꮡf);
    @unsafe.Pointer.StoreArray<uintptr>((@unsafe.Pointer)dest, 2, @unsafe.Pointer.LoadArray<uintptr>((@unsafe.Pointer)((uintptr)GoFuncCookie.Of(f)), 2));
}

public static partial void Yquick_exit(ж<TLS> tls, int32 code);

public static partial int32 /*r*/ Yquotactl(ж<TLS> tls, int32 cmd, uintptr special, int32 id, uintptr addr);

public static partial int32 /*r*/ Yraise(ж<TLS> tls, int32 sig);

public static partial int32 /*r*/ Yrand(ж<TLS> tls);

public static partial int32 /*r*/ Yrand_r(ж<TLS> tls, uintptr seed);

public static partial int64 /*r*/ Yrandom(ж<TLS> tls);

public static partial int32 /*_2*/ Yrandom_r(ж<TLS> t, uintptr buf, uintptr result);

public static partial Tssize_t /*r*/ Yread(ж<TLS> tls, int32 fd, uintptr buf, Tsize_t count);

public static partial Tssize_t /*r*/ Yreadahead(ж<TLS> tls, int32 fd, Toff_t pos, Tsize_t len1);

public static partial uintptr /*r*/ Yreaddir(ж<TLS> tls, uintptr dir);

public static partial uintptr /*r*/ Yreaddir64(ж<TLS> tls, uintptr dir);

public static partial int32 /*r*/ Yreaddir_r(ж<TLS> tls, uintptr dir, uintptr buf, uintptr result);

public static partial Tssize_t /*r1*/ Yreadlink(ж<TLS> tls, uintptr path, uintptr buf, Tsize_t bufsize);

public static partial Tssize_t /*r1*/ Yreadlinkat(ж<TLS> tls, int32 fd, uintptr path, uintptr buf, Tsize_t bufsize);

public static partial Tssize_t /*r*/ Yreadv(ж<TLS> tls, int32 fd, uintptr iov, int32 count);

public static partial uintptr /*r*/ Yrealloc(ж<TLS> tls, uintptr p, Tsize_t n);

public static partial uintptr /*r*/ Yreallocarray(ж<TLS> tls, uintptr ptr, Tsize_t m, Tsize_t n);

public static partial uintptr /*r*/ Yrealpath(ж<TLS> tls, uintptr filename, uintptr resolved);

public static partial int32 /*r*/ Yreboot(ж<TLS> tls, int32 type1);

public static partial Tssize_t /*r*/ Yrecv(ж<TLS> tls, int32 fd, uintptr buf, Tsize_t len1, int32 flags);

public static partial Tssize_t /*r1*/ Yrecvfrom(ж<TLS> tls, int32 fd, uintptr buf, Tsize_t len1, int32 flags, uintptr addr, uintptr alen);

public static partial int32 /*r*/ Yrecvmmsg(ж<TLS> tls, int32 fd, uintptr msgvec, uint32 vlen, uint32 flags, uintptr timeout);

public static partial Tssize_t /*r2*/ Yrecvmsg(ж<TLS> tls, int32 fd, uintptr msg, int32 flags);

public static partial int32 /*r*/ Yregcomp(ж<TLS> tls, uintptr preg, uintptr regex, int32 cflags);

public static partial Tsize_t /*r*/ Yregerror(ж<TLS> tls, int32 e, uintptr preg, uintptr buf, Tsize_t size);

public static partial int32 /*r*/ Yregexec(ж<TLS> tls, uintptr preg, uintptr string1, Tsize_t nmatch, uintptr pmatch, int32 eflags);

public static partial void Yregfree(ж<TLS> tls, uintptr preg);

public static partial float64 /*r*/ Yremainder(ж<TLS> tls, float64 x, float64 y);

public static partial float32 /*r*/ Yremainderf(ж<TLS> tls, float32 x, float32 y);

public static partial float64 /*r*/ Yremainderl(ж<TLS> tls, float64 x, float64 y);

public static partial int32 /*r*/ Yremap_file_pages(ж<TLS> tls, uintptr addr, Tsize_t size, int32 prot, Tsize_t pgoff, int32 flags);

public static partial int32 /*r1*/ Yremove(ж<TLS> tls, uintptr path);

public static partial int32 /*r*/ Yremovexattr(ж<TLS> tls, uintptr path, uintptr name);

public static partial void Yremque(ж<TLS> tls, uintptr element);

public static partial float64 /*r*/ Yremquo(ж<TLS> tls, float64 x, float64 y, uintptr quo);

public static partial float32 /*r*/ Yremquof(ж<TLS> tls, float32 x, float32 y, uintptr quo);

public static partial float64 /*r*/ Yremquol(ж<TLS> tls, float64 x, float64 y, uintptr quo);

public static partial int32 /*r*/ Yrename(ж<TLS> tls, uintptr old, uintptr new1);

public static partial int32 /*r*/ Yrenameat(ж<TLS> tls, int32 oldfd, uintptr old, int32 newfd, uintptr new1);

public static partial int32 /*_6*/ Yrenameat2(ж<TLS> t, int32 olddirfd, uintptr oldpath, int32 newdirfd, uintptr newpath, int32 flags);

public static partial int32 /*r*/ Yres_init(ж<TLS> tls);

public static partial int32 /*r*/ Yres_mkquery(ж<TLS> tls, int32 op, uintptr dname, int32 @class, int32 type1, uintptr data, int32 datalen, uintptr newrr, uintptr buf, int32 buflen);

public static partial int32 /*r*/ Yres_send(ж<TLS> tls, uintptr _msg, int32 _msglen, uintptr _answer, int32 _anslen);

public static partial void Yrewind(ж<TLS> tls, uintptr f);

public static partial void Yrewinddir(ж<TLS> tls, uintptr dir);

public static partial uintptr /*r*/ Yrindex(ж<TLS> tls, uintptr s, int32 c);

public static partial float64 /*r*/ Yrint(ж<TLS> tls, float64 x);

public static partial float32 /*r*/ Yrintf(ж<TLS> tls, float32 x);

public static partial float64 /*r*/ Yrintl(ж<TLS> tls, float64 x);

public static partial int32 /*r*/ Yrmdir(ж<TLS> tls, uintptr path);

public static partial float64 /*r*/ Yround(ж<TLS> tls, float64 x3);

public static partial float32 /*r*/ Yroundf(ж<TLS> tls, float32 x3);

public static partial float64 /*r*/ Yroundl(ж<TLS> tls, float64 x);

public static partial uintptr /*r*/ Ysbrk(ж<TLS> tls, Tintptr_t inc);

public static partial float64 /*r*/ Yscalb(ж<TLS> tls, float64 x, float64 fn);

public static partial float32 /*r*/ Yscalbf(ж<TLS> tls, float32 x, float32 fn);

public static partial float64 /*r*/ Yscalbln(ж<TLS> tls, float64 x, int64 n);

public static partial float32 /*r*/ Yscalblnf(ж<TLS> tls, float32 x, int64 n);

public static partial float64 /*r*/ Yscalblnl(ж<TLS> tls, float64 x, int64 n);

public static partial float64 /*r*/ Yscalbn(ж<TLS> tls, float64 x, int32 n);

public static partial float32 /*r*/ Yscalbnf(ж<TLS> tls, float32 x, int32 n);

public static partial float64 /*r*/ Yscalbnl(ж<TLS> tls, float64 x, int32 n);

public static partial int32 /*r*/ Yscandir(ж<TLS> tls, uintptr path, uintptr res, uintptr __ccgo_fp_sel, uintptr __ccgo_fp_cmp);

internal static partial int32 /*_2*/ __ccgo_abi0_scandir_2(ж<TLS> _0, uintptr _1, uintptr __ccgo_fp);

internal static void __ccgo_abiInternal_scandir_2(ref TLS tls, uintptr dest, uintptr abi0CodePtr) {
    ref var f = ref heap<Func<ж<TLS>, uintptr, int32>>((ж<TLS> _0, uintptr _1) => {
        return __ccgo_abi0_scandir_2(_0, _1, abi0CodePtr);
    }, out var Ꮡf);
    @unsafe.Pointer.StoreArray<uintptr>((@unsafe.Pointer)dest, 2, @unsafe.Pointer.LoadArray<uintptr>((@unsafe.Pointer)((uintptr)GoFuncCookie.Of(f)), 2));
}

internal static partial int32 /*_3*/ __ccgo_abi0_scandir_3(ж<TLS> _0, uintptr _1, uintptr _2, uintptr __ccgo_fp);

internal static void __ccgo_abiInternal_scandir_3(ref TLS tls, uintptr dest, uintptr abi0CodePtr) {
    ref var f = ref heap<Func<ж<TLS>, uintptr, uintptr, int32>>((ж<TLS> _0, uintptr _1, uintptr _2) => {
        return __ccgo_abi0_scandir_3(_0, _1, _2, abi0CodePtr);
    }, out var Ꮡf);
    @unsafe.Pointer.StoreArray<uintptr>((@unsafe.Pointer)dest, 2, @unsafe.Pointer.LoadArray<uintptr>((@unsafe.Pointer)((uintptr)GoFuncCookie.Of(f)), 2));
}

public static partial int32 /*r*/ Yscanf(ж<TLS> tls, uintptr fmt, uintptr va);

public static partial int32 /*_1*/ Ysched_yield(ж<TLS> tls);

public static partial uintptr /*r*/ Ysecure_getenv(ж<TLS> tls, uintptr name);

public static partial uintptr /*r*/ Yseed48(ж<TLS> tls, uintptr s);

public static partial void Yseekdir(ж<TLS> tls, uintptr dir, int64 off);

public static partial int32 /*r*/ Yselect(ж<TLS> tls, int32 n, uintptr rfds, uintptr wfds, uintptr efds, uintptr tv);

public static partial int32 /*r1*/ Ysemctl(ж<TLS> tls, int32 id, int32 num, int32 cmd, uintptr va);

public static partial int32 /*r*/ Ysemget(ж<TLS> tls, Tkey_t key, int32 n, int32 fl);

public static partial int32 /*r*/ Ysemop(ж<TLS> tls, int32 id, uintptr buf, Tsize_t n);

public static partial int32 /*r*/ Ysemtimedop(ж<TLS> tls, int32 id, uintptr buf, Tsize_t n, uintptr ts);

public static partial Tssize_t /*r*/ Ysend(ж<TLS> tls, int32 fd, uintptr buf, Tsize_t len1, int32 flags);

public static partial Tssize_t /*r*/ Ysendfile(ж<TLS> tls, int32 out_fd, int32 in_fd, uintptr ofs, Tsize_t count);

public static partial int32 /*r1*/ Ysendmmsg(ж<TLS> tls, int32 fd, uintptr msgvec, uint32 vlen, uint32 flags);

public static partial Tssize_t /*r1*/ Ysendmsg(ж<TLS> tls, int32 fd, uintptr msg, int32 flags);

public static partial Tssize_t /*r1*/ Ysendto(ж<TLS> tls, int32 fd, uintptr buf, Tsize_t len1, int32 flags, uintptr addr, Tsocklen_t alen);

public static partial void Ysetbuf(ж<TLS> tls, uintptr f, uintptr buf);

public static partial void Ysetbuffer(ж<TLS> tls, uintptr f, uintptr buf, Tsize_t size);

public static partial int32 /*r*/ Ysetdomainname(ж<TLS> tls, uintptr name, Tsize_t len1);

public static partial int32 /*r*/ Ysetenv(ж<TLS> tls, uintptr var1, uintptr value, int32 overwrite);

public static partial int32 /*r*/ Ysetfsgid(ж<TLS> tls, Tgid_t gid);

public static partial int32 /*r*/ Ysetfsuid(ж<TLS> tls, Tuid_t uid);

public static partial int32 /*r*/ Ysetgid(ж<TLS> tls, Tgid_t gid);

public static partial void Ysetgrent(ж<TLS> tls);

public static partial void Ysethostent(ж<TLS> tls, int32 x);

public static partial int32 /*r*/ Ysethostname(ж<TLS> tls, uintptr name, Tsize_t len1);

public static partial int32 /*r1*/ Ysetitimer(ж<TLS> tls, int32 which, uintptr new1, uintptr old);

public static partial int32 /*_2*/ Ysetjmp(ж<TLS> t, uintptr env);

public static partial void Ysetkey(ж<TLS> tls, uintptr key);

public static partial void Ysetlinebuf(ж<TLS> tls, uintptr f);

public static partial uintptr /*r*/ Ysetlocale(ж<TLS> tls, int32 cat, uintptr name);

public static partial int32 /*r*/ Ysetlogmask(ж<TLS> tls, int32 maskpri);

public static partial uintptr /*r*/ Ysetmntent(ж<TLS> tls, uintptr name, uintptr mode);

public static partial void Ysetnetent(ж<TLS> tls, int32 x);

public static partial int32 /*r*/ Ysetns(ж<TLS> tls, int32 fd, int32 nstype);

public static partial int32 /*r*/ Ysetpgid(ж<TLS> tls, Tpid_t pid, Tpid_t pgid);

public static partial Tpid_t /*r*/ Ysetpgrp(ж<TLS> tls);

public static partial int32 /*r*/ Ysetpriority(ж<TLS> tls, int32 which, Tid_t who, int32 prio);

public static partial void Ysetprotoent(ж<TLS> tls, int32 stayopen);

public static partial void Ysetpwent(ж<TLS> tls);

public static partial int32 /*r*/ Ysetrlimit(ж<TLS> tls, int32 resource, uintptr rlim);

public static partial int32 /*r*/ Ysetrlimit64(ж<TLS> tls, int32 resource, uintptr rlim);

public static partial void Ysetservent(ж<TLS> tls, int32 stayopen);

public static partial Tpid_t /*r*/ Ysetsid(ж<TLS> tls);

public static partial int32 /*r2*/ Ysetsockopt(ж<TLS> tls, int32 fd, int32 level, int32 optname, uintptr optval, Tsocklen_t optlen);

public static partial void Ysetspent(ж<TLS> tls);

public static partial uintptr /*r*/ Ysetstate(ж<TLS> tls, uintptr state);

public static partial int32 /*r*/ Ysettimeofday(ж<TLS> tls, uintptr tv, uintptr tz);

public static partial int32 /*r*/ Ysetuid(ж<TLS> tls, Tuid_t uid);

public static partial void Ysetusershell(ж<TLS> tls);

public static partial void Ysetutent(ж<TLS> tls);

public static partial void Ysetutxent(ж<TLS> tls);

public static partial int32 /*r*/ Ysetvbuf(ж<TLS> tls, uintptr f, uintptr buf, int32 type1, Tsize_t size);

public static partial int32 /*r*/ Ysetxattr(ж<TLS> tls, uintptr path, uintptr name, uintptr value, Tsize_t size, int32 flags);

public static partial int32 /*r*/ Yshm_open(ж<TLS> tls, uintptr name, int32 flag, Tmode_t mode);

public static partial int32 /*r*/ Yshm_unlink(ж<TLS> tls, uintptr name);

public static partial uintptr /*r*/ Yshmat(ж<TLS> tls, int32 id, uintptr addr, int32 flag);

public static partial int32 /*r1*/ Yshmctl(ж<TLS> tls, int32 id, int32 cmd, uintptr buf);

public static partial int32 /*r*/ Yshmdt(ж<TLS> tls, uintptr addr);

public static partial int32 /*r*/ Yshmget(ж<TLS> tls, Tkey_t key, Tsize_t size, int32 flag);

public static partial int32 /*r1*/ Yshutdown(ж<TLS> tls, int32 fd, int32 how);

public static partial int32 /*r*/ Ysigaction(ж<TLS> tls, int32 sig, uintptr sa, uintptr old);

public static partial int32 /*r*/ Ysigaddset(ж<TLS> tls, uintptr set, int32 sig);

public static partial int32 /*r*/ Ysigaltstack(ж<TLS> tls, uintptr ss, uintptr old);

public static partial int32 /*r1*/ Ysigandset(ж<TLS> tls, uintptr dest, uintptr left, uintptr right);

public static partial int32 /*r*/ Ysigdelset(ж<TLS> tls, uintptr set, int32 sig);

public static partial int32 /*r*/ Ysigemptyset(ж<TLS> tls, uintptr set);

public static partial int32 /*r*/ Ysigfillset(ж<TLS> tls, uintptr set);

public static partial int32 /*r*/ Ysigisemptyset(ж<TLS> tls, uintptr set);

public static partial int32 /*r*/ Ysigismember(ж<TLS> tls, uintptr set, int32 sig);

public static partial uintptr /*r*/ Ysignal(ж<TLS> tls, int32 signum, uintptr handler);

public static partial int32 /*r*/ Ysignalfd(ж<TLS> tls, int32 fd, uintptr sigs, int32 flags);

public static partial float64 /*r*/ Ysignificand(ж<TLS> tls, float64 x);

public static partial float32 /*r*/ Ysignificandf(ж<TLS> tls, float32 x);

public static partial int32 /*r1*/ Ysigorset(ж<TLS> tls, uintptr dest, uintptr left, uintptr right);

public static partial int32 /*r*/ Ysigpending(ж<TLS> tls, uintptr set);

public static partial int32 /*r1*/ Ysigprocmask(ж<TLS> tls, int32 how, uintptr set, uintptr old);

public static partial int32 /*r1*/ Ysigqueue(ж<TLS> tls, Tpid_t pid, int32 sig, Tsigval value);

public static partial int32 /*r*/ Ysigsuspend(ж<TLS> tls, uintptr mask);

public static partial int32 /*r*/ Ysigtimedwait(ж<TLS> tls, uintptr mask, uintptr si, uintptr timeout);

public static partial int32 /*r*/ Ysigwait(ж<TLS> tls, uintptr mask, uintptr sig);

public static partial int32 /*r*/ Ysigwaitinfo(ж<TLS> tls, uintptr mask, uintptr si);

public static partial float64 /*r*/ Ysin(ж<TLS> tls, float64 x3);

public static partial void Ysincos(ж<TLS> tls, float64 x3, uintptr sin, uintptr cos);

public static partial void Ysincosf(ж<TLS> tls, float32 x3, uintptr sin, uintptr cos);

public static partial void Ysincosl(ж<TLS> tls, float64 x, uintptr sin, uintptr cos);

public static partial float32 /*r*/ Ysinf(ж<TLS> tls, float32 x3);

public static partial float64 /*r*/ Ysinh(ж<TLS> tls, float64 x);

public static partial float32 /*r*/ Ysinhf(ж<TLS> tls, float32 x);

public static partial float64 /*r*/ Ysinhl(ж<TLS> tls, float64 x);

public static partial float64 /*r*/ Ysinl(ж<TLS> tls, float64 x);

public static partial uint32 /*r*/ Ysleep(ж<TLS> tls, uint32 seconds);

public static partial int32 /*r*/ Ysnprintf(ж<TLS> tls, uintptr s, Tsize_t n, uintptr fmt, uintptr va);

public static partial int32 /*r*/ Ysockatmark(ж<TLS> tls, int32 s);

public static partial int32 /*r1*/ Ysocket(ж<TLS> tls, int32 domain, int32 type1, int32 protocol);

public static partial int32 /*r2*/ Ysocketpair(ж<TLS> tls, int32 domain, int32 type1, int32 protocol, uintptr fd);

public static partial Tssize_t /*r*/ Ysplice(ж<TLS> tls, int32 fd_in, uintptr off_in, int32 fd_out, uintptr off_out, Tsize_t len1, uint32 flags);

public static partial int32 /*r*/ Ysprintf(ж<TLS> tls, uintptr s, uintptr fmt, uintptr va);

public static partial float64 /*r1*/ Ysqrt(ж<TLS> tls, float64 x1);

public static partial float32 /*r1*/ Ysqrtf(ж<TLS> tls, float32 x1);

public static partial float64 /*r*/ Ysqrtl(ж<TLS> tls, float64 x);

public static partial void Ysrand(ж<TLS> tls, uint32 s);

public static partial void Ysrand48(ж<TLS> tls, int64 seed);

public static partial void Ysrandom(ж<TLS> tls, uint32 seed);

public static partial int32 /*r*/ Ysscanf(ж<TLS> tls, uintptr s, uintptr fmt, uintptr va);

public static partial int32 /*r*/ Ystat(ж<TLS> tls, uintptr path, uintptr buf);

public static partial int32 /*r*/ Ystat64(ж<TLS> tls, uintptr path, uintptr buf);

public static partial int32 /*r*/ Ystatvfs(ж<TLS> tls, uintptr path, uintptr buf);

public static partial int32 /*r*/ Ystatx(ж<TLS> tls, int32 dirfd, uintptr path, int32 flags, uint32 mask, uintptr stx);

public static partial int32 /*r*/ Ystime(ж<TLS> tls, uintptr t);

public static partial uintptr /*r*/ Ystpcpy(ж<TLS> tls, uintptr d, uintptr s);

public static partial uintptr /*r*/ Ystpncpy(ж<TLS> tls, uintptr d, uintptr s, Tsize_t n);

public static partial int32 /*r1*/ Ystrcasecmp(ж<TLS> tls, uintptr _l, uintptr _r);

public static partial int32 /*r1*/ Ystrcasecmp_l(ж<TLS> tls, uintptr l, uintptr r, Tlocale_t loc);

public static partial uintptr /*r*/ Ystrcasestr(ж<TLS> tls, uintptr h, uintptr n);

public static partial uintptr /*r*/ Ystrcat(ж<TLS> tls, uintptr dest, uintptr src);

public static partial uintptr /*r1*/ Ystrchr(ж<TLS> tls, uintptr s, int32 c);

public static partial uintptr /*r*/ Ystrchrnul(ж<TLS> tls, uintptr s, int32 c);

public static partial int32 /*r1*/ Ystrcmp(ж<TLS> tls, uintptr l, uintptr r);

public static partial int32 /*r1*/ Ystrcoll(ж<TLS> tls, uintptr l, uintptr r);

public static partial int32 /*r1*/ Ystrcoll_l(ж<TLS> tls, uintptr l, uintptr r, Tlocale_t loc);

public static partial uintptr /*r*/ Ystrcpy(ж<TLS> tls, uintptr dest, uintptr src);

public static partial Tsize_t /*r*/ Ystrcspn(ж<TLS> tls, uintptr s, uintptr c);

public static partial uintptr /*r*/ Ystrdup(ж<TLS> tls, uintptr s);

public static partial uintptr /*r*/ Ystrerror(ж<TLS> tls, int32 e);

public static partial uintptr /*r*/ Ystrerror_l(ж<TLS> tls, int32 e, Tlocale_t loc);

public static partial int32 /*r*/ Ystrerror_r(ж<TLS> tls, int32 err, uintptr buf, Tsize_t buflen);

public static partial Tssize_t /*r*/ Ystrfmon(ж<TLS> tls, uintptr s, Tsize_t n, uintptr fmt, uintptr va);

public static partial Tssize_t /*r*/ Ystrfmon_l(ж<TLS> tls, uintptr s, Tsize_t n, Tlocale_t loc, uintptr fmt, uintptr va);

public static partial Tsize_t /*r*/ Ystrftime(ж<TLS> tls, uintptr s, Tsize_t n, uintptr f, uintptr tm);

public static partial Tsize_t /*r*/ Ystrftime_l(ж<TLS> tls, uintptr s, Tsize_t n, uintptr f, uintptr tm, Tlocale_t loc);

public static partial Tsize_t /*r*/ Ystrlcat(ж<TLS> tls, uintptr d, uintptr s, Tsize_t n);

public static partial Tsize_t /*r*/ Ystrlcpy(ж<TLS> tls, uintptr d, uintptr s, Tsize_t n);

public static partial Tsize_t /*r*/ Ystrlen(ж<TLS> t, uintptr s);

public static partial int32 /*r1*/ Ystrncasecmp(ж<TLS> tls, uintptr _l, uintptr _r, Tsize_t n);

public static partial int32 /*r1*/ Ystrncasecmp_l(ж<TLS> tls, uintptr l, uintptr r, Tsize_t n, Tlocale_t loc);

public static partial uintptr /*r*/ Ystrncat(ж<TLS> tls, uintptr d, uintptr s, Tsize_t n);

public static partial int32 /*r1*/ Ystrncmp(ж<TLS> tls, uintptr _l, uintptr _r, Tsize_t n);

public static partial uintptr /*r*/ Ystrncpy(ж<TLS> tls, uintptr d, uintptr s, Tsize_t n);

public static partial uintptr /*r*/ Ystrndup(ж<TLS> tls, uintptr s, Tsize_t n);

public static partial Tsize_t /*r*/ Ystrnlen(ж<TLS> tls, uintptr s, Tsize_t n);

public static partial uintptr /*r*/ Ystrpbrk(ж<TLS> tls, uintptr s, uintptr b);

public static partial uintptr /*r*/ Ystrptime(ж<TLS> tls, uintptr s, uintptr f, uintptr tm);

public static partial uintptr /*r*/ Ystrrchr(ж<TLS> tls, uintptr s, int32 c);

public static partial uintptr /*r*/ Ystrsep(ж<TLS> tls, uintptr str, uintptr sep);

public static partial uintptr /*r*/ Ystrsignal(ж<TLS> tls, int32 signum);

public static partial Tsize_t /*r*/ Ystrspn(ж<TLS> tls, uintptr s, uintptr c);

public static partial uintptr /*r*/ Ystrstr(ж<TLS> tls, uintptr h, uintptr n);

public static partial float64 /*r*/ Ystrtod(ж<TLS> tls, uintptr s, uintptr p);

public static partial float64 /*r*/ Ystrtod_l(ж<TLS> tls, uintptr s, uintptr p, Tlocale_t l);

public static partial float32 /*r*/ Ystrtof(ж<TLS> tls, uintptr s, uintptr p);

public static partial float32 /*r*/ Ystrtof_l(ж<TLS> tls, uintptr s, uintptr p, Tlocale_t l);

public static partial Tintmax_t /*r*/ Ystrtoimax(ж<TLS> tls, uintptr s, uintptr p, int32 @base);

public static partial uintptr /*r*/ Ystrtok(ж<TLS> tls, uintptr s, uintptr sep);

public static partial uintptr /*r*/ Ystrtok_r(ж<TLS> tls, uintptr s, uintptr sep, uintptr p);

public static partial int64 /*r*/ Ystrtol(ж<TLS> tls, uintptr s, uintptr p, int32 @base);

public static partial float64 /*r*/ Ystrtold(ж<TLS> tls, uintptr s, uintptr p);

public static partial float64 /*r*/ Ystrtold_l(ж<TLS> tls, uintptr s, uintptr p, Tlocale_t l);

public static partial int64 /*r*/ Ystrtoll(ж<TLS> tls, uintptr s, uintptr p, int32 @base);

public static partial uint64 /*r*/ Ystrtoul(ж<TLS> tls, uintptr s, uintptr p, int32 @base);

public static partial uint64 /*r*/ Ystrtoull(ж<TLS> tls, uintptr s, uintptr p, int32 @base);

public static partial Tuintmax_t /*r*/ Ystrtoumax(ж<TLS> tls, uintptr s, uintptr p, int32 @base);

public static partial int32 /*r1*/ Ystrverscmp(ж<TLS> tls, uintptr l0, uintptr r0);

public static partial Tsize_t /*r*/ Ystrxfrm(ж<TLS> tls, uintptr dest, uintptr src, Tsize_t n);

public static partial Tsize_t /*r*/ Ystrxfrm_l(ж<TLS> tls, uintptr dest, uintptr src, Tsize_t n, Tlocale_t loc);

public static partial void Yswab(ж<TLS> tls, uintptr _src, uintptr _dest, Tssize_t n);

public static partial int32 /*r*/ Yswapoff(ж<TLS> tls, uintptr path);

public static partial int32 /*r*/ Yswapon(ж<TLS> tls, uintptr path, int32 flags);

public static partial int32 /*r*/ Yswprintf(ж<TLS> tls, uintptr s, Tsize_t n, uintptr fmt, uintptr va);

public static partial int32 /*r*/ Yswscanf(ж<TLS> tls, uintptr s, uintptr fmt, uintptr va);

public static partial int32 /*r*/ Ysymlink(ж<TLS> tls, uintptr existing, uintptr new1);

public static partial int32 /*r*/ Ysymlinkat(ж<TLS> tls, uintptr existing, int32 fd, uintptr new1);

public static partial void Ysync(ж<TLS> tls);

public static partial int32 /*r*/ Ysync_file_range(ж<TLS> tls, int32 fd, Toff_t pos, Toff_t len1, uint32 flags);

public static partial int32 /*r*/ Ysyncfs(ж<TLS> tls, int32 fd);

public static partial int64 /*r*/ Ysyscall(ж<TLS> tls, int64 n, uintptr va);

public static partial int64 /*r*/ Ysysconf(ж<TLS> tls, int32 name);

public static partial int32 /*_3*/ Ysysctlbyname(ж<TLS> t, uintptr name, uintptr oldp, uintptr oldlenp, uintptr newp, Tsize_t newlen);

public static partial int32 /*r*/ Ysysinfo(ж<TLS> tls, uintptr info);

public static partial void Ysyslog(ж<TLS> tls, int32 priority, uintptr message, uintptr va);

public static partial int32 /*_2*/ Ysystem(ж<TLS> t, uintptr command);

public static partial float64 /*r*/ Ytan(ж<TLS> tls, float64 x3);

public static partial float32 /*r*/ Ytanf(ж<TLS> tls, float32 x3);

public static partial float64 /*r*/ Ytanh(ж<TLS> tls, float64 x3);

public static partial float32 /*r*/ Ytanhf(ж<TLS> tls, float32 x3);

public static partial float64 /*r*/ Ytanhl(ж<TLS> tls, float64 x);

public static partial float64 /*r*/ Ytanl(ж<TLS> tls, float64 x);

public static partial int32 /*r*/ Ytcdrain(ж<TLS> tls, int32 fd);

public static partial int32 /*r*/ Ytcflow(ж<TLS> tls, int32 fd, int32 action);

public static partial int32 /*r*/ Ytcflush(ж<TLS> tls, int32 fd, int32 queue);

public static partial int32 /*r*/ Ytcgetattr(ж<TLS> tls, int32 fd, uintptr tio);

public static partial Tpid_t /*r*/ Ytcgetpgrp(ж<TLS> tls, int32 fd);

public static partial Tpid_t /*r*/ Ytcgetsid(ж<TLS> tls, int32 fd);

public static partial int32 /*r*/ Ytcgetwinsize(ж<TLS> tls, int32 fd, uintptr wsz);

public static partial int32 /*r*/ Ytcsendbreak(ж<TLS> tls, int32 fd, int32 dur);

public static partial int32 /*r*/ Ytcsetattr(ж<TLS> tls, int32 fd, int32 act, uintptr tio);

public static partial int32 /*r*/ Ytcsetpgrp(ж<TLS> tls, int32 fd, Tpid_t pgrp);

public static partial int32 /*r*/ Ytcsetwinsize(ж<TLS> tls, int32 fd, uintptr wsz);

public static partial uintptr /*r*/ Ytdelete(ж<TLS> tls, uintptr key, uintptr rootp, uintptr __ccgo_fp_cmp);

internal static partial int32 /*_3*/ __ccgo_abi0_tdelete_2(ж<TLS> _0, uintptr _1, uintptr _2, uintptr __ccgo_fp);

internal static void __ccgo_abiInternal_tdelete_2(ref TLS tls, uintptr dest, uintptr abi0CodePtr) {
    ref var f = ref heap<Func<ж<TLS>, uintptr, uintptr, int32>>((ж<TLS> _0, uintptr _1, uintptr _2) => {
        return __ccgo_abi0_tdelete_2(_0, _1, _2, abi0CodePtr);
    }, out var Ꮡf);
    @unsafe.Pointer.StoreArray<uintptr>((@unsafe.Pointer)dest, 2, @unsafe.Pointer.LoadArray<uintptr>((@unsafe.Pointer)((uintptr)GoFuncCookie.Of(f)), 2));
}

public static partial void Ytdestroy(ж<TLS> tls, uintptr root, uintptr __ccgo_fp_freekey);

internal static partial void __ccgo_abi0_tdestroy_1(ж<TLS> _0, uintptr _1, uintptr __ccgo_fp);

internal static void __ccgo_abiInternal_tdestroy_1(ref TLS tls, uintptr dest, uintptr abi0CodePtr) {
    ref var f = ref heap<Action<ж<TLS>, uintptr>>((ж<TLS> _0, uintptr _1) => {
        __ccgo_abi0_tdestroy_1(_0, _1, abi0CodePtr);
    }, out var Ꮡf);
    @unsafe.Pointer.StoreArray<uintptr>((@unsafe.Pointer)dest, 2, @unsafe.Pointer.LoadArray<uintptr>((@unsafe.Pointer)((uintptr)GoFuncCookie.Of(f)), 2));
}

public static partial Tssize_t /*r*/ Ytee(ж<TLS> tls, int32 src, int32 dest, Tsize_t len1, uint32 flags);

public static partial int64 /*r*/ Ytelldir(ж<TLS> tls, uintptr dir);

public static partial uintptr /*r1*/ Ytempnam(ж<TLS> tls, uintptr dir, uintptr pfx);

public static partial uintptr /*r*/ Ytextdomain(ж<TLS> tls, uintptr domainname);

public static partial uintptr /*r*/ Ytfind(ж<TLS> tls, uintptr key, uintptr rootp, uintptr __ccgo_fp_cmp);

internal static partial int32 /*_3*/ __ccgo_abi0_tfind_2(ж<TLS> _0, uintptr _1, uintptr _2, uintptr __ccgo_fp);

internal static void __ccgo_abiInternal_tfind_2(ref TLS tls, uintptr dest, uintptr abi0CodePtr) {
    ref var f = ref heap<Func<ж<TLS>, uintptr, uintptr, int32>>((ж<TLS> _0, uintptr _1, uintptr _2) => {
        return __ccgo_abi0_tfind_2(_0, _1, _2, abi0CodePtr);
    }, out var Ꮡf);
    @unsafe.Pointer.StoreArray<uintptr>((@unsafe.Pointer)dest, 2, @unsafe.Pointer.LoadArray<uintptr>((@unsafe.Pointer)((uintptr)GoFuncCookie.Of(f)), 2));
}

public static partial float64 /*r1*/ Ytgamma(ж<TLS> tls, float64 x3);

public static partial float32 /*r*/ Ytgammaf(ж<TLS> tls, float32 x);

public static partial float64 /*r*/ Ytgammal(ж<TLS> tls, float64 x);

public static partial Ttime_t /*r*/ Ytime(ж<TLS> tls, uintptr t);

public static partial Ttime_t /*r*/ Ytimegm(ж<TLS> tls, uintptr tm);

public static partial int32 /*r*/ Ytimer_delete(ж<TLS> tls, Ttimer_t t);

public static partial int32 /*r*/ Ytimer_getoverrun(ж<TLS> tls, Ttimer_t t);

public static partial int32 /*r*/ Ytimer_gettime(ж<TLS> tls, Ttimer_t t, uintptr val);

public static partial int32 /*r*/ Ytimer_settime(ж<TLS> tls, Ttimer_t t, int32 flags, uintptr val, uintptr old);

public static partial int32 /*r*/ Ytimerfd_create(ж<TLS> tls, int32 clockid, int32 flags);

public static partial int32 /*r*/ Ytimerfd_gettime(ж<TLS> tls, int32 fd, uintptr cur);

public static partial int32 /*r*/ Ytimerfd_settime(ж<TLS> tls, int32 fd, int32 flags, uintptr new1, uintptr old);

public static partial Tclock_t /*r*/ Ytimes(ж<TLS> tls, uintptr tms);

public static partial int32 /*r*/ Ytimespec_get(ж<TLS> tls, uintptr ts, int32 @base);

public static partial uintptr /*r*/ Ytmpfile(ж<TLS> tls);

public static partial uintptr /*r1*/ Ytmpnam(ж<TLS> tls, uintptr buf);

public static partial int32 /*r*/ Ytoascii(ж<TLS> tls, int32 c);

public static partial int32 /*r*/ Ytolower(ж<TLS> tls, int32 c);

public static partial int32 /*r*/ Ytolower_l(ж<TLS> tls, int32 c, Tlocale_t l);

public static partial int32 /*r*/ Ytoupper(ж<TLS> tls, int32 c);

public static partial int32 /*r*/ Ytoupper_l(ж<TLS> tls, int32 c, Tlocale_t l);

public static partial Twint_t /*r*/ Ytowctrans(ж<TLS> tls, Twint_t wc, Twctrans_t trans);

public static partial Twint_t /*r*/ Ytowctrans_l(ж<TLS> tls, Twint_t c, Twctrans_t t, Tlocale_t l);

public static partial Twint_t /*r*/ Ytowlower(ж<TLS> tls, Twint_t wc);

public static partial Twint_t /*r*/ Ytowlower_l(ж<TLS> tls, Twint_t c, Tlocale_t l);

public static partial Twint_t /*r*/ Ytowupper(ж<TLS> tls, Twint_t wc);

public static partial Twint_t /*r*/ Ytowupper_l(ж<TLS> tls, Twint_t c, Tlocale_t l);

public static partial float64 /*r*/ Ytrunc(ж<TLS> tls, float64 x3);

public static partial int32 /*r*/ Ytruncate(ж<TLS> tls, uintptr path, Toff_t length);

public static partial float32 /*r*/ Ytruncf(ж<TLS> tls, float32 x3);

public static partial float64 /*r*/ Ytruncl(ж<TLS> tls, float64 x);

public static partial uintptr /*r1*/ Ytsearch(ж<TLS> tls, uintptr key, uintptr rootp, uintptr __ccgo_fp_cmp);

internal static partial int32 /*_3*/ __ccgo_abi0_tsearch_2(ж<TLS> _0, uintptr _1, uintptr _2, uintptr __ccgo_fp);

internal static void __ccgo_abiInternal_tsearch_2(ref TLS tls, uintptr dest, uintptr abi0CodePtr) {
    ref var f = ref heap<Func<ж<TLS>, uintptr, uintptr, int32>>((ж<TLS> _0, uintptr _1, uintptr _2) => {
        return __ccgo_abi0_tsearch_2(_0, _1, _2, abi0CodePtr);
    }, out var Ꮡf);
    @unsafe.Pointer.StoreArray<uintptr>((@unsafe.Pointer)dest, 2, @unsafe.Pointer.LoadArray<uintptr>((@unsafe.Pointer)((uintptr)GoFuncCookie.Of(f)), 2));
}

public static partial uintptr /*r*/ Yttyname(ж<TLS> tls, int32 fd);

public static partial int32 /*r*/ Yttyname_r(ж<TLS> tls, int32 fd, uintptr name, Tsize_t size);

public static partial void Ytwalk(ж<TLS> tls, uintptr root, uintptr __ccgo_fp_action);

internal static partial void __ccgo_abi0_twalk_1(ж<TLS> _0, uintptr _1, int32 _2, int32 _3, uintptr __ccgo_fp);

internal static void __ccgo_abiInternal_twalk_1(ref TLS tls, uintptr dest, uintptr abi0CodePtr) {
    ref var f = ref heap<Action<ж<TLS>, uintptr, int32, int32>>((ж<TLS> _0, uintptr _1, int32 _2, int32 _3) => {
        __ccgo_abi0_twalk_1(_0, _1, _2, _3, abi0CodePtr);
    }, out var Ꮡf);
    @unsafe.Pointer.StoreArray<uintptr>((@unsafe.Pointer)dest, 2, @unsafe.Pointer.LoadArray<uintptr>((@unsafe.Pointer)((uintptr)GoFuncCookie.Of(f)), 2));
}

public static partial void Ytzset(ж<TLS> tls);

public static partial uint32 /*r*/ Yualarm(ж<TLS> tls, uint32 value, uint32 interval);

public static partial int32 /*r*/ Yulckpwdf(ж<TLS> tls);

public static partial int64 /*r*/ Yulimit(ж<TLS> tls, int32 cmd, uintptr va);

public static partial Tmode_t /*r*/ Yumask(ж<TLS> tls, Tmode_t mode);

public static partial int32 /*r*/ Yumount(ж<TLS> tls, uintptr special);

public static partial int32 /*r*/ Yumount2(ж<TLS> tls, uintptr special, int32 flags);

public static partial int32 /*r*/ Yuname(ж<TLS> tls, uintptr uts);

public static partial int32 /*r*/ Yungetc(ж<TLS> tls, int32 c, uintptr f);

public static partial Twint_t /*r*/ Yungetwc(ж<TLS> tls, Twint_t c, uintptr f);

public static partial int32 /*r*/ Yunlink(ж<TLS> tls, uintptr path);

public static partial int32 /*r*/ Yunlinkat(ж<TLS> tls, int32 fd, uintptr path, int32 flag);

public static partial int32 /*r*/ Yunlockpt(ж<TLS> tls, int32 fd);

public static partial int32 /*r*/ Yunsetenv(ж<TLS> tls, uintptr name);

public static partial int32 /*r*/ Yunshare(ж<TLS> tls, int32 flags);

public static partial void Yupdwtmp(ж<TLS> tls, uintptr f, uintptr u);

public static partial void Yupdwtmpx(ж<TLS> tls, uintptr f, uintptr u);

public static partial Tlocale_t /*r*/ Yuselocale(ж<TLS> tls, Tlocale_t new1);

public static partial int32 /*r*/ Yusleep(ж<TLS> tls, uint32 useconds);

public static partial int32 /*r*/ Yutime(ж<TLS> tls, uintptr path, uintptr times);

public static partial int32 /*r1*/ Yutimensat(ж<TLS> tls, int32 fd, uintptr path, uintptr times, int32 flags);

public static partial int32 /*r*/ Yutimes(ж<TLS> tls, uintptr path, uintptr times);

public static partial void Yuuid_copy(ж<TLS> t, uintptr dst, uintptr src);

public static partial void Yuuid_generate_random(ж<TLS> t, uintptr @out);

public static partial int32 /*_3*/ Yuuid_parse(ж<TLS> t, uintptr @in, uintptr uu);

public static partial void Yuuid_unparse(ж<TLS> t, uintptr uu, uintptr @out);

public static partial int32 /*r*/ Yvasprintf(ж<TLS> tls, uintptr s, uintptr fmt, Tva_list ap);

public static partial int32 /*r*/ Yvdprintf(ж<TLS> tls, int32 fd, uintptr fmt, Tva_list ap);

public static partial void Yverr(ж<TLS> tls, int32 status, uintptr fmt, Tva_list ap);

public static partial void Yverrx(ж<TLS> tls, int32 status, uintptr fmt, Tva_list ap);

public static partial int32 /*r*/ Yversionsort(ж<TLS> tls, uintptr a, uintptr b);

public static partial Tpid_t /*r*/ Yvfork(ж<TLS> tls);

public static partial int32 /*r*/ Yvfprintf(ж<TLS> tls, uintptr f, uintptr fmt, Tva_list ap);

public static partial int32 /*r*/ Yvfscanf(ж<TLS> tls, uintptr f, uintptr fmt, Tva_list ap);

public static partial int32 /*r*/ Yvfwprintf(ж<TLS> tls, uintptr f, uintptr fmt, Tva_list ap);

public static partial int32 /*r*/ Yvfwscanf(ж<TLS> tls, uintptr f, uintptr fmt, Tva_list ap);

public static partial int32 /*r*/ Yvhangup(ж<TLS> tls);

public static partial Tssize_t /*r*/ Yvmsplice(ж<TLS> tls, int32 fd, uintptr iov, Tsize_t cnt, uint32 flags);

public static partial int32 /*r*/ Yvprintf(ж<TLS> tls, uintptr fmt, Tva_list ap);

public static partial int32 /*r*/ Yvscanf(ж<TLS> tls, uintptr fmt, Tva_list ap);

public static partial int32 /*r*/ Yvsnprintf(ж<TLS> tls, uintptr s, Tsize_t n, uintptr fmt, Tva_list ap);

public static partial int32 /*r*/ Yvsprintf(ж<TLS> tls, uintptr s, uintptr fmt, Tva_list ap);

public static partial int32 /*r*/ Yvsscanf(ж<TLS> tls, uintptr s, uintptr fmt, Tva_list ap);

public static partial int32 /*r1*/ Yvswprintf(ж<TLS> tls, uintptr s, Tsize_t n, uintptr fmt, Tva_list ap);

public static partial int32 /*r*/ Yvswscanf(ж<TLS> tls, uintptr s, uintptr fmt, Tva_list ap);

public static partial void Yvwarn(ж<TLS> tls, uintptr fmt, Tva_list ap);

public static partial void Yvwarnx(ж<TLS> tls, uintptr fmt, Tva_list ap);

public static partial int32 /*r*/ Yvwprintf(ж<TLS> tls, uintptr fmt, Tva_list ap);

public static partial int32 /*r*/ Yvwscanf(ж<TLS> tls, uintptr fmt, Tva_list ap);

public static partial Tpid_t /*r*/ Ywait(ж<TLS> tls, uintptr status);

public static partial Tpid_t /*r*/ Ywait3(ж<TLS> tls, uintptr status, int32 options, uintptr usage);

public static partial Tpid_t /*r1*/ Ywait4(ж<TLS> tls, Tpid_t pid, uintptr status, int32 options, uintptr ru);

public static partial int32 /*r*/ Ywaitid(ж<TLS> tls, Tidtype_t type1, Tid_t id, uintptr info, int32 options);

public static partial Tpid_t /*r*/ Ywaitpid(ж<TLS> tls, Tpid_t pid, uintptr status, int32 options);

public static partial void Ywarn(ж<TLS> tls, uintptr fmt, uintptr va);

public static partial void Ywarnx(ж<TLS> tls, uintptr fmt, uintptr va);

public static partial uintptr /*r*/ Ywcpcpy(ж<TLS> tls, uintptr d, uintptr s);

public static partial uintptr /*r*/ Ywcpncpy(ж<TLS> tls, uintptr d, uintptr s, Tsize_t n);

public static partial Tsize_t /*r*/ Ywcrtomb(ж<TLS> tls, uintptr s, Twchar_t wc, uintptr st);

public static partial int32 /*r1*/ Ywcscasecmp(ж<TLS> tls, uintptr l, uintptr r);

public static partial int32 /*r1*/ Ywcscasecmp_l(ж<TLS> tls, uintptr l, uintptr r, Tlocale_t locale);

public static partial uintptr /*r*/ Ywcscat(ж<TLS> tls, uintptr dest, uintptr src);

public static partial uintptr /*r*/ Ywcschr(ж<TLS> tls, uintptr s, Twchar_t c);

public static partial int32 /*r1*/ Ywcscmp(ж<TLS> tls, uintptr l, uintptr r);

public static partial int32 /*r1*/ Ywcscoll(ж<TLS> tls, uintptr l, uintptr r);

public static partial int32 /*r1*/ Ywcscoll_l(ж<TLS> tls, uintptr l, uintptr r, Tlocale_t locale);

public static partial uintptr /*r*/ Ywcscpy(ж<TLS> tls, uintptr d, uintptr s);

public static partial Tsize_t /*r*/ Ywcscspn(ж<TLS> tls, uintptr s, uintptr c);

public static partial uintptr /*r*/ Ywcsdup(ж<TLS> tls, uintptr s);

public static partial Tsize_t /*r*/ Ywcsftime(ж<TLS> tls, uintptr wcs, Tsize_t n, uintptr f, uintptr tm);

public static partial Tsize_t /*r*/ Ywcsftime_l(ж<TLS> tls, uintptr s, Tsize_t n, uintptr f, uintptr tm, Tlocale_t loc);

public static partial Tsize_t /*r*/ Ywcslen(ж<TLS> tls, uintptr s);

public static partial int32 /*r1*/ Ywcsncasecmp(ж<TLS> tls, uintptr l, uintptr r, Tsize_t n);

public static partial int32 /*r1*/ Ywcsncasecmp_l(ж<TLS> tls, uintptr l, uintptr r, Tsize_t n, Tlocale_t locale);

public static partial uintptr /*r*/ Ywcsncat(ж<TLS> tls, uintptr d, uintptr s, Tsize_t n);

public static partial int32 /*r1*/ Ywcsncmp(ж<TLS> tls, uintptr l, uintptr r, Tsize_t n);

public static partial uintptr /*r*/ Ywcsncpy(ж<TLS> tls, uintptr d, uintptr s, Tsize_t n);

public static partial Tsize_t /*r*/ Ywcsnlen(ж<TLS> tls, uintptr s, Tsize_t n);

public static partial Tsize_t /*r*/ Ywcsnrtombs(ж<TLS> tls, uintptr dst, uintptr wcs, Tsize_t wn, Tsize_t n, uintptr st);

public static partial uintptr /*r*/ Ywcspbrk(ж<TLS> tls, uintptr s, uintptr b);

public static partial uintptr /*r*/ Ywcsrchr(ж<TLS> tls, uintptr s, Twchar_t c);

public static partial Tsize_t /*r*/ Ywcsrtombs(ж<TLS> tls, uintptr s, uintptr ws, Tsize_t n, uintptr st);

public static partial Tsize_t /*r*/ Ywcsspn(ж<TLS> tls, uintptr s, uintptr c);

public static partial uintptr /*r*/ Ywcsstr(ж<TLS> tls, uintptr h, uintptr n);

public static partial float64 /*r*/ Ywcstod(ж<TLS> tls, uintptr s, uintptr p);

public static partial float32 /*r*/ Ywcstof(ж<TLS> tls, uintptr s, uintptr p);

public static partial Tintmax_t /*r*/ Ywcstoimax(ж<TLS> tls, uintptr s, uintptr p, int32 @base);

public static partial uintptr /*r*/ Ywcstok(ж<TLS> tls, uintptr s, uintptr sep, uintptr p);

public static partial int64 /*r*/ Ywcstol(ж<TLS> tls, uintptr s, uintptr p, int32 @base);

public static partial float64 /*r*/ Ywcstold(ж<TLS> tls, uintptr s, uintptr p);

public static partial int64 /*r*/ Ywcstoll(ж<TLS> tls, uintptr s, uintptr p, int32 @base);

public static partial Tsize_t /*r*/ Ywcstombs(ж<TLS> tls, uintptr s, uintptr ws, Tsize_t n);

public static partial uint64 /*r*/ Ywcstoul(ж<TLS> tls, uintptr s, uintptr p, int32 @base);

public static partial uint64 /*r*/ Ywcstoull(ж<TLS> tls, uintptr s, uintptr p, int32 @base);

public static partial Tuintmax_t /*r*/ Ywcstoumax(ж<TLS> tls, uintptr s, uintptr p, int32 @base);

public static partial uintptr /*r*/ Ywcswcs(ж<TLS> tls, uintptr haystack, uintptr needle);

public static partial int32 /*r*/ Ywcswidth(ж<TLS> tls, uintptr wcs, Tsize_t n);

public static partial Tsize_t /*r*/ Ywcsxfrm(ж<TLS> tls, uintptr dest, uintptr src, Tsize_t n);

public static partial Tsize_t /*r*/ Ywcsxfrm_l(ж<TLS> tls, uintptr dest, uintptr src, Tsize_t n, Tlocale_t loc);

public static partial int32 /*r*/ Ywctob(ж<TLS> tls, Twint_t c);

public static partial int32 /*r*/ Ywctomb(ж<TLS> tls, uintptr s, Twchar_t wc);

public static partial Twctrans_t /*r*/ Ywctrans(ж<TLS> tls, uintptr @class);

public static partial Twctrans_t /*r*/ Ywctrans_l(ж<TLS> tls, uintptr s, Tlocale_t l);

public static partial Twctype_t /*r*/ Ywctype(ж<TLS> tls, uintptr s);

public static partial Twctype_t /*r*/ Ywctype_l(ж<TLS> tls, uintptr s, Tlocale_t l);

public static partial int32 /*r*/ Ywcwidth(ж<TLS> tls, Twchar_t wc);

public static partial uintptr /*r*/ Ywmemchr(ж<TLS> tls, uintptr s, Twchar_t c, Tsize_t n);

public static partial int32 /*r1*/ Ywmemcmp(ж<TLS> tls, uintptr l, uintptr r, Tsize_t n);

public static partial uintptr /*r*/ Ywmemcpy(ж<TLS> tls, uintptr d, uintptr s, Tsize_t n);

public static partial uintptr /*r*/ Ywmemmove(ж<TLS> tls, uintptr d, uintptr s, Tsize_t n);

public static partial uintptr /*r*/ Ywmemset(ж<TLS> tls, uintptr d, Twchar_t c, Tsize_t n);

public static partial int32 /*r*/ Ywprintf(ж<TLS> tls, uintptr fmt, uintptr va);

public static partial Tssize_t /*r*/ Ywrite(ж<TLS> tls, int32 fd, uintptr buf, Tsize_t count);

public static partial Tssize_t /*r*/ Ywritev(ж<TLS> tls, int32 fd, uintptr iov, int32 count);

public static partial int32 /*r*/ Ywscanf(ж<TLS> tls, uintptr fmt, uintptr va);

public static partial float64 /*r*/ Yy0(ж<TLS> tls, float64 x);

public static partial float32 /*r*/ Yy0f(ж<TLS> tls, float32 x);

public static partial float64 /*r*/ Yy1(ж<TLS> tls, float64 x);

public static partial float32 /*r*/ Yy1f(ж<TLS> tls, float32 x);

public static partial float64 /*r*/ Yyn(ж<TLS> tls, int32 n, float64 x);

public static partial float32 /*r*/ Yynf(ж<TLS> tls, int32 n, float32 x);

} // end libc_package
