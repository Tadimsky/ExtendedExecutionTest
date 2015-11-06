using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.ExtendedExecution;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace ExtendedExecutionTest
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : Application
    {

		public static bool RequestExtension { get; set; }
		public bool canContinue = true;
		public static ApplicationDataContainer LocalSettings { get { return ApplicationData.Current.LocalSettings; } }
		public static ApplicationExecutionState PreviousExecutionState {
			get; private set; }

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
			Microsoft.ApplicationInsights.WindowsAppInitializer.InitializeAsync(
				Microsoft.ApplicationInsights.WindowsCollectors.Metadata |
				Microsoft.ApplicationInsights.WindowsCollectors.Session);
			this.InitializeComponent();
            this.Suspending += OnSuspending;
			this.Resuming += OnResuming;
			
        }

		private async void OnResuming(object sender, object e)
		{
			Debug.WriteLine("Resuming?");
			(Window.Current.Content as Frame).Navigate(typeof(MainPage));
			MessageDialog m = new MessageDialog("Hello!");
			await m.ShowAsync();
		}

		/// <summary>
		/// Invoked when the application is launched normally by the end user.  Other entry points
		/// will be used such as when the application is launched to open a specific file.
		/// </summary>
		/// <param name="e">Details about the launch request and process.</param>
		protected override void OnLaunched(LaunchActivatedEventArgs e)
        {

            Frame rootFrame = Window.Current.Content as Frame;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();

                rootFrame.NavigationFailed += OnNavigationFailed;

				PreviousExecutionState = e.PreviousExecutionState;
                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {	

                }

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }

            if (rootFrame.Content == null)
            {
                // When the navigation stack isn't restored navigate to the first page,
                // configuring the new page by passing required information as a navigation
                // parameter
                rootFrame.Navigate(typeof(MainPage), e.Arguments);
            }
            // Ensure the current window is active
            Window.Current.Activate();
        }

        /// <summary>
        /// Invoked when Navigation to a certain page fails
        /// </summary>
        /// <param name="sender">The Frame which failed navigation</param>
        /// <param name="e">Details about the navigation failure</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private async void OnSuspending(object sender, SuspendingEventArgs e)
        {
			MessageDialog m = new MessageDialog($"{(RequestExtension ? "" : "Not ")} Requesting Extension", "Suspending!");
			m.ShowAsync();

			Debug.WriteLine("Suspending...");
			DateTime now = DateTime.Now;

			LocalSettings.Values["Suspended"] = now.Ticks;
			

            var deferral = e.SuspendingOperation.GetDeferral();			
			Debug.WriteLine($"Suspend Deadline: {e.SuspendingOperation.Deadline - now}");
			using (var extension = new ExtendedExecutionSession())
			{
				extension.Description = "Testing...";
				extension.Reason = ExtendedExecutionReason.SavingData;
				extension.Revoked += RevokedExtension;
				//var result = ExtendedExecutionResult.Denied;
				var result = RequestExtension ? await extension.RequestExtensionAsync() : ExtendedExecutionResult.Denied;
				Debug.WriteLine($"Suspend Deadline after Request: {e.SuspendingOperation.Deadline - now}");
                if (result == ExtendedExecutionResult.Allowed)
				{
					Debug.WriteLine("Granted Extended Execution");
					await DoWork(e.SuspendingOperation);
				}
				else
				{
					Debug.WriteLine("Denied Extended Execution");
					await DoWork(e.SuspendingOperation);
				}
			}
			Debug.WriteLine("Done.");
			deferral.Complete();
        }

		private void RevokedExtension(object sender, ExtendedExecutionRevokedEventArgs args)
		{
			LocalSettings.Values["Revoked"] = DateTime.Now.Ticks;
			LocalSettings.Values["RevokedReason"] = args.Reason.ToString();

			Debug.WriteLine("Revoked - going down!");
			Debug.WriteLine(args.Reason);
			canContinue = false;
		}

		private async Task<bool> DoWork(SuspendingOperation operation)
		{
			canContinue = true;

            while (canContinue)
			{
				Debug.WriteLine(DateTime.Now);
				LocalSettings.Values["Revoked"] = DateTime.Now.Ticks;
				await Task.Delay(1000);
			}
			return true;
		}

    }
}
