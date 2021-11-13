namespace mprJoin.ViewModels
{
    using System.Collections.ObjectModel;
    using Models;

    public class JoinContext
    {
        public JoinContext()
        {
            
        }

        /// <summary>
        /// Список с парами элементов для соединения
        /// </summary>
        public ObservableCollection<CustomElementPair> Pairs { get; set; } = new ()
        {
            new ()
            {
                WhatToJoinCategory = "Стены",
                WithWhatToJoin = "Плиты"
            },
            new ()
            {
                WhatToJoinCategory = "Колоны",
                WithWhatToJoin = "Балки"
            },
            new ()
            {
                WhatToJoinCategory = "Ограждения",
                WithWhatToJoin = "Переходы"
            }
        };
    }
}