using InventoryManagementLibrary;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

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
            SalesSearchCommand = new DelegateCommand(ExecuteSalesSearchCommand, CanExecuteSalesSearchCommand);
            ShoppingListAddItemCommand = new DelegateCommand(ExecuteShoppingListAddItemCommand);
            ShoppingListConfirmCommand = new DelegateCommand(ExecuteShoppingListConfirmCommand);
            ShoppingListCancelCommand = new DelegateCommand(ExecuteShoppingListCancelCommand);
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
            ShoppingList = new ObservableCollection<Item>();

            LoadAvailableTherapeuticClass();
        }

        #region Sales
        private async void ExecuteSalesSearchCommand()
        {
            var items = (await _itemProcessor.GetItems(SalesSearchGenericName, SalesSearchBrandName,
                SalesSearchManufacturer, SalesSearchBarcode, SalesSearchTherapeuticClass)).ToList();

            var inventoryItems = await _inventoryProcessor.GetInventoryItemsById(items.Select(x => x.ItemId));

            if (inventoryItems != null)
            {
                LoadSalesItemsSearchResults(inventoryItems);
            }

            ClearSalesSearchControls();
        }

        private bool CanExecuteSalesSearchCommand()
        {
            return (ShoppingList.Count == 0);
        }

        private void ExecuteShoppingListAddItemCommand()
        {
            if (SalesSearchResultSelectedItem.InventoryModel.Quantity > 0)
            {
                var quantity = GetItemQuantityToAdd();

                var newInventoryModel = new InventoryDbModel()
                {
                    BatchNumber = SalesSearchResultSelectedItem.InventoryModel.BatchNumber,
                    ExpiryDate = SalesSearchResultSelectedItem.InventoryModel.ExpiryDate,
                    InventoryId = SalesSearchResultSelectedItem.InventoryModel.InventoryId,
                    ItemId = SalesSearchResultSelectedItem.InventoryModel.ItemId,
                    LotNumber = SalesSearchResultSelectedItem.InventoryModel.LotNumber,
                    ManufacturingDate = SalesSearchResultSelectedItem.InventoryModel.ManufacturingDate,
                    Quantity = quantity
                };
                var item = new Item(SalesSearchResultSelectedItem.ItemModel, newInventoryModel,
                    SalesSearchResultSelectedItem.TherapeuticClass);

                if (quantity > 0)
                {
                    AddShoppingListItem(item);
                    RemoveStockListItem(item);
                }
            }
            else
            {
                MessageBox.Show("Selected item has no more stocks left.");
                SalesSearchResultSelectedItem = null;
            }
        }

        private async void ExecuteShoppingListConfirmCommand()
        {
            var currentShoppingList = ShoppingList.ToList();

            int resultsChanged = 0;
            foreach (var item in currentShoppingList)
            {
                var inventoryId = item.InventoryModel.InventoryId;

                var inventoryItem = SalesSearchResults.FirstOrDefault(x => x.InventoryModel.InventoryId == inventoryId)?.InventoryModel;

                resultsChanged += await _inventoryProcessor.UpdateInventoryItem(inventoryItem);
            }

            if (currentShoppingList.Count != resultsChanged)
            {
                throw new Exception("Error updating one or more of the items in inventory.");
            }

            ShoppingList.Clear();
            CalculateTotalPrice();
            UpdateItemsSearchResults();
        }

        private void ExecuteShoppingListCancelCommand()
        {
            var currentShoppingList = ShoppingList.ToList();

            foreach (var item in currentShoppingList)
            {
                var matchedItem = SalesSearchResults.FirstOrDefault(x => x.InventoryModel.InventoryId == item.InventoryModel.InventoryId);
                var matchedItemIndex = SalesSearchResults.IndexOf(matchedItem);
                SalesSearchResults[matchedItemIndex].InventoryModel.Quantity += item.InventoryModel.Quantity;
            }

            SalesSearchResults = new ObservableCollection<Item>(SalesSearchResults);

            ShoppingList.Clear();
            CalculateTotalPrice();
        }

        private int GetItemQuantityToAdd()
        {
            var addItemToShoppingListWindow = new AddShoppingListItemWindow();
            var addItemToShoppingListItemWindowViewModel = new AddShoppingListItemWindowViewModel(addItemToShoppingListWindow, SalesSearchResultSelectedItem.DisplayName, SalesSearchResultSelectedItem.InventoryModel.Quantity);
            addItemToShoppingListWindow.ShowDialog();

            return addItemToShoppingListItemWindowViewModel.Quantity;
        }

        private void AddShoppingListItem(Item item)
        {
            var matchedItems = ShoppingList.Where(x => x.InventoryModel.InventoryId == item.InventoryModel.InventoryId).ToList();

            if (matchedItems.Count > 0)
            {
                var matchedItemIndex = ShoppingList.IndexOf(matchedItems.FirstOrDefault());
                ShoppingList[matchedItemIndex].InventoryModel.Quantity += item.InventoryModel.Quantity;

                ShoppingList = new ObservableCollection<Item>(ShoppingList);
            }
            else
            {
                ShoppingList.Add(item);
            }

            CalculateTotalPrice();
        }

        private void RemoveStockListItem(Item item)
        {
            var matchedItem = SalesSearchResults.FirstOrDefault(x => x.InventoryModel.InventoryId == item.InventoryModel.InventoryId);
            var itemIndex = SalesSearchResults.IndexOf(matchedItem);
            SalesSearchResults[itemIndex].InventoryModel.Quantity -= item.InventoryModel.Quantity;

            SalesSearchResults = new ObservableCollection<Item>(SalesSearchResults);
        }

        private void CalculateTotalPrice()
        {
            var subTotals = ShoppingList.Select(x => x.SubTotal);
            TotalPrice = subTotals.Sum();
        }
        #endregion

        #region Inventory
        private async void ExecuteInventorySearchCommand()
        {
            var items = (await _itemProcessor.GetItems(InventorySearchGenericName, InventorySearchBrandName,
                InventorySearchManufacturer, InventorySearchBarcode, InventorySearchTherapeuticClass)).ToList();

            var inventoryItems = await _inventoryProcessor.GetInventoryItemsById(items.Select(x => x.ItemId),
                InventorySearchBatchNumber, InventorySearchLotNumber);

            if (inventoryItems != null)
            {
                LoadInventoryItemsSearchResult(inventoryItems);
            }

            //Clears inventory search UI
            ClearInventorySearchControls();
        }

        private async void ExecuteAddInventoryItemCommand()
        {
            var newInventory = new InventoryDbModel()
            {
                InventoryId = InventorySelectedItem.InventoryModel.InventoryId,
                ItemId = InventorySelectedItem.ItemModel.ItemId,
                BatchNumber = InventorySelectedItem.InventoryModel.BatchNumber,
                LotNumber = InventorySelectedItem.InventoryModel.LotNumber,
                ExpiryDate = InventorySelectedItem.InventoryModel.ExpiryDate,
                ManufacturingDate = InventorySelectedItem.InventoryModel.ManufacturingDate,
                Quantity = InventorySelectedItem.InventoryModel.Quantity + AddInventoryQuantity
            };

            var resultsChanged = await _inventoryProcessor.AddInventoryItem(newInventory);

            //Creates a visual indication for the update result
            DisplayAddInventoryResult(resultsChanged > 0);

            //Clears UI
            ClearAddInventoryControls();

            //Updates search results
            UpdateItemsSearchResults();
        }

        private async void DisplayAddInventoryResult(bool isSuccessful)
        {
            AddInventoryResult = isSuccessful ? Result.Success : Result.Fail;

            await Task.Delay(5000);

            AddInventoryResult = Result.NoResult;
        }

        private void ExecuteAddNewItemCommand()
        {
            var addNewItemWindow = new AddNewItemWindow { DataContext = new AddNewItemViewModel(_connectionString) };
            addNewItemWindow.Show();
        }
        #endregion

        private async void LoadAvailableTherapeuticClass()
        {
            var therapeuticClass = await _therapeuticClassProcessor.GetTherapeuticClass();
            AvailableTherapeuticClass = new ObservableCollection<TherapeuticClassDbModel>(therapeuticClass);
        }

        private void UpdateItemsSearchResults()
        {
            UpdateSalesItemsSearchResult();
            UpdateInventoryItemsSearchResult();
        }

        private async void UpdateSalesItemsSearchResult()
        {
            if (SalesSearchResults != null)
            {
                var itemIds = SalesSearchResults.Select(x => x.InventoryModel.ItemId);

                var output = await _inventoryProcessor.GetInventoryItemsById(itemIds);

                LoadSalesItemsSearchResults(output); 
            }
        }

        private async void UpdateInventoryItemsSearchResult()
        {
            if (InventorySearchResults != null)
            {
                var itemIds = InventorySearchResults.Select(x => x.InventoryModel.ItemId);

                var output = await _inventoryProcessor.GetInventoryItemsById(itemIds);

                LoadInventoryItemsSearchResult(output); 
            }
        }

        private async void LoadSalesItemsSearchResults(IEnumerable<InventoryDbModel> inventoryItems)
        {
            var itemsSearchResult = new List<Item>();

            foreach (var inventoryItem in inventoryItems)
            {
                var itemModel = (await _itemProcessor.GetItemsById(new List<int>() { inventoryItem.ItemId })).FirstOrDefault();

                var therapeuticClass =
                    AvailableTherapeuticClass.FirstOrDefault(x => x.TherapeuticClassId == itemModel.TherapeuticClassId);

                itemsSearchResult.Add(new Item(itemModel, inventoryItem, therapeuticClass));
            }

            SalesSearchResults = new ObservableCollection<Item>(itemsSearchResult);
        }

        private async void LoadInventoryItemsSearchResult(IEnumerable<InventoryDbModel> inventoryItems)
        {
            var itemsSearchResult = new List<Item>();

            foreach (var inventoryItem in inventoryItems)
            {
                var itemModel = (await _itemProcessor.GetItemsById(new List<int>() { inventoryItem.ItemId })).FirstOrDefault();

                var therapeuticClass =
                    AvailableTherapeuticClass.FirstOrDefault(x => x.TherapeuticClassId == itemModel.TherapeuticClassId);

                itemsSearchResult.Add(new Item(itemModel, inventoryItem, therapeuticClass));
            }

            InventorySearchResults = new ObservableCollection<Item>(itemsSearchResult);
        }

        private void ClearSalesSearchControls()
        {
            SalesSearchBrandName = "";
            SalesSearchGenericName = "";
            SalesSearchManufacturer = "";
            SalesSearchBarcode = "";
            SalesSearchTherapeuticClass = null;
        }

        private void ClearInventorySearchControls()
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
            InventorySelectedItem = null;
        }

        #region Common Properties
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
        #endregion

        #region Sales Tab Properties
        #region ICommand : SalesSearchCommand
        private ICommand _salesSearchCommand;

        public ICommand SalesSearchCommand
        {
            get { return _salesSearchCommand; }
            set
            {
                _salesSearchCommand = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        #region ICommand : ShoppingListAddItemCommand
        private ICommand _shoppingListAddItemCommand;

        public ICommand ShoppingListAddItemCommand
        {
            get { return _shoppingListAddItemCommand; }
            set
            {
                _shoppingListAddItemCommand = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        #region ICommand : ShoppingListConfirmCommand
        private ICommand _shoppingListConfirmCommand;

        public ICommand ShoppingListConfirmCommand
        {
            get { return _shoppingListConfirmCommand; }
            set
            {
                _shoppingListConfirmCommand = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        #region ICommand : ShoppingListCancelCommand
        private ICommand _shoppingListCancelCommand;

        public ICommand ShoppingListCancelCommand
        {
            get { return _shoppingListCancelCommand; }
            set
            {
                _shoppingListCancelCommand = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        #region string : SalesSearchBrandName
        private string _salesSearchBrandName;

        public string SalesSearchBrandName
        {
            get { return _salesSearchBrandName; }
            set
            {
                _salesSearchBrandName = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        #region string : SalesSearchGenericName
        private string _salesSearchGenericName;

        public string SalesSearchGenericName
        {
            get { return _salesSearchGenericName; }
            set
            {
                _salesSearchGenericName = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        #region string : SalesSearchManufacturer
        private string _salesSearchManufacturer;

        public string SalesSearchManufacturer
        {
            get { return _salesSearchManufacturer; }
            set
            {
                _salesSearchManufacturer = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        #region string : SalesSearchBarcode
        private string _salesSearchBarcode;

        public string SalesSearchBarcode
        {
            get { return _salesSearchBarcode; }
            set
            {
                _salesSearchBarcode = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        #region TherapeuticClassDbModel : SalesSearchTherapeuticClass
        private TherapeuticClassDbModel _salesSearchTherapeuticClass;

        public TherapeuticClassDbModel SalesSearchTherapeuticClass
        {
            get { return _salesSearchTherapeuticClass; }
            set
            {
                _salesSearchTherapeuticClass = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        #region ObservableCollection<Item> : SalesSearchResults
        private ObservableCollection<Item> _salesSearchResults;

        public ObservableCollection<Item> SalesSearchResults
        {
            get { return _salesSearchResults; }
            set
            {
                _salesSearchResults = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        #region Item : SalesSearchResultSelectedItem
        private Item _salesSearchResultSelectedItem;

        public Item SalesSearchResultSelectedItem
        {
            get { return _salesSearchResultSelectedItem; }
            set
            {
                _salesSearchResultSelectedItem = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        #region ObservableCollection<Item> : ShoppingList
        private ObservableCollection<Item> _shoppingList;

        public ObservableCollection<Item> ShoppingList
        {
            get { return _shoppingList; }
            set
            {
                _shoppingList = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        #region decimal : TotalPrice
        private decimal _totalPrice;

        public decimal TotalPrice
        {
            get { return _totalPrice; }
            set
            {
                _totalPrice = value;
                RaisePropertyChanged();
            }
        }
        #endregion
        #endregion

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

        #region ObservableCollection<Item> : InventorySearchResults
        private ObservableCollection<Item> _inventorySearchResults;

        public ObservableCollection<Item> InventorySearchResults
        {
            get { return _inventorySearchResults; }
            set
            {
                _inventorySearchResults = value;
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
        #endregion
    }
}