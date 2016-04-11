using System;

namespace GrepSearch
{
    public class GrepResultEventArgs : EventArgs
    {
        public string FilePath { get; private set; }
        public int LineNumber { get; set; }
        public string Line { get; private set; }

        public GrepResultEventArgs(string filePath, int lineNumber, string line)
        {
            FilePath = filePath;
            LineNumber = lineNumber;
            Line = line;
        }
    }
}
