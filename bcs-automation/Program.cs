using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;
using bestcaptchasolver;
using HtmlAgilityPack;

namespace bcs_automation
{
    class Program
    {
        static void Main(string[] args)
        {
            new Automate().run();
        }
    }
    class Automate
    {
        // taken from /account page
        private string ACCESS_TOKEN = "BAC21DFA5FE5415CA9608BED45F8D703";
        private int SLEEP = 10;     
        private BestCaptchaSolverAPI bcs;

        /// <summary>
        /// Image captcha automation with browser
        /// </summary>
        private void browser_image()
        {
            Console.WriteLine("[+] Image (classic) captcha automation with browser");
            Console.WriteLine("---------------------------------");
            Console.WriteLine("[+] Starting browser");
            string url = "https://bestcaptchasolver.com/automation/image";
            using (var b = new ChromeDriver())
            {
                Console.WriteLine("[+] Navigating to image captcha page");
                b.Navigate().GoToUrl(url);
                Console.WriteLine("[+] Completing fields of form");
                b.FindElementById("username").SendKeys("image-browser-user-123");
                string captcha_image_b64 = b.FindElementById("automation-container").FindElement(By.ClassName("img-responsive")).GetAttribute("src");
                Console.WriteLine("[+] Submitting image to bestcaptchasolver");
                var opts = new Dictionary<string, string>();
                opts.Add("image", captcha_image_b64);

                // submit to bcs for solving
                var id = bcs.submit_image_captcha(opts);        
                Console.WriteLine(string.Format("[+] Got ID {0}, waiting for completion...", id));
                string text = "";
                while(text == "")
                {
                    text = bcs.retrieve(id)["text"];
                    if (text != "") break;        // get out if not null
                    Thread.Sleep(5000);
                }
                Console.WriteLine(string.Format("[+] Response from bestcaptchasolver.com: {0}", text));
                b.FindElementById("captcha-text").SendKeys(text);      // write text received from bcs
                Console.WriteLine("[+] Submitting form");
                b.FindElementById("automation_submit").Click();       // submit form
                Console.WriteLine("[+] Form submitted !");
                Thread.Sleep(SLEEP * 1000);
            }
        }

        /// <summary>
        /// Image captcha automation with requests
        /// </summary>
        private void requests_image()
        {
            var r = new Requests();     // use this, which holds a session / cookies for all requests
            Console.WriteLine("[+] Image (classic) captcha automation with requests");
            Console.WriteLine("---------------------------------");
            Console.WriteLine("[+] Making request to get image captcha and set the cookie");
            string url = "https://bestcaptchasolver.com/automation/image";
            var resp = r.GET(url);
            var doc = new HtmlDocument();       // use htmlagilitypack
            doc.LoadHtml(resp);
            var y = doc.DocumentNode.SelectNodes("//img")[1];       // get captcha image
            var captcha_image_b64 = y.GetAttributeValue("src", "");
            // replace `strange` chars with ==
            captcha_image_b64 = captcha_image_b64.Replace("&#x3D;", "=");
            Console.WriteLine("[+] Got data needed from page, submitting captcha to bestcaptchasolver");
            var opts = new Dictionary<string, string>();
            opts.Add("image", captcha_image_b64);

            // solve using bcs
            var id = bcs.submit_image_captcha(opts);        // submit to bcs for solving
            Console.WriteLine(string.Format("[+] Got ID {0}, waiting for completion...", id));
            string text = "";
            while (text == "")
            {
                text = bcs.retrieve(id)["text"];
                if (text != "") break;        // get out if not ""
                Thread.Sleep(5000);
            }
            Console.WriteLine(string.Format("[+] Response from bestcaptchasolver.com: {0}", text));
            Console.WriteLine("[+] Submitting data to site...");
            resp = r.PUT("https://bestcaptchasolver.com/automation/image/verify", "{\"username\":\"image-requests-user-123\",\"text\":\"" + text + "\"}");
            Console.WriteLine(string.Format("[+] Response from site: {0}", resp));
        }

