using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace ExtendedExecutionTest
{
	/// <summary>
	/// An empty page that can be used on its own or navigated to within a Frame.
	/// </summary>
	public sealed partial class MainPage : Page
	{

		public TimeSpan Duration { get; set; }
		public string Reason { get; set; }

		public MainPage()
		{
			InitializeComponent();

			//App.Current.Suspending += (sender, args) => { Spinner.IsActive = true; };
		}


		protected override void OnNavigatedTo(NavigationEventArgs e)
		{
			string duration = "";
			string reason = "";
			string prevexec = App.PreviousExecutionState.ToString();
			try
			{
				long revoked = (long)App.LocalSettings.Values["Revoked"];
				long suspended = (long)App.LocalSettings.Values["Suspended"];
				TimeSpan elapsed = new TimeSpan(revoked - suspended);
				Duration = elapsed;
				duration = Duration.ToString();
			}
			catch (NullReferenceException er)
			{

			}

			try
			{
				reason = (string)App.LocalSettings.Values["RevokedReason"];

			}
			catch (Exception er)
			{

			}
			App.LocalSettings.Values.Clear();

			txtTime.Text = $"Duration: {duration}";
			txtReason.Text = $"Reason: {reason}";

			txtPrevState.Text = $"Previous Execution State: {prevexec}";
		}

		protected override void OnNavigatedFrom(NavigationEventArgs e)
		{

			base.OnNavigatedFrom(e);
		}

		private void chkExtendedEx_Checked(object sender, RoutedEventArgs e)
		{
			App.RequestExtension = chkExtendedEx.IsChecked ?? false;
		}
	}
}
