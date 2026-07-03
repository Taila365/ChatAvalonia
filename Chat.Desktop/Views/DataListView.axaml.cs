using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Chat.Desktop.ViewModels;

namespace Chat.Desktop;

public partial class DataListView : UserControl
{
    public DataListView()
    {
        InitializeComponent();
        DataContext = Furion.App.GetRequiredService<DataListViewModel>();
    }

    protected override async void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

        var vm = DataContext as DataListViewModel;
        await vm.LoadDataAsync();
    }
}