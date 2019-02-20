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

using FluentAssert;
using System.Text.RegularExpressions;

namespace AliExpressAutomationTools
{
    [TestClass]
    public class AliExpressMobileChargePayment
    {
        private string telephoneNumber = "9001234567";

        //how much iteration
        private int repeat = 1;

        //if false - only create order
        private bool makePayment = false;

        private int valueForCharge = 100;

        [TestMethod]
        public void MakeMobileChargePayment()
        {
            AppiumDriver<AppiumWebElement> driver;

            DesiredCapabilities capabilities = new DesiredCapabilities();
            capabilities.SetCapability("deviceName", "Test");
            //capabilities.SetCapability("platformVersion", "4.4.4");
            capabilities.SetCapability("PlatformName", "Android");

            capabilities.SetCapability("appPackage", "com.alibaba.aliexpresshd");
            capabilities.SetCapability("appActivity", ".home.ui.MainActivity");

            capabilities.SetCapability("noReset", "true");

            driver = new AndroidDriver<AppiumWebElement>(new Uri("http://127.0.0.1:4723/wd/hub"), capabilities);
            driver.Manage().Timeouts().ImplicitlyWait(new TimeSpan(0, 0, 10));
            WebDriverWait waiter = new WebDriverWait(driver, new TimeSpan(0, 0, 20));

            //click Profile
            driver.FindElement(By.Id("com.alibaba.aliexpresshd:id/navigation_my_ae")).Click();
            //click Following
            driver.FindElement(By.Id("com.alibaba.aliexpresshd:id/rl_following")).Click();

            //get all favourite shop, find element with text, then click
            var favouriteShops = waiter.Until(ExpectedConditions.VisibilityOfAllElementsLocatedBy(By.Id("com.alibaba.aliexpresshd:id/follow_item")));

            var ruChargeShop = favouriteShops.Where(shop => ((AppiumWebElement)shop).FindElementByClassName("android.widget.TextView").Text == "RU recharge Store").Single();
            ruChargeShop.Click();

            //click on picture in shop
            driver.FindElementById("com.alibaba.aliexpresshd:id/iv_photo").Click();

            for (int i = 0; i < this.repeat; i++) //repeat
            {
                Console.WriteLine($"\r\nIteration {i + 1} from {this.repeat}");
                //get all available charge value
                var chargeValues = driver.FindElementsById("com.alibaba.aliexpresshd:id/ll_phone_recharge_product_item_container");

                AppiumWebElement chargeValForPay = null;
                //show their in console
                foreach (var chargeVal in chargeValues)
                {
                    var textCharge = chargeVal.FindElementByClassName("android.widget.TextView").Text;
                    if (textCharge.Contains(this.valueForCharge.ToString()))
                    {
                        textCharge += " <- select this value";
                        chargeValForPay = chargeVal;
                    }

                    Console.WriteLine("Find charge value - " + textCharge);
                }

                //select last value
                chargeValForPay
                    .ShouldNotBeNull($"Not found needed value for charge - {this.valueForCharge}")
                    .Click();

                //find input field and send telephone number
                var inputField = driver.FindElementById("com.alibaba.aliexpresshd:id/pniv_phone_number");
                var telNumber = inputField.Text;

                //check - if input not needed number
                if (Regex.Replace(telNumber, @"\(|\)|-| ", "") != this.telephoneNumber)
                {
                    inputField.Click();
                    inputField.SendKeys(this.telephoneNumber);

                    telNumber = inputField.Text;// driver.FindElementById("com.alibaba.aliexpresshd:id/pniv_phone_number").Text;
                    Console.WriteLine("Input telephone number - " + telNumber);
                    Regex.Replace(telNumber, @"\(|\)|-| ", "").ShouldBeEqualTo(this.telephoneNumber);
                }

                Console.WriteLine("Input telephone number - " + telNumber);

                //click on Recharge button
                driver.FindElementById("com.alibaba.aliexpresshd:id/bt_place_order").Click();

                //verify payment method and amount
                var selectedPaymentMethon = driver.FindElementById("com.alibaba.aliexpresshd:id/tv_selected_payment_method_text").Text;
                selectedPaymentMethon.ShouldContain("4167");
                Console.WriteLine("select payment method - " + selectedPaymentMethon);

                var paymentAmount = driver.FindElementById("com.alibaba.aliexpresshd:id/tv_payment_order_total_amount").Text;

                double.Parse(paymentAmount.Replace("руб.", ""))
                    .ShouldBeLessThanOrEqualTo(this.valueForCharge, $"Ammount for pay great than set value for charge - {this.valueForCharge}");
                Console.WriteLine("Amount for payment - " + paymentAmount);

                if (this.makePayment)
                {
                    //click Confirm and Pay
                    driver.FindElementById("com.alibaba.aliexpresshd:id/bt_confirm_and_pay").Click();

                    //find result of payment
                    waiter.Until(ExpectedConditions.ElementIsVisible(By.Id("com.alibaba.aliexpresshd:id/tv_payment_result_success_message")));
                }

                //return to back for next iteration
                ((AndroidDriver<AppiumWebElement>)driver).PressKeyCode(AndroidKeyCode.Back);
            }
        }
    }
}
