using System;
using System.Reflection;

namespace VArchiveHelper;

internal static class AppBranding
{
	public const string AppName = "vArchiveHelper";

	public static string Version { get; } = FormatVersion(Assembly.GetExecutingAssembly().GetName().Version);

	public static string DisplayName { get; } = $"{AppName} {Version}";

	public static string SettingsTitle(bool unsaved = false) =>
		unsaved ? DisplayName + " *" : DisplayName;

	public static string ManualWindowTitle => $"사용 매뉴얼 — {DisplayName}";

	public static string PathSetupTitle => $"{DisplayName} — v-archive 경로";

	public static string ManualContentTitle => $"{AppName} 사용 안내";

	private static string FormatVersion(Version? version)
	{
		if (version == null)
		{
			return "0.0.0";
		}

		return $"{version.Major}.{version.Minor}.{version.Build}";
	}
}
