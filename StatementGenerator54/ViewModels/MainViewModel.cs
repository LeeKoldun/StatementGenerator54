using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using ReactiveUI;
using System;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;

namespace StatementGenerator54.ViewModels;

public class MainViewModel : ViewModelBase
{
    private readonly ObservableAsPropertyHelper<string> _studentsPath;
    private readonly ObservableAsPropertyHelper<string> _teachersPath;
    private readonly ObservableAsPropertyHelper<string> _tariffPath;
    private readonly ObservableAsPropertyHelper<string> _planPath;
    public string StudentsPath => _studentsPath.Value;
    public string TeachersPath => _teachersPath.Value;
    public string TariffPath => _tariffPath.Value;
    public string PlanPath => _planPath.Value;

    public bool IsLeft { get; set; } = false;

    public ReactiveCommand<Unit, string> StudentCommand { get; set; }
    public ReactiveCommand<Unit, string> TeacherCommand { get; set; }
    public ReactiveCommand<Unit, string> TariffCommand  { get; set; }
    public ReactiveCommand<Unit, string> PlanCommand    { get; set; }
    public ReactiveCommand<Unit, string> StartGenerationCommand { get; set; }

    public MainViewModel() {
        IsLeft= true;
        StudentCommand = ReactiveCommand.CreateFromTask(OpenFileDialog);
        TeacherCommand = ReactiveCommand.CreateFromTask(OpenFileDialog);
        TariffCommand = ReactiveCommand.CreateFromTask(OpenFileDialog);
        PlanCommand = ReactiveCommand.CreateFromTask(OpenFileDialog);
        StartGenerationCommand = ReactiveCommand.CreateFromTask(StartGeneration);

        _studentsPath = StudentCommand.ToProperty(this, x => x.StudentsPath);
        _teachersPath = TeacherCommand.ToProperty(this, x => x.TeachersPath);
        _tariffPath = TariffCommand.ToProperty(this, x => x.TariffPath);
        _planPath = PlanCommand.ToProperty(this, x => x.PlanPath);
    }
    public async Task<string> OpenFileDialog()
    {
        if(App.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var file = await TopLevel.GetTopLevel(desktop.MainWindow).StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Пока не работает но скоро заработает",
                AllowMultiple = false
            });
            if(file.Count == 0)
            {
                return string.Empty;
            }
            this.RaisePropertyChanged();
            return System.IO.Path.GetFullPath(file.First().Path.ToString());
        }
        throw new NotImplementedException();
    }
    public async Task<string> StartGeneration()
    {
        throw new NotImplementedException();
    }
}
