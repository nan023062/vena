// -----------------------------------------------------------------------------
// Vena Framework
// Unity game framework layer built on Vena Core.
// Copyright (c) Nan Li.
// Licensed under the terms defined in the repository LICENSE file.
// -----------------------------------------------------------------------------

using UnityEngine;
using UnityEngine.EventSystems;

namespace Vena.Framework
{
    public delegate void OnToggleChange(bool isOn);
    
    public delegate bool CheckSwapToggle();
    
    public class CustomToggle : MonoBehaviour, IPointerClickHandler
    {
        public bool isOn = false;
        public bool isLocked = false;
        public event OnToggleChange onToggleChange;
        public event CheckSwapToggle checkSwapToggle;
        [SerializeField] CustomToggleGroup group;
        public GameObject isOnObject;
        public GameObject[] isOnObjects;
        public GameObject lockedObject;
        
        [ExecuteInEditMode]
        private void Awake()
        {
            // ReSharper disable once Unity.NoNullPropagation
            group?.AddSubToggle(this);
            SetLocked(isLocked);
        }

        public void SetGroup(CustomToggleGroup group)
        {
            if (this.group != group)
            {
                this.group?.DelSubToggle(this);
                this.group = group;
                this.group?.AddSubToggle(this);
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (isLocked) return;
            
            if (checkSwapToggle == null || checkSwapToggle())
            {
                DoClicked();
            }
        }
        
        public void DoClicked()
        {
            if (group != null) group.OnToggleClick(this);
            else OnValueChange(true);
        }

        public void OnValueChange(bool isOn)
        {
            if (!isLocked)
            {
                this.isOn = isOn;
                onToggleChange?.Invoke(isOn);
                SetIsOnObject(isOn);
            }
        }

        private void SetIsOnObject(bool isOn)
        {
            if(isOnObjects != null)
            {
                foreach (var obj in isOnObjects)
                {
                    if(obj != null) obj.SetActive(isOn);
                }
            }
            if(isOnObject!=null)
                isOnObject.SetActive(isOn);
        }

        public void SetOn(bool isOn)
        {
            this.isOn = isOn;
            SetIsOnObject(isOn);
        }

        public void SetLocked(bool locked)
        {
            isLocked = locked;
            if(lockedObject!=null)
                lockedObject.SetActive(isLocked);
        }

        [ExecuteInEditMode]
        private void OnValidate()
        {
            if (!isLocked) OnValueChange(isOn);
            SetLocked(isLocked);
        }
    }
}
