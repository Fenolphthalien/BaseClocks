using System;
using Oculus.Newtonsoft.Json;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Text;
using System.Collections;

namespace BaseClocks
{
    public abstract class BaseClock : HandTarget, IHandTarget, IProtoEventListener, IConstructable
    {
        private bool m_UseSystemTime;

        public const string k_SetSystemTime = "SetSystemTime";
        public const string k_SetGameTime = "SetGameTime";

        public bool UsingSystemTime()
        {
            return m_UseSystemTime;
        }

        private List<Graphic> _Graphics = new List<Graphic>(8);
        public void SetColor(Color color)
        {
            _Graphics.Clear();
            GetComponentsInChildren<Graphic>(true, _Graphics);
            foreach (Graphic graphic in _Graphics)
            {
                graphic.color = color;
            }
        }

        protected virtual void Start()
        {
            SetColor(BaseClocksConfig.ClockFaceColor);
            BaseClocksConfig.OnFaceColorChanged += OnClockFaceColorChanged;
        }

        protected virtual void OnDestroy()
        {
            BaseClocksConfig.OnFaceColorChanged -= OnClockFaceColorChanged;
        }

        private void OnClockFaceColorChanged(object sender, Color e)
        {
            SetColor(e);
        }

        protected abstract void Update();

        protected abstract void UseSystemTimeSet(bool newValue);

        protected TimeSpan GetTime()
        {
            if (m_UseSystemTime)
            {
                DateTime now = DateTime.Now;
                TimeSpan nowSpan = new TimeSpan(now.Hour, now.Minute, now.Second);

                return new TimeSpan(now.Hour, now.Minute, now.Second);
            }
            else
            {
                float dayScalar = DayNightCycle.main.GetDayScalar();
                int hour = Mathf.FloorToInt(dayScalar * 24);
                int minute = Mathf.FloorToInt(Mathf.Repeat(dayScalar * 24 * 60, 60));

                return new TimeSpan(hour, minute, 0);
            }
        }

        public void OnHandClick(GUIHand hand)
        {
            m_UseSystemTime = !m_UseSystemTime;
            UseSystemTimeSet(m_UseSystemTime);
        }

        public void OnHandHover(GUIHand hand)
        {
            if (base.enabled)
            {
                HandReticle main = HandReticle.main;
                main.SetIcon(HandReticle.IconType.Hand, 1f);
                main.SetInteractText(m_UseSystemTime ? k_SetGameTime : k_SetSystemTime);
            }
        }

        [System.Serializable]
        private struct BaseClockSaveData
        {
            public bool usesSystemTime;
        }

        public void OnProtoSerialize(ProtobufSerializer serializer)
        {
            PrefabIdentifier prefabIdentifier = GetComponent<PrefabIdentifier>();
            string slot = SaveLoadManager.main.GetCurrentSlot();
            string fileName = $"{prefabIdentifier.Id}.json";

            slot = Path.Combine(slot, "BaseClocks");
            var userStorage = PlatformUtils.main.GetUserStorage();
            userStorage.CreateContainerAsync(slot);
            BaseClockSaveData saveData = new BaseClockSaveData()
            {
                usesSystemTime = UsingSystemTime()
            };
            string json = JsonConvert.SerializeObject(saveData);
            Dictionary<string, byte[]> saveFiles = new Dictionary<string, byte[]>
            {
                [fileName] = Encoding.ASCII.GetBytes(json)
            };

            userStorage.SaveFilesAsync(slot, saveFiles);
        }

        public void OnProtoDeserialize(ProtobufSerializer serializer)
        {
            PrefabIdentifier prefabIdentifier = GetComponent<PrefabIdentifier>();
            Debug.Log($"Deserializing {prefabIdentifier.Id}");
            string oldDataPath = QPatch.GetOldSaveDirectory();
            oldDataPath = string.Concat(oldDataPath, prefabIdentifier.Id, ".json");
            //Port old data
            if (File.Exists(oldDataPath))
            {
                Debug.Log($"Loading {prefabIdentifier.Id} from old data");
                BaseClockSaveData saveData = JsonConvert.DeserializeObject<BaseClockSaveData>(File.ReadAllText(oldDataPath));
                m_UseSystemTime = saveData.usesSystemTime;
                UseSystemTimeSet(m_UseSystemTime);

                File.Delete(oldDataPath);
                Debug.Log($"Ported {prefabIdentifier.Id}");
            }
            else
            {
                string containerPath = Path.Combine("BaseClocks", $"{prefabIdentifier.Id}.json");
                BaseClocksCoroutineRunner.Instance.StartCoroutine(DeserializeCoroutine(containerPath));
            }
        }

        private IEnumerator DeserializeCoroutine(string path)
        {
            Debug.Log($"trying to load {path}");
            UserStorage userStorage = PlatformUtils.main.GetUserStorage();
            UserStorageUtils.LoadOperation loadOperation = userStorage.LoadFilesAsync(SaveLoadManager.main.GetCurrentSlot(), new List<string>() { path });
            yield return loadOperation;

            if (loadOperation.GetSuccessful())
            {
                string json = Encoding.ASCII.GetString(loadOperation.files[path]);
                BaseClockSaveData saveData = JsonConvert.DeserializeObject<BaseClockSaveData>(json);
                m_UseSystemTime = saveData.usesSystemTime;
                UseSystemTimeSet(m_UseSystemTime);

                Debug.Log($"{path} loaded");
            }
        }

        public void OnConstructedChanged(bool constructed) { }

        public bool CanDeconstruct(out string reason)
        {
            reason = string.Empty;
            return true;
        }
    }
}
