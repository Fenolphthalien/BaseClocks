#if DEBUG
#define kLOG
#define kINCLUDE_TEST_BUILDABLES
#endif

using System.Collections.Generic;
using System.Reflection;
using System.IO;

using Harmony;
using UnityEngine;
using SMLHelper;
using SMLHelper.V2;
using SMLHelper.V2.Handlers;

using UnityEngine.UI;
using Oculus.Newtonsoft.Json;


using System;
using SMLHelper.V2.Crafting;
using SMLHelper.V2.Assets;

namespace BaseClocks
{
    public class QPatch
    {
        public static HarmonyInstance harmony;

        public static string s_ModPath
        {
            get;
            private set;
        }

        private const string k_ClassID = "ActualTimeAnalogueClock";
#if LOG && INCLUDE_TEST_BUILDABLES
        private const string k_ClassID_Materials = "MaterialBalls";
        private const string k_ClassID_TextureTest = "TextureTest";
#endif
        private const string k_ClassID_Digital = "ActualTimeDigitalClock";

        private static readonly string[] s_DefaultKeywords = new string[]
        {
            "MARMO_EMISSION",
            "MARMO_SPECMAP",
        };

        public static void Patch()
        {
            Debug.Log("Patching base clocks");
            QPatch.harmony = HarmonyInstance.Create("com.baseclocks.mod");
            BaseClocksConfig.Load();
            OptionsPanelHandler.RegisterModOptions(new BaseClocksModOptions());

            AssetBundle assetBundle = AssetBundle.LoadFromFile("./QMods/BaseClocks/clocks");

            s_ModPath = "./QMods/BaseClocks";

            GameObject sign = Resources.Load<GameObject>("Submarine/Build/Sign");
            Font signFont = sign.GetComponentInChildren<Text>(true).font;

            Shader marmosetUber = Shader.Find("MarmosetUBER");
            Material marmosetUberMat = new Material(marmosetUber);
#if LOG
            string desktopPath = System.Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string fullpath = string.Concat(desktopPath, "/MarmosetUBERProperties.txt");

            using (System.IO.TextWriter tw = System.IO.File.CreateText(fullpath))
            {
                PrintShaderProperty("_Color", marmosetUberMat, tw);
                PrintShaderProperty("_ReflectColor", marmosetUberMat, tw);
                tw.Line();

                PrintShaderProperty("_Cube", marmosetUberMat, tw);
                PrintShaderProperty("_MainTex", marmosetUberMat, tw);
                tw.Line();

                PrintShaderProperty("_MarmoSpecEnum", marmosetUberMat, tw);
                tw.Line();

                PrintShaderProperty("_Roughness", marmosetUberMat, tw);
                PrintShaderProperty("_Glossiness", marmosetUberMat, tw);
                PrintShaderProperty("_Gloss", marmosetUberMat, tw);
                PrintShaderProperty("_Metal", marmosetUberMat, tw);
                PrintShaderProperty("_Metallic", marmosetUberMat, tw);
                PrintShaderProperty("_Metalness", marmosetUberMat, tw);
                PrintShaderProperty("_Metallicness", marmosetUberMat, tw);
                PrintShaderProperty("_ReflectColor", marmosetUberMat, tw);
                PrintShaderProperty("_Reflectivity", marmosetUberMat, tw);
                tw.Line();

                PrintShaderProperty("_Spec", marmosetUberMat, tw);
                PrintShaderProperty("_SpecTex", marmosetUberMat, tw);
                PrintShaderProperty("_SpecColor", marmosetUberMat, tw);
                PrintShaderProperty("_SpecColor2", marmosetUberMat, tw);
                PrintShaderProperty("_SpecColor3", marmosetUberMat, tw);
                PrintShaderProperty("_SpecCubeIBL", marmosetUberMat, tw);
                PrintShaderProperty("_SpecInt", marmosetUberMat, tw);
                PrintShaderProperty("_SpecGlossMap", marmosetUberMat, tw);
                PrintShaderProperty("_Specular", marmosetUberMat, tw);
                PrintShaderProperty("_Shininess", marmosetUberMat, tw);
                PrintShaderProperty("_SpecularAmount", marmosetUberMat, tw);
                tw.Line();

                PrintShaderProperty("_EnableGlow", marmosetUberMat, tw);
                PrintShaderProperty("_SIGMap", marmosetUberMat, tw);

                PrintShaderProperty("_AffectedByDayNightCycle", marmosetUberMat, tw);
                tw.Line();

                PrintShaderProperty("_SelfIllumination", marmosetUberMat, tw);
                PrintShaderProperty("_EnableGlow", marmosetUberMat, tw);
                PrintShaderProperty("_GlowColor", marmosetUberMat, tw);
                PrintShaderProperty("_Illum", marmosetUberMat, tw);
                PrintShaderProperty("_GlowStrength", marmosetUberMat, tw);
                PrintShaderProperty("_GlowStrengthNight", marmosetUberMat, tw);

                tw.Line();

                PrintShaderProperty("_Fresnel", marmosetUberMat, tw);
                PrintShaderProperty("_FresnelFade", marmosetUberMat, tw);

                tw.Line();

                PrintShaderProperty("_BaseLight", marmosetUberMat, tw);
                PrintShaderProperty("_AO", marmosetUberMat, tw);
                tw.Close();
            }
#endif

            //Analogue clock
            GameObject analogueBaseClock = assetBundle.LoadAsset<GameObject>("Actual Time Analog Clock UGUI");

            SMLHelper.V2.Utility.PrefabUtils.AddBasicComponents(ref analogueBaseClock, k_ClassID);

            ReplaceMaterialShader(analogueBaseClock, marmosetUber, true, true);

            ApplySkyApplier(analogueBaseClock);

            Constructable constructable = analogueBaseClock.AddComponent<Constructable>();

            constructable.allowedOnWall = true;
            constructable.allowedInSub = true;
            constructable.allowedOnGround = false;
            constructable.allowedOutside = false;
            constructable.model = analogueBaseClock.transform.GetChild(0).gameObject;

            DestroyPhysicsComponents(analogueBaseClock);

            ConstructableBounds constructableBounds = analogueBaseClock.AddComponent<ConstructableBounds>();

            TechTag techTag = analogueBaseClock.AddComponent<TechTag>();
            BaseAnalogueClock actualTimeAnalogueClock = analogueBaseClock.AddComponent<BaseAnalogueClock>();

            actualTimeAnalogueClock.HourHand = analogueBaseClock.transform.GetChild(1).GetChild(1);
            actualTimeAnalogueClock.MinuteHand = analogueBaseClock.transform.GetChild(1).GetChild(2);
            actualTimeAnalogueClock.SecondHand = analogueBaseClock.transform.GetChild(1).GetChild(3);

            TechData techData = new TechData();
            techData.Ingredients.Add(new Ingredient(TechType.Titanium, 1));
            techData.Ingredients.Add(new Ingredient(TechType.CopperWire, 1));

            BaseClockBuildable analogueClockBuildable = new BaseClockBuildable(k_ClassID, "Analogue Clock", "An Analogue clock.", "analogueClock.png", analogueBaseClock.gameObject, techData);
            analogueClockBuildable.Patch();
            Debug.Log("Patched analogueClockBuildable");

            //Digital clock
            GameObject digitalBaseClock = assetBundle.LoadAsset<GameObject>("Actual Time Digital Clock UGUI");

            SMLHelper.V2.Utility.PrefabUtils.AddBasicComponents(ref digitalBaseClock, k_ClassID_Digital);

            ReplaceMaterialShader(digitalBaseClock, marmosetUber);

            ApplySkyApplier(digitalBaseClock);

            constructable = digitalBaseClock.AddComponent<Constructable>();

            constructable.allowedOnWall = true;
            constructable.allowedInSub = true;
            constructable.allowedOnGround = false;
            constructable.allowedOutside = false;
            constructable.model = digitalBaseClock.transform.GetChild(0).gameObject;

            DestroyPhysicsComponents(digitalBaseClock);

            constructableBounds = digitalBaseClock.AddComponent<ConstructableBounds>();

            techTag = digitalBaseClock.AddComponent<TechTag>();

            BaseDigitalClock digitalClock = digitalBaseClock.AddComponent<BaseDigitalClock>();
            digitalClock.Text = digitalBaseClock.transform.GetChild(1).GetChild(0).GetComponent<Text>();
            digitalClock.PeriodText = digitalBaseClock.transform.GetChild(1).GetChild(1).GetComponent<Text>();
            digitalClock.SetFont(signFont);

            techData = new TechData();
            techData.Ingredients.Add(new Ingredient(TechType.Titanium, 1));
            techData.Ingredients.Add(new Ingredient(TechType.CopperWire, 1));

            LanguageHandler.SetLanguageLine(BaseClock.k_SetGameTime, "Set to Normal Time");
            LanguageHandler.SetLanguageLine(BaseClock.k_SetSystemTime, "Set to System Time");

            BaseClockBuildable digitalClockBuildable = new BaseClockBuildable(k_ClassID_Digital, "Digital Clock", "A Digital clock.", "digitalClock.png", digitalClock.gameObject, techData);
            digitalClockBuildable.Patch();
            Debug.Log("Patched digitalClockBuildable");

#if INCLUDE_TEST_BUILDABLES
            //Material balls.
            techType = TechTypePatcher.AddTechType(k_ClassID_Materials, "Material Balls", "Material Test");

            GameObject materialBalls = assetBundle.LoadAsset<GameObject>("Material Balls");

            Utility.AddBasicComponents(ref materialBalls, k_ClassID_Materials);
            DestroyPhysicsComponents(materialBalls);

            constructable = materialBalls.AddComponent<Constructable>();

            constructable.allowedOnWall = false;
            constructable.allowedInSub = true;
            constructable.allowedOnGround = true;
            constructable.allowedOnCeiling = false;
            constructable.allowedOnConstructables = false;
            constructable.allowedOutside = true;
            constructable.model = materialBalls.transform.GetChild(0).gameObject;

            constructable.name = "Material Balls";

            constructableBounds = materialBalls.AddComponent<ConstructableBounds>();

            techTag = materialBalls.AddComponent<TechTag>();
            techTag.type = techType;

            ReplaceMaterialShader(materialBalls, marmosetUber, false, true);
            AddSkyApplier(materialBalls);

            materialBalls.AddComponent<MaterialLogger>();

            CustomPrefabHandler.customPrefabs.Add(new CustomPrefab(k_ClassID_Materials, "Submarine/Build/MaterialBalls", materialBalls, techType));

            techDataHelper = new TechDataHelper();
            techDataHelper._ingredients = new List<IngredientHelper>();
            techDataHelper._ingredients.Add(new IngredientHelper(TechType.Titanium, 1));
            techDataHelper._techType = techType;

            CraftDataPatcher.customTechData.Add(techType, techDataHelper);
            CraftDataPatcher.customBuildables.Add(techType);

            dictionary[TechGroup.InteriorModules][TechCategory.InteriorModule].Add(techType);

            //Texture test.
            techType = TechTypePatcher.AddTechType(k_ClassID_TextureTest, "Texture Test", "Texture Test");

            GameObject textureTest = assetBundle.LoadAsset<GameObject>("Texture Test");

            Utility.AddBasicComponents(ref textureTest, k_ClassID_TextureTest);
            DestroyPhysicsComponents(textureTest);

            constructable = textureTest.AddComponent<Constructable>();

            constructable.allowedOnWall = true;
            constructable.allowedInSub = true;
            constructable.allowedOnGround = false;
            constructable.allowedOnCeiling = false;
            constructable.allowedOnConstructables = false;
            constructable.allowedOutside = false;
            constructable.model = textureTest.transform.GetChild(0).gameObject;

            constructable.name = "Texture Test";

            constructableBounds = textureTest.AddComponent<ConstructableBounds>();

            techTag = textureTest.AddComponent<TechTag>();
            techTag.type = techType;

            ReplaceMaterialShader(textureTest, marmosetUber, true, true);

            AddSkyApplier(textureTest);

            textureTest.AddComponent<MaterialLogger>();

            CustomPrefabHandler.customPrefabs.Add(new CustomPrefab(k_ClassID_TextureTest, "Submarine/Build/TextureTest", textureTest, techType));

            techDataHelper = new TechDataHelper();
            techDataHelper._ingredients = new List<IngredientHelper>();
            techDataHelper._ingredients.Add(new IngredientHelper(TechType.Titanium, 1));
            techDataHelper._techType = techType;

            CraftDataPatcher.customTechData.Add(techType, techDataHelper);
            CraftDataPatcher.customBuildables.Add(techType);

            dictionary[TechGroup.InteriorModules][TechCategory.InteriorModule].Add(techType);
#endif
#if LOG
            //Print small locker objects and components to desktop.
            fullpath = string.Concat(desktopPath, "/FabricatorComponents.txt");

            GameObject fabricator = Resources.Load<GameObject>("Submarine/Build/Fabricator");
            GameObject medicalCabinet = Resources.Load<GameObject>("Submarine/Build/MedicalCabinet");

            if (fabricator != null)
            {
                using (System.IO.TextWriter tw = System.IO.File.CreateText(fullpath))
                {
                    PrintComponents(fabricator, tw);
                    tw.Close();
                }
                MaterialLogger.LogMaterialsToDesktop(fabricator);
            }

            fullpath = string.Concat(desktopPath, "/SignComponents.txt");

            if (sign != null)
            {
                using (System.IO.TextWriter tw = System.IO.File.CreateText(fullpath))
                {
                    PrintComponents(sign, tw);
                    tw.Close();
                }
            }

            fullpath = string.Concat(desktopPath, "/MedicalCabinetComponents.txt");
            if (medicalCabinet != null)
            {
                using (System.IO.TextWriter tw = System.IO.File.CreateText(fullpath))
                {
                    PrintComponents(medicalCabinet, tw);
                    tw.Close();
                }
                MaterialLogger.LogMaterialsToDesktop(medicalCabinet);
            }

            MaterialLogger.LogMaterialsToDesktop(analogueBaseClock);


            Resources.UnloadAsset(sign);
            Resources.UnloadAsset(fabricator);
            Resources.UnloadAsset(medicalCabinet);
            MonoBehaviour.Destroy(marmosetUberMat);
#endif
        }

