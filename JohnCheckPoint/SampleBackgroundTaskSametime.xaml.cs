using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Background;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace JohnCheckPoint
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SampleBackgroundTaskSametime : Page
    {
        public SampleBackgroundTaskSametime()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            foreach (var task in BackgroundTaskRegistration.AllTasks)
            {
                if (task.Value.Name == BackgroundTaskSample.SampleBackgroundTaskName)
                {
                    AttachProgressAndCompletedHandlers(task.Value);
                    BackgroundTaskSample.UpdateBackgroundTaskStatus(BackgroundTaskSample.SampleBackgroundTaskName, true);
                    break;
                }
            }

            UpdateUI();
        }

        /// <summary>
        /// Update the scenario UI.
        /// </summary>
        private async void UpdateUI()
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
           () =>
           {
               RegisterButton.IsEnabled = !BackgroundTaskSample.SampleBackgroundTaskSametimeRegistered;
               UnregisterButton.IsEnabled = BackgroundTaskSample.SampleBackgroundTaskSametimeRegistered;
               Progress.Text = BackgroundTaskSample.SampleBackgroundTaskSametimeProgress;
               Status.Text = BackgroundTaskSample.GetBackgroundTaskStatus(BackgroundTaskSample.SampleBackgroundTaskSametime);
           });
        }

        /// <summary>
        /// Attach progress and completed handers to a background task.
        /// </summary>
        /// <param name="task">The task to attach progress and completed handlers to.</param>
        private void AttachProgressAndCompletedHandlers(IBackgroundTaskRegistration task)
        {
            task.Progress += new BackgroundTaskProgressEventHandler(OnProgress);
            task.Completed += new BackgroundTaskCompletedEventHandler(OnCompleted);
        }

        /// <summary>
        /// Handle background task completion.
        /// </summary>
        /// <param name="task">The task that is reporting completion.</param>
        /// <param name="e">Arguments of the completion report.</param>
        private void OnCompleted(BackgroundTaskRegistration sender, BackgroundTaskCompletedEventArgs args)
        {
            UpdateUI();
        }

        /// <summary>
        /// Handle background task progress.
        /// </summary>
        /// <param name="task">The task that is reporting progress.</param>
        /// <param name="e">Arguments of the progress report.</param>
        private void OnProgress(BackgroundTaskRegistration sender, BackgroundTaskProgressEventArgs args)
        {
            var progress = "Progress: " + args.Progress + "%";
            BackgroundTaskSample.SampleBackgroundTaskProgress = progress;
            UpdateUI();
        }

        /// <summary>
        /// Unregister a SampleBackgroundTask.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void RegisterBackgroundTask(object sender, RoutedEventArgs e)
        {
            var task = BackgroundTaskSample.RegisterBackgroundTask(BackgroundTaskSample.SampleBackgroundTaskSametimeEntryPoint,
                                                                   BackgroundTaskSample.SampleBackgroundTaskSametime,
                                                                   new SystemTrigger(SystemTriggerType.TimeZoneChange, false),
                                                                   null);
            await task;
            AttachProgressAndCompletedHandlers(task.Result);
            UpdateUI();
        }

        /// <summary>
        /// Unregister a SampleBackgroundTask.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UnregisterBackgroundTask(object sender, RoutedEventArgs e)
        {
            BackgroundTaskSample.UnregisterBackgroundTasks(BackgroundTaskSample.SampleBackgroundTaskName);
            UpdateUI();
        }
    }
}