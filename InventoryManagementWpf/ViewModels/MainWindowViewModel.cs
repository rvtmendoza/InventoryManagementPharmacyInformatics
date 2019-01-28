using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Linq;
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
        private readonly IItemProcessor _itemProcessor;
        private readonly IInventoryProcessor _inventoryProcessor;

        private readonly string _connectionString =
            ConfigurationManager.ConnectionStrings["SqliteConnection"].ConnectionString;

        public MainWindowViewModel()
        {
            //Initialize commands
            InventorySearchCommand = new DelegateCommand(ExecuteInventorySearchCommand);
            AddInventoryItemCommand = new DelegateCommand(ExecuteAddInventoryItemCommand);
            AddNewItemCommand = new DelegateCommand(ExecuteAddNewItemCommand);

            //Initialize processors
            _itemProcessor = new ItemProcessor(_connectionString);
            _inventoryProcessor = new InventoryProcessor(_connectionString);
            _therapeuticClassProcessor = new TherapeuticClassProcessor(_connectionString);

            //Initialize values
            AddInventoryExpiryDate = DateTime.Now;
            AddInventoryManufacturingDate = DateTime.Now;
            
            LoadAvailableTherapeuticClass();
        }
        
        private async void ExecuteInventorySearchCommand()
        {
            var items = (await _itemProcessor.GetItems(InventorySearchGenericName, InventorySearchBrandName,
                InventorySearchManufacturer, InventorySearchBarcode, InventorySearchTherapeuticClass)).ToList();

            var inventoryItems = (await _inventoryProcessor.GetInventoryItemsById(items.Select(x => x.ItemId),
                InventorySearchBatchNumber, InventorySearchLotNumber));

            if (inventoryItems != null)
            {
                LoadInventoryItemsSearchResult(inventoryItems); 
            }

            //Clears inventory search UI
            ClearSearchInventoryControls();
        }

        private async void ExecuteAddInventoryItemCommand()
        {
            //TODO: Add Data Validation

            var newInventory = new InventoryDbModel()
            {
                ItemId = InventorySelectedItem.ItemModel.ItemId,
                BatchNumber = InventorySelectedItem.InventoryModel.BatchNumber,
                LotNumber = InventorySelectedItem.InventoryModel.LotNumber,
                ExpiryDate = InventorySelectedItem.InventoryModel.ExpiryDate,
                ManufacturingDate = InventorySelectedItem.InventoryModel.ManufacturingDate,
                Quantity = InventorySelectedItem.InventoryModel.Quantity
            };

            var resultsChanged = await _inventoryProcessor.AddInventoryItem(newInventory);

            //Creates a visual indication for the update result
            DisplayAddInventoryResult(resultsChanged > 0);

            //Clears UI
            ClearAddInventoryControls();

            //Updates displayed inventory items
            var itemIds = InventoryItems.Select(x => x.ItemModel.ItemId).Distinct().ToList();

            var items = await _itemProcessor.GetItemsById(itemIds);
            LoadInventoryItemsSearchResult(items);
        }

        private void ExecuteAddNewItemCommand()
        {
            var addNewItemWindow = new AddNewItemWindow {DataContext = new AddNewItemViewModel(_connectionString)};
            addNewItemWindow.Show();
        }

        private async void LoadAvailableTherapeuticClass()
        {
            var therapeuticClass = await _therapeuticClassProcessor.GetTherapeuticClass();
            AvailableTherapeuticClass = new ObservableCollection<TherapeuticClassDbModel>(therapeuticClass);
        }

        private async void LoadInventoryItemsSearchResult(IEnumerable<ItemDbModel> items)
        {
            var inventoryItems = await _inventoryProcessor.GetInventoryItemsById(items.Select(x => x.ItemId));

            var itemsSearchResult = new List<Item>();

            foreach (InventoryDbModel inventoryItem in inventoryItems)
            {
                var itemModel = items.FirstOrDefault(x => x.ItemId == inventoryItem.ItemId);
                var therapeuticClass =
                    AvailableTherapeuticClass.FirstOrDefault(x => x.TherapeuticClassId == itemModel.TherapeuticClassId);

                itemsSearchResult.Add(new Item(itemModel, inventoryItem, therapeuticClass));
            }

            InventoryItems = new ObservableCollection<Item>(itemsSearchResult);
        }

        private async void LoadInventoryItemsSearchResult(IEnumerable<InventoryDbModel> inventoryItems)
        {
            var itemsSearchResult = new List<Item>();

            foreach (var inventoryItem in inventoryItems)
            {
                var itemModel = (await _itemProcessor.GetItemsById(new List<int>() {inventoryItem.ItemId})).FirstOrDefault();
                
                var therapeuticClass =
                    AvailableTherapeuticClass.FirstOrDefault(x => x.TherapeuticClassId == itemModel.TherapeuticClassId);

                itemsSearchResult.Add(new Item(itemModel, inventoryItem, therapeuticClass));
            }

            InventoryItems = new ObservableCollection<Item>(itemsSearchResult);
        }

        private async void DisplayAddInventoryResult(bool isSuccessful)
        {
            AddInventoryResult = isSuccessful ? Result.Success : Result.Fail;

            await Task.Delay(5000);

            AddInventoryResult = Result.NoResult;
        }

        private void ClearSearchInventoryControls()
        {
            InventorySearchBrandName = "";
            InventorySearchGenericName = "";
            InventorySearchManufacturer = "";
            InventorySearchBarcode = "";
            InventorySearchTherapeuticClass = null;
        }

        private void ClearAddInventoryControls()
        {
            AddInventoryQuantity = 0;
            AddInventoryLotNumber = "";
            AddInventoryBatchNumber = "";
            AddInventoryExpiryDate = DateTime.Now;
            AddInventoryManufacturingDate = DateTime.Now;
            InventorySelectedItem = null;
        }

        #region Inventory Tab Properties
        #region ICommand : InventorySearchCommand
        private ICommand _inventorySearchCommand;

        public ICommand InventorySearchCommand
        {
            get { return _inventorySearchCommand; }
            set 
            { 
                _inventorySearchCommand = value; 
                RaisePropertyChanged();
            }
        }
        #endregion

        #region ICommand : AddInventoryItemCommand
        private ICommand _addInventoryItemCommand;

        public ICommand AddInventoryItemCommand
        {
            get { return _addInventoryItemCommand; }
            set 
            { 
                _addInventoryItemCommand = value; 
                RaisePropertyChanged();
            }
        }
        #endregion

        #region ICommand : AddNewItemCommand
        private ICommand _addNewItemCommand;

        public ICommand AddNewItemCommand
        {
            get { return _addNewItemCommand; }
            set 
            { 
                _addNewItemCommand = value; 
                RaisePropertyChanged();
            }
        }
        #endregion

        #region string : InventorySearchBrandName
        private string _inventorySearchBrandName;

        public string InventorySearchBrandName
        {
            get { return _inventorySearchBrandName; }
            set 
            { 
                _inventorySearchBrandName = value; 
                RaisePropertyChanged();
            }
        }
        #endregion

        #region string : InventorySearchGenericName
        private string _inventorySearchGenericName;

        public string InventorySearchGenericName
        {
            get { return _inventorySearchGenericName; }
            set 
            { 
                _inventorySearchGenericName = value; 
                RaisePropertyChanged();
            }
        }
        #endregion

        #region string : InventorySearchManufacturer
        private string _inventorySearchManufacturer;

        public string InventorySearchManufacturer
        {
            get { return _inventorySearchManufacturer; }
            set 
            { 
                _inventorySearchManufacturer = value; 
                RaisePropertyChanged();
            }
        }
        #endregion

        #region string : InventorySearchBarcode
        private string _inventorySearchBarcode;

        public string InventorySearchBarcode
        {
            get { return _inventorySearchBarcode; }
            set 
            { 
                _inventorySearchBarcode = value; 
                RaisePropertyChanged();
            }
        }
        #endregion

        #region string : InventorySearchBatchNumber
        private string _inventorySearchBatchNumber;

        public string InventorySearchBatchNumber
        {
            get { return _inventorySearchBatchNumber; }
            set 
            { 
                _inventorySearchBatchNumber = value; 
                RaisePropertyChanged();
            }
        }
        #endregion

        #region string : InventorySearchLotNumber
        private string _inventorySearchLotNumber;

        public string InventorySearchLotNumber
        {
            get { return _inventorySearchLotNumber; }
            set 
            { 
                _inventorySearchLotNumber = value; 
                RaisePropertyChanged();
            }
        }
        #endregion

        #region TherapeuticClassDbModel : InventorySearchTherapeuticClass
        private TherapeuticClassDbModel _inventorySearchTherapeuticClass;

        public TherapeuticClassDbModel InventorySearchTherapeuticClass
        {
            get { return _inventorySearchTherapeuticClass; }
            set
            {
                _inventorySearchTherapeuticClass = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        #region int : AddInventoryQuantity
        private int _addInventoryQuantity;

        public int AddInventoryQuantity
        {
            get { return _addInventoryQuantity; }
            set 
            { 
                _addInventoryQuantity = value; 
                RaisePropertyChanged();
            }
        }
        #endregion

        #region string : AddInventoryLotNumber
        private string _addInventoryLotNumber;

        public string AddInventoryLotNumber
        {
            get { return _addInventoryLotNumber; }
            set 
            { 
                _addInventoryLotNumber = value; 
                RaisePropertyChanged();
            }
        }
        #endregion

        #region string : AddInventoryBatchNumber
        private string _addInventoryBatchNumber;

        public string AddInventoryBatchNumber
        {
            get { return _addInventoryBatchNumber; }
            set 
            { 
                _addInventoryBatchNumber = value; 
                RaisePropertyChanged();
            }
        }
        #endregion

        #region DateTime : AddInventoryExpiryDate
        private DateTime _addInventoryExpiryDate;

        public DateTime AddInventoryExpiryDate
        {
            get { return _addInventoryExpiryDate; }
            set 
            { 
                _addInventoryExpiryDate = value; 
                RaisePropertyChanged();
            }
        }
        #endregion

        #region DateTime : AddInventoryManufacturingDate
        private DateTime _addInventoryManufacturingDate;

        public DateTime AddInventoryManufacturingDate
        {
            get { return _addInventoryManufacturingDate; }
            set 
            { 
                _addInventoryManufacturingDate = value; 
                RaisePropertyChanged();
            }
        }
        #endregion

        #region Result : AddInventoryResult
        private Result _addInventoryResult;

        public Result AddInventoryResult
        {
            get { return _addInventoryResult; }
            set 
            { 
                _addInventoryResult = value; 
                RaisePropertyChanged();
            }
        }
        #endregion

        #region ObservableCollection<TherapeuticClassDbModel> : AvailableTherapeuticClass
        private ObservableCollection<TherapeuticClassDbModel> _availableTherapeuticClass;

        public ObservableCollection<TherapeuticClassDbModel> AvailableTherapeuticClass
        {
            get { return _availableTherapeuticClass; }
            set
            {
                _availableTherapeuticClass = value;
                RaisePropertyChanged();
            }
        }
        #endregion 

        #region ObservableCollection<Item> : InventoryItems
        private ObservableCollection<Item> _inventoryItems;

        public ObservableCollection<Item> InventoryItems
        {
            get { return _inventoryItems; }
            set 
            { 
                _inventoryItems = value; 
                RaisePropertyChanged();
            }
        }
        #endregion

        #region Item : InventorySelectedItem
        private Item _inventorySelectedItem;

        public Item InventorySelectedItem
        {
            get { return _inventorySelectedItem; }
            set 
            { 
                _inventorySelectedItem = value; 
                RaisePropertyChanged();
            }
        }
        #endregion
        #endregion
    }
}