#ifndef _SOLANG_H_
#define _SOLANG_H_

// Compiles the solidity src to wasm, returns the number of bytes written to the buffer
int build_wasm(const char* src, char* buf, int max_len);

#endif