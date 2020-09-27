#region License
/*
MIT License

Copyright(c) 2020 Petteri Kautonen

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace CryptEnvVar
{
    /// <summary>
    /// A class to encrypt or decrypt strings using <see cref="AesManaged"/> using base-64 encoding for the encrypted data.
    /// </summary>
    public class EncryptDecryptTextAesBase64
    {
        /// <summary>
        /// The alphabets both upper and lower to generate characters to the pass-phrase end in case it is too short.
        /// </summary>
        private const string Alphabets = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";

        /// <summary>
        /// Encrypts the file blocks using <see cref="AesManaged"/> and returns them as list of base-64 encoded strings.
        /// </summary>
        /// <param name="fileName">Name of the file to split into encrypted base-64 encoded string blocks.</param>
        /// <param name="password">The password to use for the encryption.</param>
        /// <param name="blockSize">Size of the block to use to split the file data.</param>
        /// <returns>A List&lt;System.String&gt; containing the encrypted data.</returns>
        public static List<string> EncryptFileBlocks(string fileName, string password, int blockSize)
        {
            return EncryptFileBlocks(File.ReadAllBytes(fileName), password, blockSize);
        }

        /// <summary>
        /// Fills the password to match the length required for <see cref="AesManaged"/> 256 bit key size.
        /// </summary>
        /// <param name="password">The password to fill with characters.</param>
        /// <returns>The password matching the length required for <see cref="AesManaged"/> 256 bit key size.</returns>
        /// <remarks>The data is not randomized encrypted.</remarks>
        private static string FillPassword(string password)
        {
            while (password.Length < 32)
            {
                password += Alphabets[(Alphabets.Length * password.Length) % 32];
            }

            return password;
        }

        /// <summary>
        /// Gets or sets the size of the buffer to be used with the encryption and decryption algorithms.
        /// </summary>
        /// <value>The size of the buffer to be used with the encryption and decryption algorithms.</value>
        public static int BufferSize { get; set; } = 100000;

        /// <summary>
        /// Encrypts the file blocks using <see cref="AesManaged"/> encryption with the specified file contents, password and block size (data blocks).
        /// </summary>
        /// <param name="contents">The contents of the file.</param>
        /// <param name="password">The password for the encryption.</param>
        /// <param name="blockSize">Size of the block.</param>
        /// <returns>List&lt;System.String&gt;. containing the file data divided into encrypted base-64 encoded string blocks.</returns>
        /// <exception cref="ArgumentOutOfRangeException">password - At least six digits is required for the password.</exception>
        public static List<string> EncryptFileBlocks(byte[] contents, string password, int blockSize)
        {
            List<string> result = new List<string>();

            if (password.Length < 6)
            {
                throw new ArgumentOutOfRangeException(nameof(password), @"At least six digits is required for the password.");
            }

            password = FillPassword(password);

            using var aes = new AesManaged
            {
                Padding = PaddingMode.ISO10126, 
                KeySize = 256, 
                Key = Encoding.UTF8.GetBytes(password, 0, 32),
            };

            using var fileContents = new MemoryStream(contents);

            using var reader = fileContents;

            List<byte> block = new List<byte>();
            byte [] buffer = new byte[BufferSize];
            int read;

            var additionalDecrement = aes.KeySize / 8;

            while ((read = reader.Read(buffer, 0, blockSize - aes.IV.Length - additionalDecrement)) > 0) 
            {
                // generate a new IV..
                aes.GenerateIV();

                // add the read data block to the buffer..
                AddBytes(ref block, buffer, read);

                using var memoryStream = new MemoryStream();

                // ReSharper disable once IdentifierTypo
                using var encryptor = aes.CreateEncryptor();

                using var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write);

                cryptoStream.Write(buffer, 0, read);
                cryptoStream.FlushFinalBlock();

                result.Add(Convert.ToBase64String(InsertBytes(memoryStream.ToArray(), aes.IV)));
            }

            return result;
        }

        /// <summary>
        /// Adds specified amount of bytes to a specified byte list and additionally specified length.
        /// </summary>
        /// <param name="bytes">The bytes to add more bytes into.</param>
        /// <param name="addBytes">The bytes to add.</param>
        /// <param name="length">The length of how many bytes to add.</param>
        private static void AddBytes(ref List<byte> bytes, byte[] addBytes, int length = 0)
        {
            length = length == 0 ? addBytes.Length : length;

            for (int i = 0; i < length; i++)
            {
                bytes.Add(addBytes[i]);
            }
        }

        /// <summary>
        /// Inserts the specified bytes into a specified byte array and additionally specified length.
        /// </summary>
        /// <param name="bytes">The bytes to insert more bytes into.</param>
        /// <param name="addBytes">The bytes to insert.</param>
        /// <param name="length">The length of bytes to insert.</param>
        /// <returns>System.Byte[] containing the combined byte array result.</returns>
        private static byte[] InsertBytes(byte[] bytes, byte[] addBytes, int length = 0)
        {
            List<byte> result = new List<byte>(bytes);

            length = length == 0 ? addBytes.Length : length;

            for (int i = 0; i < length; i++)
            {
                result.Insert(i, addBytes[i]);
            }

            return result.ToArray();
        }

        /// <summary>
        /// Decrypts the specified encrypted base-64 environment variables into a byte array.
        /// </summary>
        /// <param name="password">The password to use for the decryption.</param>
        /// <param name="blocks">A semicolon-delimited array of environment variables containing the data to decrypt.</param>
        /// <returns>System.Byte[] containing the combined data of the from the specified environment variables.</returns>
        /// <exception cref="ArgumentOutOfRangeException">password - At least six digits is required for the password.</exception>
        public static byte[] DecryptBase64Blocks(string password, params string[] blocks)
        {
            using var result = new MemoryStream();

            if (password.Length < 6)
            {
                throw new ArgumentOutOfRangeException(nameof(password), @"At least six digits is required for the password.");
            }

            foreach (var block in blocks)
            {
                var writeData = DecryptBase64Block(password, Environment.GetEnvironmentVariable(block));
                result.Write(writeData, 0, writeData.Length);
            }

            return result.ToArray();
        }

        /// <summary>
        /// Decrypts a single base-64 encoded encrypted data.
        /// </summary>
        /// <param name="password">The password to use for the decryption.</param>
        /// <param name="base64data">The base-64 value containing the data to decrypt.</param>
        /// <returns>System.Byte[] containing the decrypted data.</returns>
        /// <exception cref="ArgumentOutOfRangeException">password - At least six digits is required for the password.</exception>
        public static byte[] DecryptBase64Block(string password, string base64data)
        {
            if (password.Length < 6)
            {
                throw new ArgumentOutOfRangeException(nameof(password), @"At least six digits is required for the password.");
            }

            using var result = new MemoryStream();
            password = FillPassword(password);

            using var aes = new AesManaged
            {
                Padding = PaddingMode.ISO10126,
                KeySize = 256,
                Key = Encoding.UTF8.GetBytes(password.ToCharArray(), 0, 32),
            };

            using var blockStream = new MemoryStream();
            blockStream.Write(Convert.FromBase64String(base64data));
            blockStream.Position = 0;
            var iv = new byte[aes.IV.Length];

            blockStream.Read(iv, 0, aes.IV.Length);
            aes.IV = iv;

            var encryptedData = new byte [BufferSize];
            var read = blockStream.Read(encryptedData, 0, BufferSize);

            // ReSharper disable once IdentifierTypo
            using var decryptor = aes.CreateDecryptor();
            using var encryptedStream = new MemoryStream();

            encryptedStream.Write(encryptedData, 0, read);
            encryptedStream.Position = 0;

            using var cryptoStream = new CryptoStream(encryptedStream, decryptor, CryptoStreamMode.Read);
            read = cryptoStream.Read(encryptedData, 0, read);

            result.Write(encryptedData, 0, read);
            return result.ToArray();
        }
    }
}
