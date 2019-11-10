using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;
using System.IO;

namespace BaseClocks
{
    public class MaterialLogger : MonoBehaviour, IConstructable
    {
        public bool CanDeconstruct(out string reason)
        {
            reason = string.Empty;
            return true;
        }

        public void OnConstructedChanged(bool constructed)
        {
            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string suffex = constructed ? "_OnConstructed.txt" : "_OnDeconstructed.txt";

            string fullPath = string.Concat(desktopPath, "/", name, suffex);

            using (TextWriter tw = File.CreateText(fullPath))
            {
                LogMaterial(gameObject, tw);
                tw.Line();
                LogUBERMaterialProperties(gameObject, tw);
                tw.Close();
            }
        }

        public static void LogMaterialsToDesktop(GameObject gameObject)
        {
            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string suffex = "_Materials.txt";

            string fullPath = string.Concat(desktopPath, "/", gameObject.name, suffex);

            using (TextWriter tw = File.CreateText(fullPath))
            {
                LogMaterial(gameObject, tw);
                tw.Line();
                LogUBERMaterialProperties(gameObject, tw);
                tw.Close();
            }
        }

        private static void LogMaterial(GameObject go, TextWriter textWriter, int indentation = 1)
        {
            textWriter.WriteIndented(go.name.ToString(), indentation);
            textWriter.Write(":\n");

            indentation++;
            Renderer renderer = go.GetComponent<Renderer>();

            if (renderer != null)
            {
                if (renderer.material != null)
                {
                    textWriter.WriteLineIndented(renderer.material.name, indentation);
                    textWriter.WriteLineIndented(renderer.material.shader.name, indentation);
                }
                else
                {
                    textWriter.WriteLineIndented("Material is null.", indentation);
                }
            }
            else
            {
                textWriter.WriteLineIndented("No Renderer attached.", indentation);
            }

            Transform goTransform = go.transform;
            int children = goTransform.childCount;
            for (int i = 0; i < children; i++)
            {
                LogMaterial(goTransform.GetChild(i).gameObject, textWriter, indentation);
            }
        }

        public static void LogUBERMaterialProperties(GameObject gameObject, TextWriter textWriter, int indentation = 1)
        {
            Shader shader = Shader.Find("MarmosetUBER");
            textWriter.WriteIndented(gameObject.name.ToString(), indentation);
            textWriter.Write(":\n");

            indentation++;
            Renderer renderer = gameObject.GetComponent<Renderer>();

            if (renderer != null)
            {
                Material material = renderer.sharedMaterial;
                if (material != null && material.shader == shader)
                {
                    textWriter.WriteIndented("Keywords = ", indentation);
                    foreach (string keyword in material.shaderKeywords)
                    {
                        textWriter.Write(keyword);
                        textWriter.Write(", ");
                    }
                    textWriter.Line();
                    textWriter.Line();

                    textWriter.WriteIndented("_Color = ", indentation);
                    textWriter.WriteLine(material.GetColor("_Color"));

                    float mainMapsEnabled = material.GetFloat("_EnableMainMaps");
                    textWriter.WriteIndented("_EnableMainMaps = ", indentation);
                    textWriter.WriteLine("{0} {1}", mainMapsEnabled, mainMapsEnabled <= 1 ? "Enabled" : "Disabled");

                    Texture mainTex = material.GetTexture("_MainTex");
                    textWriter.WriteIndented("_MainTex = ", indentation);
                    textWriter.WriteLine(mainTex != null ? mainTex.name : "Null");

                    Texture bumpTex = material.GetTexture("_BumpMap");
                    textWriter.WriteIndented("_BumpMap = ", indentation);
                    textWriter.WriteLine(bumpTex != null ? bumpTex.name : "Null");

                    textWriter.WriteIndented("_SpecColor = ", indentation);
                    textWriter.WriteLine(material.GetColor("_SpecColor"));
                    textWriter.WriteIndented("_SpecInt = ", indentation);
                    textWriter.WriteLine(material.GetFloat("_SpecInt"));
                    textWriter.WriteIndented("_Shininess = ", indentation);
                    textWriter.WriteLine(material.GetFloat("_Shininess"));

                    int enumValue = material.GetInt("_MarmoSpecEnum");
                    textWriter.WriteIndented("_MarmoSpecEnum = ", indentation);
                    textWriter.WriteLine("{0} {1}", enumValue, (MarmoSpecEnum)enumValue);

                    float lightingEnabled = material.GetFloat("_EnableLighting");
                    textWriter.WriteIndented("_EnableLighting = ", indentation);
                    textWriter.WriteLine("{0} {1}", lightingEnabled, lightingEnabled <= 1 ? "Enabled" : "Disabled");

                    Texture sigMap = material.GetTexture("_SIGMap");
                    textWriter.WriteIndented("_SIGMap = ", indentation);
                    textWriter.WriteLine(sigMap != null ? sigMap.name : "Null");

                    Texture specTex = material.GetTexture("_SpecTex");
                    textWriter.WriteIndented("_SpecTex = ", indentation);
                    textWriter.WriteLine(specTex != null ? specTex.name : "Null");

                    textWriter.WriteIndented("_Fresnel = ", indentation);
                    textWriter.WriteLine(material.GetFloat("_Fresnel"));

                    float glowEnabled = material.GetFloat("_EnableGlow");
                    textWriter.WriteIndented("_EnableGlow = ", indentation);
                    textWriter.WriteLine("{0} {1}", glowEnabled, glowEnabled <= 1 ? "Enabled" : "Disabled");

                    textWriter.WriteIndented("_GlowStrength = ", indentation);
                    textWriter.WriteLine(material.GetFloat("_GlowStrength"));

                    textWriter.WriteIndented("_GlowStrengthNight = ", indentation);
                    textWriter.WriteLine(material.GetFloat("_GlowStrengthNight"));

                    textWriter.WriteIndented("_EmissionLM = ", indentation);
                    textWriter.WriteLine(material.GetFloat("_EmissionLM"));

                    textWriter.WriteIndented("_EmissionLMNight = ", indentation);
                    textWriter.WriteLine(material.GetFloat("_EmissionLMNight"));

                    Texture illum = material.GetTexture("_Illum");
                    textWriter.WriteIndented("_Illum = ", indentation);
                    textWriter.WriteLine(illum != null ? illum.name : "Null");
                }
                else
                {
                    textWriter.WriteLineIndented(string.Format("Material shader [{0}] is not MarmosetUBER", renderer.sharedMaterial.shader.name), indentation);
                }
            }
            else
            {
                textWriter.WriteLineIndented("No Renderer", indentation);
            }

            Transform transform = gameObject.transform;
            int children = transform.childCount;

            for (int i = 0; i < children; i++)
            {
                LogUBERMaterialProperties(transform.GetChild(i).gameObject, textWriter, indentation);
            }
        }
    }
}
