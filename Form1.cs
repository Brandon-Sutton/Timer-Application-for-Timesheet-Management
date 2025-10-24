using System.Drawing.Drawing2D;
using System.Text;

namespace TimerApplication
{
    public partial class Form1 : Form
    {
        private List<TimerControl> timers = new List<TimerControl>();
        private Panel mainPanel;
        private Panel containerPanel;
        private Button createTimerButton;
        private Button exportButton;
        private Button deleteAllButton;
        private CheckBox darkModeToggle;
        private bool isDarkMode = false;
        private const int MAX_TIMERS = 30;
        private const int TIMER_HEIGHT = 60;
        private const int TIMER_SPACING = 5;
        private const int TIMER_TOTAL_HEIGHT = 65;

        // Color schemes
        private class ColorScheme
        {
            public Color FormBackground { get; set; }
            public Color PanelBackground { get; set; }
            public Color ContainerBackground { get; set; }
            public Color TimerBackground { get; set; }
            public Color TextColor { get; set; }
            public Color TimeColor { get; set; }
            public Color ButtonCreateColor { get; set; }
            public Color ButtonExportColor { get; set; }
            public Color ButtonDeleteAllColor { get; set; }
            public Color ButtonPlayColor { get; set; }
            public Color ButtonStopColor { get; set; }
            public Color ButtonDeleteColor { get; set; }
            public Color BorderColor { get; set; }
        }

        private ColorScheme lightMode = new ColorScheme
        {
            FormBackground = Color.WhiteSmoke,
            PanelBackground = Color.FromArgb(250, 250, 250),
            ContainerBackground = Color.White,
            TimerBackground = Color.White,
            TextColor = Color.Black,
            TimeColor = Color.DarkBlue,
            ButtonCreateColor = Color.LightBlue,
            ButtonExportColor = Color.LightGreen,
            ButtonDeleteAllColor = Color.FromArgb(255, 200, 200),
            ButtonPlayColor = Color.FromArgb(144, 238, 144),
            ButtonStopColor = Color.FromArgb(255, 165, 0),
            ButtonDeleteColor = Color.FromArgb(255, 182, 193),
            BorderColor = SystemColors.ControlDark
        };

        private ColorScheme darkMode = new ColorScheme
        {
            FormBackground = Color.FromArgb(30, 30, 30),
            PanelBackground = Color.FromArgb(40, 40, 40),
            ContainerBackground = Color.FromArgb(45, 45, 45),
            TimerBackground = Color.FromArgb(50, 50, 50),
            TextColor = Color.FromArgb(230, 230, 230),
            TimeColor = Color.FromArgb(100, 180, 255),
            ButtonCreateColor = Color.FromArgb(70, 130, 180),
            ButtonExportColor = Color.FromArgb(60, 120, 60),
            ButtonDeleteAllColor = Color.FromArgb(120, 40, 40),
            ButtonPlayColor = Color.FromArgb(60, 120, 60),
            ButtonStopColor = Color.FromArgb(180, 100, 40),
            ButtonDeleteColor = Color.FromArgb(150, 50, 50),
            BorderColor = Color.FromArgb(60, 60, 60)
        };

        public Form1()
        {
            InitializeComponent();
            SetupUI();
        }

