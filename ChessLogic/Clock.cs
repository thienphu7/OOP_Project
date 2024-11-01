using System;
using System.Timers;
using ChessLogic;

public class Clock
{
    private System.Timers.Timer player1Timer;
    private System.Timers.Timer player2Timer;
    private TimeSpan player1Time;
    private TimeSpan player2Time;
    private TimeSpan increment;
    private bool isPlayer1Turn;

    public TimeMode SelectedTimeMode { get; set; }

    public Clock(TimeMode mode)
    {
        SelectedTimeMode = mode;

        player1Timer = new System.Timers.Timer(1000);
        player2Timer = new System.Timers.Timer(1000);

        player1Timer.Elapsed += Player1Timer_Elapsed;
        player2Timer.Elapsed += Player2Timer_Elapsed;
    }

    public void StartClock()
    {
        switch (SelectedTimeMode)
        {
            //Standard: 90 minutes + 30 seconds/move
            case TimeMode.Standard: 
                player1Time = TimeSpan.FromMinutes(90);
                player2Time = TimeSpan.FromMinutes(90);
                increment = TimeSpan.FromSeconds(30);
                break;

            //Rapid: 15 minutes + 10 seconds/move
            case TimeMode.Rapid:
                player1Time = TimeSpan.FromMinutes(15);
                player2Time = TimeSpan.FromMinutes(15);
                increment = TimeSpan.FromSeconds(10);
                break;

            //Blitz: 3 minutes + 2 seconds/move
            case TimeMode.Blitz:
                player1Time = TimeSpan.FromMinutes(3);
                player2Time = TimeSpan.FromMinutes(3);
                increment = TimeSpan.FromSeconds(2);
                break;
        }

        isPlayer1Turn = true;
        player1Timer.Start();
    }

    public void SwitchTurn()
    {
        if (isPlayer1Turn)
        {
            player1Timer.Stop();
            player1Time += increment;
            player2Timer.Start();
        }
        else
        {
            player2Timer.Stop();
            player2Time += increment;
            player1Timer.Start();
        }
        isPlayer1Turn = !isPlayer1Turn;

        OnPlayer1TimeUpdate?.Invoke(this, new PlayerTimeEventArgs(player1Time));
        OnPlayer2TimeUpdate?.Invoke(this, new PlayerTimeEventArgs(player2Time));
    }

    private void Player1Timer_Elapsed(object sender, ElapsedEventArgs e)
    {
        player1Time -= TimeSpan.FromSeconds(1);
        if (player1Time <= TimeSpan.Zero)
        {
            player1Timer.Stop();
            OnPlayer1TimeOut?.Invoke(this, EventArgs.Empty);
        }
        OnPlayer1TimeUpdate?.Invoke(this, new PlayerTimeEventArgs(player1Time));
    }

    private void Player2Timer_Elapsed(object sender, ElapsedEventArgs e)
    {
        player2Time -= TimeSpan.FromSeconds(1);
        if (player2Time <= TimeSpan.Zero)
        {
            player2Timer.Stop();
            OnPlayer2TimeOut?.Invoke(this, EventArgs.Empty);
        }
        OnPlayer2TimeUpdate?.Invoke(this, new PlayerTimeEventArgs(player2Time));
    }

    public event EventHandler OnPlayer1TimeOut;
    public event EventHandler<PlayerTimeEventArgs> OnPlayer1TimeUpdate;
    public event EventHandler OnPlayer2TimeOut;
    public event EventHandler<PlayerTimeEventArgs> OnPlayer2TimeUpdate;

    public class PlayerTimeEventArgs : EventArgs
    {
        public PlayerTimeEventArgs(TimeSpan time)
        {
            Time = time;
        }
        public TimeSpan Time { get; private set; }
    }

    public bool IsCurrentPlayerOutOfTime()
    {
        return (isPlayer1Turn && player1Time <= TimeSpan.Zero) ||
               (!isPlayer1Turn && player2Time <= TimeSpan.Zero);
    }
}
