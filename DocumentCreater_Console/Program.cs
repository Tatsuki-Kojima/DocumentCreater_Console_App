using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocumentCreater_Console
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string projectDirPath = null;
            if (File.Exists("ProjectDirPath.txt"))
            {
                using (var sr = new StreamReader("ProjectDirPath.txt"))
                {
                    projectDirPath = sr.ReadToEnd();
                }
            }

            var dirPath = Environment.CurrentDirectory;

            var targetFilePath = Path.Combine(dirPath, "src", "test_typescript.ts");

            Console.WriteLine($"read file path: {targetFilePath} ");

            string targetFileString = null;

            if (File.Exists(targetFilePath))
            {
                using (var sr = new StreamReader(targetFilePath))
                {
                    targetFileString = sr.ReadToEnd();
                }
            }

            Console.WriteLine("TargetFile's Contents");
            Console.WriteLine($"{targetFileString ?? ""}");

            var reader = new DocumentReader(targetFileString);

            Console.WriteLine("Import Contains List");
            foreach (var item in reader.Decode())
            {
                Console.WriteLine($"import {item.Type}, from {item.From}");
            }

            Console.WriteLine("Press Enter, This program is end.");
            Console.ReadLine();
        }
    }

    public class DocumentReader
    {
        private readonly string TargetText;

        public DocumentReader(string targetText)
        {
            TargetText = targetText;
        }

        public IReadOnlyCollection<ImportDecoder.ImportType> Decode()
        {
            var impprter = new ImportDecoder(TargetText);

            return impprter.Decode();
        }
    }

    // importに関してを読み解く
    public class ImportDecoder
    {
        private readonly string TargetText;

        public ImportDecoder(string targetText)
        {
            TargetText = targetText;
        }

        public IReadOnlyCollection<ImportType> Decode()
        {
            var result = new List<ImportType>();

            foreach (var text in TargetText.Split('\n'))
            {
                if (!text.Contains(ImportTags.ImportTag))
                    continue;

                var texts = text.Split(';')
                    .Where(_ => !_.Contains('\r'))
                    .ToArray();

                // 複数処理存在する
                if (texts.Length > 1)
                    throw new NotImplementedException(text.Split(';').Last()); // 未実装

                var s = texts[0];

                var inBlock = s.Substring(s.IndexOf('{') + 1, s.IndexOf('}') - s.IndexOf('{') - 1)
                    .Replace(" ", "");

                var inFrom = s.Substring(s.IndexOf('\"') + 1, s.LastIndexOf('\"') - s.IndexOf('\"') - 1);



                foreach (var type in inBlock.Split(','))
                {
                    result.Add(new ImportType(type, inFrom));
                }
            }

            return result;
        }

        private class ImportTags
        {
            public const string ImportTag = "import";
            public const string FromTag = "from";
        }

        public class ImportType
        {
            public readonly string Type;
            public readonly string From;

            public ImportType(string type, string from)
            {
                Type = type;
                From = from;
            }
        }
    }
}
