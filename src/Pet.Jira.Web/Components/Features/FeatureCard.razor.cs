using Microsoft.AspNetCore.Components;
using MudBlazor;
using Pet.Jira.Web.Shared;
using System;
using System.Threading.Tasks;

namespace Pet.Jira.Web.Components.Features
{
    public partial class FeatureCard : ComponentBase
    {
        private const int NewThresholdDays = 14;

        [Parameter] public FeatureSummary Feature { get; set; } = default!;

        [Inject] private IFeatureCatalogService FeatureCatalogService { get; init; } = default!;
        [Inject] private IDialogService DialogService { get; init; } = default!;
        [CascadingParameter] public ErrorHandler ErrorHandler { get; set; } = default!;

        private bool IsNew =>
            Feature.Metadata.Date >= DateOnly.FromDateTime(DateTime.Today).AddDays(-NewThresholdDays);

        private async Task OpenDetailAsync()
        {
            try
            {
                var detail = await FeatureCatalogService.GetFeatureAsync(Feature.Metadata.Id);
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
