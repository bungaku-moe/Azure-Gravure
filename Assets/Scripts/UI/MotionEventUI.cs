using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace Kiraio.Azure.UI
{
    [AddComponentMenu("Azure Gravure/UI/Motion Event")]
    public class MotionEventUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text m_MotionName;
        [SerializeField] private RectTransform m_EventListContainer;

        public string MotionName
        {
            get => m_MotionName.text;
            set => m_MotionName.text = value;
        }

        public RectTransform EventListContainer => m_EventListContainer;

        public List<AudioEventItem> AudioEvents =>
            EventListContainer.GetComponentsInChildren<AudioEventItem>().ToList();
    }
}
