namespace SDClub.Sdk
{
    public interface IWXStorage
    {
        void SetString(string key, string value);
        void SetInt(string key, int value);
        void SetFloat(string key, float value);
        string GetString(string key, string defaultValue = "");
        int GetInt(string key, int defaultValue = 0);
        float GetFloat(string key, float defaultValue = 0f);
        void Remove(string key);
        void Clear();
        bool HasKey(string key);
    }
}
