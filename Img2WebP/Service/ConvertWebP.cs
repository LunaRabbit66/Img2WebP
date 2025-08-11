using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp;

namespace Png2WebP.Service
{
    internal static class ConvertWebP
    {
        // ImageSharpの排他制御用ロック
        private static readonly object imageSharpLock = new();

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

                    using (Image image = Image.Load(pngFilePath))
                    {
                        var encoder = new WebpEncoder
                        {
                            Quality = 75, // 圧縮品質（0～100）
                            FileFormat = WebpFileFormatType.Lossy // または Lossless
                        };

                        image.Save(webpFilePath, encoder);
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
