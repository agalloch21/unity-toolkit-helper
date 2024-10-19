using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

namespace Xiaobo.UnityToolkit.Helper
{
    public class HelperModule : MonoBehaviour
    {

        [SerializeField] bool isVisible = false;
        public bool Visible
        {
            get => isVisible;
            set
            {
                isVisible = value;
                SetDebugPanelVisible(value);
            }
        }

        enum HelperItemType
        {
            Info,
            Slider,
            SpaceInfo
        }

        [Header("Info Panel")]
        
        [SerializeField] private Transform contentRootInfo;
        [SerializeField] private GameObject prefabInfo;
        Dictionary<string, GameObject> infoList = new Dictionary<string, GameObject>();

        [Header("Slider Panel")]
        
        [SerializeField] private Transform contentRootSlider;
        [SerializeField] private GameObject prefabSlider;
        Dictionary<string, GameObject> sliderList = new Dictionary<string, GameObject>();

        [Header("SpaceInfo Panel")]
        
        [SerializeField] private Transform contentRootSpaceInfo;
        [SerializeField] private GameObject prefabSpaceInfo;
        Dictionary<string, GameObject> spaceInfoList = new Dictionary<string, GameObject>();

        [Header("UI System")]
        [SerializeField] private Button buttonTogglePanel;
        [SerializeField] private Transform tabsRoot;
    
        private Button buttonInfo;
        private Button buttonSlider;
        private Button buttonSpaceInfo;

        private Transform panelInfo;
        private Transform panelSlider;
        private Transform panelSpaceInfo;



        #region public functions
        public void ToggleDebugPanel()
        {
            if (enabled == false) return;

            isVisible = !isVisible;
            SetDebugPanelVisible(isVisible);
        }

        public void SetDebugPanelVisible(bool state)
        {
            if (enabled == false) return;

            tabsRoot.gameObject.SetActive(state);
            if (state == false)
            {
                HidePanel(HelperItemType.Info);
                HidePanel(HelperItemType.Slider);
                HidePanel(HelperItemType.SpaceInfo);
            }
            else
            {
                ShowPanel(HelperItemType.Info);
            }

            buttonTogglePanel.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = state ? "Hide Panel" : "Show Panel";
        }

        public void SetInfo(string name, string info)
        {
            if (enabled == false) return;

            GameObject go = null;

            if (!infoList.ContainsKey(name))
            {
                go = CreateGameObject(name, HelperItemType.Info);

                infoList.Add(name, go);
            }
            else go = infoList[name];


            TextMeshProUGUI text_ugui = GetUITextComponent(go);
            text_ugui.text = info;
        }

        public void DeleleInfo(string name, string info)
        {
            if (enabled == false) return;

            if (infoList.ContainsKey(name))
            {
                Destroy(infoList[name]);
                infoList.Remove(name);
            }
        }

        public void SetSlider(string name, Action<float> action, float default_value = 0, float min = 0, float max = 1)
        {
            if (enabled == false) return;

            GameObject go = null;

            if (!infoList.ContainsKey(name))
            {
                go = CreateGameObject(name, HelperItemType.Slider);

                sliderList.Add(name, go);
            }
            else go = infoList[name];

            TextMeshProUGUI value_text = GetUITextComponent(go);
            Slider slider = GetSliderComponent(go);
            slider.minValue = min;
            slider.maxValue = max;
            slider.value = Mathf.Clamp(default_value, min, max);
            value_text.text = slider.value.ToString("0.00");

            slider.onValueChanged.AddListener((float v) =>
            {
                value_text.text = v.ToString("0.00");

                action?.Invoke(v);
            });
        }

        public void DeleteSlider(string name)
        {
            if (enabled == false) return;

            if (sliderList.ContainsKey(name))
            {
                Slider slider = GetSliderComponent(sliderList[name]);
                slider.onValueChanged.RemoveAllListeners();
                Destroy(sliderList[name]);
                sliderList.Remove(name);
            }
        }

        public void SetSapceInfo(string name, Transform trans, string info)
        {
            if (enabled == false) return;

            GameObject go = null;

            if (!spaceInfoList.ContainsKey(name))
            {
                go = CreateGameObject(name, HelperItemType.SpaceInfo);

                spaceInfoList.Add(name, go);
            }
            else go = infoList[name];

            go.transform.SetPositionAndRotation(trans.position, trans.rotation);
            TextMesh text_space = GetSpaceTextComponent(go);
            text_space.text = info;
        }

        public void DeleleSapceInfo(string name)
        {
            if (enabled == false) return;

            if (spaceInfoList.ContainsKey(name))
            {
                Destroy(spaceInfoList[name]);
                spaceInfoList.Remove(name);
            }
        }
        #endregion

