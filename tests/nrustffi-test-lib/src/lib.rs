use core::slice;
use std::{mem, time::Duration};

#[async_ffi::async_ffi]
#[unsafe(no_mangle)]
pub async extern "C" fn sleep(millis: u64) {
    smol::Timer::after(Duration::from_millis(millis)).await;
}

#[async_ffi::async_ffi]
#[unsafe(no_mangle)]
pub async extern "C" fn sleep_and_get_usize(millis: u64) -> usize {
    smol::Timer::after(Duration::from_millis(millis)).await;
    123
}

#[async_ffi::async_ffi]
#[unsafe(no_mangle)]
pub async extern "C" fn sleep_and_get_u32(millis: u64) -> u32 {
    smol::Timer::after(Duration::from_millis(millis)).await;
    123
}

#[async_ffi::async_ffi]
#[unsafe(no_mangle)]
pub async extern "C" fn sleep_and_get_static_str(millis: u64) -> FFIStr<'static> {
    smol::Timer::after(Duration::from_millis(millis)).await;
    "A static str from Rust!".into()
}

#[async_ffi::async_ffi]
#[unsafe(no_mangle)]
pub async extern "C" fn sleep_and_get_string(millis: u64) -> FFIString {
    smol::Timer::after(Duration::from_millis(millis)).await;
    String::from("An owned String from Rust!").into()
}

#[unsafe(no_mangle)]
pub extern "C" fn drop_string(_s: FFIString) {}

#[async_ffi::async_ffi]
#[unsafe(no_mangle)]
pub async extern "C" fn sleep_and_panic(millis: u64) {
    smol::Timer::after(Duration::from_millis(millis)).await;
    panic!("Something went wrong!")
}

#[async_ffi::async_ffi]
#[unsafe(no_mangle)]
pub async extern "C" fn instant_panic() {
    panic!("Something went wrong!")
}

#[async_ffi::async_ffi]
#[unsafe(no_mangle)]
pub async extern "C" fn sleep_and_get_option_usize(millis: u64, with_some: bool) -> FFIOption<usize> {
    smol::Timer::after(Duration::from_millis(millis)).await;
    (if with_some { Some(123) } else { None }).into()
}

#[async_ffi::async_ffi]
#[unsafe(no_mangle)]
pub async extern "C" fn sleep_and_get_result_usize(millis: u64, with_ok: bool) -> FFIResult<usize, FFIStr<'static>> {
    smol::Timer::after(Duration::from_millis(millis)).await;
    (if with_ok { Ok(123) } else { Err("An error occurred".into()) }).into()
}

#[repr(C)]
pub struct FFIString {
    ptr: *mut u8,
    len: usize,
    capacity: usize
}

impl From<String> for FFIString {
    #[inline]
    fn from(mut value: String) -> Self {
        let v = Self {
            ptr: value.as_mut_ptr(),
            len: value.len(),
            capacity: value.capacity()
        };        
        mem::forget(value);
        v
    }
}

impl Drop for FFIString {
    fn drop(&mut self) {
        unsafe {
            String::from_raw_parts(self.ptr, self.len, self.capacity);
        }
    }
}

#[repr(C)]
pub struct FFIStr<'a> {
    ptr: &'a u8,
    len: usize
}

impl<'a> From<&'a str> for FFIStr<'a> {
    fn from(value: &'a str) -> Self {
        unsafe {
            Self {
                ptr: &*value.as_ptr(),
                len: value.len()
            }
        }
    }
}

impl<'a> FFIStr<'a> {
    fn as_str(&self) -> &'a str {
        unsafe {
            str::from_utf8_unchecked(slice::from_raw_parts(self.ptr, self.len))
        }
    }
}

#[repr(C, u8)]
pub enum FFIOption<T> {
    None,
    Some(T)
}

impl<T> From<Option<T>> for FFIOption<T> {
    fn from(value: Option<T>) -> Self {
        match value {
            Some(v) => FFIOption::Some(v),
            None => FFIOption::None,
        }
    }
}

impl<T, E> From<Result<T, E>> for FFIOption<T> {
    fn from(value: Result<T, E>) -> Self {
        match value {
            Ok(v) => FFIOption::Some(v),
            Err(_) => FFIOption::None,
        }
    }
}

#[repr(C, u8)]
pub enum FFIResult<T, E> {
    Ok(T),
    Err(E)
}

impl<T, E> From<Result<T, E>> for FFIResult<T, E> {
    fn from(value: Result<T, E>) -> Self {
        match value {
            Ok(v) => FFIResult::Ok(v),
            Err(e) => FFIResult::Err(e),
        }
    }
}