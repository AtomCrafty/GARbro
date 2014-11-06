//! \file       Audio.cs
//! \date       Tue Nov 04 17:35:54 2014
//! \brief      audio format class.
//
// Copyright (C) 2014 by morkt
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to
// deal in the Software without restriction, including without limitation the
// rights to use, copy, modify, merge, publish, distribute, sublicense, and/or
// sell copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS
// IN THE SOFTWARE.
//

using System.IO;

namespace GameRes
{
    public struct WaveFormat
    {
        public ushort FormatTag;
        public ushort Channels;
        public   uint SamplesPerSecond;
        public   uint AverageBytesPerSecond;
        public ushort BlockAlign;
        public ushort BitsPerSample;
    }

    public abstract class SoundInput : Stream
    {
        protected Stream    m_input;
        protected long      m_pcm_size;

        public override bool  CanRead { get { return m_input.CanRead; } }
        public override bool CanWrite { get { return false; } }
        public override long   Length { get { return m_pcm_size; } }

        public long PcmSize
        {
            get { return m_pcm_size; }
            protected set { m_pcm_size = value; }
        }

        public abstract int SourceBitrate { get; }

        public WaveFormat Format { get; protected set; }

        protected SoundInput (Stream input)
        {
            m_input = input;
        }

        public virtual void Reset ()
        {
            Position = 0;
        }

        #region System.IO.Stream methods
        public override void Flush()
        {
            m_input.Flush();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            if (origin == SeekOrigin.Begin)
                Position = offset;
            else if (origin == SeekOrigin.Current)
                Position += offset;
            else
                Position = Length + offset;
            return Position;
        }

        public override void SetLength (long length)
        {
            throw new System.NotSupportedException ("SoundInput.SetLength method is not supported");
        }

        public override void Write (byte[] buffer, int offset, int count)
        {
            throw new System.NotSupportedException ("SoundInput.Write method is not supported");
        }

        public override void WriteByte (byte value)
        {
            throw new System.NotSupportedException ("SoundInput.WriteByte method is not supported");
        }
        #endregion

        #region IDisposable Members
        bool disposed = false;
        protected override void Dispose (bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    m_input.Dispose();
                }
                disposed = true;
                base.Dispose (disposing);
            }
        }
        #endregion
    }

    public abstract class AudioFormat : IResource
    {
        public override string Type { get { return "audio"; } }

        public abstract SoundInput TryOpen (Stream file);

        public static SoundInput Read (Stream file)
        {
            var input = new MemoryStream();
            file.CopyTo (input);
            try
            {
                var sound = FindFormat (input);
                if (null != sound)
                    input = null; // input stream is owned by sound object now, don't dispose it
                return sound;
            }
            finally
            {
                if (null != input)
                    input.Dispose();
            }
        }

        public static SoundInput FindFormat (Stream file)
        {
            uint signature = FormatCatalog.ReadSignature (file);
            for (;;)
            {
                var range = FormatCatalog.Instance.LookupSignature<AudioFormat> (signature);
                foreach (var impl in range)
                {
                    try
                    {
                        file.Position = 0;
                        SoundInput sound = impl.TryOpen (file);
                        if (null != sound)
                            return sound;
                    }
                    catch { }
                }
                if (0 == signature)
                    break;
                signature = 0;
            }
            return null;
        }
    }
}
