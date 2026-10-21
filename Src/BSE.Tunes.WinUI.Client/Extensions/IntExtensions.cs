namespace BSE.Tunes.WinUI.Client.Extensions
{
    public static class IntExtensions
    {
        /// <summary>
        /// Mathematical modulo operation that handles negative numbers correctly
        /// </summary>
        public static int Mod(this int value, int modulus)
        {
            if (modulus == 0)
                return 0;

            int result = value % modulus;
            return result < 0 ? result + modulus : result;
        }
    }
}