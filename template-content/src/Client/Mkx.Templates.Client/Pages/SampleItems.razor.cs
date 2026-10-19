using Microsoft.AspNetCore.Components;
using Mkx.Templates.Client.Components.SampleItems;
using Mkx.Templates.Sdk.Server.Shared.Data;
using Mkx.Templates.Shared.Abstractions;
using Mkx.Templates.Shared.DTOs.SampleItems;
using Mkx.Templates.Client.Components.UI;

namespace Mkx.Templates.Client.Pages;

public partial class SampleItems
{
    private UiTable<GetSampleItemResponse>? _table;
    private string? _search;
    private bool _loadFailed;
    [SupplyParameterFromQuery(Name = "search")] public string? Search { get; set; }
    [SupplyParameterFromQuery(Name = "create")] public bool Create { get; set; }
    private bool _createHandled;
    protected override void OnParametersSet()
    {
        _search = Search;
        if (!Create) _createHandled = false;
    }
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!Create || _createHandled) return;
        _createHandled = true;
        Navigation.NavigateTo(Navigation.GetUriWithQueryParameter("create", (bool?)null), replace: true);
        if (await HasPolicyAsync(Shared.Authorization.AppPolicies.SampleItems.Manage)) await EditAsync(null);
    }
    private async Task<TableData<GetSampleItemResponse>> LoadAsync(TableState state, CancellationToken token)
    {
        var filter = new RequestFilter(state.Page * state.PageSize, state.PageSize, _search, state.SortLabel,
            state.SortDirection == SortDirection.Descending ? Sdk.Server.Shared.Enums.SortDirection.Descending : Sdk.Server.Shared.Enums.SortDirection.Ascending);
        var result = await TryRequestAsync<ISampleItemService, PagedList<GetSampleItemResponse>>((service, _) => service.GetAllAsync(filter, token));
        _loadFailed = !result.Succeeded;
        return new() { Items = result.Value?.Data ?? [], TotalItems = result.Value?.Total ?? 0 };
    }
    private async Task SearchChanged(string? value)
    {
        _search = value;
        Navigation.NavigateTo(Navigation.GetUriWithQueryParameter("search", value), replace: true);
        if (_table is not null) { _table.NavigateTo(0); await _table.ReloadServerData(); }
    }
    private Task ReloadAsync() => _table?.ReloadServerData() ?? Task.CompletedTask;
    private async Task EditAsync(GetSampleItemResponse? item)
    {
        var dialog = await DialogService.ShowAsync<SampleItemEditor>(item is null ? "افزودن نمونه" : "ویرایش نمونه",
            new DialogParameters<SampleItemEditor> { { x => x.Item, item } },
            new DialogOptions { MaxWidth = MaxWidth.Small, FullWidth = true, BackdropClick = false, CloseOnEscapeKey = false });
        if (await dialog.Result is { Canceled: false }) await ReloadAsync();
    }
    private async Task DeleteAsync(GetSampleItemResponse item)
    {
        if (IsBusy || await DialogService.ShowMessageBoxAsync("حذف نمونه", $"نمونه «{item.Name}» حذف شود؟", yesText: "حذف", cancelText: "انصراف") != true) return;
        var result = await TryRequestAsync<ISampleItemService, bool>(async (service, token) => { await service.DeleteAsync(item.Id, item.Version, token); return true; });
        if (result.Succeeded) { AddSuccessToast("حذف شد."); await ReloadAsync(); }
    }
}
