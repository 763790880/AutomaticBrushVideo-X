using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Shapes;

namespace X学堂
{
    public static class RWFile
    {

        private readonly static object _lock = new object();
        // 追加内容到文件
        private static void AppendToFile(string filePath, string content)
        {
            try
            {
                string basePath = AppDomain.CurrentDomain.BaseDirectory;
                string path = System.IO.Path.Combine(basePath, filePath);
                // 以追加模式打开文件
                using (StreamWriter writer = new StreamWriter(path, true))
                {
                    // 追加内容到文件
                    writer.WriteLine(content);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error appending to file: {ex.Message}");
            }
        }
        private static string ReadFromFile(string path)
        {
            string basePath = AppDomain.CurrentDomain.BaseDirectory;
            string filePath = System.IO.Path.Combine(basePath, path);
            string jsonContent = System.IO.File.ReadAllText(filePath);
            return jsonContent;
        }
        #region 任务记录
        public static void SuccesTask(string id)
        {
            try
            {
                string filePath = "已完成.txt";
                lock (_lock)
                {
                    RWFile.AppendToFile(filePath, $",{id}");
                }
            }
            catch (Exception)
            {

                throw;
            }

        }
        public static void PnProgressTask(string id)
        {
            string filePath = "进行中.txt";
            lock (_lock)
            {
                RWFile.AppendToFile(filePath, $",{id}");
            }
        }
        public static List<string> GetAllTask()
        {
            List<string> list = new List<string>();
            var strall = RWFile.ReadFromFile("File/所有课程ID.txt");
            var ppall = RWFile.ReadFromFile("File/进行中.txt");
            var sall = RWFile.ReadFromFile("File/已完成.txt");
            var stralls = strall.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            var ppalls = ppall.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            var salls = sall.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            ppalls.ForEach(x => stralls.Remove(x.Replace("\r\n", "")));
            salls.ForEach(x => stralls.Remove(x.Replace("\r\n", "")));
            foreach (var item in stralls.OrderBy(f => f))
            {
                list.Add($"https://bjnxhl.study.moxueyuan.com/new/course/{item}?isCoursePackage=0");
            }
            return list;
        }
        public static string GetVal(string url)
        {
            var val = url.Substring(url.LastIndexOf('/') + 1);
            return val.Substring(0, val.LastIndexOf('?'));
        }
        public static void LogTask(string msg)
        {
            string filePath = "EX.log";
            lock (_lock)
            {
                RWFile.AppendToFile(filePath, $"{msg}");
            }
        }
        #endregion
    }
}
