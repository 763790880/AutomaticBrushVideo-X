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
        private ObservableCollectionBusiness _observable;
        private ConcurrentDictionary<int, IWebDriver> webDrivers = new ConcurrentDictionary<int, IWebDriver>();
        private List<string> urls = new List<string>();
        private static bool _Auto = false;
        private static int _TaskCount = 8;//最大并行任务数
        public MainWindow()
        {
            InitializeComponent();
            // 初始化 ObservableCollection
            websiteList = new ObservableCollection<XStatus>();
            _observable = new ObservableCollectionBusiness(websiteList, webDrivers);
            // 将 ObservableCollection 绑定到 DataGrid
            this.dataGrid.ItemsSource = websiteList;

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
            _observable.AddStatus(driver, guid);//增加grid数据
            manualResetEvent.Set();

            while (b && webDrivers.ContainsKey(chromeDriverPort))
            {
                if (!b)
                    return;
                CreateNewTimer(driver);
                _observable.UpdateStatus(driver, guid, "未完成", url, ref b, ref whileCount);
                whileCount =whileCount+1;
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
