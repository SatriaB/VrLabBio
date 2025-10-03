using System;
using UnityEngine;

namespace FatahDev
{
    public class GameController : MonoBehaviour
    {
        private static GameController _instance;
        public static GameController Instance => _instance;
        [SerializeField] private CharacterController characterController;

        private void Awake()
        {
            _instance = this;
        }

        public void DisableCharacterController()
        {
            if (characterController == null) return;
            
            characterController.enabled = false;
        }

        public void EnableCharacterController()
        {
            if (characterController == null) return;
            
            characterController.enabled = true;
        }
    }
}
