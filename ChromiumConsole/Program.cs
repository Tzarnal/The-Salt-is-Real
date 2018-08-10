using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using CefSharp;
using CefSharp.OffScreen;
using CefSharp.Internals;
using System.IO;
using System.Globalization;

namespace ChromiumConsole
{
    class Program
    {
        public static ChromiumWebBrowser frontPageBrowser;

        static LoginService loginService;
        static Thread refreshThread;

        static SaltyStateMachine saltyStateMachine;

        public static bool refreshLoopInitiated = false;

        static void Main(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.White;

            //ChooseBettingRiskLevel();

            InitializeServices();

            Console.ReadKey();

            MessageService.OnClosingBotMessage();

            Console.ReadKey();

            ShutDownServices();
        }

        private static void ChooseBettingRiskLevel()
        {
            Console.WriteLine("Choose betting risk level:");
            Console.WriteLine("1 = Very Safe, 2 = Safe, 3 = Moderate, 4 = Risky, 5 = Very Risky");

            while (true)
            {
                int result;

                if (Int32.TryParse(Console.ReadLine(), out result))
                {
                    switch (result)
                    {
                        case 1:
                            BetService.BetSettings.Risk_Level = BetSettings.RISK_LEVEL.VERY_SAFE;
                            Console.WriteLine("Betting risk set to: Very Safe!");
                            return;
                        case 2:
                            BetService.BetSettings.Risk_Level = BetSettings.RISK_LEVEL.SAFE;
                            Console.WriteLine("Betting risk set to: Safe!");
                            return;
                        case 3:
                            BetService.BetSettings.Risk_Level = BetSettings.RISK_LEVEL.MODERATE;
                            Console.WriteLine("Betting risk set to: Moderate!");
                            return;
                        case 4:
                            BetService.BetSettings.Risk_Level = BetSettings.RISK_LEVEL.RISKY;
                            Console.WriteLine("Betting risk set to: Risky!");
                            return;
                        case 5:
                            BetService.BetSettings.Risk_Level = BetSettings.RISK_LEVEL.VERY_RISKY;
                            Console.WriteLine("Betting risk set to: Very Risky!");
                            return;

                        default:
                            Console.WriteLine("You must enter an integer value from 1 to 5!");
                            break;
                    }
                }
                else
                {
                    Console.WriteLine("You must enter an integer value from 1 to 5!");
                }
            }
        }

        private static void ShutDownServices()
        {
            refreshThread.Abort();
            Cef.Shutdown();
        }

        private static void InitializeServices()
        {
            refreshThread = new Thread(RefreshLoop);

            var settings = new CefSettings()
            {
                CachePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CefSharp\\Cache"),
                LogSeverity = LogSeverity.Disable
            };

            Cef.Initialize(settings, performDependencyCheck: true, browserProcessHandler: null);

            frontPageBrowser = new ChromiumWebBrowser("https://www.saltybet.com/authenticate?signin=1");

            saltyStateMachine = new SaltyStateMachine();


            saltyStateMachine.StateClosed += SaltyStateMachine_StateClosed;
            saltyStateMachine.StateOpened += SaltyStateMachine_StateOpenend;


            //RISK LEVEL
            BetService.BetSettings.Risk_Level = BetSettings.RISK_LEVEL.RISKY;

            loginService = new LoginService("<usersname>", "<password>", frontPageBrowser);

            frontPageBrowser.LoadingStateChanged += Browser_LoadingStateChanged;

        }

        
        private static void SaltyStateMachine_StateOpenend(object sender, EventArgs e)
        {
            //If last match was bet on
            if (MatchInformation.HasPlacedBet)
            {
                MessageService.OnMatchEndedMessage();
                MatchInformation.UpdateMatchEarnings();
            }

            Console.WriteLine($"\nCurrent salt is {PlayerInformation.SaltAmount.ToString("C0", CultureInfo.CreateSpecificCulture("en-US"))}");
            MatchInformation.statsBeenUpdated = false;
            MatchInformation.HasPlacedBet = false;     
        }

        private static void SaltyStateMachine_StateClosed(object sender, EventArgs e)
        {
            MatchInformation.statsBeenUpdated = true;
        }

        private static void Browser_LoadingStateChanged(object sender, LoadingStateChangedEventArgs e)
        {
            if (!e.IsLoading)
            {
                if (!loginService.IsLoggedIn)
                {
                    loginService.Login();
                }
                if (loginService.IsLoggedIn)
                {
                    if (!refreshLoopInitiated)
                    {
                        refreshThread.Start();
                        refreshLoopInitiated = true;
                    }
                }
            }
            
        }


        private static void Refresh()
        {
            try
            {
                saltyStateMachine.RefreshState();
                MatchInformation.UpdateFighterData();
            }
            catch (Exception)
            {
                Console.WriteLine("Something went wrong!");
            }

            if (true)
            {
                
                if (MatchInformation.IsBetStateOpen())
                {
                    MatchInformation.UpdateData();

                    BetService.Bet(MatchInformation.p1WinRate > MatchInformation.p2WinRate ? "player1" : "player2", BetService.CalculateWager());
                }
            }
            else
            {
                MatchInformation.UpdateData();
            }
            
        }

        private static void RefreshLoop()
        {
            while (true)
            {
                try
                {
                    Thread.Sleep(3000);
                    Refresh();
                }
                catch (InvalidOperationException)
                {
                    break;
                }
                catch (ThreadInterruptedException)
                {
                    break;
                }
            }
        }
    }
}
