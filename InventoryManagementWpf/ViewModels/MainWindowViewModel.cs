using System.Collections.ObjectModel;
using System.Configuration;
using System.Threading.Tasks;
using System.Windows.Input;
using InventoryManagementLibrary;
using Prism.Commands;
using Prism.Mvvm;

namespace InventoryManagementWpf
{
    public class MainWindowViewModel : BindableBase
    {
        private readonly ITherapeuticClassProcessor _therapeuticClassProcessor;

        private readonly string _connectionString =
            ConfigurationManager.ConnectionStrings["SqliteConnection"].ConnectionString;

        public MainWindowViewModel()
        {
            //Initialize commands
            ItemSearchCommand = new DelegateCommand(ExecuteSearchCommand);

            //Initialize processors
            _therapeuticClassProcessor = new TherapeuticClassProcessor();

            Task.Run(LoadTherapeuticClass);
        }

        private async void LoadTherapeuticClass()
        {
            var therapeuticClass = await _therapeuticClassProcessor.GetTherapeuticClass(_connectionString);
            TherapeuticClass = new ObservableCollection<TherapeuticClassDbModel>(therapeuticClass);
        }

        private void ExecuteSearchCommand()
        {
            throw new System.NotImplementedException();
        }
        
        #region string : GenericNameSearch
        private string _genericNameSearch;

        public string GenericNameSearch
        {
            get { return _genericNameSearch; }
            set 
            { 
                _genericNameSearch = value; 
                RaisePropertyChanged();
            }
        }
        #endregion

        #region string : BrandNameSearch
        private string _brandNameSearch;

        public string BrandNameSearch
        {
            get { return _brandNameSearch; }
            set 
            { 
                _brandNameSearch = value; 
                RaisePropertyChanged();
            }
        }
        #endregion

        #region string : ManufacturerSearch
        private string _manufacturerSearch;

        public string ManufacturerSearch
        {
            get { return _manufacturerSearch; }
            set 
            { 
                _manufacturerSearch = value; 
                RaisePropertyChanged();
            }
        }
        #endregion

        #region TherapeuticClassDbModel : TherapeuticClassSearch
        private TherapeuticClassDbModel _therapeuticClassSearch;

        public TherapeuticClassDbModel TherapeuticClassSearch
        {
            get { return _therapeuticClassSearch; }
            set 
            { 
                _therapeuticClassSearch = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        #region string : BarcodeSearch
        private string _barcodeSearch;

        public string BarcodeSearch
        {
            get { return _barcodeSearch; }
            set 
            { 
                _barcodeSearch = value; 
                RaisePropertyChanged();
            }
        }
        #endregion

        #region ICommand : ItemSearchCommand
        private ICommand _itemSearchCommand;

        public ICommand ItemSearchCommand
        {
            get { return _itemSearchCommand; }
            set 
            { 
                _itemSearchCommand = value; 
                RaisePropertyChanged();
            }
        }
        #endregion

        #region Item : SelectedItem
        private Item _selectedItem;

        public Item SelectedItem
        {
            get { return _selectedItem; }
            set 
            { 
                _selectedItem = value; 
                RaisePropertyChanged();
            }
        }
        #endregion

        #region ObservableCollection<TherapeuticClassDbModel> : TherapeuticClass
        private ObservableCollection<TherapeuticClassDbModel> _therapeuticClass;

        public ObservableCollection<TherapeuticClassDbModel> TherapeuticClass
        {
            get { return _therapeuticClass; }
            set 
            { 
                _therapeuticClass = value; 
                RaisePropertyChanged();
            }
        }
        #endregion

        #region ObservableCollection<Item> : Items
        private ObservableCollection<Item> _items;

        public ObservableCollection<Item> Items
        {
            get { return _items; }
            set 
            { 
                _items = value; 
                RaisePropertyChanged();
            }
        }
        #endregion
    }
}