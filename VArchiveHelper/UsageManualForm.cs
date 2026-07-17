using System;
using System.Drawing;
using System.Windows.Forms;

namespace VArchiveHelper;

internal sealed class UsageManualForm : Form
{
	private sealed class VerticalOnlyScrollPanel : Panel
	{
		private const int WmHscroll = 276;

		private const int WmMousehwheel = 526;

		public VerticalOnlyScrollPanel()
		{
			Dock = DockStyle.Fill;
			AutoScroll = true;
		}

		protected override void OnLayout(LayoutEventArgs levent)
		{
			base.OnLayout(levent);
			SuppressHorizontalScroll();
		}

		protected override void WndProc(ref Message m)
		{
			int msg = m.Msg;
			if ((msg != 276 && msg != 526) || 1 == 0)
			{
				base.WndProc(ref m);
			}
		}

		public void SuppressHorizontalScroll()
		{
			base.HorizontalScroll.Enabled = false;
			base.HorizontalScroll.Visible = false;
			if (base.AutoScrollMinSize.Width != 0)
			{
				base.AutoScrollMinSize = new Size(0, base.AutoScrollMinSize.Height);
			}
			try
			{
				base.HorizontalScroll.Value = base.HorizontalScroll.Minimum;
			}
			catch (ArgumentException)
			{
			}
		}
	}

	private const int FormContentWidth = 600;

	private const int FormClientHeight = 500;

	private const int FormPaddingHorizontal = 20;

	private const int FormChromeMargin = 16;

	private const int FormMinClientHeight = 360;

	private static UsageManualForm _openInstance;

	private VerticalOnlyScrollPanel _scrollPanel;

	private FlowLayoutPanel _contentFlow;

	private Label _headline;

	private string _layoutScreenDeviceName;

	private int _lastLayoutWidth = -1;

	private int _lastLayoutHeight = -1;

	public static void ShowFor(IWin32Window owner)
	{
		if (_openInstance != null && !_openInstance.IsDisposed)
		{
			if (_openInstance.WindowState == FormWindowState.Minimized)
			{
				_openInstance.WindowState = FormWindowState.Normal;
			}
			_openInstance.ApplyPreferredClientSize();
			_openInstance.BringToFront();
			_openInstance.Activate();
			ApplyCurrentThemeIfOpen();
		}
		else
		{
			_openInstance = new UsageManualForm();
			_openInstance.FormClosed += delegate
			{
				_openInstance = null;
			};
			_openInstance.Show(owner);
		}
	}

	public static void ApplyCurrentThemeIfOpen()
	{
		if (_openInstance != null && !_openInstance.IsDisposed)
		{
			_openInstance.ApplyTheme();
		}
	}

	private UsageManualForm()
	{
		base.Icon = AppIcon.Get();
		Text = AppBranding.ManualWindowTitle;
		base.StartPosition = FormStartPosition.CenterParent;
		base.FormBorderStyle = FormBorderStyle.FixedDialog;
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		base.ShowInTaskbar = false;
		base.AutoScaleMode = AutoScaleMode.None;
		base.Padding = new Padding(10);
		base.ClientSize = GetPreferredClientSize();
		Font = UiFonts.Regular(10f);
		_headline = new Label
		{
			AutoSize = true,
			Dock = DockStyle.Top,
			Text = AppBranding.ManualContentTitle,
			Font = UiFonts.Bold(11f),
			Margin = new Padding(0, 0, 0, 6),
			MinimumSize = new Size(0, 20)
		};
		_scrollPanel = new VerticalOnlyScrollPanel();
		_contentFlow = new FlowLayoutPanel
		{
			AutoSize = true,
			AutoSizeMode = AutoSizeMode.GrowAndShrink,
			FlowDirection = FlowDirection.TopDown,
			WrapContents = false,
			Margin = new Padding(0),
			Padding = new Padding(0)
		};
		foreach (UsageGuideSection section in UsageGuide.Sections)
		{
			_contentFlow.Controls.Add(BuildSectionCard(section));
		}
		_contentFlow.Location = new Point(0, 0);
		_scrollPanel.Controls.Add(_contentFlow);
		Button button = new Button
		{
			Text = "닫기",
			AutoSize = true,
			Enabled = true,
			Font = UiFonts.Regular(10f),
			Padding = new Padding(12, 4, 12, 4),
			Margin = new Padding(0)
		};
		button.Click += delegate
		{
			Close();
		};
		FlowLayoutPanel flowLayoutPanel = new FlowLayoutPanel
		{
			AutoSize = true,
			AutoSizeMode = AutoSizeMode.GrowAndShrink,
			FlowDirection = FlowDirection.RightToLeft,
			WrapContents = false,
			Dock = DockStyle.Bottom,
			Margin = new Padding(0, 8, 0, 0)
		};
		flowLayoutPanel.Controls.Add(button);
		base.Controls.Add(_scrollPanel);
		base.Controls.Add(flowLayoutPanel);
		base.Controls.Add(_headline);
		base.AcceptButton = button;
		base.CancelButton = button;
		base.Load += delegate
		{
			ApplyTheme();
			LayoutManualContent();
		};
		base.Shown += delegate
		{
			_layoutScreenDeviceName = Screen.FromControl(this).DeviceName;
			RelayoutManualContentDeferred();
		};
		base.Move += delegate
		{
			OnMovedToAnotherScreen();
		};
		_scrollPanel.Resize += delegate
		{
			LayoutManualContent();
		};
	}

