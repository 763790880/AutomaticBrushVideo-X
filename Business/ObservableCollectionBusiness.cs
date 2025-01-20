using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace X学堂
{
    public class ObservableCollectionBusiness
    {
        private readonly ObservableCollection<XStatus> _websiteList;
        private readonly ConcurrentDictionary<int, IWebDriver> _webDrivers;
        public ObservableCollectionBusiness(ObservableCollection<XStatus> xStatuses, ConcurrentDictionary<int, IWebDriver> keyValues)
        {
            _websiteList=xStatuses;
            _webDrivers=keyValues;
        }
        /// <summary>
        /// 修改行状态
        /// </summary>
        /// <param name="url"></param>
        /// <param name="newStatus"></param>
        public void UpdateStatus(ChromeDriver driver, string guid, string newStatus, string url, ref bool b, ref int whileCount)
        {
            var Schedule = "";
            var name = "";
            try
            {
                IWebElement element2 = driver.FindElement(By.CssSelector("div.course_list_progress.flex-1.dis-flex.dis-col"));
                if (element2 != null)
                    Schedule = element2.Text.Substring(0, 10);
                IWebElement element1 = driver.FindElement(By.CssSelector("span.c666.f14.last-span"));
                if (element1 != null)
                    name = element1.Text;
            }
            catch (Exception)
            {
            }
            if (Schedule.Contains("已学") || (whileCount > 3 && string.IsNullOrWhiteSpace(name)))
            {
                b = false;
                _webDrivers.TryRemove(int.Parse(guid), out _);
                DeleteStatus(guid);
                RWFile.SuccesTask(RWFile.GetVal(url));
            }
            // 根据网址找到关联行，并修改状态
            Application.Current.Dispatcher.Invoke(() =>
            {
                foreach (var item in _websiteList)
                {
                    if (item.Guid == guid)
                    {
                        if (!string.IsNullOrWhiteSpace(newStatus))
                            item.Status = newStatus;
                        if (!string.IsNullOrWhiteSpace(Schedule))
                            item.Schedule = Schedule;
                        if (!string.IsNullOrWhiteSpace(name))
                            item.Url = name;
                    }
                }
            });

        }
        public void AddStatus(ChromeDriver driver, string guid)
        {
            var name = "";
            var Schedule = "";
            try
            {
                IWebElement element1 = driver.FindElement(By.CssSelector("span.c666.f14.last-span"));
                if (element1 != null)
                    name = element1.Text;
                IWebElement element2 = driver.FindElement(By.CssSelector("div.course_list_progress.flex-1.dis-flex.dis-col"));
                if (element2 != null)
                    Schedule = element2.Text;
            }
            catch (Exception)
            {
                name = "";
                Schedule = "";
            }

            Application.Current.Dispatcher.Invoke(() =>
            {
                _websiteList.Add(new XStatus { Url = name, Status = "未完成", Guid = guid, Schedule = Schedule });
                //this.Url.Text = "";
                //this.Url.IsReadOnly = false;
                //MessageBox.Show("后台执行中！");
            });
        }
        public void DeleteStatus(string guid)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var model = _websiteList.FirstOrDefault(f => f.Guid == guid);
                _websiteList.Remove(model);
                //MessageBox.Show($"已完成课时《{model.Url}》");
            });
        }
    }
}
