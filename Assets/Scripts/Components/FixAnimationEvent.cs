using UnityEngine;

namespace Kiraio.Azure.Components
{
    [AddComponentMenu("")]
    public class FixAnimationEvent : MonoBehaviour
    {
        public CharacterViewer Viewer { get; set; }

        protected void InstanceId()
        {
            // there's nothing interesting here...
        }

        protected void PlayVoice(string voiceName)
        {
            Debug.Log($"Play voice: {voiceName}");
            Viewer.PlayVoice(voiceName);
        }
    }
}
