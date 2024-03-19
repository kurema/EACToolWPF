using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
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
using static Vanara.PInvoke.OleAut32.PICTDESC.PICTDEC_UNION;

namespace EACToolWpf
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private static readonly HttpClient httpClient = new();

		public MainWindow()
		{
			InitializeComponent();
		}

		private async void TextBox_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Enter)
			{
				await DownloadImageCache();
			}
		}

		private async void Button_Click(object sender, RoutedEventArgs e)
		{
			await DownloadImageCache();
		}

		public async Task DownloadImageCache()
		{
			var url = TextBoxImageUrl.Text;
			try
			{
				var response = await httpClient.GetAsync(url);
				response.EnsureSuccessStatusCode();

				if (response.Content.Headers.ContentType?.MediaType is not null && !response.Content.Headers.ContentType.MediaType.StartsWith("image"))
					throw new Exception("Response is not image.");

				switch (response.Content.Headers.ContentType?.MediaType)
				{
					case "image/jpeg":
					case "image/gif":
						CurrentImageJpeg = null;
						CurrentImageRaw = await response.Content.ReadAsByteArrayAsync();
						break;
					case "image/webp":
						{
							var dec = new Imazen.WebP.SimpleDecoder();
							CurrentImageRaw = await response.Content.ReadAsByteArrayAsync();
							var bmp = dec.DecodeFromBytes(CurrentImageRaw, CurrentImageRaw.Length);
							var ms = new MemoryStream();
							bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
							CurrentImageJpeg = ms.ToArray();
						}
						break;
					default:
						{
							CurrentImageRaw = await response.Content.ReadAsByteArrayAsync();
							using var ms1 = new MemoryStream(CurrentImageRaw);
							var image = new System.Drawing.Bitmap(ms1);
							var ms2 = new MemoryStream();
							image.Save(ms2, System.Drawing.Imaging.ImageFormat.Jpeg);
							CurrentImageJpeg = ms2.ToArray();
						}
						break;
				}

				CurrentImageRawExt = response.Content.Headers.ContentType?.MediaType switch
				{
					"image/webp" => ".webp",
					"image/bmp" => ".bmp",
					"image/gif" => ".gif",
					"image/jpeg" => ".jpeg",
					"image/png" => ".png",
					"image/tiff" => ".tiff",
					_ => ".unknown",
				};
				CurrentTempPath = System.IO.Path.GetTempFileName() + ".jpeg";
				await File.WriteAllBytesAsync(CurrentTempPath, CurrentImageJpeg ?? CurrentImageRaw);


				ShowImage();
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Failed.\n{ex}", "Failed to load!");
			}
		}

		byte[]? CurrentImageRaw = null;
		byte[]? CurrentImageJpeg = null;
		string? CurrentImageRawExt = null;
		string? CurrentTempPath = null;

		void ShowImage()
		{
			if (CurrentImageRaw is null) return;
			using MemoryStream ms = new MemoryStream(CurrentImageJpeg ?? CurrentImageRaw);
			ImagePreview.Source = BitmapFrame.Create(ms, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
		}


		string DownloadsPath
		{
			get
			{
				var result = Vanara.PInvoke.Shell32.SHGetKnownFolderPath(new Guid("374DE290-123F-4565-9164-39C4925E467B"), 0, 0, out var path);
				if (result == Vanara.PInvoke.HRESULT.S_OK && !string.IsNullOrEmpty(path)) return path;
				return System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
			}
		}

		string DownloadsImgPath => System.IO.Path.Combine(DownloadsPath, "Img");

		private void Button_Click_OpenDownload(object sender, RoutedEventArgs e)
		{

			OpenOnExplorer(DownloadsPath);
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

		private async void Button_Click_CreateImg(object sender, RoutedEventArgs e)
		{
			if (System.IO.Directory.Exists(DownloadsImgPath)) return;
			System.IO.Directory.CreateDirectory(DownloadsImgPath);
			await SaveImage(DownloadsImgPath);
		}

		private async void Button_Click_SaveImage(object sender, RoutedEventArgs e)
		{
			await SaveImage(DownloadsPath);
		}

		async Task SaveImage(string directory)
		{
			if (CurrentImageRaw is null) return;
			await File.WriteAllBytesAsync(System.IO.Path.Combine(directory, $"Cover{CurrentImageRawExt}"), CurrentImageRaw);
			if (CurrentImageJpeg is not null) await File.WriteAllBytesAsync(System.IO.Path.Combine(directory, $"Cover.jpeg"), CurrentImageJpeg);
		}

		private void ImagePreview_MouseMove(object sender, MouseEventArgs e)
		{
			if (CurrentImageRaw is null) return;
			if (CurrentTempPath is null) return;
			var img = sender as Image;
			if (img is null || e.LeftButton != MouseButtonState.Pressed) return;
			//using MemoryStream ms = new MemoryStream(CurrentImageJpeg ?? CurrentImageRaw);
			var dataobject = new DataObject(DataFormats.FileDrop, new[] { CurrentTempPath });
			//dataobject.SetImage(BitmapFrame.Create(ms, BitmapCreateOptions.None, BitmapCacheOption.OnLoad));
			DragDrop.DoDragDrop(img, dataobject, DragDropEffects.Copy);
		}
	}
}