	private void OnMovedToAnotherScreen()
	{
		if (base.IsHandleCreated && base.WindowState == FormWindowState.Normal)
		{
			string deviceName = Screen.FromControl(this).DeviceName;
			if (!string.Equals(_layoutScreenDeviceName, deviceName, StringComparison.Ordinal))
			{
				_layoutScreenDeviceName = deviceName;
				OnScreenEnvironmentChanged();
			}
		}
	}

	private void OnScreenEnvironmentChanged()
	{
		_lastLayoutWidth = -1;
		_lastLayoutHeight = -1;
		base.ClientSize = GetPreferredClientSize();
		EnsureWithinWorkingArea();
		RefreshHeadlineLayout();
		RelayoutManualContentDeferred();
	}

	private Size GetPreferredClientSize()
	{
		int val = 620 + SystemInformation.VerticalScrollBarWidth;
		int val2 = 500;
		if (!base.IsHandleCreated)
		{
			return new Size(val, val2);
		}
		Rectangle workingArea = Screen.FromControl(this).WorkingArea;
		int num = Math.Max(0, base.Width - base.ClientSize.Width);
		int num2 = Math.Max(0, base.Height - base.ClientSize.Height);
		val = Math.Min(val, Math.Max(280, workingArea.Width - num - 16));
		val2 = Math.Min(val2, Math.Max(360, workingArea.Height - num2 - 16));
		return new Size(val, val2);
	}

	private void RelayoutManualContentDeferred()
	{
		LayoutManualContent();
		BeginInvoke((Action)delegate
		{
			_lastLayoutWidth = -1;
			_lastLayoutHeight = -1;
			LayoutManualContent();
		});
	}

	protected override void OnDpiChanged(DpiChangedEventArgs e)
	{
		base.OnDpiChanged(e);
		OnScreenEnvironmentChanged();
	}

	private void ApplyPreferredClientSize()
	{
		_lastLayoutWidth = -1;
		_lastLayoutHeight = -1;
		base.ClientSize = GetPreferredClientSize();
		EnsureWithinWorkingArea();
		RefreshHeadlineLayout();
		LayoutManualContent();
	}

	private void RefreshHeadlineLayout()
	{
		if (_headline != null)
		{
			int num = Math.Max(1, base.ClientSize.Width - base.Padding.Horizontal);
			_headline.MaximumSize = new Size(num, 0);
			_headline.PerformLayout();
			_headline.Invalidate();
		}
	}

	private void EnsureWithinWorkingArea()
	{
		Rectangle workingArea = Screen.FromControl(this).WorkingArea;
		if (base.Width > workingArea.Width - 8)
		{
			base.Width = workingArea.Width - 8;
		}
		if (base.Height > workingArea.Height - 8)
		{
			base.Height = workingArea.Height - 8;
		}
		if (base.Left < workingArea.Left)
		{
			base.Left = workingArea.Left;
		}
		if (base.Top < workingArea.Top)
		{
			base.Top = workingArea.Top;
		}
		if (base.Left + base.Width > workingArea.Right)
		{
			base.Left = workingArea.Right - base.Width;
		}
		if (base.Top + base.Height > workingArea.Bottom)
		{
			base.Top = workingArea.Bottom - base.Height;
		}
	}

