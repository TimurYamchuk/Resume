using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using System.Runtime.CompilerServices;

namespace Resume
{
    public class MainVM : INotifyPropertyChanged
    {
        private const string ResumesFileName = "DataResumes.txt";

        private string _userNameInput = string.Empty;
        private string _userAgeInput = string.Empty;
        private string _userAddressInput = string.Empty;
        private string _userEmailInput = string.Empty;
        private string _chosenMaritalStatus = string.Empty;

        private bool _skillCSharp, _skillJava, _skillPython, _skillSql, _skillHtml, _skillCss, _skillGit, _skillDocker, _skillScrum, _skillEnglish;

        private ResumeModel? _selectedResume;

        public ObservableCollection<ResumeModel> AllResumes { get; } = new();
        public ObservableCollection<string> MaritalStatuses { get; } = new()
        {
            "Холост(а)",
            "Женат/Замужем",
            "Разведён(а)"
        };

        public ResumeModel? SelectedResume
        {
            get => _selectedResume;
            set { _selectedResume = value; OnPropertyChanged(); }
        }

        public string UserNameInput
        {
            get => _userNameInput;
            set
            {
                if (value != null && !Regex.IsMatch(value, @"^[a-zA-Zа-яА-ЯёЁ\s\-]*$")) return;
                _userNameInput = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CanAddResume));
            }
        }

        public string UserAgeInput
        {
            get => _userAgeInput;
            set
            {
                if (!Regex.IsMatch(value ?? string.Empty, @"^\d{0,2}$")) return;
                _userAgeInput = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CanAddResume));
            }
        }

        public string UserAddressInput
        {
            get => _userAddressInput;
            set { _userAddressInput = value; OnPropertyChanged(); OnPropertyChanged(nameof(CanAddResume)); }
        }

        public string UserEmailInput
        {
            get => _userEmailInput;
            set { _userEmailInput = value; OnPropertyChanged(); OnPropertyChanged(nameof(CanAddResume)); }
        }

        public string ChosenMaritalStatus
        {
            get => _chosenMaritalStatus;
            set { _chosenMaritalStatus = value; OnPropertyChanged(); OnPropertyChanged(nameof(CanAddResume)); }
        }

        public bool SkillCSharp { get => _skillCSharp; set { _skillCSharp = value; OnPropertyChanged(); } }
        public bool SkillJava { get => _skillJava; set { _skillJava = value; OnPropertyChanged(); } }
        public bool SkillPython { get => _skillPython; set { _skillPython = value; OnPropertyChanged(); } }
        public bool SkillSql { get => _skillSql; set { _skillSql = value; OnPropertyChanged(); } }
        public bool SkillHtml { get => _skillHtml; set { _skillHtml = value; OnPropertyChanged(); } }
        public bool SkillCss { get => _skillCss; set { _skillCss = value; OnPropertyChanged(); } }
        public bool SkillGit { get => _skillGit; set { _skillGit = value; OnPropertyChanged(); } }
        public bool SkillDocker { get => _skillDocker; set { _skillDocker = value; OnPropertyChanged(); } }
        public bool SkillScrum { get => _skillScrum; set { _skillScrum = value; OnPropertyChanged(); } }
        public bool SkillEnglish { get => _skillEnglish; set { _skillEnglish = value; OnPropertyChanged(); } }

        public bool CanAddResume =>
            !string.IsNullOrWhiteSpace(UserNameInput) &&
            int.TryParse(UserAgeInput, out var ageNum) && ageNum is >= 1 and <= 99 &&
            !string.IsNullOrEmpty(ChosenMaritalStatus) &&
            !string.IsNullOrWhiteSpace(UserAddressInput) &&
            !string.IsNullOrWhiteSpace(UserEmailInput);

        public ICommand AddResumeCmd { get; }
        public ICommand ClearInputsCmd { get; }
        public ICommand ShowResumeCmd { get; }
        public ICommand DeleteSelectedCmd { get; }

        public MainVM()
        {
            AddResumeCmd = new RelayCmd(_ => AddResume(), _ => CanAddResume);
            ClearInputsCmd = new RelayCmd(_ => ClearInputs());
            ShowResumeCmd = new RelayCmd(_ => ShowSelectedResume(), _ => SelectedResume != null);
            DeleteSelectedCmd = new RelayCmd(_ => DeleteSelectedResume(), _ => SelectedResume != null);

            LoadFromFile();
        }

        private void AddResume()
        {
            try
            {
                int.TryParse(UserAgeInput, out var age);
                var skillList = CollectSkills();

                var newResume = new ResumeModel
                {
                    UserFullName = UserNameInput.Trim(),
                    UserAge = age,
                    MaritalStat = ChosenMaritalStatus,
                    UserAddress = UserAddressInput.Trim(),
                    UserEmail = UserEmailInput.Trim(),
                    UserSkills = string.Join(",", skillList)
                };

                AllResumes.Add(newResume);
                AppendToFile(newResume);
                ClearInputs();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении резюме: {ex.Message}");
            }
        }

        private void DeleteSelectedResume()
        {
            if (SelectedResume == null) return;
            var confirm = MessageBox.Show($"Удалить '{SelectedResume.UserFullName}' из базы?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (confirm == MessageBoxResult.Yes)
            {
                AllResumes.Remove(SelectedResume);
                RewriteAllFile();
                SelectedResume = null;
            }
        }

        private void ShowSelectedResume()
        {
            if (SelectedResume == null) return;
            var dlg = new DetailWindow(SelectedResume)
            {
                Owner = Application.Current.MainWindow
            };
            dlg.ShowDialog();
        }

        private void ClearInputs()
        {
            UserNameInput = string.Empty;
            UserAgeInput = string.Empty;
            UserAddressInput = string.Empty;
            UserEmailInput = string.Empty;
            ChosenMaritalStatus = string.Empty;

            SkillCSharp = false;
            SkillJava = false;
            SkillPython = false;
            SkillSql = false;
            SkillHtml = false;
            SkillCss = false;
            SkillGit = false;
            SkillDocker = false;
            SkillScrum = false;
            SkillEnglish = false;
        }

        private void LoadFromFile()
        {
            try
            {
                if (!File.Exists(ResumesFileName)) return;

                var lines = File.ReadAllLines(ResumesFileName);
                AllResumes.Clear();
                foreach (var line in lines)
                {
                    var resume = ResumeModel.FromLine(line);
                    if (resume != null)
                        AllResumes.Add(resume);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки из файла: {ex.Message}");
            }
        }

        private void AppendToFile(ResumeModel resume)
        {
            try
            {
                using var writer = new StreamWriter(ResumesFileName, true);
                writer.WriteLine(resume.ToString());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка записи в файл: {ex.Message}");
            }
        }

        private void RewriteAllFile()
        {
            try
            {
                using var writer = new StreamWriter(ResumesFileName, false);
                foreach (var resume in AllResumes)
                {
                    writer.WriteLine(resume.ToString());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка перезаписи файла: {ex.Message}");
            }
        }

        private List<string> CollectSkills()
        {
            var skills = new List<string>();
            if (SkillCSharp) skills.Add("CSharp");
            if (SkillJava) skills.Add("Java");
            if (SkillPython) skills.Add("Python");
            if (SkillSql) skills.Add("SQL");
            if (SkillHtml) skills.Add("HTML");
            if (SkillCss) skills.Add("CSS");
            if (SkillGit) skills.Add("Git");
            if (SkillDocker) skills.Add("Docker");
            if (SkillScrum) skills.Add("Scrum");
            if (SkillEnglish) skills.Add("English");
            return skills;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
