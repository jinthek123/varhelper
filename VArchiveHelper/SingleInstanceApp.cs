using System.Threading;
using System.Windows.Forms;

namespace VArchiveHelper;

internal static class SingleInstanceApp
{
	private const string MutexName = "Global\\VArchiveHelper.SingleInstance";

	private static Mutex _mutex;

	public static bool TryAcquire()
	{
		_mutex = new Mutex(initiallyOwned: true, "Global\\VArchiveHelper.SingleInstance", out var createdNew);
		if (createdNew)
		{
			return true;
		}
		MessageBox.Show("VArchiveHelper가 이미 실행 중입니다.\n작업 표시줄이나 설정 창을 확인하세요.", AppBranding.DisplayName, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
		return false;
	}

	public static void Release()
	{
		if (_mutex != null)
		{
			try
			{
				_mutex.ReleaseMutex();
			}
			catch
			{
			}
			_mutex.Dispose();
			_mutex = null;
		}
	}
}
