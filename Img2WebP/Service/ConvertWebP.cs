using ImageMagick;

namespace Png2WebP.Service
{
    internal static class ConvertWebP
    {
        /// <summary>
        /// 画像ファイルをWebPに変換
        /// </summary>
        /// <param name="pngFilePath">対象ファイルパス</param>
        /// <param name="quality">画質(1-100)</param>
        /// <param name="deleteOriginal">元ファイルを削除するか</param>
        public static void ConvertToWebP(string pngFilePath, uint quality = 75, bool deleteOriginal = true)
        {
            const int maxRetry = 3;
            int retryCount = 0;
            bool success = false;

            while (retryCount < maxRetry && !success)
            {
                try
                {
                    string webpFilePath = Path.ChangeExtension(pngFilePath, ".webp");

                    using (var image = new MagickImage(pngFilePath))
                    {
                        image.Format = MagickFormat.WebP;
                        image.Quality = quality;

                        image.Write(webpFilePath);
                    }

                    // 作成したWebPファイルのタイムスタンプを元のPNGファイルと同じにする
                    TimeStampSetter.SetTimeStamp(pngFilePath, webpFilePath);

                    // コマンドライン引数で--keep-originalが指定された場合は元ファイルを削除しない
                    if (deleteOriginal)
                    {
                        File.Delete(pngFilePath);
                    }

                    Console.WriteLine($"[Converted] {webpFilePath}");
                    success = true;
                }
                catch (System.AccessViolationException ex)
                {
                    Environment.FailFast("[Fatal] Access violation while processing", ex);
                }
                catch (Exception ex)
                {
                    retryCount++;
                    Console.WriteLine($"[Error] Failed to convert {Path.GetFileName(pngFilePath)} to WebP (quality={quality}). Retry {retryCount}/{maxRetry}: {ex.Message}");
                    if (retryCount >= maxRetry)
                    {
                        Console.WriteLine($"[Error] Conversion of {Path.GetFileName(pngFilePath)} to WebP ultimately failed. The file will not be deleted.");
                    }
                    else
                    {
                        Thread.Sleep(500);
                    }
                }
            }
        }
    }
}
