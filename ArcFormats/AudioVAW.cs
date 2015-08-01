//! \file       AudioVAW.cs
//! \date       Sat Aug 01 12:28:22 2015
//! \brief      Black Cyc audio file.
//
// Copyright (C) 2015 by morkt
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

using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Text;
using GameRes.Utility;

namespace GameRes.Formats.BlackCyc
{
    [Export(typeof(AudioFormat))]
    public class VawAudio : AudioFormat
    {
        public override string         Tag { get { return "VAW"; } }
        public override string Description { get { return "Black Cyc audio format"; } }
        public override uint     Signature { get { return 0; } }

        public VawAudio ()
        {
            Extensions = new string[] { "vaw", "wgq" };
        }

        static readonly Lazy<AudioFormat> WavFormat = new Lazy<AudioFormat> (() => FormatCatalog.Instance.AudioFormats.FirstOrDefault (x => x.Tag == "WAV"));
        static readonly Lazy<AudioFormat> OggFormat = new Lazy<AudioFormat> (() => FormatCatalog.Instance.AudioFormats.FirstOrDefault (x => x.Tag == "OGG"));

        public override SoundInput TryOpen (Stream file)
        {
            var header = ResourceHeader.Read (file);
            if (null == header)
                return null;
            AudioFormat format;
            int offset;
            if (0 == header.PackType)
            {
                if (4 != file.Read (header.Bytes, 0, 4))
                    return null;
                if (!Binary.AsciiEqual (header.Bytes, "RIFF"))
                    return null;
                format = WavFormat.Value;
                offset = 0x40;
            }
            else if (2 == header.PackType)
            {
                format = OggFormat.Value;
                offset = 0x6C;
            }
            else if (6 == header.PackType && Binary.AsciiEqual (header.Bytes, 0x10, "OGG "))
            {
                format = OggFormat.Value;
                offset = 0x40;
            }
            else
                return null;
            var input = new StreamRegion (file, offset, file.Length-offset, true);
            try
            {
                return format.TryOpen (input);
                // FIXME file will be left undisposed
            }
            catch
            {
                input.Dispose();
                throw;
            }
        }

        public override void Write (SoundInput source, Stream output)
        {
            throw new System.NotImplementedException ("EdimFormat.Write not implemenented");
        }
    }
}