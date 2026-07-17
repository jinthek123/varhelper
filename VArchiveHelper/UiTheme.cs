using System;
using System.Drawing;
using System.Windows.Forms;

namespace VArchiveHelper;

internal static class UiTheme
{
	private static readonly UiThemePalette LightPalette = new UiThemePalette
	{
		WindowBackground = Color.FromArgb(243, 244, 248),
		PrimaryText = Color.FromArgb(26, 29, 38),
		SecondaryText = Color.FromArgb(92, 99, 120),
		Accent = Color.FromArgb(227, 139, 130),
		AccentSecondary = Color.FromArgb(227, 139, 130),
		Success = Color.FromArgb(46, 155, 106),
		Error = Color.FromArgb(212, 76, 85),
		InputBackground = Color.White,
		InputText = Color.FromArgb(26, 29, 38),
		InputBorder = Color.FromArgb(216, 220, 232),
		CardBackground = Color.White,
		CardBorder = Color.FromArgb(216, 220, 232),
		SectionTitle = Color.FromArgb(179, 94, 86),
		CardBody = Color.FromArgb(74, 80, 104),
		ButtonBackground = Color.White,
		ButtonBorder = Color.FromArgb(216, 220, 232),
		ButtonSelectedBackground = Color.FromArgb(227, 139, 130),
		ButtonSelectedText = Color.White
	};

	private static readonly UiThemePalette DarkPalette = new UiThemePalette
	{
		WindowBackground = Color.FromArgb(17, 19, 26),
		PrimaryText = Color.FromArgb(230, 232, 240),
		SecondaryText = Color.FromArgb(148, 155, 179),
		Accent = Color.FromArgb(241, 167, 159),
		AccentSecondary = Color.FromArgb(241, 167, 159),
		Success = Color.FromArgb(90, 212, 168),
		Error = Color.FromArgb(240, 113, 120),
		InputBackground = Color.FromArgb(22, 25, 37),
		InputText = Color.FromArgb(230, 232, 240),
		InputBorder = Color.FromArgb(46, 51, 72),
		CardBackground = Color.FromArgb(28, 32, 48),
		CardBorder = Color.FromArgb(46, 51, 72),
		SectionTitle = Color.FromArgb(241, 167, 159),
		CardBody = Color.FromArgb(184, 191, 212),
		ButtonBackground = Color.FromArgb(28, 32, 48),
		ButtonBorder = Color.FromArgb(46, 51, 72),
		ButtonSelectedBackground = Color.FromArgb(227, 139, 130),
		ButtonSelectedText = Color.White
	};

	public static UiThemeMode Current { get; private set; } = UiThemeMode.Light;

	public static UiThemePalette Palette
	{
		get
		{
			if (Current != UiThemeMode.Dark)
			{
				return LightPalette;
			}
			return DarkPalette;
		}
	}

	public static void SetMode(UiThemeMode mode)
	{
		Current = mode;
	}

	public static UiThemeMode Parse(string value)
	{
		if (!string.Equals(value, "Dark", StringComparison.OrdinalIgnoreCase))
		{
			return UiThemeMode.Light;
		}
		return UiThemeMode.Dark;
	}

	public static string ToConfigValue(UiThemeMode mode)
	{
		if (mode != UiThemeMode.Dark)
		{
			return "Light";
		}
		return "Dark";
	}

	public static Color StatusColor(SettingsStatusTone tone)
	{
		return tone switch
		{
			SettingsStatusTone.Success => Palette.AccentSecondary, 
			SettingsStatusTone.Error => Palette.Error, 
			_ => Palette.SecondaryText, 
		};
	}

	public static void ApplyTo(Form form)
	{
		UiThemePalette palette = Palette;
		form.BackColor = palette.WindowBackground;
		form.ForeColor = palette.PrimaryText;
		ApplyRecursive(form, palette);
		UiFonts.ApplyTo(form);
	}

	public static void ApplyThemeSwitch(RadioButton light, RadioButton dark)
	{
		if (light != null && dark != null)
		{
			ApplySwitchButton(light, light.Checked);
			ApplySwitchButton(dark, dark.Checked);
		}
	}

	private static void ApplySwitchButton(RadioButton radioButton, bool selected)
	{
		radioButton.Font = UiFonts.Regular();
		radioButton.Padding = new Padding(10, 3, 10, 3);
		UiRounded.AttachButtonPaint(radioButton, selected ? RoundButtonStyle.ThemeSwitchOn : RoundButtonStyle.ThemeSwitchOff);
	}

