namespace mprJoin.Enums
{
    /// <summary>
    /// Условия для нахождения параметра.
    /// </summary>
    public enum Condition
    {
        /// <summary>
        /// Равно.
        /// </summary>
        Equals,
        
        /// <summary>
        /// Не равно.
        /// </summary>
        NotEquals,
        
        /// <summary>
        /// Содержит.
        /// </summary>
        Contains,
        
        /// <summary>
        /// Не содержит.
        /// </summary>
        NotContains,
        
        /// <summary>
        /// Начинается.
        /// </summary>
        Begin,
        
        /// <summary>
        /// Не начинается.
        /// </summary>
        NotBegin,
    }
}