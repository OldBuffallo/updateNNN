# Local installer styling. Never changes the Windows theme or registry.
$script:installerColors = @{
    Window = [Drawing.ColorTranslator]::FromHtml('#E6F3F0')
    Surface = [Drawing.ColorTranslator]::FromHtml('#FFFFFF')
    Input = [Drawing.ColorTranslator]::FromHtml('#FFFFFF')
    Header = [Drawing.ColorTranslator]::FromHtml('#11534E')
    Text = [Drawing.ColorTranslator]::FromHtml('#0F2E2B')
    Muted = [Drawing.ColorTranslator]::FromHtml('#4A6B66')
    Accent = [Drawing.ColorTranslator]::FromHtml('#157A72')
    Button = [Drawing.ColorTranslator]::FromHtml('#157A72')
    ButtonHover = [Drawing.ColorTranslator]::FromHtml('#0E5E57')
    Border = [Drawing.ColorTranslator]::FromHtml('#B7D5CE')
    Success = [Drawing.ColorTranslator]::FromHtml('#15803D')
    Warning = [Drawing.ColorTranslator]::FromHtml('#8A4B0F')
    Error = [Drawing.ColorTranslator]::FromHtml('#BE123C')
}

if (-not ('IrmInstaller.DarkListView' -as [type])) {
    Add-Type -ReferencedAssemblies System.Windows.Forms,System.Drawing -TypeDefinition @'
using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace IrmInstaller
{
    public static class WindowTheme
    {
        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr window, int attribute, ref int value, int size);
        public static void ApplyTitleBar(Form form)
        {
            // Older Windows versions may not support this attribute. Body colors still apply.
            try { int dark = 0; DwmSetWindowAttribute(form.Handle, 20, ref dark, sizeof(int)); }
            catch (DllNotFoundException) { }
            catch (EntryPointNotFoundException) { }
        }
    }

    public class DarkCheckBox : CheckBox
    {
        protected override void OnPaint(PaintEventArgs e)
        {
            if (Enabled) { base.OnPaint(e); return; }
            e.Graphics.Clear(BackColor);
            var state = Checked ? System.Windows.Forms.VisualStyles.CheckBoxState.CheckedDisabled : System.Windows.Forms.VisualStyles.CheckBoxState.UncheckedDisabled;
            CheckBoxRenderer.DrawCheckBox(e.Graphics, new Point(0,Math.Max(0,(Height-14)/2)), state);
            TextRenderer.DrawText(e.Graphics, Text, Font, new Rectangle(18,0,Math.Max(1,Width-18),Height), Color.FromArgb(74,107,102),
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);
        }
    }

    public class DarkButton : Button
    {
        protected override void OnPaint(PaintEventArgs e)
        {
            if (Enabled) { base.OnPaint(e); return; }
            e.Graphics.Clear(Color.FromArgb(230,243,240));
            using (var pen = new Pen(Color.FromArgb(183,213,206)))
                e.Graphics.DrawRectangle(pen, 0, 0, Width-1, Height-1);
            TextRenderer.DrawText(e.Graphics, Text, Font, ClientRectangle, Color.FromArgb(74,107,102),
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
        }
    }

    public class DarkListView : ListView
    {
        private static readonly Color Header = Color.FromArgb(230,243,240);
        private static readonly Color Alternate = Color.FromArgb(242,249,247);
        private static readonly Color Selection = Color.FromArgb(211,235,229);
        private static readonly Color Line = Color.FromArgb(183,213,206);
        public DarkListView()
        {
            OwnerDraw = true;
            DoubleBuffered = true;
            BackColor = Color.White;
            ForeColor = Color.FromArgb(15,46,43);
            GridLines = false;
            HideSelection = false;
            ShowItemToolTips = true;
        }
        private void FillDetailColumn()
        {
            if (Columns.Count == 3)
                Columns[2].Width = Math.Max(300, ClientSize.Width-Columns[0].Width-Columns[1].Width);
        }
        protected override void OnHandleCreated(EventArgs e) { base.OnHandleCreated(e); FillDetailColumn(); }
        protected override void OnResize(EventArgs e) { base.OnResize(e); FillDetailColumn(); }
        protected override void OnDrawColumnHeader(DrawListViewColumnHeaderEventArgs e)
        {
            using (var brush = new SolidBrush(Header)) e.Graphics.FillRectangle(brush, e.Bounds);
            using (var pen = new Pen(Line)) e.Graphics.DrawLine(pen, e.Bounds.Right-1, e.Bounds.Top, e.Bounds.Right-1, e.Bounds.Bottom);
            var text = Rectangle.Inflate(e.Bounds, -7, 0);
            using (var font = new Font(Font, FontStyle.Bold))
                TextRenderer.DrawText(e.Graphics, e.Header.Text, font, text, ForeColor,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
        }
        protected override void OnDrawItem(DrawListViewItemEventArgs e)
        {
            if (View != View.Details) e.DrawDefault = true;
        }
        protected override void OnDrawSubItem(DrawListViewSubItemEventArgs e)
        {
            var background = e.Item.Selected ? Selection : (e.ItemIndex % 2 == 0 ? BackColor : Alternate);
            using (var brush = new SolidBrush(background)) e.Graphics.FillRectangle(brush, e.Bounds);
            var text = Rectangle.Inflate(e.Bounds, -7, 0);
            if (e.ColumnIndex == 0 && CheckBoxes)
            {
                int size = Math.Max(10, Math.Min(14, e.Bounds.Height-4));
                var box = new Rectangle(e.Bounds.Left+6, e.Bounds.Top+(e.Bounds.Height-size)/2, size, size);
                using (var brush = new SolidBrush(Color.White)) e.Graphics.FillRectangle(brush, box);
                using (var pen = new Pen(Color.FromArgb(74,107,102))) e.Graphics.DrawRectangle(pen, box);
                if (e.Item.Checked)
                    using (var pen = new Pen(Color.FromArgb(21,128,61), 2))
                        e.Graphics.DrawLines(pen, new [] { new Point(box.Left+3, box.Top+size/2), new Point(box.Left+size/2, box.Bottom-3), new Point(box.Right-2, box.Top+3) });
                text.X = box.Right+7;
                text.Width = Math.Max(1, e.Bounds.Right-text.X-5);
            }
            TextRenderer.DrawText(e.Graphics, e.SubItem.Text, Font, text, e.Item.ForeColor,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.SingleLine | TextFormatFlags.NoPrefix);
            using (var pen = new Pen(Line))
                e.Graphics.DrawLine(pen, e.Bounds.Left, e.Bounds.Bottom-1, e.Bounds.Right, e.Bounds.Bottom-1);
        }
    }

    public class DarkProgressBar : Control
    {
        private readonly Timer animation = new Timer();
        private int value;
        private int position;
        private int animationSpeed = 30;
        private ProgressBarStyle style;
        public DarkProgressBar()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
            BackColor = Color.FromArgb(211,235,229);
            ForeColor = Color.FromArgb(21,122,114);
            AccessibleRole = AccessibleRole.ProgressBar;
            animation.Interval = 30;
            animation.Tick += delegate { position = (position+6) % Math.Max(1, Width+Width/4); Invalidate(); };
        }
        public int Value { get { return value; } set { this.value = Math.Max(0,Math.Min(100,value)); AccessibleDescription = this.value+"%"; Invalidate(); } }
        public ProgressBarStyle Style { get { return style; } set { style = value; UpdateAnimation(); Invalidate(); } }
        public int MarqueeAnimationSpeed { get { return animationSpeed; } set { animationSpeed = Math.Max(1,value); animation.Interval = animationSpeed; } }
        private void UpdateAnimation() { animation.Enabled = Visible && style == ProgressBarStyle.Marquee; }
        protected override void OnVisibleChanged(EventArgs e) { base.OnVisibleChanged(e); UpdateAnimation(); }
        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.Clear(BackColor);
            using (var brush = new SolidBrush(ForeColor))
            {
                if (style == ProgressBarStyle.Marquee)
                    e.Graphics.FillRectangle(brush, position-Width/4, 0, Width/4, Height);
                else e.Graphics.FillRectangle(brush, 0, 0, Width*value/100, Height);
            }
        }
        protected override void Dispose(bool disposing) { if (disposing) animation.Dispose(); base.Dispose(disposing); }
    }
}
'@
}

