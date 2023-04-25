using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using MyTools;
using UnityEngine.EventSystems;

namespace Lobby
{

    public class InputDropdownController : MonoBehaviour
    {
        [SerializeField] public InputField inputField;
        [SerializeField] public Button button;
        [SerializeField] public ScrollRect scrollViewContent;
        [SerializeField] public GameObject scrollViewItemPrefab;
        [SerializeField] public int maxDisplayCount = 5;

        private List<GameObject> scrollViewItems = new List<GameObject>();
        private string currentInput;
        public float itemHeight;

        private List<string> options = new List<string>()
        {
            "勾股定理的含义和例子",
            "勾股定理的证明方法",
            "勾股定理的历史发展",
        };

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
            onSel.callback.AddListener(e => OnInputValueChanged());
            onSel.eventID = EventTriggerType.Select;
            eventTrigger.triggers.Add(onSel);

            var onUnsel = new EventTrigger.Entry();
            onUnsel.callback.AddListener(e => OnEndInputValueChanged());
            onUnsel.eventID = EventTriggerType.Deselect;
            eventTrigger.triggers.Add(onUnsel);
        }

        private void OnInputValueChanged()
        {
            scrollViewContent.gameObject.SetActive(true);
            currentInput = inputField.text;
            FilterOptions();
        }

        private void OnEndInputValueChanged()
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
                filteredOptions = options;
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
                        optionWordCounts.Add(option, count);
                    }
                }

                filteredOptions = optionWordCounts.OrderBy(pair => pair.Value)
                    .Select(pair => pair.Key)
                    .ToList();

            }

            // Add the filtered options to the scroll view items
            for (var i = 0; i < Mathf.Min(filteredOptions.Count, maxDisplayCount); i++)
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
            inputField.text = option;
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

