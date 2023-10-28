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

    public List<string> GroupsList { get; set; }
    public List<string> TeachersList { get; set; }
    public List<string> SubjectsList { get; set; }

    public List<Student> Students { get; set; }
    public List<Teacher> Teachers { get; set; }

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
                    await LoadData(CmdRunner.ParserType.StudentParser, "./student.json");
                    SetGroups();
                break;
                case "tariff":
                    TariffPath = pickResult.First().Path.AbsolutePath;
                    await LoadData(CmdRunner.ParserType.TeacherParser, "./teacher.json");
                    SetTeachers();
                    SetSubjects();
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

    private void SetGroups() {
        GroupsList = Students.DistinctBy(e => e.Group)
            .Select(e => e.Group)
            .Order()
            .ToList();
        this.RaisePropertyChanged(nameof(GroupsList));
    }

    private void SetTeachers() {
        TeachersList = Teachers.DistinctBy(e => e.FullName)
            .Select(e => e.FullName)
            .Order()
            .ToList();
        this.RaisePropertyChanged(nameof(TeachersList));
    }

    private void SetSubjects() {
        SubjectsList = Teachers.DistinctBy(e => e.FullSubjectName)
            .Select(e => e.FullSubjectName)
            .Order()
            .ToList();
        this.RaisePropertyChanged(nameof(SubjectsList));
    }

    private async Task LoadData(CmdRunner.ParserType parserType, string jsonPath) {
        var mbox = GetPromtBox();
        var desk = App.Current!.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime;
        var result = await mbox.ShowWindowDialogAsync(desk.MainWindow);

        if(result != "Подтвердить" || string.IsNullOrEmpty(mbox.InputValue)) return;

        string filePath;
        if(parserType == CmdRunner.ParserType.StudentParser) filePath = StudentsPath;
        else filePath = TariffPath;
        CmdRunner.Execute(parserType, filePath, mbox.InputValue);

        if(!File.Exists(jsonPath)) return;

        if(parserType == CmdRunner.ParserType.StudentParser) {
            Students = new Context().Students(jsonPath);
        }
        else {
            Teachers = new Context().Teachers(jsonPath);
        }
    }

    private IMsBox<string> GetPromtBox() {
        return MessageBoxManager.GetMessageBoxCustom(
            new MessageBoxCustomParams() {
                Icon = MsBox.Avalonia.Enums.Icon.Info,
                ContentTitle = "Еще кое-что...",
                ContentHeader = "Впишите название листа",
                ContentMessage = "Можно выбрать несколько листов, разделяя их имена символом \";\"\nПример: \"ВБ;ВБ точечники;Бюджет\"",
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

    private IMsBox<string> ShowError() {
        return MessageBoxManager.GetMessageBoxCustom(
            new MessageBoxCustomParams() {
                Icon = MsBox.Avalonia.Enums.Icon.Error,
                ContentHeader = "Ошибка!",
                ContentTitle = "Не удалось загрузить данные. Убедитесь, что лист введён верно",
            }
        );
    }
}
