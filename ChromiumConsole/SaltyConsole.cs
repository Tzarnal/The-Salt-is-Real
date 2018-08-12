using System;
using System.Threading;
using CefSharp;
using CefSharp.OffScreen;
using System.IO;
using ChromiumConsole.EventArguments;


namespace ChromiumConsole
{
    public class SaltyConsole
    {
        public enum Players
        {
            RedPlayer,
            BluePlayer,
            Unknown,
        }

        public event EventHandler LoginSuccess;
        public event EventHandler MatchStart;
        public event EventHandler MatchEnded;

        public static ChromiumWebBrowser frontPageBrowser;
        public static bool refreshLoopInitiated = false;

        public static bool exit = false;

        private string _saltyAccount;
        private string _saltyPassword;

        private static bool _checkedTournament;
        private static bool _lastMatchWasTournament = false;
        private static int _lastTournamentCount = 0;
        
        static LoginService loginService;
        static Thread refreshThread;

        static SaltyStateMachine saltyStateMachine;
       
        public void Start(string account, string password)
        {
            _saltyAccount = account;
            _saltyPassword = password;
            
            InitializeServices();
            
            while (!exit)
            {
                Thread.Sleep(500);
            }


            ShutDownServices();            
        }

        public void PlaceBet(Players player, int BetAmount)
        {
            if (BetAmount > PlayerInformation.SaltAmount)
            {
                BetAmount = PlayerInformation.SaltAmount;
            }

            var playerId = "player1";

            if (player == Players.BluePlayer)
            {
                playerId = "player2";
            }

            bool successflag1, successflag2;

            JavaScriptService.ExecuteJS($"document.getElementById(\"wager\").value = \"{BetAmount}\"", SaltyConsole.frontPageBrowser, out successflag1);
            JavaScriptService.ExecuteJS($"document.getElementById(\"{playerId}\").click()", SaltyConsole.frontPageBrowser, out successflag2);

            if (!successflag1 || !successflag2)
            {
                MatchInformation.HasPlacedBet = false;
                return;
            }

            MatchInformation.SaltBettedOnMatch = BetAmount;
            MatchInformation.SaltBeforeMatch = PlayerInformation.SaltAmount;
            SessionStatistics.MatchesPlayed++;
            MatchInformation.currentBettedPlayer = player == Players.RedPlayer ? MatchInformation.currentRedPlayer : MatchInformation.currentBluePlayer;
            MatchInformation.HasPlacedBet = true;

        }

        protected virtual void OnMatchStart(EventArgs e)
        {            
            MatchStart?.Invoke(this, e);
        }

        protected virtual void OnMatchEnd(EventArgs e)
        {
            MatchEnded?.Invoke(this, e);
        }

        protected virtual void OnLoginSuccess(EventArgs e)
        {
            LoginSuccess?.Invoke(this, e);
        }

        private static void ShutDownServices()
        {
            refreshThread.Abort();
            Cef.Shutdown();
        }

        private void InitializeServices()
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

            
            loginService = new LoginService(_saltyAccount, _saltyPassword, frontPageBrowser);