        private void SetupUI()
        {
            // Set DPI awareness for consistent scaling
            this.AutoScaleMode = AutoScaleMode.Dpi;

            // Update form properties - Slimmer design
            this.Text = "Multi-Timer Application";
            this.ClientSize = new Size(480, 650);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = lightMode.FormBackground;
            this.MinimumSize = new Size(496, 400);
            this.FormBorderStyle = FormBorderStyle.Sizable;

            // Create Timer button
            createTimerButton = new Button();
            createTimerButton.Text = "➕ New Timer";
            createTimerButton.Size = new Size(110, 32);
            createTimerButton.Location = new Point(8, 8);
            createTimerButton.BackColor = lightMode.ButtonCreateColor;
            createTimerButton.FlatStyle = FlatStyle.Flat;
            createTimerButton.FlatAppearance.BorderSize = 0;
            createTimerButton.Font = new Font("Segoe UI", 9F, FontStyle.Regular);
            createTimerButton.Click += CreateTimerButton_Click;
            createTimerButton.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            createTimerButton.Cursor = Cursors.Hand;
            this.Controls.Add(createTimerButton);

            // Export button
            exportButton = new Button();
            exportButton.Text = "📊 Export";
            exportButton.Size = new Size(85, 32);
            exportButton.Location = new Point(123, 8);
            exportButton.BackColor = lightMode.ButtonExportColor;
            exportButton.FlatStyle = FlatStyle.Flat;
            exportButton.FlatAppearance.BorderSize = 0;
            exportButton.Font = new Font("Segoe UI", 9F, FontStyle.Regular);
            exportButton.Click += ExportButton_Click;
            exportButton.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            exportButton.Cursor = Cursors.Hand;
            this.Controls.Add(exportButton);

            // Delete All button
            deleteAllButton = new Button();
            deleteAllButton.Text = "Delete All";
            deleteAllButton.Size = new Size(90, 32);
            deleteAllButton.Location = new Point(213, 8);
            deleteAllButton.BackColor = lightMode.ButtonDeleteAllColor;
            deleteAllButton.FlatStyle = FlatStyle.Flat;
            deleteAllButton.FlatAppearance.BorderSize = 0;
            deleteAllButton.Font = new Font("Segoe UI", 9F, FontStyle.Regular);
            deleteAllButton.Click += DeleteAllButton_Click;
            deleteAllButton.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            deleteAllButton.Cursor = Cursors.Hand;
            this.Controls.Add(deleteAllButton);

            // Dark mode toggle
            darkModeToggle = new CheckBox();
            darkModeToggle.Text = "🌙";
            darkModeToggle.Size = new Size(50, 32);
            darkModeToggle.Location = new Point(this.ClientSize.Width - 58, 8);
            darkModeToggle.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            darkModeToggle.Appearance = Appearance.Button;
            darkModeToggle.FlatStyle = FlatStyle.Flat;
            darkModeToggle.FlatAppearance.BorderSize = 0;
            darkModeToggle.Font = new Font("Segoe UI", 12F);
            darkModeToggle.TextAlign = ContentAlignment.MiddleCenter;
            darkModeToggle.Cursor = Cursors.Hand;
            darkModeToggle.BackColor = Color.FromArgb(200, 200, 200);
            darkModeToggle.CheckedChanged += DarkModeToggle_CheckedChanged;
            this.Controls.Add(darkModeToggle);

            // Main panel with custom scrollbar
            mainPanel = new Panel();
            mainPanel.Location = new Point(8, 48);
            mainPanel.Size = new Size(464, 594);
            mainPanel.BorderStyle = BorderStyle.FixedSingle;
            mainPanel.BackColor = lightMode.PanelBackground;
            mainPanel.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            mainPanel.AutoScroll = false;
            this.Controls.Add(mainPanel);

            // Container panel inside main panel
            containerPanel = new Panel();
            containerPanel.Location = new Point(0, 0);
            containerPanel.Size = new Size(mainPanel.ClientSize.Width - 25, 10);
            containerPanel.BackColor = lightMode.ContainerBackground;
            containerPanel.AutoScroll = false;
            mainPanel.Controls.Add(containerPanel);

            // Create custom scrollbar
            VScrollBar scrollBar = new VScrollBar();
            scrollBar.Dock = DockStyle.Right;
            scrollBar.Width = 25;
            scrollBar.Minimum = 0;
            scrollBar.Maximum = 0;
            scrollBar.SmallChange = 20;
            scrollBar.LargeChange = 100;
            scrollBar.Scroll += ScrollBar_Scroll;
            mainPanel.Controls.Add(scrollBar);

            // Store scrollbar reference
            mainPanel.Tag = scrollBar;

            // Handle mouse wheel
            mainPanel.MouseWheel += MainPanel_MouseWheel;
            containerPanel.MouseWheel += MainPanel_MouseWheel;

            // Handle form resize
            this.Resize += Form1_Resize;

            UpdateDeleteAllButtonState();
        }

        private void DarkModeToggle_CheckedChanged(object sender, EventArgs e)
        {
            isDarkMode = darkModeToggle.Checked;
            darkModeToggle.Text = isDarkMode ? "☀" : "🌙";
            darkModeToggle.BackColor = isDarkMode ? Color.FromArgb(60, 60, 60) : Color.FromArgb(200, 200, 200);
            ApplyColorScheme();
        }

