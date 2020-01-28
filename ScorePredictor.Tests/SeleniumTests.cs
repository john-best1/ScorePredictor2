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

        public SeleniumTests()
        {
            driver = new ChromeDriver("C:/Users/john.best/source/repos/ScorePredictor2/ScorePredictor.Tests/bin/Debug/netcoreapp2.1");
        }

        [Fact]
        public void NavBarLinkTitlesShouldBeCorrect()
        {
            driver.Navigate().GoToUrl("http://score-predictor.azurewebsites.net");

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
            driver.Navigate().GoToUrl("http://score-predictor.azurewebsites.net");

            var mainHeader = driver.FindElement(By.ClassName("matchtype")).Text;
            var datePlaceholder = driver.FindElement(By.Id("myDatepicker")).GetAttribute("placeholder");
            var expectedDate = (DateTime.Now.DayOfWeek.ToString() + "    " + DateTime.Now.Day + "/" + DateTime.Now.Month + "/" + DateTime.Now.Year);
            Assert.Equal("Fixtures", mainHeader);
            Assert.Equal(expectedDate, datePlaceholder);

            CleanUp();
        }


        internal void Initiliaize()
        {
            driver = new ChromeDriver("C:/Users/john.best/sour ce/repos/ScorePredictor2/ScorePredictor.Tests/bin/Debug/netcoreapp2.1");
        }

        //internal void EnterText(string element, string value, string elementType)
        //{
        //    if (elementType == "Id")
        //        driver.FindElement(By.Id(element));
        //    if (elementType == "Name")
        //        driver.FindElement(By.Name(element));
        //    if (elementType == "LinkText")
        //        driver.FindElement(By.LinkText(element));
        //}

        internal void CleanUp()
        {
            driver.Close();
        }


    }
}