        /// <summary>
        /// reCAPTCHA v2 automation with browser
        /// </summary>
        private void browser_recaptchav2()
        {
            Console.WriteLine("[+] reCAPTCHA v2 automation with browser");
            Console.WriteLine("---------------------------------");
            Console.WriteLine("[+] Starting browser");
            string url = "https://bestcaptchasolver.com/automation/recaptcha-v2";
            using (var b = new ChromeDriver())
            {
                Console.WriteLine("[+] Navigating to reCAPTCHA page");
                b.Navigate().GoToUrl(url);
                Console.WriteLine("[+] Completing fields of form");
                b.FindElementById("username").SendKeys("recaptcha-browser-user-123");
                string site_key = b.FindElementByClassName("g-recaptcha").GetAttribute("data-sitekey");
                Console.WriteLine("[+] Submitting page_url with site_key to bestcaptchasolver");
                var opts = new Dictionary<string, string>();
                opts.Add("page_url", url);
                opts.Add("site_key", site_key);

                // submit to bcs for solving
                var id = bcs.submit_recaptcha(opts);
                Console.WriteLine(string.Format("[+] Got ID {0}, waiting for completion...", id));
                string gresponse = "";
                while (gresponse == "")
                {
                    gresponse = bcs.retrieve(id)["gresponse"];
                    if (gresponse != "") break;        // get out if not null
                    Thread.Sleep(5000);
                }
                Console.WriteLine(string.Format("[+] Response from bestcaptchasolver.com: {0}", gresponse));
                IJavaScriptExecutor js = (IJavaScriptExecutor)b;
                js.ExecuteScript("$(\"#g-recaptcha-response\").val(\"" + gresponse + "\")");
                Console.WriteLine("[+] Submitting form");
                b.FindElementById("automation_submit").Click();       // submit form
                Console.WriteLine("[+] Form submitted !");
                Thread.Sleep(SLEEP * 1000);
            }
        }

        /// <summary>
        /// reCAPTCHA v2 automation with requests
        /// </summary>
        private void requests_recaptchav2()
        {
            var r = new Requests();     // use this, which holds a session / cookies for all requests
            Console.WriteLine("[+] reCAPTCHA v2 automation with requests");
            Console.WriteLine("---------------------------------");
            Console.WriteLine("[+] Making request to get sitekey");
            string url = "https://bestcaptchasolver.com/automation/recaptcha-v2";
            var resp = r.GET(url);
            var doc = new HtmlDocument();       // use htmlagilitypack
            doc.LoadHtml(resp);
            // get sitekey
            var y = doc.DocumentNode.SelectSingleNode("//div[@class=\"g-recaptcha\"]");       // get captcha image
            var site_key = y.GetAttributeValue("data-sitekey", "");
            Console.WriteLine(string.Format("[+] Got site_key from page: {0}, submitting captcha to bestcaptchasolver", site_key));
            var opts = new Dictionary<string, string>();
            opts.Add("page_url", url);
            opts.Add("site_key", site_key);
            // solve using bcs
            var id = bcs.submit_recaptcha(opts);        // submit to bcs for solving
            Console.WriteLine(string.Format("[+] Got ID {0}, waiting for completion...", id));
            string gresponse = "";
            while (gresponse == "")
            {
                gresponse = bcs.retrieve(id)["gresponse"];
                if (gresponse != "") break;        // get out if not ""
                Thread.Sleep(5000);
            }
            Console.WriteLine(string.Format("[+] Response from bestcaptchasolver.com: {0}", gresponse));
            Console.WriteLine("[+] Submitting data to site...");
            var data = "{\"username\":\"recaptcha-requests-user-123\",\"token\":\"" + gresponse + "\", \"site_key\":\"" + site_key + "\"}";
            Console.WriteLine(data);
            resp = r.PUT("https://bestcaptchasolver.com/automation/recaptcha/verify", data);
            Console.WriteLine(string.Format("[+] Response from site: {0}", resp));
        }

