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
using System.IO;
using CommandLine;

namespace CryptEnvVar
{
    /// <summary>
    /// A program to encrypt or decrypt data from small files.
    /// </summary>
    class Program
    {
        /// <summary>
        /// The arguments passed to the program.
        /// </summary>
        public class Arguments
        {
            /// <summary>
            /// Gets or sets the password to be used with the encryption/decryption.
            /// </summary>
            [Option('s', "secret", Required = true, HelpText = "The secret to encrypt or decrypt data.")]
            public string Password { get; set; }

            /// <summary>
            /// Gets or sets the name of the file for data input or output.
            /// </summary>
            [Option('f', "file", Required = false, HelpText = "The file name to use.")]
            public string FileName { get; set; }

            /// <summary>
            /// Gets or sets the size of the block to be used with the encryption.
            /// </summary>
            [Option('b', "block", Required = false, HelpText = "The amount of encrypted data to include in a single base-64 encoded block. The default is 1024.")]
            public int BlockSize { get; set; }

            /// <summary>
            /// Gets or sets the environment variables as a semicolon (';') delimited list.
            /// </summary>
            /// <value>The environment variables.</value>
            [Option('e', "environment", Required = false, HelpText = "Environment variables to decrypt. This is a semicolon (';') delimited list.")]
            public string EnvironmentVariables { get; set; }
        }

        /// <summary>
        /// Defines the entry point of the application.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <returns>A integer value describing the execution status of the program.</returns>
        static int Main(string[] args)
        {
            void WriteBlockData(int blockNumber, string blockData, string split)
            {
                Console.WriteLine($"Block ({blockNumber:D4})");
                Console.WriteLine(split);
                Console.WriteLine(blockData);
                Console.WriteLine(split);
                Console.WriteLine();
            }

            // get the arguments given to the program..
            var arguments = Parser.Default.ParseArguments<Arguments>(args).Value;

            // validate that arguments were given to the software..-
            if (arguments == null)
            {
                return -1;
            }

            // default the block size..
            arguments.BlockSize = arguments.BlockSize == 0 ? 1024 : arguments.BlockSize;

            // create a console sized splitter line..
            var splitString = new string('-', 72);
            try
            {
                splitString = new string('-', Console.WindowWidth);
            }
            catch
            {
                // the Console.WindowWidth threw an exception..
            }

            try // we try so the possible error can be reported..
            {
                // validate the arguments for encryption of file contents.
                if ((arguments.FileName != null || Console.IsInputRedirected) && arguments.EnvironmentVariables == null)
                {
                    // encrypt a file passed as a parameter..
                    if (arguments.FileName != null && File.Exists(arguments.FileName))
                    {
                        var blockNum = 0;

                        foreach (var block in EncryptDecryptTextAesBase64.EncryptFileBlocks(arguments.FileName,
                            arguments.Password, arguments.BlockSize))
                        {
                            // display the result to the user..
                            WriteBlockData(blockNum++, block, splitString);
                        }
                    }

                    // encrypt a redirected file data..
                    if (Console.IsInputRedirected)
                    {
                        using var memoryStream = new MemoryStream();
                        int bytes;

                        // read the file contents from the standard input..
                        using var inputStream = Console.OpenStandardInput();

                        byte[] buffer = new byte[1000];

                        // ..and write the data read to a memory stream..
                        while ((bytes = inputStream.Read(buffer, 0, 1000)) > 0)
                        {
                            memoryStream.Write(buffer, 0, bytes);
                        }

                        var blockNum = 0;

                        foreach (var block in EncryptDecryptTextAesBase64.EncryptFileBlocks(memoryStream.ToArray(),
                            arguments.Password, arguments.BlockSize))
                        {
                            // display the result to the user..
                            WriteBlockData(blockNum++, block, splitString);
                        }
                    }

                    return 0;
                }

                // validate the arguments for a file generation from encrypted environment variables..
                if ((arguments.FileName != null || Console.IsOutputRedirected) &&
                    arguments.EnvironmentVariables != null)
                {
                    var variables = arguments.EnvironmentVariables.Split(';');
                    var byteData = EncryptDecryptTextAesBase64.DecryptBase64Blocks(arguments.Password, variables);

                    // if a file is specified, write the data to the file..
                    if (arguments.FileName != null) 
                    {
                        File.WriteAllBytes(arguments.FileName, byteData);
                    }

                    // if the standard output is redirected, write the data the standard output..-
                    if (Console.IsOutputRedirected)
                    {
                        using var stream = Console.OpenStandardOutput();
                        stream.Write(byteData, 0, byteData.Length);
                    }

                    return 0;
                }

                // the help is required at this point..
                ShowHelp();

                return 0;
            }
            catch (Exception ex) // inform the user of an error..
            {
                Console.WriteLine($"An error occurred with the software execution: '{ex.Message}'. See --help for help.");
                return -1;
            }
        }

        /// <summary>
        /// Displays the command line arguments for the software.
        /// </summary>
        private static void ShowHelp()
        {
            Parser.Default.ParseArguments<Arguments>(new[] {"--help"});
        }
    }
}
