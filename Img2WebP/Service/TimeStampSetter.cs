namespace Png2WebP.Service
{
    internal static class TimeStampSetter
    {
        /// <summary>
        /// ファイルのタイムスタンプを統一する
        /// </summary>
        /// <param name="srcFile">ソース元ファイル</param>
        /// <param name="destFile">作成後ファイル</param>
        public static void SetTimeStamp(string srcFile, string destFile)
        {
            FileInfo srcFileInfo = new FileInfo(srcFile);
            FileInfo destFileInfo = new FileInfo(destFile);

            // WebPファイルのタイムスタンプを元のpngの情報と置き換える
            destFileInfo.CreationTime = srcFileInfo.CreationTime;
            destFileInfo.LastAccessTime = srcFileInfo.LastAccessTime;
            destFileInfo.LastWriteTime = srcFileInfo.LastWriteTime;
        }
    }
}
