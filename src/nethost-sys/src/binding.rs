/* automatically generated by rust-bindgen 0.55.1 */

pub type wchar_t = ::std::os::raw::c_ushort;
pub type max_align_t = f64;
pub type char_t = wchar_t;
#[repr(C)]
#[derive(Debug, Copy, Clone)]
pub struct get_hostfxr_parameters {
    pub size: usize,
    pub assembly_path: *const char_t,
    pub dotnet_root: *const char_t,
}
#[test]
fn bindgen_test_layout_get_hostfxr_parameters() {
    assert_eq!(
        ::std::mem::size_of::<get_hostfxr_parameters>(),
        24usize,
        concat!("Size of: ", stringify!(get_hostfxr_parameters))
    );
    assert_eq!(
        ::std::mem::align_of::<get_hostfxr_parameters>(),
        8usize,
        concat!("Alignment of ", stringify!(get_hostfxr_parameters))
    );
    assert_eq!(
        unsafe { &(*(::std::ptr::null::<get_hostfxr_parameters>())).size as *const _ as usize },
        0usize,
        concat!(
            "Offset of field: ",
            stringify!(get_hostfxr_parameters),
            "::",
            stringify!(size)
        )
    );
    assert_eq!(
        unsafe {
            &(*(::std::ptr::null::<get_hostfxr_parameters>())).assembly_path as *const _ as usize
        },
        8usize,
        concat!(
            "Offset of field: ",
            stringify!(get_hostfxr_parameters),
            "::",
            stringify!(assembly_path)
        )
    );
    assert_eq!(
        unsafe {
            &(*(::std::ptr::null::<get_hostfxr_parameters>())).dotnet_root as *const _ as usize
        },
        16usize,
        concat!(
            "Offset of field: ",
            stringify!(get_hostfxr_parameters),
            "::",
            stringify!(dotnet_root)
        )
    );
}
extern "C" {
    pub fn get_hostfxr_path(
        buffer: *mut char_t,
        buffer_size: *mut usize,
        parameters: *const get_hostfxr_parameters,
    ) -> ::std::os::raw::c_int;
}
