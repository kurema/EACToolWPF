﻿using System;
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

using System.Linq;
using Vanara.PInvoke;

namespace EACToolWpf
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private static readonly HttpClient httpClient = new();

		public RegexEntry[] RegexEntries { get; set; } = [
			new RegexEntry(@"^[\s\t　]*(\d+)[\s\t　]*[\.．:：]?[\s\t　]*",""),
			new RegexEntry(@"[\s\t　]*$",""),
			new RegexEntry(@"^[\s\t　]+",""),
			new RegexEntry(@"\(\d+\)","\n"),
			new RegexEntry(@"\n{2,}","\n"),
		];

		public MainWindow()
		{
			InitializeComponent();
			CurrentTempPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), System.IO.Path.GetRandomFileName() + ".jpeg");
			ErrorTempPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), System.IO.Path.GetRandomFileName());

			for (int i = 1; i <= 50; i++)
			{
				var fn = $"Error.{i:00}.txt";
				var cb = new CheckBox()
				{
					Content = fn,
					Tag = fn,
				};
				StackPanelErrorFiles.Children.Add(cb);
			}
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
		string CurrentTempPath;

		string ErrorTempPath;

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
			if (img is null || e.LeftButton != System.Windows.Input.MouseButtonState.Pressed) return;
			var dataobject = new DataObject(DataFormats.FileDrop, new[] { CurrentTempPath });
			DragDrop.DoDragDrop(img, dataobject, DragDropEffects.Copy);
		}

		private void WindowTop_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if (CurrentTempPath is not null && System.IO.File.Exists(CurrentTempPath)) File.Delete(CurrentTempPath);
		}

		private void Button_Click_TracksPaste(object sender, RoutedEventArgs e)
		{
			TextBoxTracksFrom.Text = Clipboard.GetText();
		}

		private void TextBoxTracksFrom_TextChanged(object sender, TextChangedEventArgs e)
		{
			var text = (sender as TextBox)?.Text;
			if (text is null) return;

			foreach (var entry in RegexEntries)
			{
				text = System.Text.RegularExpressions.Regex.Replace(text, entry.RegexFrom, entry.RegexTo, System.Text.RegularExpressions.RegexOptions.Multiline);
			}
			TextBoxTracksTo.Text = text;
		}

		private void ImagePreview_Drop(object sender, DragEventArgs e)
		{
			if (e.Data is not DataObject dataobject) return;
			var files = dataobject.GetFileDropList();

			foreach (var item in files)
			{
				try
				{
					if (item is null) continue;

					var dir = System.IO.Path.GetDirectoryName(item);
					string resultPath = System.IO.Path.Combine(dir ?? "", System.IO.Path.GetFileNameWithoutExtension(item) + ".resized.jpeg");

					var src = new System.Drawing.Bitmap(item);
					var scale = Math.Min(1000.0 / src.Width, 1000.0 / src.Height);
					if (scale > 1.0)
					{
						src.Save(resultPath, System.Drawing.Imaging.ImageFormat.Jpeg);
						continue;
					}
					var dest = new System.Drawing.Bitmap((int)(src.Width * scale), (int)(src.Height * scale));
					var g = System.Drawing.Graphics.FromImage(dest);
					g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.High;
					g.DrawImage(src, 0, 0, dest.Width, dest.Height);
					var enc = System.Drawing.Imaging.ImageCodecInfo.GetImageEncoders().FirstOrDefault(a => a.FormatID == System.Drawing.Imaging.ImageFormat.Jpeg.Guid);
					if (enc is null)
					{
						dest.Save(resultPath, System.Drawing.Imaging.ImageFormat.Jpeg);
					}
					else
					{
						var epara = new System.Drawing.Imaging.EncoderParameters(1);
						epara.Param[0] = new System.Drawing.Imaging.EncoderParameter(System.Drawing.Imaging.Encoder.Quality, (long)70);
						dest.Save(resultPath, enc, epara);
					}

					//if (!Aspose.Imaging.Image.CanLoad(item)) continue;
					//using var image = Aspose.Imaging.Image.Load(item);
					//var scale = Math.Min(1000.0 / image.Width, 1000.0 / image.Height);
					//if (scale < 1) image.Resize((int)(image.Width * scale), (int)(image.Height * scale));
					//var dir = System.IO.Path.GetDirectoryName(item);
					//string resultPath;
					//resultPath = System.IO.Path.Combine(dir ?? "", System.IO.Path.GetFileNameWithoutExtension(item) + ".resized.jpeg");
					//image.Save(resultPath);
				}
				catch { }
			}
		}

		private async void Button_Click_PasteUrl(object sender, RoutedEventArgs e)
		{
			try
			{
				TextBoxImageUrl.Text = Clipboard.GetText();
				await DownloadImageCache();
			}
			catch (Exception ex)
			{
				MessageBox.Show($"貼り付けに失敗。\n{ex}", "失敗");
			}

		}

		private void Button_Click_TrackCopy(object sender, RoutedEventArgs e)
		{
			try
			{
				Clipboard.SetText(TextBoxTracksTo.Text);
			}
			catch (Exception ex)
			{
				MessageBox.Show($"コピーに失敗。\n{ex}", "失敗");
			}
		}

		private void Button_Click_Pattern1(object sender, RoutedEventArgs e)
		{
			var text = TextBoxTracksFrom.Text;
			var result = new StringBuilder();
			var sb = new StringReader(text);
			var reg1 = new System.Text.RegularExpressions.Regex(@"^[\s\t　]*(\d+)[\s\t　]*[\.．]?[\s\t　]*");
			var reg2 = new System.Text.RegularExpressions.Regex(@"[\s\t　]*$");
			while (true)
			{
				var line = sb.ReadLine();
				if (line is null) break;
				if (reg1.IsMatch(line))
				{
					result.AppendLine(reg2.Replace(reg1.Replace(line, ""), ""));
				}
			}
			TextBoxTracksTo.Text = result.ToString();
		}

		private void Button_Click_Hankaku(object sender, RoutedEventArgs e)
		{
			var text = TextBoxTracksFrom.Text;
			var sb = new StringBuilder();
			text = text.Replace("（", " (").Replace("）", ") ");
			foreach (var t in text)
			{
				if (t >= '０' && t <= '９') sb.Append((char)(t - '０' + '0'));
				else if (t >= 'ａ' && t <= 'ｚ') sb.Append((char)(t - 'ａ' + 'a'));
				else if (t >= 'Ａ' && t <= 'Ｚ') sb.Append((char)(t - 'Ａ' + 'A'));
				else sb.Append(t);
			}
			TextBoxTracksTo.Text = sb.ToString();
		}

		private void Rectangle_MouseDown(object sender, MouseButtonEventArgs e)
		{
			var rect = sender as Rectangle;
			if (rect is null) return;

			if (!Directory.Exists(ErrorTempPath)) Directory.CreateDirectory(ErrorTempPath);
			List<string> files = new List<string>();
			foreach (var item in StackPanelErrorFiles.Children)
			{
				if (item is not CheckBox cb) continue;
				if (cb.IsChecked != true) continue;
				var fn = System.IO.Path.Combine(ErrorTempPath, ((string)cb.Tag));
				files.Add(fn);
				if (!File.Exists(fn)) { using var _ = System.IO.File.Create(fn); }
			}
			if (files.Count == 0) return;
			var dataobject = new DataObject(DataFormats.FileDrop, files.ToArray());
			DragDrop.DoDragDrop(rect, dataobject, DragDropEffects.Copy);
		}

		private void Button_Click_ClearCheckboxError(object sender, RoutedEventArgs e)
		{
			foreach (var item in StackPanelErrorFiles.Children)
			{
				if (item is not CheckBox cb) continue;
				cb.IsChecked = false;
			}
		}
	}
}