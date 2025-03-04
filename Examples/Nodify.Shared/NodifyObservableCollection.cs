﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Nodify
{
    public interface INodifyObservableCollection<T>
    {
        /// <summary>
        /// Called when a new item is added
        /// </summary>
        /// <param name="added">The callback to execute when an item is added</param>
        /// <returns>Returns self</returns>
        INodifyObservableCollection<T> WhenAdded(Action<T> added);

        /// <summary>
        /// Called when an existing item is removed
        /// Note: It is not called when items are cleared if <see cref="WhenCleared(Action{IList{T}})"/> is used
        /// </summary>
        /// <param name="added">The callback to execute when an item is removed</param>
        /// <returns>Returns self</returns>
        INodifyObservableCollection<T> WhenRemoved(Action<T> removed);

        /// <summary>
        /// Called when the collection is cleared
        /// NOTE: It does not call <see cref="WhenRemoved(Action{T})"/> on each item
        /// </summary>
        /// <param name="added">The callback to execute when the collection is cleared</param>
        /// <returns>Returns self</returns>
        INodifyObservableCollection<T> WhenCleared(Action<IList<T>>? cleared);
    }

    public class NodifyObservableCollection<T> : Collection<T>, INodifyObservableCollection<T>, INotifyPropertyChanged, INotifyCollectionChanged
    {
        protected static readonly PropertyChangedEventArgs IndexerPropertyChanged = new PropertyChangedEventArgs("Item[]");
        protected static readonly PropertyChangedEventArgs CountPropertyChanged = new PropertyChangedEventArgs("Count");
        protected static readonly NotifyCollectionChangedEventArgs ResetCollectionChanged = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);

        private readonly List<Action<T>>         _added   = new List<Action<T>>();
        private readonly List<Action<T>>         _removed = new List<Action<T>>();
        private readonly List<Action<IList<T>>?> _cleared = new List<Action<IList<T>>?>();

        public event NotifyCollectionChangedEventHandler? CollectionChanged;
        public event PropertyChangedEventHandler? PropertyChanged;

        public NodifyObservableCollection()
        {
        }

        public NodifyObservableCollection(IEnumerable<T> collection)
            : base(new List<T>(collection))
        {
        }

        #region Collection Events

        public INodifyObservableCollection<T> WhenAdded(Action<T>? added)
        {
            if (added != null)
                _added.Add(added);

            return this;
        }

        public INodifyObservableCollection<T> WhenRemoved(Action<T>? removed)
        {
            if (removed != null)
                _removed.Add(removed);

            return this;
        }

        public INodifyObservableCollection<T> WhenCleared(Action<IList<T>>? cleared)
        {
            if (cleared != null)
                _cleared.Add(cleared);

            return this;
        }

        protected virtual void NotifyOnItemAdded(T item)
        {
            foreach (var action in _added)
                action(item);
        }

        protected virtual void NotifyOnItemRemoved(T item)
        {
            foreach (var action in _removed)
                action(item);
        }

        protected virtual void NotifyOnItemsCleared(IList<T> items)
        {
            foreach (var action in _cleared)
                action?.Invoke(items);
        }

        #endregion

        #region Collection Handlers

        protected override void ClearItems()
        {
            var items = new List<T>(this);
            base.ClearItems();

            if (_cleared.Count > 0)
            {
                NotifyOnItemsCleared(items);
            }
            else
            {
                for (int i = 0; i < items.Count; i++)
                {
                    NotifyOnItemRemoved(items[i]);
                }
            }

            OnPropertyChanged(CountPropertyChanged);
            OnPropertyChanged(IndexerPropertyChanged);
            OnCollectionChanged(ResetCollectionChanged);
        }

        protected override void InsertItem(int index, T item)
        {
            base.InsertItem(index, item);

            OnPropertyChanged(CountPropertyChanged);
            OnPropertyChanged(IndexerPropertyChanged);
            OnCollectionChanged(NotifyCollectionChangedAction.Add, item, index);
            NotifyOnItemAdded(item);
        }

        protected override void RemoveItem(int index)
        {
            var item = base[index];
            base.RemoveItem(index);

            OnPropertyChanged(CountPropertyChanged);
            OnPropertyChanged(IndexerPropertyChanged);
            OnCollectionChanged(NotifyCollectionChangedAction.Remove, item, index);
            NotifyOnItemRemoved(item);
        }

        protected override void SetItem(int index, T item)
        {
            T prev = base[index];
            base.SetItem(index, item);
            OnPropertyChanged(IndexerPropertyChanged);
            OnCollectionChanged(NotifyCollectionChangedAction.Replace, prev, item, index);
            NotifyOnItemRemoved(prev);
            NotifyOnItemAdded(item);
        }

        public void Move(int oldIndex, int newIndex)
        {
            T prev = base[oldIndex];
            base.RemoveItem(oldIndex);
            base.InsertItem(newIndex, prev);
            OnPropertyChanged(IndexerPropertyChanged);
            OnCollectionChanged(NotifyCollectionChangedAction.Move, prev, newIndex, oldIndex);
        }

        protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
            => CollectionChanged?.Invoke(this, e);

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs args)
            => PropertyChanged?.Invoke(this, args);

        private void OnCollectionChanged(NotifyCollectionChangedAction action, object? item, int index)
            => OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, item, index));

        private void OnCollectionChanged(NotifyCollectionChangedAction action, object? item, int index, int oldIndex)
            => OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, item, index, oldIndex));

        private void OnCollectionChanged(NotifyCollectionChangedAction action, object? oldItem, object? newItem, int index)
            => OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, newItem, oldItem, index));

        #endregion
    }
}