            frontPageBrowser.LoadingStateChanged += Browser_LoadingStateChanged;

        }

        
        private void SaltyStateMachine_StateOpenend(object sender, EventArgs e)
        {
            _checkedTournament = false;

            var winningPlayer = Players.Unknown;
            var matchEndEventArgs = new MatchEndEventArgs
            {
                Salt = DataExtractor.GetSaltBalanceNum(),
                BluePlayer = MatchInformation.currentBluePlayer,
                RedPlayer = MatchInformation.currentRedPlayer,

                Tournament = _lastMatchWasTournament,                
                TournamentPlayersRemaining = _lastTournamentCount,

                WinningPlayer = winningPlayer,
                SaltBalanceChange =DataExtractor.GetSaltBalanceNum() - MatchInformation.SaltBeforeMatch,
            };


            //If last match was bet on
            if (MatchInformation.HasPlacedBet)
            {
                matchEndEventArgs.PickedPlayerName = MatchInformation.currentBettedPlayer;

                MatchInformation.UpdateMatchEarnings();

                var pickedPlayerWon = (MatchInformation.SaltBeforeMatch < PlayerInformation.SaltAmount);
                var pickedPlayer = Players.RedPlayer;

                if (MatchInformation.currentBettedPlayer == MatchInformation.currentBluePlayer)
                {
                    pickedPlayer = Players.BluePlayer;
                }

                if (pickedPlayerWon)
                {
                    if (pickedPlayer == Players.BluePlayer)
                    {
                        matchEndEventArgs.WinningPlayer = Players.BluePlayer;
                        matchEndEventArgs.WinningPlayerName = MatchInformation.currentBluePlayer;
                        matchEndEventArgs.LoosingPlayerName = MatchInformation.currentRedPlayer;

                    }
                    else
                    {
                        matchEndEventArgs.WinningPlayer = Players.RedPlayer;
                        matchEndEventArgs.WinningPlayerName = MatchInformation.currentRedPlayer;
                        matchEndEventArgs.LoosingPlayerName = MatchInformation.currentBluePlayer;
                    }
                }
                else
                {
                    if (pickedPlayer == Players.BluePlayer)
                    {
                        matchEndEventArgs.WinningPlayer = Players.RedPlayer;
                        matchEndEventArgs.WinningPlayerName = MatchInformation.currentRedPlayer;
                        matchEndEventArgs.LoosingPlayerName = MatchInformation.currentBluePlayer;
                    }
                    else
                    {
                        matchEndEventArgs.WinningPlayer = Players.BluePlayer;
                        matchEndEventArgs.WinningPlayerName = MatchInformation.currentBluePlayer;
                        matchEndEventArgs.LoosingPlayerName = MatchInformation.currentRedPlayer;
                    }
                }
            }                        
            
            OnMatchEnd(matchEndEventArgs);
            
            MatchInformation.statsBeenUpdated = false;
            MatchInformation.HasPlacedBet = false;
            MatchInformation.HasOfferedBet = false;
        }

        private void SaltyStateMachine_StateClosed(object sender, EventArgs e)
        {
            MatchInformation.statsBeenUpdated = true;
        }

        private void Browser_LoadingStateChanged(object sender, LoadingStateChangedEventArgs e)
        {
            if (!e.IsLoading)
            {
                if (!loginService.IsLoggedIn)
                {
                    loginService.Login();
                    OnLoginSuccess(new EventArgs());
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

        private void Refresh()
        {
            if (DataExtractor.GetBetState() == "locked" && !_checkedTournament)
            {
                _checkedTournament = true;

                var isTournament = DataExtractor.GetTournamentActive();
                var bracketCount = DataExtractor.GetBracketCount();

                if (isTournament && !_lastMatchWasTournament)
                {
                    //its a tournament now, offer a new bet;
                    var matchStartEventArgs = new MatchStartEventArgs
                    {
                        Salt = DataExtractor.GetSaltBalanceNum(),
                        BluePlayer = MatchInformation.currentBluePlayer,
                        RedPlayer = MatchInformation.currentRedPlayer,

                        Tournament = true,
                        TournamentPlayersRemaining = bracketCount,
                    };

                    OnMatchStart(matchStartEventArgs);
                }

                _lastMatchWasTournament = isTournament;
                _lastTournamentCount = bracketCount;
            }

            saltyStateMachine.RefreshState();
            MatchInformation.UpdateFighterData();
                            
            if (!MatchInformation.HasOfferedBet)
            {
                MatchInformation.UpdateData();

                if (_lastMatchWasTournament && _lastTournamentCount > 1)
                {
                    _lastTournamentCount--;
                }

                if (_lastTournamentCount <= 1)
                {
                    _lastMatchWasTournament = false;
                }


                var matchStartEventArgs = new MatchStartEventArgs
                {
                    Salt = DataExtractor.GetSaltBalanceNum(),
                    BluePlayer = MatchInformation.currentBluePlayer,
                    RedPlayer = MatchInformation.currentRedPlayer,

                    Tournament = _lastMatchWasTournament,
                    TournamentPlayersRemaining = _lastTournamentCount,
                };

                OnMatchStart(matchStartEventArgs);
                MatchInformation.HasOfferedBet = true;
            }
            
        }

        private void RefreshLoop()
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
