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

namespace TimerApplication
{
    public partial class Form1 : Form
    {
        private List<TimerControl> timers = new List<TimerControl>();
        private Panel mainPanel;
        private Button createTimerButton;
        private Button exportButton;

        public Form1()
        {
            InitializeComponent();
            SetupUI();
        }

        private void SetupUI()
        {
            // Update form properties
            this.Text = "Multi-Timer Application";
            this.Size = new Size(600, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.WhiteSmoke;

            // Create Timer button
            createTimerButton = new Button();
            createTimerButton.Text = "Create New Timer";
            createTimerButton.Size = new Size(150, 40);
            createTimerButton.Location = new Point(20, 20);
            createTimerButton.BackColor = Color.LightBlue;
            createTimerButton.FlatStyle = FlatStyle.Flat;
            createTimerButton.Click += CreateTimerButton_Click;
            this.Controls.Add(createTimerButton);

            // Export button
            exportButton = new Button();
            exportButton.Text = "Export to CSV";
            exportButton.Size = new Size(150, 40);
            exportButton.Location = new Point(190, 20);
            exportButton.BackColor = Color.LightGreen;
            exportButton.FlatStyle = FlatStyle.Flat;
            exportButton.Click += ExportButton_Click;
            this.Controls.Add(exportButton);

            // Main panel for timers
            mainPanel = new Panel();
            mainPanel.Location = new Point(20, 80);
            mainPanel.Size = new Size(550, 580);
            mainPanel.AutoScroll = true;
            mainPanel.BorderStyle = BorderStyle.FixedSingle;
            this.Controls.Add(mainPanel);
        }

        private void CreateTimerButton_Click(object sender, EventArgs e)
        {
            if (timers.Count >= 10)
            {
                MessageBox.Show("Maximum of 10 timers allowed.", "Limit Reached", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            TimerCreationDialog dialog = new TimerCreationDialog();
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                TimerControl newTimer = new TimerControl(dialog.TimerName, dialog.TimerNarrative);
                newTimer.Location = new Point(10, timers.Count * 120 + 10);
                newTimer.TimerDeleted += NewTimer_TimerDeleted;

                timers.Add(newTimer);
                mainPanel.Controls.Add(newTimer);

                updateCreateButtonState();
            }
        }

        private void NewTimer_TimerDeleted(object sender, EventArgs e)
        {
            TimerControl timerToRemove = sender as TimerControl;
            if (timerToRemove != null)
            {
                timers.Remove(timerToRemove);
                mainPanel.Controls.Remove(timerToRemove);
                timerToRemove.Dispose();

                // Reposition remaining timers
                for (int i = 0; i < timers.Count; i++)
                {
                    timers[i].Location = new Point(10, i * 120 + 10);
                }

                updateCreateButtonState();
            }
        }

        private void updateCreateButtonState()
        {
            createTimerButton.Enabled = timers.Count < 10;
            if (timers.Count >= 10)
            {
                createTimerButton.Text = "Max Timers (10)";
            }
            else
            {
                createTimerButton.Text = "Create New Timer";
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
            this.Size = new Size(520, 100);
            this.BorderStyle = BorderStyle.FixedSingle;
            this.BackColor = Color.White;

            // Name label
            nameLabel = new Label();
            nameLabel.Text = "Name: " + TimerName;
            nameLabel.Location = new Point(10, 10);
            nameLabel.Size = new Size(400, 20);
            nameLabel.Font = new Font("Arial", 10, FontStyle.Bold);
            this.Controls.Add(nameLabel);

            // Narrative label
            narrativeLabel = new Label();
            narrativeLabel.Text = "Description: " + TimerNarrative;
            narrativeLabel.Location = new Point(10, 30);
            narrativeLabel.Size = new Size(400, 20);
            narrativeLabel.Font = new Font("Arial", 9);
            this.Controls.Add(narrativeLabel);

            // Time label
            timeLabel = new Label();
            timeLabel.Text = "00:00:00";
            timeLabel.Location = new Point(10, 55);
            timeLabel.Size = new Size(200, 30);
            timeLabel.Font = new Font("Arial", 16, FontStyle.Bold);
            timeLabel.ForeColor = Color.DarkBlue;
            this.Controls.Add(timeLabel);

            // Start/Stop button
            startStopButton = new Button();
            startStopButton.Text = "Stop";
            startStopButton.Size = new Size(80, 30);
            startStopButton.Location = new Point(350, 55);
            startStopButton.BackColor = Color.Orange;
            startStopButton.FlatStyle = FlatStyle.Flat;
            startStopButton.Click += StartStopButton_Click;
            this.Controls.Add(startStopButton);

            // Delete button
            deleteButton = new Button();
            deleteButton.Text = "Delete";
            deleteButton.Size = new Size(80, 30);
            deleteButton.Location = new Point(435, 55);
            deleteButton.BackColor = Color.LightCoral;
            deleteButton.FlatStyle = FlatStyle.Flat;
            deleteButton.Click += DeleteButton_Click;
            this.Controls.Add(deleteButton);
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

            this.Text = "Create New Timer";
            this.Size = new Size(400, 250);
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
            nameLabel.Location = new Point(20, 20);
            nameLabel.Size = new Size(100, 20);
            this.Controls.Add(nameLabel);

            // Name textbox
            nameTextBox = new TextBox();
            nameTextBox.Location = new Point(20, 45);
            nameTextBox.Size = new Size(340, 25);
            nameTextBox.MaxLength = 50;
            this.Controls.Add(nameTextBox);

            // Narrative label
            Label narrativeLabel = new Label();
            narrativeLabel.Text = "Description:";
            narrativeLabel.Location = new Point(20, 80);
            narrativeLabel.Size = new Size(100, 20);
            this.Controls.Add(narrativeLabel);

            // Narrative textbox
            narrativeTextBox = new TextBox();
            narrativeTextBox.Location = new Point(20, 105);
            narrativeTextBox.Size = new Size(340, 60);
            narrativeTextBox.Multiline = true;
            narrativeTextBox.MaxLength = 200;
            narrativeTextBox.ScrollBars = ScrollBars.Vertical;
            this.Controls.Add(narrativeTextBox);

            // OK button
            okButton = new Button();
            okButton.Text = "Create Timer";
            okButton.Size = new Size(100, 30);
            okButton.Location = new Point(180, 180);
            okButton.BackColor = Color.LightBlue;
            okButton.FlatStyle = FlatStyle.Flat;
            okButton.Click += OkButton_Click;
            this.Controls.Add(okButton);

            // Cancel button
            cancelButton = new Button();
            cancelButton.Text = "Cancel";
            cancelButton.Size = new Size(100, 30);
            cancelButton.Location = new Point(290, 180);
            cancelButton.BackColor = Color.LightGray;
            cancelButton.FlatStyle = FlatStyle.Flat;
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