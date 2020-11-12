namespace ThreeHousesPersonDataEditor
{
    public class BinarySourceInfo
    {
        /// <summary>
        /// Path to the file from which the object was read.
        /// </summary>
        public string FilePath { get; }

        /// <summary>
        /// Offset from which the object was read.
        /// </summary>
        public long Offset { get; }

        /// <summary>
        /// Endianness in which the object was read.
        /// </summary>
        public Endianness Endianness { get; }

        internal BinarySourceInfo( string filePath, long offset, Endianness endianness )
        {
            FilePath   = filePath;
            Offset     = offset;
            Endianness = endianness;
        }
    }
}