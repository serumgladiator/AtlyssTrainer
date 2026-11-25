using System;
using System.IO;
using BepInEx;

namespace AtlyssTrainer
{
    /// <summary>Copies the character profile folder to a timestamped backup directory.</summary>
    internal static class Backup
    {
        public static string Profiles()
        {
            string source = Path.Combine(Paths.GameRootPath, "ATLYSS_Data", "profileCollections");
            if (!Directory.Exists(source))
                return null;

            string folder = Path.Combine(Paths.GameRootPath, "BepInEx", "cheatmenu_backups",
                DateTime.Now.ToString("yyyyMMdd_HHmmss"));

            try
            {
                Directory.CreateDirectory(folder);
                CopyDirectory(source, folder);
                return folder;
            }
            catch (Exception e)
            {
                Plugin.Log.LogError("Backup: " + e);
                return null;
            }
        }

        private static void CopyDirectory(string source, string destination)
        {
            Directory.CreateDirectory(destination);

            string[] files = Directory.GetFiles(source);
            for (int i = 0; i < files.Length; i++)
            {
                string target = Path.Combine(destination, Path.GetFileName(files[i]));
                File.Copy(files[i], target, true);
            }

            string[] dirs = Directory.GetDirectories(source);
            for (int i = 0; i < dirs.Length; i++)
            {
                CopyDirectory(dirs[i], Path.Combine(destination, Path.GetFileName(dirs[i])));
            }
        }
    }
}
