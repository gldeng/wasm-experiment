package main

/*
#cgo LDFLAGS:-lsolang_wrapper -L/usr/local/lib -L${SRCDIR}/../target/release
#cgo CFLAGS:-I${SRCDIR}/solang-wrapper/include -I/usr/local/include
#include <stdlib.h>
#include <solang.h>
*/
import "C"
import (
	b64 "encoding/base64"
	"fmt"
	"unsafe"
)

const BufferSize = 2097152

func main() {
	src :=
		`contract simple {
        function foo() public pure returns (uint32) {
            return 2;
        }
    }`

	buffer := C.malloc(C.size_t(BufferSize))
	defer C.free(buffer)
	cText := C.CString(src)
	defer C.free(unsafe.Pointer(cText))
	res := C.build_wasm(cText, (*C.char)(buffer), BufferSize)
	if res < 0 {
		fmt.Printf("Failed with error code %v\n", res)
	}
	if res > 0 {
		fmt.Println(b64.StdEncoding.EncodeToString(unsafe.Slice((*byte)(buffer), int(res))))
	}
}
