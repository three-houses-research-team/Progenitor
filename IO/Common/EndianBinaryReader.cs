using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Text;

namespace ThreeHousesPersonDataEditor
{
    public sealed class EndianBinaryReader : BinaryReader
    {
        private Endianness mEndianness;
        private Dictionary<long, object> mObjectLookup;
        private Stack<long> mBaseOffsetStack;

        public Endianness Endianness
        {
            get => mEndianness;
            set
            {
                SwapBytes = value != EndiannessHelper.SystemEndianness;
                mEndianness = value;
            }
        }

        public string FileName { get; set; }

        public bool SwapBytes { get; private set; }

        public long Position
        {
            get => BaseStream.Position;
            set => BaseStream.Position = value;
        }

        public long Length => BaseStream.Length;

        public long BaseOffset => mBaseOffsetStack.Peek();

        public Encoding Encoding { get; set; }

        public EndianBinaryReader( Stream input, Endianness endianness )
            : base( input )
        {
            FileName = input is FileStream fs ? fs.Name : null;
            Init( Encoding.Default, endianness );
        }

        public EndianBinaryReader( Stream input, string fileName, Endianness endianness )
            : base( input )
        {
            FileName = input is FileStream fs ? fs.Name : fileName;
            Init( Encoding.Default, endianness );
        }

        public EndianBinaryReader( string filepath, Endianness endianness )
            : base( File.OpenRead( filepath ) )
        {
            FileName = filepath;
            Init( Encoding.Default, endianness );
        }

        public EndianBinaryReader( Stream input, Encoding encoding, Endianness endianness )
            : base( input, encoding )
        {
            FileName = input is FileStream fs ? fs.Name : null;
            Init( encoding, endianness );
        }

        public EndianBinaryReader( Stream input, bool leaveOpen, Endianness endianness )
            : base( input, Encoding.Default, leaveOpen )
        {
            FileName = input is FileStream fs ? fs.Name : null;
            Init( Encoding.Default, endianness );
        }

        public EndianBinaryReader( Stream input, Encoding encoding, bool leaveOpen, Endianness endianness )
            : base( input, encoding, leaveOpen )
        {
            FileName = input is FileStream fs ? fs.Name : null;
            Init( encoding, endianness );
        }

        private void Init( Encoding encoding, Endianness endianness )
        {
            Encoding = encoding;
            Endianness = endianness;
            mBaseOffsetStack = new Stack<long>();
            mBaseOffsetStack.Push( 0 );
            mObjectLookup = new Dictionary<long, object> { [0] = null };
        }

        public void Seek( long offset, SeekOrigin origin )
        {
            BaseStream.Seek( offset, origin );
        }

        public void SeekBegin( long offset )
        {
            BaseStream.Seek( offset, SeekOrigin.Begin );
        }

        public void SeekCurrent( long offset )
        {
            BaseStream.Seek( offset, SeekOrigin.Current );
        }

        public void SeekEnd( long offset )
        {
            BaseStream.Seek( offset, SeekOrigin.End );
        }

        public void PushBaseOffset() => mBaseOffsetStack.Push( Position );

        public void PushBaseOffset( long position ) => mBaseOffsetStack.Push( position );

        public void PopBaseOffset() => mBaseOffsetStack.Pop();

        public bool IsValidOffset( int offset )
        {
            if ( offset == 0 )
                return true;

            if ( ( offset % 4 ) != 0 )
                return false;

            var effectiveOffset = offset + BaseOffset;
            return offset >= 0 && effectiveOffset >= 0 && effectiveOffset <= Length;
        }

        public void ReadOffset( Action action )
        {
            var offset = ReadInt32();
            if ( offset != 0 )
            {
                long current = Position;
                SeekBegin( offset + BaseOffset );
                action();
                SeekBegin( current );
            }
        }

        public void ReadOffset( int count, Action<int> action )
        {
            ReadOffset( () =>
            {
                for ( var i = 0; i < count; ++i )
                    action( i );
            } );
        }

