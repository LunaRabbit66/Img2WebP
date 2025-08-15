using ImageMagick;

namespace Png2WebP.Service
{
    internal static class ConvertWebP
    {
        /// <summary>
        /// 画像ファイルをWebPに変換
        /// </summary>
        /// <param name="pngFilePath">対象ファイルパス</param>
        public static void ConvertToWebP(string pngFilePath)
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
                        image.Quality = 75;

                        image.Write(webpFilePath);
                    }

                    // 作成したWebPファイルのタイムスタンプを元のPNGファイルと同じにする
                    TimeStampSetter.SetTimeStamp(pngFilePath, webpFilePath);

                    File.Delete(pngFilePath);

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
                    Console.WriteLine($"[Error] Failed to convert {Path.GetFileName(pngFilePath)} to WebP. Retry {retryCount}/{maxRetry}: {ex.Message}");
                    if (retryCount >= maxRetry)
                    {
                        Console.WriteLine($"[Error] Conversion of {Path.GetFileName(pngFilePath)} to WebP ultimately failed. The file will not be deleted.");
                    }
                    else
                    {
                        // Wait before retrying (e.g., 500ms)
                        Thread.Sleep(500);
                    }
                }
            }
        }
    }
}
