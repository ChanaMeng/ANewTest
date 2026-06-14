namespace SDClub.UIFrameWork
{
    public static class PathHelper
    {
        public static string GetUIPrefabPath(string uiName)
        {
            return $"Assets/Prefabs/UI/{uiName}.prefab";
        }
        
        public static string GetAudioPath(string audioName)
        {
            return $"Assets/Art/Audio/{audioName}";
        }
        
        public static string GetConfigPath(string configName)
        {
            return $"Assets/Config/{configName}";
        }
    }
}
