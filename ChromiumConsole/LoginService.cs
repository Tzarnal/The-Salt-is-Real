using CefSharp.OffScreen;


namespace ChromiumConsole
{
    class LoginService
    {
        private string email;
        private string password;
        private ChromiumWebBrowser browser;

        public bool IsLoggedIn = false;

        public LoginService(string email, string password, ChromiumWebBrowser browser)
        {
            this.email = email;
            this.password = password;
            this.browser = browser;
        }


        public void Login()
        {
            
            if (browser.Address == "https://www.saltybet.com/authenticate?signin=1")
            {

                JavaScriptService.ExecuteJS($"document.getElementById(\"email\").value = \"{email}\"", browser);
                JavaScriptService.ExecuteJS($"document.getElementById(\"pword\").value = \"{password}\"", browser);
                JavaScriptService.ExecuteJS($"document.getElementById(\"signinform\").submit()", browser);

                IsLoggedIn = true;

                
            }
            else
            {
                browser.Load("https://www.saltybet.com/authenticate?signin=1");
            }
            
        }
    }
}
