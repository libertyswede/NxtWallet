using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using NxtWallet.ViewModel;
using NxtWallet.Core.Models;

namespace NxtWallet.Controls
{
    public class TransactionListView : ListView
    {
        public static readonly DependencyProperty OddRowBackgroundProperty =
            DependencyProperty.Register("OddRowBackground", typeof(Brush), typeof(TransactionListView), null);
        public Brush OddRowBackground
        {
            get { return (Brush)GetValue(OddRowBackgroundProperty); }
            set { SetValue(OddRowBackgroundProperty, value); }
        }

        public static readonly DependencyProperty EvenRowBackgroundProperty =
            DependencyProperty.Register("EvenRowBackground", typeof(Brush), typeof(TransactionListView), null);
        public Brush EvenRowBackground
        {
            get { return (Brush)GetValue(EvenRowBackgroundProperty); }
            set { SetValue(EvenRowBackgroundProperty, value); }
        }

        public static readonly DependencyProperty UnconfirmedForegroundProperty =
            DependencyProperty.Register("UnconfirmedForeground", typeof(Brush), typeof(TransactionListView), null);
        public Brush UnconfirmedForeground
        {
            get { return (Brush)GetValue(UnconfirmedForegroundProperty); }
            set { SetValue(UnconfirmedForegroundProperty, value); }
        }

        public TransactionListView()
        {
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            var source = (ObservableCollection<Transaction>)ItemsSource;
            source.CollectionChanged += Source_CollectionChanged;

            foreach (var viewModelTransaction in source)
            {
                viewModelTransaction.PropertyChanged += TransactionOnPropertyChanged;
            }
        }

        private void OnUnloaded(object sender, RoutedEventArgs routedEventArgs)
        {
            var source = (ObservableCollection<Transaction>)ItemsSource;
            source.CollectionChanged -= Source_CollectionChanged;

            foreach (var viewModelTransaction in source)
            {
                viewModelTransaction.PropertyChanged -= TransactionOnPropertyChanged;
            }
        }

        private void Source_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (var newTransaction in e.NewItems?.Cast<Transaction>())
                {
                    newTransaction.PropertyChanged += TransactionOnPropertyChanged;
                }
            }
            if (e.OldItems != null)
            {
                foreach (var oldTransaction in e.OldItems.Cast<Transaction>())
                {
                    oldTransaction.PropertyChanged -= TransactionOnPropertyChanged;
                }
            }
        }

        private void TransactionOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            if (propertyChangedEventArgs.PropertyName.Equals(nameof(Transaction.IsConfirmed)))
            {
                var listViewItem = (ListViewItem)ContainerFromItem(sender);
                var index = IndexFromContainer(listViewItem);
                SetColors(listViewItem, index);
            }
        }

        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            base.PrepareContainerForItemOverride(element, item);
            var listViewItem = element as ListViewItem;
            if (listViewItem != null)
            {
                var index = IndexFromContainer(element);
                SetColors(listViewItem, index);

                for (index++; index < Items?.Count; index++)
                {
                    listViewItem = ContainerFromIndex(index) as ListViewItem;
                    if (listViewItem != null)
                        SetColors(listViewItem, index);
                }
            }
        }

        private void SetColors(ListViewItem listViewItem, int index)
        {
            var transaction = Items[index] as Transaction;
            SetBackgroundColor(listViewItem, index);
            SetForegroundColor(listViewItem, transaction);
        }

        private void SetBackgroundColor(ListViewItem listViewItem, int index)
        {
            listViewItem.Background = (index + 1) % 2 == 1 ? OddRowBackground : EvenRowBackground;
        }

        private void SetForegroundColor(ListViewItem listViewItem, Transaction transaction)
        {
            if (transaction != null && transaction.IsConfirmed)
            {
                listViewItem.Foreground = new SolidColorBrush(Colors.Black);
            }
            else
            {
                listViewItem.Foreground = UnconfirmedForeground;
            }
        }
    }
}
