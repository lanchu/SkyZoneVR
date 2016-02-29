//--------------------------------------------
// Movie Player
// Copyright © 2014 SHUU Games
//--------------------------------------------
using UnityEngine;
using System.IO;

namespace MP
{
	public class MovieSource
	{
		public Stream stream;
		public string url;
	}

	public class Movie
	{	
		// contains video data and optionally audio too
		public Stream sourceStream;
		
		// demux is for separating streams so that they can be decoded
		public Demux demux;

		// decoders
		public VideoDecoder videoDecoder;
		public AudioDecoder audioDecoder;
	}

	/// <summary>
	/// Static methods loading, unloading and processing the movies.
	/// </summary>
	public class MoviePlayerUtil
	{
		#region ----- Loading -----

		/// <summary>
		/// Loads movie without audio. It will be ready for playback.
		/// </summary>
		/// <param name="source">Source</param>
		/// <param name="targetFramebuffer">Target framebuffer</param>
		/// <param name="loadOptions">Load options</param>
		public static Movie Load (Stream srcStream, out Texture2D targetFramebuffer, LoadOptions loadOptions = null)
		{
			AudioClip dummyTargetAudioBuffer;
			return Load (new MovieSource () { stream = srcStream }, out targetFramebuffer, out dummyTargetAudioBuffer, loadOptions);
		}

		public static Movie Load (Stream srcStream, out Texture2D targetFramebuffer, out AudioClip targetAudioBuffer, LoadOptions loadOptions = null)
		{
			return Load (new MovieSource () { stream = srcStream }, out targetFramebuffer, out targetAudioBuffer, loadOptions);
		}

		public static Movie Load (string srcUrl, out Texture2D targetFramebuffer, LoadOptions loadOptions = null)
		{
			AudioClip dummyTargetAudioBuffer;
			return Load (new MovieSource () { url = srcUrl }, out targetFramebuffer, out dummyTargetAudioBuffer, loadOptions);
		}

		public static Movie Load (string srcUrl, out Texture2D targetFramebuffer, out AudioClip targetAudioBuffer, LoadOptions loadOptions = null)
		{
			return Load (new MovieSource () { url = srcUrl }, out targetFramebuffer, out targetAudioBuffer, loadOptions);
		}

		/// <summary>
		/// Loads movie with audio. It will be ready for playback
		/// </summary>
		/// <param name="source">Source</param>
		/// <param name="targetFramebuffer">Target framebuffer</param>
		/// <param name="targetAudioBuffer">Target audio buffer</param>
		/// <param name="loadOptions">Load options</param>
		public static Movie Load (MovieSource source, out Texture2D targetFramebuffer, out AudioClip targetAudioBuffer, LoadOptions loadOptions = null)
		{
			if (loadOptions == null)
				loadOptions = LoadOptions.Default;

			if (source.stream == null && source.url == null) {
				throw new MpException ("Either source.stream or source.url must be provided");
			}

			targetFramebuffer = null;
			targetAudioBuffer = null;

			var movie = new Movie ();
			movie.sourceStream = source.stream; // can be NULL

			// create and initialize demux for the source data
			if (source.url != null) {
				movie.demux = loadOptions.demuxOverride != null ? loadOptions.demuxOverride : Streamer.forUrl (source.url);
				((Streamer)movie.demux).Connect (source.url, loadOptions);
			} else {
				movie.demux = loadOptions.demuxOverride != null ? loadOptions.demuxOverride : Demux.forSource (source.stream);
				movie.demux.Init (source.stream, loadOptions);
			}

			if (movie.demux.hasVideo && !loadOptions.skipVideo) {
				movie.videoDecoder = VideoDecoder.CreateFor (movie.demux.videoStreamInfo);
				movie.videoDecoder.Init (out targetFramebuffer, movie.demux, loadOptions);
			}
			if (movie.demux.hasAudio && !loadOptions.skipAudio) {
				movie.audioDecoder = AudioDecoder.CreateFor (movie.demux.audioStreamInfo);
				movie.audioDecoder.Init (out targetAudioBuffer, movie.demux, loadOptions);
			}
			return movie;
		}

		/// <summary>
		/// Unloads the movie and releases all resources like file handlers associated with it.
		/// It's a good idea to stop the playback before unloading the movie.
		/// </summary>
		public static void Unload (Movie movie)
		{
			if (movie != null) {
				if (movie.sourceStream != null) {
					movie.sourceStream.Dispose ();
					movie.sourceStream = null;
				}
				if (movie.videoDecoder != null) {
					movie.videoDecoder.Shutdown ();
					movie.videoDecoder = null;
				}
				if (movie.audioDecoder != null) {
					movie.audioDecoder.Shutdown ();
					movie.audioDecoder = null;
				}
				if (movie.demux != null) {
					movie.demux.Shutdown ();
					movie.demux = null;
				}
			}
		}

		#endregion

		#region ----- Extracting raw audio or video data from containers -----

		/// <summary>
		/// Extracts raw audio from the movie
		/// </summary>
		/// <returns>The raw audio</returns>
		/// <param name="src">File contents</param>
		public static byte[] ExtractRawAudio (Stream sourceStream)
		{
			Demux dummyDemux;
			return ExtractRawAudio (sourceStream, out dummyDemux);
		}

		/// <summary>
		/// Extracts raw audio from the movie.
		/// Usable only for reasonable sized streams, because byte[] is returned.
		/// </summary>
		/// <returns>The raw audio</returns>
		/// <param name="src">File contents</param>
		/// <param name="demuxUsed">Demux used</param>
		public static byte[] ExtractRawAudio (Stream sourceStream, out Demux demux)
		{
			demux = Demux.forSource (sourceStream);
			demux.Init (sourceStream);
			if (!demux.hasAudio)
				return null;

			byte[] bigBuf = new byte[demux.audioStreamInfo.lengthBytes];
			demux.ReadAudioSamples (out bigBuf, demux.audioStreamInfo.sampleCount);
			return bigBuf;
		}

		/// <summary>
		/// Extracts raw video from the movie
		/// </summary>
		/// <returns>The raw video</returns>
		/// <param name="src">File contents</param>
		public static byte[] ExtractRawVideo (Stream sourceStream)
		{
			Demux dummyDemux;
			return ExtractRawVideo (sourceStream, out dummyDemux);
		}

		/// <summary>
		/// Extracts raw video from the movie.
		/// Usable only for reasonable sized streams, because byte[] is returned.
		/// </summary>
		/// <returns>The raw video</returns>
		/// <param name="src">File contents</param>
		/// <param name="demuxUsed">Demux used</param>
		public static byte[] ExtractRawVideo (Stream sourceStream, out Demux demux)
		{
			demux = Demux.forSource (sourceStream);
			demux.Init (sourceStream);
			if (!demux.hasVideo)
				return null;

			byte[] bigBuf = new byte[demux.videoStreamInfo.lengthBytes];
			int bigBufOffset = 0;
			int bytesRead = 0;
			do {
				byte[] buf;
				bytesRead = demux.ReadVideoFrame (out buf);
				System.Array.Copy (buf, 0, bigBuf, bigBufOffset, bytesRead);
				bigBufOffset += bytesRead;
			} while(bytesRead > 0);
			return bigBuf;
		}

		#endregion
	}
}
