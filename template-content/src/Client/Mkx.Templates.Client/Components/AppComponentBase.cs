using System.Net;
using System.Security.Claims;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Mkx.Templates.Sdk.Server.Shared.Exceptions;
using Mkx.Templates.Sdk.Shared.Exceptions;
using MudBlazor;
using Severity = MudBlazor.Severity;

namespace Mkx.Templates.Client.Components;

public class AppComponentBase : ComponentBase, IAsyncDisposable
{
    private CancellationTokenSource? _cancellation;
    private IServiceScope? _scope;
    private int _busyCount;
    private bool _skipRender;
    private PersistingComponentStateSubscription _persistSubscription;
    protected bool IsDisposed { get; private set; }
    public bool IsBusy => _busyCount > 0;
    protected ClaimsPrincipal? User { get; private set; }
    protected string? LastRequestError { get; private set; }
    protected IReadOnlyDictionary<string, string[]> ValidationErrors { get; private set; } = new Dictionary<string, string[]>();
    protected Guid? UserId => Guid.TryParse(User?.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var id) ? id : null;
    protected CancellationTokenSource CancellationTokenSource => _cancellation ??= new();
    protected CancellationToken CancellationToken => CancellationTokenSource.Token;
    private IServiceScope CurrentScope => _scope ??= CreateServiceScope();

    [Inject] private IServiceScopeFactory ServiceScopeFactory { get; set; } = default!;
    [Inject] protected IDialogService DialogService { get; set; } = default!;
    [Inject] protected ISnackbar ToastService { get; set; } = default!;
    [Inject] protected IAuthorizationService AuthorizationService { get; set; } = default!;
    [Inject] protected AuthenticationStateProvider AuthenticationStateProvider { get; set; } = default!;
    [Inject] protected NavigationManager Navigation { get; set; } = default!;
    [Inject] private PersistentComponentState PersistentState { get; set; } = default!;

    protected override async Task OnInitializedAsync()
    {
        User = (await AuthenticationStateProvider.GetAuthenticationStateAsync()).User;
        AuthenticationStateProvider.AuthenticationStateChanged += AuthenticationStateChanged;
        _persistSubscription = PersistentState.RegisterOnPersisting(OnPersisting);
        await base.OnInitializedAsync();
    }
    private async void AuthenticationStateChanged(Task<AuthenticationState> task)
    {
        try
        {
            var state = await task;
            if (!IsDisposed) await InvokeAsync(() => { User = state.User; StateHasChanged(); });
        }
        catch (Exception ex) { if (!IsDisposed) await DispatchExceptionAsync(ex); }
    }
    protected virtual Task OnPersisting() => Task.CompletedTask;
    protected void PersistStateAsJson<T>(string key, T instance) => PersistentState.PersistAsJson(key, instance);
    protected bool RestoreStateFromJson<T>(string key, out T? restored) => PersistentState.TryTakeFromJson(key, out restored);
    protected override bool ShouldRender() { var render = !_skipRender; _skipRender = false; return render; }
    protected void ShouldNotRender() => _skipRender = true;
    protected async Task<bool> HasPolicyAsync(string policy) =>
        (await AuthorizationService.AuthorizeAsync((await AuthenticationStateProvider.GetAuthenticationStateAsync()).User, policy)).Succeeded;
    protected void AddSuccessToast(string message) => ToastService.Add(message, Severity.Success);
    protected void AddErrorToast(string message) => ToastService.Add(message, Severity.Error);
    protected void AddWarningToast(string message) => ToastService.Add(message, Severity.Warning);
    protected void AddInfoToast(string message) => ToastService.Add(message, Severity.Info);
    protected IServiceScope CreateServiceScope() => ServiceScopeFactory.CreateScope();
    protected T GetRequiredService<T>() where T : notnull => CurrentScope.ServiceProvider.GetRequiredService<T>();
    protected T GetRequiredService<T>(IServiceScope scope) where T : notnull => scope.ServiceProvider.GetRequiredService<T>();
    protected IEnumerable<T> GetServices<T>() where T : notnull => CurrentScope.ServiceProvider.GetServices<T>();
    protected void SetBusy(bool stateChanged = false) { Interlocked.Increment(ref _busyCount); if (stateChanged) StateHasChanged(); }
    protected void SetIdeal(bool stateChanged = false) { Interlocked.Decrement(ref _busyCount); if (stateChanged && !IsDisposed) StateHasChanged(); }
    protected void CancelToken() { _cancellation?.Cancel(); DestroyCancellationToken(); }
    protected void DestroyCancellationToken() { _cancellation?.Dispose(); _cancellation = null; }
    protected readonly record struct RequestResult<T>(bool Succeeded, T? Value);

    // New data flows use this explicit result. Failure must never be mistaken for an empty page or false flag.
    protected async Task<RequestResult<TResponse>> TryRequestAsync<TService, TResponse>(
        Func<TService, CancellationToken, Task<TResponse>> action, bool createScope = false, bool cancelPrevious = false,
        Func<Task>? onFailure = null, bool ignoreCancellation = false) where TService : notnull
    {
        if (IsDisposed) return new(false, default);
        if (cancelPrevious) CancelToken();
        var scope = createScope ? CreateServiceScope() : CurrentScope;
        SetBusy();
        LastRequestError = null;
        ValidationErrors = new Dictionary<string, string[]>();
        await InvokeAsync(StateHasChanged);
        var token = ignoreCancellation ? CancellationToken.None : CancellationToken;
        try
        {
            var value = await action(GetRequiredService<TService>(scope), token);
            return IsDisposed ? new(false, default) : new(true, value);
        }
        catch (OperationCanceledException) when (IsDisposed || token.IsCancellationRequested) { return new(false, default); }
        catch (Exception ex)
        {
            if (!IsDisposed)
            {
                HandleRequestException(ex);
                if (onFailure is not null) await onFailure();
            }
            return new(false, default);
        }
        finally
        {
            if (createScope) scope.Dispose();
            SetIdeal();
            if (!IsDisposed) await InvokeAsync(StateHasChanged);
        }
    }