        public void ReadAtOffset( long offset, Action action )
        {
            if ( offset == 0 && BaseOffset == 0 )
                return;

            long current = Position;
            SeekBegin( offset + BaseOffset );
            action();
            SeekBegin( current );
        }

        public void ReadAtOffset( long offset, int count, Action<int> action )
        {
            if ( offset == 0 && BaseOffset == 0 )
                return;

            ReadAtOffset( offset, () =>
            {
                for ( var i = 0; i < count; ++i )
                    action( i );
            } );
        }

        public void ReadAtOffset<T>( long offset, int count, List<T> list, object context = null ) where T : IBinarySerializable, new()
        {
            if ( offset == 0 && BaseOffset == 0 )
                return;

            ReadAtOffset( offset, () =>
            {
                for ( var i = 0; i < count; ++i )
                {
                    var item = new T();
                    item.Read( this, context );
                    list.Add( item );
                }
            } );
        }

        public byte ReadByteExpects( byte expected, string message )
        {
            var actual = ReadByte();
            if ( actual != expected )
                throw new InvalidDataException( message );

            return actual;
        }

        public List<byte> ReadByteList( int count )
        {
            var list = new List<byte>( count );
            for ( var i = 0; i < list.Capacity; i++ )
            {
                list.Add( ReadByte() );
            }

            return list;
        }

        public sbyte[] ReadSBytes( int count )
        {
            var array = new sbyte[count];
            for ( var i = 0; i < array.Length; i++ )
                array[i] = ReadSByte();

            return array;
        }

        public bool[] ReadBooleans( int count )
        {
            var array = new bool[count];
            for ( var i = 0; i < array.Length; i++ )
                array[i] = ReadBoolean();

            return array;
        }

        public override short ReadInt16()
        {
            return SwapBytes ? EndiannessHelper.Swap( base.ReadInt16() ) : base.ReadInt16();
        }

        public short ReadInt16Expects( short expected, string message )
        {
            var actual = ReadInt16();
            if ( actual != expected )
                throw new InvalidDataException( message );

            return actual;
        }

        public short[] ReadInt16Array( int count )
        {
            var array = new short[count];
            for ( var i = 0; i < array.Length; i++ )
            {
                array[i] = ReadInt16();
            }

            return array;
        }

        public List<short> ReadInt16List( int count )
        {
            var list = new List<short>( count );
            for ( var i = 0; i < list.Capacity; i++ )
            {
                list.Add( ReadInt16() );
            }

            return list;
        }

        public override ushort ReadUInt16()
        {
            return SwapBytes ? EndiannessHelper.Swap( base.ReadUInt16() ) : base.ReadUInt16();
        }

        public ushort ReadUInt16( ushort expected, string message )
        {
            var actual = ReadUInt16();
            if ( actual != expected )
                throw new InvalidDataException( message );

            return actual;
        }

        public ushort[] ReadUInt16Array( int count )
        {
            var array = new ushort[count];
            for ( var i = 0; i < array.Length; i++ )
            {
                array[i] = ReadUInt16();
            }

            return array;
        }

        public override decimal ReadDecimal()
        {
            return SwapBytes ? EndiannessHelper.Swap( base.ReadDecimal() ) : base.ReadDecimal();
        }

        public decimal[] ReadDecimals( int count )
        {
            var array = new decimal[count];
            for ( var i = 0; i < array.Length; i++ )
            {
                array[i] = ReadDecimal();
            }

            return array;
        }

        public override double ReadDouble()
        {
            return SwapBytes ? EndiannessHelper.Swap( base.ReadDouble() ) : base.ReadDouble();
        }

        public double[] ReadDoubles( int count )
        {
            var array = new double[count];
            for ( var i = 0; i < array.Length; i++ )
            {
                array[i] = ReadDouble();
            }

            return array;
        }

