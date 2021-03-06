﻿using GrepSearch;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace WinGrep
{
    public enum SearchState { Idle, Searching }

    public partial class FormWinGrep : Form
    {
        private class StringConst
        {
            public const string Search = "Search";
            public const string Cancel = "Cancel";
        }

        private readonly GrepWorker _grepWorker = new GrepWorker();

        private object _updatelock = new object();
        private int _totalFileMatches;
        private int _totalLineMatches;

        protected SearchState SearchState { get { return GetText(btnSearch) == StringConst.Search ? SearchState.Idle : SearchState.Searching; } }

        public FormWinGrep()
        {
            InitializeComponent();

            ckLineNumbers.Enabled = ckCountLines.Enabled = !ckJustFiles.Checked;
            btnSearch.Enabled = !string.IsNullOrWhiteSpace(txtDir.Text) && !string.IsNullOrWhiteSpace(txtSearchText.Text);
            if (btnSearch.Enabled)
            {
                BuildCheckedComboBoxItems(checkedComboBox1);
                var extensions = (Properties.Settings.Default.FileExtensions ?? string.Empty).Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim());
                if (extensions.Any())
                {
                    EnsureExtensionFiltersExist(checkedComboBox1);
                    var selection = checkedComboBox1.Items.OfType<CheckComboBoxControl.CheckedComboBoxItem>().Select((x, i) => new { Index = i, Name = x.Name }).Where(x => extensions.Contains(x.Name));
                    foreach (var ext in selection.ToArray())
                        checkedComboBox1.SetItemChecked(ext.Index, true);
                }
            }

            _grepWorker.GrepFileChanged += GrepFileChanged;
            _grepWorker.GrepFileHasMatch += _grepWorker_GrepFileHasMatch;
            _grepWorker.GrepResult += GrepResult;
            _grepWorker.GrepFinishedFile += GrepFinishedFile;
            _grepWorker.GrepException += GrepException;
            _grepWorker.GrepCanceled += GrepCanceled;
            _grepWorker.GrepComplete += GrepComplete;
        }

        #region Thread Safe Control Delegates
        delegate void SetCursorDelegate(Control c, Cursor cursor);
        delegate void SetTextDelegate(Control c, string text);
        delegate string GetTextDelegate(Control c);
        delegate void AppendTextDelegate(TextBox tb, string text);
        delegate bool GetCheckedDelegate(CheckBox cb);
        delegate void ClearListBoxDelegate(ListBox lb);
        delegate void AddListBoxItemDelegate<T>(ListBox lb, T item);
        delegate void RefreshListBoxItemIndexDelegate<T>(ListBox lb, int index);
        delegate void RefreshListBoxItemDelegate<T>(ListBox lb, T item);
        delegate int GetListBoxItemIndexDelegate<T>(ListBox lb, Predicate<T> predicate);
        delegate T GetListBoxItemDelegate<T>(ListBox lb, Predicate<T> predicate);

        private void SetCursor(Control c, Cursor cursor)
        {
            if (c.InvokeRequired)
            {
                c.Invoke(new SetCursorDelegate(SetCursor), c, cursor);
                return;
            }
            c.Cursor = cursor;
        }

        private void SetText(Control c, string text)
        {
            if (c.InvokeRequired)
            {
                c.Invoke(new SetTextDelegate(SetText), c, text);
                return;
            }
            c.Text = text;
        }

        private string GetText(Control c)
        {
            if (c.InvokeRequired)
                return (string)c.Invoke(new GetTextDelegate(GetText), c);
            return c.Text;
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

        private bool GetChecked(CheckBox cb)
        {
            if (cb.InvokeRequired)
                return (bool)cb.Invoke(new GetCheckedDelegate(GetChecked), cb);
            return cb.Checked;
        }

        private void ClearListBox(ListBox lb)
        {
            if (lb.InvokeRequired)
            {
                lb.Invoke(new ClearListBoxDelegate(ClearListBox), lb);
                return;
            }
            lb.Items.Clear();
        }

        private void AddListBoxItem<T>(ListBox lb, T item)
        {
            if (lb.InvokeRequired)
            {
                lb.Invoke(new AddListBoxItemDelegate<T>(AddListBoxItem<T>), lb, item);
                return;
            }
            lb.Items.Add(item);
        }

        private void RefreshListBoxItem<T>(ListBox lb, int index)
        {
            if (lb.InvokeRequired)
            {
                lb.Invoke(new RefreshListBoxItemIndexDelegate<T>(RefreshListBoxItem<T>), lb, index);
                return;
            }
            var items = lb.Items.OfType<T>();
            var count = items.Count();
            if (count < 0 || index < 0 || index > count - 1)
                return;
            lb.Items[index] = lb.Items[index];
        }

        private void RefreshListBoxItem<T>(ListBox lb, T item)
        {
            if (lb.InvokeRequired)
            {
                lb.Invoke(new RefreshListBoxItemDelegate<T>(RefreshListBoxItem<T>), lb, item);
                return;
            }
            var index = GetListBoxItemIndex<T>(lb, x => x.Equals(item));
            RefreshListBoxItem<T>(lb, index);
        }

        private int GetListBoxItemIndex<T>(ListBox lb, Predicate<T> predicate)
        {
            if (lb.InvokeRequired)
                return (int)lb.Invoke(new GetListBoxItemIndexDelegate<T>(GetListBoxItemIndex<T>), lb, predicate);
            var items = lb.Items.OfType<T>().Select((x, i) => new { Index = i, Result = x });
            var item = items.FirstOrDefault(x => predicate(x.Result));
            return item == null ? -1 : item.Index;
        }

        private T GetListBoxItem<T>(ListBox lb, Predicate<T> predicate)
        {
            if (lb.InvokeRequired)
                return (T)lb.Invoke(new GetListBoxItemDelegate<T>(GetListBoxItem<T>), lb, predicate);
            var index = GetListBoxItemIndex<T>(lb, x => predicate(x));
            return lb.Items.OfType<T>().ElementAtOrDefault(index);
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
                var showLineNum = GetChecked(ckLineNumbers);
                var showLineCount = fileNamesOnly ? false : GetChecked(ckCountLines);
                var grepResult = new GrepResult(e.FilePath, fileNamesOnly, showLineNum, showLineCount);
                AddListBoxItem(lbResults, grepResult);
            }
        }

        void GrepResult(object sender, GrepResultEventArgs e)
        {
            lock (_updatelock)
            {
                var grepResult = GetListBoxItem<GrepResult>(lbResults, x => x.FilePath == e.FilePath);
                if (grepResult != null)
                {
                    grepResult[e.LineNumber] = e.Line;
                    RefreshListBoxItem<GrepResult>(lbResults, grepResult);
                }
            }
        }

        void GrepFinishedFile(object sender, GrepFinishedFileEventArgs e)
        {
            SetCursor(this, Cursors.WaitCursor);
            lock (_updatelock)
            {
                if (e.MatchingLineCount > 0)
                    _totalLineMatches += e.MatchingLineCount;
                var index = GetListBoxItemIndex<GrepResult>(lbResults, x => x.FilePath == e.FilePath);
                RefreshListBoxItem<GrepResult>(lbResults, index);
            }
        }

        void GrepException(object sender, UnhandledExceptionEventArgs e)
        {
            lock (_updatelock)
            {
                var err = (Exception)e.ExceptionObject;
                var line = string.Format("{0}{1}\r\n", e.IsTerminating ? "Fatal Error: " : null, err.Message);
                AddListBoxItem(lbResults, line);
            }
            if (e.IsTerminating)
            {
                SetCursor(this, Cursors.Arrow);
                SetText(btnSearch, StringConst.Search);
            }
        }

        void GrepCanceled(object sender, EventArgs e)
        {
            lock (_updatelock)
            {
                _totalFileMatches = 0;
                _totalLineMatches = 0;
                var line = string.Format("Win Grep Canceled.\r\n");
                ClearListBox(lbResults);
                AddListBoxItem(lbResults, line);
            }
            SetCursor(this, Cursors.Arrow);
            SetText(btnSearch, StringConst.Search);
        }

        void GrepComplete(object sender, EventArgs e)
        {
            lock (_updatelock)
            {
                var fileNamesOnly = GetChecked(ckJustFiles);
                var countLines = fileNamesOnly ? false : GetChecked(ckCountLines);

                var line = string.Format("Win Grep Complete. {0} Total Matching File(s){1}", _totalFileMatches, !countLines ? null : string.Format(", {0} Total Matching Line(s)", _totalLineMatches));
                AddListBoxItem(lbResults, line);
            }
            SetCursor(this, Cursors.Arrow);
            SetText(btnSearch, StringConst.Search);
        }
        #endregion Grep Search Events

        private void StartSearch()
        {
            if (SearchState == SearchState.Searching)
                return;
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
            ClearListBox(lbResults);

            var isRecursive = GetChecked(ckRecursive);
            var ignoreCase = GetChecked(ckIgnoreCase);
            var fileNamesOnly = GetChecked(ckJustFiles);
            SetText(btnSearch, StringConst.Cancel);
            SetCursor(btnSearch, Cursors.Arrow);
            _grepWorker.Start(directory, "*.*", extensions, pattern, isRecursive, ignoreCase, fileNamesOnly);
        }

        private void CancelSearch()
        {
            if (SearchState == SearchState.Idle)
                return;
            _grepWorker.Cancel();
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
            if (e.KeyCode == Keys.Enter && SearchState == SearchState.Idle)
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
            switch (SearchState)
            {
                case WinGrep.SearchState.Idle:
                    StartSearch();
                    break;
                case WinGrep.SearchState.Searching:
                    CancelSearch();
                    break;
            }
        }

        private void ckJustFiles_Click(object sender, EventArgs e)
        {
            ckLineNumbers.Enabled = ckCountLines.Enabled = !ckJustFiles.Checked;
        }

        private void lbResults_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0)
                return;
            var lb = sender as ListBox;
            var lbi = lb.Items[e.Index];
            var backcolor = e.Index % 2 == 0 ? e.BackColor : System.Drawing.Color.LightGray;
            if (backcolor != e.BackColor)
                e = new DrawItemEventArgs(e.Graphics, e.Font, e.Bounds, e.Index, e.State, e.ForeColor, backcolor);
            e.DrawBackground();
            e.Graphics.DrawString(lbi.ToString(), e.Font, System.Drawing.Brushes.Black, e.Bounds, System.Drawing.StringFormat.GenericDefault);
            e.DrawFocusRectangle();
        }

        private void lbResults_MeasureItem(object sender, MeasureItemEventArgs e)
        {
            var lb = sender as ListBox;
            var lbi = lb.Items[e.Index];
            var size = e.Graphics.MeasureString(lbi.ToString(), lb.Font).ToSize();
            e.ItemHeight = size.Height;
        }

        private void lbResults_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            var lb = sender as ListBox;
            var index = lb.IndexFromPoint(e.Location);
            if (index == ListBox.NoMatches)
                return;
            var item = lb.Items[index] as GrepResult;
            if (item == null)
                return;
            Process.Start("explorer.exe", "/select, " + item.FilePath);
        }

        private void lbResults_KeyDown(object sender, KeyEventArgs e)
        {
            var lb = sender as ListBox;
            if (e.Control)
            {
                e.SuppressKeyPress = true;
                switch (e.KeyCode)
                {
                    case Keys.A:
                        for (var i = 0; i < lb.Items.Count; i++)
                            lb.SetSelected(i, true);
                        break;
                    case Keys.C:
                        var sb = new StringBuilder();
                        foreach (var item in lb.SelectedItems)
                            sb.AppendFormat("{0}\r\n", item);
                        var text = sb.ToString();
                        if (!string.IsNullOrEmpty(text))
                        {
                            Clipboard.Clear();
                            Clipboard.SetText(text);
                        }
                        break;
                }
            }
        }

        private void FormWinGrep_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (btnSearch.Enabled)
                Properties.Settings.Default.FileExtensions = checkedComboBox1.Text;
            Properties.Settings.Default.Save();
        }
    }
}
