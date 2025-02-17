﻿using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using X学堂.Static;

namespace X学堂
{
    public class ChromeHelp
    {
        public static MyChromeDriver Create(ref int port)
        {
            var chromeDriverService = ChromeDriverService.CreateDefaultService();
#if !DEBUG
            chromeDriverService.HideCommandPromptWindow = true;
#endif
            #region 启动浏览器调试

            var options = new ChromeOptions();
            // 启用 headless 模式
#if !DEBUG
            options.AddArgument("--headless");
#endif
            // 禁用 GPU 加速（某些系统上可能需要）
            //options.AddArgument("--disable-gpu");

            // 禁用 Chrome 的软件渲染

            //options.AddArgument("--disable-software-rasterizer");

            // 禁用沙盒模式
            //options.AddArgument("--no-sandbox");

            // 禁用浏览器正在被自动化程序控制的提示信息
            //options.AddArgument("--disable-infobars");

            // 不显示 GPU 进程的控制台日志
            options.AddArgument("--disable-logging");

            // 隐藏命令行窗口
            options.AddArgument("--disable-popup-blocking");
            // 禁用扩展
            //options.AddArgument("--disable-extensions");

            options.AddArgument("hide_console");
            //options.AddArgument("--hide");
#endregion
            var driver = new MyChromeDriver(chromeDriverService, options);
            port = chromeDriverService.Port;
            
            return driver;
        }
    }
}