	private static void ApplyRecursive(Control control, UiThemePalette palette)
	{
		if (!(control is Label label))
		{
			if (!(control is TextBox textBox))
			{
				if (!(control is ComboBox comboBox))
				{
					if (!(control is NumericUpDown numericUpDown))
					{
						if (!(control is ThemedCheckBox themedCheckBox))
						{
							if (!(control is CheckBox checkBox))
							{
								if (!(control is RadioButton radioButton))
								{
									if (!(control is Button button))
									{
										if (!(control is TableLayoutPanel tableLayoutPanel))
										{
											if (!(control is FlowLayoutPanel flowLayoutPanel))
											{
												if (control is Panel panel)
												{
													if (string.Equals(panel.Tag as string, "theme-card", StringComparison.Ordinal))
													{
														UiRounded.AttachCardPaint(panel);
													}
													else
													{
														panel.BackColor = palette.WindowBackground;
													}
												}
											}
											else
											{
												flowLayoutPanel.BackColor = palette.WindowBackground;
											}
										}
										else if (string.Equals(tableLayoutPanel.Tag as string, "theme-card", StringComparison.Ordinal))
										{
											UiRounded.AttachCardPaint(tableLayoutPanel);
										}
										else
										{
											tableLayoutPanel.BackColor = palette.WindowBackground;
										}
									}
									else if (string.Equals(button.Tag as string, "accent", StringComparison.Ordinal))
									{
										ApplyAccentButton(button, palette);
									}
									else
									{
										Button button2 = button;
										if (string.Equals(button2.Tag as string, "primary", StringComparison.Ordinal))
										{
											ApplyPrimaryButton(button2, palette);
										}
										else
										{
											ApplyStandardButton(button, palette);
										}
									}
								}
								else if (!string.Equals(radioButton.Tag as string, "theme-switch", StringComparison.Ordinal))
								{
									radioButton.ForeColor = palette.PrimaryText;
									radioButton.BackColor = palette.WindowBackground;
								}
							}
							else
							{
								checkBox.ForeColor = palette.PrimaryText;
								checkBox.BackColor = palette.WindowBackground;
							}
						}
						else
						{
							themedCheckBox.ForeColor = palette.PrimaryText;
							themedCheckBox.BackColor = palette.WindowBackground;
							themedCheckBox.Invalidate();
						}
					}
					else
					{
						numericUpDown.BackColor = palette.InputBackground;
						numericUpDown.ForeColor = palette.InputText;
						numericUpDown.BorderStyle = BorderStyle.FixedSingle;
						UiThemedInputs.AttachNumericUpDown(numericUpDown);
					}
				}
				else
				{
					comboBox.BackColor = palette.InputBackground;
					comboBox.ForeColor = palette.InputText;
					UiThemedInputs.AttachComboBox(comboBox);
				}
			}
			else
			{
				textBox.BackColor = palette.InputBackground;
				textBox.ForeColor = palette.InputText;
				textBox.BorderStyle = BorderStyle.FixedSingle;
			}
		}
		else if (string.Equals(label.Tag as string, "accent", StringComparison.Ordinal))
		{
			label.ForeColor = palette.AccentSecondary;
		}
		else if (!string.Equals(label.Tag as string, "settings-status", StringComparison.Ordinal))
		{
			Label label2 = label;
			if (string.Equals(label2.Tag as string, "section-header", StringComparison.Ordinal))
			{
				label2.ForeColor = palette.SectionTitle;
				label2.Font = UiFonts.Bold(10f);
			}
			else
			{
				Label label3 = label;
				label3.ForeColor = palette.PrimaryText;
				Font font = label3.Font;
				if (font != null && font.Style == FontStyle.Bold)
				{
					label3.Font = UiFonts.Bold(label3.Font.Size);
				}
			}
		}
		foreach (Control control2 in control.Controls)
		{
			ApplyRecursive(control2, palette);
		}
	}

	private static void ApplyStandardButton(Button button, UiThemePalette palette)
	{
		button.Font = UiFonts.Regular();
		button.Padding = new Padding(8, 2, 8, 2);
		UiRounded.AttachButtonPaint(button, RoundButtonStyle.Standard);
	}

	private static void ApplyAccentButton(Button button, UiThemePalette palette)
	{
		button.Font = UiFonts.Bold();
		button.Padding = new Padding(10, 3, 10, 3);
		UiRounded.AttachButtonPaint(button, RoundButtonStyle.Accent);
	}

	private static void ApplyPrimaryButton(Button button, UiThemePalette palette)
	{
		button.Font = UiFonts.Bold();
		button.Padding = new Padding(10, 4, 10, 4);
		UiRounded.AttachButtonPaint(button, RoundButtonStyle.Primary);
	}
}
