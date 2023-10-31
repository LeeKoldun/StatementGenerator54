using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using MsBox.Avalonia;
using MsBox.Avalonia.Base;
using MsBox.Avalonia.Dto;
using MsBox.Avalonia.Models;
using Newtonsoft.Json;
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

    public string SelectedGroup { get; set; } = "";

    private string _selectedTeacher = "";
    public string SelectedTeacher {
        get => _selectedTeacher; 
        set {
            _selectedTeacher = value;
            this.RaisePropertyChanged(nameof(SelectedTeacher));

            if(!string.IsNullOrWhiteSpace(SelectedTeacher) && UseFilters) SortByTeacher();
        }
    }
    private string _selectedTeacher2 = "";
    public string SelectedTeacher2 {
        get => _selectedTeacher2; 
        set {
            _selectedTeacher2 = value;
            this.RaisePropertyChanged(nameof(SelectedTeacher2));

            if(!string.IsNullOrWhiteSpace(SelectedTeacher2) && UseFilters) SortByTeacher(true);
        }
    }


    private string _selectedSubject = "";
    public string SelectedSubject {
        get => _selectedSubject; 
        set {
            _selectedSubject = value;
            this.RaisePropertyChanged(nameof(SelectedSubject));

            if(!string.IsNullOrWhiteSpace(SelectedSubject) && UseFilters) SortBySubject();
        }
    }
    private string _selectedSubject2 = "";
    public string SelectedSubject2 {
        get => _selectedSubject2; 
        set {
            _selectedSubject2 = value;
            this.RaisePropertyChanged(nameof(SelectedSubject2));

            if(!string.IsNullOrWhiteSpace(SelectedSubject2) && UseFilters) SortBySubject(true);
        }
    }


    public WordHelper.StatementType? SelectedStatementType { get; set; } = null;
    public bool[] StatementValues { get; set; } = { false, false, false, false };

    public string StudentsPath { get; set; } = "";
    public bool StudentsLoaded { get => Students.Count > 0; }
    public string TariffPath { get; set; } = "";
    public bool TariffLoaded { get => Teachers.Count > 0; }

    public bool IsComplexExam { get => SelectedStatementType == WordHelper.StatementType.ComplexExam; }
    public bool UseFilters { get; set; } = true;

    public List<string> GroupsList { get; set; } = new List<string> { };
    public List<string> TeachersList { get; set; } = new List<string> { };
    public List<string> TeachersList2 { get; set; } = new List<string> { };
    public List<string> SubjectsList { get; set; } = new List<string> { };
    public List<string> SubjectsList2 { get; set; } = new List<string> { };

    public List<Student> Students { get; set; } = new List<Student> { };
    public List<Teacher> Teachers { get; set; } = new List<Teacher> { };

    public ReactiveCommand<string, Unit> Test { get; set; }
    public ReactiveCommand<string, Unit> StatementSelect{ get; set; }
    public ReactiveCommand<Unit, Unit> ResetFilters { get; set; }
    public ReactiveCommand<Unit, Unit> GenerateStatement{ get; set; }
    public MainViewModel() {
        Test = ReactiveCommand.CreateFromTask<string>(OpenFileDialog);
        StatementSelect = ReactiveCommand.Create<string>(ChooseStatement);
        GenerateStatement = ReactiveCommand.CreateFromTask(Generate);
        ResetFilters = ReactiveCommand.Create(Reset);

        new Task(async () => {
            await Task.Delay(1000);
            LoadAndCheckSavedPaths();
        }).Start();
    }

    public void LoadAndCheckSavedPaths() {
        var paths = Context.LoadSavePaths("save_paths.json");

        if(string.IsNullOrWhiteSpace(paths.StudentsXlsxPath) && string.IsNullOrWhiteSpace(paths.TariffXlsxPath)) return;

        StudentsPath = paths.StudentsXlsxPath;
        TariffPath = paths.TariffXlsxPath;

        if(!string.IsNullOrWhiteSpace(StudentsPath)) LoadStudentsJson(paths);
        if(!string.IsNullOrWhiteSpace(TariffPath)) LoadTeachersJson(paths);

        IsCenter = false;
        IsLeft = true;
        this.RaisePropertyChanged(nameof(IsLeft));
        this.RaisePropertyChanged(nameof(IsCenter));
        UpdateLoadedState();
    }

    public void UpdateLoadedState() {
        this.RaisePropertyChanged(nameof(StudentsLoaded));
        this.RaisePropertyChanged(nameof(TariffLoaded));
    }

    public void Reset() {
        SelectedTeacher = "";
        SelectedSubject = "";
        
        SelectedTeacher2 = "";
        SelectedSubject2 = "";

        SetTeachers();
        SetSubjects();
    }

    public async Task Generate() {
        var desk = App.Current!.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime;

        if(
            string.IsNullOrWhiteSpace(SelectedSubject) || 
            string.IsNullOrWhiteSpace(SelectedGroup) || 
            string.IsNullOrWhiteSpace(SelectedTeacher)
        ) {
            await ShowError("Выберите все данные!").ShowAsPopupAsync(desk!.MainWindow);
            return;
        }

        if(SelectedStatementType is null) {
            await ShowError("Выберите тип ведомости!").ShowAsPopupAsync(desk!.MainWindow);
            return;
        }

        var storage = TopLevel.GetTopLevel(desk!.MainWindow)!.StorageProvider;

        string teacher2 = "";
        string subject2 = "";

        string statementName;
        switch(SelectedStatementType) {
            case WordHelper.StatementType.Exam:
                statementName = "Экзаменационная ведомость";
            break;
            case WordHelper.StatementType.ComplexExam:
                statementName = "Экзаменационная ведомость (комплексный экзамен)";
                teacher2 = SelectedTeacher2;
                subject2 = SelectedSubject2;
            break;
            case WordHelper.StatementType.Coursework:
                statementName = "Курсовая ведомость";
            break;
            case WordHelper.StatementType.Test:
                statementName = "Зачётная ведомость";
            break;
            case WordHelper.StatementType.ComplexTest:
                statementName = "Зачётная ведомость (комплексный зачёт)";
                teacher2 = SelectedTeacher2;
                subject2 = SelectedSubject2;
            break;

            default:
            throw new Exception("Invalid statement type!");
        }
        var path = await storage.SaveFilePickerAsync(new FilePickerSaveOptions {
            DefaultExtension = "doc",
            ShowOverwritePrompt = true,
            Title = "Выберите путь сохранения документа",
            SuggestedFileName = $"{statementName} {SelectedGroup} {SelectedTeacher} {SelectedSubject} {DateTime.Now.ToString("yyyy")}"
        });
        if(path is null) return;

        string savePath = path.Path.AbsolutePath;

        var filteredStudents = Students
            .Where(e => e.Group == SelectedGroup)
            .OrderBy(e => e.FullName)
            .ToList();
        var stud = filteredStudents.First();
        var teacher = Teachers.First(e => (e.FullName == SelectedTeacher) && (e.FullSubjectName == SelectedSubject));

        WordHelper.TextChanger(
            savePath, 
            teacher.FullName, 
            teacher.FullSubjectName, 
            stud.Group, 
            stud.Specialization, 
            stud.Course, 
            filteredStudents,
            (WordHelper.StatementType)SelectedStatementType,
            teacher2,
            subject2
        );
    }

    public void ChooseStatement(string type) {
        switch(type) {
            case "exam":
                SelectedStatementType = WordHelper.StatementType.Exam;
            break;
            case "complexExam":
                SelectedStatementType = WordHelper.StatementType.ComplexExam;
            break;
            case "course":
                SelectedStatementType = WordHelper.StatementType.Coursework;
            break;
            case "test":
                SelectedStatementType = WordHelper.StatementType.Test;
            break;

            default:
                throw new Exception("Invalid statement type!");
        }

        if(type != "complex") {
            SelectedTeacher2 = "";
            SelectedSubject2 = "";

            this.RaisePropertyChanged(nameof(SelectedTeacher2));
            this.RaisePropertyChanged(nameof(SelectedSubject2));
        }
        this.RaisePropertyChanged(nameof(IsComplexExam));
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
                    LoadStudentsJson(pickResult.First().Path.AbsolutePath);
                break;
                case "tariff":
                    LoadTeachersJson(pickResult.First().Path.AbsolutePath);
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

    public async void LoadStudentsJson(string path) {
        StudentsPath = path;
        bool? success = await LoadData(CmdRunner.ParserType.StudentParser, "./students.json");

        if(!await CheckSuccess(success)) return;

        WriteJsonSavePath("save_paths.json");
        SetGroups();
        UpdateLoadedState();
    }
    public void LoadStudentsJson(SavePaths paths) {
        StudentsPath = paths.StudentsXlsxPath;
        _studentsXlsxLists = paths.StudentsLists;

        CmdRunner.Execute(CmdRunner.ParserType.StudentParser, StudentsPath, string.Join(';', _studentsXlsxLists));
        Students = Context.Students("students.json");

        SetGroups();
        UpdateLoadedState();
    }

    public async void LoadTeachersJson(string path) {
        TariffPath = path;
        bool? success = await LoadData(CmdRunner.ParserType.TeacherParser, "./teachers.json");

        if(!await CheckSuccess(success)) return;

        WriteJsonSavePath("save_paths.json");
        SetTeachers();
        SetSubjects();
        UpdateLoadedState();
    }
    public void LoadTeachersJson(SavePaths paths) {
        TariffPath = paths.TariffXlsxPath;
        _tariffsXlsxLists = paths.TariffsLists;

        CmdRunner.Execute(CmdRunner.ParserType.TeacherParser, TariffPath, string.Join(';', _tariffsXlsxLists));
        Teachers = Context.Teachers("teachers.json");

        SetTeachers();
        SetSubjects();
        UpdateLoadedState();
    }


    private List<string> _studentsXlsxLists = new List<string> { };
    private List<string> _tariffsXlsxLists = new List<string> { };
    public void WriteJsonSavePath(string jsonSavePath) {
        var paths = new SavePaths {
            StudentsXlsxPath = StudentsPath,
            TariffXlsxPath = TariffPath,
            StudentsLists = _studentsXlsxLists,
            TariffsLists = _tariffsXlsxLists
        };

        string json = JsonConvert.SerializeObject(paths);
        using(var sw = new StreamWriter(jsonSavePath, false)) {
            sw.Write(json);
        }
    }


    private void SortByTeacher(bool isSecond = false) {
        if(!isSecond) {
            SubjectsList = Teachers
                .Where(e => e.FullName == SelectedTeacher)
                .Select(e => e.FullSubjectName)
                .Distinct()
                .ToList();
            this.RaisePropertyChanged(nameof(SubjectsList));
        }
        else {
            SubjectsList2 = Teachers
                .Where(e => e.FullName == SelectedTeacher2)
                .Select(e => e.FullSubjectName)
                .Distinct()
                .ToList();
            this.RaisePropertyChanged(nameof(SubjectsList2));
        }
    }

    private void SortBySubject(bool isSecond = false) {
        if(!isSecond) {
            TeachersList = Teachers
                .Where(e => e.FullSubjectName == SelectedSubject)
                .Select(e => e.FullName)
                .Distinct()
                .ToList();
            this.RaisePropertyChanged(nameof(TeachersList));
        }
        else {
            TeachersList2 = Teachers
                .Where(e => e.FullSubjectName == SelectedSubject2)
                .Select(e => e.FullName)
                .Distinct()
                .ToList();
            this.RaisePropertyChanged(nameof(TeachersList2));
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
        TeachersList = TeachersList2 = Teachers.DistinctBy(e => e.FullName)
            .Select(e => e.FullName)
            .Order()
            .ToList();
        this.RaisePropertyChanged(nameof(TeachersList));
        this.RaisePropertyChanged(nameof(TeachersList2));
    }

    private void SetSubjects() {
        SubjectsList = SubjectsList2 = Teachers.DistinctBy(e => e.FullSubjectName)
            .Select(e => e.FullSubjectName)
            .Order()
            .ToList();
        this.RaisePropertyChanged(nameof(SubjectsList));
        this.RaisePropertyChanged(nameof(SubjectsList2));
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
            Students = Context.Students(jsonPath);
            _studentsXlsxLists = mbox.InputValue.Split(";").ToList();
        }
        else {
            Teachers = Context.Teachers(jsonPath);
            _tariffsXlsxLists = mbox.InputValue.Split(";").ToList();
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
            await ShowError("Не удалось загрузить данные. Убедитесь, что имя листа введено верно").ShowAsPopupAsync(desk!.MainWindow);
            return false;
        }

        return true;
    }

    private IMsBox<string> ShowError(string message) {
        return MessageBoxManager.GetMessageBoxCustom(
            new MessageBoxCustomParams() {
                Icon = MsBox.Avalonia.Enums.Icon.Error,
                ContentHeader = "Ошибка!",
                ContentMessage = message,
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
