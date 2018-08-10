using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CefSharp;
using CefSharp.OffScreen;
using System.IO;
using System.Diagnostics;

namespace ChromiumConsole
{
    static class ScreenShotService
    {

        public static void TakeScreenShot(ChromiumWebBrowser browser)
        {
            var screenshotPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "CefSharp screenshot" + DateTime.Now.Ticks + ".png");

            var task = browser.ScreenshotAsync(true);

            task.ContinueWith(x =>
            {

                Console.WriteLine();
                Console.WriteLine("Screenshot ready. Saving to {0}", screenshotPath);

                // Save the Bitmap to the path.
                // The image type is auto-detected via the ".png" extension.
                task.Result.Save(screenshotPath);

                // We no longer need the Bitmap.
                // Dispose it to avoid keeping the memory alive.  Especially important in 32-bit applications.
                task.Result.Dispose();

                Console.WriteLine("Screenshot saved.  Launching your default image viewer...");

                // Tell Windows to launch the saved image.
                Process.Start(screenshotPath);

                Console.WriteLine("Image viewer launched.  Press any key to exit.");
            }, TaskScheduler.Default);
        }
    }
}
