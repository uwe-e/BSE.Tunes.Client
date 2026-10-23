namespace BSE.Tunes.WinUI.Client.Controls
{
    public class IntEventArgs : EventArgs
    {
        public int Value { get; private set; }
        public IntEventArgs(int value)
        {
            this.Value = value;
        }
    }
}
