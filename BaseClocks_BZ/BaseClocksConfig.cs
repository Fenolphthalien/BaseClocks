using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using SMLHelper.V2.Json;
using SMLHelper.V2.Options;
using UnityEngine;
using UnityEngine.UI;

namespace BaseClocks
{
    internal class OldBaseClocksConfig
    {
        public bool UseTwelveHourFormat = false;
        public string Color = "default";

        public static readonly Color32 k_DefaultColor = new Color32(115, 255, 252, 255);
        public const string k_OldConfigPath = "./QMods/BaseClocks/config.json";

        public Color GetActualColor()
        {
            string lowercaseColor = Color.ToLower();
            if (ColorUtility.TryParseHtmlString(lowercaseColor, out Color color))
            {
                return color;
            }
            else
            {
                return k_DefaultColor;
            }
        }
    }

    [Serializable]
    internal class BaseClocksConfig
    {
        private static BaseClocksConfig m_Instance;
        public static readonly Color32 k_DefaultColor = new Color32(115, 255, 252, 255);
        private const string k_ConfigPlayerPrefsKey = "BaseClocksConfig";

        [JsonProperty] private DigitalClockFormat m_DigitalClockFormat;
        [JsonProperty] private Color m_ClockFaceColor;

        public static event EventHandler<DigitalClockFormat> OnFormatChanged;
        public static event EventHandler<Color> OnFaceColorChanged;

        private JsonSerializerSettings SerializerSettings = new JsonSerializerSettings()
        {
            ContractResolver = new IgnorePropertiesContractResolver()
        };

        public static DigitalClockFormat DigitalClockFormat
        {
            get
            {
                return m_Instance.m_DigitalClockFormat;
            }
            set
            {
                m_Instance.m_DigitalClockFormat = value;
                OnFormatChanged?.Invoke(m_Instance, value);
            }
        }

        public static Color ClockFaceColor
        {
            get
            {
                return m_Instance?.m_ClockFaceColor ?? k_DefaultColor;
            }
            set
            {
                if (m_Instance.m_ClockFaceColor != value)
                {
                    m_Instance.m_ClockFaceColor = value;
                    OnFaceColorChanged?.Invoke(m_Instance, value);
                }
            }
        }

        public static void Load()
        {
            if (PlayerPrefs.HasKey(k_ConfigPlayerPrefsKey))
            {
                string json = PlayerPrefs.GetString(k_ConfigPlayerPrefsKey, string.Empty);
                JsonSerializerSettings settings = new JsonSerializerSettings();

                m_Instance = JsonConvert.DeserializeObject<BaseClocksConfig>(json);
            }
            else
            {
                //Port the old data
                if (File.Exists(OldBaseClocksConfig.k_OldConfigPath))
                {
                    OldBaseClocksConfig oldConfig = JsonConvert.DeserializeObject<OldBaseClocksConfig>(File.ReadAllText(OldBaseClocksConfig.k_OldConfigPath));
                    if (oldConfig != null)
                    {
                        m_Instance = new BaseClocksConfig();
                        DigitalClockFormat = oldConfig.UseTwelveHourFormat ? DigitalClockFormat.TWELVE_HOUR : DigitalClockFormat.TWENTY_FOUR_HOUR;
                        ClockFaceColor = oldConfig.GetActualColor();

                        File.Delete(OldBaseClocksConfig.k_OldConfigPath);
                        return;
                    }
                }

                m_Instance = new BaseClocksConfig();
                SetToDefaults();
            }

        }

        public static void Save()
        {
           string json = JsonConvert.SerializeObject(m_Instance, m_Instance.SerializerSettings);
           Debug.Log($"[BaseClocks]Serialised: {json}");
           PlayerPrefs.SetString(k_ConfigPlayerPrefsKey, json);
        }

        public static void SetToDefaults()
        {
            DigitalClockFormat = DigitalClockFormat.TWELVE_HOUR;
            ClockFaceColor = k_DefaultColor;
            Save();
        }

