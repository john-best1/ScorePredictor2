using ScorePredictor.Models;
using System;
using ScorePredictor.Data;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using Xunit;

namespace ScorePredictor.Tests
{
    public class SeleniumTests
    {
        IWebDriver driver;
        String url;

        public SeleniumTests()
        {
            driver = new ChromeDriver("C:/Users/john.best/source/repos/ScorePredictor2/ScorePredictor.Tests/bin/Debug/netcoreapp2.1");
            url = "https://score-predictor.azurewebsites.net/";
        }

        [Fact]
        public void NavBarLinkTitlesShouldBeCorrect()
        {
            driver.Navigate().GoToUrl(url);

            var h1 = driver.FindElement(By.ClassName("navbar-brand")).Text;
            var link1 = driver.FindElement(By.CssSelector("ul >li:nth-child(1)")).Text;
            var link2 = driver.FindElement(By.CssSelector("ul >li:nth-child(2)")).Text;
            var link3 = driver.FindElement(By.CssSelector("ul >li:nth-child(3)")).Text;

            Assert.Equal("ScorePredictor", h1);
            Assert.Equal("Fixtures", link1);
            Assert.Equal("Leagues", link2);
            Assert.Equal("Data", link3);

            CleanUp();
        }

        [Fact]
        public void HomePageConstantsShouldBeCorrect()
        {
            driver.Navigate().GoToUrl(url);

            var mainHeader = driver.FindElement(By.ClassName("matchtype")).Text;
            var datePlaceholder = driver.FindElement(By.Id("myDatepicker")).GetAttribute("placeholder");
            var expectedDate = (DateTime.Now.DayOfWeek.ToString() + "    " + DateTime.Now.Day + "/" + DateTime.Now.Month + "/" + DateTime.Now.Year);
            Assert.Equal("Fixtures", mainHeader);
            Assert.Equal(expectedDate, datePlaceholder);

            CleanUp();
        }

        [Fact]
        public void FixturesLinkAndH1LinkShouldAllBeSamePage()
        {
            driver.Navigate().GoToUrl(url);
            var element = driver.FindElement(By.ClassName("navbar-brand"));
            element.Click();
            Assert.Equal(url, driver.Url);

            element = driver.FindElement(By.CssSelector("ul >li:nth-child(1)"));
            element.Click();
            Assert.Equal(url, driver.Url);

            CleanUp();
        }

        [Fact]
        public void LeaguesLinkShouldOpenLeaguesPageOnPremierLeagueByDefault()
        {
            driver.Navigate().GoToUrl(url);
            var element = driver.FindElement(By.CssSelector("ul >li:nth-child(2)"));  // leagues link
            element.Click();
            string expectedUrl = url + "Leagues/Leagues";
            Assert.Equal(expectedUrl, driver.Url);

            var leagueTypeButton = driver.FindElement(By.Id("leagueType"));
            var leagueButton = driver.FindElement(By.Id("league"));

            Assert.Equal("Overall", leagueTypeButton.Text);
            Assert.Equal("Premier League", leagueButton.Text);

            var leagueHeading = driver.FindElement(By.ClassName("heading"));
            Assert.Equal("England - Premier League", leagueHeading.Text);

            CleanUp();
        }


        internal void Initiliaize()
        {
            driver = new ChromeDriver("C:/Users/john.best/sour ce/repos/ScorePredictor2/ScorePredictor.Tests/bin/Debug/netcoreapp2.1");
        }

        internal void CleanUp()
        {
            driver.Close();
        }


    }
}
