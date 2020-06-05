using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Input;
using LocFileCreator.DataObjects;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.WindowsAPICodePack.Dialogs;
using ZimLabs.WpfBase;

namespace LocFileCreator.ViewModel
{
    /// <summary>
    /// Provides the logic for the main window (MVVM pattern)
    /// </summary>
    internal class MainWindowViewModel : ObservableObject
    {
        /// <summary>
        /// Contains the instance of the mah apps dialog coordinator
        /// </summary>
        private IDialogCoordinator _dialogCoordinator;

        /// <summary>
        /// Backing field for <see cref="SourceDirectory"/>
        /// </summary>
        private string _sourceDirectory;

        /// <summary>
        /// Gets or sets the path of the directory which contains the files
        /// </summary>
        public string SourceDirectory
        {
            get => _sourceDirectory;
            set
            {
                SetField(ref _sourceDirectory, value);

                if (string.IsNullOrEmpty(value))
                {
                    ShowDirErrorMessage = false;
                    SourceSelected = false;
                    return;
                }

                ShowDirErrorMessage = !Directory.Exists(value);
                SourceSelected = !ShowDirErrorMessage;
            }
        }

        /// <summary>
        /// Backing field for <see cref="LocPath"/>
        /// </summary>
        private string _locPath;

        /// <summary>
        /// Gets or sets the main "loc" path
        /// </summary>
        public string LocPath
        {
            get => _locPath;
            set
            {
                SetField(ref _locPath, value);
                CreateButtonEnabled = !string.IsNullOrEmpty(value) && FileList != null && FileList.Any();
            }
        }

        /// <summary>
        /// Backing field for <see cref="SourceSelected"/>
        /// </summary>
        private bool _sourceSelected;

        /// <summary>
        /// Gets or sets the value which indicates if the source directory was selected
        /// </summary>
        public bool SourceSelected
        {
            get => _sourceSelected;
            set => SetField(ref _sourceSelected, value);
        }

        /// <summary>
        /// Backing field for <see cref="ShowDirErrorMessage"/>
        /// </summary>
        private bool _showDirErrorMessage;

        /// <summary>
        /// Gets or sets the value which indicates if the error message should be shown
        /// </summary>
        public bool ShowDirErrorMessage
        {
            get => _showDirErrorMessage;
            set => SetField(ref _showDirErrorMessage, value);
        }

        /// <summary>
        /// Backing field for <see cref="FileList"/>
        /// </summary>
        private ObservableCollection<FileEntry> _fileList;

        /// <summary>
        /// Gets or sets the list with the found files
        /// </summary>
        public ObservableCollection<FileEntry> FileList
        {
            get => _fileList;
            set
            {
                SetField(ref _fileList, value);
                CreateButtonEnabled = !string.IsNullOrEmpty(LocPath) && value.Any();
            }
        }

        /// <summary>
        /// Backing field for <see cref="CreateButtonEnabled"/>
        /// </summary>
        private bool _createButtonEnabled;

        /// <summary>
        /// Gets or sets the value which indicates if the create button is enabled
        /// </summary>
        public bool CreateButtonEnabled
        {
            get => _createButtonEnabled;
            set => SetField(ref _createButtonEnabled, value);
        }

        /// <summary>
        /// Backing field for <see cref="ToolInfo"/>
        /// </summary>
        private string _toolInfo;

        /// <summary>
        /// Gets or sets the info of the tool
        /// </summary>
        public string ToolInfo
        {
            get => _toolInfo;
            set => SetField(ref _toolInfo, value);
        }

        /// <summary>
        /// Init the view model
        /// </summary>
        /// <param name="dialogCoordinator">The instance of the mah apps dialog coordinator</param>
        public void InitViewModel(IDialogCoordinator dialogCoordinator)
        {
            _dialogCoordinator = dialogCoordinator;

            ToolInfo = $"LocFileCreator - Version: {Assembly.GetExecutingAssembly().GetName().Version} - (c) 2020 by InvaderZim85";
        }

        /// <summary>
        /// The command to search for the source directory
        /// </summary>
        public ICommand BrowseCommand => new DelegateCommand(BrowseSource);

        /// <summary>
        /// The command to load the files which are stored in the source directory
        /// </summary>
        public ICommand LoadCommand => new DelegateCommand(LoadFiles);

        /// <summary>
        /// The command to create the loc files
        /// </summary>
        public ICommand CreateCommand => new DelegateCommand(CreateLocFile);

        /// <summary>
        /// The command to select all files
        /// </summary>
        public ICommand SelectAllCommand => new DelegateCommand(() => SetSelection(true));

        /// <summary>
        /// The command to select no file
        /// </summary>
        public ICommand SelectNoneCommand => new DelegateCommand(() => SetSelection(false));

        /// <summary>
        /// Opens a browse dialog to search for the source directory
        /// </summary>
        private void BrowseSource()
        {
            var dialog = new CommonOpenFileDialog
            {
                IsFolderPicker = true,
                Title = "Select the folder which contains the image files"
            };

            if (dialog.ShowDialog() != CommonFileDialogResult.Ok)
                return;

            SourceDirectory = dialog.FileName;
        }

        /// <summary>
        /// Loads the files of the source directory
        /// </summary>
        private async void LoadFiles()
        {
            var controller =
                await _dialogCoordinator.ShowProgressAsync(this, "Loading", "Please wait while loading the files...");
            controller.SetIndeterminate();

            try
            {
                var files = await Task.Run(() => Helper.LoadFiles(SourceDirectory));

                FileList = new ObservableCollection<FileEntry>(files);
            }
            catch (Exception ex)
            {
                await _dialogCoordinator.ShowMessageAsync(this, "Error",
                    $"An error has occured while loading the files of desired directory.\r\n\r\nMessage: {ex.Message}");
            }
            finally
            {
                await controller.CloseAsync();
            }
        }

        /// <summary>
        /// Creates the loc files for the selected entries
        /// </summary>
        private async void CreateLocFile()
        {
            if (string.IsNullOrEmpty(LocPath))
                return;

            var files = FileList.Where(w => w.Selected).ToList();

            if (!files.Any())
            {
                await _dialogCoordinator.ShowMessageAsync(this, "No file selected",
                    "You have to select at least one file.");
                return;
            }

            var controller =
                await _dialogCoordinator.ShowProgressAsync(this, "Creating",
                    "Please wait while creating the loc files...");
            controller.SetIndeterminate();

            try
            {
                var result = await Task.Run(() => Helper.CreateLocFile(files, SourceDirectory, LocPath));

                if (result)
                    await _dialogCoordinator.ShowMessageAsync(this, "Done", $"{files.Count} loc file(s) created.");
                else
                    await _dialogCoordinator.ShowMessageAsync(this, "Error",
                        "An error has occured while creating the loc files.");
            }
            catch (Exception ex)
            {
                await _dialogCoordinator.ShowMessageAsync(this, "Error",
                    $"An error has occured while creating the loc files.\r\n\r\nMessage: {ex.Message}");
            }
            finally
            {
                await controller.CloseAsync();
            }
        }

        /// <summary>
        /// Sets the selection
        /// </summary>
        /// <param name="select">true to set the selection to true, otherwise false</param>
        private void SetSelection(bool select)
        {
            foreach (var entry in FileList)
            {
                entry.Selected = select;
            }
        }
    }
}
