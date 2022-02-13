/* **********************************************************************
 Copyright 2022 Christof Dittmar
 
 Licensed under the Apache License, Version 2.0 (the "License");
 you may not use this file except in compliance with the License.
 You may obtain a copy of the License at
 
     http://www.apache.org/licenses/LICENSE-2.0
 
 Unless required by applicable law or agreed to in writing, software
 distributed under the License is distributed on an "AS IS" BASIS,
 WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 See the License for the specific language governing permissions and
 limitations under the License.
********************************************************************** */

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.IO;
using ExecutionExceptions;
using Execution;
using SeismicStandards;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using BusgsReflect;
using ExcelIO;
using BaseIO;
using BatchRequest;
using UsgsResponseCompile;
using SimpleTable;
using WriteSamples;
using DIServices;

namespace Busgs
{
    class Program
    {
        // TODO: add a revision dictionary (or simple record)
        // TODO: list all failed responses, give user option to show them or do so automatically - write into excel file
        //      add the http header status code and error message, sheet 'Fails'
        //      add option -t retry all failed requests

        private const string usage =
@"Usage: busgs [-h] [-v] [-f] [-b BASE_URL] [-r INPUT_FILE OUTPUT_FILE | -s FILE | -l | -t]";

        private const string cmdLineHelp =
@"Usage: busgs [-h] [-v] [-f] [-b BASE_URL] [-r INPUT_FILE OUTPUT_FILE | -s FILE | -l | -t]

Batch request of seismic parameters from USGS server.

Optional Arguments:
  -h, --help            Show this help message and exit
  -v, --version         Print program version
  -f, --force-overwrite
                        Force overwriting of existing files
  -r INPUT_FILE OUTPUT_FILE, --request INPUT_FILE OUTPUT_FILE
                        Read parameters of input file, request seismic parameters 
                        from server and write to output file.
  -s FILE, --sample FILE
                        Write a sample input file (type auto-detected) to 
                        specified file
  -b, --base-url
                        Uses an alternative base URL instead of the default. 
                        List default by -l command. Default URL are the part
                        before 'asce...', 'ibc...' (reference document). 
                        A base URL must end by /, like 
                        http://earthquake.usgs.gov/ws/designmaps/
  -l, --list-refdocs    List all supported reference documents (seismic standards)
  -t, --input-filetypes
                        List all supported input file types

The tool will read a list of parameters from an input file, request the seismic 
parameters from the USGS server and writes them to an Excel file. Input file 
formats will be detected automatically by their extension. 
For more information on seismic design maps visit:
https://earthquake.usgs.gov/ws/designmaps/";

        private const string supportedInputFileTypes =
@"Extension  Description
----------------------------------------
.xlsx      Microsoft Excel File
.json      JavaScript Object Notation File";

        private static string inputFile = "";
        private static string outputFile = "";
        private static bool isForceOverwrite = false;
        private static bool isPrintVersion = false;
        private static bool isPrintHelp = false;
        private static bool isListInputFileTypes = false;
        private static bool isListSupportedRefDocs = false;
        private static string sampleInputFile = "";
        private static string baseUrl = SeismicStandards.UsaStandards.BaseUrl;

        static void ListSupportedRefDocs()
        {
            const int widthCol1 = -10;
            Console.WriteLine($"{new String("RefDoc"),widthCol1} {new String("URL")}");
            Console.WriteLine("----------------------------------------");
            foreach (var std in UsaStandards.standards)
                Console.WriteLine($"{std.Name,widthCol1} {std.GetUrlDoc(UsaStandards.BaseUrl)}");
        }

        static void ParseCommandLineArgs(string[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                string argument = args[i];
                switch (argument.ToLower())
                {
                    case "--help":
                    case "-h":
                        isPrintHelp = true;
                        break;
                    case "--version":
                    case "-v":
                        isPrintVersion = true;
                        break;
                    case "--input-filetypes":
                    case "-t":
                        isListInputFileTypes = true;
                        break;
                    case "--list-refdocs":
                    case "-l":
                        isListSupportedRefDocs = true;
                        break;
                    case "--force-overwrite":
                    case "-f":
                        isForceOverwrite = true;
                        break;
                    case "--request":
                    case "-r":
                        try
                        {
                            inputFile = args[i + 1];
                            outputFile = args[i + 2];
                        }
                        catch (IndexOutOfRangeException)
                        {
                            throw new CommandLineException("Too few file names for [-r, --request]");
                        }
                        i += 2;
                        break;
                    case "--sample":
                    case "-s":
                        try
                        {
                            sampleInputFile = args[i + 1];
                        }
                        catch (IndexOutOfRangeException)
                        {
                            throw new CommandLineException("Missing output file name for [-s, --sample]");
                        }
                        i++;
                        break;
                    case "--base-url":
                    case "-b":
                        baseUrl = args[i + 1];
                        i++;
                        break;
                    default:
                        throw new CommandLineException($"Unsupported argument \"{argument}\"");
                }
            }
        }

        static void validateOutputFile(string outputFile)
        {
            if (File.Exists(outputFile) && !isForceOverwrite)
            {
                throw new FileOverwriteException(outputFile);
            }
        }

