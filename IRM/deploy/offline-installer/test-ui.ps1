Add-Type -AssemblyName System.Windows.Forms
Add-Type -AssemblyName System.Drawing
[System.Windows.Forms.Application]::EnableVisualStyles()

$f = New-Object System.Windows.Forms.Form
$f.Text = "IRM v1.0.2 - Ki" + [char]0x1EC3 + "m tra giao di" + [char]0x1EC7 + "n"
$f.Width = 520
$f.Height = 300
$f.StartPosition = "CenterScreen"
$f.BackColor = [System.Drawing.Color]::FromArgb(18, 28, 40)
$f.TopMost = $true
$f.FormBorderStyle = "FixedSingle"
$f.MaximizeBox = $false

$l = New-Object System.Windows.Forms.Label
$l.Text = "Giao di" + [char]0x1EC7 + "n ho" + [char]0x1EA1 + "t " + [char]0x0111 + [char]0x1ED9 + "ng!"
$l.ForeColor = [System.Drawing.Color]::FromArgb(76, 175, 80)
$l.Font = New-Object System.Drawing.Font("Segoe UI", 18, [System.Drawing.FontStyle]::Bold)
$l.AutoSize = $true
$l.Location = New-Object System.Drawing.Point(60, 60)
$f.Controls.Add($l)

$l2 = New-Object System.Windows.Forms.Label
$l2.Text = "Installer IRM v1.0.2 s" + [char]0x1EB5 + "n s" + [char]0x00E0 + "ng. B" + [char]0x1EA1 + "n c" + [char]0x00F3 + " th" + [char]0x1EC3 + " ch" + [char]0x1EA1 + "y C" + [char]0x00C0 + "I " + [char]0x0110 + [char]0x1EB6 + "T IRM.bat"
$l2.ForeColor = [System.Drawing.Color]::FromArgb(150, 180, 200)
$l2.Font = New-Object System.Drawing.Font("Segoe UI", 10)
$l2.AutoSize = $true
$l2.Location = New-Object System.Drawing.Point(40, 120)
$f.Controls.Add($l2)

$btn = New-Object System.Windows.Forms.Button
$btn.Text = [char]0x0110 + [char]0x00F3 + "ng"
$btn.Width = 140
$btn.Height = 42
$btn.Location = New-Object System.Drawing.Point(190, 190)
$btn.BackColor = [System.Drawing.Color]::FromArgb(21, 101, 192)
$btn.ForeColor = [System.Drawing.Color]::White
$btn.FlatStyle = "Flat"
$btn.Font = New-Object System.Drawing.Font("Segoe UI", 11, [System.Drawing.FontStyle]::Bold)
$btn.Add_Click({ $f.Close() })
$f.Controls.Add($btn)

[void]$f.ShowDialog()
