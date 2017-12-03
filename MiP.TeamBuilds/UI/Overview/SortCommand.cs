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
            if (!(parameter is CollectionViewSource collectionViewSource))
                return;

            var currentSort = collectionViewSource.SortDescriptions.First();
            var direction = ListSortDirection.Ascending;

            if (PropertyName == currentSort.PropertyName)
                direction = currentSort.Direction == ListSortDirection.Ascending ? ListSortDirection.Descending : ListSortDirection.Ascending;

            using (collectionViewSource.DeferRefresh())
            {
                collectionViewSource.SortDescriptions.Clear();
                collectionViewSource.SortDescriptions.Add(new SortDescription(PropertyName, direction));
                collectionViewSource.LiveSortingProperties.Clear();
                collectionViewSource.LiveSortingProperties.Add(PropertyName);
            }
        }

        public override object ProvideValue(IServiceProvider serviceProvider) => this;
    }
}
