<h1 align="center">

<img src="https://qoiformat.org/qoi-logo.svg" alt="QoiSharp" width="256"/>
<br/>
QoiSharp 
</h1>

#### ✅ **Project status: active**. [What does it mean?](https://github.com/NUlliiON/QoiSharp/blob/main/docs/project-status.md)
### QoiSharp is an implementation of the [QOI](https://github.com/phoboslab/qoi) format for fast, lossless image compression

Supported functionality:
- [x] Encoding
- [x] Decoding

## Installation

Install stable releases via Nuget

| Package Name                   | Release (NuGet) |
|--------------------------------|-----------------|
| `QoiSharp`         | [![NuGet](https://img.shields.io/nuget/v/QoiSharp.svg)](https://www.nuget.org/packages/QoiSharp/)

## API

### Encoding
```csharp
byte[] data = GetRawPixels();
int width = 1920;
int height = 1080;
var channels = Channels.RgbWithAlpha;
byte[] qoiData = QoiEncoder.Encode(new QoiImage(data, width, height, channels));
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
## Example Usage
### [Click here](https://github.com/NUlliiON/QoiSharp/tree/main/samples)

## TODOs
* Streams
* Benchmarks
* CLI

## License

QoiSharp is licensed under the [MIT](LICENSE) license.
