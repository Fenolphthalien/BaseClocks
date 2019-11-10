using System;
using System.IO;
using Oculus.Newtonsoft.Json;
using SMLHelper.V2.Options;
using SMLHelper.V2.Utility;
using UnityEngine;

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

        public DigitalClockFormat m_DigitalClockFormat;
        public Color m_ClockFaceColor;

        public static event EventHandler<DigitalClockFormat> OnFormatChanged;
        public static event EventHandler<Color> OnFaceColorChanged;

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
                return m_Instance.m_ClockFaceColor;
            }
            set
            {
                m_Instance.m_ClockFaceColor = value;
                OnFaceColorChanged?.Invoke(m_Instance, value);
            }
        }

        public static void Load()
        {
            if (PlayerPrefs.HasKey(k_ConfigPlayerPrefsKey))
            {
                string json = PlayerPrefs.GetString(k_ConfigPlayerPrefsKey, string.Empty);
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
           string json = JsonConvert.SerializeObject(m_Instance);
           PlayerPrefs.SetString(k_ConfigPlayerPrefsKey, json);
        }

        public static void SetToDefaults()
        {
            DigitalClockFormat = DigitalClockFormat.TWELVE_HOUR;
            ClockFaceColor = k_DefaultColor;
        }
    }


    internal class BaseClocksModOptions : ModOptions
    {
        private const string k_DigitalClockFormatChoiceId = "BaseClocksDigitalTimeFormat";
        private const string k_ColorSliderRedId = "BaseClocksClockColorR";
        private const string k_ColorSliderGreenId = "BaseClocksClockColorG";
        private const string k_ColorSliderBlueId = "BaseClocksClockColorB";

        public BaseClocksModOptions() : base("Base Clocks")
        {
            ChoiceChanged += OnChoiceChanged;
            SliderChanged += OnSlinderChanged;
        }

        private void OnSlinderChanged(object sender, SliderChangedEventArgs e)
        {
            switch (e.Id)
            {
                case k_ColorSliderRedId:
                    BaseClocksConfig.ClockFaceColor = BaseClocksConfig.ClockFaceColor.SetRed(e.Value);
                    break;
                case k_ColorSliderGreenId:
                    BaseClocksConfig.ClockFaceColor = BaseClocksConfig.ClockFaceColor.SetGreen(e.Value);
                    break;
                case k_ColorSliderBlueId:
                    BaseClocksConfig.ClockFaceColor = BaseClocksConfig.ClockFaceColor.SetBlue(e.Value);
                    break;
            }
        }

        private void OnChoiceChanged(object sender, ChoiceChangedEventArgs e)
        {
            switch (e.Id)
            {
                case k_DigitalClockFormatChoiceId:
                    DigitalClockFormat format = (DigitalClockFormat)e.Index;
                    BaseClocksConfig.DigitalClockFormat = format;
                    break;
            }
        }

        public override void BuildModOptions()
        {
            DigitalClockFormat[] clockFormats = (DigitalClockFormat[])Enum.GetValues(typeof(DigitalClockFormat));
            string[] digitalFormatStrings = new string[clockFormats.Length];
            for (int i = 0; i < digitalFormatStrings.Length; i++)
            {
                digitalFormatStrings[i] = clockFormats[i].ToDisplayString();
            }

            Color color = BaseClocksConfig.ClockFaceColor;
            AddChoiceOption(k_DigitalClockFormatChoiceId, "Digital Clock Time Format", digitalFormatStrings, (int)BaseClocksConfig.DigitalClockFormat);
            AddSliderOption(k_ColorSliderRedId, "Clock Face Colour Red", 0, 1f, color.r);
            AddSliderOption(k_ColorSliderGreenId, "Clock Face Colour Green", 0, 1f, color.g);
            AddSliderOption(k_ColorSliderBlueId, "Clock Face Colour Blue", 0, 1f, color.b);
        }
    }
}
