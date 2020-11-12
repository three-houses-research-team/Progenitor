namespace ThreeHousesPersonDataEditor
{
    public struct BitField
    {
        public int From { get; }

        public int To { get; }

        public int Count { get; }

        public BitField( int from, int to )
        {
            From = from;
            To = to;
            Count = ( to - from ) + 1;
        }

        public byte Unpack( byte value )
        {
            const int TOTAL_BIT_COUNT = sizeof( byte ) * 8;
            const byte MAX_VALUE = byte.MaxValue;

            var mask         = MAX_VALUE >> ( TOTAL_BIT_COUNT - Count );
            var valueShifted = value >> From;
            return ( byte )( valueShifted & mask );
        }

        public void Pack( ref byte destination, byte value )
        {
            const int TOTAL_BIT_COUNT = sizeof( byte ) * 8;
            const byte MAX_VALUE = byte.MaxValue;

            var mask         = MAX_VALUE >> ( TOTAL_BIT_COUNT - Count );
            destination = ( byte )( ( destination & ~( mask << From ) ) | ( ( value & mask ) << From ) );
        }

        public ushort Unpack( ushort value )
        {
            const int TOTAL_BIT_COUNT = sizeof( ushort ) * 8;
            const ushort MAX_VALUE = ushort.MaxValue;

            var mask = MAX_VALUE >> ( TOTAL_BIT_COUNT - Count );
            var valueShifted = value >> From;
            return ( ushort ) ( valueShifted & mask );
        }

        public void Pack( ref ushort destination, ushort value )
        {
            const int TOTAL_BIT_COUNT = sizeof( ushort ) * 8;
            const ushort MAX_VALUE = ushort.MaxValue;

            var mask = MAX_VALUE >> ( TOTAL_BIT_COUNT - Count );
            destination = ( ushort )( ( destination & ~( mask << From ) ) | ( ( value & mask ) << From ) );
        }

        public uint Unpack( uint value )
        {
            const int TOTAL_BIT_COUNT = sizeof( uint ) * 8;
            const uint MAX_VALUE = uint.MaxValue;

            var mask = MAX_VALUE >> ( TOTAL_BIT_COUNT - Count );
            var valueShifted = value >> From;
            return valueShifted & mask;
        }

        public void Pack( ref uint destination, uint value )
        {
            const int TOTAL_BIT_COUNT = sizeof( uint ) * 8;
            const uint MAX_VALUE = uint.MaxValue;

            var mask = MAX_VALUE >> ( TOTAL_BIT_COUNT - Count );
            destination = ( uint )( ( destination & ~( mask << From ) ) | ( ( value & mask ) << From ) );
        }
    }
}
