namespace Dalamud.Crypto
{
    /// <summary>
    /// An internal state of blowfish cipher.
    /// </summary>
    /// <remarks>
    /// The reason why this is implemented as a value type is to use fixed size array and therefore eliminate bound checking.
    /// </remarks>
    unsafe struct BlowfishState
    {
        public fixed uint P[Blowfish.PSize];
        public fixed uint S0[Blowfish.SSize];
        public fixed uint S1[Blowfish.SSize];
        public fixed uint S2[Blowfish.SSize];
        public fixed uint S3[Blowfish.SSize];
    }
}
