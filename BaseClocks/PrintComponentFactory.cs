using UnityEngine;
using UnityEngine.UI;
using System.IO;

namespace BaseClocks
{
    public static class PrintComponentFactory
    {
        public static void PrintComponent(MonoBehaviour monoBehaviour,TextWriter textWriter, int indentation = 1)
        {
            textWriter.WriteLineIndented(monoBehaviour.GetType().ToString(), indentation);

            if (monoBehaviour is Text)
            {
                PrintUIText(monoBehaviour as Text, textWriter, ++indentation);
            }
        }

        private static void PrintUIText(Text text, TextWriter textWriter, int indentation = 1)
        {
            textWriter.WriteIndented("Font = ", indentation);
            textWriter.WriteLine(text.font.name);
        }
    }
}
