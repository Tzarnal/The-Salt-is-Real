using System;

namespace ChromiumConsole
{
    public enum STATE
    {
        OPEN,
        CLOSED,
        ClOSED_INFORMATION
    }

    public class SaltyStateMachine
    {
        private STATE currentState;
        public event EventHandler StateOpened;
        public event EventHandler StateClosed;
        public event EventHandler StateClosedInformation;

        public void RefreshState()
        {
            STATE lastState = currentState;
            var lockState = DataExtractor.GetBetState() == "open" ? STATE.OPEN : STATE.CLOSED;

            var footerText = DataExtractor.GetFooterText();
            var footerPopulated = !string.IsNullOrWhiteSpace(footerText);

            //If closed but information is now available
            if (lockState == STATE.CLOSED && currentState == STATE.CLOSED && footerPopulated)
            {
                currentState = STATE.ClOSED_INFORMATION;
                OnStateClosedInformation();
                return;
            }

            //If state changed from last measured state
            if (lastState != lockState)
            {
                if (lockState == STATE.OPEN)
                {
                    OnOpenState();
                    currentState = STATE.OPEN;
                }
                if (lockState == STATE.CLOSED && lastState != STATE.ClOSED_INFORMATION)
                {
                    OnClosedState();
                    currentState = STATE.CLOSED;
                }
            }

        }

        protected virtual void OnOpenState()
        {
            StateOpened?.Invoke(this, new EventArgs());
        }
        protected virtual void OnClosedState()
        {
            StateClosed?.Invoke(this, new EventArgs());
        }

        protected virtual void OnStateClosedInformation()
        {
            StateClosedInformation?.Invoke(this, new EventArgs());
        }

    }
}
