using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace ThreeHousesPersonDataEditor
{
    [SuppressMessage( "ReSharper", "InconsistentNaming" )]
    public partial class EndianBinaryWriter
    {
        [DebuggerStepThrough, MethodImpl( MethodImplOptions.AggressiveInlining )]
        public void WriteByte( byte value ) => Write( value );
		
		[DebuggerStepThrough, MethodImpl( MethodImplOptions.AggressiveInlining )]
        public void WriteSByte( sbyte value ) => Write( value );

        [DebuggerStepThrough, MethodImpl( MethodImplOptions.AggressiveInlining )]
        public void WriteBytes( IEnumerable<byte> values ) => Write( values );

        [DebuggerStepThrough, MethodImpl( MethodImplOptions.AggressiveInlining )]
        public void WriteSBytes( IEnumerable<sbyte> values ) => Write( values );

        [DebuggerStepThrough, MethodImpl( MethodImplOptions.AggressiveInlining )]
        public void WriteBools( IEnumerable<bool> values ) => Write( values );

        [DebuggerStepThrough, MethodImpl( MethodImplOptions.AggressiveInlining )]
        public void WriteInt16( short value ) => Write( value );

        [DebuggerStepThrough, MethodImpl( MethodImplOptions.AggressiveInlining )]
        public void WriteInt16s( IEnumerable<short> values ) => Write( values );

        [DebuggerStepThrough, MethodImpl( MethodImplOptions.AggressiveInlining )]
        public void WriteUInt16( ushort value ) => Write( value );

        [DebuggerStepThrough, MethodImpl( MethodImplOptions.AggressiveInlining )]
        public void WriteUInt16s( IEnumerable<ushort> values ) => Write( values );

        [DebuggerStepThrough, MethodImpl( MethodImplOptions.AggressiveInlining )]
        public void WriteInt32( int value ) => Write( value );

        [DebuggerStepThrough, MethodImpl( MethodImplOptions.AggressiveInlining )]
        public void WriteInt32s( IEnumerable<int> values ) => Write( values );

        [DebuggerStepThrough, MethodImpl( MethodImplOptions.AggressiveInlining )]
        public void WriteUInt32( uint value ) => Write( value );

        [DebuggerStepThrough, MethodImpl( MethodImplOptions.AggressiveInlining )]
        public void WriteUInt32s( IEnumerable<uint> values ) => Write( values );

        [DebuggerStepThrough, MethodImpl( MethodImplOptions.AggressiveInlining )]
        public void WriteInt64( long value ) => Write( value );

        [DebuggerStepThrough, MethodImpl( MethodImplOptions.AggressiveInlining )]
        public void WriteInt64s( IEnumerable<long> values ) => Write( values );

        [DebuggerStepThrough, MethodImpl( MethodImplOptions.AggressiveInlining )]
        public void WriteUInt64( ulong value ) => Write( value );

        [DebuggerStepThrough, MethodImpl( MethodImplOptions.AggressiveInlining )]
        public void WriteUInt64s( IEnumerable<ulong> values ) => Write( values );

        [DebuggerStepThrough, MethodImpl( MethodImplOptions.AggressiveInlining )]
        public void WriteSingle( float value ) => Write( value );

        [DebuggerStepThrough, MethodImpl( MethodImplOptions.AggressiveInlining )]
        public void WriteSingles( IEnumerable<float> values ) => Write( values );

        [DebuggerStepThrough, MethodImpl( MethodImplOptions.AggressiveInlining )]
        public void WriteDecimal( decimal value ) => Write( value );

        [DebuggerStepThrough, MethodImpl( MethodImplOptions.AggressiveInlining )]
        public void WriteDecimals( IEnumerable<decimal> values ) => Write( values );

        [DebuggerStepThrough, MethodImpl( MethodImplOptions.AggressiveInlining )]
        public void WriteColor( Color color ) => Write( color );

        [DebuggerStepThrough, MethodImpl( MethodImplOptions.AggressiveInlining )]
        public void WriteColors( IEnumerable<Color> values ) => Write( values );

        [DebuggerStepThrough, MethodImpl( MethodImplOptions.AggressiveInlining )]
        public void WriteString( string value, StringBinaryFormat format, int fixedLength = -1 ) => Write( value, format, fixedLength );
    }
}