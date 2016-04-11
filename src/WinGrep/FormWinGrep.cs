using GrepSearch;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace WinGrep
{
    public partial class FormWinGrep : Form
    {
        private readonly GrepWorker _grepWorker = new GrepWorker();

        private object _updatelock = new object();
        private int _totalFileMatches;
        private int _totalLineMatches;

        public FormWinGrep()
        {
            InitializeComponent();
            _grepWorker.GrepFileChanged += GrepFileChanged;
            _grepWorker.GrepFileHasMatch += _grepWorker_GrepFileHasMatch;
            _grepWorker.GrepResult += GrepResult;
            _grepWorker.GrepFinishedFile += GrepFinishedFile;
            _grepWorker.GrepException += GrepException;
            _grepWorker.GrepCanceled += GrepCanceled;
            _grepWorker.GrepComplete += GrepComplete;
        }

        #region Thread Safe Control Delegates
        delegate void SetCursorDelegate(FormWinGrep form, Cursor cursor);
        delegate void AppendTextDelegate(TextBox tb, string text);
        delegate void SetTextDelegate(TextBox tb, string text);
        delegate string GetTextDelegate(TextBox tb);
        delegate bool GetCheckedDelegate(CheckBox cb);

        private void SetCursor(FormWinGrep form, Cursor cursor)
        {
            if (form.InvokeRequired)
            {
                form.Invoke(new SetCursorDelegate(SetCursor), form, cursor);
                return;
            }
            form.Cursor = cursor;
        }

        private void AppendText(TextBox tb, string text)
        {
            if (tb.InvokeRequired)
            {
                tb.Invoke(new AppendTextDelegate(AppendText), tb, text);
                return;
            }
            tb.Text += text;
            tb.SelectionStart = Math.Max(0, tb.Text.Length - 1);
            tb.SelectionLength = 0;
            tb.ScrollToCaret();
        }

        private void SetText(TextBox tb, string text)
        {
            if (tb.InvokeRequired)
            {
                tb.Invoke(new SetTextDelegate(SetText), tb, text);
                return;
            }
            tb.Text = text;
        }

        private string GetText(TextBox tb)
        {
            if (tb.InvokeRequired)
                return (string)tb.Invoke(new GetTextDelegate(GetText), tb);
            return tb.Text;
        }

        private bool GetChecked(CheckBox cb)
        {
            if (cb.InvokeRequired)
                return (bool)cb.Invoke(new GetCheckedDelegate(GetChecked), cb);
            return cb.Checked;
        }
        #endregion Thread Safe Control Delegates

        #region Grep Search Events
        void GrepFileChanged(object sender, GrepFileChangeEventArgs e)
        {
            SetCursor(this, Cursors.WaitCursor);
            lock (_updatelock)
            {
                SetText(txtCurFile, e.FilePath);
            }
        }

        void _grepWorker_GrepFileHasMatch(object sender, GrepFileChangeEventArgs e)
        {
            lock (_updatelock)
            {
                _totalFileMatches++;
                var fileNamesOnly = GetChecked(ckJustFiles);
                var line = string.Format("{0}{1}\r\n", e.FilePath, fileNamesOnly ? null : ":");
                AppendText(txtResults, line);
            }
        }

        void GrepResult(object sender, GrepResultEventArgs e)
        {
            lock (_updatelock)
            {
                var fileNamesOnly = GetChecked(ckJustFiles);
                if (fileNamesOnly)
                    return;
                var dispLineNum = GetChecked(ckLineNumbers);
                var line = string.Format("  {0}{1}\r\n", !dispLineNum ? null : string.Format("{0}: ", e.LineNumber), e.Line);
                AppendText(txtResults, line);
            }
        }

        void GrepFinishedFile(object sender, GrepFinishedFileEventArgs e)
        {
            SetCursor(this, Cursors.WaitCursor);
            lock (_updatelock)
            {
                var fileNamesOnly = GetChecked(ckJustFiles);
                var countLines = fileNamesOnly ? false : GetChecked(ckCountLines);
                var line = string.Empty;
                if (e.MatchingLineCount > 0)
                {
                    if (countLines)
                        line += string.Format("  {0} Matching Line(s)\r\n", e.MatchingLineCount);
                    _totalLineMatches += e.MatchingLineCount;
                }
                if (!string.IsNullOrEmpty(line))
                    AppendText(txtResults, line);
            }
        }

        void GrepException(object sender, UnhandledExceptionEventArgs e)
        {
            lock (_updatelock)
            {
                var err = (Exception)e.ExceptionObject;
                var line = string.Format("{0}{1}\r\n", e.IsTerminating ? "Fatal Error: " : null, err.Message);
                AppendText(txtResults, line);
            }
            if (e.IsTerminating)
                SetCursor(this, Cursors.Arrow);
        }

        void GrepCanceled(object sender, EventArgs e)
        {
            lock (_updatelock)
            {
                _totalFileMatches = 0;
                _totalLineMatches = 0;
                var line = string.Format("Win Grep Canceled.\r\n");
                SetText(txtResults, line);
            }
            SetCursor(this, Cursors.Arrow);
        }

        void GrepComplete(object sender, EventArgs e)
        {
            lock (_updatelock)
            {
                var fileNamesOnly = GetChecked(ckJustFiles);
                var countLines = fileNamesOnly ? false : GetChecked(ckCountLines);

                var line = string.Format("Win Grep Complete. {0} Total Matching File(s){1}", _totalFileMatches, !countLines ? null : string.Format(", {0} Total Matching Line(s)", _totalLineMatches));
                AppendText(txtResults, line);
            }
            SetCursor(this, Cursors.Arrow);
        }
        #endregion Grep Search Events

        private void StartSearch()
        {
            var directory = txtDir.Text;
            if (!Directory.Exists(directory))
            {
                MessageBox.Show("Invalid Directory Path", "Win Grep Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            var pattern = txtSearchText.Text;
            if (string.IsNullOrWhiteSpace(pattern))
            {
                MessageBox.Show("Invalid or Missing Search Pattern", "Win Grep Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            EnsureExtensionFiltersExist(checkedComboBox1);
            var extensions = checkedComboBox1.CheckedItems.OfType<CheckComboBoxControl.CheckedComboBoxItem>().Select(x => x.Name).ToArray();

            _totalFileMatches = 0;
            _totalLineMatches = 0;
            Cursor = Cursors.WaitCursor;
            txtCurFile.Text = null;
            txtResults.Text = null;

            var isRecursive = GetChecked(ckRecursive);
            var ignoreCase = GetChecked(ckIgnoreCase);
            var fileNamesOnly = GetChecked(ckJustFiles);
            _grepWorker.Start(directory, "*.*", extensions, pattern, isRecursive, ignoreCase, fileNamesOnly);
        }

        private void BuildCheckedComboBoxItems(CheckComboBoxControl.CheckedComboBox cb)
        {
            var selected = cb.CheckedItems.OfType<CheckComboBoxControl.CheckedComboBoxItem>().ToArray();
            cb.Items.Clear();
            var extensions = new HashSet<string> { "*.*" };
            try
            {
                var dirNfo = new DirectoryInfo(txtDir.Text);
                if (dirNfo.Exists)
                {
                    foreach (var filepath in Directory.EnumerateFiles(txtDir.Text, "*", SearchOption.AllDirectories))
                    {
                        var ext = Path.GetExtension(filepath).ToLower();
                        if (string.IsNullOrEmpty(ext))
                            continue;
                        extensions.Add("*" + ext);
                    }
                }
            }
            catch { }
            cb.Items.AddRange(extensions.OrderBy(x => x).Select((x, i) => new CheckComboBoxControl.CheckedComboBoxItem(x, i)).ToArray());
            foreach (var item in cb.Items.OfType<CheckComboBoxControl.CheckedComboBoxItem>().Where(x => selected.Select(y => y.Name).Contains(x.Name)).ToArray())
                cb.SetItemChecked(item, true);
            EnsureExtensionFiltersExist(cb);
        }

        private void EnsureExtensionFiltersExist(CheckComboBoxControl.CheckedComboBox cb)
        {
            if (cb.CheckedItems.Count < 1)
                cb.SetItemChecked(0, true);
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            var curdir = string.IsNullOrWhiteSpace(txtDir.Text) ? Directory.GetCurrentDirectory() : txtDir.Text;
            var fd = new FolderBrowserDialog { ShowNewFolderButton = false, SelectedPath = curdir };
            if (fd.ShowDialog() == DialogResult.OK)
                txtDir.Text = fd.SelectedPath;
        }

        private void checkedComboBox1_DropDown(object sender, EventArgs e)
        {
            var cb = sender as CheckComboBoxControl.CheckedComboBox;
            BuildCheckedComboBoxItems(cb);
        }

        private void checkedComboBox1_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            var cb = sender as CheckComboBoxControl.CheckedComboBox;
            var cbitem = cb.Items[e.Index];
            var default_cbitem = cb.Items[0];
            var checkedItems = cb.CheckedItems.OfType<object>();
            switch (e.NewValue)
            {
                case CheckState.Checked:
                    checkedItems = checkedItems.Concat(new[] { cbitem });
                    if (e.Index > 0)
                    {
                        if (checkedItems.Contains(default_cbitem))
                        {
                            cb.ItemCheck -= checkedComboBox1_ItemCheck;
                            cb.SetItemChecked(0, false);
                            cb.ItemCheck += checkedComboBox1_ItemCheck;
                        }
                    }
                    else
                    {
                        foreach (var item in checkedItems.Except(new[] { cbitem }).ToArray())
                        {
                            var ndx = cb.Items.IndexOf(item);
                            cb.SetItemChecked(ndx, false);
                        }
                    }
                    break;
                case CheckState.Unchecked:
                case CheckState.Indeterminate:
                    checkedItems = checkedItems.Except(new[] { cbitem, default_cbitem });
                    if (e.Index > 0)
                    {
                        if (!checkedItems.Any())
                        {
                            cb.ItemCheck -= checkedComboBox1_ItemCheck;
                            cb.SetItemChecked(0, true);
                            cb.ItemCheck += checkedComboBox1_ItemCheck;
                        }
                    }
                    else
                    {
                        e.NewValue = checkedItems.Any() ? e.NewValue : CheckState.Checked;
                    }
                    break;
            }
        }

        private void tb_KeyDown_Search(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && btnSearch.Enabled)
                StartSearch();
        }

        private void tb_TextChanged_Verify(object sender, EventArgs e)
        {
            btnSearch.Enabled = !string.IsNullOrWhiteSpace(txtDir.Text) && !string.IsNullOrWhiteSpace(txtSearchText.Text);
            if (sender.Equals(txtDir))
                BuildCheckedComboBoxItems(checkedComboBox1);
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            StartSearch();
        }

        private void ckJustFiles_Click(object sender, EventArgs e)
        {
            ckLineNumbers.Enabled = ckCountLines.Enabled = !ckJustFiles.Checked;
        }
    }
}
