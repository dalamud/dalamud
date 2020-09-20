use std::{
    mem,
    ffi::CString,
    boxed::Box,
    error::Error,
    path::Path,
};
use serde::{Deserialize};
use log::{trace};

use crate::dotnet::hostfxr;

/// Run `Dalamud.Core` with 
pub unsafe fn run(param_str: &str) -> Result<(), Box<dyn Error>> {
    let param = serde_json::from_str::<Param>(param_str)?;
    
    let dalamud_core_path = Path::new(&param.dalamud_root).join("Dalamud.dll");
    let dalamud_runtime_cfg = Path::new(&param.dalamud_root).join("Dalamud.runtimeconfig.json");
    
    trace!("loading dalamud boot (runtime cfg={:?}) (asm={:?})", dalamud_runtime_cfg, dalamud_core_path);
    
    let hostfxr_path = hostfxr::get_hostfxr_path()?.to_os_string();
    let hostfxr = hostfxr::Library::load(&hostfxr_path)?;
    let mut context = hostfxr::Context::from_runtime_config(&hostfxr, dalamud_runtime_cfg)?;
    
    let func_ptr = context.load_assembly_and_get_function_pointer(
        &dalamud_core_path,
        "Dalamud.Boot.EntryPoint, Dalamud",
        "Initialize", 
        None)?;
    
    let func_ptr: Option<unsafe extern "stdcall" fn(*const i8, i32) -> i32>  = mem::transmute(func_ptr);
    let param_str = CString::new(param_str)?;
    
    // Call .NET function and get its return value.
    // Returns zero on success, otherwise returns non zero value.
    let status = (func_ptr.unwrap())(param_str.as_ptr(), param_str.to_bytes().len() as i32);
    trace!("EntryPoint init status: {}", status);

    match status {
        0 => Ok(()),
        _ => Err(format!("Dalamud.Boot.EntryPoint.Initialize reported an error: {}", status).into()),
    }
}

/// A boot parameter.
#[derive(Deserialize, Debug)]
pub struct Param {
    dalamud_root: String,
    profile_root: String,
}
