﻿//--------------------------------------------
// Movie Player
// Copyright © 2014 SHUU Games
//--------------------------------------------

namespace MP
{
	/// <summary>
	/// Load options for Demuxes and Decoders.
	/// </summary>
	[System.Serializable]
	public class LoadOptions
	{
		/// <summary>
		/// Sets playback audioClip._3Daudio. When TRUE, the audio is affected by AudioListener position and effects.
		/// Otherwise it's played in stereo and all effects are bypassed (default).
		/// </summary>
		public bool _3DSound = false;
		
		/// <summary>
		/// When TRUE, all the audio is cached in memory during load. Otherwise it's streamed (default).
		/// </summary>
		public bool preloadAudio = false;
		
		/// <summary>
		/// When TRUE, the video stream and decoder are not loaded.
		/// </summary>
		public bool skipVideo = false;
		
		/// <summary>
		/// When TRUE, the audio stream and decoder are not loaded.
		/// </summary>
		public bool skipAudio = false;
		
		/// <summary>
		/// The audio stream info for cases when audio decoder
		/// is not able to get it from demux (raw streams).
		/// </summary>
		public AudioStreamInfo audioStreamInfo = null;
		
		/// <summary>
		/// The video stream info for cases when video decoder
		/// is not able to get it from demux (raw streams).
		/// </summary>
		public VideoStreamInfo videoStreamInfo = null;

		/// <summary>
		/// The connect timeout seconds for network streams, used in MP.Net
		/// </summary>
		public float connectTimeout = 10;

		public Demux demuxOverride = null;

		/// <summary>
		/// Gets the default LoadOptions
		/// </summary>
		public static LoadOptions Default { get { return new LoadOptions (); } }
	}
}
