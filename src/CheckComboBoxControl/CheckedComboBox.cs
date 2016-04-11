using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace CheckComboBoxControl
{
    public class CheckedComboBox : ComboBox
    {
        /// <summary>
        /// Internal class to represent the dropdown list of the CheckedComboBox
        /// </summary>
        internal class Dropdown : Form
        {
            // ---------------------------------- internal class CCBoxEventArgs --------------------------------------------
            /// <summary>
            /// Custom EventArgs encapsulating value as to whether the combo box value(s) should be assignd to or not.
            /// </summary>
            internal class CCBoxEventArgs : EventArgs
            {
                public bool AssignValues { get; set; }

                public CCBoxEventArgs(bool assignValues)
                    : base()
                {
                    AssignValues = assignValues;
                }
            }

            // ---------------------------------- internal class CustomCheckedListBox --------------------------------------------

            /// <summary>
            /// A custom CheckedListBox being shown within the dropdown form representing the dropdown list of the CheckedComboBox.
            /// </summary>
            internal class CustomCheckedListBox : CheckedListBox
            {
                private int _curSelIndex = -1;

                public CustomCheckedListBox()
                    : base()
                {
                    SelectionMode = SelectionMode.One;
                    HorizontalScrollbar = true;
                }

                /// <summary>
                /// Intercepts the keyboard input, [Enter] confirms a selection and [Esc] cancels it.
                /// </summary>
                /// <param name="e">The Key event arguments</param>
                protected override void OnKeyDown(KeyEventArgs e)
                {
                    var parent = (CheckedComboBox.Dropdown)Parent;
                    if (e.KeyCode == Keys.Enter)
                    {
                        // Enact selection.
                        parent.OnDeactivate(new CCBoxEventArgs(true));
                        e.Handled = true;
                    }
                    else if (e.KeyCode == Keys.Escape)
                    {
                        // Cancel selection.
                        parent.OnDeactivate(new CCBoxEventArgs(false));
                        e.Handled = true;
                    }
                    else if (e.KeyCode == Keys.Delete)
                    {
                        // Delete unckecks all, [Shift + Delete] checks all.
                        for (int i = 0; i < Items.Count; i++)
                            SetItemChecked(i, e.Shift);
                        e.Handled = true;
                    }
                    // If no Enter or Esc keys presses, let the base class handle it.
                    base.OnKeyDown(e);
                }

                protected override void OnMouseMove(MouseEventArgs e)
                {
                    base.OnMouseMove(e);
                    var index = IndexFromPoint(e.Location);
                    Debug.WriteLine("Mouse over item: " + (index >= 0 ? GetItemText(Items[index]) : "None"));
                    if (index > -1 && index != _curSelIndex)
                    {
                        _curSelIndex = index;
                        SetSelected(index, true);
                    }
                }

            } // end internal class CustomCheckedListBox

            // --------------------------------------------------------------------------------------------------------

            // ********************************************* Data *********************************************
            private CheckedComboBox _ccbParent;
            private CustomCheckedListBox _cclb;
            private bool[] _checkedStateArr; // Array holding the checked states of the items. This will be used to reverse any changes if user cancels selection.
            
            private string _oldStrValue = string.Empty;
            private bool _dropdownClosed = true;

            // Keeps track of whether checked item(s) changed, hence the value of the CheckedComboBox as a whole changed.
            // This is simply done via maintaining the old string-representation of the value(s) and the new one and comparing them!
            public bool ValueChanged
            {
                get
                {
                    var newStrValue = _ccbParent.Text;
                    if (_oldStrValue.Length > 0 && newStrValue.Length > 0)
                        return _oldStrValue.CompareTo(newStrValue) != 0;
                    return _oldStrValue.Length != newStrValue.Length;
                }
            }

            public CustomCheckedListBox List { get { return _cclb; } set { _cclb = value; } }

            // ********************************************* Construction *********************************************
            public Dropdown(CheckedComboBox ccbParent)
            {
                _ccbParent = ccbParent;
                InitializeComponent();
                ShowInTaskbar = false;
                // Add a handler to notify our parent of ItemCheck events.
                _cclb.ItemCheck += cclb_ItemCheck;
            }

            // ********************************************* Methods *********************************************

            // Create a CustomCheckedListBox which fills up the entire form area.
            private void InitializeComponent()
            {
                _cclb = new CustomCheckedListBox();
                SuspendLayout();
                // 
                // cclb
                // 
                _cclb.BorderStyle = System.Windows.Forms.BorderStyle.None;
                _cclb.Dock = System.Windows.Forms.DockStyle.Fill;
                _cclb.FormattingEnabled = true;
                _cclb.Location = new System.Drawing.Point(0, 0);
                _cclb.Name = "cclb";
                _cclb.Size = new System.Drawing.Size(47, 15);
                _cclb.TabIndex = 0;
                // 
                // Dropdown
                // 
                AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
                AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
                BackColor = System.Drawing.SystemColors.Menu;
                ClientSize = new System.Drawing.Size(47, 16);
                ControlBox = false;
                Controls.Add(_cclb);
                ForeColor = System.Drawing.SystemColors.ControlText;
                FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
                MinimizeBox = false;
                Name = "ccbParent";
                StartPosition = System.Windows.Forms.FormStartPosition.Manual;
                ResumeLayout(false);
            }

            public string GetCheckedItemsStringValue()
            {
                return GetItemsStringValue(_cclb.CheckedItems.OfType<object>());
            }

            public string GetItemsStringValue(IEnumerable<object> checkedItems)
            {
                var sb = new StringBuilder();
                foreach (var checkedItem in checkedItems)
                {
                    if (sb.Length > 0)
                        sb.Append(_ccbParent.ValueSeparator);
                    sb.Append(_cclb.GetItemText(checkedItem));
                }
                return sb.ToString();
            }

            /// <summary>
            /// Closes the dropdown portion and enacts any changes according to the specified boolean parameter.
            /// NOTE: even though the caller might ask for changes to be enacted, this doesn't necessarily mean
            ///       that any changes have occurred as such. Caller should check the ValueChanged property of the
            ///       CheckedComboBox (after the dropdown has closed) to determine any actual value changes.
            /// </summary>
            /// <param name="enactChanges"></param>
            public void CloseDropdown(bool enactChanges)
            {
                if (_dropdownClosed)
                    return;
                Debug.WriteLine("CloseDropdown");
                // Perform the actual selection and display of checked items.
                if (enactChanges)
                {
                    _ccbParent.SelectedIndex = -1;
                    // Set the text portion equal to the string comprising all checked items (if any, otherwise empty!).
                    _ccbParent.Text = GetCheckedItemsStringValue();
                }
                else
                {
                    // Caller cancelled selection - need to restore the checked items to their original state.
                    for (int i = 0; i < _cclb.Items.Count; i++)
                        _cclb.SetItemChecked(i, _checkedStateArr[i]);
                }
                // From now on the dropdown is considered closed. We set the flag here to prevent OnDeactivate() calling
                // this method once again after hiding this window.
                _dropdownClosed = true;
                // Set the focus to our parent CheckedComboBox and hide the dropdown check list.
                _ccbParent.Focus();
                Hide();
                // Notify CheckedComboBox that its dropdown is closed. (NOTE: it does not matter which parameters we pass to
                // OnDropDownClosed() as long as the argument is CCBoxEventArgs so that the method knows the notification has
                // come from our code and not from the framework).
                _ccbParent.OnDropDownClosed(new CCBoxEventArgs(false));
            }

            protected override void OnActivated(EventArgs e)
            {
                Debug.WriteLine("OnActivated");
                base.OnActivated(e);
                _dropdownClosed = false;
                // Assign the old string value to compare with the new value for any changes.
                _oldStrValue = _ccbParent.Text;
                // Make a copy of the checked state of each item, in cace caller cancels selection.
                _checkedStateArr = new bool[_cclb.Items.Count];
                for (int i = 0; i < _cclb.Items.Count; i++)
                    _checkedStateArr[i] = _cclb.GetItemChecked(i);
            }

            protected override void OnDeactivate(EventArgs e)
            {
                Debug.WriteLine("OnDeactivate");
                base.OnDeactivate(e);
                var ce = e as CCBoxEventArgs;
                // When custom event arguments are not passed, this method was called from the framework. We assume that the checked values should be registered regardless.
                var enactChanges = ce == null ? true : ce.AssignValues;
                CloseDropdown(enactChanges);
            }

            private void cclb_ItemCheck(object sender, ItemCheckEventArgs e)
            {
                var itemCheck = _ccbParent.ItemCheck;
                if (itemCheck != null)
                    itemCheck(_ccbParent, e);

                if (e.NewValue == e.CurrentValue)
                    return; // if the state change has been terminated by the previous event handler there is no need to continue.

                var cb = sender as CheckedListBox;
                var cbitem = cb.Items[e.Index];
                var checkedItems = cb.CheckedItems.OfType<object>();
                switch (e.NewValue)
                {
                    case CheckState.Checked:
                        checkedItems = checkedItems.Concat(new[] { cbitem });
                        break;
                    case CheckState.Unchecked:
                    case CheckState.Indeterminate:
                        checkedItems = checkedItems.Except(new[] { cbitem });
                        break;
                }
                _ccbParent.Text = GetItemsStringValue(checkedItems.OrderBy(x => cb.Items.IndexOf(x)).Distinct());
            }

        } // end internal class Dropdown

        // ******************************** Data ********************************

        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        // A form-derived object representing the drop-down list of the checked combo box.
        private Dropdown _dropdown;
        private string _valueSeparator = ", ";

        // Event handler for when an item check state changes.
        public event ItemCheckEventHandler ItemCheck;

        // The ValueSeparator character(s) between the ticked elements as they appear in the text portion of the CheckedComboBox.
        public string ValueSeparator { get { return _valueSeparator; } set { _valueSeparator = value; } }
        public bool CheckOnClick { get { return _dropdown.List.CheckOnClick; } set { _dropdown.List.CheckOnClick = value; } }
        public new string DisplayMember { get { return _dropdown.List.DisplayMember; } set { _dropdown.List.DisplayMember = value; } }

        public new CheckedListBox.ObjectCollection Items { get { return _dropdown.List.Items; } }
        public CheckedListBox.CheckedItemCollection CheckedItems { get { return _dropdown.List.CheckedItems; } }
        public CheckedListBox.CheckedIndexCollection CheckedIndices { get { return _dropdown.List.CheckedIndices; } }
        public bool ValueChanged { get { return _dropdown.ValueChanged; } }

        // ******************************** Construction ********************************

        public CheckedComboBox()
            : base()
        {
            // We want to do the drawing of the dropdown.
            DrawMode = DrawMode.OwnerDrawVariable;
            // This prevents the actual ComboBox dropdown to show, although it's not strickly-speaking necessary.
            // But including this remove a slight flickering just before our dropdown appears (which is caused by
            // the empty-dropdown list of the ComboBox which is displayed for fractions of a second).
            DropDownHeight = 1;
            // This is the default setting - text portion is editable and user must click the arrow button
            // to see the list portion. Although we don't want to allow the user to edit the text portion
            // the DropDownList style is not being used because for some reason it wouldn't allow the text
            // portion to be programmatically set. Hence we set it as editable but disable keyboard input (see below).
            DropDownStyle = ComboBoxStyle.DropDown;
            _dropdown = new Dropdown(this);
            // CheckOnClick style for the dropdown (NOTE: must be set after dropdown is created).
            CheckOnClick = true;
        }

        // ******************************** Operations ********************************

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
                components.Dispose();
            base.Dispose(disposing);
        }

        protected override void OnDropDown(EventArgs e)
        {
            base.OnDropDown(e);
            DoDropDown();
        }

        private void DoDropDown()
        {
            if (_dropdown.Visible)
                return;
            var rect = RectangleToScreen(ClientRectangle);
            _dropdown.Location = new Point(rect.X, rect.Y + Size.Height);
            var count = Math.Max(1, Math.Min(_dropdown.List.Items.Count, MaxDropDownItems));
            var height = (_dropdown.List.ItemHeight * count) + 2;
            _dropdown.Size = new Size(Size.Width, height);
            _dropdown.Show(this);
        }

        protected override void OnDropDownClosed(EventArgs e)
        {
            // Call the handlers for this event only if the call comes from our code - NOT the framework's!
            // NOTE: that is because the events were being fired in a wrong order, due to the actual dropdown list
            //       of the ComboBox which lies underneath our dropdown and gets involved every time.
            if (e is Dropdown.CCBoxEventArgs)
                base.OnDropDownClosed(e);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Down)
            {
                // Signal that the dropdown is "down". This is required so that the behaviour of the dropdown is the same
                // when it is a result of user pressing the Down_Arrow (which we handle and the framework wouldn't know that
                // the list portion is down unless we tell it so).
                // NOTE: all that so the DropDownClosed event fires correctly!                
                OnDropDown(null);
            }
            // Make sure that certain keys or combinations are not blocked.
            e.Handled = !e.Alt && !(e.KeyCode == Keys.Tab) && !((e.KeyCode == Keys.Left) || (e.KeyCode == Keys.Right) || (e.KeyCode == Keys.Home) || (e.KeyCode == Keys.End));
            base.OnKeyDown(e);
        }

        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            e.Handled = true;
            base.OnKeyPress(e);
        }

        private int GetItemIndex<T>(T item)
        {
            var items = _dropdown.List.Items.OfType<T>().Select((x, i) => new { Index = i, Item = x });
            var listItem = items.First(x => x.Item.Equals(item));
            return listItem.Index;
        }

        public bool GetItemChecked<T>(T item)
        {
            return GetItemChecked(GetItemIndex(item));
        }

        public bool GetItemChecked(int index)
        {
            if (index < 0 || index > Items.Count)
                throw new ArgumentOutOfRangeException("index", "value out of range");
            return _dropdown.List.GetItemChecked(index);
        }

        public void SetItemChecked<T>(T item, bool isChecked)
        {
            SetItemChecked(GetItemIndex(item), isChecked);
        }

        public void SetItemChecked(int index, bool isChecked)
        {
            if (index < 0 || index > Items.Count)
                throw new ArgumentOutOfRangeException("index", "value out of range");
            _dropdown.List.SetItemChecked(index, isChecked);
        }

        public CheckState GetItemCheckState<T>(int item)
        {
            return GetItemCheckState(GetItemIndex(item));
        }

        public CheckState GetItemCheckState(int index)
        {
            if (index < 0 || index > Items.Count)
                throw new ArgumentOutOfRangeException("index", "value out of range");
            return _dropdown.List.GetItemCheckState(index);
        }

        public void SetItemCheckState<T>(T item, CheckState state)
        {
            SetItemCheckState(GetItemIndex(item), state);
        }

        public void SetItemCheckState(int index, CheckState state)
        {
            if (index < 0 || index > Items.Count)
                throw new ArgumentOutOfRangeException("index", "value out of range");
            _dropdown.List.SetItemCheckState(index, state);
        }
    } // end public class CheckedComboBox
}
