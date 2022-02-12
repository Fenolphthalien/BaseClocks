using ProtoBuf;
using System;
using TMPro;
using UnityEngine;

namespace BaseClocks
{
    [ProtoContract]
    class BaseDigitalClockTMP : BaseClock
    {
        public TextMeshProUGUI Text;

        private int m_LastMinute = -1;
        private DigitalClockFormat m_Format;

        protected override void Start()
        {
            base.Start();
            SetFormat(BaseClocksConfig.DigitalClockFormat);
            BaseClocksConfig.OnFormatChanged += OnFormatChanged;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            BaseClocksConfig.OnFormatChanged -= OnFormatChanged;
        }

        private void OnFormatChanged(object sender, DigitalClockFormat e)
        {
            SetFormat(e);
        }

        public void SetFormat(DigitalClockFormat format)
        {
            m_Format = format;
            Text.fontSize = format == DigitalClockFormat.TWELVE_HOUR ? 25 : 32;
        }

        public void SetFont(TMP_FontAsset font)
        {
            Text.font = font;
        }

        protected override void Update()
        {
            System.TimeSpan time = GetTime();

            if (m_LastMinute != time.Minutes)
            {
                switch (m_Format)
                {
                    case DigitalClockFormat.TWELVE_HOUR:
                        bool isMorning;
                        time = DigitalClockUtility.TwentyFourHourToTwelveHourFormat(time, out isMorning);
                        if (isMorning)
                        {
                            Text.SetText("{0:00}:{1:00}<size=12>AM</size>", time.Hours, time.Minutes);
                        }
                        else
                        {
                            Text.SetText("{0:00}:{1:00}<size=12>PM</size>", time.Hours, time.Minutes);
                        }
                        break;
                    default:
                        Text.SetText("{0:00}:{1:00}", time.Hours, time.Minutes);
                        break;
                }
                m_LastMinute = time.Minutes;
            }
        }

        protected override void UseSystemTimeSet(bool newValue)
        {
            m_LastMinute = -1;
        }
    }
}
