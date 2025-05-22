using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Kiraio.Azure.Components;
using Kiraio.Azure.Serialization;
using Kiraio.Azure.UI;
using Kiraio.Azure.Utils;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;

namespace Kiraio.Azure.Core
{
    [AddComponentMenu("Main Control")]
    public class MainControl : MonoBehaviour
    {
        [Header("Prefabs")] [SerializeField] private MotionEventUI m_MotionEventPrefab;
        [SerializeField] private AudioEventItem m_AudioEventPrefab;

        [Header("UI")] [SerializeField] private TMP_InputField m_ModelJsonField;
        [SerializeField] private TMP_InputField m_VoiceDirectoryField;

        [Space] [SerializeField] private RectTransform m_VoiceSetupContent;

        [Space] [SerializeField] private string m_ShipDataPath;
        [SerializeField] private string m_SkinDataPath;
        [SerializeField] private string m_VoiceDataPath;

        private CharacterViewer _currentCharacter;

        public Dictionary<int, ShipData> ShipData { get; private set; } = new();

        public List<SkinData> SkinData { get; private set; } = new();

        public Dictionary<string, Dictionary<string, VoiceData>> VoiceData { get; private set; } = new();

        private void Awake()
        {
            Initialize().ToAsyncLazy();
        }

        private void Start()
        {
            // if (m_VoiceData.TryGetValue("100000", out var characterLines) &&
            //     characterLines.TryGetValue("battle", out var battleLine))
            // {
            //     Debug.Log($"Battle voice line: {battleLine.Line}");
            //     Debug.Log($"Link: {battleLine.Link}");
            // }
        }

        private async UniTask Initialize()
        {
            try
            {
                //! Censored version file is postfixes with _hx.
                Debug.Log("Loading databases...");
                ShipData =
                    JsonConvert.DeserializeObject<Dictionary<int, ShipData>>(
                        await WebRequestHelper.GetTextDataAsync(StorageHelper.NormalizePath(m_ShipDataPath)));
                // Populate only the skins with "live2d" tag
                var skinDataList =
                    JsonConvert.DeserializeObject<List<SkinData>>(
                        await WebRequestHelper.GetTextDataAsync(StorageHelper.NormalizePath(m_SkinDataPath)));
                SkinData = (from skinData in skinDataList
                    from skinItem in skinData.Skins
                    from tagItem in skinItem.Tag
                    where tagItem == "live2d"
                    select skinData).ToList();
                VoiceData = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, VoiceData>>>(
                    await WebRequestHelper.GetTextDataAsync(StorageHelper.NormalizePath(m_VoiceDataPath)));

                _currentCharacter =
                    (CharacterViewer)FindFirstObjectByType(typeof(CharacterViewer), FindObjectsInactive.Include);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to load databases: {ex.Message}");
            }
        }

        private CharacterViewer InstantiateViewer()
        {
            return new GameObject("CharacterViewer").AddComponent<CharacterViewer>();
        }

        public async void OpenModelJson(TMP_InputField inputField)
        {
            try
            {
                var result = await StorageHelper.OpenFileDialogAsync("Load Model File", false, new[] { ".model3.json" })
                    .AsValueTask();
                inputField.text = result.Length > 0 ? result[0] : inputField.text;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to open model JSON: {ex.Message}");
            }
        }

        public async void OpenVoiceDirectory(TMP_InputField inputField)
        {
            try
            {
                var result = await StorageHelper.OpenDirectoryDialogAsync("Load Voice Folder", false,
                    new[] { ".ogg", ".mp3", ".wav" });
                inputField.text = result.Length > 0 ? result[0] : inputField.text;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to open Voices directory: {ex.Message}");
            }
        }

        #region Apply, Save & Load Settings

        public async void ApplySettings()
        {
            if (_currentCharacter != null)
                Destroy(_currentCharacter);

            _currentCharacter = InstantiateViewer();
            _currentCharacter.ModelJsonFile = m_ModelJsonField.text;
            _currentCharacter.VoicesDirectory = m_VoiceDirectoryField.text;

            // Await the async initialization
            await _currentCharacter.Initialize();

            // Setup motion event UI after initialization
            foreach (var characterAnimation in _currentCharacter.Animations)
            {
                var motionEventUI = Instantiate(m_MotionEventPrefab, m_VoiceSetupContent.transform);
                motionEventUI.MotionName = characterAnimation.Key;

                var animationEvents = characterAnimation.Value.events;
                foreach (var animationEvent in animationEvents)
                {
                    var audioEventItem =
                        Instantiate(m_AudioEventPrefab, motionEventUI.EventListContainer.transform);
                    audioEventItem.Time = animationEvent.time;

                    // Populate audio dropdown options
                    audioEventItem.Audio = _currentCharacter.Voices.Select(voice => voice.Key).ToList();
                }

                motionEventUI.transform.SetParent(m_VoiceSetupContent.transform, false);
            }
        }

        public void SaveSettings()
        {
        }

        public void LoadSettings()
        {
        }

        #endregion
    }
}
