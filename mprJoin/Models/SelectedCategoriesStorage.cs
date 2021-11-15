namespace mprJoin.Models
{
    using System.Collections.Generic;
    using System.Linq;
    using ModPlusAPI.Mvvm;

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

        public List<SelectedCategory> SelectedCategories { get; }

        public string DisplayName => string.Join(", ", SelectedCategories.Where(c => c.IsSelected).Select(c => c.Name));
    }
}
