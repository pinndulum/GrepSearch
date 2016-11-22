using System.Text;

namespace GrepSearch
{
    public class LineResult
    {
        public int LineNumber { get; set; }
        public string Line { get; private set; }
        public bool ShowLineNumbers { get; private set; }

        public LineResult(int lineNumber, string line, bool showLineNumbers = false)
        {
            LineNumber = lineNumber;
            Line = line;
            ShowLineNumbers = showLineNumbers;
        }

        public override string ToString()
        {
            return string.Format("{0}{1}", !ShowLineNumbers ? null : string.Format("{0}: ", LineNumber), Line);
        }
    }
}