        /// <summary>
        /// invisible reCAPTCHA automation with browser
        /// </summary>
        private void browser_invisible()
        {
            Console.WriteLine("[+] invisible reCAPTCHA automation with browser");
            Console.WriteLine("---------------------------------");
            Console.WriteLine("[+] Starting browser");
            string url = "https://bestcaptchasolver.com/automation/recaptcha-invisible";
            using (var b = new ChromeDriver())
            {
                Console.WriteLine("[+] Navigating to invisible reCAPTCHA page");
                b.Navigate().GoToUrl(url);
                Console.WriteLine("[+] Completing fields of form");
                b.FindElementById("username").SendKeys("invisible-browser-user-123");
                string site_key = b.FindElementByClassName("g-recaptcha").GetAttribute("data-sitekey");
                Console.WriteLine("[+] Submitting page_url with site_key to bestcaptchasolver");
                var opts = new Dictionary<string, string>();
                opts.Add("page_url", url);
                opts.Add("site_key", site_key);

                // submit to bcs for solving
                var id = bcs.submit_recaptcha(opts);
                Console.WriteLine(string.Format("[+] Got ID {0}, waiting for completion...", id));
                string gresponse = "";
                while (gresponse == "")
                {
                    gresponse = bcs.retrieve(id)["gresponse"];
                    if (gresponse != "") break;        // get out if not null
                    Thread.Sleep(5000);
                }
                Console.WriteLine(string.Format("[+] Response from bestcaptchasolver.com: {0}", gresponse));
                IJavaScriptExecutor js = (IJavaScriptExecutor)b;
                js.ExecuteScript("check(\"" + gresponse + "\")");
                Console.WriteLine("[+] Submitting form");
                Console.WriteLine("[+] Form submitted !");
                Thread.Sleep(SLEEP * 1000);
            }
        }

        /// <summary>
        /// invisible reCAPTCHA automation with requests
        /// </summary>
        private void requests_invisible()
        {
            var r = new Requests();     // use this, which holds a session / cookies for all requests
            Console.WriteLine("[+] invisible reCAPTCHA automation with requests");
            Console.WriteLine("---------------------------------");
            Console.WriteLine("[+] Making request to get sitekey");
            string url = "https://bestcaptchasolver.com/automation/recaptcha-invisible";
            var resp = r.GET(url);
            var doc = new HtmlDocument();       // use htmlagilitypack
            doc.LoadHtml(resp);
            // get sitekey
            var y = doc.DocumentNode.SelectSingleNode("//button[@class=\"g-recaptcha btn btn-primary btn-link btn-wd btn-lg\"]");       // get captcha image
            var site_key = y.GetAttributeValue("data-sitekey", "");
            Console.WriteLine(string.Format("[+] Got site_key from page: {0}, submitting captcha to bestcaptchasolver", site_key));
            var opts = new Dictionary<string, string>();
            opts.Add("page_url", url);
            opts.Add("site_key", site_key);
            opts.Add("type", "2");      // set type to 2, default: 1 - v2
            // solve using bcs
            var id = bcs.submit_recaptcha(opts);        // submit to bcs for solving
            Console.WriteLine(string.Format("[+] Got ID {0}, waiting for completion...", id));
            string gresponse = "";
            while (gresponse == "")
            {
                gresponse = bcs.retrieve(id)["gresponse"];
                if (gresponse != "") break;        // get out if not ""
                Thread.Sleep(5000);
            }
            Console.WriteLine(string.Format("[+] Response from bestcaptchasolver.com: {0}", gresponse));
            Console.WriteLine("[+] Submitting data to site...");
            var data = "{\"username\":\"invisible-requests-user-123\",\"token\":\"" + gresponse + "\", \"site_key\":\"" + site_key + "\"}";
            Console.WriteLine(data);
            resp = r.PUT("https://bestcaptchasolver.com/automation/recaptcha/verify", data);
            Console.WriteLine(string.Format("[+] Response from site: {0}", resp));
        }

        /// <summary>
        /// Main automation method
        /// </summary>
        public void run()
        {
            Console.WriteLine("[+] Automation started");
            // initialize bcs object using token
            bcs = new BestCaptchaSolverAPI(ACCESS_TOKEN);
            try
            {
                browser_image();
                requests_image();

                browser_recaptchav2();
                requests_recaptchav2();

                browser_invisible();
                requests_invisible();
            }
            catch(Exception ex)
            {
                Console.WriteLine("[!] Error: " + ex.Message);
            }
            finally
            {
                Console.WriteLine("[+] Automation finished !");
                Console.ReadLine();
            }
        }
    }
}
