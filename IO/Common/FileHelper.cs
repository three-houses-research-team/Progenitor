using System.IO;

namespace ThreeHousesPersonDataEditor
{
    public static class FileEx
    {
        /// <summary>
        /// Creates a file. Deletes file if it already exists, creates directory if it doesn't exist.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static FileStream Create( string path )
        {
            path = Path.GetFullPath( path );

            if ( File.Exists( path ) )
                File.Delete( path );
            else if ( Directory.Exists( path ) )
                Directory.Delete( path );

            var directoryName = Path.GetDirectoryName( path );
            if ( directoryName != null )
                Directory.CreateDirectory( directoryName );

            return File.Create( path );
        }
    }
}
