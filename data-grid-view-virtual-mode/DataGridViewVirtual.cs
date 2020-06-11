/*
Copyright 2020 IVSoftware, LLC 
Author: Thomas C. Gregor

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace data_grid_view_virtual_mode
{
    enum DragMode
    {
        Remove,
        Dim
    }
    class DataGridViewVirtual : DataGridView, IList<DataValue>
    {
        public List<DataValue> _data = new List<DataValue>();

        // This is STRICTLY so that test code matches
        // the way the original post reads.
        public List<DataValue> dataList => _data;

        public DataGridViewVirtual()
        {
            _timerMultiSelect = new System.Windows.Forms.Timer();
            _timerMultiSelect.Interval = 2000;
            _timerMultiSelect.Tick += _timerMultiSelect_Tick;
        }
        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            if(!DesignMode)
            {
                // Test data
                Add(new DataValue("A"));
                Add(new DataValue("B"));
                Add(new DataValue("C"));
                Add(new DataValue("D"));

                PropertyInfo[] properties = typeof(DataValue).GetProperties();
                DataGridViewColumn c;
                foreach (var property in properties)
                {
                    switch (property.PropertyType.Name)
                    {
                        case "String":
                            c = new DataGridViewTextBoxColumn();
                            c.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                            break;
                        case "Boolean":
                            c = new DataGridViewCheckBoxColumn();
                            c.AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader;
                            break;
                        default:
                            throw new NotImplementedException("TODO for other types");
                    }
                    c.Name = property.Name;
                    c.HeaderText = c.Name; // But sometimes you want 'different or no' header text.
                    c.ReadOnly = !property.CanWrite;
                    Columns.Add(c);
                }
                RowCount = AllowUserToAddRows ? Count + 1 : Count; // Do this LAST or it will mess with the column count.
                SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                AllowDrop = true;
            }
        }

        // 'Provisional' because adding the row could still be cancelled.
        DataValue _provisional = null;
        private void EnsureProvisionalEditTarget()
        {
            if(_provisional == null)
            {
                _provisional = new DataValue();
                Add(_provisional);
                RowCount = Count;
                CurrentCell = Rows[Count - 1].Cells[0];
                BeginEdit(true);
                Refresh();
            }
        }
        protected override void OnNewRowNeeded(DataGridViewRowEventArgs e)
        {
            base.OnNewRowNeeded(e);
            if(_dragCount == 0) // <== Disallow during Drag op.
            {
                AllowUserToAddRows = false;
                Refresh();
                BeginInvoke((MethodInvoker)delegate { EnsureProvisionalEditTarget(); });
            }
        }

        #region C E L L S

        // Opportunity to cancel
        protected override void OnCellBeginEdit(DataGridViewCellCancelEventArgs e)
        {
            if (_dragCount == 0)
            {
                Debug.WriteLine("CellBeginEdit-ENTER: EditMode=" + IsCurrentCellInEditMode + " Dirty=" + IsCurrentCellDirty);
                if (e.RowIndex == Count) // New object required
                {
                    // Create a 'provisional' object if we don't have one already
                    EnsureProvisionalEditTarget();

                    BeginInvoke((MethodInvoker)delegate
                    {
                        // Force a draw the new item. For example, it
                        // already has a value for the ID field that 
                        // won't show up without a Refresh();
                        Refresh();
                    });
                }
            }
            else
            {
                // We're in a drag op. Disallow edit.
                e.Cancel = true;
            }
            base.OnCellBeginEdit(e);

            // If not cancelled, both EditMode and Dirty 'will be' true after this call.
        }
        protected override void OnCellEndEdit(DataGridViewCellEventArgs e)
        {
            Debug.WriteLine("CellEndEdit: EditMode=" + IsCurrentCellInEditMode + " Dirty=" + IsCurrentCellDirty);
            base.OnCellEndEdit(e);
        }
        private bool IsCellValid(int columnIndex, int rowIndex)
        {
            if (columnIndex == -1) return false;
            if (rowIndex == -1) return false;
            if (rowIndex >= Count) return false;
            return true;
        }

        public bool HandleError { get; set; } = true;
        protected override void OnCellValueNeeded(DataGridViewCellValueEventArgs e)
        {
            base.OnCellValueNeeded(e);

            // Debug.WriteLine("OnCellValueNeeded: " + e.RowIndex + ":" + e.ColumnIndex);
            if (!IsCellValid(columnIndex: e.ColumnIndex, rowIndex: e.RowIndex))
            {
                if(HandleError)
                {
                    Debug.WriteLine("Unexpected!");
                    return;
                }
                else
                {   /* G T K */
                    // Make sure your C# Managed Exceptions are set to break when thrown.
                    // See this answer: https://stackoverflow.com/a/62025885/5438626
                }
            }
            DataValue editTarget = this[e.RowIndex];
            PropertyInfo pi = typeof(DataValue).GetProperty(Columns[e.ColumnIndex].Name);
            Debug.Assert(pi != null);
            e.Value = pi.GetValue(editTarget);
        }

        protected override void OnEditingControlShowing(DataGridViewEditingControlShowingEventArgs e)
        {
            base.OnEditingControlShowing(e);
            e.Control.PreviewKeyDown += _editingControl_PreviewKeyDown;
            e.Control.VisibleChanged += Control_VisibleChanged;
        }

        private void Control_VisibleChanged(object sender, EventArgs e)
        {
            if(!Visible)
            {
                AllowSelect = true;
            }
        }

        private void _editingControl_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            switch(e.KeyData)
            {
                case Keys.Enter:
                    // DFW for this: e.IsInputKey = false;
                    AllowSelect = false;
                    break;
            }
        }


        // Called as a result of CommitEdit() or EndEdit().
        // This will leave DirtyState = false;
        protected override void OnCellValuePushed(DataGridViewCellValueEventArgs e)
        {
            Debug.WriteLine("OnCellValuePushed-Begin: EditMode=" + IsCurrentCellInEditMode + " Dirty=" + IsCurrentCellDirty);
            DataValue editTarget;
            if(e.RowIndex == Count) 
            {
                // New object required
                // For example, clicking the checkbox in the new row.
                EnsureProvisionalEditTarget();
                editTarget = _provisional;
            }
            else
            {
                editTarget = this[e.RowIndex];
            }
            Debug.Assert(editTarget != null);
            PropertyInfo pi = typeof(DataValue).GetProperty(Columns[e.ColumnIndex].Name);
            Debug.Assert(pi != null);
            pi.SetValue(editTarget, e.Value);

            // This apparently enqueues the DirtyState = false
            base.OnCellValuePushed(e);
            // But it is still DIRTY here.
        }
        protected override void OnCurrentCellDirtyStateChanged(EventArgs e)
        {
            Debug.WriteLine("OnCurrentCellDirtyStateChanged-Begin: EditMode=" + IsCurrentCellInEditMode + " Dirty=" + IsCurrentCellDirty);
            base.OnCurrentCellDirtyStateChanged(e);
            if (IsCurrentCellDirty) 
            {
                if (IsCheckboxCell)
                {
                    if (IsCurrentCellInEditMode)
                    {
                        BeginInvoke((MethodInvoker) delegate
                        {
                            Debug.WriteLine("Invoked: EndEdit");
                            EndEdit();
                        });
                    }
                }
                else
                {
                    _isRowDirty = true;
                    Refresh();
                }
            }
            else
            {   /* G T K */
            }

            Debug.WriteLine("OnCurrentCellDirtyStateChanged-End: EditMode=" + IsCurrentCellInEditMode + " Dirty=" + IsCurrentCellDirty);
        }
        protected override void OnCellPainting(DataGridViewCellPaintingEventArgs e)
        {
            if (IsCellValid(columnIndex: e.ColumnIndex, rowIndex: e.RowIndex))
            {
                switch (DragMode)
                {
                    case DragMode.Remove:
                        e.CellStyle.BackColor = SystemColors.Control;
                        e.CellStyle.ForeColor = SystemColors.WindowText;
                        break;
                    case DragMode.Dim:
                        if (_dragItems.Contains(this[e.RowIndex]))
                        {
                            e.CellStyle.BackColor = Color.LightGray;
                            e.CellStyle.ForeColor = Color.White;
                        }
                        else
                        {
                            e.CellStyle.BackColor = SystemColors.Control;
                            e.CellStyle.ForeColor = SystemColors.WindowText;
                        }
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
            base.OnCellPainting(e);
        }
        protected override void OnCellContentClick(DataGridViewCellEventArgs e)
        {
            base.OnCellContentClick(e);
            try
            {
                //if (typeof(DataGridViewCheckBoxCell).IsAssignableFrom(CurrentCell.GetType()))
                //{
                //    CommitEdit(DataGridViewDataErrorContexts.Commit);
                //}
            }
            catch (Exception ex)
            {
                Debug.Assert(false, ex.Message);
            }
        }
        #endregion

        #region R O W S
        bool _isRowDirty = false;
        protected override void OnRowDirtyStateNeeded(QuestionEventArgs e)
        {
            e.Response = _isRowDirty;
            base.OnRowDirtyStateNeeded(e);
        }
        protected override void OnRowValidated(DataGridViewCellEventArgs e)
        {
            base.OnRowValidated(e);
            if (_isRowDirty)
            {
                _isRowDirty = false;
                AllowUserToAddRows = true;
                if(_provisional != null)
                {
                    _provisional = null;
                }
            }
            Refresh();
        }
        protected override void OnCancelRowEdit(QuestionEventArgs e)
        {
            base.OnCancelRowEdit(e);
            if(_provisional != null)
            {
                Remove(_provisional);
                RowCount = Count;
            }
            _provisional = null;
            _isRowDirty = false;
            AllowUserToAddRows = true;
        }
        protected override void OnRowPostPaint(DataGridViewRowPostPaintEventArgs e)
        {
            base.OnRowPostPaint(e);
            if (
                    (_dragCount > 0) &&
                    (e.RowIndex != -1) &&
                    (e.RowIndex < Count)
                )
            {
                if (ExecHitTest(MousePosition, out bool above, out int insertIndex).RowIndex == e.RowIndex)
                {
                    if(!_dragItems.Contains(this[e.RowIndex]))
                    {
                        using (Pen red = new Pen(Color.Red, 2F))
                        {
                            Rectangle displayRect = GetRowDisplayRectangle(e.RowIndex, false);
                            int offsetY = above ? 0 : displayRect.Height;
                            e.Graphics.DrawLine(
                                red,
                                new Point(
                                    displayRect.X,
                                    displayRect.Y + offsetY
                                ),
                                new Point(
                                    displayRect.X + displayRect.Width,
                                    displayRect.Y + offsetY
                                )
                            );
                        }
                    }
                }
            }
        }
        protected override void OnSelectionChanged(EventArgs e)
        {
            base.OnSelectionChanged(e);
            _timerMultiSelect.Stop();
            if(SelectedRows.Count > 1)
            {
                _timerMultiSelect.Start();
            }
        }
        protected override void OnUserDeletingRow(DataGridViewRowCancelEventArgs e)
        {
            base.OnUserDeletingRow(e);
            DataValue delete = this[e.Row.Index];
            Remove(delete);
        }
        #endregion
        readonly System.Windows.Forms.Timer _timerMultiSelect;
        private void _timerMultiSelect_Tick(object sender, EventArgs e)
        {
            _timerMultiSelect.Stop();
        }

        int _dbcount = 0;
        bool IsCheckboxCell
        {
            get =>
                CurrentCell != null &&
                typeof(DataGridViewCheckBoxCell).IsAssignableFrom(CurrentCell.GetType()
            );
        }

        #region M O U S E
        int _mouseDownX;
        int _mouseDeltaX = 0;
        protected override void OnMouseDown(MouseEventArgs e)
        {
            // You have a short period to drag a multiselect.
            // After timer expires, a click selects a single item.
            if(!_timerMultiSelect.Enabled)
            {
                base.OnMouseDown(e);
            }
            _mouseDownX = e.X;            
            _mouseDeltaX = 0;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if(MouseButtons == MouseButtons.Left)
            {
                _mouseDeltaX = e.X - _mouseDownX;
                if(_mouseDeltaX < -30)
                {
                    BeginInvoke((MethodInvoker)delegate { ExecDragDrop(); });
                }
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            if (_dragCount != 0)
            {
                FinalizeDrag();
            }
            base.OnMouseUp(e);
        }

        protected override void SetSelectedRowCore(int rowIndex, bool selected)
        {
            base.SetSelectedRowCore(
                rowIndex, 
                selected && 
                (_mouseDeltaX >= 0) &&  // "Swipe to the right" is OK
                (_dragCount == 0)   &&  // Make sure this isn't a drag op.
                AllowSelect
            );
        }
        bool AllowSelect { get; set; }
        #endregion

        #region D R A G    D R O P
        public DragMode DragMode { get; set; }
        int _lastInsertIndex = -1;
        List<DataValue> _dragItems { get; } = new List<DataValue>();

        // Idles at 0...
        int _dragCount = 0;
        private void ExecDragDrop()
        {
            // ...and only executes when the count is actually 0
            //   (not the other hundreds of times it's called).
            if (_dragCount++ == 0) // <= Increments on every call. 
            {
                _revert.AddRange(this); 
                _lastInsertIndex = -1;
                _dragItems.Clear();
                var selectedRows = (
                    from DataGridViewRow row
                    in SelectedRows
                    select row
                )
                .OrderBy(field => field.Index)
                .ToList();
                foreach (DataGridViewRow row in selectedRows)
                {
                    _dragItems.Add(this[row.Index]);
                }
                switch (DragMode)
                {
                    case DragMode.Remove:
                        foreach (var dragItem in _dragItems)
                        {
                            Remove(dragItem);
                        }
                        break;
                    case DragMode.Dim:
                        break;
                    default:
                        throw new NotImplementedException();
                }
                AllowUserToAddRows = false;
                RowCount = Count;
                ClearSelection();
                Capture = true;
                DoDragDrop(_dragItems, DragDropEffects.Move);
                FinalizeDrag(); // <<= The FinalizeDrag method resets counter to idle at 0
            }
        }

        private void FinalizeDrag()
        {
            RowCount = Count;
            _mouseDeltaX = 0;
            _dragCount = 0;
            _lastInsertIndex = -1;
             Capture = false;
            if(_revert.Count > 0)
            {
                Clear();
                foreach (var orig in _revert)
                {
                    Add(orig);
                }
                _revert.Clear();
                RowCount = Count;   // <= Critical
            }
            for (int i = 0; i < Count; i++)
            {
                if(_dragItems.Contains(this[i]))
                {
                    Rows[i].Selected = true;
                }
            }
            if(_dragItems.Count > 1)
            {
                _timerMultiSelect.Stop();
                _timerMultiSelect.Start();
            }
            _dragItems.Clear();
            AllowUserToAddRows = true;  
            RowCount = Count + 1;  // <= Critical
            Refresh();
        }

        protected override void OnDragOver(DragEventArgs e)
        {            
            base.OnDragOver(e);
            e.Effect = e.AllowedEffect & (DragDropEffects.Move | DragDropEffects.Copy);
            ExecHitTest(MousePosition, out bool above, out int currentInsertIndex);
            if (currentInsertIndex != _lastInsertIndex)
            {
                _lastInsertIndex = currentInsertIndex;
                Refresh();
            }
        }

        protected override void OnDragDrop(DragEventArgs e)
        {
            ExecHitTest(new Point(e.X, e.Y), out bool above, out int insertIndex);
            List<DataValue> dragItems = (List<DataValue>)e.Data.GetData(typeof(List<DataValue>));
            switch (DragMode)
            {
                case DragMode.Remove:
                    foreach (var dragItem in dragItems)
                    {
                        Insert(insertIndex++, dragItem);
                    }
                    break;
                case DragMode.Dim:
                    bool last = insertIndex == Count;
                    if(last)
                    {
                        foreach (var dragItem in dragItems)
                        {
                            Remove(dragItem);
                        }
                        insertIndex = Count;
                    }
                    else
                    {
                        DataValue insertItem = this[insertIndex];
                        foreach (var dragItem in dragItems)
                        {
                            Remove(dragItem);
                        }
                        insertIndex = this.IndexOf(insertItem);
                    }
                    foreach (var dragItem in dragItems)
                    {
                        Insert(insertIndex++, dragItem);
                    }
                    break;
                default:
                    throw new NotImplementedException();
            }
            _revert.Clear();
            base.OnDragDrop(e);
        }

        private HitTestInfo ExecHitTest(Point screen, out bool above, out int insertIndex)
        {
            Point client = PointToClient(screen);
            HitTestInfo hitTest = HitTest(client.X, client.Y);
            if (
                    (hitTest.RowIndex != -1) &&
                    (hitTest.RowIndex < Count)
                 )
            {
                Rectangle displayRect = GetRowDisplayRectangle(hitTest.RowIndex, false);
                int midline = displayRect.Y + (displayRect.Height / 2);
                above = client.Y <= midline;
                insertIndex = above ? hitTest.RowIndex : hitTest.RowIndex + 1;
            }
            else
            {
                above = true;
                insertIndex = -1;
            }
            return hitTest;
        }

        List<DataValue> _revert { get; } = new List<DataValue>();
        #endregion

        #region I L I S T

        public DataValue this[int index] 
        { 
            get => ((IList<DataValue>)_data)[index]; 
            set => ((IList<DataValue>)_data)[index] = value;
        }

        public int Count => ((IList<DataValue>)_data).Count;

        public bool IsReadOnly => ((IList<DataValue>)_data).IsReadOnly;

        public void Add(DataValue item)
        {
            ((IList<DataValue>)_data).Add(item);
        }

        public void Clear()
        {
            ((IList<DataValue>)_data).Clear();
        }

        public bool Contains(DataValue item)
        {
            return ((IList<DataValue>)_data).Contains(item);
        }

        public void CopyTo(DataValue[] array, int arrayIndex)
        {
            ((IList<DataValue>)_data).CopyTo(array, arrayIndex);
        }

        public IEnumerator<DataValue> GetEnumerator()
        {
            return ((IList<DataValue>)_data).GetEnumerator();
        }

        public int IndexOf(DataValue item)
        {
            return ((IList<DataValue>)_data).IndexOf(item);
        }

        public void Insert(int index, DataValue item)
        {
            ((IList<DataValue>)_data).Insert(index, item);
        }

        public bool Remove(DataValue item)
        {
            return ((IList<DataValue>)_data).Remove(item);
        }

        public void RemoveAt(int index)
        {
            ((IList<DataValue>)_data).RemoveAt(index);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IList<DataValue>)_data).GetEnumerator();
        }
        #endregion

    }
}
