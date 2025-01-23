using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Linq;

namespace X学堂
{
    public class MonitorBusiness
    {
        /// <summary>
        /// 查找签到确认
        /// </summary>
        /// <returns></returns>
        public static void FindConfirmButton(ChromeDriver web)
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
        public static void FindYesButton(IWebDriver web)
        {
            try
            {
                By ele = By.CssSelector("button.vjs-big-play-button");
                var es = web.FindElements(ele);
                if (es.Count>0)
                {
                    var e=es.First();
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
                var yesButtons = web.FindElements(elementsLocator);
                if (yesButtons.Count>0)
                {
                    yesButtons.First().Click();
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
        public static void FindSubmitButton(ChromeDriver web)
        {
            try
            {
                // 使用 Selenium 提供的查找元素方法
                var submitButtons = web.FindElements(By.ClassName("close-btn"));
                // 如果找到确认按钮，则模拟点击
                if (submitButtons.Count>0)
                {
                    submitButtons.First().Click();
                }
            }
            catch (Exception)
            {
                // 没有找到按钮，可以添加一些处理逻辑或返回 null
            }
        }

        /// <summary>
        /// 如果视频被暂停
        /// </summary>
        /// <param name="web"></param>
        public static void ListenForClassAndClickVideo(ChromeDriver web)
        {
            // 执行 JavaScript 创建 MutationObserver 并开始监听
            ((IJavaScriptExecutor)web).ExecuteScript(@"
            (function() {
                var playerBox = document.getElementById('player-box');
                if (!playerBox) return;

                // 获取第一个子 div
                var targetNode = playerBox.querySelector('div:first-child');
                if (!targetNode) return;

                var observerOptions = {
                    attributes: true, // 监听属性变化
                    attributeFilter: ['class'] // 只监听 'class' 属性的变化
                };

                var callback = function(mutationsList, observer) {
                    for(var mutation of mutationsList) {
                        if (mutation.type === 'attributes' && (targetNode.classList.contains('vjs-ended')||targetNode.classList.contains('vjs-paused'))) {
                            console.log('vjs-ended class detected.');
                            
                            // 尝试点击 video 标签
                            var videoElement = document.querySelector('video');
                            if (videoElement) {
                                videoElement.click();
                                console.log('Clicked the video element.');
                            } else {
                                console.error('No video element found.');
                            }

                            observer.disconnect(); // 停止观察
                        }
                    }
                };

                var observer = new MutationObserver(callback);
                observer.observe(targetNode, observerOptions);

                // 检查是否已经结束（防止错过初始状态）
                if (targetNode.classList.contains('vjs-ended')) {
                    callback([{ type: 'attributes' }], observer);
                }
            })();
        ");
        }
        public static void Continue(ChromeDriver web)
        {
            try
            {
                IWebElement playerBox = null;
                var playerBoxs = web.FindElements(By.Id("player-box"));
                if (playerBoxs.Count > 0)
                {
                    playerBox= playerBoxs[0];
                    IWebElement firstChildDiv = playerBox.FindElement(By.CssSelector("div:first-child"));
                    // 如果初始状态下就有 vjs-ended 类，则直接点击视频
                    if (firstChildDiv.GetAttribute("class").Contains("vjs-ended") || firstChildDiv.GetAttribute("class").Contains("vjs-paused"))
                    {
                        IWebElement videoElement = web.FindElement(By.TagName("video"));
                        videoElement.Click();
                        Console.WriteLine("Initial state had vjs-ended class; clicked the video.");
                        return;
                    }
                }
                
            }
            catch (Exception ex)
            {
                return;
            }

        }
    }
}
