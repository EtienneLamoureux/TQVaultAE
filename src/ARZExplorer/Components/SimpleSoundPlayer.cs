using System;
using System.Drawing;
using System.IO;
using System.Media;
using System.Windows.Forms;
using TQVaultAE.Domain.Entities;

namespace ArzExplorer.Components
{
	public partial class SimpleSoundPlayer : UserControl
	{
		private RecordId _CurrentSoundId;
		private SoundPlayer _CurrentSoundPlayer;

		public SoundPlayer CurrentSoundPlayer
		{
			get => _CurrentSoundPlayer;
			set
			{
				_CurrentSoundPlayer?.Stop();

				_CurrentSoundPlayer = value;

				buttonPlay.PerformClick();
			}
		}
		public RecordId CurrentSoundId
		{
			get => _CurrentSoundId;
			set
			{
				_CurrentSoundId = value;

				if (_CurrentSoundId is not null)
					this.labelFileName.Text = Path.GetFileName(_CurrentSoundId);
			}
		}

		public byte[] CurrentSoundWavData { get; set; }

		public bool MustLoop => this.buttonLoop.BackColor == SystemColors.ActiveCaption;

		public SimpleSoundPlayer()
		{
			InitializeComponent();
		}

		private void SimpleSoundPlayer_Load(object sender, EventArgs e)
		{
			// Adjust UI
			this.buttonLoop.BackColor = SystemColors.Control;

			// TODO SoundPlayer doesn't have a Freeze/Resume
			this.buttonPause.Visible = false;
		}

		private void buttonPlay_Click(object sender, EventArgs e)
		{
			if (MustLoop) CurrentSoundPlayer?.PlayLooping();
			else CurrentSoundPlayer?.Play();
		}

		private void buttonLoop_Click(object sender, EventArgs e)
		{
			this.buttonLoop.BackColor = this.buttonLoop.BackColor == SystemColors.ActiveCaption
				? SystemColors.Control
				: SystemColors.ActiveCaption;
		}

		private void buttonPause_Click(object sender, EventArgs e)
		{
			CurrentSoundPlayer?.Stop();// Freeze/Resume ??
		}

		private void buttonStop_Click(object sender, EventArgs e)
		{
			CurrentSoundPlayer?.Stop();
		}
	}
}
