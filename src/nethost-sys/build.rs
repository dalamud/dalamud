fn main() {
    println!("cargo:rustc-link-search=all=lib/dotnet");
    println!("cargo:rustc-link-lib=dylib=nethost");
}
