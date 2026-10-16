using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Mkx.Templates.Client.Components.UI;

namespace Mkx.Templates.Tests;

public sealed class NativeFormTests
{
    [Theory]
    [InlineData("", false)]
    [InlineData("ab", false)]
    [InlineData("valid", true)]
    public async Task NativeFieldsRegisterWithTheirFormAndValidateBeforeRequests(string name, bool valid)
    {
        await using var services = new ServiceCollection().AddLogging().BuildServiceProvider();
        await using var renderer = new HtmlRenderer(services, services.GetRequiredService<ILoggerFactory>());
        UiForm? form = null;
        var model = new FormModel { Name = name };
        var parameters = ParameterView.FromDictionary(new Dictionary<string, object?>
        {
            [nameof(FormProbe.Model)] = model,
            [nameof(FormProbe.Capture)] = (Action<UiForm>)(value => form = value)
        });
        await renderer.Dispatcher.InvokeAsync(async () =>
        {
            var output = await renderer.RenderComponentAsync<FormProbe>(parameters);
            Assert.NotNull(form);
            await form.ValidateAsync();
            Assert.Equal(valid, form.IsValid);
            var html = output.ToHtmlString();
            Assert.Contains("<input", html);
            Assert.Contains("<label", html);
            Assert.Contains("maxlength=\"200\"", html);
            if (!valid) Assert.Contains("role=\"alert\"", html);
        });
    }

    public sealed class FormModel
    {
        [Required, StringLength(200, MinimumLength = 3)] public string Name { get; set; } = "";
    }

    public sealed class FormProbe : ComponentBase
    {
        [Parameter] public FormModel Model { get; set; } = new();
        [Parameter] public Action<UiForm> Capture { get; set; } = default!;
        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            builder.OpenComponent<UiForm>(0);
            builder.AddAttribute(1, nameof(UiForm.Model), Model);
            builder.AddAttribute(2, nameof(UiForm.ChildContent), (RenderFragment)(field =>
            {
                field.OpenComponent<UiTextField>(0);
                field.AddAttribute(1, nameof(UiTextField.Value), Model.Name);
                field.AddAttribute(2, nameof(UiTextField.For), (System.Linq.Expressions.Expression<Func<string?>>)(() => Model.Name));
                field.AddAttribute(3, nameof(UiTextField.Required), true);
                field.AddAttribute(4, nameof(UiTextField.Label), "Name");
                field.AddAttribute(5, nameof(UiTextField.MaxLength), 200);
                field.CloseComponent();
            }));
            builder.AddComponentReferenceCapture(3, value => Capture((UiForm)value));
            builder.CloseComponent();
        }
    }
}