function Set-IrmInstallerButtonTheme {
    param([Windows.Forms.Button]$Button, [switch]$Primary)
    $Button.UseVisualStyleBackColor = $false
    $Button.FlatStyle = 'Flat'
    $Button.BackColor = if ($Primary) { $script:installerColors.Button } else { $script:installerColors.Surface }
    $Button.ForeColor = if ($Primary) { [Drawing.Color]::White } else { $script:installerColors.Text }
    $Button.FlatAppearance.BorderColor = $script:installerColors.Border
    $Button.FlatAppearance.MouseOverBackColor = if ($Primary) { $script:installerColors.ButtonHover } else { $script:installerColors.Window }
    $Button.FlatAppearance.MouseDownBackColor = if ($Primary) { $script:installerColors.Header } else { $script:installerColors.Border }
}

function Show-IrmInstallerMessage {
    param([string]$Message, [string]$Caption, [ValidateSet('OK','YesNo')][string]$Buttons = 'OK')
    $dialog = New-Object Windows.Forms.Form
    try {
        $dialog.Text = $Caption
        $dialog.StartPosition = 'CenterParent'
        $dialog.FormBorderStyle = 'FixedDialog'
        $dialog.ClientSize = New-Object Drawing.Size(590,260)
        $dialog.MaximizeBox = $false
        $dialog.MinimizeBox = $false
        $dialog.ShowInTaskbar = $false
        $dialog.Font = New-Object Drawing.Font('Segoe UI',10)
        $dialog.BackColor = $script:installerColors.Window
        $dialog.ForeColor = $script:installerColors.Text
        $messageBox = New-Object Windows.Forms.TextBox
        $messageBox.Multiline = $true
        $messageBox.ReadOnly = $true
        $messageBox.BorderStyle = 'None'
        $messageBox.ScrollBars = 'Vertical'
        $messageBox.BackColor = $dialog.BackColor
        $messageBox.ForeColor = $dialog.ForeColor
        $messageBox.Text = $Message
        $messageBox.SetBounds(22,22,546,170)
        $dialog.Controls.Add($messageBox)
        $accept = New-Object IrmInstaller.DarkButton
        $accept.Text = if ($Buttons -eq 'YesNo') { 'Có' } else { 'OK' }
        $accept.DialogResult = if ($Buttons -eq 'YesNo') { 'Yes' } else { 'OK' }
        $accept.SetBounds(340,210,110,34)
        Set-IrmInstallerButtonTheme $accept -Primary
        $dialog.Controls.Add($accept)
        $dialog.AcceptButton = $accept
        if ($Buttons -eq 'YesNo') {
            $cancel = New-Object IrmInstaller.DarkButton
            $cancel.Text = 'Không'; $cancel.DialogResult = 'No'; $cancel.SetBounds(462,210,106,34)
            Set-IrmInstallerButtonTheme $cancel
            $dialog.Controls.Add($cancel)
            $dialog.CancelButton = $cancel
        } else { $dialog.CancelButton = $accept }
        $dialog.Add_HandleCreated({ param($sender,$e) [IrmInstaller.WindowTheme]::ApplyTitleBar($sender) })
        return $dialog.ShowDialog($form)
    } finally { $dialog.Dispose() }
}
