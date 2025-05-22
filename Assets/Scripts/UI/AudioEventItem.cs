using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using TMPro;
using UnityEngine;

namespace Kiraio.Azure.UI
{
    [AddComponentMenu("Azure Gravure/UI/Audio Event Item")]
    public class AudioEventItem : MonoBehaviour
    {
        [SerializeField] private TMP_Text m_Time;
        [SerializeField] private TMP_Dropdown m_Audio;

        public float Time
        {
            get => float.Parse(m_Time.text.Trim(), CultureInfo.InvariantCulture);
            set => m_Time.text = value.ToString(CultureInfo.InvariantCulture);
        }

        public List<string> Audio
        {
            get { return m_Audio.options.Select(option => option.text).ToList(); }
            set
            {
                m_Audio.options.Clear();
                foreach (var text in value)
                    m_Audio.options.Add(new TMP_Dropdown.OptionData(text));
                m_Audio.RefreshShownValue(); // Refresh UI
            }
        }
    }
}
