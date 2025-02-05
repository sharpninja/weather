﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Drizzle.Common.Helpers;
using Drizzle.ImageProcessing;
using Drizzle.ML.DepthEstimate;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#if WINDOWS_UWP
using Windows.ApplicationModel.Core;
using Windows.Storage;
using Windows.UI.Core;
#endif

namespace Drizzle.UI.UWP.ViewModels
{
    public partial class DepthEstimateViewModel : ObservableObject
    {
        private readonly IDepthEstimate depthEstimate;
        private readonly IDownloadUtil downloader;

        public string DepthAssetDir { get; private set; }
        public event EventHandler OnRequestClose;

        private readonly string modelPath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "ML", "midas", "model.onnx");

        public DepthEstimateViewModel(IDepthEstimate depthEstimate, IDownloadUtil downloader)
        {
            this.depthEstimate = depthEstimate;
            this.downloader = downloader;

            IsModelExists = CheckModel();
            _canRunCommand = IsModelExists && SelectedImage is not null;
            RunCommand.NotifyCanExecuteChanged();
        }

        [ObservableProperty]
        private bool isModelExists;

        [ObservableProperty]
        private bool isRunning;

        [ObservableProperty]
        private string errorText;

        [ObservableProperty]
        private string backgroundImage;

        [ObservableProperty]
        private string previewImage;

        [ObservableProperty]
        private float modelDownloadProgress;

        [ObservableProperty]
        private string modelDownloadProgressText = "--/-- MB";

        private string _selectedImage;
        public string SelectedImage
        {
            get => _selectedImage;
            set
            {
                SetProperty(ref _selectedImage, value);
                BackgroundImage = value;
                PreviewImage = value;
            }
        }

        private bool _canRunCommand = false;
        private RelayCommand _runCommand;
        public RelayCommand RunCommand => _runCommand ??= new RelayCommand(async () => await CreateDepthAsset(), () => _canRunCommand);

        private bool _canDownloadModelCommand = true;
        private RelayCommand _downloadModelCommand;
        public RelayCommand DownloadModelCommand => _downloadModelCommand ??= new RelayCommand(async () => await DownloadModel(), () => _canDownloadModelCommand);

        private bool _canCancelCommand = true;
        private RelayCommand _cancelCommand;
        public RelayCommand CancelCommand => _cancelCommand ??= new RelayCommand(CancelOperations, () => _canCancelCommand);

        private async Task CreateDepthAsset()
        {
            var localFolder = ApplicationData.Current.LocalFolder;
            var cacheFolder = await localFolder.CreateFolderAsync("Backgrounds", CreationCollisionOption.OpenIfExists);
            var depthCacheFolder = await cacheFolder.CreateFolderAsync("Depth", CreationCollisionOption.OpenIfExists);
            var destinationFolder = await depthCacheFolder.CreateFolderAsync(Path.GetFileNameWithoutExtension(SelectedImage), CreationCollisionOption.GenerateUniqueName);
            var inputImageFile = await StorageFile.GetFileFromPathAsync(SelectedImage);
            var depthImagePath = Path.Combine(destinationFolder.Path, "depth.jpg");
            var inputImageCopyPath = Path.Combine(destinationFolder.Path, "image.jpg");

            try
            {
                IsRunning = true;
                _canRunCommand = false;
                RunCommand.NotifyCanExecuteChanged();
                _canCancelCommand = false;
                CancelCommand.NotifyCanExecuteChanged();

                await Task.Run(async () =>
                {
                    using var inputImage = Image.Load(inputImageFile.Path);
                    //Resize input for performance and memory
                    if (inputImage.Width > 3840 || inputImage.Height > 3840)
                    {
                        //Fit the image within aspect ratio, if width > height = 3840x.. else ..x3840
                        inputImage.Mutate(x =>
                        {
                            x.Resize(new ResizeOptions()
                            {
                                Size = new Size(3840, 3840),
                                Mode = ResizeMode.Max
                            });
                        });
                    }

                    if (!modelPath.Equals(depthEstimate.ModelPath, StringComparison.Ordinal))
                        depthEstimate.LoadModel(modelPath);
                    var depthOutput = depthEstimate.Run(inputImageFile.Path);
                    //Resize depth to same size as input
                    using var depthImage = ImageUtil.FloatArrayToImage(depthOutput.Depth, depthOutput.Width, depthOutput.Height);
                    depthImage.Mutate(x =>
                    {
                        x.Resize(new ResizeOptions()
                        {
                            Mode = ResizeMode.Stretch,
                            Size = new Size(inputImage.Width, inputImage.Height)
                        });
                    });
                    await depthImage.SaveAsJpegAsync(depthImagePath);
                    await inputImage.SaveAsJpegAsync(inputImageCopyPath);
                });

                // Preview output to user
                await Task.Delay(500);
                PreviewImage = depthImagePath;
                await Task.Delay(1500);

                DepthAssetDir = destinationFolder.Path;
                // Close dialog
                OnRequestClose?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                ErrorText = $"Error: {ex.Message}";
                await destinationFolder.DeleteAsync();
            }
            finally
            {
                IsRunning = false;
                _canCancelCommand = true;
                CancelCommand.NotifyCanExecuteChanged();
            }
        }

        private async Task DownloadModel()
        {
            try
            {
                _canDownloadModelCommand = false;
                DownloadModelCommand.NotifyCanExecuteChanged();

                var uri = new Uri("https://github.com/rocksdanister/lively-ml-models/releases/download/v1.0.0.0/model.onnx");
                var localFolder = ApplicationData.Current.LocalFolder;
                var machineLearningFolder = await localFolder.CreateFolderAsync("ML", CreationCollisionOption.OpenIfExists);
                var midasFolder = await machineLearningFolder.CreateFolderAsync("Midas", CreationCollisionOption.OpenIfExists);
                var downloadPath = Path.Combine(midasFolder.Path, "model.onnx");
                downloader.DownloadProgressChanged += async(s, e) =>
                {
                    await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        ModelDownloadProgressText = $"{e.DownloadedSize}/{e.TotalSize} MB";
                        ModelDownloadProgress = (float)e.Percentage;
                    });
                };
                downloader.DownloadFileCompleted += async(s, success) =>
                {
                    await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        if (success)
                        {                 
                            IsModelExists = CheckModel();
                            BackgroundImage = IsModelExists ? SelectedImage : BackgroundImage;

                            _canRunCommand = IsModelExists;
                            RunCommand.NotifyCanExecuteChanged();
                        }
                        else
                        {
                            ErrorText = $"Error: Download failed.";
                        }
                    });
                };

                await downloader.DownloadFile(uri, downloadPath);
            }
            catch (Exception ex)
            {
                ErrorText = $"Error: {ex.Message}";
            }
            //finally
            //{
            //    _canDownloadModelCommand = true;
            //    DownloadModelCommand.NotifyCanExecuteChanged();
            //}
        }

        private void CancelOperations()
        {
            downloader?.Cancel();
        }

        private bool CheckModel() => File.Exists(modelPath);
    }
}
