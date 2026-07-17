using System;
using System.Drawing;
using System.Windows.Forms;

namespace VArchiveHelper;

internal sealed class SettingsForm : Form
{
	private const int SettingsLabelColumnWidth = 120;

	private const int SettingsFieldColumnWidth = 360;

	private const int SettingsFormClientHeight = 520;

	private const int SettingsContentRightGap = 12;

	private const int SettingsContentTopGap = 14;

	private const int SettingsStatusButtonReserve = 112;

	private static readonly (string Label, int Width, int Height)[] ResolutionPresets = new(string, int, int)[5]
	{
		("FHD", 1920, 1080),
		("QHD", 2560, 1440),
		("UHD", 3840, 2160),
		("FHD 16:10", 1920, 1200),
		("QHD 16:10", 2560, 1600)
	};

	private readonly HelperConfig _config;

	private readonly Label _captureStatusLabel;

	private HotkeyWindow _hotkey;

	private HelperConfig _lastSaved;

	private bool _suppressDirty;

	private TextBox _hotkeyDisplay;

	private Button _hotkeyAssignButton;

	private bool _capturingHotkey;

	private int _pendingHotkeyModifiers;

	private int _pendingHotkeyVirtualKey;

	private ComboBox _monitorCombo;

	private NumericUpDown _targetWidthInput;

	private NumericUpDown _targetHeightInput;

	private TextBox _vArchivePathInput;

	private ThemedCheckBox _useDxgiCheck;

	private ThemedCheckBox _requireDxgiCheck;

	private ThemedCheckBox _usePhysicalPixelsCheck;

	private Label _settingsStatusLabel;

	private Button _manualButton;

	private RadioButton _themeLightRadio;

	private RadioButton _themeDarkRadio;

	private SettingsStatusTone _settingsStatusTone;

	private bool _captureInProgress;

	private Panel _settingsScrollPanel;

	private Panel _settingsContentPanel;

	private TableLayoutPanel _settingsLayout;

	private int _lastLayoutContentWidth = -1;

	private int _lastLayoutContentHeight = -1;

	private string _layoutScreenDeviceName;

