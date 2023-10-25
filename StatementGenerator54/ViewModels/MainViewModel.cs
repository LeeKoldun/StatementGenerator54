using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using ReactiveUI;
using System;
using System.Reactive;
using System.Threading.Tasks;

namespace StatementGenerator54.ViewModels;

public class MainViewModel : ViewModelBase
{
    public bool IsLeft { get; set; } = false;
    public bool IsCenter { get; set; } = true;
    public ReactiveCommand<Unit, Unit> Test { get; set; }
    public MainViewModel() {
        Test = ReactiveCommand.CreateFromTask(OpenFileDialog);
    }
    public async Task OpenFileDialog()
    {
        if(App.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            await TopLevel.GetTopLevel(desktop.MainWindow).StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Пока не работает но скоро заработает",
                AllowMultiple = false
            });
            IsCenter = false;
            IsLeft = true;
            this.RaisePropertyChanged();
            this.RaisePropertyChanged(nameof(IsLeft));
            this.RaisePropertyChanged(nameof(IsCenter));
        }
    }
}
