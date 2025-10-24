using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace TimerApplication
{
    public partial class Form1 : Form
    {
        private List<TimerControl> timers = new List<TimerControl>();
        private Panel mainPanel;
        private Panel containerPanel; // Inner container for timers
        private Button createTimerButton;
        private Button exportButton;
        private const int MAX_TIMERS = 30;
        private const int TIMER_HEIGHT = 85;
        private const int TIMER_SPACING = 5; // Space between timers
        private const int TIMER_TOTAL_HEIGHT = 90; // Height + spacing

        public Form1()
        {
            InitializeComponent();
            SetupUI();
        }

        private void SetupUI()
        {
            // Set DPI awareness for consistent scaling
            this.AutoScaleMode = AutoScaleMode.Dpi;

            // Update form properties
            this.Text = "Multi-Timer Application";
            this.ClientSize = new Size(600, 650);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.WhiteSmoke;
            this.MinimumSize = new Size(616, 400);
            this.FormBorderStyle = FormBorderStyle.Sizable;

            // Create Timer button
            createTimerButton = new Button();
            createTimerButton.Text = "Create New Timer";
            createTimerButton.Size = new Size(150, 35);
            createTimerButton.Location = new Point(10, 10);
            createTimerButton.BackColor = Color.LightBlue;
            createTimerButton.FlatStyle = FlatStyle.Flat;
            createTimerButton.Font = new Font("Segoe UI", 9F, FontStyle.Regular);
            createTimerButton.Click += CreateTimerButton_Click;
            createTimerButton.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            this.Controls.Add(createTimerButton);

            // Export button
            exportButton = new Button();
            exportButton.Text = "Export to CSV";
            exportButton.Size = new Size(150, 35);
            exportButton.Location = new Point(170, 10);
            exportButton.BackColor = Color.LightGreen;
            exportButton.FlatStyle = FlatStyle.Flat;
            exportButton.Font = new Font("Segoe UI", 9F, FontStyle.Regular);
            exportButton.Click += ExportButton_Click;
            exportButton.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            this.Controls.Add(exportButton);

            // Main panel with custom scrollbar
            mainPanel = new Panel();
            mainPanel.Location = new Point(10, 55);
            mainPanel.Size = new Size(580, 585);
            mainPanel.BorderStyle = BorderStyle.FixedSingle;
            mainPanel.BackColor = Color.FromArgb(250, 250, 250);
            mainPanel.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            mainPanel.AutoScroll = false; // We'll handle scrolling manually
            this.Controls.Add(mainPanel);

            // Container panel inside main panel (this will hold the timers)
            containerPanel = new Panel();
            containerPanel.Location = new Point(0, 0);
            containerPanel.Size = new Size(mainPanel.ClientSize.Width - 25, 10); // Reserve space for scrollbar
            containerPanel.BackColor = Color.White;
            containerPanel.AutoScroll = false;
            mainPanel.Controls.Add(containerPanel);

            // Create custom scrollbar
            VScrollBar scrollBar = new VScrollBar();
            scrollBar.Dock = DockStyle.Right;
            scrollBar.Width = 25; // Wider scrollbar
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

                    // Adjust container width when scrollbar is visible
                    containerPanel.Width = mainPanel.ClientSize.Width - scrollBar.Width - 5;
                }
                else
                {
                    scrollBar.Visible = false;
                    scrollBar.Value = 0;
                    containerPanel.Top = 0;

                    // Full width when no scrollbar
                    containerPanel.Width = mainPanel.ClientSize.Width - 5;
                }

                // Update container height
                containerPanel.Height = Math.Max(totalHeight, visibleHeight);
            }

            // Update all timer widths
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

            TimerCreationDialog dialog = new TimerCreationDialog();
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                // Calculate exact position for new timer
                int yPosition = timers.Count * TIMER_TOTAL_HEIGHT + 5;

                TimerControl newTimer = new TimerControl(dialog.TimerName, dialog.TimerNarrative);
                newTimer.Location = new Point(5, yPosition);
                newTimer.Size = new Size(containerPanel.Width - 10, TIMER_HEIGHT);
                newTimer.TimerDeleted += NewTimer_TimerDeleted;

                timers.Add(newTimer);
                containerPanel.Controls.Add(newTimer);

                UpdateScrollBar();
                updateCreateButtonState();

                // Scroll to show the new timer
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

                // Reposition all timers after the removed one
                for (int i = removedIndex; i < timers.Count; i++)
                {
                    int yPosition = i * TIMER_TOTAL_HEIGHT + 5;
                    timers[i].Location = new Point(5, yPosition);
                }

                UpdateScrollBar();
                updateCreateButtonState();
            }
        }

        private void updateCreateButtonState()
        {
            createTimerButton.Enabled = timers.Count < MAX_TIMERS;
            if (timers.Count >= MAX_TIMERS)
            {
                createTimerButton.Text = $"Max Timers ({MAX_TIMERS})";
            }
            else
            {
                createTimerButton.Text = $"Create Timer ({timers.Count}/{MAX_TIMERS})";
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
                    MessageBox.Show("Timers exported successfully to:\n" + saveDialog.FileName, "Export Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error exporting timers:\n" + ex.Message, "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }

    public class TimerControl : UserControl
    {
        private System.Windows.Forms.Timer timer;
        private TimeSpan elapsedTime;
        private bool isRunning;

        private Label nameLabel;
        private Label narrativeLabel;
        private Label timeLabel;
        private Button startStopButton;
        private Button deleteButton;

        public string TimerName { get; private set; }
        public string TimerNarrative { get; private set; }
        public string ElapsedTimeDisplay
        {
            get { return timeLabel != null ? timeLabel.Text : "00:00:00"; }
        }

        public event EventHandler TimerDeleted;

        public TimerControl(string name, string narrative)
        {
            TimerName = name;
            TimerNarrative = narrative;
            InitializeTimer();
            SetupTimerUI();
            StartTimer();
        }

        private void InitializeTimer()
        {
            timer = new System.Windows.Forms.Timer();
            timer.Interval = 1000; // 1 second
            timer.Tick += Timer_Tick;
            elapsedTime = TimeSpan.Zero;
            isRunning = false;
        }

        private void SetupTimerUI()
        {
            this.Size = new Size(520, 85);
            this.BorderStyle = BorderStyle.FixedSingle;
            this.BackColor = Color.White;

            // Disable anchor for fixed positioning
            this.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

            // Name label
            nameLabel = new Label();
            nameLabel.Text = "Name: " + TimerName;
            nameLabel.Location = new Point(8, 8);
            nameLabel.Size = new Size(350, 18);
            nameLabel.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            nameLabel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            nameLabel.AutoEllipsis = true;
            this.Controls.Add(nameLabel);

            // Narrative label
            narrativeLabel = new Label();
            narrativeLabel.Text = "Description: " + TimerNarrative;
            narrativeLabel.Location = new Point(8, 28);
            narrativeLabel.Size = new Size(350, 18);
            narrativeLabel.Font = new Font("Segoe UI", 8.5F);
            narrativeLabel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            narrativeLabel.AutoEllipsis = true;
            this.Controls.Add(narrativeLabel);

            // Time label
            timeLabel = new Label();
            timeLabel.Text = "00:00:00";
            timeLabel.Location = new Point(8, 50);
            timeLabel.Size = new Size(150, 25);
            timeLabel.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            timeLabel.ForeColor = Color.DarkBlue;
            this.Controls.Add(timeLabel);

            // Start/Stop button
            startStopButton = new Button();
            startStopButton.Text = "Stop";
            startStopButton.Size = new Size(75, 28);
            startStopButton.BackColor = Color.Orange;
            startStopButton.FlatStyle = FlatStyle.Flat;
            startStopButton.Font = new Font("Segoe UI", 9F);
            startStopButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            startStopButton.Click += StartStopButton_Click;
            this.Controls.Add(startStopButton);

            // Delete button
            deleteButton = new Button();
            deleteButton.Text = "Delete";
            deleteButton.Size = new Size(75, 28);
            deleteButton.BackColor = Color.LightCoral;
            deleteButton.FlatStyle = FlatStyle.Flat;
            deleteButton.Font = new Font("Segoe UI", 9F);
            deleteButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            deleteButton.Click += DeleteButton_Click;
            this.Controls.Add(deleteButton);

            // Position buttons based on control width
            PositionButtons();

            // Handle resize to adjust components
            this.Resize += TimerControl_Resize;
        }

        private void PositionButtons()
        {
            if (startStopButton != null && deleteButton != null)
            {
                startStopButton.Location = new Point(this.Width - 165, 50);
                deleteButton.Location = new Point(this.Width - 85, 50);
            }
        }

        private void TimerControl_Resize(object sender, EventArgs e)
        {
            if (nameLabel != null && narrativeLabel != null)
            {
                int labelWidth = this.Width - 180;
                nameLabel.Width = labelWidth;
                narrativeLabel.Width = labelWidth;
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

        private void StartStopButton_Click(object sender, EventArgs e)
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
            startStopButton.Text = "Stop";
            startStopButton.BackColor = Color.Orange;
        }

        private void StopTimer()
        {
            timer.Stop();
            isRunning = false;
            startStopButton.Text = "Start";
            startStopButton.BackColor = Color.LightGreen;
        }

        private void DeleteButton_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Are you sure you want to delete the timer '" + TimerName + "'?",
                "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                timer.Stop();
                timer.Dispose();
                if (TimerDeleted != null)
                {
                    TimerDeleted(this, EventArgs.Empty);
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && timer != null)
            {
                timer.Stop();
                timer.Dispose();
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

        public string TimerName
        {
            get { return nameTextBox != null ? nameTextBox.Text.Trim() : ""; }
        }

        public string TimerNarrative
        {
            get { return narrativeTextBox != null ? narrativeTextBox.Text.Trim() : ""; }
        }

        public TimerCreationDialog()
        {
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

            // Set DPI awareness
            this.AutoScaleMode = AutoScaleMode.Dpi;

            this.Text = "Create New Timer";
            this.ClientSize = new Size(380, 210);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            SetupDialogUI();

            this.ResumeLayout(false);
        }

        private void SetupDialogUI()
        {
            // Name label
            Label nameLabel = new Label();
            nameLabel.Text = "Timer Name:";
            nameLabel.Location = new Point(15, 15);
            nameLabel.Size = new Size(100, 20);
            nameLabel.Font = new Font("Segoe UI", 9F);
            this.Controls.Add(nameLabel);

            // Name textbox
            nameTextBox = new TextBox();
            nameTextBox.Location = new Point(15, 38);
            nameTextBox.Size = new Size(350, 25);
            nameTextBox.MaxLength = 50;
            nameTextBox.Font = new Font("Segoe UI", 9F);
            this.Controls.Add(nameTextBox);

            // Narrative label
            Label narrativeLabel = new Label();
            narrativeLabel.Text = "Description:";
            narrativeLabel.Location = new Point(15, 70);
            narrativeLabel.Size = new Size(100, 20);
            narrativeLabel.Font = new Font("Segoe UI", 9F);
            this.Controls.Add(narrativeLabel);

            // Narrative textbox
            narrativeTextBox = new TextBox();
            narrativeTextBox.Location = new Point(15, 93);
            narrativeTextBox.Size = new Size(350, 55);
            narrativeTextBox.Multiline = true;
            narrativeTextBox.MaxLength = 200;
            narrativeTextBox.ScrollBars = ScrollBars.Vertical;
            narrativeTextBox.Font = new Font("Segoe UI", 9F);
            this.Controls.Add(narrativeTextBox);

            // OK button
            okButton = new Button();
            okButton.Text = "Create Timer";
            okButton.Size = new Size(100, 30);
            okButton.Location = new Point(165, 165);
            okButton.BackColor = Color.LightBlue;
            okButton.FlatStyle = FlatStyle.Flat;
            okButton.Font = new Font("Segoe UI", 9F);
            okButton.Click += OkButton_Click;
            this.Controls.Add(okButton);

            // Cancel button
            cancelButton = new Button();
            cancelButton.Text = "Cancel";
            cancelButton.Size = new Size(100, 30);
            cancelButton.Location = new Point(270, 165);
            cancelButton.BackColor = Color.LightGray;
            cancelButton.FlatStyle = FlatStyle.Flat;
            cancelButton.Font = new Font("Segoe UI", 9F);
            cancelButton.DialogResult = DialogResult.Cancel;
            this.Controls.Add(cancelButton);

            this.AcceptButton = okButton;
            this.CancelButton = cancelButton;
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