using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Data.Xml.Dom;
using Windows.Storage;
using Windows.System.Threading;
using Windows.UI.Notifications;

namespace MyBackGroundTask
{
    public sealed class SampleBackgroundSametime : IBackgroundTask
    {
        private BackgroundTaskCancellationReason _cancelReason = BackgroundTaskCancellationReason.Abort;
        private volatile bool _cancelRequested = false;
        private BackgroundTaskDeferral _deferral = null;
        private ThreadPoolTimer _periodicTimer = null;
        private uint _progress = 0;
        private IBackgroundTaskInstance _taskInstance = null;

        public void Run(IBackgroundTaskInstance taskInstance)
        {
            Debug.WriteLine("Background " + taskInstance.Task.Name + " Starting...");

            //
            // Query BackgroundWorkCost
            // Guidance: If BackgroundWorkCost is high, then perform only the minimum amount
            // of work in the background task and return immediately.
            //
            var cost = BackgroundWorkCost.CurrentBackgroundWorkCost;
            var settings = ApplicationData.Current.LocalSettings;

            // Recommended: Adjust task behavior based on CPU and network availability      
            // For example: A mail app could download mail for all folders when cost is     
            // low and only download mail for the Inbox folder when cost is hig
            switch (cost)
            {
                case BackgroundWorkCostValue.Low: // The task can use CPU & network

                case BackgroundWorkCostValue.Medium: // The task can use some CPU & network

                case BackgroundWorkCostValue.High: // The task should avoid using CPU & network

                    // This example records the last trigger time in an application data setting         
                    // so the app can read it later if it chooses. We do regardless of work cost.
                    settings.Values["LastTriggerTime"] = DateTimeOffset.Now;
                    break;
            }

            settings.Values["BackgroundWorkCost"] = cost.ToString();

            //
            // Associate a cancellation handler with the background task.
            // NOTE: Once canceled, atask has 5 seconds tocomplete orthe processis killed
            //
            taskInstance.Canceled += new BackgroundTaskCanceledEventHandler(OnCanceled);
            //
            // Get the deferral object from the task instance, and take a reference to the taskInstance;
            //
            _deferral = taskInstance.GetDeferral();
            _taskInstance = taskInstance;

            _periodicTimer = ThreadPoolTimer.CreatePeriodicTimer(new TimerElapsedHandler(PeriodicTimerCallback), TimeSpan.FromSeconds(1));
        }

        //
        // Simulate the background task activity.
        //
        private void PeriodicTimerCallback(ThreadPoolTimer timer)
        {
            if ((_cancelRequested == false) && (_progress < 100))
            {
                _progress += 10;
                _taskInstance.Progress = _progress;
            }
            else
            {
                _periodicTimer.Cancel();

                var settings = ApplicationData.Current.LocalSettings;
                var key = _taskInstance.Task.Name;

                //
                // Write to LocalSettings to indicate that this background task ran.
                //
                settings.Values[key] = (_progress < 100) ? "Canceled with reason: " + _cancelReason.ToString() : "Completed";
                Debug.WriteLine("Background " + _taskInstance.Task.Name + settings.Values[key]);
                InvokeSimpleToast(_taskInstance.Task.Name + settings.Values[key]);
                //
                // Indicate that the background task has completed.
                //
                _deferral.Complete();
            }
        }

        //
        // Handles background task cancellation.
        //
        private void OnCanceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            //
            // Indicate that the background task is canceled.
            //
            _cancelRequested = true;
            _cancelReason = reason;

            Debug.WriteLine("Background " + sender.Task.Name + " Cancel Requested...");
        }

        public void InvokeSimpleToast(string toastMessage)
        {
            XmlDocument toastXml = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastImageAndText02);

            XmlNodeList stringElements = toastXml.GetElementsByTagName("text");

            stringElements.Item(0).AppendChild(toastXml.CreateTextNode(toastMessage));
            stringElements.Item(1).AppendChild(toastXml.CreateTextNode(toastMessage));

            ToastNotification toast = new ToastNotification(toastXml);
            ToastNotificationManager.CreateToastNotifier().Show(toast);
        }
    }
}