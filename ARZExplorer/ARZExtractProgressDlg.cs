using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using TQVaultData;

namespace ARZExplorer
{
    public partial class ARZExtractProgressDlg : Form
    {
        public ARZExtractProgressDlg(string baseFolder)
        {
            m_baseFolder = baseFolder;
            InitializeComponent();
        }
        private

        string m_baseFolder;
        string m_recordIDBeingProcessed;
        Exception m_exception;
        bool m_cancel;

        private void ARZExtractProgressDlg_Load(object sender, EventArgs e)
        {
            m_cancel = false;

            // Setup the progress bar
            progressBar1.Maximum = arzFile.Count;
            progressBar1.Value = 0;

            // Create a thread to do the extraction
            ThreadStart tstart = new ThreadStart(this.DoExtraction);
            Thread t = new Thread(tstart);
            t.Priority = ThreadPriority.Normal;
            t.Start();
        }

        private void DoExtraction()
        {
            try
            {
                System.Collections.IEnumerator records = ARZFile.arzFile.GetRecordIDEnumerator();
                bool cancelled = false;

                while (!cancelled && records.MoveNext())
                {
                    string recordID = (string)(records.Current);

                    // update label with recordID
                    m_recordIDBeingProcessed = recordID;
                    this.Invoke(new MethodInvoker(this.UpdateLabel));

                    // Write the record
                    arzFile.GetRecordUnCached(recordID).Write(m_baseFolder);

                    // Update progressbar
                    this.Invoke(new MethodInvoker(this.IncrementProgress));

                    // see if we need to cancel
                    Monitor.Enter(this);
                    cancelled = m_cancel;
                    Monitor.Exit(this);
                }

                // notify complete
                this.Invoke(new MethodInvoker(this.ExtractComplete));
            }
            catch (Exception err)
            {
                // notify failure
                m_exception = err;
                this.Invoke(new MethodInvoker(this.ExtractFailed));
            }
        }

        private void ExtractFailed()
        {
            this.DialogResult = DialogResult.Abort;
            MessageBox.Show(m_exception.ToString(), "Extraction Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            this.Close();
        }

        private void ExtractComplete()
        {
            if (m_cancel)
            {
                MessageBox.Show("Extraction cancelled.");
                this.DialogResult = DialogResult.Cancel;
            }
            else
            {
                MessageBox.Show("Extraction complete.");
                this.DialogResult = DialogResult.OK;
            }
            this.Close();
        }
        private void IncrementProgress()
        {
            progressBar1.PerformStep();
        }
        private void UpdateLabel()
        {
            label1.Text = string.Concat("Extracting ", m_recordIDBeingProcessed, " ...");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Monitor.Enter(this);
            m_cancel = true;
            Monitor.Exit(this);
        }
    }
}