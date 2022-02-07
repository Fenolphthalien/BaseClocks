using System.IO;

namespace BaseClocks
{
    public static class TextWriterExtensions
    {
        public static void Line(this TextWriter textWriter)
        {
            textWriter.Write('\n');
        }

        public static void WriteIndented(this TextWriter textWriter, string text, int indentation)
        {
            for (int i = 0; i < indentation; i++)
            {
                textWriter.Write('\t');
            }
            textWriter.Write(text);
        }

        public static void WriteLineIndented(this TextWriter textWriter, string text, int indentation)
        {
            for (int i = 0; i < indentation; i++)
            {
                textWriter.Write('\t');
            }
            textWriter.WriteLine(text);
        }
    }
}
