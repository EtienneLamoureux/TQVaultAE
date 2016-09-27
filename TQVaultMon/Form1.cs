//-----------------------------------------------------------------------
// <copyright file="Form1.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace TQVaultMon
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Drawing;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Windows.Forms;

    /// <summary>
    /// Windows Form class for TQVaultMon
    /// </summary>
    internal partial class Form1 : Form
    {
        /// <summary>
        /// Message Queue
        /// </summary>
        private Queue messageQueue;

        /// <summary>
        /// Header which is displayed in the message queue at program start
        /// </summary>
        private string[] header;

        /// <summary>
        /// Maximum messages to show to the user
        /// </summary>
        private int maximumMessages;

        /// <summary>
        /// The last process ID that was patched.
        /// </summary>
        private int lastProcessIDHandled;

        /// <summary>
        /// Retry counter for patching
        /// </summary>
        private int numberOfRetries;

        /// <summary>
        /// Initializes a new instance of the Form1 class.
        /// </summary>
        public Form1()
        {
            this.InitializeComponent();

            this.lastProcessIDHandled = -1;
            this.maximumMessages = 500;

            // will store our messages
            this.messageQueue = new Queue();
            this.numberOfRetries = -1;

            // Get the version number from the assembly
            Assembly assembly = Assembly.GetExecutingAssembly();
            AssemblyName assemblyName = assembly.GetName();
            string[] titleFields =
            {
                assemblyName.Name, assemblyName.Version.ToString(), "by bman654 (wickedsoul_@yahoo.com)"
            };

            string[] tmpHeader =
            {
                string.Join(", ", titleFields),
                string.Empty
            };

            this.header = tmpHeader;

            // I had the rest of this stuff in the header but I decided it would be better for it to scroll off.
            string[] openingMsg =
            {
                "This program will prevent Titan Quest from detecting that your character's inventory has been modified outside of the game.",
                "As long as this program is running, whenever it sees Titan Quest starting on your computer, it will patch it to allow modified characters.",
                "Once you have successfully loaded your character, you do not need to use this tool again unless you modify your character again.",
                string.Empty,
                "Of course, you could always get in the habit of starting Titan Quest from this program so you never need worry again...",
                string.Empty,
                "This version works with Titan Quest versions 1.01, 1.08, 1.11, 1.15, 1.20, 1.30",
                string.Empty,
                "Special thanks to _spx_ for his original Character Recovery Tool.  The patch engine in this program is based upon his work.",
                "Special thanks to SoulSeekkor and his TQ Defiler for pointing me in the right direction to get started on finding the checksum.",
                string.Empty,
                "NEW IN V1.1: If you need to override the startup program used to start Titan Quest.exe, then make a file called TQSTART.BAT",
                "and put in this batch file the correct startup commands to launch Titan Quest.  TQVaulMon will use TQSTART.BAT if it finds it",
                string.Empty,
                "Monitoring the process list for Titan Quest.exe...",
                string.Empty
            };

            foreach (string msg in openingMsg)
            {
                this.messageQueue.Enqueue(msg);
            }

            this.MarkHappy();
            this.AssignOutput();
        }

        /// <summary>
        /// Gets the path to Titan Quest install folder
        /// </summary>
        private static string TQPath
        {
            get
            {
                string[] path = new string[4];
                path[0] = "SOFTWARE";
                path[1] = "Iron Lore";
                path[2] = "Titan Quest";
                path[3] = "Install Location";
                string registryKey = ReadRegistryKey(Microsoft.Win32.Registry.LocalMachine, path);
                if (string.IsNullOrEmpty(registryKey))
                {
                    throw new ArgumentException("Unable to locate Titan Quest installation via the registry");
                }

                return registryKey;
            }
        }

        /// <summary>
        /// Attempts to read a key from the registry
        /// </summary>
        /// <param name="key">key that we are reading</param>
        /// <param name="path">string array which has the path to the key</param>
        /// <returns>string with the key value</returns>
        private static string ReadRegistryKey(Microsoft.Win32.RegistryKey key, string[] path)
        {
            int valueKey = path.Length - 1;
            int lastSubKey = path.Length - 2;

            for (int i = 0; i <= lastSubKey; ++i)
            {
                key = key.OpenSubKey(path[i]);
                if (key == null)
                {
                    return string.Empty;
                    ////throw new ApplicationException(string.Format("Unable to read registry setting '{0}'", string.Join("\\", path)));
                }
            }

            return (string)key.GetValue(path[valueKey]);
        }

        /// <summary>
        /// Tries to find the Titan Quest Exe process in memory.
        /// </summary>
        /// <returns>Returns the TQ Process or NULL if not found</returns>
        private static Process FindTQProcess()
        {
            Process[] processes = Process.GetProcessesByName("titan quest");

            if (processes.Length > 0)
            {
                return processes[0];
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Tries to find the TQVault Exe process in memory.
        /// </summary>
        /// <returns>Returns the TQVault Process or NULL if not found</returns>
        private static Process FindTQVaultProcess()
        {
            Process[] processes = Process.GetProcessesByName("tqvault");

            if (processes.Length > 0)
            {
                return processes[0];
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Changes the color to blue to indicate things are good.
        /// </summary>
        private void MarkHappy()
        {
            this.outputTextBox.ForeColor = Color.DeepSkyBlue;
        }

        /// <summary>
        /// Changes the color to red to indicate something is wrong.
        /// </summary>
        private void MarkSad()
        {
            this.outputTextBox.ForeColor = Color.Red;
        }

        /// <summary>
        /// Updates the display text box
        /// </summary>
        private void AssignOutput()
        {
            string[] lines = new string[this.header.Length + this.messageQueue.Count + 1];
            Array.Copy(this.header, 0, lines, 0, this.header.Length);

            // Now copy the output lines
            int iline = this.header.Length;
            foreach (object obj in this.messageQueue)
            {
                lines[iline++] = (string)obj;
            }

            lines[iline] = string.Empty;

            // now assign to the box
            this.outputTextBox.Lines = lines;
            this.outputTextBox.Select(this.outputTextBox.TextLength, 0);
            this.outputTextBox.ScrollToCaret();
        }

        /// <summary>
        /// Adds an array of messages to the message queue
        /// </summary>
        /// <param name="messages">string array of messages to be added.</param>
        private void AddMessages(string[] messages)
        {
            // Tack on the timestamp to the front of the message
            string timestamp = DateTime.Now.ToString();

            foreach (string message in messages)
            {
                this.messageQueue.Enqueue(string.Format(CultureInfo.InvariantCulture, "{0}: {1}", timestamp, message));
            }

            while (this.messageQueue.Count > this.maximumMessages)
            {
                this.messageQueue.Dequeue();
            }

            this.AssignOutput();
        }

        /// <summary>
        /// Adds a single message to the message queue
        /// </summary>
        /// <param name="message">Message to be added</param>
        private void AddMessage(string message)
        {
            // Split the message on newlines
            char[] newlines = { '\n' };
            this.AddMessages(message.Split(newlines));
        }

        /// <summary>
        /// Checks to see the process was already patched.  Compares the current process to the last patched process.
        /// </summary>
        /// <param name="process">Process name we are looking for</param>
        /// <returns>True if the process was the last one that was patched.</returns>
        private bool HavePatchedProcessAlready(Process process)
        {
            return process.Id == this.lastProcessIDHandled;
        }

        /// <summary>
        /// Attempts to find Titan Quest process and apply the proper patch.
        /// </summary>
        private void FindAndPatchTQProcess()
        {
            Process process = FindTQProcess();

            // Enable/Disable the Start TQ button depending on if TQ is already running.
            this.startTitanQuestButton.Visible = process == null;

            // Make sure we found a process and that we have not patched it already
            if ((process != null) && !this.HavePatchedProcessAlready(process))
            {
                this.AddMessage(string.Format(CultureInfo.InvariantCulture, "Titan Quest Startup Detected (pid={0}).", process.Id));

                this.PatchProcess(process);
            }
        }

        /// <summary>
        /// Patches the Titan Quest process
        /// </summary>
        /// <param name="process">name of the process</param>
        private void PatchProcess(Process process)
        {
            // Open the process memory
            // 0x38 = PROCESS_VM_READ | PROCESS_VM_WRITE | PROCESS_VM_OPERATION
            IntPtr handle = NativeMethods.OpenProcess(0x38, 0, (uint)process.Id);
            if (handle == IntPtr.Zero)
            {
                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "Unable to access the process memory of pid={0}.", process.Id));
            }

            try
            {
                if (!this.TryPatchV1_20(handle) && !this.TryPatchV1_15(handle) && !this.TryPatchV1_11(handle) && !this.TryPatchV1_08(handle) && !this.TryPatchV1_01(handle))
                {
                    this.MarkSad();
                    if (this.numberOfRetries == -1)
                    {
                        this.numberOfRetries = 60;
                    }

                    if (this.numberOfRetries == 0)
                    {
                        this.numberOfRetries = -1;
                        this.AddMessage("Unable to patch.  I give up.  This must be a version of Titan Quest I have not seen.");

                        // mark it as done.
                        this.lastProcessIDHandled = process.Id;
                    }
                    else
                    {
                        this.AddMessage(string.Format(CultureInfo.InvariantCulture, "Unable to patch.  It may not be fully loaded yet.  Will try {0} more times.", this.numberOfRetries));
                        --this.numberOfRetries;
                    }
                }
                else
                {
                    this.MarkHappy();

                    // Remember that we have gotten this guy already.
                    this.lastProcessIDHandled = process.Id;
                    this.numberOfRetries = -1;
                }
            }
            finally
            {
                if (NativeMethods.CloseHandle(handle) == 0)
                {
                    this.AddMessage("Unable to close the handle");
                }
            }
        }

        /// <summary>
        /// Attempts to apply a patch to process in memory
        /// </summary>
        /// <param name="handle">handle to the process we are patching</param>
        /// <param name="items">PatchItems array of patches</param>
        /// <param name="version">version we are attempting to patch</param>
        /// <returns>true if already patched or the patch was successful.</returns>
        private bool TryPatch(IntPtr handle, PatchItem[] items, string version)
        {
            int alreadyPatchedCount = 0;

            // verify all the patch locations
            foreach (PatchItem item in items)
            {
                IntPtr ptrNumBytes;
                byte[] readBytes = new byte[item.OriginalData.Length];

                if (NativeMethods.ReadProcessMemory(handle, item.Address, readBytes, (IntPtr)item.OriginalData.Length, out ptrNumBytes) == 0)
                {
                    ////throw new System.Exception(String.Format("Unable to read memory 0x{X}", item.Address));
                    return false;
                }

                // Verify that what we read matches what we were looking for
                if (ptrNumBytes.ToInt32() != item.OriginalData.Length)
                {
                    ////throw new System.Exception(String.Format("Invalid length at 0x{0:X}", item.Address));
                    return false; // not enough bytes
                }

                ////System.Text.StringBuilder dbgStr = new System.Text.StringBuilder();
                ////dbgStr.AppendFormat("Reading 0x{0:x} = ", item.Address);
                ////for (int jj = 0; jj < item.originalData.Length; ++jj) {
                //// dbgStr.AppendFormat(" {0:X}", readBytes[jj]);
                ////}
                ////AddMessage(dbgStr.ToString());

                bool isOK = true;
                for (int b = 0; b < item.OriginalData.Length; ++b)
                {
                    if (readBytes[b] != item.OriginalData[b])
                    {
                        isOK = false;
                        break;
                    }
                }

                if (!isOK)
                {
                    isOK = true;

                    // Hmm it does not match original.  Lets check if maybe it has been patched already
                    for (int b = 0; b < item.OriginalData.Length; ++b)
                    {
                        if (readBytes[b] != item.NewData[b])
                        {
                            isOK = false;
                            break;
                        }
                    }

                    if (!isOK)
                    {
                        // does not match new either.  It must truly be the wrong match.
                        return false;
                    }
                    else
                    {
                        // It matches new.  Lets increment the alreadyPatchedCount
                        alreadyPatchedCount++;
                    }
                }
            }

            // See if we found some sections already patched
            if (alreadyPatchedCount == items.Length)
            {
                // Patch is already applied.  Mark it as such.
                this.AddMessage(string.Format(CultureInfo.InvariantCulture, "Version {0} detected.  It looks like this process has already been patched.", version));
                return true;
            }
            else if (alreadyPatchedCount > 0)
            {
                // Hmm we had *some* but not all tagged as already patched.
                // Consider this a non-match
                return false;
            }

            // now apply the patches
            foreach (PatchItem item in items)
            {
                IntPtr ptrNumBytes;

                // Now write the patched data
                if (NativeMethods.WriteProcessMemory(handle, item.Address, item.NewData, (IntPtr)item.NewData.Length, out ptrNumBytes) == 0)
                {
                    throw new InvalidOperationException("Unable to modify process memory.");
                }
            }

            this.AddMessage(string.Format(CultureInfo.InvariantCulture, "Version {0} detected.  Game patched successfully.", version));
            return true;
        }

        /// <summary>
        /// Attempts to patch version 1.20
        /// </summary>
        /// <param name="handle">process handle we are patching</param>
        /// <returns>true if already patched or successful.</returns>
        private bool TryPatchV1_20(IntPtr handle)
        {
            PatchItem[] items = new PatchItem[2];

            // 0x00403CAD F3 A6 75 04 -> F3 A6 75 00
            byte[] o1 = { 0xF3, 0xA6, 0x75, 0x04 };
            byte[] n1 = { 0xF3, 0xA6, 0x75, 0x00 };
            items[0] = new PatchItem((IntPtr)0x00403CAD, o1, n1);

            // 0x004085C5 0f 84 7c 00 00 00 (je 00408647) -> e9 7d 00 00 00 90 (jmp 00408647)
            byte[] o2 = { 0x0F, 0x84, 0x7C, 0x00, 0x00, 0x00 };
            byte[] n2 = { 0xE9, 0x7D, 0x00, 0x00, 0x00, 0x90 };
            items[1] = new PatchItem((IntPtr)0x004085C5, o2, n2);

            return this.TryPatch(handle, items, "1.20/1.30");
        }

        /// <summary>
        /// Attempts to patch version 1.15
        /// </summary>
        /// <param name="handle">process handle we are patching</param>
        /// <returns>true if already patched or successful.</returns>
        private bool TryPatchV1_15(IntPtr handle)
        {
            PatchItem[] items = new PatchItem[2];

            // 0x0040400D F3 A6 75 04 -> F3 A6 75 00
            byte[] o1 = { 0xF3, 0xA6, 0x75, 0x04 };
            byte[] n1 = { 0xF3, 0xA6, 0x75, 0x00 };
            items[0] = new PatchItem((IntPtr)0x0040400D, o1, n1);

            // 0x004089E5 0f 84 7c 00 00 00 (je 00408a67) -> e9 7d 00 00 00 90 (jmp 00408a67)
            byte[] o2 = { 0x0F, 0x84, 0x7C, 0x00, 0x00, 0x00 };
            byte[] n2 = { 0xE9, 0x7D, 0x00, 0x00, 0x00, 0x90 };
            items[1] = new PatchItem((IntPtr)0x004089E5, o2, n2);

            return this.TryPatch(handle, items, "1.15");
        }

        /// <summary>
        /// Attempts to patch version 1.11
        /// </summary>
        /// <param name="handle">process handle we are patching</param>
        /// <returns>true if already patched or successful.</returns>
        private bool TryPatchV1_11(IntPtr handle)
        {
            PatchItem[] items = new PatchItem[2];

            byte[] o1 = { 0xF3, 0xA6, 0x75, 0x04 };
            byte[] n1 = { 0xF3, 0xA6, 0x75, 0x00 };
            items[0] = new PatchItem((IntPtr)0x00403F33, o1, n1);

            byte[] o2 = { 0xF3, 0xA6, 0x74, 0x6E };
            byte[] n2 = { 0xF3, 0xA6, 0xEB, 0x6E };
            items[1] = new PatchItem((IntPtr)0x00408943, o2, n2);

            return this.TryPatch(handle, items, "1.11");
        }

        /// <summary>
        /// Attempts to patch version 1.08
        /// </summary>
        /// <param name="handle">process handle we are patching</param>
        /// <returns>true if already patched or successful.</returns>
        private bool TryPatchV1_08(IntPtr handle)
        {
            PatchItem[] items = new PatchItem[2];

            byte[] o1 = { 0xF3, 0xA6, 0x75, 0x04 };
            byte[] n1 = { 0xF3, 0xA6, 0x75, 0x00 };
            items[0] = new PatchItem((IntPtr)0x00403F13, o1, n1);

            byte[] o2 = { 0xF3, 0xA6, 0x74, 0x6E };
            byte[] n2 = { 0xF3, 0xA6, 0xEB, 0x6E };
            items[1] = new PatchItem((IntPtr)0x00408923, o2, n2);

            return this.TryPatch(handle, items, "1.08");
        }

        /// <summary>
        /// Attempts to patch version 1.01
        /// </summary>
        /// <param name="handle">process handle we are patching</param>
        /// <returns>true if already patched or successful.</returns>
        private bool TryPatchV1_01(IntPtr handle)
        {
            PatchItem[] items = new PatchItem[2];

            byte[] o1 = { 0xF3, 0xA6, 0x75, 0x04 };
            byte[] n1 = { 0xF3, 0xA6, 0x75, 0x00 };
            items[0] = new PatchItem((IntPtr)0x00403F13, o1, n1);

            byte[] o2 = { 0x84, 0xC0, 0x74, 0x5F };
            byte[] n2 = { 0x84, 0xC0, 0xEB, 0x5F };
            items[1] = new PatchItem((IntPtr)0x004082EE, o2, n2);

            return this.TryPatch(handle, items, "1.01");
        }

        /// <summary>
        /// Handler for the timer tick
        /// </summary>
        /// <param name="sender">sender object</param>
        /// <param name="e">EventArgs data</param>
        private void TimerTick(object sender, EventArgs e)
        {
            try
            {
                this.FindAndPatchTQProcess();
            }
            catch (ArgumentException exception)
            {
                this.AddMessage(exception.ToString());
            }
        }

        /// <summary>
        /// Handler for clicking the Start Titan Quest Button
        /// </summary>
        /// <param name="sender">sender object</param>
        /// <param name="e">EventArgs data</param>
        private void StartTitanQuestButtonClick(object sender, EventArgs e)
        {
            try
            {
                // Make sure TQ is not already running
                Process process = FindTQProcess();
                if (process != null)
                {
                    this.AddMessage("Titan Quest is already running!");
                }
                else
                {
                    string command = Path.Combine(TQPath, "Titan Quest.exe");
                    if (File.Exists("TQStart.bat"))
                    {
                        command = "TQStart.bat";
                    }

                    // Let's try to start it!
                    ProcessStartInfo startInfo = new ProcessStartInfo(command);
                    startInfo.WorkingDirectory = TQPath;
                    process = Process.Start(startInfo);
                    this.AddMessage(string.Format(CultureInfo.InvariantCulture, "Titan Quest successfully started with pid={0}", process.Id));
                }
            }
            catch (FileNotFoundException)
            {
                this.AddMessage("Error starting Titan Quest.");
                this.AddMessage("The file was not found.");
            }
            catch (ObjectDisposedException exception)
            {
                this.AddMessage("Error starting Titan Quest.");
                this.AddMessage(exception.ToString());
            }
            catch (Win32Exception win32Exception)
            {
                this.AddMessage("Error starting Titan Quest.");
                this.AddMessage(win32Exception.ToString());
            }
        }

        /// <summary>
        /// Handler for clicking the Start TQVault Button
        /// </summary>
        /// <param name="sender">sender object</param>
        /// <param name="e">EventArgs data</param>
        private void StartTQVaultButtonClick(object sender, EventArgs e)
        {
            try
            {
                // Make sure TQ is not already running
                Process process = FindTQVaultProcess();
                if (process != null)
                {
                    this.AddMessage("TQVault is already running!");
                }
                else
                {
                    string command = string.Concat(Path.GetDirectoryName(Application.ExecutablePath), "\\TQVault.exe");

                    // Let's try to start it!
                    process = Process.Start(command);
                    this.AddMessage(string.Format(CultureInfo.InvariantCulture, "TQVault successfully started with pid={0}", process.Id));
                }
            }
            catch (FileNotFoundException)
            {
                this.AddMessage("Error starting TQVault.");
                this.AddMessage("The file was not found.");
            }
            catch (ObjectDisposedException exception)
            {
                this.AddMessage("Error starting TQVault.");
                this.AddMessage(exception.ToString());
            }
            catch (Win32Exception win32Exception)
            {
                this.AddMessage("Error starting TQVault.");
                this.AddMessage(win32Exception.ToString());
            }
        }

        /// <summary>
        /// Structure which holds the details of a memory patch
        /// </summary>
        public struct PatchItem
        {
            /// <summary>
            /// Address of the patch
            /// </summary>
            public IntPtr Address;

            /// <summary>
            /// Original data which should be at the address before patching
            /// </summary>
            public byte[] OriginalData;

            /// <summary>
            /// Data that will be written into the address.
            /// </summary>
            public byte[] NewData;

            /// <summary>
            /// Initializes a new instance of the PatchItem struct.
            /// </summary>
            /// <param name="address">address of the patch</param>
            /// <param name="originalData">original data which should be at the address</param>
            /// <param name="newData">new data that will be patched into the address</param>
            public PatchItem(IntPtr address, byte[] originalData, byte[] newData)
            {
                this.Address = address;
                this.OriginalData = originalData;
                this.NewData = newData;
            }
        }

        /// <summary>
        /// Native Methods class for P/Invoke
        /// </summary>
        internal static class NativeMethods
        {
            [DllImport("kernel32.dll")]
            public static extern IntPtr OpenProcess(
                uint desiredAccess,
                int inheritHandle,
                uint processId);

            [DllImport("kernel32.dll")]
            public static extern int ReadProcessMemory(
                IntPtr process,
                IntPtr baseAddress,
                [In, Out] byte[] buffer,
                IntPtr size,
                out IntPtr numberOfBytesRead);

            [DllImport("kernel32.dll")]
            public static extern int CloseHandle(
                IntPtr objectHandle);

            [DllImport("kernel32.dll")]
            public static extern int WriteProcessMemory(
                IntPtr process,
                IntPtr baseAddress,
                [In, Out] byte[] buffer,
                IntPtr size,
                out IntPtr numberOfBytesWritten);
        }
    }
}