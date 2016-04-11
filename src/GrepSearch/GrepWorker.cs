using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace GrepSearch
{
    public class GrepWorker
    {
        private class WorkerData
        {
            public string Directory { get; set; }
            public string FilePattern { get; set; }
            public string[] Extentions { get; set; }
            public string RegEx { get; set; }
            public bool IsRecursive { get; set; }
            public bool IgnoreCase { get; set; }
            public bool FileNameOnly { get; set; }
        }

        private BackgroundWorker _bgw;
        private ManualResetEventSlim _resetEvent = new ManualResetEventSlim(false);

        public event EventHandler<GrepFileChangeEventArgs> GrepFileChanged;
        public event EventHandler<GrepFileChangeEventArgs> GrepFileHasMatch;
        public event EventHandler<GrepResultEventArgs> GrepResult;
        public event EventHandler<GrepFinishedFileEventArgs> GrepFinishedFile;
        public event EventHandler<UnhandledExceptionEventArgs> GrepException;
        public event EventHandler GrepCanceled;
        public event EventHandler GrepComplete;

        private void OnGrepFileChanged(object sender, string filePath)
        {
            var grepFileChanged = GrepFileChanged;
            if (grepFileChanged != null)
                grepFileChanged(sender, new GrepFileChangeEventArgs(filePath));
        }

        private void OnGrepFileHasMatch(object sender, string filePath)
        {
            var grepFileHasMatch = GrepFileHasMatch;
            if (grepFileHasMatch != null)
                grepFileHasMatch(sender, new GrepFileChangeEventArgs(filePath));
        }

        private void OnGrepResult(object sender, string filePath, int lineNumber, string line)
        {
            var grepResult = GrepResult;
            if (grepResult != null)
                grepResult(sender, new GrepResultEventArgs(filePath, lineNumber, line));
        }

        private void OnGrepFinishedFile(object sender, string filePath, int matchingLineCount)
        {
            var grepFinishedFile = GrepFinishedFile;
            if (grepFinishedFile != null)
                grepFinishedFile(sender, new GrepFinishedFileEventArgs(filePath, matchingLineCount));
        }

        private void OnGrepException(object sender, Exception err, bool isTerminating)
        {
            var grepException = GrepException;
            if (grepException != null)
                grepException(sender, new UnhandledExceptionEventArgs(err, isTerminating));
        }

        private void OnGrepCanceled(object sender)
        {
            var grepCanceled = GrepCanceled;
            if (grepCanceled != null)
                grepCanceled(sender, EventArgs.Empty);
        }

        private void OnGrepComplete(object sender)
        {
            var grepComplete = GrepComplete;
            if (grepComplete != null)
                grepComplete(sender, EventArgs.Empty);
        }

        public void Start(string directory, string filePattern, string[] extentions, string regex, bool isRecursive, bool ignoreCase, bool fileNameOnly)
        {
            if (string.IsNullOrEmpty(directory))
                throw new ArgumentNullException("directory");
            filePattern = string.IsNullOrEmpty(filePattern) ? "*.*" : filePattern;
            extentions = !(extentions ?? new string[] { }).Any() ? new[] { "*.*" } : extentions;
            if (string.IsNullOrEmpty(regex))
                throw new ArgumentNullException("regex");

            var workerData = new WorkerData { Directory = directory, FilePattern = filePattern, Extentions = extentions, RegEx = regex, IsRecursive = isRecursive, IgnoreCase = ignoreCase, FileNameOnly = fileNameOnly };
            Task.Run(() =>
            {
                if (_bgw != null)
                {
                    _bgw.CancelAsync();
                    _resetEvent.Wait();
                }
            }).ContinueWith(x =>
            {
                _bgw = new BackgroundWorker { WorkerSupportsCancellation = true };
                _bgw.DoWork += BackgroundWorker_DoWork;
                _bgw.RunWorkerCompleted += BackgroundWorker_RunWorkerCompleted;
                _bgw.RunWorkerAsync(workerData);
            });
        }

        void BackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            if (_resetEvent.IsSet)
                _resetEvent.Reset();
            var bgw = sender as BackgroundWorker;
            var workerData = e.Argument as WorkerData;
            var searchOption = workerData.IsRecursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            var filePaths = Directory.EnumerateFiles(workerData.Directory, workerData.FilePattern, searchOption);
            try
            {
                foreach (var filePath in filePaths)
                {
                    if (bgw.CancellationPending)
                    {
                        e.Cancel = true;
                        break;
                    }
                    OnGrepFileChanged(sender, filePath);
                    if (!workerData.Extentions.Contains("*.*") && !workerData.Extentions.Contains("*" + Path.GetExtension(filePath)))
                        continue;
                    var matchcount = 0;
                    using (var sr = File.OpenText(filePath))
                    {
                        var linenum = 0;
                        var line = string.Empty;
                        var regexoptions = workerData.IgnoreCase ? RegexOptions.IgnoreCase : RegexOptions.None;
                        while ((line = sr.ReadLine()) != null)
                        {
                            if (bgw.CancellationPending)
                            {
                                e.Cancel = true;
                                break;
                            }
                            linenum++;
                            var match = Regex.Match(line, workerData.RegEx, regexoptions);
                            if (!match.Success)
                                continue;
                            matchcount++;
                            if (matchcount == 1)
                                OnGrepFileHasMatch(sender, filePath);
                            if (workerData.FileNameOnly)
                                break;
                            OnGrepResult(sender, filePath, linenum, Regex.Replace(line, @"[^\u0020-\u007E]", string.Empty));
                        }
                    }
                    OnGrepFinishedFile(sender, filePath, matchcount);
                }
            }
            //catch (SecurityException){}catch (FileNotFoundException){}
            catch (Exception err)
            {
                OnGrepException(sender, err, false);
            }
        }

        void BackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                OnGrepException(sender, e.Error, true);
            }
            else if (e.Cancelled)
            {
                OnGrepCanceled(sender);
            }
            else
            {
                OnGrepComplete(sender);
            }
            _resetEvent.Set();
        }
    }
}
