using System.Threading;
using System.Threading.Tasks;

namespace Mkx.Templates.Application.Abstractions;

public interface ISmsSender
{
    Task<bool> SendAsync(string phoneNumber, string message, CancellationToken cancellationToken = default);
}
