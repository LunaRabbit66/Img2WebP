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

        [STAThread]
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.WriteLine($"Img2WebP");

            // 引数が無い場合はOpenFileDialogを表示
            if (args.Length == 0)
            {
                Application.EnableVisualStyles();
                using OpenFileDialog ofd = new()
                {
                    Filter = "Image Files|*.png;*.jpg;*.jpeg;*.gif;*.bmp;*.tiff;*.heic",
                    Multiselect = true,
                    Title = "Select image files to convert to WebP"
                };

                var dr = ofd.ShowDialog();
                if (dr != DialogResult.OK)
                {
                    return; // キャンセル
                }

                args = ofd.FileNames;
            }

            // ファイルとディレクトリを分けてリスト化
            var fileList = new List<string>();
            var dirList = new List<string>();

            foreach (string path in args)
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

            Console.WriteLine($"[Process] Start: {DateTime.Now} MaxDegreeOfParallelism: {Environment.ProcessorCount}");

            // 並列処理(コア数は環境によって変動)
            var options = new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount };
            Parallel.ForEach(fileList, options, ConvertWebP.ConvertToWebP);

            Console.WriteLine($"[Process] End: {DateTime.Now}");
        }
    }
}
