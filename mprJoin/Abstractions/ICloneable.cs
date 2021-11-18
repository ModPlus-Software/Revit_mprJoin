namespace mprJoin.Abstractions
{
    /// <summary>
    /// Интетерфейс копирования
    /// </summary>
    /// <typeparam name="T">Ресурс</typeparam>
    public interface ICloneable<T>
    {
        public T Clone();
    }
}