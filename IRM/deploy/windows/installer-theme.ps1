# Local installer styling. Never changes the Windows theme or registry.
$script:installerThemeRoot = $PSScriptRoot

$script:installerColors = @{
    Window = [Drawing.ColorTranslator]::FromHtml('#B5CA6E')
    Surface = [Drawing.ColorTranslator]::FromHtml('#FAFCF5')
    Input = [Drawing.ColorTranslator]::FromHtml('#FFFFFF')
    Header = [Drawing.ColorTranslator]::FromHtml('#3F5223')
    Text = [Drawing.ColorTranslator]::FromHtml('#1E2D10')
    Muted = [Drawing.ColorTranslator]::FromHtml('#4D6136')
    Accent = [Drawing.ColorTranslator]::FromHtml('#16240B')
    Button = [Drawing.ColorTranslator]::FromHtml('#EA580C')
    ButtonHover = [Drawing.ColorTranslator]::FromHtml('#C2410C')
    Border = [Drawing.ColorTranslator]::FromHtml('#C5D6A7')
    Success = [Drawing.ColorTranslator]::FromHtml('#15803D')
    Warning = [Drawing.ColorTranslator]::FromHtml('#8A4B0F')
    Error = [Drawing.ColorTranslator]::FromHtml('#BE123C')
}

