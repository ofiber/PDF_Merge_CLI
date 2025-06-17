//  ██████╗ ██████╗ ███████╗    ███╗   ███╗███████╗██████╗  ██████╗ ███████╗██████╗ 
//  ██╔══██╗██╔══██╗██╔════╝    ████╗ ████║██╔════╝██╔══██╗██╔════╝ ██╔════╝██╔══██╗
//  ██████╔╝██║  ██║█████╗      ██╔████╔██║█████╗  ██████╔╝██║  ███╗█████╗  ██████╔╝
//  ██╔═══╝ ██║  ██║██╔══╝      ██║╚██╔╝██║██╔══╝  ██╔══██╗██║   ██║██╔══╝  ██╔══██╗
//  ██║     ██████╔╝██║         ██║ ╚═╝ ██║███████╗██║  ██║╚██████╔╝███████╗██║  ██║
//  ╚═╝     ╚═════╝ ╚═╝         ╚═╝     ╚═╝╚══════╝╚═╝  ╚═╝ ╚═════╝ ╚══════╝╚═╝  ╚═╝

namespace PDF_Merge_CLI
{
    using System;
    using System.Collections.Generic;
    using System.Numerics;
    using System.Reflection.Metadata.Ecma335;
    using PdfSharp.Pdf;
    using PdfSharp.Pdf.IO;
    using Spectre.Console;

    public class PDF_Merge
    {
        public static void Main()
        {
            Menu();

            string folderPath = GetFolderPath();

            string outputFileName = GetMergedFileName();

            string[] files = System.IO.Directory.GetFiles(folderPath, "*.pdf");

            if (files.Length == 0)
            {
                Console.WriteLine("No PDF files found in the specified folder.");
                return;
            }

            MergePDFs(files, outputFileName, folderPath);

            Console.WriteLine("\n\nPress any key to exit...");
            Console.ReadKey();

            //Environment.Exit(0);
        }

        private static void Menu()
        {
            WriteAscii(true);

            var selection = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                .Title("Please select an option:")
                .PageSize(3)
                .AddChoices(
                    new[]
                    {
                        "Merge PDF Files",
                        "Exit"
                    }));

            if(selection == "Exit")
            {
                Environment.Exit(0);
            }
                
            //Console.WriteLine("=======================================");
            //Console.WriteLine("This tool merges all PDF files in a specified folder into a single PDF file.");
            //Console.WriteLine("You will be prompted to enter the folder path and the desired name for the merged file.");
            //Console.WriteLine("=======================================\n");
            //Console.WriteLine("Press any key to continue...");
            //Console.ReadKey();
        }

        private static void WriteAscii(bool clear)
        {
            if (clear)
                Console.Clear();

            AnsiConsole.MarkupLine("[blue]\r\n\r\n██████╗ ██████╗ ███████╗    ███╗   ███╗███████╗██████╗  ██████╗ ███████╗██████╗ " +
                                             "\r\n██╔══██╗██╔══██╗██╔════╝    ████╗ ████║██╔════╝██╔══██╗██╔════╝ ██╔════╝██╔══██╗" +
                                             "\r\n██████╔╝██║  ██║█████╗      ██╔████╔██║█████╗  ██████╔╝██║  ███╗█████╗  ██████╔╝" +
                                             "\r\n██╔═══╝ ██║  ██║██╔══╝      ██║╚██╔╝██║██╔══╝  ██╔══██╗██║   ██║██╔══╝  ██╔══██╗" +
                                             "\r\n██║     ██████╔╝██║         ██║ ╚═╝ ██║███████╗██║  ██║╚██████╔╝███████╗██║  ██║" +
                                             "\r\n╚═╝     ╚═════╝ ╚═╝         ╚═╝     ╚═╝╚══════╝╚═╝  ╚═╝ ╚═════╝ ╚══════╝╚═╝  ╚═╝" +
                                             "\r\n                                                                                " +
                                             "\r\n\r\n[/]");
        }

        private static void MergePDFs(string[] files, string outputFileName, string folderPath)
        {
            // Display loading screen
            ShowLoadingScreen();

            Array.Sort(files, StringComparer.InvariantCultureIgnoreCase);
            PdfDocument outputDocument = new PdfDocument();
            List<BigInteger> fileSizes = new List<BigInteger>();
            long totalSize = 0;
            int fileCount = 0;

            foreach (var file in files)
            {
                try
                {
                    using (PdfDocument inputDocument = PdfReader.Open(file, PdfDocumentOpenMode.Import))
                    {
                        int pageCount = inputDocument.PageCount;

                        for (int idx = 0; idx < pageCount; idx++)
                        {
                            outputDocument.AddPage(inputDocument.Pages[idx]);
                        }

                        var fileInfo = new System.IO.FileInfo(file);
                        fileSizes.Add((BigInteger)fileInfo.Length);
                        totalSize += fileInfo.Length;
                        fileCount++;

                        //Console.WriteLine($"Merged: {file.Substring(file.LastIndexOf("\\") + 1)} ({FormatBytes(fileInfo.Length)})");
                    }
                }
                catch (Exception ex)
                {
                    AnsiConsole.MarkupLine($"[red]Error processing file[/] {file}: {ex.Message}");
                }
            }

            outputDocument.Save(System.IO.Path.Combine(folderPath, outputFileName));

            AnsiConsole.MarkupLine($"\n[green]Successfully merged {fileCount} files into {outputFileName}[/]");
            AnsiConsole.MarkupLine($"[blue]Total size of merged files: {FormatBytes(totalSize)}[/]");
        }

        private static string FormatBytes(BigInteger bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double len = (double)bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len /= 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }

        private static string GetFolderPath()
        {
            var folderPath = "";

            do
            {
                folderPath = AnsiConsole.Prompt(
                    new TextPrompt<string>("Enter the [blue]path[/] to the folder containing the PDFs to merge:")
                        .Validate(path =>
                        {
                            return string.IsNullOrWhiteSpace(path) || !System.IO.Directory.Exists(path)
                                ? ValidationResult.Error("[red]The folder path is invalid or does not exist.[/]")
                                : ValidationResult.Success();
                        }));

            } while (string.IsNullOrWhiteSpace(folderPath) || !System.IO.Directory.Exists(folderPath));

            return folderPath;
        }

        private static string GetMergedFileName()
        {
            var outputFileName = AnsiConsole.Prompt(
                new TextPrompt<string>("[blue]Name[/] the merged file ([underline]without[/] extension):"));

            //Console.Write("\n\nEnter a name for the merged PDF file (without extension, leave blank for default):\n-> ");
            //string? outputFileName = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(outputFileName))
            {
                outputFileName = "MergedPDF_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".pdf";
            }
            else
            {
                outputFileName += ".pdf";
            }

            return outputFileName;
        }

        private static void ShowLoadingScreen()
        { 
            WriteAscii(true);

            AnsiConsole.Progress()
                .Columns(new ProgressColumn[]
                {
                    new TaskDescriptionColumn(),    // Task description
                    new ProgressBarColumn(),        // Progress bar
                    new PercentageColumn(),         // Percentage
                    new SpinnerColumn(),            // Spinner
                })
                .Start(ctx =>
                {
                    var task = ctx.AddTask("[blue]Merging PDFs...[/]");

                    while (!task.IsFinished)
                    {
                        task.Increment(0.5); // Simulate progress over 3 seconds
                        Thread.Sleep(15); // Wait for 1 second
                    }
                });
        }
    }
}