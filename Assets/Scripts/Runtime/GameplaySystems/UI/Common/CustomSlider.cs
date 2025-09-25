using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

public class CustomSlider : MonoBehaviour
{
    [System.Serializable]
    private class SliderData
    {
        public string sliderName;
        public Slider slider;
        public VisualElement dragger;
        public VisualElement bar;
    }

    [Header("Slider Names")]
    [SerializeField]
    private string[] sliderNames = {
        "MasterVolumeSlider",
        "MusicVolumeSlider",
        "SFXVolumeSlider",
        "UIVolumeSlider"
    };

    private UIDocument m_Document;
    private List<SliderData> m_Sliders = new List<SliderData>();

    private void Start()
    {
        m_Document = GetComponent<UIDocument>();

        if (m_Document == null || m_Document.rootVisualElement == null)
        {
            Debug.LogError("UIDocument or rootVisualElement not found!");
            return;
        }

        InitializeSliders();
    }

    private void InitializeSliders()
    {
        foreach (string sliderName in sliderNames)
        {
            Slider slider = m_Document.rootVisualElement.Q<Slider>(sliderName);

            if (slider != null)
            {
                VisualElement dragger = slider.Q<VisualElement>("unity-dragger");

                if (dragger != null)
                {
                    SliderData sliderData = new SliderData
                    {
                        sliderName = sliderName,
                        slider = slider,
                        dragger = dragger
                    };

                    AddBarElement(sliderData);
                    m_Sliders.Add(sliderData);
                }
                else
                {
                    Debug.LogWarning($"Dragger not found for slider: {sliderName}");
                }
            }
            else
            {
                Debug.LogWarning($"Slider not found: {sliderName}");
            }
        }
    }

    private void AddBarElement(SliderData sliderData)
    {
        VisualElement existingBar = sliderData.dragger.Q<VisualElement>("Bar");
        existingBar?.RemoveFromHierarchy();

        sliderData.bar = new VisualElement
        {
            name = "Bar"
        };
        sliderData.bar.AddToClassList("bar");
        sliderData.dragger.Add(sliderData.bar);
    }

    // Optional: Public method to get a specific slider's bar for customization
    public VisualElement GetSliderBar(string sliderName)
    {
        SliderData sliderData = m_Sliders.Find(data => data.sliderName == sliderName);
        return sliderData?.bar;
    }

    // Optional: Public method to update all bars (if you need dynamic behavior)
    public void UpdateAllBars()
    {
        foreach (SliderData sliderData in m_Sliders)
        {
            // Add any bar update logic here if needed
            // For example, you could adjust the bar appearance based on slider value
        }
    }

    // Optional: If you need to access specific slider data
    public Slider GetSlider(string sliderName)
    {
        SliderData sliderData = m_Sliders.Find(data => data.sliderName == sliderName);
        return sliderData?.slider;
    }
}