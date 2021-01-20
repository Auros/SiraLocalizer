namespace SiraLocalizer.UI
{
    internal class ItalicizedFlyingTextEffect : FlyingTextEffect
    {
        public void Awake()
        {
            // this isn't serialized for some reason
            _text.autoSizeTextContainer = true;
        }
    }
}
