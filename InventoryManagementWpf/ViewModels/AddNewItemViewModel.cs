using InventoryManagementLibrary;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace InventoryManagementWpf
{
    public class AddNewItemViewModel : BindableBase
    {
        private readonly ITherapeuticClassProcessor _therapeuticClassProcessor;
        private readonly IItemProcessor _itemProcessor;
        private readonly IInventoryProcessor _inventoryProcessor;

        public AddNewItemViewModel(string connectionString)
        {
            AddItemCommand = new DelegateCommand(ExecuteAddItemCommand);
            CancelAddItemCommand = new DelegateCommand(ExecuteCancelAddItemCommand);

            _therapeuticClassProcessor = new TherapeuticClassProcessor(connectionString);
            _itemProcessor = new ItemProcessor(connectionString);
            _inventoryProcessor = new InventoryProcessor(connectionString);

            LoadTherapeuticClass();
            InitializeValues();
        }

        private async void ExecuteAddItemCommand()
        {
            var errorList = ValidateInputParameters();

            if (errorList.Count > 0)
            {
                MessageBox.Show("Error encountered in adding new item.\n" + string.Join(Environment.NewLine, errorList));
                return;
            }

            var item = new ItemDbModel()
            {
                Barcode = Barcode,
                BrandName = BrandName,
                DosageForm = DosageForm,
                DosageStrength = DosageStrength,
                GenericName = GenericName,
                Manufacturer = Manufacturer,
                Price = (int)(UnitPrice * 100),
                TherapeuticClassId = SelectedTherapeuticClass.TherapeuticClassId
            };

            var resultsChanged = await _itemProcessor.AddItem(item);

            if (resultsChanged == 0)
            {
                throw new Exception("Failed to add item");
            }

            var newItem = (await _itemProcessor.GetItems(item.GenericName, item.BrandName, item.Manufacturer,
                item.Barcode, SelectedTherapeuticClass)).FirstOrDefault();

            var inventoryItem = new InventoryDbModel()
            {
                ItemId = newItem.ItemId,
                BatchNumber = BatchNumber,
                LotNumber = LotNumber,
                ExpiryDate = ExpiryDate.Date,
                ManufacturingDate = ManufacturingDate.Date,
                Quantity = Quantity
            };

            resultsChanged = await _inventoryProcessor.AddInventoryItem(inventoryItem);

            DisplayAddItemResult(resultsChanged>0);
            InitializeValues();
        }

        private void ExecuteCancelAddItemCommand()
        {
            InitializeValues();
        }

        private async void LoadTherapeuticClass()
        {
            var output = (await _therapeuticClassProcessor.GetTherapeuticClass()).ToList();

            AvailableTherapeuticClass = new ObservableCollection<TherapeuticClassDbModel>(output);
        }

        private List<string> ValidateInputParameters()
        {
            var errorList = new List<string>();

            if (string.IsNullOrWhiteSpace(GenericName))
            {
                errorList.Add("Generic name cannot be empty.");
            }
            if (string.IsNullOrWhiteSpace(BrandName))
            {
                errorList.Add("Brand name cannot be empty.");
            }
            if (string.IsNullOrWhiteSpace(Manufacturer))
            {
                errorList.Add("Manufacturer cannot be empty.");
            }
            if (string.IsNullOrWhiteSpace(DosageForm))
            {
                errorList.Add("Dosage form cannot be empty.");
            }
            if (string.IsNullOrWhiteSpace(DosageStrength))
            {
                errorList.Add("Dosage strength cannot be empty.");
            }
            if (SelectedTherapeuticClass == null)
            {
                errorList.Add("Therapeutic class cannot be empty.");
            }
            if (!long.TryParse(Barcode, out _))
            {
                errorList.Add("Barcode must be numerical only.");
            }
            else
            {
                if (Barcode.Length != 12)
                {
                    errorList.Add("Barcode must be 12 digits.");
                }
            }
            if (ManufacturingDate.Date > DateTime.Now.Date)
            {
                errorList.Add("Manufacturing date cannot be in the future.");
            }
            if (ExpiryDate.Date < DateTime.Now.Date)
            {
                errorList.Add("Item being added has already expired.");
            }
            if (ManufacturingDate.Date > ExpiryDate.Date)
            {
                errorList.Add("Date of manufacture cannot be after date of expiry.");
            }

            return errorList;
        }

        private async void DisplayAddItemResult(bool isSuccessful)
        {
            AddItemResult = isSuccessful ? Result.Success : Result.Fail;

            await Task.Delay(5000);

            AddItemResult = Result.NoResult;
        }

        private void InitializeValues()
        {
            GenericName = "";
            BrandName = "";
            Manufacturer = "";
            DosageForm = "";
            DosageStrength = "";
            Barcode = "";
            BatchNumber = "";
            LotNumber = "";
            Quantity = 0;
            UnitPrice = 0m;
            ManufacturingDate = DateTime.Today;
            ExpiryDate = DateTime.Today;
            SelectedTherapeuticClass = null;
        }

        #region ICommand : AddItemCommand
        private ICommand _addItemCommand;

        public ICommand AddItemCommand
        {
            get { return _addItemCommand; }
            set
            {
                _addItemCommand = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        #region ICommand : CancelAddItemCommand
        private ICommand _cancelAddItemCommand;

        public ICommand CancelAddItemCommand
        {
            get { return _cancelAddItemCommand; }
            set
            {
                _cancelAddItemCommand = value;
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

        #region TherapeuticClassDbModel : SelectedTherapeuticClass
        private TherapeuticClassDbModel _selectedTherapeuticClass;

        public TherapeuticClassDbModel SelectedTherapeuticClass
        {
            get { return _selectedTherapeuticClass; }
            set
            {
                _selectedTherapeuticClass = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        #region string : GenericName
        private string _genericName;

        public string GenericName
        {
            get { return _genericName; }
            set
            {
                _genericName = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        #region string : BrandName
        private string _brandName;

        public string BrandName
        {
            get { return _brandName; }
            set
            {
                _brandName = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        #region string : Manufacturer
        private string _manufacturer;

        public string Manufacturer
        {
            get { return _manufacturer; }
            set
            {
                _manufacturer = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        #region string : DosageForm
        private string _dosageForm;

        public string DosageForm
        {
            get { return _dosageForm; }
            set
            {
                _dosageForm = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        #region string : DosageStrength
        private string _dosageStrength;

        public string DosageStrength
        {
            get { return _dosageStrength; }
            set
            {
                _dosageStrength = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        #region string : Barcode
        private string _barcode;

        public string Barcode
        {
            get { return _barcode; }
            set
            {
                _barcode = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        #region string : BatchNumber
        private string _batchNumber;

        public string BatchNumber
        {
            get { return _batchNumber; }
            set
            {
                _batchNumber = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        #region string : LotNumber
        private string _lotNumber;

        public string LotNumber
        {
            get { return _lotNumber; }
            set
            {
                _lotNumber = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        #region int : DesiredQuantity
        private int _quantity;

        public int Quantity
        {
            get { return _quantity; }
            set
            {
                _quantity = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        #region DateTime : ManufacturingDate
        private DateTime _manufacturingDate;

        public DateTime ManufacturingDate
        {
            get { return _manufacturingDate; }
            set
            {
                _manufacturingDate = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        #region DateTime : ExpiryDate
        private DateTime _expiryDate;

        public DateTime ExpiryDate
        {
            get { return _expiryDate; }
            set
            {
                _expiryDate = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        #region decimal : UnitPrice
        private decimal _unitPrice;

        public decimal UnitPrice
        {
            get { return _unitPrice; }
            set
            {
                _unitPrice = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        #region Result : AddItemResult
        private Result _addItemResult;

        public Result AddItemResult
        {
            get { return _addItemResult; }
            set
            {
                _addItemResult = value;
                RaisePropertyChanged();
            }
        }
        #endregion
    }
}