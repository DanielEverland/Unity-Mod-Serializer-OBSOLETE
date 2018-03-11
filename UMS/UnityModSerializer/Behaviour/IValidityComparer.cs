namespace UMS.Behaviour
{
    public interface IValidityComparer<T>
    {
        bool IsValid(T obj);
    }
}