using System;
using System.IO;
using System.Linq;

namespace Pulumi.Azure.Extensions.Utils
{
    internal static class ZipFileUtilities
    {
        private static readonly byte[] ZipBytes1 = { 0x50, 0x4b, 0x03, 0x04, 0x0a };
        private static readonly byte[] GzipBytes = { 0x1f, 0x8b };
        private static readonly byte[] TarBytes = { 0x1f, 0x9d };
        private static readonly byte[] LzhBytes = { 0x1f, 0xa0 };
        private static readonly byte[] Bzip2Bytes = { 0x42, 0x5a, 0x68 };
        private static readonly byte[] LzipBytes = { 0x4c, 0x5a, 0x49, 0x50 };
        private static readonly byte[] ZipBytes2 = { 0x50, 0x4b, 0x05, 0x06 };
        private static readonly byte[] ZipBytes3 = { 0x50, 0x4b, 0x07, 0x08 };
        private static readonly byte[][] All = { ZipBytes1, ZipBytes2, ZipBytes3, GzipBytes, TarBytes, LzhBytes, Bzip2Bytes, LzipBytes };

        public static bool IsZipFile(string filepath)
        {
            return IsCompressedData(GetFirstBytes(filepath, 5));
        }

        private static byte[] GetFirstBytes(string filepath, int length)
        {
            using (var streamReader = new StreamReader(filepath))
            {
                streamReader.BaseStream.Seek(0, 0);

                var bytes = new byte[length];
                streamReader.BaseStream.Read(bytes, 0, length);

                return bytes;
            }
        }

        private static bool IsCompressedData(byte[] data)
        {
            foreach (byte[] headerBytes in All)
            {
                if (HeaderBytesMatch(headerBytes, data))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool HeaderBytesMatch(byte[] headerBytes, byte[] dataBytes)
        {
            if (dataBytes.Length < headerBytes.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(dataBytes), $"Passed dataBytes length ({dataBytes.Length}) is shorter than the headerBytes ({headerBytes.Length})");
            }

            return !headerBytes.Where((t, i) => t != dataBytes[i]).Any();
        }

    }
}