        public class IgnorePropertiesContractResolver : DefaultContractResolver
        {
            protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
            {
                var property = base.CreateProperty(member, memberSerialization);
                property.ShouldSerialize = _ => ShouldSerialize(member);
                return property;
            }

            private bool ShouldSerialize(MemberInfo member)
            {
                return member.MemberType != MemberTypes.Property;
            }
        }
    }


    internal class BaseClocksModOptions : ModOptions
    {
        private struct ColorPreset
        {
            public string DisplayName;
            public Color Color;

            public ColorPreset(string displayName, Color color)
            {
                DisplayName = displayName;
                Color = color;
            }
        }

        private static readonly ColorPreset[] m_Presets = new ColorPreset[]
        {
            new ColorPreset("Black", Color.black),
            new ColorPreset("Blue", Color.blue),
            new ColorPreset("Cyan", Color.cyan),
            new ColorPreset("Default", BaseClocksConfig.k_DefaultColor),
            new ColorPreset("Grey", Color.gray),
            new ColorPreset("Green", Color.green),
            new ColorPreset("Magenta", Color.magenta),
            new ColorPreset("Red", Color.red),
            new ColorPreset("White", Color.white),
            new ColorPreset("Yellow", Color.yellow)
        };

        private const string k_DigitalClockFormatChoiceId = "BaseClocksDigitalTimeFormat";
        private const string k_ColorPresetChoiceId = "BaseClocksColorPreset";
        private const string k_ColorSliderRedId = "BaseClocksClockColorR";
        private const string k_ColorSliderGreenId = "BaseClocksClockColorG";
        private const string k_ColorSliderBlueId = "BaseClocksClockColorB";

        private string[] m_DigitalFormatChoiceStrings;
        private string[] m_ColorPresetsChoiceStrings;
        private Dictionary<string, ColorPreset> m_NameToPreset;

        private Dictionary<string, Component> m_IdToControl;
        private bool m_Syncronizing = false;

        public BaseClocksModOptions() : base("Base Clocks")
        {
            ChoiceChanged += OnChoiceChanged;
            SliderChanged += OnSliderChanged;

            DigitalClockFormat[] clockFormats = (DigitalClockFormat[])Enum.GetValues(typeof(DigitalClockFormat));
            m_DigitalFormatChoiceStrings = new string[clockFormats.Length];
            for (int i = 0; i < m_DigitalFormatChoiceStrings.Length; i++)
            {
                m_DigitalFormatChoiceStrings[i] = clockFormats[i].ToDisplayString();
            }

            List<string> colorPresetsChoices = new List<string>();
            m_NameToPreset = new Dictionary<string, ColorPreset>();
            foreach(ColorPreset colorPreset in m_Presets)
            {
                colorPresetsChoices.Add(colorPreset.DisplayName);
                m_NameToPreset[colorPreset.DisplayName] = colorPreset;
                Debug.Log($"[BaseClocks]{colorPreset.DisplayName} added");
            }

            colorPresetsChoices.Add("Custom");
            m_ColorPresetsChoiceStrings = colorPresetsChoices.ToArray();
        }

        private void OnSliderChanged(object sender, SliderChangedEventArgs e)
        {
            if (m_Syncronizing)
            {
                return;
            }

            switch (e.Id)
            {
                case k_ColorSliderRedId:
                    BaseClocksConfig.ClockFaceColor = BaseClocksConfig.ClockFaceColor.SetRed(e.Value);
                    SetPresetChoiceToCustom();
                    break;
                case k_ColorSliderGreenId:
                    BaseClocksConfig.ClockFaceColor = BaseClocksConfig.ClockFaceColor.SetGreen(e.Value);
                    SetPresetChoiceToCustom();
                    break;
                case k_ColorSliderBlueId:
                    BaseClocksConfig.ClockFaceColor = BaseClocksConfig.ClockFaceColor.SetBlue(e.Value);
                    SetPresetChoiceToCustom();
                    break;
            }

            BaseClocksConfig.Save();
        }

