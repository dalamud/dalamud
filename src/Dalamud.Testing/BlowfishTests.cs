using System;
using Dalamud.Crypto;
using Xunit;

namespace Dalamud.Testing
{
    public class BlowfishTests
    {
        // let's not have a regression, shall we?

        [Fact]
        public void SimpleData()
        {
            var key = new byte[]
            {
                0x01, 0x23, 0x45, 0x67,
                0x89, 0xAB, 0xCD, 0xEF,
            };
            var data = new byte[]
            {
                0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41,
                0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41,
            };
            var expected = new byte[]
            {
                0xD4, 0x85, 0x7F, 0xFB, 0x02, 0x55, 0xCB, 0x80,
                0xD4, 0x85, 0x7F, 0xFB, 0x02, 0x55, 0xCB, 0x80,
            };

            // check BlowfishLE impl
            var blowfish = BlowfishLE.Create(key);
            blowfish.Encrypt(data);

            Assert.Equal(expected, data);
        }

        [Fact]
        public void InvalidKeyLength()
        {
            // notice that key here is too short
            var key = new byte[] { 0x01 };

            Assert.Throws<ArgumentException>(() =>
            {
                BlowfishLE.Create(key);
            });
        }

        [Fact]
        public void InvalidBlockLength()
        {
            ReadOnlySpan<byte> key = stackalloc byte[]
            {
                0x01, 0x23, 0x45, 0x67,
                0x89, 0xAB, 0xCD, 0xEF,
            };

            // notice that data length here is not multiple of block size (which is 8)
            var data = new byte[]
            {
                0x01, 0x23, 0x45, 0x67,
            };

            var blowfish = BlowfishLE.Create(key);

            Assert.Throws<ArgumentException>(() =>
            {
                blowfish.Encrypt(data);
            });
        }
    }
}
