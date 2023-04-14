using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

// gui version coming out soon.

namespace Shredder {

    internal class Program {

        private static void Main(string[] args) {
            // Show usage if no arguments are passed
            if (args.Length == 0) {
                Console.WriteLine("Usage: Shredder.exe -f <file location> -i <number of iterations>\n" +
                                  "The more times you shred the file, the harder it will be to recover.\n" +
                                  "Default number of iterations is 3.");
                Console.ReadKey();
                return;
            }

            // Check for valid number of arguments
            if (args.Length != 4) {
                Console.WriteLine("Invalid command-line arguments.");
                return;
            }

            // Parse arguments
            string filePath = args[1];
            int iterations = 0;
            bool isValidIterations = int.TryParse(args[3], out iterations);

            // Check for valid number of iterations
            if (!isValidIterations || iterations < 1 || iterations > 25) {
                Console.WriteLine("Invalid number of iterations.");
                return;
            }

            // Check if file exists
            if (!File.Exists(filePath)) {
                Console.WriteLine("File not found.");
                return;
            }

            // Define list of allowed file extensions
            string[] allowedExtensions = { ".txt", ".docx", ".pdf", ".xlsx", ".doc", ".pptx", ".ppt", ".xls", ".csv", ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".mp3", ".wav", ".mp4", ".avi", ".mov", ".exe" };

            // Check if file has a valid extension
            string fileExtension = Path.GetExtension(filePath).ToLower();
            if (!allowedExtensions.Contains(fileExtension)) {
                Console.WriteLine("Invalid file extension.");
                return;
            }

            // Check if file has a valid size
            long fileSize = new FileInfo(filePath).Length;
            if (fileSize < 1 || fileSize > 100000000) {
                Console.WriteLine("Invalid file size.");
                return;
            }

            // Check if file is not read-only
            if ((File.GetAttributes(filePath) & FileAttributes.ReadOnly) == FileAttributes.ReadOnly) {
                Console.WriteLine("You do not have permission to shred this file.");
                return;
            }

            // Check if file path is valid
            if (Path.GetInvalidFileNameChars().Any(c => filePath.Contains(c))) {
                Console.WriteLine("Invalid file path.");
                return;
            }

            // Define header for the shredded file
            string header = "+---------------------------------------------+\n" +
                            "|            Shredded by The Shredder         |\n" +
                            "|                made by zorky                |\n" +
                            "+---------------------------------------------+\n\n";

            // Use a using statement to ensure that the file stream is properly closed and disposed.
            using (FileStream stream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write)) {
                byte[] headerBytes = System.Text.Encoding.UTF8.GetBytes(header);
                stream.Write(headerBytes, 0, headerBytes.Length);

                long headerLength = headerBytes.Length;
                int numThreads = Environment.ProcessorCount; // use the number of available logical processors
                int iterationsPerThread = iterations / numThreads;

                List<Thread> threads = new List<Thread>();
                for (int i = 0; i < numThreads; i++) {
                    int startIndex = i * iterationsPerThread;
                    int endIndex = (i == numThreads - 1) ? iterations : (i + 1) * iterationsPerThread;

                    Thread thread = new Thread(() =>
                    {
                        for (int j = startIndex; j < endIndex; j++) {
                            byte[] data = new byte[stream.Length - headerLength];
                            new Random().NextBytes(data);
                            stream.Write(data, 0, data.Length);
                            Console.WriteLine($"Iteration {j}: Overwritten bytes {startIndex} to {endIndex}");
                            headerLength = 0;
                        }
                    });

                    thread.Start();
                    threads.Add(thread);
                }

                foreach (Thread thread in threads) {
                    thread.Join();
                }

                stream.Seek(0, SeekOrigin.Begin);
                stream.Write(headerBytes, 0, headerBytes.Length);
            }

            Console.WriteLine("---------------------------------------------------");
            Console.WriteLine("File Overwritten successfully.");
            Console.WriteLine("File shredded successfully.");
            Console.WriteLine("Do you want to delete the file completely? (Y/N)");
            Console.Write("# ");
            string response = Console.ReadLine();
            if (response.Equals("Y", StringComparison.OrdinalIgnoreCase)) {
                File.Delete(filePath);
                Console.WriteLine("File deleted.");
            }

        }
    }
}