using Microsoft.AspNetCore.Components;
using Mkx.Templates.Client.Components.Tests;
using Mkx.Templates.Sdk.Server.Shared.Data;
using Mkx.Templates.Shared.Abstractions;
using Mkx.Templates.Shared.DTOs.Tests;
using MudBlazor;

namespace Mkx.Templates.Client.Pages;

public partial class Tests
{
    private MudTable<GetTestResponse>? _table;
    private string? _search;
    private bool _loadFailed;
    [SupplyParameterFromQuery(Name = "search")] public string? Search { get; set; }
    protected override void OnParametersSet() => _search = Search;
    private async Task<TableData<GetTestResponse>> LoadAsync(TableState state, CancellationToken token)
    {
        var filter = new RequestFilter(state.Page * state.PageSize, state.PageSize, _search, state.SortLabel,
            state.SortDirection == SortDirection.Descending ? Sdk.Server.Shared.Enums.SortDirection.Descending : Sdk.Server.Shared.Enums.SortDirection.Ascending);
        var result = await TryRequestAsync<ITestService, PagedList<GetTestResponse>>((service, _) => service.GetAllAsync(filter, token));
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
    private async Task EditAsync(GetTestResponse? item)
    {
        var dialog = await DialogService.ShowAsync<TestEditor>(item is null ? "افزودن تست" : "ویرایش تست",
            new DialogParameters<TestEditor> { { x => x.Item, item } },
            new DialogOptions { MaxWidth = MaxWidth.Small, FullWidth = true, BackdropClick = false, CloseOnEscapeKey = false });
        if (await dialog.Result is { Canceled: false }) await ReloadAsync();
    }
    private async Task DeleteAsync(GetTestResponse item)
    {
        if (IsBusy || await DialogService.ShowMessageBoxAsync("حذف تست", $"تست «{item.Name}» حذف شود؟", yesText: "حذف", cancelText: "انصراف") != true) return;
        var result = await TryRequestAsync<ITestService, bool>(async (service, token) => { await service.DeleteAsync(item.Id, item.Version, token); return true; });
        if (result.Succeeded) { AddSuccessToast("حذف شد."); await ReloadAsync(); }
    }
}