        private void ApplyColorScheme()
        {
            ColorScheme scheme = isDarkMode ? darkMode : lightMode;

            this.BackColor = scheme.FormBackground;
            mainPanel.BackColor = scheme.PanelBackground;
            containerPanel.BackColor = scheme.ContainerBackground;

            createTimerButton.BackColor = scheme.ButtonCreateColor;
            createTimerButton.ForeColor = scheme.TextColor;

            exportButton.BackColor = scheme.ButtonExportColor;
            exportButton.ForeColor = scheme.TextColor;

            deleteAllButton.BackColor = scheme.ButtonDeleteAllColor;
            deleteAllButton.ForeColor = scheme.TextColor;

            // Update all existing timers
            foreach (TimerControl timer in timers)
            {
                timer.ApplyColorScheme(isDarkMode);
            }

            // Refresh the form
            this.Refresh();
        }

        private void DeleteAllButton_Click(object sender, EventArgs e)
        {
            if (timers.Count == 0) return;

            DialogResult result = MessageBox.Show(
                $"Are you sure you want to delete all {timers.Count} timers?",
                "Delete All Timers",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                // Stop and dispose all timers
                foreach (var timer in timers.ToList())
                {
                    containerPanel.Controls.Remove(timer);
                    timer.Dispose();
                }

                timers.Clear();
                UpdateScrollBar();
                updateCreateButtonState();
                UpdateDeleteAllButtonState();
            }
        }

        private void UpdateDeleteAllButtonState()
        {
            deleteAllButton.Enabled = timers.Count > 0;
            deleteAllButton.Text = "🗑 Delete All";
        }

        private void ScrollBar_Scroll(object sender, ScrollEventArgs e)
        {
            containerPanel.Top = -e.NewValue;
        }

        private void MainPanel_MouseWheel(object sender, MouseEventArgs e)
        {
            VScrollBar scrollBar = mainPanel.Tag as VScrollBar;
            if (scrollBar != null && scrollBar.Visible)
            {
                int newValue = scrollBar.Value - (e.Delta / 3);
                newValue = Math.Max(scrollBar.Minimum, Math.Min(newValue, scrollBar.Maximum - scrollBar.LargeChange + 1));
                scrollBar.Value = newValue;
                containerPanel.Top = -newValue;
            }
        }

        private void UpdateScrollBar()
        {
            VScrollBar scrollBar = mainPanel.Tag as VScrollBar;
            if (scrollBar != null)
            {
                int totalHeight = timers.Count * TIMER_TOTAL_HEIGHT + 10;
                int visibleHeight = mainPanel.ClientSize.Height;

                if (totalHeight > visibleHeight)
                {
                    scrollBar.Visible = true;
                    scrollBar.Maximum = totalHeight - visibleHeight + scrollBar.LargeChange - 1;
                    scrollBar.LargeChange = visibleHeight;
                    containerPanel.Width = mainPanel.ClientSize.Width - scrollBar.Width - 5;
                }
                else
                {
                    scrollBar.Visible = false;
                    scrollBar.Value = 0;
                    containerPanel.Top = 0;
                    containerPanel.Width = mainPanel.ClientSize.Width - 5;
                }

                containerPanel.Height = Math.Max(totalHeight, visibleHeight);
            }

            foreach (TimerControl timer in timers)
            {
                timer.Width = containerPanel.Width - 10;
            }
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            if (mainPanel != null && containerPanel != null)
            {
                UpdateScrollBar();
            }
        }

