using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Readers;

namespace FatahDev
{
    public class AnimatedHandOnInput : MonoBehaviour
    {
        [Header("Trigger")]
        [SerializeField]
        XRInputValueReader<float> m_TriggerInput = new XRInputValueReader<float>("Trigger");

        [Header("Grip")]
        [SerializeField]
        XRInputValueReader<float> m_GripInput = new XRInputValueReader<float>("Grip");
        
        private Animator m_Animator;

        private void OnEnable()
        {
            m_Animator =  GetComponent<Animator>();
        }

        private void OnDisable()
        {
            m_Animator = null;
        }

        private void Update()
        {
            if(m_Animator == null)  return;
            
            float triggerValue = m_TriggerInput.ReadValue();
            m_Animator.SetFloat("Trigger", triggerValue);
            
            float gripValue = m_GripInput.ReadValue();
            m_Animator.SetFloat("Grip", gripValue);
        }
    }
}
