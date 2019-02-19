using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Android;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace AliExpressAutomationTools
{
    [TestClass]
    public class AliExpressMobileChargePayment
    {
        private string telephoneNumber = "9001234567";

        [TestMethod]
        public void MakeMobileChargePayment()
        {
            AppiumDriver<AppiumWebElement> driver;

            DesiredCapabilities capabilities = new DesiredCapabilities();
            capabilities.SetCapability("deviceName", "Test");
            capabilities.SetCapability("platformVersion", "4.4.4");
            capabilities.SetCapability("PlatformName", "Android");

            capabilities.SetCapability("appPackage", "com.alibaba.aliexpresshd");
            capabilities.SetCapability("appActivity", ".home.ui.MainActivity");

            capabilities.SetCapability("noReset", "true");

            driver = new AndroidDriver<AppiumWebElement>(new Uri("http://127.0.0.1:4723/wd/hub"), capabilities);

            //click Profile
            driver.FindElement(By.Id("com.alibaba.aliexpresshd:id/navigation_my_ae")).Click();
            //click Following
            driver.FindElement(By.Id("com.alibaba.aliexpresshd:id/rl_following")).Click();

            WebDriverWait waiter = new WebDriverWait(driver, new TimeSpan(0, 0, 20));

            //get all favourite shop, find element with text, then click
            var favouriteShops = waiter.Until(ExpectedConditions.VisibilityOfAllElementsLocatedBy(By.Id("com.alibaba.aliexpresshd:id/follow_item")));
            
            var ruChargeShop = favouriteShops.Where(shop => ((AppiumWebElement)shop).FindElementByClassName("android.widget.TextView").Text == "RU recharge Store").Single();
            ruChargeShop.Click();

            //click on picture in shop
            driver.FindElementById("com.alibaba.aliexpresshd:id/iv_photo").Click();

            for (int i = 0; i < 1; i++) //repeat
            {
                //get all available charge value
                var chargeValue = driver.FindElementsById("com.alibaba.aliexpresshd:id/ll_phone_recharge_product_item_container");

                //show their in console
                foreach (var chargeVal in chargeValue)
                {
                    Console.WriteLine("Find charge value - " + chargeVal.FindElementByClassName("android.widget.TextView").Text);
                }

                //select last value
                chargeValue.Last().Click();

                //find input field and send telephone number
                var inputField = driver.FindElementById("com.alibaba.aliexpresshd:id/pniv_phone_number");
                inputField.Click();
                inputField.SendKeys(this.telephoneNumber);

                Console.WriteLine("Input telephone number - " + driver.FindElementById("com.alibaba.aliexpresshd:id/pniv_phone_number").Text);

                //click on Recharge button
                driver.FindElementById("com.alibaba.aliexpresshd:id/bt_place_order").Click();

                //verify payment method and amount
                var selectedPaymentMethon = driver.FindElementById("com.alibaba.aliexpresshd:id/tv_selected_payment_method_text").Text;
                Console.WriteLine("select payment method - " + selectedPaymentMethon);

                var paymentAmount = driver.FindElementById("com.alibaba.aliexpresshd:id/tv_payment_order_total_amount").Text;
                Console.WriteLine("Amount for payment - " + paymentAmount);

                //click Confirm and Pay
                driver.FindElementById("com.alibaba.aliexpresshd:id/bt_confirm_and_pay").Click();
            }
        }
    }
}
