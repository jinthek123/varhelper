using System;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VArchiveHelper;

internal static class CapturePipeline
{
	public static string Run(HelperConfig config)
	{
		return RunAsync(config).GetAwaiter().GetResult();
	}

	public static async Task<string> RunAsync(HelperConfig config)
	{
		try
		{
			(Bitmap, string, string) tuple = await Task.Run(delegate
			{
				string error;
				string methodUsed;
				Bitmap bitmap = ScreenCapture.CaptureMonitorToTarget(config, out error, out methodUsed);
				return ((Bitmap Image, string Error, string Method))((bitmap == null) ? (Image: null, Error: error, Method: null) : (Image: bitmap, Error: null, Method: methodUsed));
			}).ConfigureAwait(continueOnCapturedContext: true);
			if (tuple.Item1 == null)
			{
				return tuple.Item2;
			}
			using (tuple.Item1)
			{
				return FinishOnUiThread(config, tuple.Item1, tuple.Item3);
			}
		}
		catch (Exception ex)
		{
			return "오류가 발생하였습니다. " + ex.Message;
		}
	}

	private static string FinishOnUiThread(HelperConfig config, Bitmap bitmap, string method)
	{
		VArchiveLauncher.EnsureRunning(config);
		Clipboard.SetImage(bitmap);
		Thread.Sleep(config.ClipboardSettleMs);
		Thread.Sleep(config.AfterClipboardBeforeRecognizeMs);
		InputHelper.SendAltInsert();
		return $"완료하였습니다. {bitmap.Width}×{bitmap.Height}, {method}, Index {config.MonitorIndex + 1}";
	}
}
