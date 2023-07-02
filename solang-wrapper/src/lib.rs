mod protos;

use std::ffi::{CStr, OsStr};
use contract_build;
use solang::{compile, Target};
use solang::file_resolver::FileResolver;
use inkwell;
use protos::messages::{Output, CompiledContract};
use protobuf::Message;


// include!(concat!(env!("OUT_DIR"), "/protos/mod.rs"));
// use messages::{Output, CompiledContract};

const ERROR_PROTOBUF_SERIALIZATION_ERROR: i32 = -0x1001;
const ERROR_INTEROP_BUFFER_TOO_SMALL_ERROR: i32 = -0x4001;

#[no_mangle]
pub extern "C" fn build_wasm(src: *const cty::c_char, buf: *mut cty::c_char, max_len: cty::c_int) -> cty::c_int {
    let src = unsafe { CStr::from_ptr(src).to_str().unwrap().to_owned() };
    let tmp_file = OsStr::new("test.sol");
    let mut cache = FileResolver::new();
    cache.set_file_contents(tmp_file.to_str().unwrap(), src);
    let opt = inkwell::OptimizationLevel::Default;
    let target = Target::default_substrate();
    let (wasm, ns) = compile(
        tmp_file,
        &mut cache,
        opt,
        target,
        false,
        true,
        true,
        Some(contract_build::OptimizationPasses::Z),
    );
    ns.print_diagnostics_in_plain(&cache, false);
    assert!(!wasm.is_empty());

    let mut output = Output::new();
    for (code, abi) in wasm {
        let mut cc = CompiledContract::new();
        cc.abi = abi;
        cc.wasm_code = code;
        output.contracts.push(cc);
    }

    match output.write_to_bytes() {
        Ok(result) => {
            copy_bytes(result.as_slice(), result.len(), buf, max_len);
            result.len().try_into().unwrap()
        }
        Err(_) => {
            ERROR_PROTOBUF_SERIALIZATION_ERROR
        }
    }
}

fn write_to_buffer(output: &String, buf: *mut cty::c_char, max_len: cty::c_int) -> cty::c_int {
    let src = output.as_bytes();
    let len = output.as_bytes().len();
    copy_bytes(src, len, buf, max_len)
}

fn copy_bytes(src: &[u8], len: usize, buf: *mut cty::c_char, max_len: cty::c_int) -> cty::c_int {
    let len_c_int = len as cty::c_int;
    if len_c_int <= max_len - 1 {
        unsafe {
            std::ptr::copy(src.as_ptr(), buf as *mut u8, len);
            (*buf.offset(len as isize)) = 0;
        }
        len_c_int
    } else {
        ERROR_INTEROP_BUFFER_TOO_SMALL_ERROR
    }
}


#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn it_works() {
        // let result = add(2, 2);
        // assert_eq!(result, 4);
    }
}