	private void ApplyTheme()
	{
		UiThemePalette palette = UiTheme.Palette;
		UiTheme.ApplyTo(this);
		_headline.Font = UiFonts.Bold(11f);
		_headline.ForeColor = palette.SectionTitle;
		_scrollPanel.BackColor = palette.WindowBackground;
		foreach (Control control in _contentFlow.Controls)
		{
			if (!(control is TableLayoutPanel tableLayoutPanel))
			{
				continue;
			}
			tableLayoutPanel.BackColor = palette.WindowBackground;
			foreach (Control control2 in tableLayoutPanel.Controls)
			{
				if (control2 is Label label)
				{
					label.BackColor = Color.Transparent;
					Font font = label.Font;
					if (font != null && font.Style == FontStyle.Bold)
					{
						label.Font = UiFonts.Bold(10f);
						label.ForeColor = palette.SectionTitle;
					}
					else
					{
						label.Font = UiFonts.Regular(10f);
						label.ForeColor = palette.CardBody;
					}
				}
			}
			tableLayoutPanel.Invalidate();
		}
		_lastLayoutWidth = -1;
		_lastLayoutHeight = -1;
		LayoutManualContent();
	}

	private void LayoutManualContent()
	{
		if (_scrollPanel == null || _contentFlow == null)
		{
			return;
		}
		int num = _scrollPanel.DisplayRectangle.Width;
		if (num < 1)
		{
			return;
		}
		RefreshHeadlineLayout();
		_contentFlow.Width = num;
		_contentFlow.MaximumSize = new Size(num, 0);
		foreach (Control control in _contentFlow.Controls)
		{
			if (!(control is TableLayoutPanel tableLayoutPanel))
			{
				continue;
			}
			int num2 = Math.Max(1, num - tableLayoutPanel.Padding.Horizontal);
			tableLayoutPanel.Width = num;
			tableLayoutPanel.MaximumSize = new Size(num, 0);
			foreach (Control control2 in tableLayoutPanel.Controls)
			{
				if (control2 is Label label)
				{
					label.MaximumSize = new Size(num2, 0);
				}
			}
			tableLayoutPanel.PerformLayout();
		}
		_contentFlow.PerformLayout();
		int num3 = _contentFlow.PreferredSize.Height + 2;
		if (num == _lastLayoutWidth && num3 == _lastLayoutHeight)
		{
			_scrollPanel.SuppressHorizontalScroll();
			return;
		}
		_lastLayoutWidth = num;
		_lastLayoutHeight = num3;
		_contentFlow.Location = new Point(0, 0);
		_scrollPanel.AutoScrollPosition = new Point(0, 0);
		_scrollPanel.AutoScrollMinSize = new Size(0, num3);
		_scrollPanel.SuppressHorizontalScroll();
	}

	private static Control BuildSectionCard(UsageGuideSection section)
	{
		UiThemePalette palette = UiTheme.Palette;
		TableLayoutPanel obj = new TableLayoutPanel
		{
			ColumnCount = 1,
			RowCount = 2,
			Tag = "theme-card",
			AutoSize = true,
			AutoSizeMode = AutoSizeMode.GrowAndShrink,
			BackColor = palette.WindowBackground,
			Margin = new Padding(0, 0, 0, 6),
			Padding = new Padding(8, 6, 8, 6),
			RowStyles = 
			{
				new RowStyle(SizeType.AutoSize),
				new RowStyle(SizeType.AutoSize)
			},
			ColumnStyles = 
			{
				new ColumnStyle(SizeType.Percent, 100f)
			}
		};
		Label control = new Label
		{
			AutoSize = true,
			Text = section.Title,
			Font = UiFonts.Bold(10f),
			ForeColor = palette.SectionTitle,
			BackColor = Color.Transparent,
			Margin = new Padding(0, 0, 0, 4)
		};
		Label control2 = new Label
		{
			AutoSize = true,
			Text = string.Join(Environment.NewLine, section.Lines),
			Font = UiFonts.Regular(10f),
			ForeColor = palette.CardBody,
			BackColor = Color.Transparent,
			Margin = new Padding(0)
		};
		obj.Controls.Add(control, 0, 0);
		obj.Controls.Add(control2, 0, 1);
		UiRounded.AttachCardPaint(obj);
		return obj;
	}
}
