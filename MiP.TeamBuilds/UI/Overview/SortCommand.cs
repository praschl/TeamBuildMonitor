﻿using System.Windows.Data;
using System.Windows.Input;
using System;
using System.Windows.Markup;
using System.ComponentModel;

namespace MiP.TeamBuilds.UI.Overview
{
    public class SortCommand : MarkupExtension, ICommand
    {
        public string PropertyName { get; set; }

        public event EventHandler CanExecuteChanged;
        public bool CanExecute(object parameter) => true;

        public void Execute(object parameter)
        {
            if (!(parameter is ListCollectionView collectionView))
                return;

            var comparer = collectionView.CustomSort as BuildInfoComparer;
            if (comparer == null)
                throw new InvalidOperationException("OverviewViewModel.BuildCollectionView must use a OverviewViewModel.BuildInfoComparer to compare items.");

            var direction = ListSortDirection.Ascending;

            if (PropertyName == comparer.PropertyName)
                direction = comparer.Direction == ListSortDirection.Ascending ? ListSortDirection.Descending : ListSortDirection.Ascending;

            comparer.PropertyName = PropertyName;
            comparer.Direction = direction;

            using (collectionView.DeferRefresh())
            {
                collectionView.LiveSortingProperties.Clear();
                collectionView.LiveSortingProperties.Add(PropertyName);
            }
        }

        public override object ProvideValue(IServiceProvider serviceProvider) => this;
    }
}
