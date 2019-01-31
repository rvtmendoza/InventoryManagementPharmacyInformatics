using System.Threading.Tasks;
using System.Windows.Input;
using Prism.Commands;
using Prism.Mvvm;

namespace InventoryManagementWpf
{
    public class AddShoppingListItemWindowViewModel : BindableBase
    {
        private readonly AddShoppingListItemWindow _window;

        public AddShoppingListItemWindowViewModel(AddShoppingListItemWindow window,string itemName, int unitsInStock)
        {
            window.DataContext = this;
            _window = window;
            ItemName = itemName;
            MaximumQuantity = unitsInStock;

            ConfirmCommand = new DelegateCommand(ExecuteConfirmCommand);
        }

        private void ExecuteConfirmCommand()
        {
            Quantity = DesiredQuantity;
            _window.Close();
        }

        private async void MaximumQuantityExceeded()
        {
            Status = "Quantity entered exceeded maximum quantity";

            await Task.Delay(2000);

            Status = "";
        }

        public int MaximumQuantity { get; set; }
        public int Quantity { get; set; }

        #region ICommand : ConfirmCommand
        private ICommand _confirmCommand;

        public ICommand ConfirmCommand
        {
            get { return _confirmCommand; }
            set 
            { 
                _confirmCommand = value; 
                RaisePropertyChanged();
            }
        }
        #endregion

        #region string : ItemName
        private string _itemName;

        public string ItemName
        {
            get { return _itemName; }
            set 
            { 
                _itemName = value; 
                RaisePropertyChanged();
            }
        }
        #endregion

        #region int : DesiredQuantity
        private int _desiredQuantity;

        public int DesiredQuantity
        {
            get { return _desiredQuantity; }
            set 
            {
                if (value > MaximumQuantity)
                {
                    _desiredQuantity = MaximumQuantity;
                    MaximumQuantityExceeded();
                }
                else
                {
                    _desiredQuantity = value;
                }
                RaisePropertyChanged();
            }
        }
        #endregion

        #region string : Status
        private string _status;

        public string Status
        {
            get { return _status; }
            set 
            { 
                _status = value; 
                RaisePropertyChanged();
            }
        }
        #endregion
    }
}