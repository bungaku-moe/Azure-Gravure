using UnityEngine;
using UnityEngine.InputSystem;

namespace Kiraio.Azure.Core
{
    [AddComponentMenu("Azure Gravure/Core/Input Manager")]
    public class InputManager : MonoBehaviour
    {
        public InputAction PointerClick { get; private set; }
        public InputAction PointerHold { get; private set; }
        public InputAction ToggleUI { get; private set; }

        [SerializeField]
        InputActionAsset inputSettings;

        void Awake()
        {
            PointerClick = inputSettings.FindActionMap("Character").FindAction("PointerClick");
            PointerHold = inputSettings.FindActionMap("Character").FindAction("PointerHold");
            ToggleUI = inputSettings.FindActionMap("UI").FindAction("ToggleUI");
        }

        void OnEnable()
        {
            inputSettings.Enable();
        }

        void OnDestroy()
        {
            inputSettings.Disable();
        }
    }
}
