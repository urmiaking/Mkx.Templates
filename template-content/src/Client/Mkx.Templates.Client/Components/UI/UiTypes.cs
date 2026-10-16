using System.Collections;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Components;

namespace Mkx.Templates.Client.Components.UI;

public enum Severity { Info, Success, Warning, Error, Normal }
public enum Color { Default, Primary, Secondary, Info, Success, Warning, Error, Inherit, Dark }
public enum Variant { Text, Filled, Outlined }
public enum InputType { Text, Password, Email, Telephone, Number }
public enum ButtonType { Button, Submit, Reset }
public enum MaxWidth { Small, Medium, Large, ExtraLarge, False }
public enum SortDirection { None, Ascending, Descending }
public record BreadcrumbItem(string Text, string? href = null, string? icon = null)
{
    public string? Href => href;
}
public sealed class TableState
{
    public int Page { get; set; }
    public int PageSize { get; set; } = 25;
    public string? SortLabel { get; set; }
    public SortDirection SortDirection { get; set; }
}
public sealed class TableData<T>
{
    public IEnumerable<T> Items { get; set; } = [];
    public int TotalItems { get; set; }
}
public interface ITableController
{
    Task SortAsync(string label);
}
public sealed record DialogResult(bool Canceled, object? Data = null)
{
    public static DialogResult Ok(object? data) => new(false, data);
}
public sealed class DialogOptions
{
    public bool CloseButton { get; set; }
    public bool CloseOnEscapeKey { get; set; } = true;
    public bool BackdropClick { get; set; } = true;
    public MaxWidth MaxWidth { get; set; } = MaxWidth.Small;
    public bool FullWidth { get; set; } = true;
}
public sealed class DialogParameters<T> : IEnumerable<KeyValuePair<string, object?>>
{
    private readonly Dictionary<string, object?> _values = [];
    public void Add<TValue>(Expression<Func<T, TValue>> member, TValue value) =>
        _values.Add(((MemberExpression)member.Body).Member.Name, value);
    public IEnumerator<KeyValuePair<string, object?>> GetEnumerator() => _values.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
public sealed class DialogSession(string title, Type component, IDictionary<string, object?> parameters, DialogOptions options, Action<DialogSession> closed, Action changed)
{
    private readonly TaskCompletionSource<DialogResult> _completion = new(TaskCreationOptions.RunContinuationsAsynchronously);
    public string Title { get; } = title;
    public Type Component { get; } = component;
    public IDictionary<string, object?> Parameters { get; } = parameters;
    public DialogOptions Options { get; } = options;
    public Task<DialogResult> Result => _completion.Task;
    public bool IsBusy { get; private set; }
    public void SetBusy(bool busy) { IsBusy = busy; changed(); }
    public void Close(DialogResult result) { if (_completion.TrySetResult(result)) closed(this); }
    public void Close(object? data = null) => Close(DialogResult.Ok(data));
    public void Cancel() => Close(new DialogResult(true));
}
public sealed class DialogService
{
    public List<DialogSession> Sessions { get; } = [];
    public event Action? Changed;
    public Task<DialogSession> ShowAsync<T>(string title, DialogOptions? options = null) where T : IComponent => Open<T>(title, new Dictionary<string, object?>(), options);
    public Task<DialogSession> ShowAsync<T>(string title, DialogParameters<T> parameters, DialogOptions? options = null) where T : IComponent => Open<T>(title, parameters.ToDictionary(x => x.Key, x => x.Value), options);
    private Task<DialogSession> Open<T>(string title, IDictionary<string, object?> parameters, DialogOptions? options) where T : IComponent
    {
        var session = new DialogSession(title, typeof(T), parameters, options ?? new(), s => { Sessions.Remove(s); Changed?.Invoke(); }, () => Changed?.Invoke());
        Sessions.Add(session);
        Changed?.Invoke();
        return Task.FromResult(session);
    }
    public async Task<bool?> ShowMessageBoxAsync(string title, string message, string yesText = "تأیید", string cancelText = "انصراف")
    {
        var session = await Open<UiConfirmation>(title, new Dictionary<string, object?> { ["Message"] = message, ["YesText"] = yesText, ["CancelText"] = cancelText }, new());
        return (await session.Result).Canceled ? null : true;
    }
}
public sealed record ToastMessage(Guid Id, string Message, Severity Severity);
public sealed class ToastService
{
    public List<ToastMessage> Messages { get; } = [];
    public event Action? Changed;
    public void Add(string message, Severity severity)
    {
        if (Messages.Count >= 4) Messages.RemoveAt(0);
        Messages.Add(new(Guid.NewGuid(), message, severity));
        Changed?.Invoke();
    }
    public void Dismiss(Guid id) { Messages.RemoveAll(x => x.Id == id); Changed?.Invoke(); }
}
