using AventStack.ExtentReports.Reporter;
using AventStack.ExtentReports;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Google.Selenium_Tests.Pages
{
    internal class BasePage
    {
        protected IWebDriver Driver;
        protected ExtentReports Extent;
        protected ExtentSparkReporter SparkReporter;
        protected ExtentTest Test;

        private Dictionary<string, string> Properties;
        protected string? currdir;
        protected string url;

        //overloaded constructors
        protected BasePage()
        {
            currdir = Directory.GetParent(@"../../../")?.FullName;
        }
        public BasePage(IWebDriver driver)
        {
            Driver = driver;
        }

        public void ReadConfigSettings()
        {
            string currentDir = Directory.GetParent(@"../../../").FullName;

            string fullPath = currentDir + "/configsettings/config.properties";

            string[] lines = File.ReadAllLines(fullPath);
            Properties = new Dictionary<string, string>();
            foreach (string line in lines)
            {
                if (!string.IsNullOrEmpty(line) && line.Contains('='))
                {
                    string[] split = line.Split('=');
                    string key = split[0].Trim();
                    string value = split[1].Trim();
                    Properties[key] = value;
                }
            }

        }

        protected static void ScrollViewInto(IWebDriver driver, IWebElement element)
        {
            IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
            js.ExecuteScript("arguments[0].scrollIntoView(true)", element);
        }
        protected void TakeScreenShot(string testName)
        {
            ITakesScreenshot screenshot = (ITakesScreenshot)Driver;
            Screenshot ss = screenshot.GetScreenshot();
            string currdir = Directory.GetParent(@"../../../").FullName;
            string filepath = currdir + "/Screenshots/" + testName + "scs_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".png";
            ss.SaveAsFile(filepath);

        }
        protected void LogTestResult(string testName, string type, string result, string errorMessage = null)
        {
            if (type.ToLower().Equals("info"))
            {
                Log.Information(result);
                Test?.Info(result);
            }
            else if (type.ToLower().Equals("pass") && errorMessage == null)
            {
                Log.Information(testName + "passed");
                Log.Information(".....................................");
                Test?.Pass(result);
            }
            else
            {
                Log.Error($"Test failed for {testName} .\n Exception: \n {errorMessage}");
                Log.Information("........................................");
                var screenshot = ((ITakesScreenshot)Driver).GetScreenshot().AsBase64EncodedString;
                Test?.AddScreenCaptureFromBase64String(screenshot);
                Test?.Fail(result);

            }

        }
        protected void InitializeBrowser()
        {
            ReadConfigSettings();
            if (Properties["﻿browser"].ToLower() == "edge")
            {
                Driver = new EdgeDriver();
            }
            else if (Properties["﻿browser"].ToLower() == "chrome")
            {
                Driver = new ChromeDriver();
            }

            url = Properties["baseUrl"];
            Driver.Url = url;
            Driver.Manage().Window.Maximize();

        }

        [OneTimeSetUp]
        public void Setup()
        {
            InitializeBrowser();

            string directory = Directory.GetParent(@"../../../").FullName;
            string logfilepath = directory + "/Logs/log_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".txt";
            Log.Logger = new LoggerConfiguration().
                    WriteTo.Console().
                    WriteTo.File(logfilepath, rollingInterval: RollingInterval.Day).
                    CreateLogger();
            string currentDir = Directory.GetParent(@"../../../").FullName;
            Extent = new ExtentReports();
            SparkReporter = new ExtentSparkReporter(currentDir + "/Reports/report_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".html");
            Extent.AttachReporter(SparkReporter);
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            Extent.Flush();
            Driver.Quit();
            Log.CloseAndFlush();
        }
    }
}
