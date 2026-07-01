using Microsoft.AspNetCore.Components;
using MudBlazor;
using Pet.Jira.Web.Shared;
using System;
using System.Threading.Tasks;

namespace Pet.Jira.Web.Components.Features
{
    public partial class RandomFeatureViewer : ComponentBase
    {
        /// <summary>
        /// Shown when there are no features to display.
        /// </summary>
        [Parameter] public string FallbackMessage { get; set; } = string.Empty;

        [Inject] private IFeatureCatalogService FeatureCatalogService { get; init; } = default!;
        [Inject] private IDialogService DialogService { get; init; } = default!;
        [CascadingParameter] public ErrorHandler ErrorHandler { get; set; } = default!;

        private FeatureSummary _feature;
        private bool _isLoading = true;

        protected override async Task OnInitializedAsync()
        {
            try
            {
                _feature = await FeatureCatalogService.GetRandomFeatureAsync();
            }
            catch (Exception e)
            {
                ErrorHandler.ProcessError(e);
            }
            finally
            {
                _isLoading = false;
            }
        }

        private async Task OpenDetailAsync()
        {
            try
            {
                var detail = await FeatureCatalogService.GetFeatureAsync(_feature.Metadata.Id);
                if (detail is null)
                {
                    return;
                }

                var parameters = new DialogParameters
                {
                    { nameof(FeatureDetailDialog.Detail), detail }
                };
                var options = new DialogOptions
                {
                    MaxWidth = MaxWidth.Medium,
                    FullWidth = true,
                    CloseButton = true
                };
                await DialogService.ShowAsync<FeatureDetailDialog>(detail.Metadata.Title, parameters, options);
            }
            catch (Exception e)
            {
                ErrorHandler.ProcessError(e);
            }
        }
    }
}