        public override int ReadInt32()
        {
            return SwapBytes ? EndiannessHelper.Swap( base.ReadInt32() ) : base.ReadInt32();
        }

        public int ReadInt32Expects( int expected, string message = "Unexpected value" )
        {
            var actual = ReadInt32();
            if ( actual != expected )
                throw new InvalidDataException( message );

            return actual;
        }

        public int[] ReadInt32s( int count )
        {
            var array = new int[count];
            for ( var i = 0; i < array.Length; i++ )
            {
                array[i] = ReadInt32();
            }

            return array;
        }

        public override long ReadInt64()
        {
            return SwapBytes ? EndiannessHelper.Swap( base.ReadInt64() ) : base.ReadInt64();
        }

        public long[] ReadInt64s( int count )
        {
            var array = new long[count];
            for ( var i = 0; i < array.Length; i++ )
            {
                array[i] = ReadInt64();
            }

            return array;
        }

        public override float ReadSingle()
        {
            return SwapBytes ? EndiannessHelper.Swap( base.ReadSingle() ) : base.ReadSingle();
        }

        public float ReadSingleExpects( float expected, string message )
        {
            var actual = ReadSingle();
            if ( actual != expected )
                throw new InvalidDataException( message );

            return actual;
        }

        public float[] ReadSingleArray( int count )
        {
            var array = new float[count];
            for ( var i = 0; i < array.Length; i++ )
            {
                array[i] = ReadSingle();
            }

            return array;
        }

        public override uint ReadUInt32()
        {
            return SwapBytes ? EndiannessHelper.Swap( base.ReadUInt32() ) : base.ReadUInt32();
        }

        public uint ReadUInt32Expects( uint expected, string message )
        {
            var actual = ReadUInt32();
            if ( actual != expected )
                throw new InvalidDataException( message );

            return actual;
        }

        public uint[] ReadUInt32s( int count )
        {
            var array = new uint[count];
            for ( var i = 0; i < array.Length; i++ )
            {
                array[i] = ReadUInt32();
            }

            return array;
        }

        public Color ReadColor()
        {
            Color color;
            color.R = ReadByte();
            color.G = ReadByte();
            color.B = ReadByte();
            color.A = ReadByte();

            return color;
        }

        public Color[] ReadColors( int count )
        {
            var array = new Color[count];
            for ( var i = 0; i < array.Length; i++ )
                array[i] = ReadColor();

            return array;
        }

        public override ulong ReadUInt64()
        {
            return SwapBytes ? EndiannessHelper.Swap( base.ReadUInt64() ) : base.ReadUInt64();
        }

        public ulong[] ReadUInt64s( int count )
        {
            var array = new ulong[count];
            for ( var i = 0; i < array.Length; i++ )
            {
                array[i] = ReadUInt64();
            }

            return array;
        }

        public string ReadString( StringBinaryFormat format, int fixedLength = -1 )
        {
            var bytes = new List<byte>();

            switch ( format )
            {
                case StringBinaryFormat.NullTerminated:
                    {
                        byte b;
                        while ( ( b = ReadByte() ) != 0 )
                            bytes.Add( b );
                    }
                    break;

                case StringBinaryFormat.FixedLength:
                    {
                        if ( fixedLength == -1 )
                            throw new ArgumentException( "Invalid fixed length specified" );

                        byte b;
                        var terminated = false;
                        for ( var i = 0; i < fixedLength; i++ )
                        {
                            b = ReadByte();
                            if ( b == 0 )
                                terminated = true;

                            if ( !terminated )
                                bytes.Add( b );
                        }
                    }
                    break;

                case StringBinaryFormat.PrefixedLength8:
                    {
                        byte length = ReadByte();
                        for ( var i = 0; i < length; i++ )
                            bytes.Add( ReadByte() );
                    }
                    break;

                case StringBinaryFormat.PrefixedLength16:
                    {
                        ushort length = ReadUInt16();
                        for ( var i = 0; i < length; i++ )
                            bytes.Add( ReadByte() );
                    }
                    break;

                case StringBinaryFormat.PrefixedLength32:
                    {
                        uint length = ReadUInt32();
                        for ( var i = 0; i < length; i++ )
                            bytes.Add( ReadByte() );
                    }
                    break;

                default:
                    throw new ArgumentException( "Unknown string format", nameof( format ) );
            }

            return Encoding.GetString( bytes.ToArray() );
        }

