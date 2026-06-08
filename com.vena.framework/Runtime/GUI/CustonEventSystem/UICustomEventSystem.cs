using UnityEngine;
using System.Collections.Generic;
using UnityEngine.EventSystems;

namespace Vena.Framework
{
    [RequireComponent(typeof(UIRoot))]
    [RequireComponent(typeof(EventSystem))]
    [RequireComponent(typeof(StandaloneInputModule))]
    public class UICustomEventSystem : MonoBehaviour
    {
        enum TouchState
        {
            Normal,
            Press,
            Dragging,
        }

        public static UICustomEventSystem current => _sCurrent;
        
        private static UICustomEventSystem _sCurrent;
        
        private static List<UICustomEvent> _customEventLst;
        
        public static void AddUIEvent(UICustomEvent e)
        {
            _customEventLst ??= new List<UICustomEvent>();
            _customEventLst.Add(e);
        }
        public static void RemoveUIEvent(UICustomEvent e)
        {
            _customEventLst?.Remove(e);
        }

        private TouchState _touchState = TouchState.Normal;

        private UIRoot _uiRoot;
        public UIRoot UIRoot => _uiRoot;
        
        private UICustomEvent _currentEvent;
        
        private EventSystem _eventSystem;
        
        private Vector3 _lastPressPoint = Vector3.zero;
        
        private Vector3 _currentPoint = Vector3.zero;

        private void Awake()
        {
            _sCurrent = this;
            
            _uiRoot = GetComponent<UIRoot>();
            
            _eventSystem = GetComponent<EventSystem>();
            
            if (_customEventLst == null) _customEventLst = new List<UICustomEvent>();
        }

        private void Update()
        {
            if (_touchState == TouchState.Normal)
            {
                if (IsTouchdown(ref _currentPoint))
                {
                    GameObject go = null;
                    _currentEvent = RaycastUIObject(_currentPoint, ref go);
                    foreach (var eventObj in _customEventLst)
                    {
                        if (go != eventObj.go) 
                            eventObj.OnPressOther(go);
                    }
                    if (_currentEvent != null)
                    {
                        _currentEvent.OnPressed(_currentPoint);
                        _touchState = TouchState.Press;
                        _lastPressPoint = _currentPoint;
                    }
                }
            }

            if (_currentEvent != null)
            {
                if (IsTouchup(ref _currentPoint))
                {
                    _currentEvent.OnRelease(_currentPoint);
                    if (_touchState == TouchState.Dragging) _currentEvent.OnEndDrag(_currentPoint);
                    _touchState = TouchState.Normal;
                    _currentEvent = null;
                }
                else
                {
                    Vector2 delta = _currentPoint - _lastPressPoint;
                    float deltaSqr = Vector2.Dot(delta, delta);
                    if (_touchState == TouchState.Press && deltaSqr > 250f)
                    {
                        _currentEvent.OnBeginDrag(_currentPoint, delta);
                        _touchState = TouchState.Dragging;
                        _lastPressPoint = _currentPoint;
                    }
                    else if (_touchState == TouchState.Dragging && deltaSqr > 1f)
                    {
                        _currentEvent.OnDragging(_currentPoint, delta);
                        _lastPressPoint = _currentPoint;
                    }
                }
            }
        }
        
        bool IsTouchdown(ref Vector3 position)
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            position = Input.mousePosition;
            return Input.GetKeyDown(KeyCode.Mouse0);
#else
            if (Input.touchCount == 1)
            {
                Touch touch = Input.GetTouch(0);
                postion = touch.position;
                return touch.phase == TouchPhase.Began;
            }
            return false;
#endif
        }

        bool IsTouchup(ref Vector3 postion)
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            postion = Input.mousePosition;
            return Input.GetKeyUp(KeyCode.Mouse0);
#else
            if (Input.touchCount == 1)
            {
                Touch touch = Input.GetTouch(0);
                postion = touch.position;
                return touch.phase == TouchPhase.Ended;
            }
            return false;
#endif
        }
        
        private List<RaycastResult> _raycastResults;

        private PointerEventData _pointEventData;

        private UICustomEvent RaycastUIObject(Vector2 position,ref GameObject go)
        {
            if (_raycastResults == null)
            {
                _raycastResults = new List<RaycastResult>();
            }
            if (_pointEventData == null)
            {
                _pointEventData = new PointerEventData(_eventSystem);
            }

            go = null;
            UICustomEvent focus = null;
            if (_customEventLst != null && _customEventLst.Count > 0)
            {
                _raycastResults.Clear();
                _pointEventData.position = position;
                _eventSystem.RaycastAll(_pointEventData, _raycastResults);
                if (_raycastResults.Count > 0)
                {
                    go = _raycastResults[0].gameObject;
                    return CheckContainsToEventObject(go);
                }
            }
            return focus;
        }

        private UICustomEvent CheckContainsToEventObject(GameObject go)
        {
            for (int i = 0; i < _customEventLst.Count; i++)
            {
                if (go == _customEventLst[i].go)  
                    return _customEventLst[i];
            }

            Transform parent = go.transform.parent;
            if (parent == null)
            {
                return null;
            }
            else
            {
                if (parent == transform) return null;
                return CheckContainsToEventObject(parent.gameObject);
            }
        }
    }
}
