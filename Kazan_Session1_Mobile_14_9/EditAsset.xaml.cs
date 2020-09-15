using Newtonsoft.Json;
using Plugin.FilePicker;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
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
        List<AssetPhotoList> _assetPhotoList = new List<AssetPhotoList>();
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
            if (pAccountable.SelectedItem == null || pAssetGroup.SelectedItem == null || pDepartment.SelectedItem == null || pLocation.SelectedItem == null)
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
            if (_assetPhotoList.Count == 0)
            {
                var assetPhotoResponse = await client.PostAsync(null, $"AssetPhotoes/GetPhotoAsset?AssetID={_asset.ID}");
                var assetPhotoList = JsonConvert.DeserializeObject<List<AssetPhoto>>(assetPhotoResponse);
                foreach (var item in assetPhotoList)
                {
                    var image = ImageSource.FromStream(() => new MemoryStream(item.AssetPhoto1));
                    var getNumberOfPhotos = _assetPhotoList.Count();
                    _assetPhotoList.Add(new AssetPhotoList() { AssetPhoto = image, PhotoName = $"Image {getNumberOfPhotos + 1}" });
                }
                lvPhotos.ItemsSource = _assetPhotoList;
            }
           
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
            if (pAssetGroup.SelectedItem != null && pLocation.SelectedItem != null && _assetID == 0)
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
            if (pDepartment.SelectedItem != null && pLocation.SelectedItem != null && _assetID == 0)
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
                        var boolCheck = true;
                        foreach (var item in _assetPhotoList)
                        {
                            byte[] imageArray;
                            var imageContent = await GetStreamFromImageSourceAsync((StreamImageSource)item.AssetPhoto);
                            using (var memoryStream = new MemoryStream())
                            {
                                Stream stream = imageContent;
                                stream.CopyTo(memoryStream);
                                imageArray = memoryStream.ToArray();
                            }
                            var newAssetPhotos = new AssetPhoto()
                            {
                                AssetID = _asset.ID,
                                AssetPhoto1 = imageArray
                            };
                            var jsonData1 = JsonConvert.SerializeObject(newAssetPhotos);
                            var responsePhoto = await client.PostAsync(jsonData1, "AssetPhotoes/Create");
                            if (responsePhoto != "\"Photo(s) saved successfully!\"")
                            {
                                boolCheck = false;
                                break;
                            }
                        }
                        if (boolCheck == false)
                        {
                            await DisplayAlert("Edit Asset", "There was an issue with adding photos! Please contact our administrator!", "Ok");
                        }
                        else
                        {
                            await Navigation.PopAsync();
                        }
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
                        var boolCheck = true;
                        var getAssetIDString = await client.PostAsync(null, $"Assets/GetAssetID?AssetSN={lblAssetSN.Text}");
                        var assetID = long.Parse(JsonConvert.DeserializeObject<string>(getAssetIDString));
                        foreach (var item in _assetPhotoList)
                        {
                            byte[] imageArray;
                            var imageContent = await GetStreamFromImageSourceAsync((StreamImageSource)item.AssetPhoto);
                            using (var memoryStream = new MemoryStream())
                            {
                                Stream stream = imageContent;
                                stream.CopyTo(memoryStream);
                                imageArray = memoryStream.ToArray();
                            }
                            var newAssetPhotos = new AssetPhoto()
                            {
                                AssetID = assetID,
                                AssetPhoto1 = imageArray
                            };
                            var jsonData1 = JsonConvert.SerializeObject(newAssetPhotos);
                            var responsePhoto = await client.PostAsync(jsonData1, " AssetPhotoes/Create");
                            if (responsePhoto != "\"Photo(s) saved successfully!\"")
                            {
                                boolCheck = false;
                                break;
                            }
                        }
                        if (boolCheck == false)
                        {
                            await DisplayAlert("Add Asset", "There was an issue with adding photos! Please contact our administrator!", "Ok");
                        }
                        else
                        {
                            await Navigation.PopAsync();
                        }
                    }
                    else if (response == "\"Asset already exist in the location of choice!\"")
                    {
                        await DisplayAlert("Add Asset", "Asset already exist in the location of choice!", "Ok");
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

        private async void btnCapture_Clicked(object sender, EventArgs e)
        {
            lvPhotos.ItemsSource = null;
            var photo = await Plugin.Media.CrossMedia.Current.TakePhotoAsync(new Plugin.Media.Abstractions.StoreCameraMediaOptions() { });

            if (photo != null)
            {
                var Image = ImageSource.FromStream(() => { return photo.GetStream(); });
                var getNumberOfPhotos = _assetPhotoList.Count();
                _assetPhotoList.Add(new AssetPhotoList() { AssetPhoto = Image, PhotoName = $"Image {getNumberOfPhotos + 1}" });
            }
            lvPhotos.ItemsSource = _assetPhotoList;



        }


        private async void btnBrowse_Clicked(object sender, EventArgs e)
        {
            lvPhotos.ItemsSource = null;
            var file = await CrossFilePicker.Current.PickFile();

            if (file != null)
            {
                var data = file.DataArray;
                var image = ImageSource.FromStream(() => new MemoryStream(data));
                var getNumberOfPhotos = _assetPhotoList.Count();
                _assetPhotoList.Add(new AssetPhotoList() { AssetPhoto = image, PhotoName = $"Image {getNumberOfPhotos + 1}" });
            }
            lvPhotos.ItemsSource = _assetPhotoList;


        }

        private static async Task<Stream> GetStreamFromImageSourceAsync(StreamImageSource imageSource, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (imageSource.Stream != null)
            {
                return await imageSource.Stream(cancellationToken);
            }
            return null;
        }

    }
}