using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using static Kazan_Session1_Mobile_14_9.GlobalClass;

namespace Kazan_Session1_Mobile_14_9
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AssetHistory : ContentPage
    {
        long _assetID = 0;
        public AssetHistory(long AssetID)
        {
            InitializeComponent();
            _assetID = AssetID;
        }

        protected async override void OnAppearing()
        {
            base.OnAppearing();
            await LoadData();
        }

        private async Task LoadData()
        {
            var client = new WebApi();
            var response = await client.PostAsync(null, $"AssetTransferLogs/GetCustomHistory?AssetID={_assetID}");
            var listOfHistory = JsonConvert.DeserializeObject<List<CustomHistory>>(response);
            if (listOfHistory.Count == 0)
            {
                lblNoHistory.IsVisible = true;
                lvHistory.IsVisible = false;
            }
            else
            {
                lblNoHistory.IsVisible = false;
                lvHistory.IsVisible = true;
                lvHistory.ItemsSource = listOfHistory;
            }
        }
    }
}