	public SettingsForm(HelperConfig config)
	{
		_config = config;
		_lastSaved = _config.Clone();
		base.KeyPreview = true;
		base.Icon = AppIcon.Get();
		Text = AppBranding.SettingsTitle();
		base.StartPosition = FormStartPosition.CenterScreen;
		base.FormBorderStyle = FormBorderStyle.FixedSingle;
		base.MaximizeBox = false;
		base.AutoScaleMode = AutoScaleMode.None;
		base.Padding = new Padding(14);
		Font = UiFonts.Regular(9.5f);
		base.ClientSize = GetPreferredClientSize();
		_captureStatusLabel = new Label
		{
			AutoSize = true,
			Tag = "accent",
			Text = "상태: 대기 중입니다.",
			TextAlign = ContentAlignment.TopLeft,
			Margin = new Padding(0, 0, 8, 0)
		};
		_manualButton = new Button
		{
			Text = "사용 매뉴얼",
			AutoSize = true,
			Tag = "accent",
			Font = UiFonts.Bold(),
			FlatStyle = FlatStyle.Standard,
			Margin = new Padding(8, 0, 0, 0),
			Padding = new Padding(10, 3, 10, 3)
		};
		_manualButton.Click += delegate
		{
			UsageManualForm.ShowFor(this);
		};
		TableLayoutPanel tableLayoutPanel = new TableLayoutPanel
		{
			ColumnCount = 2,
			AutoSize = true,
			AutoSizeMode = AutoSizeMode.GrowAndShrink,
			Margin = new Padding(0, 0, 12, 12),
			MinimumSize = new Size(480, 0)
		};
		tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
		tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
		tableLayoutPanel.Controls.Add(_captureStatusLabel, 0, 0);
		tableLayoutPanel.Controls.Add(_manualButton, 1, 0);
		_settingsScrollPanel = new Panel
		{
			Dock = DockStyle.Fill,
			AutoScroll = true,
			Padding = new Padding(0, 14, 12, 4)
		};
		_settingsScrollPanel.HorizontalScroll.Enabled = false;
		_settingsScrollPanel.HorizontalScroll.Visible = false;
		_settingsContentPanel = BuildSettingsPanel();
		_settingsContentPanel.Location = new Point(0, 0);
		_settingsContentPanel.Margin = new Padding(0);
		_settingsScrollPanel.Controls.Add(_settingsContentPanel);
		Button button = new Button
		{
			Text = "적용 및 저장",
			Tag = "primary",
			AutoSize = true,
			Margin = new Padding(0)
		};
		button.Click += delegate
		{
			ApplyAndSaveSettings();
		};
		FlowLayoutPanel flowLayoutPanel = new FlowLayoutPanel
		{
			AutoSize = true,
			AutoSizeMode = AutoSizeMode.GrowAndShrink,
			FlowDirection = FlowDirection.LeftToRight,
			WrapContents = false,
			Dock = DockStyle.Top,
			Margin = new Padding(0, 8, 12, 0)
		};
		flowLayoutPanel.Controls.Add(button);
		TableLayoutPanel tableLayoutPanel2 = new TableLayoutPanel
		{
			ColumnCount = 1,
			RowCount = 3,
			Dock = DockStyle.Fill
		};
		tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.AutoSize));
		tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
		tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.AutoSize));
		tableLayoutPanel2.Controls.Add(tableLayoutPanel, 0, 0);
		tableLayoutPanel2.Controls.Add(_settingsScrollPanel, 0, 1);
		tableLayoutPanel2.Controls.Add(flowLayoutPanel, 0, 2);
		base.Controls.Add(tableLayoutPanel2);
		base.FormClosing += OnFormClosing;
		base.Load += delegate
		{
			LoadSettingsToUi();
			LayoutSettingsScroll();
		};
		base.Shown += delegate
		{
			ApplyScreenBounds();
			PromptVArchivePathIfNeeded();
		};
		base.Move += delegate
		{
			OnFormMovedToAnotherScreen();
		};
		_settingsScrollPanel.Resize += delegate
		{
			LayoutSettingsScroll();
		};
	}

	private void OnFormMovedToAnotherScreen()
	{
		if (base.IsHandleCreated && base.WindowState == FormWindowState.Normal)
		{
			string deviceName = Screen.FromControl(this).DeviceName;
			if (!string.Equals(_layoutScreenDeviceName, deviceName, StringComparison.Ordinal))
			{
				_layoutScreenDeviceName = deviceName;
				ApplyScreenBounds();
			}
		}
	}

	protected override void OnDpiChanged(DpiChangedEventArgs e)
	{
		base.OnDpiChanged(e);
		ApplyScreenBounds();
	}

	private void ApplyScreenBounds()
	{
		if (base.IsHandleCreated && base.WindowState == FormWindowState.Normal)
		{
			_layoutScreenDeviceName = Screen.FromControl(this).DeviceName;
			_lastLayoutContentWidth = -1;
			_lastLayoutContentHeight = -1;
			base.ClientSize = GetPreferredClientSize();
			EnsureWithinWorkingArea();
			LayoutSettingsScroll();
		}
	}

	private Size GetPreferredClientSize()
	{
		int num = 492 + SystemInformation.VerticalScrollBarWidth;
		int val = num + 112;
		int val2 = Math.Max(num, val) + base.Padding.Horizontal;
		int val3 = 520;
		if (base.IsHandleCreated)
		{
			Rectangle workingArea = Screen.FromControl(this).WorkingArea;
			int num2 = base.Width - base.ClientSize.Width;
			int num3 = base.Height - base.ClientSize.Height;
			int val4 = workingArea.Width - num2 - 16;
			int val5 = workingArea.Height - num3 - 16;
			val2 = Math.Min(val2, val4);
			val3 = Math.Min(val3, val5);
		}
		val2 = Math.Max(val2, 480 + base.Padding.Horizontal + 40);
		val3 = Math.Max(val3, 420);
		return new Size(val2, val3);
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

	public void AttachHotkey(HotkeyWindow hotkey)
	{
		_hotkey = hotkey;
	}

	protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
	{
		if (_capturingHotkey)
		{
			if (keyData == Keys.Escape)
			{
				CancelHotkeyCapture();
				return true;
			}
			if (CaptureHotkey.TryParseKeyData(keyData, out var modifiers, out var virtualKey, out var error))
			{
				_pendingHotkeyModifiers = modifiers;
				_pendingHotkeyVirtualKey = virtualKey;
				_hotkeyDisplay.Text = CaptureHotkey.FormatDisplay(modifiers, virtualKey);
				EndHotkeyCapture();
				SaveHotkeyOnly();
			}
			else
			{
				UpdateSettingsStatus(error, SettingsStatusTone.Error);
			}
			return true;
		}
		return base.ProcessCmdKey(ref msg, keyData);
	}

	private void OnFormClosing(object sender, FormClosingEventArgs e)
	{
		if (e.CloseReason != CloseReason.UserClosing || !IsDirty())
		{
			return;
		}
		switch (MessageBox.Show("저장하지 않은 설정이 있습니다.\n저장하고 종료할까요?", AppBranding.DisplayName, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question))
		{
		case DialogResult.Yes:
		{
			if (!TrySaveSettings(out var error))
			{
				e.Cancel = true;
				UpdateSettingsStatus(error, SettingsStatusTone.Error);
			}
			break;
		}
		case DialogResult.Cancel:
			e.Cancel = true;
			break;
		}
	}

	private Panel BuildSettingsPanel()
	{
		_settingsLayout = new TableLayoutPanel
		{
			ColumnCount = 2,
			AutoSize = true,
			AutoSizeMode = AutoSizeMode.GrowAndShrink,
			Margin = new Padding(0),
			MinimumSize = new Size(480, 0)
		};
		_settingsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120f));
		_settingsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 360f));
		int row = 0;
		AddSectionHeader(_settingsLayout, ref row, "표시");
		_themeLightRadio = new RadioButton
		{
			Text = "라이트",
			Appearance = Appearance.Button,
			Tag = "theme-switch",
			AutoSize = true,
			Margin = new Padding(0, 0, 4, 0)
		};
		_themeDarkRadio = new RadioButton
		{
			Text = "다크",
			Appearance = Appearance.Button,
			Tag = "theme-switch",
			AutoSize = true,
			Margin = new Padding(0)
		};
		_themeLightRadio.CheckedChanged += delegate
		{
			OnThemeSwitchChanged(_themeLightRadio);
		};
		_themeDarkRadio.CheckedChanged += delegate
		{
			OnThemeSwitchChanged(_themeDarkRadio);
		};
		FlowLayoutPanel flowLayoutPanel = new FlowLayoutPanel
		{
			AutoSize = true,
			FlowDirection = FlowDirection.LeftToRight,
			WrapContents = false,
			Dock = DockStyle.Fill,
			Margin = new Padding(0)
		};
		flowLayoutPanel.Controls.Add(_themeLightRadio);
		flowLayoutPanel.Controls.Add(_themeDarkRadio);
		AddLabeledControl(_settingsLayout, ref row, "테마", flowLayoutPanel);
		AddSectionHeader(_settingsLayout, ref row, "캡처 단축키");
		_hotkeyDisplay = new TextBox
		{
			ReadOnly = true,
			Text = CaptureHotkey.FormatDisplay(_config.CaptureHotkeyModifiers, _config.CaptureHotkeyVirtualKey),
			Dock = DockStyle.Fill,
			Margin = new Padding(0)
		};
		_hotkeyAssignButton = new Button
		{
			Text = "단축키 지정",
			AutoSize = true,
			Margin = new Padding(8, 0, 0, 0),
			Anchor = AnchorStyles.Left
		};
		_hotkeyAssignButton.Click += delegate
		{
			BeginHotkeyCapture();
		};
		Button button = new Button
		{
			Text = "캡처 동작 실행",
			AutoSize = true,
			Margin = new Padding(0, 6, 0, 0),
			Anchor = AnchorStyles.Left
		};
		button.Click += delegate
		{
			RunCapturePipeline();
		};
		TableLayoutPanel tableLayoutPanel = new TableLayoutPanel
		{
			ColumnCount = 2,
			RowCount = 2,
			AutoSize = true,
			AutoSizeMode = AutoSizeMode.GrowAndShrink,
			Dock = DockStyle.Fill,
			Margin = new Padding(0)
		};
		tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
		tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
		tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
		tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
		tableLayoutPanel.Controls.Add(_hotkeyDisplay, 0, 0);
		tableLayoutPanel.Controls.Add(_hotkeyAssignButton, 1, 0);
		tableLayoutPanel.Controls.Add(button, 0, 1);
		tableLayoutPanel.SetColumnSpan(button, 2);
		AddLabeledControl(_settingsLayout, ref row, "트리거 키", tableLayoutPanel);
		_settingsStatusLabel = new Label
		{
			AutoSize = true,
			Tag = "settings-status",
			Margin = new Padding(0)
		};
		AddFieldOnlyRow(_settingsLayout, ref row, _settingsStatusLabel);
		AddSectionHeader(_settingsLayout, ref row, "V-Archive 출력 (가로×세로)");
		_targetWidthInput = CreateDimensionInput();
		_targetHeightInput = CreateDimensionInput();
		_targetWidthInput.ValueChanged += delegate
		{
			MarkDirty();
		};
		_targetHeightInput.ValueChanged += delegate
		{
			MarkDirty();
		};
		AddLabeledControl(_settingsLayout, ref row, "가로 (px)", _targetWidthInput);
		AddLabeledControl(_settingsLayout, ref row, "세로 (px)", _targetHeightInput);
		FlowLayoutPanel flowLayoutPanel2 = new FlowLayoutPanel
		{
			AutoSize = true,
			FlowDirection = FlowDirection.TopDown,
			WrapContents = false,
			Dock = DockStyle.Fill,
			Margin = new Padding(0)
		};
		FlowLayoutPanel flowLayoutPanel3 = new FlowLayoutPanel
		{
			AutoSize = true,
			FlowDirection = FlowDirection.LeftToRight,
			WrapContents = true,
			Margin = new Padding(0, 0, 0, 4)
		};
		FlowLayoutPanel flowLayoutPanel4 = new FlowLayoutPanel
		{
			AutoSize = true,
			FlowDirection = FlowDirection.LeftToRight,
			WrapContents = true,
			Margin = new Padding(0)
		};
		for (int num = 0; num < ResolutionPresets.Length; num++)
		{
			(string Label, int Width, int Height) tuple = ResolutionPresets[num];
			string item = tuple.Label;
			int item2 = tuple.Width;
			int item3 = tuple.Height;
			Button presetButton = new Button
			{
				Text = item,
				AutoSize = true,
				Margin = new Padding(0, 0, 4, 0),
				Tag = (item2, item3)
			};
			presetButton.Click += delegate
			{
				var (item4, item5) = ((int, int))presetButton.Tag;
				ApplyPreset((Width: item4, Height: item5));
			};
			if (num < 3)
			{
				flowLayoutPanel3.Controls.Add(presetButton);
			}
			else
			{
				flowLayoutPanel4.Controls.Add(presetButton);
			}
		}
		Button button2 = new Button
		{
			Text = "선택 모니터 해상도",
			AutoSize = true,
			Margin = new Padding(0, 4, 0, 0)
		};
		button2.Click += delegate
		{
			ApplyCurrentMonitorResolution();
		};
		FlowLayoutPanel flowLayoutPanel5 = new FlowLayoutPanel
		{
			AutoSize = true,
			FlowDirection = FlowDirection.LeftToRight,
			WrapContents = false,
			Margin = new Padding(0)
		};
		flowLayoutPanel5.Controls.Add(button2);
		flowLayoutPanel2.Controls.Add(flowLayoutPanel3);
		flowLayoutPanel2.Controls.Add(flowLayoutPanel4);
		flowLayoutPanel2.Controls.Add(flowLayoutPanel5);
		AddLabeledControl(_settingsLayout, ref row, "프리셋", flowLayoutPanel2);
		AddSectionHeader(_settingsLayout, ref row, "캡처 대상");
		_monitorCombo = new ThemedComboBox
		{
			DropDownStyle = ComboBoxStyle.DropDownList
		};
		_monitorCombo.SelectedIndexChanged += delegate
		{
			MarkDirty();
		};
		AddLabeledControl(_settingsLayout, ref row, "모니터", _monitorCombo);
		AddSectionHeader(_settingsLayout, ref row, "V-Archive");
		_vArchivePathInput = new TextBox
		{
			Dock = DockStyle.Fill,
			Margin = new Padding(0)
		};
		_vArchivePathInput.TextChanged += delegate
		{
			MarkDirty();
		};
		TableLayoutPanel tableLayoutPanel2 = new TableLayoutPanel
		{
			ColumnCount = 2,
			AutoSize = true,
			AutoSizeMode = AutoSizeMode.GrowAndShrink,
			Dock = DockStyle.Fill,
			Margin = new Padding(0)
		};
		tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
		tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
		tableLayoutPanel2.Controls.Add(_vArchivePathInput, 0, 0);
		Button button3 = new Button
		{
			Text = "찾기",
			AutoSize = true,
			Margin = new Padding(8, 0, 0, 0),
			Anchor = AnchorStyles.Left
		};
		button3.Click += delegate
		{
			BrowseVArchivePath();
		};
		tableLayoutPanel2.Controls.Add(button3, 1, 0);
		_useDxgiCheck = new ThemedCheckBox
		{
			Text = "DXGI 캡처"
		};
		_requireDxgiCheck = new ThemedCheckBox
		{
			Text = "DXGI 필수(권장)"
		};
		_usePhysicalPixelsCheck = new ThemedCheckBox
		{
			Text = "물리 픽셀"
		};
		_useDxgiCheck.CheckedChanged += delegate
		{
			MarkDirty();
		};
		_requireDxgiCheck.CheckedChanged += delegate
		{
			MarkDirty();
		};
		_usePhysicalPixelsCheck.CheckedChanged += delegate
		{
			MarkDirty();
			RefreshMonitorCombo(preserveIndex: true);
		};
		FlowLayoutPanel flowLayoutPanel6 = new FlowLayoutPanel
		{
			AutoSize = true,
			FlowDirection = FlowDirection.TopDown,
			WrapContents = false,
			Dock = DockStyle.Fill,
			Margin = new Padding(0)
		};
		flowLayoutPanel6.Controls.Add(_useDxgiCheck);
		flowLayoutPanel6.Controls.Add(_requireDxgiCheck);
		flowLayoutPanel6.Controls.Add(_usePhysicalPixelsCheck);
		AddLabeledControl(_settingsLayout, ref row, "exe 경로", tableLayoutPanel2);
		AddLabeledControl(_settingsLayout, ref row, "옵션", flowLayoutPanel6);
		return new Panel
		{
			AutoSize = true,
			AutoSizeMode = AutoSizeMode.GrowAndShrink,
			Margin = new Padding(0),
			Controls = { (Control)_settingsLayout }
		};
	}

	private void UpdateSettingsStatus(string text, SettingsStatusTone tone)
	{
		_settingsStatusTone = tone;
		_settingsStatusLabel.Text = text;
		_settingsStatusLabel.ForeColor = UiTheme.StatusColor(tone);
	}

	private bool IsDarkThemeSelected()
	{
		return _themeDarkRadio?.Checked ?? false;
	}

	private void SetThemeSwitch(bool dark)
	{
		if (_themeLightRadio != null && _themeDarkRadio != null)
		{
			_themeLightRadio.Checked = !dark;
			_themeDarkRadio.Checked = dark;
		}
	}

	private void OnThemeSwitchChanged(RadioButton source)
	{
		if (!_suppressDirty && _themeLightRadio != null && _themeDarkRadio != null && source.Checked)
		{
			_config.UiTheme = (IsDarkThemeSelected() ? "Dark" : "Light");
			ApplyUiTheme();
			MarkDirty();
		}
	}

	private void ApplyUiTheme()
	{
		UiTheme.SetMode(UiTheme.Parse(_config.UiTheme));
		UiTheme.ApplyTo(this);
		UiTheme.ApplyThemeSwitch(_themeLightRadio, _themeDarkRadio);
		_captureStatusLabel.ForeColor = UiTheme.Palette.AccentSecondary;
		_settingsStatusLabel.ForeColor = UiTheme.StatusColor(_settingsStatusTone);
		_settingsScrollPanel.BackColor = UiTheme.Palette.WindowBackground;
		UsageManualForm.ApplyCurrentThemeIfOpen();
		LayoutSettingsScroll();
	}

	private void LayoutSettingsScroll()
	{
		if (_settingsScrollPanel != null && _settingsContentPanel != null && _settingsLayout != null)
		{
			int num = _settingsScrollPanel.ClientSize.Width - _settingsScrollPanel.Padding.Horizontal;
			int num2 = 480;
			if (num < num2)
			{
				num = num2;
			}
			bool num3 = num != _lastLayoutContentWidth;
			if (num3)
			{
				_settingsLayout.Width = num;
				_lastLayoutContentWidth = num;
			}
			_settingsLayout.PerformLayout();
			_settingsContentPanel.PerformLayout();
			int num4 = _settingsContentPanel.PreferredSize.Height;
			if (num3 || num4 != _lastLayoutContentHeight)
			{
				_lastLayoutContentHeight = num4;
				_settingsContentPanel.Location = new Point(0, 0);
				_settingsScrollPanel.AutoScrollPosition = new Point(0, 0);
				_settingsScrollPanel.AutoScrollMinSize = new Size(0, num4);
			}
		}
	}

	private void BeginHotkeyCapture()
	{
		_capturingHotkey = true;
		_hotkeyAssignButton.Text = "키 입력… (Esc 취소)";
		_hotkeyAssignButton.Enabled = false;
		_hotkeyDisplay.Text = "키를 누르세요…";
		UpdateSettingsStatus("조합키 예: Ctrl+Shift+F12", SettingsStatusTone.Neutral);
	}

	private void CancelHotkeyCapture()
	{
		EndHotkeyCapture();
		_hotkeyDisplay.Text = CaptureHotkey.FormatDisplay(_pendingHotkeyModifiers, _pendingHotkeyVirtualKey);
		UpdateSettingsStatus("단축키 지정이 취소되었습니다.", SettingsStatusTone.Neutral);
	}

	private void EndHotkeyCapture()
	{
		_capturingHotkey = false;
		_hotkeyAssignButton.Text = "단축키 지정";
		_hotkeyAssignButton.Enabled = true;
	}

	private void SaveHotkeyOnly()
	{
		_config.CaptureHotkeyModifiers = _pendingHotkeyModifiers;
		_config.CaptureHotkeyVirtualKey = _pendingHotkeyVirtualKey;
		if (!CaptureHotkey.IsValidForRegistration(_config.CaptureHotkeyModifiers, _config.CaptureHotkeyVirtualKey, out var error))
		{
			UpdateSettingsStatus(error, SettingsStatusTone.Error);
			return;
		}
		HelperConfig backup = _config.Clone();
		try
		{
			_config.Save();
		}
		catch (Exception ex)
		{
			RestoreConfig(backup);
			UpdateSettingsStatus("단축키 저장 실패: " + ex.Message, SettingsStatusTone.Error);
			return;
		}
		if (_hotkey != null && !_hotkey.RegisterFromConfig())
		{
			RestoreConfig(backup);
			try
			{
				_config.Save();
			}
			catch
			{
			}
			_pendingHotkeyModifiers = _config.CaptureHotkeyModifiers;
			_pendingHotkeyVirtualKey = _config.CaptureHotkeyVirtualKey;
			_hotkeyDisplay.Text = CaptureHotkey.FormatDisplay(_pendingHotkeyModifiers, _pendingHotkeyVirtualKey);
			_hotkey?.RegisterFromConfig();
			UpdateSettingsStatus("단축키 등록 실패 — 다른 조합을 시도하세요.", SettingsStatusTone.Error);
		}
		else
		{
			_lastSaved = _config.Clone();
			UpdateSettingsStatus("단축키 저장됨 — " + CaptureHotkey.FormatDisplay(_config.CaptureHotkeyModifiers, _config.CaptureHotkeyVirtualKey), SettingsStatusTone.Success);
		}
	}

	private static NumericUpDown CreateDimensionInput()
	{
		return new ScrollSafeNumericUpDown
		{
			Minimum = 320m,
			Maximum = 7680m,
			Increment = 1m,
			Dock = DockStyle.Fill,
			Margin = new Padding(0)
		};
	}

	private static void AddSectionHeader(TableLayoutPanel layout, ref int row, string text)
	{
		Label control = new Label
		{
			Text = text,
			Tag = "section-header",
			Font = UiFonts.Bold(10f),
			AutoSize = true,
			Margin = new Padding(0, (row == 0) ? 4 : 20, 0, 12)
		};
		layout.Controls.Add(control, 0, row);
		layout.SetColumnSpan(control, 2);
		row++;
	}

	private static void AddLabeledControl(TableLayoutPanel layout, ref int row, string label, Control control)
	{
		layout.Controls.Add(CreateFieldLabel(label), 0, row);
		PrepareFieldControl(control);
		layout.Controls.Add(control, 1, row);
		row++;
	}

	private static void AddFieldOnlyRow(TableLayoutPanel layout, ref int row, Control control)
	{
		layout.Controls.Add(CreateFieldLabel(string.Empty), 0, row);
		control.Dock = DockStyle.None;
		control.Anchor = AnchorStyles.Top | AnchorStyles.Left;
		control.Margin = new Padding(0, 4, 0, 8);
		layout.Controls.Add(control, 1, row);
		row++;
	}

	private static Label CreateFieldLabel(string text)
	{
		return new Label
		{
			Text = text,
			AutoSize = false,
			Dock = DockStyle.Fill,
			TextAlign = ContentAlignment.MiddleLeft,
			Margin = new Padding(0, 4, 10, 4)
		};
	}

	private static void PrepareFieldControl(Control control)
	{
		control.Dock = DockStyle.Fill;
		control.Margin = new Padding(0, 4, 0, 8);
	}

	private void LoadSettingsToUi()
	{
		_suppressDirty = true;
		try
		{
			_pendingHotkeyModifiers = _config.CaptureHotkeyModifiers;
			_pendingHotkeyVirtualKey = _config.CaptureHotkeyVirtualKey;
			_hotkeyDisplay.Text = CaptureHotkey.FormatDisplay(_pendingHotkeyModifiers, _pendingHotkeyVirtualKey);
			RefreshMonitorCombo(preserveIndex: true);
			MonitorList.SelectByIndex(_monitorCombo, _config.MonitorIndex);
			_targetWidthInput.Value = Math.Max(_targetWidthInput.Minimum, Math.Min(_targetWidthInput.Maximum, _config.TargetWidth));
			_targetHeightInput.Value = Math.Max(_targetHeightInput.Minimum, Math.Min(_targetHeightInput.Maximum, _config.TargetHeight));
			_vArchivePathInput.Text = _config.VArchiveExePath ?? "";
			_useDxgiCheck.Checked = _config.UseDxgiCapture;
			_requireDxgiCheck.Checked = _config.RequireDxgiCapture;
			_usePhysicalPixelsCheck.Checked = _config.UsePhysicalPixels;
			SetThemeSwitch(string.Equals(_config.UiTheme, "Dark", StringComparison.OrdinalIgnoreCase));
			UpdateSettingsStatus("", SettingsStatusTone.Neutral);
			_lastSaved = BuildConfigFromUi().Clone();
			ApplyUiTheme();
		}
		finally
		{
			_suppressDirty = false;
		}
	}

	private void RefreshMonitorCombo(bool preserveIndex)
	{
		int index = (preserveIndex ? MonitorList.GetSelectedIndex(_monitorCombo) : _config.MonitorIndex);
		_monitorCombo.Items.Clear();
		foreach (MonitorComboItem item in MonitorList.Build(_usePhysicalPixelsCheck.Checked))
		{
			_monitorCombo.Items.Add(item);
		}
		if (_monitorCombo.Items.Count != 0)
		{
			MonitorList.SelectByIndex(_monitorCombo, index);
		}
	}

	private void MarkDirty()
	{
		if (!_suppressDirty && IsDirty())
		{
			Text = AppBranding.SettingsTitle(unsaved: true);
		}
	}

	private bool IsDirty()
	{
		return !ConfigEquals(BuildConfigFromUi(), _lastSaved);
	}

	private HelperConfig BuildConfigFromUi()
	{
		return new HelperConfig
		{
			ConfigVersion = _config.ConfigVersion,
			MonitorIndex = MonitorList.GetSelectedIndex(_monitorCombo),
			TargetWidth = (int)_targetWidthInput.Value,
			TargetHeight = (int)_targetHeightInput.Value,
			VArchiveExePath = _vArchivePathInput.Text.Trim(),
			UseDxgiCapture = _useDxgiCheck.Checked,
			RequireDxgiCapture = _requireDxgiCheck.Checked,
			UsePhysicalPixels = _usePhysicalPixelsCheck.Checked,
			CaptureHotkeyModifiers = _pendingHotkeyModifiers,
			CaptureHotkeyVirtualKey = _pendingHotkeyVirtualKey,
			VArchiveProcessName = _config.VArchiveProcessName,
			ClipboardSettleMs = _config.ClipboardSettleMs,
			AfterClipboardBeforeRecognizeMs = _config.AfterClipboardBeforeRecognizeMs,
			VArchiveStartupWaitMs = _config.VArchiveStartupWaitMs,
			GameProcessName = _config.GameProcessName,
			UseGameWindowCapture = _config.UseGameWindowCapture,
			UiTheme = (IsDarkThemeSelected() ? "Dark" : "Light")
		};
	}

	private static bool ConfigEquals(HelperConfig a, HelperConfig b)
	{
		if (a.MonitorIndex == b.MonitorIndex && a.TargetWidth == b.TargetWidth && a.TargetHeight == b.TargetHeight && string.Equals(a.VArchiveExePath?.Trim(), b.VArchiveExePath?.Trim(), StringComparison.OrdinalIgnoreCase) && a.UseDxgiCapture == b.UseDxgiCapture && a.RequireDxgiCapture == b.RequireDxgiCapture && a.UsePhysicalPixels == b.UsePhysicalPixels && a.CaptureHotkeyModifiers == b.CaptureHotkeyModifiers && a.CaptureHotkeyVirtualKey == b.CaptureHotkeyVirtualKey)
		{
			return string.Equals(a.UiTheme, b.UiTheme, StringComparison.OrdinalIgnoreCase);
		}
		return false;
	}

	private void ApplyPreset((int Width, int Height) size)
	{
		_targetWidthInput.Value = size.Width;
		_targetHeightInput.Value = size.Height;
		UpdateSettingsStatus($"프리셋 {size.Width}×{size.Height} — 적용 및 저장을 누르세요.", SettingsStatusTone.Neutral);
		MarkDirty();
	}

	private void ApplyCurrentMonitorResolution()
	{
		int selectedIndex = MonitorList.GetSelectedIndex(_monitorCombo);
		Screen[] allScreens = Screen.AllScreens;
		if (selectedIndex < 0 || selectedIndex >= allScreens.Length)
		{
			UpdateSettingsStatus($"Index {selectedIndex + 1} 없음", SettingsStatusTone.Error);
			return;
		}
		MonitorCaptureInfo captureInfo = MonitorGeometry.GetCaptureInfo(allScreens[selectedIndex], _usePhysicalPixelsCheck.Checked);
		_targetWidthInput.Value = Math.Max(_targetWidthInput.Minimum, Math.Min(_targetWidthInput.Maximum, captureInfo.NativeResolution.Width));
		_targetHeightInput.Value = Math.Max(_targetHeightInput.Minimum, Math.Min(_targetHeightInput.Maximum, captureInfo.NativeResolution.Height));
		UpdateSettingsStatus($"Index {selectedIndex + 1} → {captureInfo.NativeResolution.Width}×{captureInfo.NativeResolution.Height}", SettingsStatusTone.Neutral);
		MarkDirty();
	}

	private void PromptVArchivePathIfNeeded()
	{
		if (!VArchivePathSetupForm.IsSetupRequired(_config))
		{
			return;
		}
		using VArchivePathSetupForm vArchivePathSetupForm = new VArchivePathSetupForm(_config);
		if (vArchivePathSetupForm.ShowDialog(this) == DialogResult.OK)
		{
			_suppressDirty = true;
			_vArchivePathInput.Text = _config.VArchiveExePath;
			_lastSaved.VArchiveExePath = _config.VArchiveExePath;
			_suppressDirty = false;
			UpdateSettingsStatus("V-Archive 경로가 저장되었습니다.", SettingsStatusTone.Success);
		}
		else
		{
			UpdateSettingsStatus("V-Archive 경로를 설정해야 캡처·인식이 동작합니다.", SettingsStatusTone.Error);
		}
	}

	private void BrowseVArchivePath()
	{
		if (VArchivePathPicker.TryBrowse(this, _vArchivePathInput.Text, out var selectedPath))
		{
			_vArchivePathInput.Text = selectedPath;
			MarkDirty();
		}
	}

	private void ApplyAndSaveSettings()
	{
		if (!TrySaveSettings(out var error))
		{
			UpdateSettingsStatus(error, SettingsStatusTone.Error);
		}
		else
		{
			UpdateSettingsStatus($"저장됨 — {_config.TargetWidth}×{_config.TargetHeight}, {CaptureHotkey.FormatDisplay(_config.CaptureHotkeyModifiers, _config.CaptureHotkeyVirtualKey)}", SettingsStatusTone.Success);
		}
	}

	private bool TrySaveSettings(out string error)
	{
		HelperConfig backup = _config.Clone();
		ApplyUiToConfig();
		if (!_config.TryValidate(out error))
		{
			RestoreConfig(backup);
			LoadSettingsToUi();
			return false;
		}
		try
		{
			_config.Save();
		}
		catch (Exception ex)
		{
			RestoreConfig(backup);
			LoadSettingsToUi();
			error = "저장 실패: " + ex.Message;
			return false;
		}
		if (_hotkey != null && !_hotkey.RegisterFromConfig())
		{
			RestoreConfig(backup);
			try
			{
				_config.Save();
			}
			catch
			{
			}
			LoadSettingsToUi();
			_hotkey.RegisterFromConfig();
			error = "단축키 등록 실패 — 다른 조합을 사용하거나 충돌 프로그램을 종료하세요.";
			return false;
		}
		_lastSaved = _config.Clone();
		Text = AppBranding.SettingsTitle();
		error = null;
		return true;
	}

	private void ApplyUiToConfig()
	{
		_config.MonitorIndex = MonitorList.GetSelectedIndex(_monitorCombo);
		_config.TargetWidth = (int)_targetWidthInput.Value;
		_config.TargetHeight = (int)_targetHeightInput.Value;
		_config.VArchiveExePath = _vArchivePathInput.Text.Trim();
		_config.UseDxgiCapture = _useDxgiCheck.Checked;
		_config.RequireDxgiCapture = _requireDxgiCheck.Checked;
		_config.UsePhysicalPixels = _usePhysicalPixelsCheck.Checked;
		_config.CaptureHotkeyModifiers = _pendingHotkeyModifiers;
		_config.CaptureHotkeyVirtualKey = _pendingHotkeyVirtualKey;
		_config.UiTheme = (IsDarkThemeSelected() ? "Dark" : "Light");
	}

	private void RestoreConfig(HelperConfig backup)
	{
		_config.MonitorIndex = backup.MonitorIndex;
		_config.TargetWidth = backup.TargetWidth;
		_config.TargetHeight = backup.TargetHeight;
		_config.VArchiveExePath = backup.VArchiveExePath;
		_config.UseDxgiCapture = backup.UseDxgiCapture;
		_config.RequireDxgiCapture = backup.RequireDxgiCapture;
		_config.UsePhysicalPixels = backup.UsePhysicalPixels;
		_config.CaptureHotkeyModifiers = backup.CaptureHotkeyModifiers;
		_config.CaptureHotkeyVirtualKey = backup.CaptureHotkeyVirtualKey;
		_config.ConfigVersion = backup.ConfigVersion;
		_config.UiTheme = backup.UiTheme;
	}

	public void SetCaptureStatus(string message)
	{
		if (!base.IsDisposed)
		{
			if (base.InvokeRequired)
			{
				BeginInvoke(new Action(Apply));
			}
			else
			{
				Apply();
			}
		}
		void Apply()
		{
			_captureStatusLabel.Text = "상태: " + message;
			_captureStatusLabel.ForeColor = UiTheme.Palette.AccentSecondary;
		}
	}

	public async void RunCapturePipeline()
	{
		if (_captureInProgress)
		{
			return;
		}
		_captureInProgress = true;
		SetCaptureStatus("캡처하는 중입니다.");
		try
		{
			SetCaptureStatus(await CapturePipeline.RunAsync(_config).ConfigureAwait(continueOnCapturedContext: true));
		}
		finally
		{
			_captureInProgress = false;
		}
	}
}
