using Newtonsoft.Json.Linq;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using Polly;
using Polly.Retry;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;

namespace X学堂
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ObservableCollection<XStatus> websiteList;
        private RetryPolicy _policy;
        private Helper _hrrpHelper;
        private ObservableCollectionBusiness _observable;
        private ConcurrentDictionary<int, IWebDriver> webDrivers = new ConcurrentDictionary<int, IWebDriver>();
        private List<string> urls = new List<string>();
        private static bool _Auto = false;
        public static bool ButtonIsEnbled=true;
        private static int _TaskCount = 8;//最大并行任务数
        public MainWindow()
        {
            InitializeComponent();
            // 初始化 ObservableCollection
            websiteList = new ObservableCollection<XStatus>();
            _observable = new ObservableCollectionBusiness(websiteList, webDrivers);
            // 将 ObservableCollection 绑定到 DataGrid
            this.dataGrid.ItemsSource = websiteList;
            _policy = Policy
                        .Handle<Exception>()
                        .WaitAndRetry(3, retryAttempt => TimeSpan.FromSeconds(2), (exception, timeSpan, retryCount, context) =>
                        {
                            RWFile.LogTask($"重试失败:{exception.Message}");
                        });
            _hrrpHelper = new Helper();
            this.Closing += Window_Closed;
        }

        /// <summary>
        /// 开刷按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            this.Url.IsReadOnly = true;
            var name = this.UserName.Text;
            var pwd = this.Pwd.Password;
            var url = this.Url.Text;
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(pwd) || string.IsNullOrWhiteSpace(url))
            {
                MessageBox.Show($"用户名-密码-Url 都不能为空！");
                return;
            }
            if (!Constant.TopUpUser.Contains(name) && websiteList.Count > 5)
            {
                MessageBox.Show("未充值账户，已达刷课上限");
                return;
            }

            ManualResetEventSlim manualResetEvent = new ManualResetEventSlim(false);
            Task.Run(() =>
            {
                // 在后台线程执行登录任务
                Login(name, pwd, url, manualResetEvent);
            });
        }

        #region 私有的
        /// <summary>
        /// 监听相关按钮
        /// </summary>
        /// <param name="web"></param>
        /// <param name="guid"></param>
        /// <param name="b"></param>
        private void CreateNewTimer(ChromeDriver web)
        {
            Thread.Sleep(5000);
            // 查找续看按钮
            MonitorBusiness.FindYesButton(web);
            // 查找确认按钮
            MonitorBusiness.FindConfirmButton(web);
            // 查找提交按钮
            MonitorBusiness.FindSubmitButton(web);
        }

        private void Login(string name, string pwd, string url, ManualResetEventSlim manualResetEvent)
        {

            var b = true;
            int whileCount = 0;//循环检测次数
            // 获取ChromeDriver的实际端口号
            int chromeDriverPort = 0;
            var driver = ChromeHelp.Create(ref chromeDriverPort);
            var guid = chromeDriverPort.ToString();
            Application.Current.Dispatcher.Invoke(() =>
            {
                webDrivers.TryAdd(chromeDriverPort, driver);
            });
            //登录
            var isLogin = Loging(name, pwd, driver);
            if (!isLogin)
            {
                driver.Quit();
                driver.Dispose();
                return;
            }

            // 等待一段时间确保登录成功（你可能需要调整等待时间）
            Thread.Sleep(1000);
            //GetHttp(driver);
            // 登录成功后继续导航到新的页面
            driver.Navigate().GoToUrl(url);
            _observable.AddStatus(driver, guid);//增加grid数据
            manualResetEvent.Set();

            while (b && webDrivers.ContainsKey(chromeDriverPort))
            {
                if (!b)
                    return;
                CreateNewTimer(driver);
                _observable.UpdateStatus(driver, guid, "未完成", url, ref b, ref whileCount);
                whileCount = whileCount + 1;
            }
            Application.Current.Dispatcher.Invoke(() =>
            {
                webDrivers.TryRemove(chromeDriverPort, out _);
            });
            try
            {
                driver.Quit();
                driver.Dispose();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }
        #endregion
        #region 没点用
        private bool Loging(string name, string pwd, ChromeDriver driver)
        {
            #region 登录
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(15));
            // 导航到登录页面
            driver.Navigate().GoToUrl("https://bjnxhl.study.moxueyuan.com/new/login");
            Thread.Sleep(2000);
            // 等待一段时间确保页面加载完成（你可能需要调整等待时间）
            try
            {
                _policy.Execute(() =>
                {
                    IWebElement usernameInput = wait.Until(driver => driver.FindElement(By.XPath("//input[@placeholder='账号']")));
                    usernameInput.SendKeys(name);
                });

            }
            catch (WebDriverTimeoutException ex)
            {
                MessageBox.Show(ex.Message);
            }

            IWebElement passwordInput = driver.FindElement(By.XPath("//input[@placeholder='密码']"));
            passwordInput.SendKeys(pwd);


            IWebElement loginButton = driver.FindElement(By.ClassName("loginProBtn"));
            loginButton.Click();
            return true;
            #endregion
        }
        /// <summary>
        /// 我觉得电脑行，在加一个任务
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            _TaskCount = _TaskCount + 1;
            MessageBox.Show($"现在并行最大数：{_TaskCount}");
        }
        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            _TaskCount = _TaskCount - 1;
            MessageBox.Show($"现在并行最大数：{_TaskCount}");
        }
        private void Window_Closed(object sender, System.ComponentModel.CancelEventArgs e)
        {
            foreach (var driver in webDrivers)
            {
                try
                {
                    driver.Value.Quit();
                    driver.Value.Dispose();
                }
                catch (Exception)
                {
                    MessageBox.Show("关闭失败了");
                }
            }
            MessageBox.Show("关闭");
        }
        #endregion
        /// <summary>
        /// 自动刷非培训任务
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            var name = this.UserName.Text;
            if (!Constant.TopUpUser.Contains(name))
            {
                MessageBox.Show($"未充值用户，不能使用此功能");
                return;
            }
            if (_Auto)
            {
                MessageBox.Show($"以开启自动刷课时,还剩课程“{urls.Count - 1}”节");
                return;
            }
            var pwd = this.Pwd.Password;
            Task.Run(() =>
            {
                urls = RWFile.GetAllTask();
                urls = urls.Distinct().ToList();
                ///判断是否已经
                WhileTask(name, pwd);
            });
            _Auto = true;
        }
        /// <summary>
        /// 删除行任务
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Shanchu(object sender, RoutedEventArgs e)
        {
            var person = (XStatus)((Button)sender).DataContext;
            var guid = person?.Guid;
            if (!string.IsNullOrWhiteSpace(guid) && int.TryParse(guid, out int chromeDriverPort))
            {
                if (webDrivers.TryGetValue(chromeDriverPort, out IWebDriver web))
                {
                    web.Quit();
                    web.Dispose();
                    webDrivers.TryRemove(chromeDriverPort, out _);
                    var model = websiteList.First(f => f.Guid == guid);
                    websiteList.Remove(model);
                    MessageBox.Show("删除成功");
                    return;
                }
            }
            MessageBox.Show("删除失败!!!!");

        }



        private void Button_Click_Log(object sender, RoutedEventArgs e)
        {
            var str = $@"当前最大任务数{_TaskCount}{Environment.NewLine}当前Grid数量{websiteList.Count()}{Environment.NewLine}
                            当前浏览器内核数量{webDrivers.Count}{Environment.NewLine}";
            MessageBox.Show(str);
        }

        private void WhileTask(string name, string pwd)
        {
            ManualResetEventSlim manualResetEvent = new ManualResetEventSlim(false);
            while (urls.Count > 1)
            {
                try
                {
                    var url = urls.First();
                    if (websiteList.Count < _TaskCount)
                    {
                        Task.Run(() =>
                        {
                            try
                            {
                                // 在后台线程执行登录任务
                                Login(name, pwd, url, manualResetEvent);
                            }
                            catch (Exception ex)
                            {
                                RWFile.LogTask("自定义日志" + ex.Message);
                            }

                        });
                        manualResetEvent.Wait();
                        manualResetEvent.Reset();
                        urls.Remove(url);
                    }
                    else
                        Thread.Sleep(8000);
                }
                catch (Exception ex)
                {
                    RWFile.LogTask("引发终止的错误" + ex.Message);
                    WhileTask(name, pwd);
                }

            }
        }
        /// <summary>
        /// 读取所有未完成与进行中的任务
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ReadBack(object sender, RoutedEventArgs e)
        {
            int chromeDriverPort = 0;
            ButtonIsEnbled=false;
            var driver = ChromeHelp.Create(ref chromeDriverPort);
            //登录
            var name = this.UserName.Text;
            var pwd = this.Pwd.Password;
            var isLogin = Loging(name, pwd, driver);
            var cookies = driver.Manage().Cookies.AllCookies;
            var httpClient = _hrrpHelper.CreateHttpClient();
            foreach (var cookie in cookies)
            {
                httpClient.DefaultRequestHeaders.Add(cookie.Name, cookie.Value);
                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Cookie", $"{cookie.Name}={cookie.Value}");
            }
            string url = "https://api.moxueyuan.com/appapi.php/index?r=/apiCourse/byCategoryCache&eid=25415&token=b2f8b9f38f8f6dda9833c7821bde81c0&userid=0883cb78_20f0_2c6e_a9d1_55ea27e5fcbe&version=6.0.0&platform=pcweb&language=zh_CN&userPortalID=389&userNikkatsu=232&requestTimestamp=1737440104312&_requestPageUrl=https%253A%252F%252Fbjnxhl.study.moxueyuan.com%252Fnew%252Fcourse%253Fid%253D0%2526level%253D0%2526state%253D0%252C1&page=1&pagesize=32000&sort=top_time-desc&search[studyState]=0,1&search[ischarge]=&search[mediatype]=&search[catid]=0&search[label]=&type=";
            HttpResponseMessage response =  httpClient.GetAsync(url).Result;
            response.EnsureSuccessStatusCode();
            string jsonResponse = response.Content.ReadAsStringAsync().Result;
            JObject obj = JObject.Parse(jsonResponse);
            var arrystr = obj.SelectToken("data").ToString();
            var ids = JArray.Parse(arrystr).Select(f=>f.SelectToken("id").ToString()).ToList();
            var str=string.Join(",", ids);
            RWFile.Reset(str);
            driver.Quit();
            driver.Dispose();
            MessageBox.Show("已重置所有课程!");
            ButtonIsEnbled = true;
        }

        private void ScreenShot(object sender, RoutedEventArgs e)
        {
            var person = (XStatus)((Button)sender).DataContext;
            var guid = person?.Guid;
            if (!string.IsNullOrWhiteSpace(guid) && int.TryParse(guid, out int chromeDriverPort))
            {
                if (webDrivers.TryGetValue(chromeDriverPort, out IWebDriver driver))
                {
                    if (driver is ITakesScreenshot screenshotDriver)
                    {
                        driver.Manage().Window.Maximize();
                        Screenshot screenshot = screenshotDriver.GetScreenshot();
                        // 将截图保存为文件
                        string filePath = $"File/截图/{guid}.png";
                        screenshot.SaveAsFile(filePath);
                        MessageBox.Show($"截图已保存到: {filePath}");
                    }
                    return;
                }
            }
        }
    }
}
