using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CefSharp;
using CefSharp.OffScreen;

namespace ChromiumConsole
{
    static class JavaScriptService
    {

        public static void ExecuteJS(string script, ChromiumWebBrowser browser)
        {
            try
            {
                browser.GetMainFrame().ExecuteJavaScriptAsync(script);

            }
            catch (Exception)
            {
                Console.WriteLine("Error executing: " + script);
            }
            
        }

        public static void ExecuteJS(string script, ChromiumWebBrowser browser, out bool successFlag)
        {
            successFlag = true;

            try
            {
                browser.GetMainFrame().ExecuteJavaScriptAsync(script);
            }
            catch (Exception)
            {
                Console.WriteLine("Error executing: " + script);
                successFlag = false;
            }

        }

        public static string EvaluateJS(string script, ChromiumWebBrowser browser)
        {
           
                // If browser is initialized
                if (browser.IsBrowserInitialized)
                {
                    // Run script in a task, p1te returns the amount betted on blue in a formatted string with $
                    var task = browser.GetMainFrame().EvaluateScriptAsync(script);

                    // Run task
                    task.ContinueWith(t =>
                    {
                        if (!t.IsFaulted)
                        {
                            var response = t.Result;
                            // Return response from JS
                            return "";
                        }
                        return "0";
                    }, TaskScheduler.Current);
                    // If return is not null return the result from the task
                    if (task.Result.Result != null)
                    {
                        return task.Result.Result.ToString();
                    }
                    else
                    {
                        return "null";
                    }
                }
                return "";
        }
    }
}
