//! `hostfxr` runtime.
//!
//! Many of which are just a wrapper around `hostfxr` function.
//!
//! Reading their [design document][dotnet-hosting] first is recommended.
//!
//! [dotnet-hosting]: https://github.com/dotnet/core-setup/blob/master/Documentation/design-docs/native-hosting.md

use std::{
    ptr,
    mem::{self, ManuallyDrop},
    ffi::{OsStr, c_void},
};

use widestring::{WideCString, WideChar};
use dlopen::raw;

use hostfxr_sys::*;
use crate::dotnet::{
    Status,
    error::Error,
};


/// Call dotnet function then converts `HResult` into [`Error`](`crate::dotnet::Error`).
macro_rules! call_dotnet {
    ($fn: expr, $($arg: expr) , *) => {
        {
            // call dotnet function then convert return value to HResult
            let result: Status = ($fn.unwrap())(
                $($arg,)*
            ).into();
            
            // check if HResult is successful
            match result {
                _ if result.is_err() => Err(Error::DotNet(result)),
                _ => Ok(()),
            }
        }
    };
}


/// Search a path to `hostfxr.dll`.
///
/// This just wraps `get_hostfxr_path` from `nethost` in a way that caller doesn't have to care about size of the path buffer.
pub fn get_hostfxr_path() -> Result<WideCString, Error> {
    use nethost_sys as neth;
    
    let mut buf_len: usize = 0;
    
    // from docs:
    // If result_buffer is nullptr the input value is ignored and only the minimum required size in char_t units is set on output.
    let status: Status = unsafe { neth::get_hostfxr_path(ptr::null_mut(), &mut buf_len, ptr::null()) }.into();
    
    // It is possible that dotnet is not installed on the system
    if status != Status(/* HostApiBufferTooSmall */ 0x8000_8098) {
        return Err(Error::DotNet(status));
    }
    
    // allocate a path buffer and call again; note that buffer length counts in char_t units. (not bytes!)
    let mut buf: Vec<WideChar> = vec![0; buf_len];
    let status: Status = unsafe { neth::get_hostfxr_path(buf.as_mut_ptr(), &mut buf_len, ptr::null()) }.into();
    
    match status {
        _ if status.is_ok() => unsafe {
            // From nethost:
            // Buffer that will be populated with the hostfxr path, including a null terminator.
            Ok(WideCString::from_vec_with_nul_unchecked(buf))
        },
        _ => Err(Error::DotNet(status)),
    }
}

/// A reference to `hostfxr` library and its function pointers.
///
/// **Keep in mind** that `coreclr` will live **eternally** in the process once loaded;
/// and `hostfxr` whose job is to manage that list of "`coreclr`" handle shouldn't be an exception.
pub struct Library {
    /// A reference to `hostfxr`.
    /// This won't be droppeed even if `Library` went out of scope.
    lib: ManuallyDrop<raw::Library>,

    // func pointers
    close: hostfxr_close_fn,
    initialize_for_runtime_config: hostfxr_initialize_for_runtime_config_fn,
    get_runtime_delegate: hostfxr_get_runtime_delegate_fn,
}

impl Library {
    /// Load `hostfxr` library from `path`.
    ///
    /// # Safety
    /// This function will loaded a module pointed by `path` without actually checking it's correct `hostfxr` (there's no way to do it)
    /// and will execute entry point codes from that module blindly.
    ///
    /// Also, `hostfxr` library will be loaded if `Library` is dropped but that's undefined behavior.
    pub unsafe fn load(path: impl AsRef<OsStr>) -> Result<Library, Error> {
        let lib = ManuallyDrop::new(raw::Library::open(path)?);

        Ok(Self {
            close: lib.symbol("hostfxr_close")?,
            initialize_for_runtime_config: lib.symbol("hostfxr_initialize_for_runtime_config")?,
            get_runtime_delegate: lib.symbol("hostfxr_get_runtime_delegate")?,

            lib,
        })
    }
}


/// A wrapper for `hostfxr_handle`. (aka host context)
///
/// 
pub struct Context<'a> {
    lib: &'a Library,
    handle: hostfxr_handle,
}

impl<'a> Context<'a> {
    /// Initialize a host context based on `runtimeconfig.json` pointed by `path`.
    ///
    /// For more informations, read `hostfxr_initialize_for_runtime_config` docs.
    ///
    /// This context should not outlive its library originated; this is enforced by the lifetime `'a`.
    pub unsafe fn from_runtime_config(lib: &'a Library, path: impl AsRef<OsStr>) -> Result<Context<'a>, Error> {
        let path = WideCString::from_os_str(path)?;

        // init runtime context; note that we currently don't care about hostfxr_initialize_parameters
        let mut handle = ptr::null_mut();
        call_dotnet!(lib.initialize_for_runtime_config, path.as_ptr(), ptr::null(), &mut handle)?;

        Ok(Self {
            lib, handle
        })
    }

    /// Load an assembly from `asm_path` into its own `AssemblyLoadContext` and return a function pointer to requested method.
    ///
    /// Note that:
    /// - `ty` is a fully qualified type name and `method` is a static method name of that type. But should not be same as assembly name.
    ///   (otherwise load_assembly_and_get_function_pointer fails)
    /// - returned function pointer has a lifetime of `'static`.
    /// - this function also starts .NET runtime if it's not already running.
    /// - If `delegate_type` is `None` then `hostfxr` will use default prototype of `int (IntPtr param, int size)`
    pub unsafe fn load_assembly_and_get_function_pointer(
        &mut self, 
        asm_path: impl AsRef<OsStr>,
        ty: &str,
        method: &str,
        delegate_type: Option<&str>
    ) -> Result<*mut c_void, Error> {
        let asm_path = WideCString::from_os_str(asm_path)?;
        let ty = WideCString::from_str(ty)?;
        let method = WideCString::from_str(method)?;
        let delegate_type = delegate_type
            .and_then(|delegate_type| 
                Some(WideCString::from_str(delegate_type))
            )
            .transpose()?;
        let pdelegate_type = match &delegate_type {
            Some(x) => x.as_ptr(),
            None => ptr::null(),
        };
        
        // starts a runtime and get a delegate
        let mut get_func_ptr = ptr::null_mut();
        call_dotnet!(self.lib.get_runtime_delegate, self.handle, hostfxr_delegate_type_hdt_load_assembly_and_get_function_pointer, &mut get_func_ptr)?;

        // Get a function pointer from assembly
        let mut func = ptr::null_mut();
        call_dotnet!(
            mem::transmute::<_, load_assembly_and_get_function_pointer_fn>(get_func_ptr),
            asm_path.as_ptr(),
            ty.as_ptr(),
            method.as_ptr(),
            pdelegate_type,
            ptr::null_mut(),
            &mut func
        )?;

        Ok(func)
    }
}

impl<'a> Drop for Context<'a> {
    fn drop(&mut self) {
        unsafe {
            call_dotnet!(self.lib.close, self.handle)
                .expect("this should not fail; this is a bug");
        }
    }
}
