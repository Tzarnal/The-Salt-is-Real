using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChromiumConsole
{
    public enum STATE
    {
        OPEN,
        CLOSED
    }

    public class SaltyStateMachine
    {
        private STATE currentState;
        public event EventHandler StateOpened;
        public event EventHandler StateClosed;

        public void RefreshState()
        {
            STATE lastState = currentState;
            currentState = DataExtractor.GetBetState() == "open" ? STATE.OPEN : STATE.CLOSED;

            //If state changed from last measured state
            if (lastState != currentState)
            {
                if (currentState == STATE.OPEN)
                {
                    OnOpenState();
                }
                if (currentState == STATE.CLOSED)
                {
                    OnClosedState();
                }
            }

        }

        protected virtual void OnOpenState()
        {
            if (StateOpened != null)
            {
                StateOpened(this, new EventArgs());
            }
        }
        protected virtual void OnClosedState()
        {
            if (StateClosed != null)
            {
                StateClosed(this, new EventArgs());
            }
        }



    }
}
