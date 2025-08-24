using CommandLine;
using Png2WebP.Service;

namespace Img2WebP
{
    internal class Program
    {
        // コンバート対象の画像拡張子を定義
        static readonly HashSet<string> allowExtension = new(StringComparer.OrdinalIgnoreCase)
        {
            ".png", ".jpg", ".jpeg", ".gif", ".bmp", ".tiff", ".heic"
        };

        private class Options
        {
            [Option("half-threads", Required = false, HelpText = "Use half of logical cores for parallel conversion.")]
            public bool HalfThreads { get; set; }

            public uint Quality { get; set; } = 75u;

            [Option('k', "keep-original", Required = false, HelpText = "Keep original image files.")]
            public bool KeepOriginal { get; set; }

            // 値以外の残りは位置引数的に受け取る
            [Value(0)]
            public IEnumerable<string>? Paths { get; set; }
        }

        [STAThread]
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.WriteLine($"Img2WebP");

            ParserResult<Options> result = Parser.Default.ParseArguments<Options>(args);
            if (result.Tag == ParserResultType.NotParsed)
            {
                return; // ヘルプ表示など
            }

            var opt = ((Parsed<Options>)result).Value;

            // quality クランプ
            uint quality = (uint)Math.Clamp(opt.Quality, 1, 100);
            bool deleteOriginal = !opt.KeepOriginal;

            // 引数でパス未指定の場合はOpenFileDialog
            var pathArgs = opt.Paths?.ToArray() ?? Array.Empty<string>();
            if (pathArgs.Length == 0)
            {
                using OpenFileDialog ofd = new()
                {
                    Filter = "Image Files|" + string.Join(";", allowExtension.Select(ext => "*" + ext)),
                    Multiselect = true,
                    Title = "Select image files to convert to WebP"
                };

                var dr = ofd.ShowDialog();
                if (dr != DialogResult.OK)
                {
                    return; // キャンセル
                }

                pathArgs = ofd.FileNames;
            }

            // ファイルとディレクトリを分けてリスト化
            var fileList = new List<string>();
            var dirList = new List<string>();

            foreach (string path in pathArgs)
            {
                Console.WriteLine($"[TargetPath] {path}");

                if (File.Exists(path) && allowExtension.Contains(Path.GetExtension(path), StringComparer.OrdinalIgnoreCase))
                {
                    fileList.Add(path);
                }
                else if (Directory.Exists(path))
                {
                    dirList.Add(path);
                }
                else
                {
                    Console.WriteLine($"[Error] Unsupported file: {path}");
                }
            }

            foreach (var dir in dirList)
            {
                var imageFiles = Directory.GetFiles(dir, "*.*", SearchOption.TopDirectoryOnly)
                    .Where(file => allowExtension.Contains(Path.GetExtension(file)))
                    .ToArray();
                fileList.AddRange(imageFiles);
            }

            // 重複排除
            fileList = fileList.Distinct().ToList();

            if (fileList.Count == 0)
            {
                Console.WriteLine("[Error] No Image files found.");
                return;
            }

            int logicalCores = Environment.ProcessorCount;
            int maxDegree = opt.HalfThreads ? Math.Max(1, logicalCores / 2) : logicalCores;

            Console.WriteLine($"[Process] Start: {DateTime.Now} MaxDegreeOfParallelism: {maxDegree} Quality: {quality} DeleteOriginal: {deleteOriginal}");

            var options = new ParallelOptions { MaxDegreeOfParallelism = maxDegree };
            Parallel.ForEach(fileList, options, file => ConvertWebP.ConvertToWebP(file, quality, deleteOriginal));

            Console.WriteLine($"[Process] End: {DateTime.Now}");
        }
    }
}
