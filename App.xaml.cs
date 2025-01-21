using Microsoft.Extensions.DependencyInjection;
using System;
using System.Windows;

namespace X学堂
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private IServiceProvider _serviceProvider;
        protected override void OnStartup(StartupEventArgs e)
        {
         
            var services = new ServiceCollection();

            services.AddSingleton<MainWindow>();
            services.AddSingleton<Helper>();
            _serviceProvider = services.BuildServiceProvider();
            base.OnStartup(e);
        }
    }
}
