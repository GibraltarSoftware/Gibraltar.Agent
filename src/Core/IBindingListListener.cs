
using System;
using System.Collections.Generic;
using System.Text;

namespace Gibraltar
{
    /// <summary>
    /// Interface allowing an object to be notified of changes to a FilteredBindingList
    /// </summary>
    /// <typeparam name="T">The type of item in the FilteredBindingList</typeparam>
// ReSharper disable TypeParameterCanBeVariant
    public interface IBindingListListener<T>
// ReSharper restore TypeParameterCanBeVariant
    {
        /// <summary>
        /// Called before the first of a sequence of changes
        /// </summary>
        void BeginUpdate();

        /// <summary>
        /// Called after the last of a sequence of changes
        /// </summary>
        void EndUpdate();

        /// <summary>
        /// Called to indicate that the list is changing entirely
        /// </summary>
        void Reset();

        /// <summary>
        /// Called during a sequence of updates when an item is added to the list
        /// </summary>
        void Add(T item);

        /// <summary>
        /// Called during a sequence of updates when an item is updated that's in the list already
        /// </summary>
        void Change(T item);

        /// <summary>
        /// Called during a sequence of updates when an item is removed from the list
        /// </summary>
        void Remove(T item);
    }
}
