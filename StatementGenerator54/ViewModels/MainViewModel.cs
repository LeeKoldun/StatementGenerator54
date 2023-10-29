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

    public string SelectedGroup { get; set; } = "Группа";
    public string SelectedTeacher { get; set; } = "Преподаватель";
    public string SelectedSubject { get; set; } = "Предмет";

    public string StudentsPath { get; set; } = "";
    public bool StudentsLoaded { get; set; } = false;
    public string TariffPath { get; set; } = "";
    public bool TariffLoaded { get; set; } = false;

    public List<string> GroupsList { get; set; } = new List<string>{ };
    public List<string> TeachersList { get; set; } = new List<string> { };
    public List<string> SubjectsList { get; set; } = new List<string> { };

    public List<Student> Students { get; set; } = new List<Student> { };
    public List<Teacher> Teachers { get; set; } = new List<Teacher> { };

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

            bool? success;
            switch(filePath) {
                case "student":
                    StudentsPath = pickResult.First().Path.AbsolutePath;
                    success = await LoadData(CmdRunner.ParserType.StudentParser, "./students.json");

                    if(!await CheckSuccess(success)) return;
                    SetGroups();
                break;
                case "tariff":
                    TariffPath = pickResult.First().Path.AbsolutePath;
                    success = await LoadData(CmdRunner.ParserType.TeacherParser, "./teachers.json");

                    if(!await CheckSuccess(success)) return;
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

    private async Task<bool?> LoadData(CmdRunner.ParserType parserType, string jsonPath) {
        var mbox = GetPromtBox();
        var desk = App.Current!.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime;
        var result = await mbox.ShowWindowDialogAsync(desk!.MainWindow);

        if(result != "Подтвердить" || string.IsNullOrEmpty(mbox.InputValue)) return null;

        string filePath;
        if(parserType == CmdRunner.ParserType.StudentParser) filePath = StudentsPath;
        else filePath = TariffPath;

        if(File.Exists(jsonPath)) File.Delete(jsonPath);

        CmdRunner.Execute(parserType, filePath, mbox.InputValue);
        await Task.Delay(TimeSpan.FromSeconds(1));
        if(!File.Exists(jsonPath)) return false;

        if(parserType == CmdRunner.ParserType.StudentParser) {
            Students = new Context().Students(jsonPath);
        }
        else {
            Teachers = new Context().Teachers(jsonPath);
        }

        return true;
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

    private async Task<bool> CheckSuccess(bool? success) {
        if(success == null) return false;
        if(success == false) {
            var desk = App.Current!.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime;
            await ShowError().ShowAsPopupAsync(desk.MainWindow);
            return false;
        }

        return true;
    }

    private IMsBox<string> ShowError() {
        return MessageBoxManager.GetMessageBoxCustom(
            new MessageBoxCustomParams() {
                Icon = MsBox.Avalonia.Enums.Icon.Error,
                ContentHeader = "Ошибка!",
                ContentMessage = "Не удалось загрузить данные. Убедитесь, что имя листа введено верно",
                ButtonDefinitions = new [] {
                    new ButtonDefinition {
                        Name = "Ок",
                        IsCancel = true
                    }
                }
            }
        );
    }
}
