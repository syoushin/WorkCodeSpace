﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace JohnCheckPoint
{
    public partial class MainPage : Page
    {
        public const string FEATURE_NAME = "Background tasks";

        private List<Scenario> scenarios = new List<Scenario>
        {
            new Scenario() { Title="Background Task", ClassType=typeof(SampleBackgroundTask)},
            new Scenario() { Title="Background Task with Condition", ClassType=typeof(SampleBackgroundTaskWithCondition)},
            new Scenario() { Title="Background in APP Test", ClassType=typeof(InAppBackgroundTest)},
            new Scenario() { Title="Background Task with Time Trigger", ClassType=typeof(TimeTriggeredTask) },
            new Scenario() { Title="Background Task with Application Trigger", ClassType=typeof(ApplicationTriggerTask) },
            new Scenario() { Title="Background Task Run SameTime", ClassType=typeof(SampleBackgroundTaskSametime) }
        };
    }

    public class Scenario
    {
        public string Title { get; set; }
        public Type ClassType { get; set; }
    }
}

namespace JohnCheckPoint
{
    internal class BackgroundTaskSample
    {
        public const string SampleBackgroundTaskEntryPoint = "MyBackGroundTask.SampleBackgroundTask";
        public const string SampleBackgroundTaskName = "SampleBackgroundTask";
        public static string SampleBackgroundTaskProgress = "";
        public static bool SampleBackgroundTaskRegistered = false;

        public const string SampleBackgroundTaskWithConditionName = "SampleBackgroundTaskWithCondition";
        public static string SampleBackgroundTaskWithConditionProgress = "";
        public static bool SampleBackgroundTaskWithConditionRegistered = false;

        public const string SampleBackgroundTaskSametimeEntryPoint = "MyBackGroundTask.SampleBackgroundSametime";
        public const string SampleBackgroundTaskSametime = "SampleBackgroundTaskSametime";
        public static string SampleBackgroundTaskSametimeProgress = "";
        public static bool SampleBackgroundTaskSametimeRegistered = false;

        public const string ServicingCompleteTaskName = "InAppBackgroundTest";
        public static string ServicingCompleteTaskProgress = "";
        public static bool ServicingCompleteTaskRegistered = false;
        public const string ServicingCompleteTaskEntryPoint = "MyBackGroundTask.SampleBackgroundTaskInProcess";

        public const string TimeTriggeredTaskName = "TimeTriggeredTask";
        public static string TimeTriggeredTaskProgress = "";
        public static bool TimeTriggeredTaskRegistered = false;

        public const string ApplicationTriggerTaskName = "ApplicationTriggerTask";
        public static string ApplicationTriggerTaskProgress = "";
        public static string ApplicationTriggerTaskResult = "";
        public static bool ApplicationTriggerTaskRegistered = false;

        /// <summary>
        /// Register a background task with the specified taskEntryPoint, name, trigger,
        /// and condition (optional).
        /// </summary>
        /// <param name="taskEntryPoint">Task entry point for the background task.</param>
        /// <param name="name">A name for the background task.</param>
        /// <param name="trigger">The trigger for the background task.</param>
        /// <param name="condition">An optional conditional event that must be true for the task to fire.</param>
        public static async Task<BackgroundTaskRegistration> RegisterBackgroundTask(String taskEntryPoint, String name, IBackgroundTrigger trigger, IBackgroundCondition condition)
        {
            if (TaskRequiresBackgroundAccess(name))
            {
                await BackgroundExecutionManager.RequestAccessAsync();
            }

            var builder = new BackgroundTaskBuilder();

            builder.Name = name;
            builder.TaskEntryPoint = taskEntryPoint;
            builder.SetTrigger(trigger);

            if (condition != null)
            {
                builder.AddCondition(condition);

                //
                // If the condition changes while the background task is executing then it will
                // be canceled.
                //
                builder.CancelOnConditionLoss = true;
            }

            BackgroundTaskRegistration task = builder.Register();

            UpdateBackgroundTaskStatus(name, true);

            //
            // Remove previous completion status from local settings.
            //
            var settings = ApplicationData.Current.LocalSettings;
            settings.Values.Remove(name);

            return task;
        }

        /// <summary>
        /// Store the registration status of a background task with a given name.
        /// </summary>
        /// <param name="name">Name of background task to store registration status for.</param>
        /// <param name="registered">TRUE if registered, FALSE if unregistered.</param>
        internal static void UpdateBackgroundTaskStatus(string name, bool registered)
        {
            switch (name)
            {
                case SampleBackgroundTaskName:
                    SampleBackgroundTaskRegistered = registered;
                    break;

                case SampleBackgroundTaskWithConditionName:
                    SampleBackgroundTaskWithConditionRegistered = registered;
                    break;

                case ServicingCompleteTaskName:
                    ServicingCompleteTaskRegistered = registered;
                    break;

                case TimeTriggeredTaskName:
                    TimeTriggeredTaskRegistered = registered;
                    break;

                case ApplicationTriggerTaskName:
                    ApplicationTriggerTaskRegistered = registered;
                    break;

                case SampleBackgroundTaskSametime:
                    SampleBackgroundTaskSametimeRegistered = registered;
                    break;
            }
        }

        /// <summary>
        /// Unregister background tasks with specified name.
        /// </summary>
        /// <param name="name">Name of the background task to unregister.</param>
        internal static void UnregisterBackgroundTasks(string name)
        {
            //
            // Loop through all background tasks and unregister any with SampleBackgroundTaskName or
            // SampleBackgroundTaskWithConditionName.
            //
            foreach (var cur in BackgroundTaskRegistration.AllTasks)
            {
                Debug.WriteLine(cur.Value.Name);
                if (cur.Value.Name == name)
                {
                    cur.Value.Unregister(true);
                }
            }

            UpdateBackgroundTaskStatus(name, false);
        }

        /// <summary>
        /// Get the registration / completion status of the background task with
        /// given name.
        /// </summary>
        /// <param name="name">Name of background task to retreive registration status.</param>
        public static string GetBackgroundTaskStatus(string name)
        {
            var registered = false;
            switch (name)
            {
                case SampleBackgroundTaskName:
                    registered = SampleBackgroundTaskRegistered;
                    break;

                case SampleBackgroundTaskWithConditionName:
                    registered = SampleBackgroundTaskWithConditionRegistered;
                    break;

                case ServicingCompleteTaskName:
                    registered = ServicingCompleteTaskRegistered;
                    break;

                case TimeTriggeredTaskName:
                    registered = TimeTriggeredTaskRegistered;
                    break;

                case ApplicationTriggerTaskName:
                    registered = ApplicationTriggerTaskRegistered;
                    break;

                case SampleBackgroundTaskSametime:
                    registered = SampleBackgroundTaskSametimeRegistered;
                    break;
            }
            var status = registered ? "Registered" : "Unregistered";

            var settings = ApplicationData.Current.LocalSettings;
            if (settings.Values.ContainsKey(name))
            {
                status += " - " + settings.Values[name].ToString();
            }

            return status;
        }

        /// <summary>
        /// Determine if task with given name requires background access.
        /// </summary>
        /// <param name="name">Name of background task to query background access requirement.</param>
        private static bool TaskRequiresBackgroundAccess(string name)
        {
            if ((name == TimeTriggeredTaskName) || (name == ApplicationTriggerTaskName))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}