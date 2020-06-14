using UnityEngine;
using UnityEngine.UI;
using ProtoBuf;
using System;

namespace BaseClocks
{
    public enum DigitalClockFormat
    {
        TWELVE_HOUR,
        TWENTY_FOUR_HOUR
    };

    [ProtoContract]
    public class BaseDigitalClock : BaseClock
    {
        public Text Text;
        public Text PeriodText;

        private int m_LastMinute = -1;
        private DigitalClockFormat m_Format;

        private static readonly Vector2 k_TwelveHourTextPosition = new Vector2(0.068f, 0);

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

            PeriodText.enabled = format == DigitalClockFormat.TWELVE_HOUR;

            Text.rectTransform.localPosition = format == DigitalClockFormat.TWELVE_HOUR ? k_TwelveHourTextPosition : Vector2.zero;
            Text.fontSize = format == DigitalClockFormat.TWELVE_HOUR ? 25 : 32;
        }

        public void SetFont(Font font)
        {
            Text.font = font;
            PeriodText.font = font;
        }

        protected override void Update()
        {
            System.TimeSpan time = GetTime();

            if (m_LastMinute != time.Minutes)
            {
                switch (m_Format)
                {
                    case DigitalClockFormat.TWELVE_HOUR:
                        TimeSpan timeSpan = time;
                        bool isMorning;
                        time = DigitalClockUtility.TwentyFourHourToTwelveHourFormat(time, out isMorning);
                        PeriodText.text = isMorning ? "AM" : "PM";
                        break;
                }
                m_LastMinute = time.Minutes;
                Text.text = DigitalClockUtility.EncodeMinHourToString(DigitalClockUtility.EncodeMinuteAndHour(time.Minutes, time.Hours));
            }
        }

        protected override void UseSystemTimeSet(bool newValue)
        {
            m_LastMinute = -1;
        }
    }
}
