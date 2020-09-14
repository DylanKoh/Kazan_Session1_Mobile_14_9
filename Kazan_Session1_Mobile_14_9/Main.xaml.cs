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
    public partial class Main : ContentPage
    {
        List<CustomList> _assetList;
        List<Department> _departments;
        List<AssetGroup> _assetGroups;
        public Main()
        {
            InitializeComponent();
        }
        protected async override void OnAppearing()
        {
            base.OnAppearing();
            await LoadData();
            await LoadPickers();

        }

        private async Task LoadPickers()
        {
            pDepartment.Items.Clear();
            pAssetGroup.Items.Clear();
            pDepartment.Items.Add("No Filter");
            pAssetGroup.Items.Add("No Filter");
            pDepartment.SelectedIndex = 0;
            pAssetGroup.SelectedIndex = 0;
            var client = new WebApi();
            var departmentsResponse = await client.PostAsync(null, "Departments");
            _departments = JsonConvert.DeserializeObject<List<Department>>(departmentsResponse);
            foreach (var item in _departments)
            {
                pDepartment.Items.Add(item.Name);
            }
            var assetGroupResponse = await client.PostAsync(null, "AssetGroups");
            _assetGroups = JsonConvert.DeserializeObject<List<AssetGroup>>(assetGroupResponse);
            foreach (var item in _assetGroups)
            {
                pAssetGroup.Items.Add(item.Name);
            }
        }

        private async Task LoadData()
        {
            lblNumber.Text = "assets out of";
            var client = new WebApi();
            var assetResponse = await client.PostAsync(null, "Assets/GetCustomView");
            _assetList = JsonConvert.DeserializeObject<List<CustomList>>(assetResponse);
            lvAssets.ItemsSource = _assetList;
            lblNumber.Text = $"{_assetList.Count()} {lblNumber.Text} {_assetList.Count()}";
        }

        private void FilterAssets()
        {
            
            if (pAssetGroup.SelectedItem != null && pDepartment.SelectedItem != null)
            {
                lblNumber.Text = "assets out of";
                if (pAssetGroup.SelectedItem.ToString() == "No Filter" && pDepartment.SelectedItem.ToString() == "No Filter" && (pDepartment.Items.Count != 0 && pAssetGroup.Items.Count != 0))
                {
                    if (string.IsNullOrWhiteSpace(searchBar.Text))
                    {
                        var filteredList = (from x in _assetList
                                            where x.WarrantyDate >= dpStart.Date && x.WarrantyDate <= dpEnd.Date
                                            select x).ToList();
                        lvAssets.ItemsSource = filteredList;
                        var total = _assetList.Count();
                        var current = filteredList.Count();
                        lblNumber.Text = $"{current} {lblNumber.Text} {total}";
                    }
                    else
                    {
                        var filteredList = (from x in _assetList
                                            where x.WarrantyDate >= dpStart.Date && x.WarrantyDate <= dpEnd.Date
                                            where x.AssetSN.ToLower().Contains(searchBar.Text.ToLower()) || x.AssetName.ToLower().Contains(searchBar.Text.ToLower())
                                            select x).ToList();
                        lvAssets.ItemsSource = filteredList;
                        var total = _assetList.Count();
                        var current = filteredList.Count();
                        lblNumber.Text = $"{current} {lblNumber.Text} {total}";
                    }
                }
                else if (pAssetGroup.SelectedItem.ToString() == "No Filter" && pDepartment.SelectedItem.ToString() != "No Filter" && (pDepartment.Items.Count != 0 && pAssetGroup.Items.Count != 0))
                {
                    if (string.IsNullOrWhiteSpace(searchBar.Text))
                    {
                        var filteredList = (from x in _assetList
                                            where x.WarrantyDate >= dpStart.Date && x.WarrantyDate <= dpEnd.Date
                                            where x.DepartmentName == pDepartment.SelectedItem.ToString()
                                            select x).ToList();
                        lvAssets.ItemsSource = filteredList;
                        var total = _assetList.Count();
                        var current = filteredList.Count();
                        lblNumber.Text = $"{current} {lblNumber.Text} {total}";
                    }
                    else
                    {
                        var filteredList = (from x in _assetList
                                            where x.WarrantyDate >= dpStart.Date && x.WarrantyDate <= dpEnd.Date
                                            where x.DepartmentName == pDepartment.SelectedItem.ToString()
                                            where x.AssetSN.ToLower().Contains(searchBar.Text.ToLower()) || x.AssetName.ToLower().Contains(searchBar.Text.ToLower())
                                            select x).ToList();
                        lvAssets.ItemsSource = filteredList;
                        var total = _assetList.Count();
                        var current = filteredList.Count();
                        lblNumber.Text = $"{current} {lblNumber.Text} {total}";
                    }
                }
                else if (pAssetGroup.SelectedItem.ToString() != "No Filter" && pDepartment.SelectedItem.ToString() == "No Filter" && (pDepartment.Items.Count != 0 && pAssetGroup.Items.Count != 0))
                {
                    if (string.IsNullOrWhiteSpace(searchBar.Text))
                    {
                        var filteredList = (from x in _assetList
                                            where x.WarrantyDate >= dpStart.Date && x.WarrantyDate <= dpEnd.Date
                                            where x.AssetGroup == pAssetGroup.SelectedItem.ToString()
                                            select x).ToList();
                        lvAssets.ItemsSource = filteredList;
                        var total = _assetList.Count();
                        var current = filteredList.Count();
                        lblNumber.Text = $"{current} {lblNumber.Text} {total}";
                    }
                    else
                    {
                        var filteredList = (from x in _assetList
                                            where x.WarrantyDate >= dpStart.Date && x.WarrantyDate <= dpEnd.Date
                                            where x.AssetGroup == pAssetGroup.SelectedItem.ToString()
                                            where x.AssetSN.ToLower().Contains(searchBar.Text.ToLower()) || x.AssetName.ToLower().Contains(searchBar.Text.ToLower())
                                            select x).ToList();
                        lvAssets.ItemsSource = filteredList;
                        var total = _assetList.Count();
                        var current = filteredList.Count();
                        lblNumber.Text = $"{current} {lblNumber.Text} {total}";
                    }
                }
                else if (pAssetGroup.SelectedItem.ToString() != "No Filter" && pDepartment.SelectedItem.ToString() != "No Filter" && (pDepartment.Items.Count != 0 && pAssetGroup.Items.Count != 0))
                {
                    if (string.IsNullOrWhiteSpace(searchBar.Text))
                    {
                        var filteredList = (from x in _assetList
                                            where x.WarrantyDate >= dpStart.Date && x.WarrantyDate <= dpEnd.Date
                                            where x.AssetGroup == pAssetGroup.SelectedItem.ToString() && x.DepartmentName == pDepartment.SelectedItem.ToString()
                                            select x).ToList();
                        lvAssets.ItemsSource = filteredList;
                        var total = _assetList.Count();
                        var current = filteredList.Count();
                        lblNumber.Text = $"{current} {lblNumber.Text} {total}";
                    }
                    else
                    {
                        var filteredList = (from x in _assetList
                                            where x.WarrantyDate >= dpStart.Date && x.WarrantyDate <= dpEnd.Date
                                            where x.AssetGroup == pAssetGroup.SelectedItem.ToString() && x.DepartmentName == pDepartment.SelectedItem.ToString()
                                            where x.AssetSN.ToLower().Contains(searchBar.Text.ToLower()) || x.AssetName.ToLower().Contains(searchBar.Text.ToLower())
                                            select x).ToList();
                        lvAssets.ItemsSource = filteredList;
                        var total = _assetList.Count();
                        var current = filteredList.Count();
                        lblNumber.Text = $"{current} {lblNumber.Text} {total}";
                    }
                }
            }
        }

        private void btnEdit_Clicked(object sender, EventArgs e)
        {
            Console.WriteLine("Hello");

        }

        private void btnMove_Clicked(object sender, EventArgs e)
        {
            Console.WriteLine("Hello");
        }

        private void btnHistory_Clicked(object sender, EventArgs e)
        {
            Console.WriteLine("Hello");
        }

        private void pDepartment_SelectedIndexChanged(object sender, EventArgs e)
        {
            FilterAssets();
        }

        private void pAssetGroup_SelectedIndexChanged(object sender, EventArgs e)
        {
            FilterAssets();
        }

        private void searchBar_TextChanged(object sender, TextChangedEventArgs e)
        {
            FilterAssets();
        }

        private void dpStart_DateSelected(object sender, DateChangedEventArgs e)
        {
            FilterAssets();
        }

        private void dpEnd_DateSelected(object sender, DateChangedEventArgs e)
        {
            FilterAssets();
        }

    }
}
