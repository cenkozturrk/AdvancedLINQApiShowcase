namespace AdvancedLINQApiShowcase.Caching
{
    public interface ICacheService
    {
        T? GetData<T>(string key);
        void SetData<T>(string key,T data);
    }
}