        private void CreateTimerButton_Click(object sender, EventArgs e)
        {
            if (timers.Count >= MAX_TIMERS)
            {
                MessageBox.Show($"Maximum of {MAX_TIMERS} timers allowed.", "Limit Reached", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Point buttonScreenLocation = createTimerButton.PointToScreen(Point.Empty);

            TimerCreationDialog dialog = new TimerCreationDialog(isDarkMode);
            dialog.StartPosition = FormStartPosition.Manual;
            dialog.Location = new Point(buttonScreenLocation.X, buttonScreenLocation.Y + createTimerButton.Height + 5);

            Rectangle screenBounds = Screen.FromControl(this).WorkingArea;
            if (dialog.Right > screenBounds.Right)
                dialog.Left = screenBounds.Right - dialog.Width;
            if (dialog.Bottom > screenBounds.Bottom)
                dialog.Top = screenBounds.Bottom - dialog.Height;

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                int yPosition = timers.Count * TIMER_TOTAL_HEIGHT + 5;

                TimerControl newTimer = new TimerControl(dialog.TimerName, dialog.TimerNarrative, isDarkMode);
                newTimer.Location = new Point(5, yPosition);
                newTimer.Size = new Size(containerPanel.Width - 10, TIMER_HEIGHT);
                newTimer.TimerDeleted += NewTimer_TimerDeleted;

                timers.Add(newTimer);
                containerPanel.Controls.Add(newTimer);

                UpdateScrollBar();
                updateCreateButtonState();
                UpdateDeleteAllButtonState();
                ScrollToTimer(newTimer);
            }
        }

        private void ScrollToTimer(TimerControl timer)
        {
            VScrollBar scrollBar = mainPanel.Tag as VScrollBar;
            if (scrollBar != null && scrollBar.Visible)
            {
                int timerBottom = timer.Bottom;
                int visibleHeight = mainPanel.ClientSize.Height;

                if (timerBottom > visibleHeight)
                {
                    int newScrollValue = Math.Min(timerBottom - visibleHeight + 10, scrollBar.Maximum - scrollBar.LargeChange + 1);
                    scrollBar.Value = newScrollValue;
                    containerPanel.Top = -newScrollValue;
                }
            }
        }

        private void NewTimer_TimerDeleted(object sender, EventArgs e)
        {
            TimerControl timerToRemove = sender as TimerControl;
            if (timerToRemove != null)
            {
                int removedIndex = timers.IndexOf(timerToRemove);

                timers.Remove(timerToRemove);
                containerPanel.Controls.Remove(timerToRemove);
                timerToRemove.Dispose();

                for (int i = removedIndex; i < timers.Count; i++)
                {
                    int yPosition = i * TIMER_TOTAL_HEIGHT + 5;
                    timers[i].Location = new Point(5, yPosition);
                }

                UpdateScrollBar();
                updateCreateButtonState();
                UpdateDeleteAllButtonState();
            }
        }

        private void updateCreateButtonState()
        {
            createTimerButton.Enabled = timers.Count < MAX_TIMERS;
            if (timers.Count >= MAX_TIMERS)
            {
                createTimerButton.Text = $"Max ({MAX_TIMERS})";
            }
            else
            {
                createTimerButton.Text = $"➕ New ({timers.Count}/{MAX_TIMERS})";
            }
        }

        private void ExportButton_Click(object sender, EventArgs e)
        {
            if (timers.Count == 0)
            {
                MessageBox.Show("No timers to export.", "Export", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            SaveFileDialog saveDialog = new SaveFileDialog();
            saveDialog.Filter = "CSV files (*.csv)|*.csv";
            saveDialog.DefaultExt = "csv";
            saveDialog.FileName = "timers_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".csv";

            if (saveDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    StringBuilder csv = new StringBuilder();
                    csv.AppendLine("Name,Narrative,Time");

                    foreach (TimerControl timer in timers)
                    {
                        double totalMinutes = timer.GetElapsedTimeSpan().TotalMinutes;
                        double decimalHours = Math.Ceiling(totalMinutes / 6.0) * 0.1;
                        csv.AppendLine("\"" + timer.TimerName + "\",\"" + timer.TimerNarrative + "\",\"" + decimalHours.ToString("0.0") + "\"");
                    }

                    File.WriteAllText(saveDialog.FileName, csv.ToString());
                    MessageBox.Show("Timers exported successfully!", "Export Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error exporting timers:\n" + ex.Message, "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }

    // Custom Icon Button class for play/pause and delete buttons
    public class IconButton : Button
    {
        public enum ButtonIcon { Play, Stop, Delete }
        private ButtonIcon icon;
        private bool isDarkMode;

        public ButtonIcon Icon
        {
            get { return icon; }
            set { icon = value; Invalidate(); }
        }

        public bool IsDarkMode
        {
            get { return isDarkMode; }
            set { isDarkMode = value; Invalidate(); }
        }

        public IconButton()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.DoubleBuffer, true);
            this.FlatStyle = FlatStyle.Flat;
            this.FlatAppearance.BorderSize = 0;
            this.Size = new Size(32, 32);
            this.Cursor = Cursors.Hand;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // Draw background
            using (SolidBrush bgBrush = new SolidBrush(this.BackColor))
            {
                g.FillRectangle(bgBrush, this.ClientRectangle);
            }

            // Draw icon
            Color iconColor = isDarkMode ? Color.FromArgb(230, 230, 230) : Color.Black;
            using (Pen pen = new Pen(iconColor, 2))
            using (SolidBrush brush = new SolidBrush(iconColor))
            {
                int padding = 8;
                Rectangle iconRect = new Rectangle(padding, padding, Width - 2 * padding, Height - 2 * padding);

                switch (icon)
                {
                    case ButtonIcon.Play:
                        // Draw play triangle
                        Point[] playPoints = new Point[] {
                            new Point(iconRect.Left + 2, iconRect.Top),
                            new Point(iconRect.Left + 2, iconRect.Bottom),
                            new Point(iconRect.Right - 2, iconRect.Top + iconRect.Height / 2)
                        };
                        g.FillPolygon(brush, playPoints);
                        break;

                    case ButtonIcon.Stop:
                        // Draw stop square
                        g.FillRectangle(brush, iconRect);
                        break;

                    case ButtonIcon.Delete:
                        // Draw X
                        g.DrawLine(pen, iconRect.Left, iconRect.Top, iconRect.Right, iconRect.Bottom);
                        g.DrawLine(pen, iconRect.Right, iconRect.Top, iconRect.Left, iconRect.Bottom);
                        break;
                }
            }
        }
    }

    public class TimerControl : UserControl
    {
        private System.Windows.Forms.Timer timer;
        private TimeSpan elapsedTime;
        private bool isRunning;
        private bool isDarkMode;

        private Label infoLabel;
        private Label timeLabel;
        private IconButton playStopButton;
        private IconButton deleteButton;
        private ToolTip toolTip;

        public string TimerName { get; private set; }
        public string TimerNarrative { get; private set; }
        public string ElapsedTimeDisplay
        {
            get { return timeLabel != null ? timeLabel.Text : "00:00:00"; }
        }

        public event EventHandler TimerDeleted;

        public TimerControl(string name, string narrative, bool darkMode = false)
        {
            TimerName = name;
            TimerNarrative = narrative;
            isDarkMode = darkMode;
            InitializeTimer();
            SetupTimerUI();
            StartTimer();
        }

        private void InitializeTimer()
        {
            timer = new System.Windows.Forms.Timer();
            timer.Interval = 1000;
            timer.Tick += Timer_Tick;
            elapsedTime = TimeSpan.Zero;
            isRunning = false;
        }

        private void SetupTimerUI()
        {
            this.Size = new Size(420, 60);
            this.BorderStyle = BorderStyle.FixedSingle;
            this.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

            // Create tooltip
            toolTip = new ToolTip();
            toolTip.AutomaticDelay = 500;
            toolTip.ReshowDelay = 100;

            // Info label
            infoLabel = new Label();
            UpdateInfoLabel();
            infoLabel.Location = new Point(8, 6);
            infoLabel.Size = new Size(300, 20);
            infoLabel.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            infoLabel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            infoLabel.AutoEllipsis = true;

            string fullText = $"Name: {TimerName} Desc: {TimerNarrative}";
            toolTip.SetToolTip(infoLabel, fullText);

            this.Controls.Add(infoLabel);

            // Time label
            timeLabel = new Label();
            timeLabel.Text = "00:00:00";
            timeLabel.Location = new Point(8, 30);
            timeLabel.Size = new Size(130, 22);
            timeLabel.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            this.Controls.Add(timeLabel);

            // Play/Stop button
            playStopButton = new IconButton();
            playStopButton.Icon = IconButton.ButtonIcon.Stop;
            playStopButton.Size = new Size(32, 26);
            playStopButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            playStopButton.Click += PlayStopButton_Click;
            this.Controls.Add(playStopButton);

            // Delete button
            deleteButton = new IconButton();
            deleteButton.Icon = IconButton.ButtonIcon.Delete;
            deleteButton.Size = new Size(32, 26);
            deleteButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            deleteButton.Click += DeleteButton_Click;
            this.Controls.Add(deleteButton);

            ApplyColorScheme(isDarkMode);
            PositionButtons();

            this.Resize += TimerControl_Resize;
        }

        public void ApplyColorScheme(bool darkMode)
        {
            isDarkMode = darkMode;

            if (isDarkMode)
            {
                this.BackColor = Color.FromArgb(50, 50, 50);
                infoLabel.ForeColor = Color.FromArgb(230, 230, 230);
                timeLabel.ForeColor = Color.FromArgb(100, 180, 255);
                playStopButton.BackColor = isRunning ? Color.FromArgb(180, 100, 40) : Color.FromArgb(60, 120, 60);
                deleteButton.BackColor = Color.FromArgb(150, 50, 50);
            }
            else
            {
                this.BackColor = Color.White;
                infoLabel.ForeColor = Color.Black;
                timeLabel.ForeColor = Color.DarkBlue;
                playStopButton.BackColor = isRunning ? Color.FromArgb(255, 165, 0) : Color.FromArgb(144, 238, 144);
                deleteButton.BackColor = Color.FromArgb(255, 182, 193);
            }

            playStopButton.IsDarkMode = darkMode;
            deleteButton.IsDarkMode = darkMode;
        }

        private void UpdateInfoLabel()
        {
            if (infoLabel != null)
            {
                infoLabel.Text = $"{TimerName} | {TimerNarrative}";
            }
        }

        private void PositionButtons()
        {
            if (playStopButton != null && deleteButton != null)
            {
                playStopButton.Location = new Point(this.Width - 72, 28);
                deleteButton.Location = new Point(this.Width - 38, 28);
            }
        }

        private void TimerControl_Resize(object sender, EventArgs e)
        {
            if (infoLabel != null)
            {
                infoLabel.Width = this.Width - 85;
            }
            PositionButtons();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            elapsedTime = elapsedTime.Add(TimeSpan.FromSeconds(1));
            UpdateTimeDisplay();
        }

        private void UpdateTimeDisplay()
        {
            if (timeLabel != null)
            {
                timeLabel.Text = elapsedTime.ToString(@"hh\:mm\:ss");
            }
        }

        private void PlayStopButton_Click(object sender, EventArgs e)
        {
            if (isRunning)
            {
                StopTimer();
            }
            else
            {
                StartTimer();
            }
        }

        private void StartTimer()
        {
            timer.Start();
            isRunning = true;
            playStopButton.Icon = IconButton.ButtonIcon.Stop;
            playStopButton.BackColor = isDarkMode ? Color.FromArgb(180, 100, 40) : Color.FromArgb(255, 165, 0);
        }

        private void StopTimer()
        {
            timer.Stop();
            isRunning = false;
            playStopButton.Icon = IconButton.ButtonIcon.Play;
            playStopButton.BackColor = isDarkMode ? Color.FromArgb(60, 120, 60) : Color.FromArgb(144, 238, 144);
        }

        private void DeleteButton_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show($"Delete timer '{TimerName}'?",
                "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                timer.Stop();
                timer.Dispose();
                TimerDeleted?.Invoke(this, EventArgs.Empty);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (timer != null)
                {
                    timer.Stop();
                    timer.Dispose();
                }
                if (toolTip != null)
                {
                    toolTip.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        public TimeSpan GetElapsedTimeSpan()
        {
            return elapsedTime;
        }
    }

    public class TimerCreationDialog : Form
    {
        private System.ComponentModel.IContainer components = null;
        private TextBox nameTextBox;
        private TextBox narrativeTextBox;
        private Button okButton;
        private Button cancelButton;
        private bool isDarkMode;

        public string TimerName
        {
            get { return nameTextBox != null ? nameTextBox.Text.Trim() : ""; }
        }

        public string TimerNarrative
        {
            get { return narrativeTextBox != null ? narrativeTextBox.Text.Trim() : ""; }
        }

        public TimerCreationDialog(bool darkMode = false)
        {
            isDarkMode = darkMode;
            InitializeComponent();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.SuspendLayout();

            this.AutoScaleMode = AutoScaleMode.Dpi;

            this.Text = "Create New Timer";
            this.ClientSize = new Size(340, 200);
            this.StartPosition = FormStartPosition.Manual;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // Apply dark mode colors if enabled
            if (isDarkMode)
            {
                this.BackColor = Color.FromArgb(45, 45, 45);
                this.ForeColor = Color.FromArgb(230, 230, 230);
            }

            SetupDialogUI();

            this.ResumeLayout(false);
        }

        private void SetupDialogUI()
        {
            Color labelColor = isDarkMode ? Color.FromArgb(230, 230, 230) : Color.Black;
            Color textBoxBack = isDarkMode ? Color.FromArgb(60, 60, 60) : Color.White;
            Color textBoxFore = isDarkMode ? Color.FromArgb(230, 230, 230) : Color.Black;

            // Name label
            Label nameLabel = new Label();
            nameLabel.Text = "Timer Name:";
            nameLabel.Location = new Point(12, 12);
            nameLabel.Size = new Size(100, 18);
            nameLabel.Font = new Font("Segoe UI", 9F);
            nameLabel.ForeColor = labelColor;
            this.Controls.Add(nameLabel);

            // Name textbox
            nameTextBox = new TextBox();
            nameTextBox.Location = new Point(12, 33);
            nameTextBox.Size = new Size(316, 23);
            nameTextBox.MaxLength = 50;
            nameTextBox.Font = new Font("Segoe UI", 9F);
            nameTextBox.BackColor = textBoxBack;
            nameTextBox.ForeColor = textBoxFore;
            nameTextBox.BorderStyle = BorderStyle.FixedSingle;
            this.Controls.Add(nameTextBox);

            // Narrative label
            Label narrativeLabel = new Label();
            narrativeLabel.Text = "Description:";
            narrativeLabel.Location = new Point(12, 65);
            narrativeLabel.Size = new Size(100, 18);
            narrativeLabel.Font = new Font("Segoe UI", 9F);
            narrativeLabel.ForeColor = labelColor;
            this.Controls.Add(narrativeLabel);

            // Narrative textbox
            narrativeTextBox = new TextBox();
            narrativeTextBox.Location = new Point(12, 86);
            narrativeTextBox.Size = new Size(316, 50);
            narrativeTextBox.Multiline = true;
            narrativeTextBox.MaxLength = 200;
            narrativeTextBox.ScrollBars = ScrollBars.Vertical;
            narrativeTextBox.Font = new Font("Segoe UI", 9F);
            narrativeTextBox.BackColor = textBoxBack;
            narrativeTextBox.ForeColor = textBoxFore;
            narrativeTextBox.BorderStyle = BorderStyle.FixedSingle;
            this.Controls.Add(narrativeTextBox);

            // OK button
            okButton = new Button();
            okButton.Text = "Create";
            okButton.Size = new Size(90, 28);
            okButton.Location = new Point(148, 155);
            okButton.BackColor = isDarkMode ? Color.FromArgb(70, 130, 180) : Color.LightBlue;
            okButton.ForeColor = isDarkMode ? Color.FromArgb(230, 230, 230) : Color.Black;
            okButton.FlatStyle = FlatStyle.Flat;
            okButton.FlatAppearance.BorderSize = 0;
            okButton.Font = new Font("Segoe UI", 9F);
            okButton.Cursor = Cursors.Hand;
            okButton.Click += OkButton_Click;
            this.Controls.Add(okButton);

            // Cancel button
            cancelButton = new Button();
            cancelButton.Text = "Cancel";
            cancelButton.Size = new Size(90, 28);
            cancelButton.Location = new Point(244, 155);
            cancelButton.BackColor = isDarkMode ? Color.FromArgb(80, 80, 80) : Color.LightGray;
            cancelButton.ForeColor = isDarkMode ? Color.FromArgb(230, 230, 230) : Color.Black;
            cancelButton.FlatStyle = FlatStyle.Flat;
            cancelButton.FlatAppearance.BorderSize = 0;
            cancelButton.Font = new Font("Segoe UI", 9F);
            cancelButton.Cursor = Cursors.Hand;
            cancelButton.DialogResult = DialogResult.Cancel;
            this.Controls.Add(cancelButton);

            this.AcceptButton = okButton;
            this.CancelButton = cancelButton;

            this.Shown += (s, e) => nameTextBox.Focus();
        }

        private void OkButton_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(nameTextBox.Text))
            {
                MessageBox.Show("Please enter a timer name.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                nameTextBox.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(narrativeTextBox.Text))
            {
                MessageBox.Show("Please enter a description.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                narrativeTextBox.Focus();
                return;
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}