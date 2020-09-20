//! TODO: docs

mod boot;
mod dotnet;

use std::{
    error::Error,
    ffi::CStr,
    panic,
};
use winapi::um::{
    winnt,
    consoleapi,
    memoryapi,
};
use log::{trace, info, error};

/// TODO: docs
#[no_mangle]
pub unsafe extern "system" fn dalamud_init(param: *mut i8) -> u32 {
    if cfg!(debug_assertions) {
        consoleapi::AllocConsole();
    }

    #[allow(unused_must_use)]
    {
        simple_logger::SimpleLogger::new().init();
    }

    info!("dalamud_boot loaded");
    trace!("dalamud_boot::dalamud_init called (param: {:?})", param);

    // run dalamud_boot
    let ret = panic::catch_unwind(|| {
        init_impl(param)
    });

    // check the result
    match ret {
        Ok(Err(e)) => {
            // error from init_impl
            error!("initializing dalamud failed: {:?}", e);

            0xDADE_0002
        },
        Err(e) => {
            // panic!
            error!("dalamud_init panicked: {:?}", e);

            0xDADE_0003
        }
        _ => 0
    }
}

unsafe fn init_impl(param: *mut i8) -> Result<(), Box<dyn Error>> {
    // copy a param string
    let arg = CStr::from_ptr(param)
        .to_string_lossy()
        .into_owned();
    
    trace!("dalamud_boot param: {}", arg);

    // deallocate a param; param should not be accessed from here on
    memoryapi::VirtualFree(param as *mut _, 0, winnt::MEM_RELEASE);

    boot::run(&arg)?;

    Ok(())
}