        public static string GetOldSaveDirectory()
        {
            return string.Concat(s_ModPath, "/BaseClocks/");
        }

        private static void ApplySkyApplier(GameObject gameObject)
        {
            SkyApplier skyApplier = gameObject.GetComponent<SkyApplier>();
            skyApplier.renderers = gameObject.GetComponentsInChildren<Renderer>();
        }

        private static List<Renderer> _Renderers = new List<Renderer>(32);
        private static void ReplaceMaterialShader(GameObject gameObject, Shader shader, bool emmisive = false, bool specular = false)
        {
            _Renderers.Clear();
            gameObject.GetComponentsInChildren<Renderer>(_Renderers);

            foreach (Renderer renderer in _Renderers)
            {
                renderer.sharedMaterial.shader = shader;

                if (emmisive)
                {
                    renderer.sharedMaterial.EnableKeyword("MARMO_EMISSION");
                }
                if (specular)
                {
                    renderer.sharedMaterial.EnableKeyword("MARMO_SPECMAP");
                }
            }
        }

        private static void DestroyPhysicsComponents(GameObject gameObject)
        {
            Rigidbody rigidbody = gameObject.GetComponent<Rigidbody>();
            WorldForces worldForces = gameObject.GetComponent<WorldForces>();
            GameObject.DestroyImmediate(rigidbody);
            GameObject.DestroyImmediate(worldForces);
        }

#if LOG
        private static void PrintShaderProperty(string property,Material material, System.IO.TextWriter textWriter)
        {
            textWriter.Write("Has ");
            textWriter.Write(property);
            textWriter.Write(" = ");

            bool hasProperty = material.HasProperty(property);

            textWriter.WriteLine(hasProperty);
        }

