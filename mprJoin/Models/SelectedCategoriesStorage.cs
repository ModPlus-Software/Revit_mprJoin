namespace mprJoin.Models
{
    using System.Collections.Generic;
    using System.Linq;
    using ModPlusAPI.Mvvm;

    /// <summary>
    /// Модель хранения списка выбранных категорий.
    /// </summary>
    public class SelectedCategoriesStorage : ObservableObject
    {
        public SelectedCategoriesStorage(List<SelectedCategory> selectedCategories)
        {
            SelectedCategories = selectedCategories;
            foreach (var selectedCategory in SelectedCategories)
            {
                selectedCategory.PropertyChanged += (_, args) =>
                {
                    if (args.PropertyName == nameof(SelectedCategory.IsSelected))
                        OnPropertyChanged(nameof(DisplayName));
                };
            }
        }

        /// <summary>
        /// Список выбранных категорий.
        /// </summary>
        public List<SelectedCategory> SelectedCategories { get; }

        /// <summary>
        /// Строковое имя всех выбранных категорий, для View.
        /// </summary>
        public string DisplayName => string.Join(", ", SelectedCategories.Where(c => c.IsSelected).Select(c => c.Name));
    }
}