    // Compatibility overloads centralize execution; callbacks run only after a successful request.
    protected async Task<TResponse?> SendRequestAsync<TService, TResponse>(Func<TService, CancellationToken, Task<TResponse>> action,
        bool createScope = false, bool cancelPrevious = false) where TService : notnull =>
        (await TryRequestAsync(action, createScope, cancelPrevious)).Value;

    protected async Task SendRequestAsync<TService, TResponse>(Func<TService, CancellationToken, Task<TResponse>> action,
        Action<TResponse> afterSend, Action? onFailure = null, bool createScope = false, bool cancelPrevious = false) where TService : notnull
    {
        await TryRequestAsync<TService, bool>(async (service, token) => { afterSend(await action(service, token)); return true; },
            createScope, cancelPrevious, onFailure is null ? null : () => { onFailure(); return Task.CompletedTask; });
    }
    protected async Task SendRequestAsync<TService, TResponse>(Func<TService, CancellationToken, Task<TResponse>> action,
        Func<TResponse, Task> afterSend, Func<Task>? onFailure = null, bool cancelPrevious = false) where TService : notnull
    {
        await TryRequestAsync<TService, bool>(async (service, token) => { await afterSend(await action(service, token)); return true; },
            cancelPrevious: cancelPrevious, onFailure: onFailure);
    }
    protected async Task<TResult> SendRequestAsync<TService, TResult, TResponse>(Func<TService, CancellationToken, Task<TResponse>> action,
        Func<TResponse, TResult> afterSend, Action? onFailure = null, bool cancelPrevious = false) where TService : notnull =>
        (await TryRequestAsync<TService, TResult>(async (service, token) => afterSend(await action(service, token)),
            cancelPrevious: cancelPrevious, onFailure: onFailure is null ? null : () => { onFailure(); return Task.CompletedTask; })).Value!;
    protected async Task<TResult> SendRequestAsync<TService, TResult, TResponse>(Func<TService, CancellationToken, Task<TResponse>> action,
        Func<TResponse, Task<TResult>> afterSend, Func<Task>? onFailure = null, bool cancelPrevious = false) where TService : notnull =>
        (await TryRequestAsync<TService, TResult>(async (service, token) => await afterSend(await action(service, token)),
            cancelPrevious: cancelPrevious, onFailure: onFailure)).Value!;
    protected async Task SendRequestAsync<TService>(Func<TService, CancellationToken, Task> action,
        Func<Task>? afterSend = null, Func<Task>? onFailure = null, bool cancelPrevious = false, bool ignoreCancellation = false) where TService : notnull
    {
        await TryRequestAsync<TService, bool>(async (service, token) => { await action(service, token); if (afterSend is not null) await afterSend(); return true; },
            cancelPrevious: cancelPrevious, onFailure: onFailure, ignoreCancellation: ignoreCancellation);
    }

    private void HandleRequestException(Exception ex)
    {
        if (ex is HttpRequestValidationException validation) ValidationErrors = validation.Errors;
        if (ex is ValidationException domainValidation)
            ValidationErrors = domainValidation.Errors.GroupBy(e => e.PropertyName).ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
        LastRequestError = ex switch
        {
            HttpRequestAuthenticationFailedException => "نشست شما منقضی شده است. دوباره وارد شوید.",
            HttpRequestAuthorizationFailedException => "مجوز انجام این عملیات را ندارید.",
            HttpRequestFailedException failure when failure.StatusCode == HttpStatusCode.Conflict => "اطلاعات تغییر کرده است. صفحه را تازه کرده و دوباره تلاش کنید.",
            HttpRequestFailedException failure when (int)failure.StatusCode == 429 => "تعداد درخواست‌ها زیاد است. یک دقیقه صبر کنید.",
            HttpRequestValidationException or ValidationException => ValidationErrors.Count > 0 ? string.Join("\n", ValidationErrors.Values.SelectMany(e => e)) : ex.Message,
            NotFoundException => "اطلاعات مورد نظر یافت نشد.",
            HttpRequestFailedException failure when (int)failure.StatusCode >= 500 => "خطایی در سرور رخ داده است. دوباره تلاش کنید.",
            HttpRequestException or HttpRequestFailedException => "ارتباط با سرور برقرار نشد. اتصال را بررسی و دوباره تلاش کنید.",
            _ => "عملیات انجام نشد. دوباره تلاش کنید."
        };
        AddErrorToast(LastRequestError);
    }

    public virtual ValueTask DisposeAsync()
    {
        if (IsDisposed) return ValueTask.CompletedTask;
        IsDisposed = true;
        AuthenticationStateProvider.AuthenticationStateChanged -= AuthenticationStateChanged;
        _persistSubscription.Dispose();
        _cancellation?.Cancel();
        DestroyCancellationToken();
        _scope?.Dispose();
        return ValueTask.CompletedTask;
    }
}
