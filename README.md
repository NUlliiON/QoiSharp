
# QoiSharp

#### ✅ **Project status: active**. [What does it mean?](https://github.com/NUlliiON/QoiSharp/blob/main/docs/project-status.md)
### QoiSharp is an implementation of the [QOI](https://github.com/phoboslab/qoi) format for fast, lossless image compression

Supported functionality:
- [x] Encoding
- [x] Decoding
- [ ] Loading image through stbi (currently you can use StbImageSharp)

## Installation

Install stable releases via Nuget

| Package Name                   | Release (NuGet) |
|--------------------------------|-----------------|
| `QoiSharp`         | [![NuGet](https://img.shields.io/nuget/v/QoiSharp.svg)](https://www.nuget.org/packages/QoiSharp/)

## API

### Encoding
```csharp
byte[] data = GetPngData();
int width = 1920;
int height = 1080;
int channels = 4;
byte[] qoiData = QoiEncoder.Encode(new QoiImage(pngData, width, height, channels));
```
### Decoding
```csharp
var qoiImage = QoiDecoder.Decode(qoiData);
Console.WriteLine($"Width: {qoiImage.Width}");
Console.WriteLine($"Height: {qoiImage.Height}");
Console.WriteLine($"Channels: {qoiImage.Channels}");
Console.WriteLine($"Color space: {qoiImage.ColorSpace}");
Console.WriteLine($"Data length: {qoiImage.Pixels.Length}");
```
## Usage example
```csharp
// There are no examples at the moment. You can check Tests project for more examples.
```

## TODOs
* Benchmarks

## License

QoiSharp is licensed under the [MIT](LICENSE) license.
