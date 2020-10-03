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
            /// Gets or set the width of the console.
            /// </summary>
            [Option('w', "width", Required = false, HelpText = "The width of the console.")]
            public int ConsoleWidth { get; set; }

            /// <summary>
            /// Gets or sets the environment variables as a semicolon (';') delimited list.
            /// </summary>
            [Option('e', "environment", Required = false, HelpText = "Environment variables to decrypt. This is a semicolon (';') delimited list.")]
            public string EnvironmentVariables { get; set; }

            /// <summary>
            /// Gets or sets a value whether to ignore input/output redirection.
            /// </summary>
            [Option('i', "--ignoreRedirect", Required = false, HelpText = "A flag indicating whether to ignore console input/output redirection in case of a CI/CD environment.")]
            public bool IgnoreRedirect { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether display as much output as possible.
            /// </summary>
            [Option('v', "--verbose", Required = false, HelpText = "A flag indicating whether display as much output as possible.")]
            public bool Verbose { get; set; }
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
                SafeAction(() =>
                {
                    Console.WriteLine($"Block ({blockNumber:D4})");
                    Console.WriteLine(split);
                    Console.WriteLine(blockData);
                    Console.WriteLine(split);
                    Console.WriteLine();
                }, "Error occurred during the console output: '{0}'.");
            }

            try // we try so the possible error can be reported..
            {
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

                if (arguments.ConsoleWidth != 0)
                {
                    splitString = new string('-', arguments.ConsoleWidth);
                }
                else
                {
                    SafeAction(() => splitString = new string('-', Console.WindowWidth),
                        "Failed to read console width with exception: '{0}'.");
                }

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
                    SafeAction(() =>
                    {
                        if (Console.IsInputRedirected && !arguments.IgnoreRedirect)
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
                    }, "Failed to read from redirected input with exception: '{0}'.");

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

                        SafeAction(() =>
                        {
                            if (arguments.Verbose && (!Console.IsOutputRedirected || arguments.IgnoreRedirect))
                            {
                                Console.WriteLine($"The file was successfully created: '{arguments.FileName}' / '{Path.GetFullPath(arguments.FileName)}'.");
                                var info = new FileInfo(Path.GetFullPath(arguments.FileName));
                                Console.WriteLine($"File size: {info.Length}.");
                            }
                        }, "An exception occurred during data output: '{0}'.");
                    }

                    // if the standard output is redirected, write the data the standard output..-

                    SafeAction(() =>
                    {
                        if (Console.IsOutputRedirected && !arguments.IgnoreRedirect)
                        {
                            using var stream = Console.OpenStandardOutput();
                            stream.Write(byteData, 0, byteData.Length);
                        }
                    }, "Failed to write to redirected output with exception: '{0}'.");

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
        /// Executes a specified action and in case of an exception tries to report the exception to the <see cref="Console"/> standard output.
        /// </summary>
        /// <param name="action">The action to execute.</param>
        /// <param name="exceptionMessage">The message to display in case of an exception. This must have the {0} parameter defined to display the exception message.</param>
        private static void SafeAction(Action action, string exceptionMessage)
        {
            try
            {
                action.Invoke();
            }
            catch (Exception ex)
            {
                try
                {
                    if (exceptionMessage.Contains("{0}"))
                    {
                        Console.WriteLine(exceptionMessage, ex.Message);
                    }
                    else
                    {
                        Console.WriteLine(exceptionMessage);
                    }
                }
                catch
                {
                    // the console output is giving an error..
                }
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
