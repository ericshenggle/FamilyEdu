using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using MyTools;
using UnityEngine.EventSystems;
using Unity.VisualScripting;
using System.Text;
using System;
using System.Collections;
using Random = UnityEngine.Random;

namespace Lobby
{

    public class InputDropdownController : MonoBehaviour
    {
        [SerializeField] public InputField inputField;
        [SerializeField] public Button button;
        [SerializeField] public ScrollRect scrollViewContent;
        [SerializeField] public GameObject scrollViewItemPrefab;
        [SerializeField] public GameObject m_gameObject;
        [SerializeField] public int maxDisplayCount = 5;

        private List<GameObject> scrollViewItems = new List<GameObject>();
        private string currentInput;
        public float itemHeight;
        private bool isSelectInputField;
        private bool ignoreChange;

        private string[] options = StringArrayHolder.stringArray;

        // 定义一个自定义的类型作为键
        public class MyKey
        {
            public string Value { get; set; }
        }

        private void Awake()
        {
            inputField.onValueChanged.AddListener(delegate { OnInputValueChanged(); });
            button.onClick.AddListener(() =>
            {
                scrollViewContent.gameObject.SetActive(false);
            });
            var eventTrigger = inputField.GetComponent<EventTrigger>();
            if (!eventTrigger) eventTrigger = inputField.gameObject.AddComponent<EventTrigger>();

            var onSel = new EventTrigger.Entry();
            onSel.callback.AddListener(e => OnSelectInputField());
            onSel.eventID = EventTriggerType.Select;
            eventTrigger.triggers.Add(onSel);

            var onDeSel = new EventTrigger.Entry();
            onDeSel.callback.AddListener(e => OnDeSelectInputField());
            onDeSel.eventID = EventTriggerType.Deselect;
            eventTrigger.triggers.Add(onDeSel);

            var eventTrigger1 = m_gameObject.GetComponent<EventTrigger>();
            if (!eventTrigger1) eventTrigger1 = m_gameObject.gameObject.AddComponent<EventTrigger>();

            var onEnter = new EventTrigger.Entry();
            onEnter.callback.AddListener(e => OnEnter());
            onEnter.eventID = EventTriggerType.PointerEnter;
            eventTrigger1.triggers.Add(onEnter);

            var onExit = new EventTrigger.Entry();
            onExit.callback.AddListener(e => OnExit());
            onExit.eventID = EventTriggerType.PointerExit;
            eventTrigger1.triggers.Add(onExit);
        }

        private void OnSelectInputField()
        {
            isSelectInputField = true;
            OnInputValueChanged();
        }

        private void OnDeSelectInputField()
        {
            MyDebug.Log("OnDeSelectInputField");
            isSelectInputField = false;
        }

        private void OnInputValueChanged()
        {
            if (ignoreChange)
            {
                ignoreChange = false;
                return;
            }
            scrollViewContent.gameObject.SetActive(true);
            currentInput = inputField.text;
            FilterOptions();
        }

        private void OnEnter()
        {
            if (isSelectInputField)
            {
                scrollViewContent.gameObject.SetActive(true);
            }
        }

        private void OnExit()
        {
            scrollViewContent.gameObject.SetActive(false);
        }

        private void FilterOptions()
        {
            // Clear the scroll view items
            ClearScrollViewItems();

            // Filter the options based on the current input
            List<string> filteredOptions;
            if (string.IsNullOrEmpty(currentInput))
            {

                // 用于存储选取的元素的列表
                filteredOptions = new List<string>();

                // 循环 10 次，每次从数组中随机选取一个元素并添加到选取列表中
                for (int i = 0; i < 10; i++)
                {
                    // 生成随机索引
                    int index = Random.Range(0, options.Length - 1);

                    // 从数组中选取元素
                    string element = options[index];

                    // 如果选取列表中已经包含了该元素，则重新生成随机索引并选取元素，直到选取到一个新的元素为止
                    while (filteredOptions.Contains(element))
                    {
                        index = Random.Range(0, options.Length - 1);
                        element = options[index];
                    }

                    // 将选取的元素添加到选取列表中
                    filteredOptions.Add(element);
                }
            }
            else
            {
                char[] inputWords = currentInput.ToCharArray();

                Dictionary<string, int> optionWordCounts = new Dictionary<string, int>();
                foreach (string option in options)
                {
                    int count = inputWords.Count(w => option.Contains(w));
                    if (count > 0)
                    {
                        if (optionWordCounts.ContainsKey(option))
                        {
                            continue;
                        }
                        else
                        {
                            optionWordCounts.Add(option, count);
                        }

                    }
                }

                filteredOptions = optionWordCounts.OrderByDescending(pair => pair.Value)
                    .Select(pair => pair.Key)
                    .ToList();

            }

            // Add the filtered options to the scroll view items
            for (var i = Mathf.Min(filteredOptions.Count, maxDisplayCount) - 1; i >= 0; --i)
            {
                var option = filteredOptions[i];
                var scrollViewItem = Instantiate(scrollViewItemPrefab, scrollViewContent.content);
                scrollViewItem.GetComponentInChildren<Text>().text = option;
                scrollViewItem.GetComponent<Button>().onClick.AddListener(() => SelectOption(option));
                scrollViewItem.GetComponent<RectTransform>().sizeDelta = new Vector2(0, itemHeight);
                scrollViewItems.Add(scrollViewItem);
            }
        }

        private void SelectOption(string option)
        {
            ignoreChange = true;
            inputField.text = option;
            isSelectInputField = false;
            scrollViewContent.gameObject.SetActive(false);
            ClearScrollViewItems();
        }

        private void ClearScrollViewItems()
        {
            foreach (var item in scrollViewItems)
            {
                Destroy(item);
            }
            scrollViewItems.Clear();

        }


    }
}

