using UnityEngine;

namespace Vena.Framework
{
    public enum UIFollowAxis
    {
        All,
        FollowX,
        FollowY,
    }


    public class UIFollowObject : MonoBehaviour
    {
        private GameObject _followObject;
        private Vector3 _pos;
        private bool isUIPositon;
        private UIFollowAxis _followAxis;
        private UIRoot uiRoot;
        private RectTransform nodeTrans;

        public void SetFollowObject(GameObject followObject)
        {
            _followObject = followObject;
            isUIPositon = false;
            follow();
        }

        public void SetFollowPosition(Vector3 pos)
        {
            _followObject = null;
            _pos = pos;
            isUIPositon = false;
            follow();
        }

        public void SetFollowUIPosition(Vector3 pos, UIFollowAxis followAxis)
        {
            _followObject = null;
            _pos = pos;
            isUIPositon = true;
            _followAxis = followAxis;
            InitBasicPosition();
            follow();
        }

        public void SetUI(UIRoot root, Transform nodeTrans)
        {
            uiRoot = root;
            this.nodeTrans = nodeTrans.GetComponent<RectTransform>();
        }

        /// <summary>
        /// 初始化坐标
        /// </summary>
        void InitBasicPosition()
        {
            Vector3 pos = new Vector3(_pos.x, _pos.y, _pos.z);

            Vector2 nextPos = new Vector2();            //= ConvertUtil.ScreenToUIPosition(Camera.main.WorldToScreenPoint(pos));
            uiRoot.CheckInRectangle(nodeTrans, Camera.main.WorldToScreenPoint(pos), out nextPos);
            nextPos += new Vector2(0, 10);
            gameObject.transform.localPosition = nextPos;
            gameObject.SetActive(true);
        }

        void Update()
        {
            follow();
        }

        void follow()
        {
            if (isUIPositon)
            {
                Vector3 pos = new Vector3(_pos.x, _pos.y, _pos.z);
                Vector2 nextPos = new Vector2();            //= ConvertUtil.ScreenToUIPosition(Camera.main.WorldToScreenPoint(pos));
                uiRoot.CheckInRectangle(nodeTrans, Camera.main.WorldToScreenPoint(pos), out nextPos);
                switch (_followAxis)
                {
                    case UIFollowAxis.All:
                        gameObject.transform.localPosition = nextPos;
                        break;
                    case UIFollowAxis.FollowX:
                        gameObject.transform.localPosition = new Vector3(nextPos.x, gameObject.transform.localPosition.y, 0);
                        break;
                    case UIFollowAxis.FollowY:
                        gameObject.transform.localPosition = new Vector3(gameObject.transform.localPosition.x, nextPos.y, 0);
                        break;
                }
            }
            else
            {
                if (_followObject != null)
                {
                    gameObject.transform.position = Camera.main.WorldToScreenPoint(_followObject.transform.position);
                }
                else
                {
                    gameObject.transform.position = Camera.main.WorldToScreenPoint(_pos);
                }
            }
        }
    }
}
