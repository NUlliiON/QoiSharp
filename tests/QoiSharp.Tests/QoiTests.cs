using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using QoiSharp.Codec;
using StbImageSharp;
using Xunit;

namespace QoiSharp.Tests;

public class QoiTests
{
    [Theory]
    [InlineData(Constants.Images.PhotoTecnickSubDirectory)]
    [InlineData(Constants.Images.TexturesPhotoSubDirectory)]
    [InlineData(Constants.Images.PngImgSubDirectory)]
    [InlineData(Constants.Images.PhotoWikipediaSubDirectory)]
    public async Task RgbEncodingShouldWork(string imagesDirectoryPath)
    {
        // Arrange
        imagesDirectoryPath = Constants.Images.GetFullPath(imagesDirectoryPath);
        string imageFilePath = Directory.EnumerateFiles(imagesDirectoryPath).First(x => x.EndsWith(".png"));
        var originalImg = ImageResult.FromMemory(await File.ReadAllBytesAsync(imageFilePath), ColorComponents.RedGreenBlue);
        var qoiImage = new QoiImage(originalImg.Data, originalImg.Width, originalImg.Height, (Channels)originalImg.Comp);
        
        // Act
        byte[] qoiData = QoiEncoder.Encode(qoiImage);
        
        // Assert
        var img = QoiDecoder.Decode(qoiData);
        Assert.True(img.Data.SequenceEqual(qoiImage.Data));
        img.Width.Should().Be(qoiImage.Width);
        img.Height.Should().Be(qoiImage.Height);
        img.Channels.Should().Be(qoiImage.Channels);
        img.ColorSpace.Should().Be(qoiImage.ColorSpace);
    }
    
    [Theory]
    [InlineData(Constants.Images.PhotoTecnickSubDirectory)]
    [InlineData(Constants.Images.TexturesPhotoSubDirectory)]
    [InlineData(Constants.Images.PngImgSubDirectory)]
    [InlineData(Constants.Images.PhotoWikipediaSubDirectory)]
    public async Task RgbaEncodingShouldWork(string imagesDirectoryPath)
    {
        // Arrange
        imagesDirectoryPath = Constants.Images.GetFullPath(imagesDirectoryPath);
        string imageFilePath = Directory.EnumerateFiles(imagesDirectoryPath).First(x => x.EndsWith(".png"));
        var originalImg = ImageResult.FromMemory(await File.ReadAllBytesAsync(imageFilePath), ColorComponents.RedGreenBlueAlpha);
        var qoiImage = new QoiImage(originalImg.Data, originalImg.Width, originalImg.Height, (Channels)originalImg.Comp);
        
        // Act
        byte[] qoiData = QoiEncoder.Encode(qoiImage);
        
        // Assert
        var img = QoiDecoder.Decode(qoiData);
        Assert.True(img.Data.SequenceEqual(qoiImage.Data));
        img.Width.Should().Be(qoiImage.Width);
        img.Height.Should().Be(qoiImage.Height);
        img.Channels.Should().Be(qoiImage.Channels);
        img.ColorSpace.Should().Be(qoiImage.ColorSpace);
    }

    [Theory]
    [InlineData(Constants.Images.PhotoTecnickSubDirectory)]
    [InlineData(Constants.Images.TexturesPhotoSubDirectory)]
    [InlineData(Constants.Images.PngImgSubDirectory)]
    [InlineData(Constants.Images.PhotoWikipediaSubDirectory)]
    public async Task RgbDecodingShouldWork(string imagesDirectoryPath)
    {
        // Arrange
        imagesDirectoryPath = Constants.Images.GetFullPath(imagesDirectoryPath);
        string imageFilePath = Directory.EnumerateFiles(imagesDirectoryPath).First(x => x.EndsWith(".png"));
        var originalImg = ImageResult.FromMemory(await File.ReadAllBytesAsync(imageFilePath), ColorComponents.RedGreenBlue);
        var qoiImage = new QoiImage(originalImg.Data, originalImg.Width, originalImg.Height, (Channels)originalImg.Comp);
        byte[] qoiData = QoiEncoder.Encode(qoiImage);
        
        // Act
        var img = QoiDecoder.Decode(qoiData);

        // Assert
        Assert.True(img.Data.SequenceEqual(qoiImage.Data));
        img.Width.Should().Be(qoiImage.Width);
        img.Height.Should().Be(qoiImage.Height);
        img.Channels.Should().Be(qoiImage.Channels);
        img.ColorSpace.Should().Be(qoiImage.ColorSpace);
    }
    
    [Theory]
    [InlineData(Constants.Images.PhotoTecnickSubDirectory)]
    [InlineData(Constants.Images.TexturesPhotoSubDirectory)]
    [InlineData(Constants.Images.PngImgSubDirectory)]
    [InlineData(Constants.Images.PhotoWikipediaSubDirectory)]
    public async Task RgbaDecodingShouldWork(string imagesDirectoryPath)
    {
        // Arrange
        imagesDirectoryPath = Constants.Images.GetFullPath(imagesDirectoryPath);
        string imageFilePath = Directory.EnumerateFiles(imagesDirectoryPath).First(x => x.EndsWith(".png"));
        var originalImg = ImageResult.FromMemory(await File.ReadAllBytesAsync(imageFilePath), ColorComponents.RedGreenBlueAlpha);
        var qoiImage = new QoiImage(originalImg.Data, originalImg.Width, originalImg.Height, (Channels)originalImg.Comp);
        byte[] qoiData = QoiEncoder.Encode(qoiImage);
        
        // Act
        var img = QoiDecoder.Decode(qoiData);

        // Assert
        Assert.True(img.Data.SequenceEqual(qoiImage.Data));
        img.Width.Should().Be(qoiImage.Width);
        img.Height.Should().Be(qoiImage.Height);
        img.Channels.Should().Be(qoiImage.Channels);
        img.ColorSpace.Should().Be(qoiImage.ColorSpace);
    }
}