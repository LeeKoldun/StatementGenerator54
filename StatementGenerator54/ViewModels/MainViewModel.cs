using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using MsBox.Avalonia;
using MsBox.Avalonia.Base;
using MsBox.Avalonia.Dto;
using MsBox.Avalonia.Models;
using ReactiveUI;
using StatementGenerator54.ClassHelper;
using StatementGenerator54.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;

namespace StatementGenerator54.ViewModels;

public class MainViewModel : ViewModelBase
{
    public bool IsLeft { get; set; } = false;
    public bool IsCenter { get; set; } = true;

    public string StudentsPath { get; set; }
    public bool SrudentsLoaded { get; set; }
    public string TariffPath { get; set; }
    public bool TariffLoaded { get; set; }

    public List<Student> Students { get; set; }

    public ReactiveCommand<string, Unit> Test { get; set; }
    public MainViewModel() {
        Test = ReactiveCommand.CreateFromTask<string>(OpenFileDialog);
    }
    public async Task OpenFileDialog(string filePath)
    {
        if(App.Current!.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var pickResult = await TopLevel.GetTopLevel(desktop.MainWindow)!.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Выберите файл Excel",
                AllowMultiple = false,
                FileTypeFilter = new List<FilePickerFileType> { 
                    new FilePickerFileType("Excel") { 
                        Patterns = new[] { "*.xlsx" },
                    } 
                },
            });

            if(pickResult.Count < 1) return;

            switch(filePath) {
                case "student":
                    StudentsPath = pickResult.First().Path.AbsolutePath;
                    LoadStudents();
                break;
                case "tariff":
                    TariffPath = pickResult.First().Path.AbsolutePath;
                break;
                default:
                    throw new Exception("INVALID PARAMETER!");
            }

            IsCenter = false;
            IsLeft = true;
            this.RaisePropertyChanged();
            this.RaisePropertyChanged(nameof(IsLeft));
            this.RaisePropertyChanged(nameof(IsCenter));
        }
    }

    private async Task LoadStudents() {
        var mbox = GetPromtBox();
        var desk = App.Current!.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime;
        var result = await mbox.ShowWindowDialogAsync(desk.MainWindow);

        if(result != "Подтвердить" || string.IsNullOrEmpty(mbox.InputValue)) return;

        CmdRunner.Execute(CmdRunner.STUDENT_PARSER, StudentsPath, mbox.InputValue);

        if(!File.Exists("./student.json")) return;

        Students = new Context().Students("./student.json");
    }

    private IMsBox<string> GetPromtBox() {
        return MessageBoxManager.GetMessageBoxCustom(
            new MessageBoxCustomParams() {
                Icon = MsBox.Avalonia.Enums.Icon.Info,
                ContentHeader = "Еще кое-что...",
                ContentTitle = "Впишите название листа",
                InputParams = new InputParams {
                    Label = "Имя листа"
                },
                ButtonDefinitions = new[] {
                    new ButtonDefinition {
                        Name = "Подтвердить"
                    },
                    new ButtonDefinition {
                        Name = "Отменить"
                    },
                }
            }
        );
    }

    private IMsBox<string> showError() {
        return MessageBoxManager.GetMessageBoxCustom(
            new MessageBoxCustomParams() {
                Icon = MsBox.Avalonia.Enums.Icon.Error,
                ContentHeader = "Ошибка!",
                ContentTitle = "Не удалось загрузить данные. Убедитесь, что лист введён верно",
            }
        );
    }
}
