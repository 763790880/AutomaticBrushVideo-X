using OpenQA.Selenium.Chrome;

namespace X学堂.Static
{
    public class MyChromeDriver: ChromeDriver
    {
        public MyChromeDriver(ChromeDriverService service, ChromeOptions options):base(service, options) {
        
        }
        public bool IsColse=false;
        protected override void Dispose(bool disposing=true)
        {
            IsColse = true;
            base.Dispose(disposing);
        }
    }
}
