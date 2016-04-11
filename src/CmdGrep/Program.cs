using CommandLine.Utility;
using GrepSearch;
using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;

namespace CmdGrep
{
    class Program
    {
        private static readonly GrepWorker _grepWorker = new GrepWorker();

        private static object _updatelock = new object();

        private static bool _fileNamesOnly;
        private static bool _dispLineNum;
        private static bool _countLines;
        private static int _totalFileMatches;
        private static int _totalLineMatches;

        static void Main(string[] args)
        {
            var cmdline = new Arguments(args);
            if (cmdline["h"] != null || cmdline["H"] != null)
            {
                PrintHelp();
                return;
            }

            if (cmdline["F"] == null)
            {
                Console.WriteLine("Error: files NOT provided");
                Console.WriteLine();
                PrintHelp();
                return;
            }
            var filepatterns = ((string)cmdline["F"]).Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            if (cmdline["E"] == null)
            {
                Console.WriteLine("Error: regex pattern NOT provided");
                Console.WriteLine();
                PrintHelp();
                return;
            }
            var regex = (string)cmdline["E"];

            _totalFileMatches = 0;
            _totalLineMatches = 0;

            var isRecursive = cmdline["r"] != null;
            var ignoreCase = cmdline["i"] != null;
            _fileNamesOnly = cmdline["l"] != null;
            _dispLineNum = _fileNamesOnly ? false : cmdline["n"] != null;
            _countLines = _fileNamesOnly ? false : cmdline["c"] != null;

            _grepWorker.GrepFileChanged += GrepFileChanged;
            _grepWorker.GrepFileHasMatch += GrepFileHasMatch;
            _grepWorker.GrepResult += GrepResult;
            _grepWorker.GrepFinishedFile += GrepFinishedFile;
            _grepWorker.GrepException += GrepException;
            _grepWorker.GrepCanceled += GrepCanceled;
            _grepWorker.GrepComplete += GrepComplete;

            foreach (var filepattern in filepatterns)
            {
                var directory = Path.GetDirectoryName(filepattern);
                if (string.IsNullOrEmpty(directory))
                    directory = Environment.CurrentDirectory;
                if (!Directory.Exists(directory))
                {
                    Console.WriteLine("Unable to find directory: {0}", directory);
                    continue;
                }
                var filePattern = Path.GetFileName(filepattern);
                var extensions = Regex.IsMatch((filePattern ?? string.Empty), @"^\*\.(?!.*\..*).+$") ? new[] { filePattern } : null;
                _grepWorker.Start(directory, filePattern, extensions, regex, isRecursive, ignoreCase, _fileNamesOnly);
            }
        }

        private static void PrintHelp()
        {
            Console.WriteLine("grep stands for \"Global Regular Expression and Print\".");
            Console.WriteLine("Global: entire file is searched.");
            Console.WriteLine("Regular Expression: regex is used to match a search pattern.");
            Console.WriteLine("Print: textually display matching files and lines.");
            Console.WriteLine("grep searches an entire file for the specified pattern and displays the matching lines.");
            Console.WriteLine();
            Console.WriteLine("This syntax is different from the traditional Unix, it has been implemented similar to csc, the C# compiler.");
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("Syntax:");
            Console.WriteLine("grep [/h|/H]");
            Console.WriteLine("grep [/c] [/i] [/l] [/n] [/r] /E:reg_exp /F:files");
            Console.WriteLine();
            Console.WriteLine("Switches:");
            Console.WriteLine("/h or /H - display this help message.");
            Console.WriteLine("/c - print a count of matching lines for each input file");
            Console.WriteLine("/i - ignore case in pattern");
            Console.WriteLine("/l - print just file names");
            Console.WriteLine("/n - prefix each line of output with line number");
            Console.WriteLine("/r - recursively search subdirectories");
            Console.WriteLine();
            Console.WriteLine("Input:");
            Console.WriteLine("/E:regex - a Regular Expression search pattern.");
            Console.WriteLine("The Regular Expression can be delimited by quotes like \"...\" and \'...\' if blanks are to be included");
            Console.WriteLine();
            Console.WriteLine("/F:files - a list of input files.");
            Console.WriteLine("The files can be separated by commas i.e. /F:file1,file2,file3.");
            Console.WriteLine("File system pattern matching wildcards can be used as well i.e. /F:*file?.txt");
            Console.WriteLine();
            Console.WriteLine("Example:");
            Console.WriteLine("grep /c /n /r /E:\" C Sharp \" /F:*.cs");
            Console.ReadKey();
        }

        #region Grep Search Events
        static void GrepFileChanged(object sender, GrepFileChangeEventArgs e)
        {
            lock (_updatelock)
            {
                Debug.WriteLine(string.Format("Scanning File: {0}", e.FilePath));
            }
        }

        static void GrepFileHasMatch(object sender, GrepFileChangeEventArgs e)
        {
            lock (_updatelock)
            {
                _totalFileMatches++;
                var line = string.Format("{0}{1}\r\n", e.FilePath, _fileNamesOnly ? null : ":");
                Console.WriteLine(line);
            }
        }

        static void GrepResult(object sender, GrepResultEventArgs e)
        {
            lock (_updatelock)
            {
                if (_fileNamesOnly)
                    return;
                var line = string.Format("  {0}{1}\r\n", !_dispLineNum ? null : string.Format("{0}: ", e.LineNumber), e.Line);
                Console.WriteLine(line);
            }
        }

        static void GrepFinishedFile(object sender, GrepFinishedFileEventArgs e)
        {
            lock (_updatelock)
            {
                var line = string.Empty;
                if (e.MatchingLineCount > 0)
                {
                    if (_countLines)
                        line += string.Format("  {0} Matching Line(s)", e.MatchingLineCount);
                    _totalLineMatches += e.MatchingLineCount;
                }
                if (!string.IsNullOrEmpty(line))
                    Console.WriteLine(line);
            }
        }

        static void GrepException(object sender, UnhandledExceptionEventArgs e)
        {
            lock (_updatelock)
            {
                var err = (Exception)e.ExceptionObject;
                var line = string.Format("{0}{1}", e.IsTerminating ? "Fatal Error: " : null, err.Message);
                Console.WriteLine(line);
            }
        }

        static void GrepCanceled(object sender, EventArgs e)
        {
            lock (_updatelock)
            {
                _totalFileMatches = 0;
                _totalLineMatches = 0;
                var line = string.Format("Win Grep Canceled.");
                Console.WriteLine(line);
            }
        }

        static void GrepComplete(object sender, EventArgs e)
        {
            lock (_updatelock)
            {
                var line = string.Format("Win Grep Complete. {0} Total Matching File(s){1}", _totalFileMatches, !_countLines ? null : string.Format(", {0} Total Matching Line(s)", _totalLineMatches));
                Console.WriteLine(line);
            }
        }
        #endregion Grep Search Events
    }
}