        public string ReadStringAtOffset( long offset, StringBinaryFormat format, int fixedLength = -1 )
        {
            if ( offset == 0 && BaseOffset == 0 )
                return null;

            string str = null;
            ReadAtOffset( offset, () => str = ReadString( format, fixedLength ) );
            return str;
        }

        public string ReadStringOffset( StringBinaryFormat format = StringBinaryFormat.NullTerminated, int fixedLength = -1 )
        {
            var offset = ReadInt32();
            if ( offset == 0 && BaseOffset == 0 )
                return null;

            return ReadStringAtOffset( offset, format, fixedLength );
        }

        public string[] ReadStrings( int count, StringBinaryFormat format, int fixedLength = -1 )
        {
            var value = new string[count];
            for ( var i = 0; i < value.Length; i++ )
                value[i] = ReadString( format, fixedLength );

            return value;
        }

        public string[] ReadStringsAtOffset( long offset, int count, StringBinaryFormat format, int fixedLength = -1 )
        {
            string[] str = null;
            ReadAtOffset( offset, () => str = ReadStrings( count, format, fixedLength ) );
            return str;
        }

        public T ReadObject<T>( object context = null ) where T : IBinarySerializable, new()
        {
            var obj = new T
            {
                SourceInfo = new BinarySourceInfo( FileName, Position, Endianness )
            };

            obj.Read( this, context );
            return obj;
        }

        public List<T> ReadObjectListOffset<T>( int count, object context = null ) where T : IBinarySerializable, new()
        {
            List<T> list = null;
            ReadOffset( () => { list = ReadObjects<T>( count, context ); } );
            return list;
        }

        public List<T> ReadObjects<T>( int count, object context = null ) where T : IBinarySerializable, new()
        {
            var list = new List<T>( count );
            for ( int i = 0; i < count; i++ )
            {
                list.Add( ReadObject<T>( context ) );
            }

            return list;
        }

        public List<T> ReadObjectOffsets<T>( int count, object context = null ) where T : IBinarySerializable, new()
        {
            var list = new List<T>( count );
            for ( int i = 0; i < count; i++ )
            {
                list.Add( ReadObjectOffset<T>( context ) );
            }

            return list;
        }

        /// <summary>
        /// Reads an object of type <typeparamref name="T"/> from the given relative offset if it is not in the object cache.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="offset"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public T ReadObjectAtOffset<T>( long offset, object context = null ) where T : IBinarySerializable, new()
        {
            object obj = null;
            var effectiveOffset = offset + BaseOffset;

            if ( offset != 0 && !mObjectLookup.TryGetValue( effectiveOffset, out obj ) )
            {
                long current = Position;
                SeekBegin( effectiveOffset );
                obj = ReadObject<T>( context );
                SeekBegin( current );
                mObjectLookup[effectiveOffset] = obj;
            }

            return ( T )obj;
        }

        /// <summary>
        /// Reads an object offset from the current stream and reads the object of type <typeparamref name="T"/> at the given offset, if it is not in the object cache.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context"></param>
        /// <returns></returns>
        public T ReadObjectOffset<T>( object context = null ) where T : IBinarySerializable, new()
        {
            var offset = ReadInt32();
            if ( offset == 0 && BaseOffset == 0 )
                return default( T );

            return ReadObjectAtOffset<T>( offset, context );
        }

        public void Align( int i )
        {
            SeekBegin( AlignmentHelper.Align( Position, i ) );
        }
    }
}
