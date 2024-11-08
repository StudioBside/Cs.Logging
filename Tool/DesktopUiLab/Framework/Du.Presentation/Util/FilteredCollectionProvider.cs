﻿namespace Du.Presentation.Util;

using System.Collections;
using System.Windows.Data;
using Du.Core.Interfaces;
using Shared.Interfaces;

/// <summary>
/// ViewModel에서는 Windows.Base.dll에 존재하는 ICollectionView을 참조하지 않으므로 ICollectionView를 대체하는 기능을 제공합니다.
/// </summary>
public sealed class FilteredCollectionProvider : IFilteredCollectionProvider
{
    IFilteredCollection<T> IFilteredCollectionProvider.Build<T>(IEnumerable<T> collection)
    {
        IList listType = collection as IList ?? collection.ToArray();
        return new FilteredCollection<T>(listType);
    }

    private sealed class FilteredCollection<T>(IList collection) : IFilteredCollection<T>
        where T : ISearchable
    {
        private readonly ListCollectionView view = new(collection);

        public IEnumerable List => this.view;
        public IEnumerable<T> TypedList => this.view.Cast<T>();
        public int SourceCount => collection.Count;
        public int FilteredCount => this.view.Count;

        public void Refresh(string searchKeyword, Predicate<T>? filter)
        {
            if (string.IsNullOrEmpty(searchKeyword) && filter is null)
            {
                this.view.Filter = null;
                return;
            }

            this.view.Filter = e =>
            {
                if (e is not T item)
                {
                    return false;
                }

                if (filter is not null && filter(item) == false)
                {
                    return false;
                }

                return item.IsTarget(searchKeyword);
            };
        }
    }
}
