using System.Text;

namespace ThreeHousesPersonDataEditor
{
    public static class EncodingExtensions
    {
        /// <summary>
        /// Calculates the minimum amount of bytes required for a character.
        /// </summary>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public static int GetMinByteCount( this Encoding encoding )
        {
            return encoding.IsSingleByte ? 1 : encoding.GetByteCount( "\0" );
        }
    }
}