        static  List<MonoBehaviour> _MonoBehaviours = new List<MonoBehaviour>(32);
        static void PrintComponents(GameObject gameObject, System.IO.TextWriter textWriter, int indentation = 1)
        {
            _MonoBehaviours.Clear();
            gameObject.GetComponents<MonoBehaviour>(_MonoBehaviours);

            textWriter.WriteIndented(gameObject.name, indentation);
            textWriter.Write(":\n");

            indentation++;
            foreach (MonoBehaviour monoBehaviour in _MonoBehaviours)
            {
                PrintComponentFactory.PrintComponent(monoBehaviour, textWriter, indentation);
            }

            Transform transform = gameObject.transform;
            int children = transform.childCount;

            for (int i = 0; i < children; i++)
            {
                PrintComponents(transform.GetChild(i).gameObject, textWriter, indentation);
            }
        }
#endif
    }

    internal class BaseClockBuildable : Buildable
    {
        private GameObject m_Prefab;
        private TechData m_TechData;
        private string m_IconName;

        public BaseClockBuildable(string classId, string displayName, string description, string iconName, GameObject prefab, TechData techData)
            :base (classId, displayName, description)
        {
            m_Prefab = prefab;
            m_TechData = techData;
            m_IconName = iconName;
        }

        public override TechGroup GroupForPDA => TechGroup.Miscellaneous;

        public override TechCategory CategoryForPDA => TechCategory.Misc;

        public override string AssetsFolder => "/BaseClocks/Assets/";

        public override string IconFileName => m_IconName;

        public override TechType RequiredForUnlock => TechType.None;

        public override GameObject GetGameObject()
        {
            return m_Prefab;
        }

        protected override TechData GetBlueprintRecipe()
        {
            return m_TechData;
        }
    }
}
