using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using static Kazan_Session1_Mobile_14_9.GlobalClass;

namespace Kazan_Session1_Mobile_14_9
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MoveAsset : ContentPage
    {
        long _assetID = 0;
        List<Department> _departmentList;
        List<DepartmentLocation> _departmentLocationList;
        List<Location> _locationList;
        List<AssetTransferLog> _transferLogList;
        Asset _asset;
        public MoveAsset(long AssetID)
        {
            InitializeComponent();
            _assetID = AssetID;
        }
        protected async override void OnAppearing()
        {
            base.OnAppearing();
            await LoadPickers();
            await LoadData();
        }

        private async Task LoadData()
        {
            var client = new WebApi();
            var assetResponse = await client.PostAsync(null, $"Assets/Details/{_assetID}");
            _asset = JsonConvert.DeserializeObject<Asset>(assetResponse);
            entryAssetName.Text = _asset.AssetName;
            entryAssetSN.Text = _asset.AssetSN;
            entryDepartment.Text = (from x in _departmentLocationList
                                    where _asset.DepartmentLocationID == x.ID
                                    join y in _departmentList on x.DepartmentID equals y.ID
                                    select y.Name).FirstOrDefault();
            lblAssetSN.Text = $"??/{_asset.AssetGroupID.ToString().PadLeft(2, '0')}/????";
        }

        private async Task LoadPickers()
        {
            pLocation.Items.Clear();
            pDepartment.Items.Clear();
            var client = new WebApi();
            var departmentResponse = await client.PostAsync(null, "Departments");
            _departmentList = JsonConvert.DeserializeObject<List<Department>>(departmentResponse);
            foreach (var item in _departmentList)
            {
                if (item.Name == entryDepartment.Text)
                {
                    continue;
                }
                pDepartment.Items.Add(item.Name);
            }
            var departmentLocationResponse = await client.PostAsync(null, "DepartmentLocations");
            _departmentLocationList = JsonConvert.DeserializeObject<List<DepartmentLocation>>(departmentLocationResponse);

            var locationResponse = await client.PostAsync(null, "Locations");
            _locationList = JsonConvert.DeserializeObject<List<Location>>(locationResponse);

            var transferLogResponse = await client.PostAsync(null, "AssetTransferLogs");
            _transferLogList = JsonConvert.DeserializeObject<List<AssetTransferLog>>(transferLogResponse);
        }

        private void pDepartment_SelectedIndexChanged(object sender, EventArgs e)
        {
            pLocation.Items.Clear();
            LoadLocations();
        }

        private void LoadLocations()
        {
            var getLocations = (from x in _departmentList
                                where x.Name == pDepartment.SelectedItem.ToString()
                                join y in _departmentLocationList on x.ID equals y.DepartmentID
                                where y.EndDate == null
                                join z in _locationList on y.LocationID equals z.ID
                                select z);
            foreach (var item in getLocations)
            {
                pLocation.Items.Add(item.Name);
            }
        }

        private void pLocation_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (pDepartment.SelectedItem != null && pLocation.SelectedItem != null)
            {
                CalculateNewSN();
            }
        }

        private async void CalculateNewSN()
        {
            var client = new WebApi();
            var response = await client.PostAsync(null, "Assets/GetAllSN");
            var listOfSN = JsonConvert.DeserializeObject<List<string>>(response);
            var getDepartmentLocationID = (from x in _departmentLocationList
                                           join y in _departmentList on x.DepartmentID equals y.ID
                                           join z in _locationList on x.LocationID equals z.ID
                                           where y.Name == pDepartment.SelectedItem.ToString() && z.Name == pLocation.SelectedItem.ToString()
                                           select x.ID).FirstOrDefault();
            var findPreviousRecords = (from x in _transferLogList
                                       where x.FromDepartmentLocationID == getDepartmentLocationID && x.ToDepartmentLocationID == _asset.DepartmentLocationID && x.AssetID == _asset.ID
                                       select x).FirstOrDefault();
            if (findPreviousRecords != null)
            {
                lblAssetSN.Text = findPreviousRecords.FromAssetSN;
            }
            else
            {
                var getDepartmentID = (from x in _departmentList
                                       where x.Name == pDepartment.SelectedItem.ToString()
                                       select x.ID).FirstOrDefault();
                var dd = getDepartmentID.ToString().PadLeft(2, '0');
                var aa = _asset.AssetGroupID.ToString().PadLeft(2, '0');
                var ddaa = $"{dd}/{aa}";
                var getLastestValue = (from x in listOfSN
                                       where x.Contains(ddaa)
                                       orderby x descending
                                       select x).FirstOrDefault();
                var nnnn = string.Empty;
                if (getLastestValue != null)
                {
                    nnnn = (int.Parse(getLastestValue.Split('/')[2]) + 1).ToString().PadLeft(4, '0');
                }
                else
                {
                    nnnn = 1.ToString().PadLeft(4, '0');
                }
                var newSN = $"{ddaa}/{nnnn}";
                lblAssetSN.Text = newSN;
            }
        }

        private async void btnSubmit_Clicked(object sender, EventArgs e)
        {
            if (pDepartment.SelectedItem == null || pLocation.SelectedItem == null)
            {
                await DisplayAlert("Transfer Assset", "Please ensure that the destinations are selected!", "Ok");
            }
            else
            {
                var getNewDepartmentLocationID = (from x in _departmentLocationList
                                                  join y in _departmentList on x.DepartmentID equals y.ID
                                                  join z in _locationList on x.LocationID equals z.ID
                                                  where y.Name == pDepartment.SelectedItem.ToString() && z.Name == pLocation.SelectedItem.ToString()
                                                  select x.ID).FirstOrDefault();
                var newTransfer = new AssetTransferLog()
                {
                    AssetID = _asset.ID,
                    FromAssetSN = _asset.AssetSN,
                    ToAssetSN = lblAssetSN.Text,
                    TransferDate = DateTime.Now,
                    FromDepartmentLocationID = _asset.DepartmentLocationID,
                    ToDepartmentLocationID = getNewDepartmentLocationID
                };
                var client = new WebApi();
                var jsonData = JsonConvert.SerializeObject(newTransfer);
                var response = await client.PostAsync(jsonData, "AssetTransferLogs/Create");
                if (response == "\"Completed Transfer!\"")
                {
                    await DisplayAlert("Transfer Asset", "Completed Transfer!", "Ok");
                    _asset.AssetSN = lblAssetSN.Text;
                    _asset.DepartmentLocationID = getNewDepartmentLocationID;
                    var jsonData2 = JsonConvert.SerializeObject(_asset);
                    var responseAsset = await client.PostAsync(jsonData2, "Assets/Edit");
                    if (responseAsset == "\"Successfully edited Asset!\"")
                    {
                        await DisplayAlert("Edit Asset", "Successfully edited Asset!", "Ok");
                        await Navigation.PopAsync();
                    }
                    else
                    {
                        await DisplayAlert("Edit Asset", "An error occured while editing Asset! Please contact our administrator!", "Ok");
                    }
                }
                else
                {
                    await DisplayAlert("Transfer Asset", "Unable to transfer Asset! Please check and try again!", "Ok");
                }
            }

        }

        private async void btnCancel_Clicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }
    }
}