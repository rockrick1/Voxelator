using StarterAssets;
using UnityEngine;

namespace World
{
    public class WorldController : MonoBehaviour
    {
        [SerializeField] ThirdPersonController _playerController;

        public ThirdPersonController PlayerController => _playerController;

        public static WorldController Instance;

        void Awake()
        {
            if (Instance == null) Instance = this;
        }
    }
}