        #region private functions

        void Start()
        {            
            SetDebugPanelVisible(isVisible);
        }

        void InitializeUI()
        {
            // assign component            
            panelInfo = transform.Find("UICanvas/InfoPanel"); CheckNullity(panelInfo, "panelInfo");
            panelSlider = transform.Find("UICanvas/SliderPanel"); CheckNullity(panelSlider, "panelSlider");
            panelSpaceInfo = transform.Find("SpaceInfoPanel"); CheckNullity(panelSpaceInfo, "panelSpaceInfo");

            buttonInfo = tabsRoot.GetChild(0).GetComponent<Button>(); CheckNullity(buttonInfo, "buttonInfo");
            buttonSlider = tabsRoot.GetChild(1).GetComponent<Button>(); CheckNullity(buttonSlider, "buttonSlider");
            buttonSpaceInfo = tabsRoot.GetChild(2).GetComponent<Button>(); CheckNullity(buttonSpaceInfo, "buttonSpaceInfo");


            // Add listener
            buttonInfo.onClick.AddListener(delegate { ShowPanel(HelperItemType.Info); });            
            buttonSlider.onClick.AddListener(delegate { ShowPanel(HelperItemType.Slider); });            
            buttonSpaceInfo.onClick.AddListener(delegate { ShowPanel(HelperItemType.SpaceInfo); });

            //
            if (FindFirstObjectByType<EventSystem>() == null)
            {
                var eventSystem = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            }
        }

        void CheckNullity(Component com, string name = "")
        {
            if(com == null)
            {
                name = name.Length > 0 ? name : "component";
                Debug.Log($"[{this.GetType().Name}] Didn't find {name} when initailizing helper module");
            }                
        }

        void ShowPanel(HelperItemType type)
        {
            panelInfo.gameObject.SetActive(false);
            panelSlider.gameObject.SetActive(false);
            panelSpaceInfo.gameObject.SetActive(false);

            Transform trans_shown = null;
            if (type == HelperItemType.Info) trans_shown = panelInfo;
            else if (type == HelperItemType.Slider) trans_shown = panelSlider;
            else if (type == HelperItemType.SpaceInfo) trans_shown = panelSpaceInfo;

            trans_shown.gameObject.SetActive(true);
        }
        void HidePanel(HelperItemType type)
        {
            Transform trans_shown = null;
            if (type == HelperItemType.Info) trans_shown = panelInfo;
            else if (type == HelperItemType.Slider) trans_shown = panelSlider;
            else if (type == HelperItemType.SpaceInfo) trans_shown = panelSpaceInfo;

            trans_shown.gameObject.SetActive(false);
        }    

        GameObject CreateGameObject(string name, HelperItemType type)
        {
            GameObject new_go = null;

            if(type == HelperItemType.Info)
            {
                new_go = Instantiate(prefabInfo, contentRootInfo);
            }            
            else if (type == HelperItemType.Slider)
            {
                new_go = Instantiate(prefabSlider, contentRootSlider);            
            }            
            else if (type == HelperItemType.SpaceInfo)
            {
                new_go = Instantiate(prefabSpaceInfo, contentRootSpaceInfo);
            }

            if(new_go != null)
            {
                new_go.name = name;
                if(type == HelperItemType.Info || type == HelperItemType.Slider)
                {
                    new_go.transform.Find("Label").GetComponent<TextMeshProUGUI>().text = name;
                    new_go.transform.Find("Value").GetComponent<TextMeshProUGUI>().text = "";
                }
                else if(type == HelperItemType.SpaceInfo)
                {
                    new_go.transform.Find("Value").GetComponent<TextMesh>().text = "";
                }
            }

            return new_go;
        }

        TextMeshProUGUI GetUITextComponent(GameObject go)
        {
            return go.transform.Find("Value").GetComponent<TextMeshProUGUI>();
        }

        Slider GetSliderComponent(GameObject go)
        {
            return go.transform.Find("Slider").GetComponent<Slider>();
        }

        TextMesh GetSpaceTextComponent(GameObject go)
        {
            return go.transform.Find("Value").GetComponent<TextMesh>();
        }
        #endregion



        private static HelperModule _Instance;

        public static HelperModule Instance
        {
            get
            {
                if (_Instance == null)
                {
                    _Instance = GameObject.FindAnyObjectByType<HelperModule>();
                    if (_Instance == null)
                    {
                        GameObject go = new GameObject();
                        _Instance = go.AddComponent<HelperModule>();
                    }
                }
                return _Instance;
            }
        }

        private void Awake()
        {
            //DontDestroyOnLoad(gameObject);

            InitializeUI();
        }

        private void OnDestroy()
        {
            _Instance = null;
        }
        
    }
}