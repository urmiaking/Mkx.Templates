using Mkx.Templates.Client.Pages.UserAccounts.Components.Editors;
using Mkx.Templates.Client.Pages.UserAccounts.ViewModels;
using Mkx.Templates.Sdk.Server.Shared.Data;
using Mkx.Templates.Shared.Abstractions;
using Mkx.Templates.Shared.DTOs.UserAccounts;
using Mkx.Templates.Shared.Routes;
using MudBlazor;

namespace Mkx.Templates.Client.Pages.UserAccounts;

public partial class AccountsList
{
    private MudTable<GetUserAccountResponse> _table = default!;
    private int _currentPage;
    private string? _searchString;

    private readonly List<BreadcrumbItem> _breadcrumbs =
    [
        new("صفحه اصلی", href: ClientRoutes.Home.Index, icon: Icons.Material.Filled.Home),
        new("مدیریت حساب", href: ClientRoutes.UserAccounts.Index, icon: Icons.Material.Filled.ManageAccounts),
        new("مدیریت کاربران", href: ClientRoutes.UserAccounts.AccountsList, icon: Icons.Material.Filled.People)
    ];

    private readonly DialogOptions _dialogOptions = new ()
    {
        CloseButton = true,
        CloseOnEscapeKey = true,
        MaxWidth = MaxWidth.Medium,
        FullWidth = true
    };

    private static int RowsPerPage => 14;
    private int PageCount => (TotalItems + RowsPerPage - 1) / RowsPerPage;
    private int TotalItems => _table?.GetFilteredItemsCount() ?? 0;
    private int StartItem => TotalItems == 0 ? 0 : (_table?.CurrentPage ?? 0) * RowsPerPage + 1;
    private int EndItem => Math.Min(((_table?.CurrentPage ?? 0) + 1) * RowsPerPage, TotalItems);
    private string InfoText => $"نمایش {StartItem} تا {EndItem} از {TotalItems} ردیف";

    private async Task LockUser(GetUserAccountResponse context)
    {
        var result = await DialogService.ShowMessageBoxAsync("قفل حساب کاربری",
            $"آیا از قفل حساب کاربری {context.FullName} اطمینان دارید؟", "بله", "انصراف");

        if (result == true)
        {
            await SendRequestAsync<IUserAccountService>(
                action: (s, ct) => s.LockUserAsync(context.Id, ct),
                afterSend: async () =>
                {
                    await RefreshAsync();
                    AddSuccessToast($"حساب کاربری {context.FullName} با موفقیت قفل شد");
                });
        }
    }

    private async Task UnlockUser(GetUserAccountResponse context)
    {
        var result = await DialogService.ShowMessageBoxAsync("فعالسازی حساب کاربری",
            $"آیا از فعالسازی حساب کاربری {context.FullName} اطمینان دارید؟", "بله", "انصراف");

        if (result == true)
        {
            await SendRequestAsync<IUserAccountService>(
                action: (s, ct) => s.UnlockUserAsync(context.Id, ct),
                afterSend: async () =>
                {
                    await RefreshAsync();
                    AddSuccessToast($"حساب کاربری {context.FullName} با موفقیت فعال شد");
                });
        }
    }

    private async Task OnAddAccount()
    {
        var dialog = await DialogService.ShowAsync<UserEditor>("حساب کاربری جدید", _dialogOptions);

        var result = await dialog.Result;

        if (result is { Canceled: false }) 
            await RefreshAsync();
    }

    private async Task OnEditAccount(GetUserAccountResponse context)
    {
        var parameters = new DialogParameters<UserEditor>
        {
            { x => x.Model, UserEditorVm.CreateFrom(context) }
        };

        var dialog = await DialogService.ShowAsync<UserEditor>("ویرایش حساب کاربری", parameters, _dialogOptions);

        var result = await dialog.Result;

        if (result is { Canceled: false })
            await RefreshAsync();
    }

    private async Task OnDeleteAccount(GetUserAccountResponse context)
    {
        var result = await DialogService.ShowMessageBoxAsync("حذف حساب کاربری",
            $"آیا از حذف حساب کاربری {context.FullName} اطمینان دارید؟", "بله", "انصراف");

        if (result == true)
        {
            await SendRequestAsync<IUserAccountService>(
                action: (s, ct) => s.DeleteAccountAsync(context.Id, ct),
                afterSend: async () =>
                {
                    await RefreshAsync();
                    AddSuccessToast($"حساب کاربری {context.FullName} با موفقیت حذف شد");
                });
        }
    }

    private void PageChanged(int page)
    {
        if (page <= 0)
            return;

        _currentPage = page - 1;
        _table.NavigateTo(_currentPage);
    }

    private async Task RefreshAsync()
    {
        await _table.ReloadServerData();
        StateHasChanged();
    }

    private async Task<TableData<GetUserAccountResponse>> LoadUsersAsync(TableState state, CancellationToken cancellationToken)
    {
        var currentPageIndex = state.Page;

        var result = new TableData<GetUserAccountResponse>();
        var filter = new RequestFilter(
            currentPageIndex * state.PageSize,
            state.PageSize,
            _searchString,
            state.SortLabel,
            state.SortDirection switch
            {
                SortDirection.None => Sdk.Server.Shared.Enums.SortDirection.None,
                SortDirection.Ascending => Sdk.Server.Shared.Enums.SortDirection.Ascending,
                SortDirection.Descending => Sdk.Server.Shared.Enums.SortDirection.Descending,
                _ => throw new ArgumentOutOfRangeException()
            });

        await SendRequestAsync<IUserAccountService, PagedList<GetUserAccountResponse>>(
            action: (s, token) => s.GetAccountsListAsync(filter, token),
            afterSend: response =>
            {
                result = new TableData<GetUserAccountResponse>
                {
                    TotalItems = response.Total,
                    Items = response.Data
                };
            },
            createScope: true,
            cancelPrevious: true
        );

        return result;
    }

    private async Task OnSearch(string text)
    {
        _searchString = text;
        await RefreshAsync();
    }
}