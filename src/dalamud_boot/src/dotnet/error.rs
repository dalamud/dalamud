use std::{
    fmt,
    error::Error as StdError
};
use dlopen;
use widestring::{self, UChar};
use crate::dotnet::{
    Status
};

#[derive(Debug)]
pub enum Error {
    /// An error from .NET components.
    /// You can check out the meaning of the code from their [github repository][code].
    /// 
    /// [code]: https://github.com/dotnet/runtime/blob/29e9b5b7fd95231d9cd9d3ae351404e63cbb6d5a/src/coreclr/src/inc/corerror.xml

    DotNet(Status),
    
    /// Nul character is not allowed but was found in the string.
    NulError,
}

impl StdError for Error {}

impl fmt::Display for Error {
    fn fmt(&self, f: &mut fmt::Formatter<'_>) -> fmt::Result {
        match self {
            Error::DotNet(status) => write!(f, ".NET runtime error (code {:?})", status),
            Error::NulError => write!(f, "nul character is not allowed"),
        }
    }
}

impl<C: UChar> From<widestring::NulError<C>> for Error {
    fn from(_: widestring::NulError<C>) -> Self {
        Error::NulError
    }
}

impl From<dlopen::Error> for Error {
    fn from(e: dlopen::Error) -> Self {
        match e {
            dlopen::Error::NullCharacter(_) => Error::NulError,
            e => todo!("{:?}", e),
        }
    }
}
