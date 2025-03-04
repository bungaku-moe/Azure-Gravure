using UnityEngine;
using UnityEngine.InputSystem;

namespace Kiraio.Azure.Core
{
    [AddComponentMenu("Azure Gravure/Core/Input Manager")]
    public class InputManager : MonoBehaviour
    {
        [SerializeField] private InputActionAsset inputSettings;
        public InputAction PointerClick { get; private set; }
        public InputAction PointerHold { get; private set; }
        public InputAction ToggleUI { get; private set; }

        private void Awake()
        {
            PointerClick = inputSettings.FindActionMap("Character").FindAction("PointerClick");
            PointerHold = inputSettings.FindActionMap("Character").FindAction("PointerHold");
            ToggleUI = inputSettings.FindActionMap("UI").FindAction("ToggleUI");
        }

        private void OnEnable()
        {
            inputSettings.Enable();
        }

        private void OnDestroy()
        {
            inputSettings.Disable();
        }
    }
}
