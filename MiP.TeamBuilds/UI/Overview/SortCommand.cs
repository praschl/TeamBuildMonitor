using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Input;
using System;
using System.Linq;
using System.Windows.Markup;

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

            var currentSort = collectionView.SortDescriptions.First();
            var direction = ListSortDirection.Ascending;

            if (PropertyName == currentSort.PropertyName)
                direction = currentSort.Direction == ListSortDirection.Ascending ? ListSortDirection.Descending : ListSortDirection.Ascending;

            using (collectionView.DeferRefresh())
            {
                collectionView.SortDescriptions.Clear();
                collectionView.SortDescriptions.Add(new SortDescription(PropertyName, direction));
                collectionView.LiveSortingProperties.Clear();
                collectionView.LiveSortingProperties.Add(PropertyName);
            }
        }

        public override object ProvideValue(IServiceProvider serviceProvider) => this;
    }
}