        private void OnChoiceChanged(object sender, ChoiceChangedEventArgs e)
        {
            if (m_Syncronizing)
            {
                return;
            }

            switch (e.Id)
            {
                case k_DigitalClockFormatChoiceId:
                    DigitalClockFormat format = (DigitalClockFormat)e.Index;
                    BaseClocksConfig.DigitalClockFormat = format;
                    break;
                case k_ColorPresetChoiceId:
                    if (e.Index < m_ColorPresetsChoiceStrings.Length - 1)
                    {
                        BaseClocksConfig.ClockFaceColor = m_NameToPreset[e.Value].Color;
                        SyncronizeColorBars();
                    }
                    break;
            }

            BaseClocksConfig.Save(); 
        }

        public override void BuildModOptions()
        {
            Debug.Log("[BaseClocks]Building Base Clocks Options");
            Color color = BaseClocksConfig.ClockFaceColor;
            AddChoiceOption(k_DigitalClockFormatChoiceId, "Digital Clock Time Format", m_DigitalFormatChoiceStrings, (int)BaseClocksConfig.DigitalClockFormat);

            Debug.Log("[BaseClocks]Format choice added");
            int presetIndex = Array.FindIndex(m_Presets, x => x.Color == color);
            if (presetIndex == -1)
            {
                presetIndex = m_ColorPresetsChoiceStrings.Length - 1;
            }

            Debug.Log("[BaseClocks]Preset Selected");
            AddChoiceOption(k_ColorPresetChoiceId, "Clock Face Colour Preset", m_ColorPresetsChoiceStrings, presetIndex);

            var defaultColor = BaseClocksConfig.k_DefaultColor;
            AddSliderOption(k_ColorSliderRedId, "Red", 0, 1f, color.r, defaultColor.r, "{0:P0}", 0.01f);
            AddSliderOption(k_ColorSliderGreenId, "Green", 0, 1f, color.g, defaultColor.g, "{0:P0}", 0.1f);
            AddSliderOption(k_ColorSliderBlueId, "Blue", 0, 1f, color.b, defaultColor.b, "{0:P0}", 0.1f);

            Debug.Log("[BaseClocks]Options Added");
        }

        private void SetPresetChoiceToCustom()
        {
            CacheControls();

            m_Syncronizing = true;
            Debug.Log("[BaseClocks]Syncronizing");
            (m_IdToControl[k_ColorPresetChoiceId] as uGUI_Choice).value = m_ColorPresetsChoiceStrings.Length - 1;
            m_Syncronizing = false;
            Debug.Log("[BaseClocks]Syncronizing Finished");
        }

        private void SyncronizeColorBars()
        {
            CacheControls();

            Color clockFace = BaseClocksConfig.ClockFaceColor;
            m_Syncronizing = true;
            (m_IdToControl[k_ColorSliderRedId] as Slider).value = clockFace.r;
            (m_IdToControl[k_ColorSliderGreenId] as Slider).value = clockFace.g;
            (m_IdToControl[k_ColorSliderBlueId] as Slider).value = clockFace.b;

            m_Syncronizing = false;
        }

        private void CacheControls()
        {
            if (m_IdToControl == null)
            {
                m_IdToControl = new Dictionary<string, Component>();
            }
            else
            {
                m_IdToControl.Clear();
            }

            var optionsList = Options;

            m_IdToControl[k_ColorPresetChoiceId] = optionsList.Find(x => x.Id == k_ColorPresetChoiceId).OptionGameObject.GetComponentInChildren<uGUI_Choice>(true);
            m_IdToControl[k_ColorSliderRedId] = optionsList.Find(x => x.Id == k_ColorSliderRedId).OptionGameObject.GetComponentInChildren<Slider>(true);
            m_IdToControl[k_ColorSliderGreenId] = optionsList.Find(x => x.Id == k_ColorSliderGreenId).OptionGameObject.GetComponentInChildren<Slider>(true);
            m_IdToControl[k_ColorSliderBlueId] = optionsList.Find(x => x.Id == k_ColorSliderBlueId).OptionGameObject.GetComponentInChildren<Slider>(true);
        }
    }
}
