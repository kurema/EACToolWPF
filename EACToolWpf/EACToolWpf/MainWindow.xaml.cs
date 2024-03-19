using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace EACToolWpf
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();
		}

		private void Image_DragLeave(object sender, DragEventArgs e)
		{
		}

		private void TextBox_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Enter)
			{

			}
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{

		}

		string DownloadPath
		{
			get
			{
				var result = Vanara.PInvoke.Shell32.SHGetKnownFolderPath(new Guid("374DE290-123F-4565-9164-39C4925E467B"), 0, 0, out var path);
				if (result == Vanara.PInvoke.HRESULT.S_OK && !string.IsNullOrEmpty(path)) return path;
				return System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
			}
		}

		string DownloadImgPath=> System.IO.Path.Combine(DownloadPath, "Img");

		private void Button_Click_OpenDownload(object sender, RoutedEventArgs e)
		{

			OpenOnExplorer(DownloadPath);
		}

		void OpenOnExplorer(string? directory)
		{
			if (string.IsNullOrWhiteSpace(directory)) return;
			System.Diagnostics.ProcessStartInfo pinfo = new()
			{
				FileName = "explorer",
				Arguments = directory
			};
			System.Diagnostics.Process.Start(pinfo);

		}

		private void Button_Click_CreateImg(object sender, RoutedEventArgs e)
		{
			if (System.IO.Directory.Exists(DownloadImgPath)) return;
			System.IO.Directory.CreateDirectory(DownloadImgPath);
		}
	}
}