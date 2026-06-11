// -----------------------------------------------------------------------------
// Vena Framework
// Unity game framework layer built on Vena Core.
// Copyright (c) Nan Li.
// Licensed under the terms defined in the repository LICENSE file.
// -----------------------------------------------------------------------------

using UnityEngine;
using System.Collections.Generic;

namespace Vena.Framework
{
    public class CustomToggleGroup : MonoBehaviour
    {
        private List<CustomToggle> mToggleArray;

        public void AddSubToggle(CustomToggle toggle)
        {
            if(mToggleArray == null)
            {
                mToggleArray = new List<CustomToggle>();
            }
            if (!mToggleArray.Contains(toggle))
            {
                mToggleArray.Add(toggle);
            }
        }

        public void DelSubToggle(CustomToggle toggle)
        {
            mToggleArray?.Remove(toggle);
        }

        public void OnToggleClick(CustomToggle toggle)
        {
            if (mToggleArray != null)
            {
                for (int i = 0; i < mToggleArray.Count; i++)
                {
                    CustomToggle __temp = mToggleArray[i];
                    if (__temp != toggle) __temp.OnValueChange(false);
                }
            }
            toggle.OnValueChange(true);
        }
    }
}
