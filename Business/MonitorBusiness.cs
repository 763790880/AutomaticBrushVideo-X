using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

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
        public static void FindSubmitButton(ChromeDriver web)
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
      
    }
}
