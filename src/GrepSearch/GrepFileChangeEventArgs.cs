using System;

namespace GrepSearch
{
    public class GrepFileChangeEventArgs : EventArgs
    {
        public string FilePath { get; private set; }

        public GrepFileChangeEventArgs(string filePath)
        {
            FilePath = filePath;
        }
    }
}
