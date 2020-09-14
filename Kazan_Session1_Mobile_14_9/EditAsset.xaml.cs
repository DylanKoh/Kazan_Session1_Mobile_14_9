using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using static Kazan_Session1_Mobile_14_9.GlobalClass;

namespace Kazan_Session1_Mobile_14_9
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class EditAsset : ContentPage
    {
        List<AssetGroup> _assetGroupList;
        List<Department> _departmentList;
        List<DepartmentLocation> _departmentLocationList;
        List<Location> _locationList;
        List<Employee> _employeeList;
        Asset _asset;
        long _assetID = 0;
        public EditAsset(long AssetID)
        {
            InitializeComponent();
            _assetID = AssetID;
        }

        protected async override void OnAppearing()
        {
            base.OnAppearing();
            await LoadPickers();
            if (_assetID != 0)
            {
                await LoadData();
                entryAssetName.IsEnabled = false;
                pLocation.IsEnabled = false;
                pDepartment.IsEnabled = false;
                pAssetGroup.IsEnabled = false;
            }
        }

        private async Task LoadPickers()
        {
            pAccountable.Items.Clear();
            pAssetGroup.Items.Clear();
            pDepartment.Items.Clear();
            pLocation.Items.Clear();
            var client = new WebApi();
            var assetGroupResponse = await client.PostAsync(null, "AssetGroups");
            _assetGroupList = JsonConvert.DeserializeObject<List<AssetGroup>>(assetGroupResponse);
            foreach (var item in _assetGroupList)
            {
                pAssetGroup.Items.Add(item.Name);
            }

            var departmentResponse = await client.PostAsync(null, "Departments");
            _departmentList = JsonConvert.DeserializeObject<List<Department>>(departmentResponse);
            foreach (var item in _departmentList)
            {
                pDepartment.Items.Add(item.Name);
            }

            var departmentLocationResponse = await client.PostAsync(null, "DepartmentLocations");
            _departmentLocationList = JsonConvert.DeserializeObject<List<DepartmentLocation>>(departmentLocationResponse);

            var locationResponse = await client.PostAsync(null, "Locations");
            _locationList = JsonConvert.DeserializeObject<List<Location>>(locationResponse);

            var employeeResponse = await client.PostAsync(null, "Employees");
            _employeeList = JsonConvert.DeserializeObject<List<Employee>>(employeeResponse);
            foreach (var item in _employeeList)
            {
                pAccountable.Items.Add($"{item.FirstName} {item.LastName}");
            }
        }

        private async Task LoadData()
        {
            var client = new WebApi();
            var assetResponse = await client.PostAsync(null, $"Assets/Details/{_assetID}");
            _asset = JsonConvert.DeserializeObject<Asset>(assetResponse);
            var assetGroup = (from x in _assetGroupList
                              where x.ID == _asset.AssetGroupID
                              select x.Name).FirstOrDefault();
            entryAssetName.Text = _asset.AssetName;
            editorDescription.Text = _asset.Description;
            dpWarranty.Date = (DateTime)_asset.WarrantyDate;
            lblAssetSN.Text = _asset.AssetSN;
            pAssetGroup.SelectedItem = assetGroup;
            var getDepartmentLocation = (from x in _departmentLocationList
                                         where _asset.DepartmentLocationID == x.ID
                                         select x).FirstOrDefault();
            pDepartment.SelectedItem = (from x in _departmentList
                                        where x.ID == getDepartmentLocation.DepartmentID
                                        select x.Name).FirstOrDefault();
            pLocation.SelectedItem = (from x in _locationList
                                      where x.ID == getDepartmentLocation.LocationID
                                      select x.Name).FirstOrDefault();

            pAccountable.SelectedItem = (from x in _employeeList
                                         where x.ID == _asset.EmployeeID
                                         select x.FirstName + " " + x.LastName).FirstOrDefault();
        }

        private void pDepartment_SelectedIndexChanged(object sender, EventArgs e)
        {
            pLocation.Items.Clear();
            pLocation.IsEnabled = true;
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
            if (pAssetGroup.SelectedItem != null && pLocation.SelectedItem != null)
            {
                CalculateAssetSN();
            }
        }

        private async void CalculateAssetSN()
        {
            var client = new WebApi();
            var response = await client.PostAsync(null, "Assets/GetAllSN");
            var listOfSN = JsonConvert.DeserializeObject<List<string>>(response);
            var getAssetID = (from x in _assetGroupList
                              where x.Name == pAssetGroup.SelectedItem.ToString()
                              select x.ID).FirstOrDefault();
            var getDepartmentID = (from x in _departmentList
                                   where x.Name == pDepartment.SelectedItem.ToString()
                                   select x.ID).FirstOrDefault();
            var dd = getDepartmentID.ToString().PadLeft(2, '0');
            var aa = getAssetID.ToString().PadLeft(2, '0');
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

        private void pAssetGroup_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (pDepartment.SelectedItem != null && pLocation.SelectedItem != null)
            {
                CalculateAssetSN();
            }
        }


        private async void btnSubmit_Clicked(object sender, EventArgs e)
        {
            if (pLocation.SelectedItem == null || pAccountable.SelectedItem == null || pAssetGroup.SelectedItem == null || string.IsNullOrWhiteSpace(entryAssetName.Text))
            {
                await DisplayAlert("Submit Changes", "Please ensure all fields are filled!", "Ok");
            }
            else
            {
                var client = new WebApi();
                if (_assetID != 0)
                {
                    _asset.EmployeeID = (from x in _employeeList
                                         where x.FirstName + " " + x.LastName == pAccountable.SelectedItem.ToString()
                                         select x.ID).FirstOrDefault();
                    if (string.IsNullOrWhiteSpace(editorDescription.Text))
                    {
                        _asset.Description = " ";
                    }
                    else
                    {
                        _asset.Description = editorDescription.Text;
                    }
                    _asset.WarrantyDate = dpWarranty.Date;
                    var jsonData = JsonConvert.SerializeObject(_asset);
                    var response = await client.PostAsync(jsonData, "Assets/Edit");
                    if (response == "\"Successfully edited Asset!\"")
                    {
                        await DisplayAlert("Edit Asset", "Successfully edited Asset!", "Ok");
                        await Navigation.PopAsync();
                    }
                    else
                    {
                        await DisplayAlert("Edit Asset", "An error occured while editing Asset! Please check and try again!", "Ok");
                    }
                }
                else
                {
                    var getAssetGroupID = (from x in _assetGroupList
                                           where x.Name == pAssetGroup.SelectedItem.ToString()
                                           select x.ID).FirstOrDefault();
                    var getDepartmentLoactionID = (from x in _departmentLocationList
                                                   join y in _departmentList on x.DepartmentID equals y.ID
                                                   join z in _locationList on x.LocationID equals z.ID
                                                   where y.Name == pDepartment.SelectedItem.ToString() && z.Name == pLocation.SelectedItem.ToString()
                                                   select x.ID).FirstOrDefault();
                    var getEmployeeID = (from x in _employeeList
                                         where x.FirstName + " " + x.LastName == pAccountable.SelectedItem.ToString()
                                         select x.ID).FirstOrDefault();
                    var newAsset = new Asset()
                    {
                        AssetName = entryAssetName.Text,
                        AssetGroupID = getAssetGroupID,
                        AssetSN = lblAssetSN.Text,
                        DepartmentLocationID = getDepartmentLoactionID,
                        EmployeeID = getEmployeeID
                    };
                    if (dpWarranty.Date == DateTime.Now.Date)
                    {
                        newAsset.WarrantyDate = null;
                    }
                    else
                    {
                        newAsset.WarrantyDate = dpWarranty.Date;
                    }
                    if (string.IsNullOrWhiteSpace(editorDescription.Text))
                    {
                        newAsset.Description = " ";
                    }
                    else
                    {
                        newAsset.Description = editorDescription.Text;
                    }
                    var jsonData = JsonConvert.SerializeObject(newAsset);
                    var response = await client.PostAsync(jsonData, "Assets/Create");
                    if (response == "\"Created Asset!\"")
                    {
                        await DisplayAlert("Add Asset", "Created Asset!", "Ok");
                        await Navigation.PopAsync();
                    }
                    else
                    {
                        await DisplayAlert("Add Asset", "Unable to create Asset! Please check and try again!", "Ok");
                    }
                }
            }
            
        }

        private async void btnCancel_Clicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }
    }
}