
# SharpQoi

### SharpQoi is an implementation of the [QOI](https://github.com/phoboslab/qoi) format for fast, lossless image compression

Supported functionality:
- [x] Encoding
- [x] Decoding
- [ ] Loading image through stbi (currently you can use StbImageSharp)

## Installation

Install stable releases via Nuget

| Package Name                   | Release (NuGet) |
|--------------------------------|-----------------|
| `SharpQoi`         | [![NuGet](https://img.shields.io/nuget/v/SharpQoi.svg)](https://www.nuget.org/packages/SharpQoi/)] (release tomorrow)

## API

### Encoding
```csharp
byte[] data = GetPngData();
int imgWidth = 1920;
int imgHeight = 1920;
int imgChannels = 3;
byte[] encoded = QoiEncoder.Encode(data, new QoiDescription(imgWidth, imgHeight, imgChannels))
```
### Decoding
```csharp
byte[] encodedData = GetEncodedData();
QoiImage img = QoiDecoder.Decode(encodedData)
```
## Usage example
```csharp
// There are no examples at the moment
```

## TODOs
* Benchmarks

## License

SharpQoi is licensed under the [MIT](LICENSE) license.
