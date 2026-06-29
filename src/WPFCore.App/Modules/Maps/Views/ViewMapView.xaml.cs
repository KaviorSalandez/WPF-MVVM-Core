using System.Windows.Controls;

namespace WPFCore.App.Modules.Maps.Views;

public partial class ViewMapView : UserControl
{
    public ViewMapView()
    {
        InitializeComponent();

        // Đăng ký sự kiện khi DataContext (ViewModel) thay đổi
        this.DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
    {
        if (e.NewValue is ViewModels.ViewMapViewModel vm)
        {
            // Móc nối Action Zoom đến Extent
            vm.ZoomToExtentAction = async (extent) =>
            {
                if (extent != null)
                {
                    await MainMapView.SetViewpointGeometryAsync(extent);
                }
            };

            // Móc nối Action Zoom In/Out theo tỷ lệ (factor)
            vm.ZoomByFactorAction = async (factor) =>
            {
                var currentViewpoint = MainMapView.GetCurrentViewpoint(Esri.ArcGISRuntime.Mapping.ViewpointType.CenterAndScale);
                if (currentViewpoint != null && currentViewpoint.TargetGeometry is Esri.ArcGISRuntime.Geometry.MapPoint center)
                {
                    double newScale = currentViewpoint.TargetScale * factor;
                    await MainMapView.SetViewpointAsync(new Esri.ArcGISRuntime.Mapping.Viewpoint(center, newScale));
                }
            };
        }
    }
}
