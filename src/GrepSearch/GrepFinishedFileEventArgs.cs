using System;

namespace GrepSearch
{
    public class GrepFinishedFileEventArgs : EventArgs
    {
        public string FilePath { get; private set; }
        public int MatchingLineCount { get; set; }

        public GrepFinishedFileEventArgs(string filePath, int matchingLineCount)
        {
            FilePath = filePath;
            MatchingLineCount = matchingLineCount;
        }
    }
}
