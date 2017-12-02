using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Input;
using System;
using System.Linq;

namespace MiP.TeamBuilds.UI.Overview
{
    // TODO: refactor SortCommand: make injectable.
    public class SortCommand : ICommand
    {
        private readonly CollectionViewSource _collectionViewSource;
        private readonly Action _afterAction;

        public SortCommand(CollectionViewSource collectionViewSource, Action afterAction)
        {
            _collectionViewSource = collectionViewSource;
            _afterAction = afterAction;
        }

        public CollectionView CollectionView { get; set; }

        public event EventHandler CanExecuteChanged;
        public bool CanExecute(object parameter) => true;

        public void Execute(object parameter)
        {
            if (!(parameter is string newSort))
                return;

            var currentSort = _collectionViewSource.SortDescriptions.First();
            var direction = ListSortDirection.Ascending;

            if (newSort == currentSort.PropertyName)
                direction = currentSort.Direction == ListSortDirection.Ascending ? ListSortDirection.Descending : ListSortDirection.Ascending;

            using (_collectionViewSource.DeferRefresh())
            {
                _collectionViewSource.SortDescriptions.Clear();
                _collectionViewSource.SortDescriptions.Add(new SortDescription(newSort, direction));
                _collectionViewSource.LiveSortingProperties.Clear();
                _collectionViewSource.LiveSortingProperties.Add(newSort);
            }
            _afterAction();
        }
    }
}
