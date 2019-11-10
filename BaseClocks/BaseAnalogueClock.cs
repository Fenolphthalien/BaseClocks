using UnityEngine;

namespace BaseClocks
{
    public class BaseAnalogueClock : BaseClock
    {
        public Transform HourHand;
        public Transform MinuteHand;
        public Transform SecondHand;

        const float k_MinuteHandRotByMinuteDeg = 360.0f / 60;
        const float k_HourHandRotByMinuteDeg = (360.0f / 12) / 60;
        const float k_HourHandRotDeg = 360.0f / 12;

        private void OnEnable()
        {
            if (SecondHand != null)
            {
                SecondHand.gameObject.SetActive(UsingSystemTime());
            }
        }

        protected override void Update()
        {
            System.TimeSpan time = GetTime();

            MinuteHand.localEulerAngles = new Vector3(0, 0, time.Minutes * k_MinuteHandRotByMinuteDeg);
            HourHand.localEulerAngles = new Vector3(0, 0, time.Hours * k_HourHandRotDeg + time.Minutes * k_HourHandRotByMinuteDeg);
            SecondHand.localEulerAngles = new Vector3(0, 0, time.Seconds * k_MinuteHandRotByMinuteDeg);
        }

        protected override void UseSystemTimeSet(bool newValue)
        {
            if (SecondHand != null)
            {
                SecondHand.gameObject.SetActive(newValue);
            }
        }
    }
}
