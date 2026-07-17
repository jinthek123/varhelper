using System;
using System.IO;
using System.Text.Json;
using System.Windows.Forms;

namespace VArchiveHelper;

internal sealed class HelperConfig
{
	public const int CurrentConfigVersion = 2;

	public int ConfigVersion { get; set; } = 2;

	public int MonitorIndex { get; set; } = 1;

	public int TargetWidth { get; set; } = 2560;

	public int TargetHeight { get; set; } = 1440;

	public string VArchiveExePath { get; set; } = "D:\\v-archive_v0.64\\v-archive.exe";

	public string VArchiveProcessName { get; set; } = "v-archive";

	public int ClipboardSettleMs { get; set; } = 80;

	public int AfterClipboardBeforeRecognizeMs { get; set; } = 120;

	public int VArchiveStartupWaitMs { get; set; } = 2500;

	public bool UseDxgiCapture { get; set; } = true;

	public bool RequireDxgiCapture { get; set; } = true;

	public bool UsePhysicalPixels { get; set; } = true;

	public string GameProcessName { get; set; } = "";

	public bool UseGameWindowCapture { get; set; }

	public int CaptureHotkeyVirtualKey { get; set; } = 45;

	public int CaptureHotkeyModifiers { get; set; }

	public string UiTheme { get; set; } = "Light";

	public static string SettingsPath => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");

	public static string ExampleSettingsPath => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.example.json");

	public static HelperConfig Load()
	{
		EnsureSettingsFileExists();
		if (!File.Exists(SettingsPath))
		{
			return new HelperConfig();
		}
		try
		{
			HelperConfig? obj = JsonSerializer.Deserialize<HelperConfig>(File.ReadAllText(SettingsPath)) ?? new HelperConfig();
			obj.NormalizeAfterLoad();
			return obj;
		}
		catch
		{
			return new HelperConfig();
		}
	}

	public static void EnsureSettingsFileExists()
	{
		if (!File.Exists(SettingsPath) && File.Exists(ExampleSettingsPath))
		{
			File.Copy(ExampleSettingsPath, SettingsPath);
		}
	}

	public void NormalizeAfterLoad()
	{
		if (ConfigVersion < 2)
		{
			ConfigVersion = 2;
		}
		if (!string.Equals(UiTheme, "Dark", StringComparison.OrdinalIgnoreCase))
		{
			UiTheme = "Light";
		}
	}

	public HelperConfig Clone()
	{
		return JsonSerializer.Deserialize<HelperConfig>(JsonSerializer.Serialize(this)) ?? new HelperConfig();
	}

	public bool TryValidate(out string error)
	{
		if (string.IsNullOrWhiteSpace(VArchiveExePath))
		{
			error = "v-archive.exe 경로를 입력하세요.";
			return false;
		}
		if (!File.Exists(VArchiveExePath.Trim()))
		{
			error = "v-archive.exe 파일을 찾을 수 없습니다: " + VArchiveExePath;
			return false;
		}
		if (!CaptureHotkey.IsValidForRegistration(CaptureHotkeyModifiers, CaptureHotkeyVirtualKey, out error))
		{
			return false;
		}
		Screen[] allScreens = Screen.AllScreens;
		if (allScreens.Length == 0)
		{
			error = "연결된 모니터가 없습니다.";
			return false;
		}
		if (MonitorIndex < 0 || MonitorIndex >= allScreens.Length)
		{
			error = $"Index {MonitorIndex + 1} 모니터가 없습니다. (1~{allScreens.Length})";
			return false;
		}
		error = null;
		return true;
	}

	public void Save()
	{
		ConfigVersion = 2;
		JsonSerializerOptions options = new JsonSerializerOptions
		{
			WriteIndented = true
		};
		File.WriteAllText(SettingsPath, JsonSerializer.Serialize(this, options));
	}
}
