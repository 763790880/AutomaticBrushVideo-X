using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.DevTools;
using OpenQA.Selenium.DevTools.V120.Network;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Policy;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;

namespace X学堂
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ObservableCollection<XStatus> websiteList;
        private ConcurrentDictionary<int, IWebDriver> webDrivers = new ConcurrentDictionary<int, IWebDriver>();
        private List<string> urls = new List<string>();
        private static bool _Auto = false;
        private static int _TaskCount = 8;//最大并行任务数
        public MainWindow()
        {
            InitializeComponent();
            // 初始化 ObservableCollection
            websiteList = new ObservableCollection<XStatus>();
            // 将 ObservableCollection 绑定到 DataGrid
            this.dataGrid.ItemsSource = websiteList;

        }
        /// <summary>
        /// 登录按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click(object sender, RoutedEventArgs e)
        {

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
        private void CreateNewTimer(ChromeDriver web, string guid,string url, ref bool b,ref int whileCount)
        {
            Thread.Sleep(5000);
            UpdateStatus(web, guid, "未完成", url,ref b,ref whileCount);
            if(!b)
                return;
            // 查找续看按钮
            FindYesButton(web);
            // 查找确认按钮
            FindConfirmButton(web);
            // 查找提交按钮
            FindSubmitButton(web);
        }
        /// <summary>
        /// 查找签到确认
        /// </summary>
        /// <returns></returns>
        private void FindConfirmButton(ChromeDriver web)
        {
            try
            {
                // 使用 Selenium 提供的查找元素方法
                var confirmButton = web.FindElements(By.ClassName("dialog-footer-confirmed"));
                // 如果找到确认按钮，则模拟点击
                if (confirmButton != null)
                {
                    foreach (var item in confirmButton)
                    {
                        // 获取元素的 display 属性值
                        string displayValue = item.GetCssValue("display");
                        // 判断元素是否可见
                        bool isVisible = !string.IsNullOrEmpty(displayValue) && !displayValue.Equals("none", StringComparison.OrdinalIgnoreCase);
                        if (item.Enabled && isVisible)
                        {
                            try
                            {
                                item.Click();
                            }
                            catch (Exception)
                            {
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                // 没有找到按钮，可以添加一些处理逻辑或返回 null
            }
        }
        /// <summary>
        /// 查找续看按钮
        /// </summary>
        /// <returns></returns>
        private void FindYesButton(IWebDriver web)
        {
            try
            {
                By ele = By.CssSelector("button.vjs-big-play-button");
                var e = web.FindElement(ele);
                if (e != null)
                {
                    // 获取元素的 display 属性值
                    string displayValue = e.GetCssValue("display");
                    // 判断元素是否可见
                    bool isVisible = !string.IsNullOrEmpty(displayValue) && !displayValue.Equals("none", StringComparison.OrdinalIgnoreCase);
                    if (e.Enabled && isVisible)
                        e.Click();
                }
                //vjs-big-play-button
                By elementsLocator = By.CssSelector("div.course-btn.course-btn-yes");
                // 使用 Selenium 提供的查找元素方法
                var yesButton = web.FindElement(elementsLocator);
                if (yesButton != null)
                {
                    yesButton.Click();
                }
            }
            catch (Exception ex)
            {
                // 没有找到按钮，可以添加一些处理逻辑或返回 null
            }
        }
        /// <summary>
        /// 查找提交确认
        /// </summary>
        /// <returns></returns>
        private void FindSubmitButton(ChromeDriver web)
        {
            try
            {
                // 使用 Selenium 提供的查找元素方法
                var submitButton = web.FindElement(By.ClassName("close-btn"));
                // 如果找到确认按钮，则模拟点击
                if (submitButton != null)
                {
                    submitButton.Click();
                }
            }
            catch (Exception)
            {
                // 没有找到按钮，可以添加一些处理逻辑或返回 null
            }
        }
        /// <summary>
        /// 修改行状态
        /// </summary>
        /// <param name="url"></param>
        /// <param name="newStatus"></param>
        private void UpdateStatus(ChromeDriver driver, string guid, string newStatus, string url,ref bool b,ref int whileCount)
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
            if (Schedule.Contains("已学")||(whileCount > 3 && string.IsNullOrWhiteSpace(name)))
            {
                b = false;
                webDrivers.TryRemove(int.Parse(guid), out _);
                DeleteStatus(guid);
                RWFile.SuccesTask(RWFile.GetVal(url));
            }
            // 根据网址找到关联行，并修改状态
            Application.Current.Dispatcher.Invoke(() =>
            {
                foreach (var item in websiteList)
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
        private void AddStatus(ChromeDriver driver, string guid)
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
                websiteList.Add(new XStatus { Url = name, Status = "未完成", Guid = guid, Schedule = Schedule });
                this.Url.Text = "";
                this.Url.IsReadOnly = false;
                //MessageBox.Show("后台执行中！");
            });
        }
        private void DeleteStatus(string guid)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var model = websiteList.FirstOrDefault(f => f.Guid == guid);
                websiteList.Remove(model);
                //MessageBox.Show($"已完成课时《{model.Url}》");
            });
        }
        private void Login(string name, string pwd, string url, ManualResetEventSlim manualResetEvent)
        {

            var b = true;
            int whileCount = 0;//循环检测次数
            // 获取ChromeDriver的实际端口号
            int chromeDriverPort=0;
            var driver=ChromeHelp.Create(ref chromeDriverPort);
            var guid = chromeDriverPort.ToString();
            Application.Current.Dispatcher.Invoke(() =>
            {
                webDrivers.TryAdd(chromeDriverPort, driver);
            });
            //登录
            var isLogin=Loging(name,pwd,driver);
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
            AddStatus(driver, guid);//增加grid数据
            manualResetEvent.Set();

            while (b && webDrivers.ContainsKey(chromeDriverPort))
            {
                CreateNewTimer(driver, guid, url, ref b,ref whileCount);
                whileCount=whileCount+1;
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
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {

        }
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
                IWebElement usernameInput = wait.Until(driver => driver.FindElement(By.XPath("//input[@placeholder='账号']")));
                usernameInput.SendKeys(name);
            }
            catch (WebDriverTimeoutException ex)
            {
                RWFile.LogTask("我超时了");
                // 在超时时处理异常
                return false;
            }
            // 输入账号
            

            // 输入密码
            IWebElement passwordInput = driver.FindElement(By.XPath("//input[@placeholder='密码']"));
            passwordInput.SendKeys(pwd);


            // 找到登录按钮并点击
            IWebElement loginButton = driver.FindElement(By.ClassName("loginProBtn"));
            loginButton.Click();
            return true;
            #endregion
        }
        public void GetHttp(ChromeDriver driver)
        {
            var devTools = driver as IDevTools;
            var session = devTools.GetDevToolsSession();
            // 启用网络日志
            session.SendCommand(new EnableCommandSettings() {
                MaxTotalBufferSize = 10000000,
                MaxResourceBufferSize = 5000000,
                MaxPostDataSize= 5000000,
            });

            var lastend = new string[] {".js",".png",".jpg" };
            // 订阅网络请求事件
            session.DevToolsEventReceived += (a, response) =>
            {
                var link="https://api.moxueyuan.com/appapi.php/index?r=/apiCourse/";
                var resp= response.EventData?.SelectToken("response");
                var _type = response.EventData?.SelectToken("type");
                var _url=resp?.SelectToken("url");
                if (_type!=null&& _type.ToString()== "XHR" && _url != null&& !lastend.Contains(_url.ToString().Substring(_url.ToString().Length-3)))
                {
                    Console.WriteLine(  1);
                }
                if (_url!=null&&_url.Contains(link))
                {
                    MessageBox.Show(response.EventData.ToString());
                }
                
            };
            session.LogMessage += (a, response) =>
            {
                Console.WriteLine(response.Message);
            };
            // 打开网页
            driver.Navigate().GoToUrl("https://bjnxhl.study.moxueyuan.com/new/course?id=0&level=0&page=2");
        }
        /// <summary>
        /// 我觉得电脑行，在加一个任务
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            _TaskCount = _TaskCount+1;
            MessageBox.Show($"现在并行最大数：{_TaskCount}");
        }
        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            _TaskCount = _TaskCount - 1;
            MessageBox.Show($"现在并行最大数：{_TaskCount}");
        }
        private void Window_Closed(object sender, EventArgs e)
        {
            foreach (var driver in webDrivers)
            {
                try
                {
                    driver.Value.Quit();
                }
                catch (Exception)
                {
                    MessageBox.Show("关闭失败了");
                }

            }
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
                urls= RWFile.GetAllTask();
                urls = urls.Distinct().ToList();
                ///判断是否已经
                WhileTask(name,pwd);
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
            if (!string.IsNullOrWhiteSpace(guid)&&int.TryParse(guid ,out int chromeDriverPort))
            {
                if (webDrivers.TryGetValue(chromeDriverPort, out IWebDriver web))
                {
                    web.Quit();
                    web.Dispose();
                    webDrivers.TryRemove(chromeDriverPort, out _);
                    var model=websiteList.First(f => f.Guid == guid);
                    websiteList.Remove(model);
                }
            }
            MessageBox.Show("删除成功");
            return;
        }

        

        private void Button_Click_Log(object sender, RoutedEventArgs e)
        {
            var str = $@"当前最大任务数{_TaskCount}{Environment.NewLine}当前Grid数量{websiteList.Count()}{Environment.NewLine}
                            当前浏览器内核数量{webDrivers.Count}{Environment.NewLine}";
            MessageBox.Show(str);
        }

        private void WhileTask(string name,string pwd)
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
                    WhileTask(name,pwd);
                }
               
            }
        }
    }
}