        static async Task RequestFromServer()
        {
            string _inputFile = Path.GetFullPath(inputFile);
            string _outputFile = Path.GetFullPath(outputFile);
            if (File.Equals(_inputFile, _outputFile))
                throw new ArgumentException("Input file and output file is the same.");
            if (!File.Exists(_inputFile))
                throw new FileNotFoundException(inputFile);
            validateOutputFile(outputFile);

            Console.WriteLine($"Reading {_inputFile}");
            var requestParametersInput = 
                ExcelReader.Read(_inputFile, "Batch", new TableHeader(1, 0));
            var requestParameters = 
                requestParametersInput.Select(p => p.ToRequestParameters()).ToList();
            Console.WriteLine(
                $"Requesting {requestParameters.Count} parameter sets "
                + $"from {baseUrl}"
            );

            ProgressBar progressBar = new ProgressBar(20);

            // TODO: add a BatchClientBuilderService that provides the client
            // or BatchClientService that encapsulates BatchClient
            
            // explicit dependency injection, get the service from the host
            var httpClientFactory = 
                ServiceBuilder.GetHost().Services.GetRequiredService<IHttpClientFactory>();
            var serverRequest = new BatchClient(httpClientFactory)
            {
                UpdateProgressCallback = progressBar.Update,
                FinishProgressCallback = progressBar.Finish
            };
            Task<BatchResponse> requesting = 
                serverRequest.RequestAsync(requestParameters, baseUrl);
            var batchResponse = await requesting;
            ImmutableList<string> responses = batchResponse.SuccessMessages;

            ServerResponseCompiler responseCompiler = new ServerResponseCompiler();
            var compiledResponse = responseCompiler.Compile(responses);
            var responseTables = 
                new TableBuilderServerResponse().CreateGroupedByRefDoc(compiledResponse);
            ExcelWriter.Write(outputFile, responseTables);
            
            if (batchResponse.FailedResponses.Count > 0)
                Console.WriteLine(
                    $"WARNING: {batchResponse.FailedResponses.Count} request(s) failed!"
                );
            Console.WriteLine($"Wrote server response to \"{outputFile}\"");
        }

        static void WriteSampleInputFile()
        {
            validateOutputFile(sampleInputFile);
            string extension = Path.GetExtension(sampleInputFile).ToLower();
            if (extension == ".xlsx")
            {
                Excel.WriteSampleRequestParameters(sampleInputFile);
            }
            else if (extension == ".json")
            {
                // TODO: implement output sample json file
                throw new NotImplementedException("Writing sample JSON input file");
            }
            else
                throw new ArgumentException($"Unsupported file type \"{Path.GetExtension(sampleInputFile)}\"");
            Console.WriteLine($"Wrote sample input file to \"{sampleInputFile}\"");
        }

        static async Task RunCommands(string[] args)
        {
            if (isPrintHelp)
            {
                Console.WriteLine(cmdLineHelp);
                Environment.Exit(0);
            }
            if (isPrintVersion)
            {
                Console.WriteLine(BusgsAssembly.GetVersion());
                Environment.Exit(0);
            }
            if (isListSupportedRefDocs)
            {
                ListSupportedRefDocs();
                Environment.Exit(0);
            }
            if (isListInputFileTypes)
            {
                Console.WriteLine(supportedInputFileTypes);
                Environment.Exit(0);
            }
            if (sampleInputFile != "")
            {
                WriteSampleInputFile();
                Environment.Exit(0);
            }
            if (inputFile != "" && outputFile != "")
            {
                await RequestFromServer();
                Environment.Exit(0);
            }
            throw new CommandLineException($"Invalid arguments {String.Join(", ", args)}");
        }

        static void PrintExceptionTrace(Exception exception, string linePrefix = "   ")
        {
            Console.WriteLine(linePrefix + $"{exception.Message}");
            if (exception.InnerException != null)
                PrintExceptionTrace(exception.InnerException, linePrefix + "   ");
        }

        static async Task Main(string[] args)
        {
            if (args.Length == 0)
                Console.WriteLine(cmdLineHelp);

            try
            {
                ParseCommandLineArgs(args);
                await RunCommands(args);
            }
            catch (CommandLineException ex)
            {
                Console.WriteLine("Command line error:");
                Console.WriteLine($"    {ex.Message}");
                Console.WriteLine(usage);
            }
            catch (FileOverwriteException ex)
            {
                Console.WriteLine("Detected existing file: use [-f, --force-overwrite] to overwrite");
                PrintExceptionTrace(ex);
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine("Invalid value(s) or parameter(s):");
                PrintExceptionTrace(ex);
            }
            catch (FileNotFoundException ex)
            {
                Console.WriteLine("File does not exist:");
                PrintExceptionTrace(ex);
            }
            catch (ServerRequestError ex)
            {
                Console.WriteLine("Connection error:");
                PrintExceptionTrace(ex);
            }
            catch (NotImplementedException ex)
            {
                Console.WriteLine("Feature not yet implemented :-(");
                PrintExceptionTrace(ex);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unclassified error:");
                PrintExceptionTrace(ex);
            }
        }
    }
}
