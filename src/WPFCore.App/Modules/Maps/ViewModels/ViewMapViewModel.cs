using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Ogc;
using WPFCore.App.Shared.ViewModels;

namespace WPFCore.App.Modules.Maps.ViewModels;

public sealed partial class ViewMapViewModel : ViewModelBase
{
    [ObservableProperty]
    private Map _map;

    // Delegates (Hành động) để ViewModel gọi UI (View) thực hiện hiệu ứng Zoom
    // mà không phá vỡ nguyên tắc MVVM (không tham chiếu trực tiếp đến MapView)
    public Action<Envelope>? ZoomToExtentAction { get; set; }
    public Action<double>? ZoomByFactorAction { get; set; }

    public ViewMapViewModel()
    {
        Title = "Hiển thị bản đồ";
        
        // Khởi tạo bản đồ trống (Offline 100%), không gọi server Esri nên không cần API Key
        _map = new Map();
    }

    [RelayCommand]
    private async Task LoadFileAsync()
    {
        // Mở hộp thoại chọn file
        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Filter = "Map Files (*.shp;*.gpkg;*.kml;*.kmz)|*.shp;*.gpkg;*.kml;*.kmz|All files (*.*)|*.*",
            Title = "Chọn file dữ liệu bản đồ"
        };

        if (dialog.ShowDialog() == true)
        {
            await LoadLocalMapFileAsync(dialog.FileName);
        }
    }

    public async Task LoadLocalMapFileAsync(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
        {
            MessageBox.Show("File không tồn tại!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        string extension = Path.GetExtension(filePath).ToLower();
        Layer? newLayer = null;

        try
        {
            // Kiểm tra đuôi file và khởi tạo Layer tương ứng
            switch (extension)
            {
                case ".shp":
                    var shapefileTable = await ShapefileFeatureTable.OpenAsync(filePath);
                    newLayer = new FeatureLayer(shapefileTable);
                    break;

                case ".gpkg":
                    var geoPackage = await GeoPackage.OpenAsync(filePath);
                    // Lấy bảng (table) đầu tiên trong GeoPackage
                    var featureTable = geoPackage.GeoPackageFeatureTables.FirstOrDefault();
                    if (featureTable != null)
                    {
                        newLayer = new FeatureLayer(featureTable);
                    }
                    else
                    {
                        throw new Exception("Không tìm thấy Feature Table nào trong GeoPackage.");
                    }
                    break;

                case ".kml":
                case ".kmz":
                    var kmlDataset = new KmlDataset(new Uri(filePath));
                    await kmlDataset.LoadAsync();
                    newLayer = new KmlLayer(kmlDataset);
                    break;

                default:
                    throw new NotSupportedException($"Định dạng file '{extension}' chưa được hỗ trợ.");
            }

            // Nếu tạo layer thành công
            if (newLayer != null)
            {
                // Nạp metadata của layer để lấy FullExtent
                await newLayer.LoadAsync();
                
                // Thêm vào bản đồ
                Map.OperationalLayers.Add(newLayer);

                // Tự động Zoom đến vùng của Layer mới
                if (newLayer.FullExtent != null)
                {
                    ZoomToExtentAction?.Invoke(newLayer.FullExtent);
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi khi nạp dữ liệu bản đồ:\n{ex.Message}", "Lỗi dữ liệu", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private void ZoomIn()
    {
        // Thu phóng x 0.5 (zoom gần hơn)
        ZoomByFactorAction?.Invoke(0.5); 
    }

    [RelayCommand]
    private void ZoomOut()
    {
        // Thu phóng x 2.0 (zoom xa hơn)
        ZoomByFactorAction?.Invoke(2.0); 
    }

    [RelayCommand]
    private void ResetView()
    {
        // Tính tổng vùng (Extent) của tất cả các layer đang có trên bản đồ
        Envelope? combinedExtent = null;
        
        foreach (var layer in Map.OperationalLayers)
        {
            if (layer.FullExtent != null)
            {
                if (combinedExtent == null)
                {
                    combinedExtent = layer.FullExtent;
                }
                else
                {
                    // Lỗi ArgumentException xảy ra nếu 2 layer khác hệ tọa độ (SpatialReference).
                    // Cách khắc phục: Project (chuyển đổi hệ tọa độ) layer hiện tại về cùng hệ với combinedExtent
                    if (layer.FullExtent.SpatialReference != null && 
                        combinedExtent.SpatialReference != null &&
                        !layer.FullExtent.SpatialReference.IsEqual(combinedExtent.SpatialReference))
                    {
                        var projectedExtent = GeometryEngine.Project(layer.FullExtent, combinedExtent.SpatialReference) as Envelope;
                        if (projectedExtent != null)
                        {
                            combinedExtent = GeometryEngine.CombineExtents(combinedExtent, projectedExtent);
                        }
                    }
                    else
                    {
                        // Nếu cùng hệ tọa độ thì gộp bình thường
                        combinedExtent = GeometryEngine.CombineExtents(combinedExtent, layer.FullExtent);
                    }
                }
            }
        }

        // Thực hiện Zoom
        if (combinedExtent != null)
        {
            ZoomToExtentAction?.Invoke(combinedExtent);
        }
    }
}
