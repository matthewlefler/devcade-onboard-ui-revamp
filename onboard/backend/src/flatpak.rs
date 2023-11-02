use glib::variant::{DictEntry, FromVariant, Variant};
use libflatpak::gio::glib;
use std::any::TypeId;
use std::error::Error;
use std::fmt::{self, Debug, Display};
use std::marker::PhantomData;

#[derive(glib::Variant)]
pub struct FlatpakFile {
    metadata: Vec<DictEntry<String, Variant>>,
}

#[derive(Debug, Clone)]
pub enum FlatpakMetadataError<T: FromVariant + Debug + 'static> {
    MissingKey(String),
    IncorrectFormat(String, PhantomData<T>),
}

impl<T: FromVariant + Debug + 'static> Error for FlatpakMetadataError<T> {}

impl<T: FromVariant + Debug + 'static> Display for FlatpakMetadataError<T> {
    fn fmt(&self, f: &mut fmt::Formatter<'_>) -> fmt::Result {
        use FlatpakMetadataError::*;
        match self {
            MissingKey(key_name) => write!(f, "Flatpak metadata incorrect: missing key {key_name}"),
            IncorrectFormat(key_name, _type) => write!(
                f,
                "Flatpak metadata incorrect: couldn't read key {key_name} of type {:?}",
                TypeId::of::<T>()
            ),
        }
    }
}

impl FlatpakFile {
    pub fn get_metadata_key<T: FromVariant + Debug>(
        &self,
        key: &str,
    ) -> Result<T, FlatpakMetadataError<T>> {
        let variant = self
            .metadata
            .iter()
            .find(|entry| entry.key() == key)
            .ok_or_else(|| FlatpakMetadataError::MissingKey(key.to_string()))?
            .value();
        T::from_variant(variant)
            .ok_or_else(|| FlatpakMetadataError::IncorrectFormat(key.to_string(), PhantomData {}))
    }
    pub fn load<T: AsRef<[u8]>>(bytes: T) -> Result<Self, FlatpakFileIncorrectFormat> {
        FlatpakFile::from_variant(&Variant::from_data::<FlatpakFile, _>(bytes))
            .ok_or(FlatpakFileIncorrectFormat)
    }
}

#[derive(Debug, Clone)]
pub struct FlatpakFileIncorrectFormat;

impl Error for FlatpakFileIncorrectFormat {}
impl Display for FlatpakFileIncorrectFormat {
    fn fmt(&self, f: &mut fmt::Formatter<'_>) -> fmt::Result {
        write!(f, "FlatpakFileIncorrectFormat")
    }
}