if (-not ('IrmInstaller.LandingForm' -as [type])) {
    Add-Type -ReferencedAssemblies System.Windows.Forms,System.Drawing -TypeDefinition @'
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
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

    public class LandingForm : Form
    {
        public Image BrandLogo { get; set; }
        public Image CharacterImage { get; set; }
        public int RightPaneWidth { get; set; }
        public int HeaderHeight { get; set; }

        public LandingForm()
        {
            RightPaneWidth = 250;
            HeaderHeight = 84;
            DoubleBuffered = true;
            ResizeRedraw = true;
        }

        private static RectangleF FitAspect(RectangleF bounds, float imgW, float imgH)
        {
            if (imgW <= 0f || imgH <= 0f || bounds.Width <= 0f || bounds.Height <= 0f)
                return bounds;
            float ratio = Math.Min(bounds.Width / imgW, bounds.Height / imgH);
            float w = imgW * ratio;
            float h = imgH * ratio;
            float x = bounds.X + (bounds.Width - w) / 2f;
            float y = bounds.Y + (bounds.Height - h) / 2f;
            return new RectangleF(x, y, w, h);
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            if (ClientSize.Width <= 0 || ClientSize.Height <= 0)
            {
                base.OnPaintBackground(e);
                return;
            }

            Graphics g = e.Graphics;
            int w = ClientSize.Width;
            int h = ClientSize.Height;
            float dpiScale = g.DpiX > 0 ? g.DpiX / 96f : 1f;

            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;

            using (var bgBrush = new LinearGradientBrush(
                ClientRectangle,
                Color.FromArgb(194, 214, 124),
                Color.FromArgb(109, 132, 70),
                125f))
            {
                var blend = new ColorBlend(3);
                blend.Colors = new[]
                {
                    Color.FromArgb(194, 214, 124),
                    Color.FromArgb(154, 181, 92),
                    Color.FromArgb(109, 132, 70)
                };
                blend.Positions = new[] { 0f, 0.48f, 1f };
                bgBrush.InterpolationColors = blend;
                g.FillRectangle(bgBrush, ClientRectangle);
            }

            using (var darkPoly1 = new SolidBrush(Color.FromArgb(20, 0, 0, 0)))
                g.FillPolygon(darkPoly1, new[] {
                    new PointF(0.48f * w, 0f), new PointF(w, 0f),
                    new PointF(w, h), new PointF(0.56f * w, h)
                });
            using (var darkPoly2 = new SolidBrush(Color.FromArgb(18, 0, 0, 0)))
                g.FillPolygon(darkPoly2, new[] {
                    new PointF(0.65f * w, 0f), new PointF(w, 0.36f * h),
                    new PointF(w, h), new PointF(0.52f * w, h)
                });
            using (var lightPoly1 = new SolidBrush(Color.FromArgb(23, 255, 255, 255)))
                g.FillPolygon(lightPoly1, new[] {
                    new PointF(0.76f * w, 0f), new PointF(w, 0f),
                    new PointF(0.84f * w, 0.68f * h), new PointF(0.59f * w, 0.24f * h)
                });
            using (var lightPoly2 = new SolidBrush(Color.FromArgb(18, 255, 255, 255)))
                g.FillPolygon(lightPoly2, new[] {
                    new PointF(0f, 0.64f * h), new PointF(0.42f * w, h),
                    new PointF(0f, h)
                });

            float headerH = HeaderHeight * dpiScale;
            using (var headerSep = new Pen(Color.FromArgb(40, 22, 36, 11)))
                g.DrawLine(headerSep, 0f, headerH, w, headerH);

            if (BrandLogo != null)
            {
                var logoBox = new RectangleF(24f * dpiScale, 14f * dpiScale, 56f * dpiScale, 56f * dpiScale);
                var drawLogo = FitAspect(logoBox, BrandLogo.Width, BrandLogo.Height);
                g.DrawImage(BrandLogo, drawLogo);
            }

            if (CharacterImage != null)
            {
                var wmBox = FitAspect(
                    new RectangleF(-10f * dpiScale, h - 460f * dpiScale, 260f * dpiScale, 460f * dpiScale),
                    CharacterImage.Width,
                    CharacterImage.Height);
                using (var attrs = new ImageAttributes())
                {
                    var matrix = new ColorMatrix { Matrix33 = 0.08f };
                    attrs.SetColorMatrix(matrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
                    g.DrawImage(
                        CharacterImage,
                        Rectangle.Round(wmBox),
                        0, 0, CharacterImage.Width, CharacterImage.Height,
                        GraphicsUnit.Pixel,
                        attrs);
                }

                float rightW = RightPaneWidth * dpiScale;
                float margin = 16f * dpiScale;
                var rightRect = new RectangleF(
                    w - rightW - margin,
                    headerH + margin,
                    rightW,
                    Math.Max(1f, h - headerH - 36f * dpiScale));

                float maxCharH = Math.Min(430f * dpiScale, Math.Max(1f, rightRect.Height - 24f * dpiScale));
                float maxCharW = Math.Max(1f, rightRect.Width - 8f * dpiScale);
                var charBounds = new RectangleF(
                    rightRect.X + (rightRect.Width - maxCharW) / 2f,
                    rightRect.Y + Math.Max(0f, (rightRect.Height - maxCharH) / 2f - 6f * dpiScale),
                    maxCharW,
                    maxCharH);
                var drawChar = FitAspect(charBounds, CharacterImage.Width, CharacterImage.Height);

                float pedW = Math.Max(130f * dpiScale, drawChar.Width * 1.1f);
                float pedH = 22f * dpiScale;
                float pedX = drawChar.X + (drawChar.Width - pedW) / 2f;
                float pedY = drawChar.Bottom - 10f * dpiScale;
                using (var pedOuter = new SolidBrush(Color.FromArgb(40, 15, 23, 42)))
                    g.FillEllipse(pedOuter, pedX - 8f * dpiScale, pedY - 2f * dpiScale, pedW + 16f * dpiScale, pedH + 6f * dpiScale);
                using (var pedBrush = new SolidBrush(Color.FromArgb(95, 15, 23, 42)))
                    g.FillEllipse(pedBrush, pedX, pedY, pedW, pedH);

                g.DrawImage(CharacterImage, drawChar);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (BrandLogo != null) { BrandLogo.Dispose(); BrandLogo = null; }
                if (CharacterImage != null) { CharacterImage.Dispose(); CharacterImage = null; }
            }
            base.Dispose(disposing);
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
            TextRenderer.DrawText(e.Graphics, Text, Font, new Rectangle(18,0,Math.Max(1,Width-18),Height), Color.FromArgb(100,118,76),
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);
        }
    }

    public class DarkButton : Button
    {
        protected override void OnPaint(PaintEventArgs e)
        {
            if (Enabled) { base.OnPaint(e); return; }
            e.Graphics.Clear(Color.FromArgb(228,237,205));
            using (var pen = new Pen(Color.FromArgb(197,214,167)))
                e.Graphics.DrawRectangle(pen, 0, 0, Width-1, Height-1);
            TextRenderer.DrawText(e.Graphics, Text, Font, ClientRectangle, Color.FromArgb(100,118,76),
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
        }
    }

    public class DarkListView : ListView
    {
        private static readonly Color Header = Color.FromArgb(235,243,213);
        private static readonly Color Alternate = Color.FromArgb(247,250,238);
        private static readonly Color Selection = Color.FromArgb(223,237,190);
        private static readonly Color Line = Color.FromArgb(197,214,167);
        public DarkListView()
        {
            OwnerDraw = true;
            DoubleBuffered = true;
            BackColor = Color.FromArgb(250,252,245);
            ForeColor = Color.FromArgb(30,45,16);
            GridLines = false;
            HideSelection = false;
            ShowItemToolTips = true;
        }
        private void FillDetailColumn()
        {
            if (Columns.Count == 3)
                Columns[2].Width = Math.Max(220, ClientSize.Width-Columns[0].Width-Columns[1].Width);
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
                using (var pen = new Pen(Color.FromArgb(77,97,54))) e.Graphics.DrawRectangle(pen, box);
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
            BackColor = Color.FromArgb(223,237,190);
            ForeColor = Color.FromArgb(234,88,12);
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

function Get-IrmInstallerImage {
    param(
        [Parameter(Mandatory)][string]$FileName,
        [string]$PayloadRoot = ''
    )
    $candidates = @()
    if (-not [string]::IsNullOrWhiteSpace($PayloadRoot)) {
        $candidates += Join-Path $PayloadRoot "app\wwwroot\images\$FileName"
    }
    if (-not [string]::IsNullOrWhiteSpace($script:installerThemeRoot)) {
        $candidates += Join-Path $script:installerThemeRoot "..\..\IRM\wwwroot\images\$FileName"
    }
    foreach ($path in $candidates) {
        if (Test-Path -LiteralPath $path -PathType Leaf) {
            $bytes = [IO.File]::ReadAllBytes($path)
            $stream = New-Object IO.MemoryStream(,$bytes)
            try {
                $tempImage = [Drawing.Image]::FromStream($stream)
                try {
                    return New-Object Drawing.Bitmap($tempImage)
                } finally {
                    $tempImage.Dispose()
                }
            } finally {
                $stream.Dispose()
            }
        }
    }
    return $null
}

function Set-IrmInstallerButtonTheme {
    param([Windows.Forms.Button]$Button, [switch]$Primary)
    $Button.UseVisualStyleBackColor = $false
    $Button.FlatStyle = 'Flat'
    $Button.BackColor = if ($Primary) { $script:installerColors.Button } else { $script:installerColors.Surface }
    $Button.ForeColor = if ($Primary) { [Drawing.Color]::White } else { $script:installerColors.Text }
    $Button.FlatAppearance.BorderColor = if ($Primary) { $script:installerColors.ButtonHover } else { $script:installerColors.Border }
    $Button.FlatAppearance.MouseOverBackColor = if ($Primary) { $script:installerColors.ButtonHover } else { [Drawing.ColorTranslator]::FromHtml('#EEF5DC') }
    $Button.FlatAppearance.MouseDownBackColor = if ($Primary) { [Drawing.ColorTranslator]::FromHtml('#9A3412') } else { $script:installerColors.Border }
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
        $dialog.BackColor = $script:installerColors.Surface
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
