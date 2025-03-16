using Microsoft.Extensions.Configuration;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

namespace scraping_anki;

class Program
{
    private static IConfiguration? Configuration { get; set; }
    private static string LastContentHashFile => Path.Combine(Directory.GetCurrentDirectory(), "last_content_hash.txt");
    private static string? EnglishFilePath => Configuration["FilePaths:EnglishFile"];

    static async Task Main(string[] args)
    {
        try
        {
            Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), "logs"));

            Logger.LogToFile("======================================================");
            Logger.LogToFile($"Anki Card Scheduler Started at {DateTime.Now}");
            Logger.LogToFile($"Current directory: {Directory.GetCurrentDirectory()}");

            Configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("secrets.json", optional: false, reloadOnChange: true)
                .Build();

            Logger.LogToFile("Configuration loaded successfully");
            Logger.LogToFile("Scheduled to run daily at 20:30");

            while (true)
            {
                var now = DateTime.Now;
                var scheduledTime = new DateTime(now.Year, now.Month, now.Day, 22, 43, 0);
                if (now > scheduledTime)
                {
                    scheduledTime = scheduledTime.AddDays(1);
                }
                var timeUntilNextRun = scheduledTime - now;
                Logger.LogToFile($"Next run scheduled for: {scheduledTime}");
                Logger.LogToFile($"Waiting {timeUntilNextRun.Hours} hours, {timeUntilNextRun.Minutes} minutes, and {timeUntilNextRun.Seconds} seconds until next run");

                await Task.Delay(timeUntilNextRun);
                Logger.LogToFile($"It's {DateTime.Now.ToShortTimeString()} - Starting scheduled run");
                await RunAnkiScraper();

                await Task.Delay(TimeSpan.FromMinutes(1));
            }
        }
        catch (Exception ex)
        {
            Logger.LogToFile($"Critical error in main loop: {ex.Message}");
            Logger.LogToFile($"Stack trace: {ex.StackTrace}");
            throw;
        }
    }

    private static Task RunAnkiScraper()
    {
        var email = Configuration?["AnkiCredentials:Email"];
        var password = Configuration?["AnkiCredentials:Password"];
        try
        {
            string? englishFilePath = EnglishFilePath;

            if (string.IsNullOrEmpty(englishFilePath))
            {
                Logger.LogToFile("Error: EnglishFile path not found in secrets.json");
                return Task.CompletedTask;
            }
            Logger.LogToFile($"English file path: {englishFilePath}");

            if (!File.Exists(englishFilePath))
            {
                Logger.LogToFile($"Error: File not found at path: {englishFilePath}");
                return Task.CompletedTask;
            }

            if (!FileUtils.HasFileContentChanged(englishFilePath, LastContentHashFile))
            {
                Logger.LogToFile("Content of english.txt hasn't changed since last run. Skipping card addition.");
                return Task.CompletedTask;
            }

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                Logger.LogToFile("Error: Email or password not found in secrets.json");
                return Task.CompletedTask;
            }
            
            var cards = CardParser.ParseEnglishFile(englishFilePath);

            if (cards.Count == 0)
            {
                Logger.LogToFile("No cards to add. Exiting.");
                return Task.CompletedTask;
            }

            Logger.LogToFile("Starting Chrome WebDriver");
            var options = new ChromeOptions();
            options.AddArgument("--headless");

            using (IWebDriver driver = new ChromeDriver(options))
            {
                Logger.LogToFile("Navigating to Anki login page");
                driver.Navigate().GoToUrl("https://ankiweb.net/account/login");
                var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));

                Logger.LogToFile("Entering login credentials");
                var emailField = wait.Until(d => d.FindElement(By.CssSelector("input[autocomplete='username']")));
                emailField.SendKeys(email);
                var passwordField = wait.Until(d => d.FindElement(By.CssSelector("input[autocomplete='current-password']")));
                passwordField.SendKeys(password);

                Logger.LogToFile("Clicking login button");
                var loginButton = wait.Until(d => d.FindElement(By.CssSelector("button.btn.btn-primary.btn-lg")));
                loginButton.Click();
                Thread.Sleep(5000);

                Logger.LogToFile("Navigating to add card page");
                driver.Navigate().GoToUrl("https://ankiweb.net/add");

                foreach (var (front, back) in cards)
                {
                    Logger.LogToFile($"Adding card: Front='{front}', Back='{back}'");
                    var frontField = wait.Until(d => d.FindElement(By.XPath("//span[text()='Front']/following-sibling::div//div[@class='form-control field']")));
                    frontField.Clear();
                    frontField.SendKeys(front);
                    var backField = wait.Until(d => d.FindElement(By.XPath("//span[text()='Back']/following-sibling::div//div[@class='form-control field']")));
                    backField.Clear();
                    backField.SendKeys(back);
                    var addButton = wait.Until(d => d.FindElement(By.CssSelector("button.btn.btn-primary.btn-large.mt-2")));
                    addButton.Click();
                    Thread.Sleep(2000);
                }
                Logger.LogToFile("Finished adding cards. Closing browser.");
                driver.Quit();
            }

            FileUtils.SaveContentHash(englishFilePath, LastContentHashFile);
            Logger.LogToFile("Updated content hash for next comparison.");

            File.WriteAllText(englishFilePath, string.Empty);
            Logger.LogToFile("Cleared the content of the English file.");
        }
        catch (Exception ex)
        {
            Logger.LogToFile($"Error during scraping: {ex.Message}");
            Logger.LogToFile($"Stack trace: {ex.StackTrace}");
        }

        return Task.CompletedTask;
    }
}