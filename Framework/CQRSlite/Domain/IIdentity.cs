using System;

namespace CQRSlite.Domain
{
    public interface IIdentity : IEquatable<IIdentity>
    {
        string Id { get; }
        bool IsValid { get; }
    }
}