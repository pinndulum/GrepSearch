using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GrepSearch
{
    public class GrepResult
    {
        private List<LineResult> _lines = new List<LineResult>();

        public string FilePath { get; private set; }
        public bool FileNameOnly { get; private set; }
        public bool ShowLineNumbers { get; private set; }
        public bool ShowLineCount { get; private set; }
        public LineResult[] Lines { get { return _lines.OrderBy(x => x.LineNumber).ToArray(); } }

        public GrepResult(string filePath, bool fileNameOnly, bool showLineNumbers, bool showLineCount)
        {
            FilePath = filePath;
            FileNameOnly = fileNameOnly;
            ShowLineNumbers = showLineNumbers;
            ShowLineCount = fileNameOnly ? false : showLineCount;
        }

        public string this[int lineNumber]
        {
            get
            {
                var lineResult = _lines.FirstOrDefault(x => x.LineNumber == lineNumber) ?? new LineResult(-1, null);
                return lineResult.Line;
            }
            set
            {
                var lineResult = _lines.FirstOrDefault(x => x.LineNumber == lineNumber);
                if (lineResult == null)
                {
                    lineResult = new LineResult(lineNumber, value, ShowLineNumbers);
                }
                else
                {
                    _lines.Remove(lineResult);
                }
                _lines.Add(lineResult);
            }
        }

        public override string ToString()
        {
            var sb = new StringBuilder(FilePath);
            if (!FileNameOnly)
            {
                sb.AppendLine(":");
                foreach (var line in Lines)
                    sb.AppendFormat("  {0}\r\n", line);
            }
            if (ShowLineCount)
                sb.AppendFormat("  {0} Matching Line(s)\r\n", Lines.Count());
            return sb.ToString();
        }
    }
}
