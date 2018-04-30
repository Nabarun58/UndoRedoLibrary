using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UndoRedoLibrary
{
    public delegate void UndoRedoPropertyChangedEventHandler(object sender, UndoRedoPropertyChangedEventArgs e);
    public class UndoRedoPropertyChangedEventArgs : EventArgs
    {
        private string _propertyName = string.Empty;
        public UndoRedoPropertyChangedEventArgs(string propertyName)
        {
            this._propertyName = propertyName;
        }
        public UndoRedoPropertyChangedEventArgs(string propertyName, object newValue)
        {
            this._propertyName = propertyName;
        }
        public UndoRedoPropertyChangedEventArgs(string propertyName, object newValue, UndoRedoCommandType type)
        {
            this._propertyName = propertyName;
            //this.OldValue = oldValue;
            this.Value = newValue;
            this.CommandType = type;
        }

      //  public object OldValue { get; set; }

        public object Value { get; set; }
        public UndoRedoCommandType CommandType { get; set; }
        public virtual string PropertyName
        {
            get { return _propertyName; }
        }
    }

    public enum UndoRedoCommandType
    {
        Commit, Undo, Redo
    }

}
