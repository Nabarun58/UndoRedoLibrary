using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UndoRedoLibrary
{
    public sealed class UndoRedoManager
    {
        #region Private/Public Properties/Variables

        private int ThresholdObjectlimit = 0;

        private Dictionary<string, List<UndoRedoPropertyChangedEventArgs>> _unDoMainDictionaryData;
        private Dictionary<string, List<UndoRedoPropertyChangedEventArgs>> UnDoMainDictionaryData
        {
            get { return _unDoMainDictionaryData; }
            set { _unDoMainDictionaryData = value; }
        }

        private Dictionary<string, List<UndoRedoPropertyChangedEventArgs>> _reDoMainDictionaryData;
        private Dictionary<string, List<UndoRedoPropertyChangedEventArgs>> ReDoMainDictionaryData
        {
            get { return _reDoMainDictionaryData; }
            set { _reDoMainDictionaryData = value; }
        }

        private bool _isBypassRequired = false;

        private bool IsBypassRequired
        {
            get
            {
                return _isBypassRequired;
            }
            set
            {
                _isBypassRequired = value;
            }
        }

        private string CurrentActionName { get; set; }

        //private List<Dictionary<string, List<UndoRedoPropertyChangedEventArgs>>> GlobalListOfUndoObjectsByActionName { get; set; }
        //private List<Dictionary<string, List<UndoRedoPropertyChangedEventArgs>>> GlobalListOfRedoObjectsByActionName { get; set; }

        #endregion

        #region Public UndoRedoTriggering Event
        public event UndoRedoPropertyChangedEventHandler UndoRedoOperationTriggerChanged;

        #endregion

        #region Manager's Singleton instance
        private UndoRedoManager()
        {            
            this.CurrentActionName = string.Empty;
            this.UnDoMainDictionaryData = new Dictionary<string, List<UndoRedoPropertyChangedEventArgs>>();
            this.ReDoMainDictionaryData = new Dictionary<string, List<UndoRedoPropertyChangedEventArgs>>();
            ThresholdObjectlimit = 8;
        }


        private static readonly Lazy<UndoRedoManager> lazy = new Lazy<UndoRedoManager>(() => new UndoRedoManager());

        public static UndoRedoManager UndoRedoManagerSingletonInstance { get { return lazy.Value; } }

        #endregion

        #region Private/Public Methods
        public void RaiseUndoRedoPropertyChangeEvent(UndoRedoCommandType actionType)
        {           
            if (!string.IsNullOrEmpty(CurrentActionName))
            {
                UndoRedoPropertyChangedEventArgs lastObject = null;
                if (actionType == UndoRedoCommandType.Undo && UnDoMainDictionaryData.Count() > 0)
                {
                    lastObject = UnDoMainDictionaryData[CurrentActionName].LastOrDefault();
                }
                else if (actionType == UndoRedoCommandType.Redo && ReDoMainDictionaryData.Count() > 0)
                {
                    lastObject = ReDoMainDictionaryData[CurrentActionName].LastOrDefault();
                }
                if (lastObject != null)
                {
                    AddAndRemoveObjectInUndoRedoList(lastObject, CurrentActionName, (actionType == UndoRedoCommandType.Undo) ? UndoRedoCommandType.Redo : UndoRedoCommandType.Undo);
                    lastObject = UnDoMainDictionaryData[CurrentActionName].LastOrDefault();
                    if (lastObject != null)
                    {
                        this.IsBypassRequired = true;
                        if (UndoRedoOperationTriggerChanged != null)
                            UndoRedoOperationTriggerChanged.Invoke(CurrentActionName, lastObject);
                        this.IsBypassRequired = false;

                    }
                }
            }
          
        }

        public void DeAssociateUndoRedoPropertychangeEvent()
        {
            if (UndoRedoOperationTriggerChanged != null)
                UndoRedoOperationTriggerChanged = null;
            UnDoMainDictionaryData.Clear();
            ReDoMainDictionaryData.Clear();
        }

        public void AddAndRemoveObjectInUndoRedoList(UndoRedoPropertyChangedEventArgs objectTochange, string actionName, UndoRedoCommandType actionType)
        {
            if (!string.IsNullOrEmpty(actionName) && CurrentActionName != actionName)
            {
                this.RefreshUndoRedoDictionary();
                this.CurrentActionName = actionName;
            }
            if (!this.IsBypassRequired && !string.IsNullOrEmpty(actionName))
            {
                if (actionType == UndoRedoCommandType.Undo && UnDoMainDictionaryData.ContainsKey(actionName) && UnDoMainDictionaryData[actionName].Count() < ThresholdObjectlimit)
                {
                    RemoveFromUndoRedoList(UndoRedoCommandType.Redo, actionName, objectTochange);
                    objectTochange.CommandType = actionType;
                    UnDoMainDictionaryData[actionName].Add(objectTochange);

                }
                else if (actionType == UndoRedoCommandType.Redo && ReDoMainDictionaryData.ContainsKey(actionName) && ReDoMainDictionaryData[actionName].Count() < ThresholdObjectlimit)
                {
                    RemoveFromUndoRedoList(UndoRedoCommandType.Undo, actionName, objectTochange);
                    objectTochange.CommandType = actionType;
                    ReDoMainDictionaryData[actionName].Add(objectTochange);

                }
                else if (actionType == UndoRedoCommandType.Undo && UnDoMainDictionaryData.ContainsKey(actionName) && UnDoMainDictionaryData[actionName].Count() == ThresholdObjectlimit)
                {
                    var firstObject = UnDoMainDictionaryData[actionName].Last();
                    UnDoMainDictionaryData[actionName].Remove(firstObject);
                    RemoveFromUndoRedoList(UndoRedoCommandType.Redo, actionName, objectTochange);
                    objectTochange.CommandType = actionType;
                    UnDoMainDictionaryData[actionName].Add(objectTochange);

                }
                else if (actionType == UndoRedoCommandType.Redo && ReDoMainDictionaryData.ContainsKey(actionName) && ReDoMainDictionaryData[actionName].Count() == ThresholdObjectlimit)
                {
                    var firstObject = ReDoMainDictionaryData[actionName].Last();
                    ReDoMainDictionaryData[actionName].Remove(firstObject);
                    RemoveFromUndoRedoList(UndoRedoCommandType.Undo, actionName, objectTochange);
                    objectTochange.CommandType = actionType;
                    ReDoMainDictionaryData[actionName].Add(objectTochange);
                }
                else if ((actionType == UndoRedoCommandType.Redo && !ReDoMainDictionaryData.ContainsKey(actionName)) || (actionType == UndoRedoCommandType.Undo && !UnDoMainDictionaryData.ContainsKey(actionName)))
                {
                    if (actionType == UndoRedoCommandType.Redo)
                        RemoveFromUndoRedoList(UndoRedoCommandType.Undo, actionName, objectTochange);
                    else if (actionType == UndoRedoCommandType.Undo)
                        RemoveFromUndoRedoList(UndoRedoCommandType.Redo, actionName, objectTochange);
                    objectTochange.CommandType = actionType;
                    AddFirstTimeInUndoRedoList(actionType, actionName, objectTochange);
                }
            }

        }

        void AddFirstTimeInUndoRedoList(UndoRedoCommandType actionType, string actionName, UndoRedoPropertyChangedEventArgs objectTochange)
        {
            List<UndoRedoPropertyChangedEventArgs> undoRedoItems = new List<UndoRedoPropertyChangedEventArgs>();
            undoRedoItems.Add(objectTochange);
            if(actionType == UndoRedoCommandType.Undo)
                UnDoMainDictionaryData.Add(actionName, undoRedoItems);
            else
                ReDoMainDictionaryData.Add(actionName, undoRedoItems);

        }

        void RefreshUndoRedoDictionary()
        {            
            UnDoMainDictionaryData.Clear();
            ReDoMainDictionaryData.Clear();
        }
        void RemoveFromUndoRedoList(UndoRedoCommandType actionType, string actionName, UndoRedoPropertyChangedEventArgs objectTochange)
        {
            if (actionType == UndoRedoCommandType.Undo && UnDoMainDictionaryData.ContainsKey(actionName))
            {
                var objToRemove = UnDoMainDictionaryData[actionName].Where(x => (x.Value == objectTochange.Value )).FirstOrDefault();
                if (objToRemove != null)
                    UnDoMainDictionaryData[actionName].Remove(objToRemove);
            }
            else if (actionType == UndoRedoCommandType.Redo && ReDoMainDictionaryData.ContainsKey(actionName))
            {
                if (ReDoMainDictionaryData.ContainsKey(actionName))
                {
                    var objToRemove = ReDoMainDictionaryData[actionName].Where(x => (x.Value == objectTochange.Value)).FirstOrDefault();
                    if (objToRemove != null)
                        ReDoMainDictionaryData[actionName].Remove(objToRemove);
                }
            }
        }
        #endregion
    }
}
