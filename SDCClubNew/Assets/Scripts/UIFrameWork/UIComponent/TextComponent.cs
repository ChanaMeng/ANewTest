using SDClub.Core;
using UnityEngine.UI;
using TMPro;

namespace SDClub.UIFrameWork
{
    public class TextComponent : Entity, IAwake
    {
        public Text Text { get; set; }
        public TMP_Text TMPText { get; set; }
        
        public void SetText(string text)
        {
            if (Text != null) Text.text = text;
            if (TMPText != null) TMPText.text = text;
        }
    }
}
