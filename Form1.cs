using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using DPCtlUruNet;
using DPUruNet;

namespace Fingerprint_All
{
    public partial class Form1 : Form
    {
        private ReaderCollection _readers;
        private Reader _reader;
        public string selectReader;

        public Form1()
        {
            InitializeComponent();
            
        }
        public Reader CurrentReader
        {
            get { return _reader; }
            set { _reader = value; }
        }
        public bool Reset
        {
            get { return reset; }
            set { reset = value; }
        }
        private bool reset;
        public bool IsReaderOpen()
        {
            return _reader != null && _reader.Status.Status == Constants.ReaderStatuses.DP_STATUS_READY;
        }
        public void allReader()
        {
            _readers = ReaderCollection.GetReaders();
            foreach (Reader Reader in _readers)
            {     
                selectReader = Reader.Description.SerialNumber;
                CurrentReader = Reader;          
                Console.WriteLine(CurrentReader);
            }
           
        }
 
        public bool OpenReader()
        {
            Constants.ResultCode result = Constants.ResultCode.DP_DEVICE_FAILURE;

            result = _reader.Open(Constants.CapturePriority.DP_PRIORITY_COOPERATIVE);

            if (result != Constants.ResultCode.DP_SUCCESS)
            {
                Console.WriteLine(("Error from openreader method: " + result));
                return false;
            }

            return true;
        }
        public void GetStatus()
        {
            Constants.ResultCode result = _reader.GetStatus();

            if ((result != Constants.ResultCode.DP_SUCCESS))
            {
               
                throw new Exception("" + result);
            }

            if ((_reader.Status.Status == Constants.ReaderStatuses.DP_STATUS_BUSY))
            {
                Thread.Sleep(50);
            }
            else if ((_reader.Status.Status == Constants.ReaderStatuses.DP_STATUS_NEED_CALIBRATION))
            {
                _reader.Calibrate();
            }
            else if ((_reader.Status.Status != Constants.ReaderStatuses.DP_STATUS_READY))
            {
                throw new Exception("Reader Status - " + _reader.Status.Status);
            }
        }
        public bool StartCaptureAsync(Reader.CaptureCallback OnCaptured)
        {
            // Activate capture handler
            _reader.On_Captured += new Reader.CaptureCallback(OnCaptured);

            // Call capture
            if (!CaptureFingerAsync())
            {
                return false;
            }

            return true;
        }
        public bool CaptureFingerAsync()
        {
            try
            {
                GetStatus();

                Constants.ResultCode captureResult = _reader.CaptureAsync(Constants.Formats.Fid.ANSI, Constants.CaptureProcessing.DP_IMG_PROC_DEFAULT, _reader.Capabilities.Resolutions[0]);
                if (captureResult != Constants.ResultCode.DP_SUCCESS)
                {
                  
                    throw new Exception("" + captureResult);
                }

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error:  " + ex.Message);
                return false;
            }
        }
        public bool CheckCaptureResult(CaptureResult captureResult)
        {
            if (captureResult.Data == null)
            {
                if (captureResult.ResultCode != Constants.ResultCode.DP_SUCCESS)
                {
                    reset = true;
                    throw new Exception(captureResult.ResultCode.ToString());
                }

                // Send message if quality shows fake finger
                if ((captureResult.Quality != Constants.CaptureQuality.DP_QUALITY_CANCELED))
                {
                    throw new Exception("Quality - " + captureResult.Quality);
                }
                return false;
            }

            return true;
        }


        private void Form1_Load(object sender, EventArgs e)
        {

        }



        private Capture_Verification _verification;
        private void button2_Click(object sender, EventArgs e)
        {
            if (_verification == null)
            {
                _verification = new Capture_Verification();
                _verification._sender = this;
                _verification.FormClosed += VerificationFormClosed; // Subscribe to FormClosed event
            }

            allReader();
            this.Enabled = false;
            _verification.Show();
        }



        public void CancelCaptureAndCloseReader(Reader.CaptureCallback OnCaptured)
        {
            if (_reader != null)
            {
                // Dispose of reader handle and unhook reader events.
                _reader.Dispose();

                if (reset)
                {
                    CurrentReader = null;
                }
            }
        }
        private void VerificationFormClosed(object sender, FormClosedEventArgs e)
        {
            _verification.FormClosed -= VerificationFormClosed; // Unsubscribe from FormClosed event
            this.Enabled = true;
            _verification = null;
            if (_reader != null)
            {
                _reader.Dispose(); // Dispose of the reader when the verification form is closed
                _reader = null;
            }
        }



        private void button3_Click(object sender, EventArgs e)
        {

        }
    }
}
