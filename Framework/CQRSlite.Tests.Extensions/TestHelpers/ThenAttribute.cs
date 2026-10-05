using System.Runtime.CompilerServices;
using Xunit;

namespace CQRSlite.Tests.Extensions.TestHelpers;

public class ThenAttribute(
    [CallerFilePath] string? sourceFilePath = null,
    [CallerLineNumber] int sourceLineNumber = -1)
    : FactAttribute(sourceFilePath, sourceLineNumber);
