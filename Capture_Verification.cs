using DPUruNet;
using System;
using System.Windows.Forms;

namespace Fingerprint_All
{
    public partial class Capture_Verification : Form
    {
        public Form1 _sender;
        private const int PROBABILITY_ONE = 0x7fffffff;
        private Fmd firstFinger;
        private Fmd secondFinger;
        private int count;

        public Capture_Verification()
        {
            InitializeComponent();
        }


        private void Capture_Verification_Load(object sender, EventArgs e)
        {
            firstFinger = null;
            secondFinger = null;
            count = 0;
            if (!_sender.OpenReader())
            {
                MessageBox.Show("Failed");
            }
            else if (!_sender.StartCaptureAsync(this.OnCaptured))
            {
                MessageBox.Show("Failed");
            }
            else
            {
                Reader reader = _sender.CurrentReader;
                UpdateStatusLabel(label3, "Using reader: " + reader.ToString());
                UpdateStatusLabel(label2, "Capture started successfully");
                UpdateStatusLabel(label4, "Place your finger on the reader");
            }
        }

        private void OnCaptured(CaptureResult captureResult)
        {
            try
            {
               
                if (!_sender.CheckCaptureResult(captureResult))
                    return;
                UpdateStatusLabel(label5, "A finger was captured.");

                DataResult<Fmd> resultConversion = FeatureExtraction.CreateFmdFromFid(captureResult.Data, Constants.Formats.Fmd.ANSI);
                if (resultConversion.ResultCode != Constants.ResultCode.DP_SUCCESS)
                {
                    _sender.Reset = true;
                    throw new Exception(resultConversion.ResultCode.ToString());
                }

                if (count == 0)
                {
                    firstFinger = resultConversion.Data;
                    count += 1;
                    UpdateStatusLabel(label6, "Now place the same or a different finger on the reader.");
                }
                else if (count == 1)
                {
                    secondFinger = resultConversion.Data;
                    CompareResult compareResult = Comparison.Compare(firstFinger, 0, secondFinger, 0);
                    if (compareResult.ResultCode != Constants.ResultCode.DP_SUCCESS)
                    {
                        _sender.Reset = true;
                        throw new Exception(compareResult.ResultCode.ToString());
                    }

                    string scoreText = (compareResult.Score < (PROBABILITY_ONE / 100000)) ? "fingerprints matched" : "fingerprints did not match";
                    UpdateStatusLabel(label7, $"Comparison resulted in a dissimilarity score of {compareResult.Score} ({scoreText})");


                    
                    // Reset variables and labels for the next person
                    firstFinger = null;
                    secondFinger = null;
                    count = 0;
               


                    _sender.CancelCaptureAndCloseReader(this.OnCaptured);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }


        private void UpdateStatusLabel(Label label, string text)
        {
            if (label.InvokeRequired)
            {
                label.Invoke(new Action(() => UpdateStatusLabel(label, text)));
            }
            else
            {
                label.Text = text;
                label.Visible = true;
            }
        }
        private void ResetProgram()
        {
            // Reset variables and labels for a fresh start
            firstFinger = null;
            secondFinger = null;
            count = 0;

            // Update the UI labels
            UpdateStatusLabel(label5, "");
            UpdateStatusLabel(label6, "");
            UpdateStatusLabel(label7, "");
            // ... (any other labels you want to reset)
        }
        private void Verification_Closed(object sender, System.EventArgs e)
        {
           
        }


        private void button1_Click(object sender, EventArgs e)
        {
            ResetProgram();

            // Open the reader before starting a new capture
            if (!_sender.OpenReader())
            {
                MessageBox.Show("Failed to open reader.");
            }
            else if (!_sender.StartCaptureAsync(this.OnCaptured))
            {
                MessageBox.Show("Failed to start capture.");
            }
            else
            {
                Reader reader = _sender.CurrentReader;
                UpdateStatusLabel(label3, "Using reader: " + reader.ToString());
                UpdateStatusLabel(label2, "Capture started successfully");
                UpdateStatusLabel(label4, "Place your finger on the reader");
            }
        }
    }
}
