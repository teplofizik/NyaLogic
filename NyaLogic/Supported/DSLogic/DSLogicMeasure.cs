using NyaLogic.Base;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

namespace NyaLogic.Supported.DSLogic
{
    public class DSLogicMeasure : Measure
    {
        private System.IO.Compression.ZipArchive Archive;
        private DSHeader? Header;
        private List<DSLogicSequence> Sequences = new List<DSLogicSequence>();

        public DSLogicMeasure(ZipArchive Archive)
        {
            this.Archive = Archive;
        }

        public override int SequenceCount => Sequences.Count;

        public override Sequence? GetSequence(int Index) => Sequences[Index];

        public void Add(DSLogicSequence Seq) 
        {
            Sequences.Add(Seq);
        }

        private void SetHeader(DSHeader Header)
        {
            this.Header = Header;
        }

        public void Save(string Filename)
        {
            using (var memoryStream = new MemoryStream())
            {
                using (var A = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                { 
                    // Add info
                    WriteZip(A, "header", ReadZip(Archive, "header"));
                    WriteZip(A, "session", ReadZip(Archive, "session"));
                    WriteZip(A, "decoders", ReadZip(Archive, "decoders"));

                    // Add data
                    for (int probe = 0; probe < Header.Probes; probe++)
                    {
                        var Seq = GetSequence(probe);

                        SetProbeData(A, probe, Header.Blocks, Seq.Data);
                    }
                }

                using (var fileStream = new FileStream(Filename, FileMode.OpenOrCreate))
                {
                    memoryStream.Seek(0, SeekOrigin.Begin);
                    memoryStream.CopyTo(fileStream);

                    fileStream.Close();
                }
            }
        }

        private static void WriteZip(ZipArchive Archive, string Path, byte[] Data)
        {
            var Entry = Archive.CreateEntry(Path, CompressionLevel.Fastest);

            using (var Dst = Entry.Open())
            using (var Src = new MemoryStream(Data))
            {
                Src.CopyTo(Dst);
            }
        }

        private static byte[] ReadZip(ZipArchive Archive, string Path)
        {
            var Entry = Archive.GetEntry(Path);
            if (Entry != null)
            {
                using (var Src = Entry.Open())
                {
                    using (var Dst = new MemoryStream())
                    {
                        Src.CopyTo(Dst);

                        return Dst.ToArray();
                    }
                }
            }

            throw new IOException($"Entry {Path} is not found in archive!");
        }

        private static byte[] GetProbeData(ZipArchive Archive, int Index, int Blocks)
        {
            var Res = new List<byte>();
            // Entries L-<probe>/<Block>  (0x200000 bytes)
            for (int i = 0; i < Blocks; i++)
            {
                var Name = $"L-{Index}/{i}";

                Res.AddRange(ReadZip(Archive, Name));
            }
            return Res.ToArray();
        }

        private static void SetProbeData(ZipArchive Archive, int Index, int Blocks, byte[] Data)
        {
            var Res = new List<byte>();
            // Entries L-<probe>/<Block>  (0x200000 bytes)
            for (int i = 0; i < Blocks; i++)
            {
                var Name = $"L-{Index}/{i}";

                int Offset = i * 0x200000;
                int Size = Math.Min(Data.Length - Offset, 0x200000);

                byte[] Part = new byte[Size];
                Array.Copy(Data, Offset, Part, 0, Size);

                WriteZip(Archive, Name, Part);
            }
        }

        public static void Save(Measure M, string Filename)
        {
            var A = new ZipArchive(new FileStream(Filename, FileMode.OpenOrCreate, FileAccess.Write));

        }

        public static DSLogicMeasure Load(string Filename)
        {
            if (File.Exists(Filename))
            {
                var A = new System.IO.Compression.ZipArchive(new FileStream(Filename, FileMode.Open, FileAccess.Read));
                var Res = new DSLogicMeasure(A);

                // Header
                var Header = new DSHeader(Encoding.UTF8.GetString(ReadZip(A, "header")).Split('\n'));

                Res.SetHeader(Header);
                for(int probe = 0; probe < Header.Probes; probe++)
                {
                    var Name = Header.GetProbeName(probe);
                    var Data = GetProbeData(A, probe, Header.Blocks);

                    var Seq = new DSLogicSequence(Name, Data);

                    Res.Add(Seq);
                }

                return Res;
            }
            else
                throw new IOException($"File {Filename} is not found!");
        }

        private class DSHeader
        {
            Dictionary<string,string> Params = new Dictionary<string, string>();

            public DSHeader(string[] Lines)
            {
                string Block = "unspec";
                foreach (var L in Lines)
                {
                    if (L.Length == 0)
                        continue;

                    if(L.StartsWith("["))
                        Block = L.Substring(1, L.Length - 2);
                    else
                    {
                        var Parts = L.Split('=');
                        var Name = Parts[0].Trim();
                        var Value = Parts[1].Trim();

                        Params[$"{Block}.{Name}"] = Value;
                    }
                }
            }

            public int Version => Convert.ToInt32(Params["version.version"]);

            public int Probes => Convert.ToInt32(Params["header.total probes"]);
            public int Blocks => Convert.ToInt32(Params["header.total blocks"]);
            public long Samples => Convert.ToInt32(Params["header.total samples"]);
            public string SampleRate => Params["header.samplerate"];

            public string GetProbeName(int Index)
            {
                var Name = $"header.probe{Index}";
                if (Params.ContainsKey(Name))
                    return Params[Name];
                else
                    throw new KeyNotFoundException($"No probe with index {Index}");
            }
        }
    }
}
