﻿using System.Collections.Generic;
using Xunit;
using System.IO;
using Core.Compression;
using Core.RocketLeague.Decryption;
using Core.Types;
using FluentAssertions;
using NSubstitute;

namespace Core.RocketLeague.Tests
{
    public class PackageUnpackerTests
    {
        [Fact()]
        public void PackageUnpackerTest_UnpackKnownPackage_BinaryEqualToKnownOutput()
        {
            // Arrange
            var inputTest = File.OpenRead("TestData/RocketPass_Premium_T_SF.upk");
            var outputStream = new MemoryStream();
            var outputExpected = File.ReadAllBytes("TestData/RocketPass_Premium_T_SF_decrypted.upk");
            var decryptionProvider = new DecryptionProvider("keys.txt");
            // Act

            var unpacked = new PackageUnpacker(inputTest, outputStream, decryptionProvider);
            var outputBuffer = outputStream.ToArray();

            // Assert 

            outputBuffer.Length.Should().Be(outputExpected.Length);
            unpacked.DeserializationState.Should().Be(DeserializationState.Success);
            outputBuffer.Should().Equal(outputExpected);
        }

        [Fact]
        public void PackageUnpackerTest_UnpackPackage_CompressionFlagUnset()
        {
            // Arrange
            var inputTest = File.OpenRead("TestData/RocketPass_Premium_T_SF.upk");
            var outputStream = new MemoryStream();
            var decryptionProvider = new DecryptionProvider("keys.txt");

            var fileSummery = new FileSummary();
            // Act

            var unpacked = new PackageUnpacker(inputTest, outputStream, decryptionProvider);
            outputStream.Position = 0;
            fileSummery.Deserialize(new BinaryReader(outputStream));

            // Assert 
            fileSummery.CompressionFlags.Should().Be(ECompressionFlags.COMPRESS_None);
        }

        [Fact]
        public void PackageUnpackerTest_UnpackPackageWithMissingKey_DeserializationStateShouldBeFail()
        {
            // Arrange
            var inputTest = File.OpenRead("TestData/RocketPass_Premium_T_SF.upk");
            var outputStream = new MemoryStream();
            var decryptionProvider = Substitute.For<IDecrypterProvider>();
            decryptionProvider.DecryptionKeys.Returns(new List<byte[]>());

            // Act
            var unpacked = new PackageUnpacker(inputTest, outputStream, decryptionProvider);

            // Assert 
            unpacked.DeserializationState.Should().NotBe(DeserializationState.Success);
            unpacked.Valid.Should().Be(false);
        }
    }
}