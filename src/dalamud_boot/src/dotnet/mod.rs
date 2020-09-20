//! A thin wrapper to .NET hosting API.
//!
//! TODO

pub mod hostfxr;
pub mod error;

pub use self::error::*;

/// A status code returned from .NET components. (which is really just `HResult`)
/// 
/// You can check out meaning of the code from their [github repository][code].
/// 
/// [code]: https://github.com/dotnet/runtime/blob/29e9b5b7fd95231d9cd9d3ae351404e63cbb6d5a/src/coreclr/src/inc/corerror.xml
#[repr(transparent)]
#[derive(Eq, PartialEq, Debug)]
pub struct Status(pub u32);

impl Status {
    /// Returns true if status code indicates a success.
    ///
    /// This value is mutually exclusive to `is_err()`. (i.e. `is_ok() == true` means `is_err() == false`)
    pub fn is_ok(&self) -> bool {
        // most significant bit represents an error
        (self.0 & 0x8000_0000) == 0
    }

    /// Returns true if status code indicates an error.
    ///
    /// This value is mutually exclusive to `is_err()`. (i.e. `is_ok() == true` means `is_err() == false`)
    #[allow(unused)]
    pub fn is_err(&self) -> bool {
        !self.is_ok()
    }
}

impl From<u32> for Status {
    fn from(c: u32) -> Self {
        Self(c)
    }
}

impl From<i32> for Status {
    fn from(c: i32) -> Self {
        // from nomicon: casting between two integers of the same size (e.g. i32 -> u32) is a no-op
        Self(c as u32)
    }
}
