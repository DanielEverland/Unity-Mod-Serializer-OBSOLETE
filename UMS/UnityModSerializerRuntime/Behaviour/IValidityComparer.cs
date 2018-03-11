namespace UMS.Runtime.Behaviour
{
    public interface IValidityComparer<T>
    {
        bool IsValid(T obj);
    }
}